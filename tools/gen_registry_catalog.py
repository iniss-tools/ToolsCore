"""
Generator katalogu nastaveni INISSu v registri (ToolsCore.Iniss/Registry).

Zdroj: iniss-tools-docs/docs/iniss/registry.mdx (jediny uplny popis registra INISSu).
Vystup:
  ToolsCore.Iniss/Registry/RegCatalog.g.cs   - sekcie a hodnoty (typ, predvolena hodnota, zapis, verzie, vetva)
  ToolsCore.Iniss/Registry/RegTexts.resx     - slovenske popisy (skratene texty z docs, bez odkazov)
  ToolsCore.Iniss/Registry/RegTexts.cs.resx  - ceske popisy; generator ich nepise, len kontroluje, ci ku kazdemu
                                               klucu existuje preklad a ci sa slovensky zdroj (komentar) nezmenil

Pouzitie (z koreňa repozitara ToolsCore):
  python tools/gen_registry_catalog.py [--docs ../iniss-tools-docs/docs/iniss/registry.mdx] [--export-sk texty.json]
"""
import argparse
import json
import re
import sys
from pathlib import Path
from xml.sax.saxutils import escape

ROOT = Path(__file__).resolve().parent.parent
OUT_DIR = ROOT / 'ToolsCore.Iniss' / 'Registry'

# sekcie -> skupina v okne (podla ucelu)
GROUPS = {
    'Driver': 'Boards', 'Tables': 'Boards',
    'Zvuky': 'Sound', 'Pauses': 'Sound', 'Volume': 'Sound', 'TORNZ': 'Sound', 'Modifiers': 'Sound',
    'Grafikon': 'Timetable',
    'Environment': 'Operator',
    'Loging': 'Logs',
    'GTN': 'Interfaces', 'IntSrv': 'Interfaces', 'Client': 'Interfaces', 'Remote': 'Interfaces', 'Switch': 'Interfaces',
    'PathNames': 'Files',
    'Implementation': 'Installation',
    'Colors': 'Appearance', 'Font': 'Appearance', 'ColumnWidth': 'Appearance',
    'OnlyDebug': 'Debug', 'LogToINISS': 'Debug',
}

# cislovane sekcie: (najvyssi index, aj dvojciferny tvar 00-09)
NUMBERED = {'Driver': (98, True), 'Volume': (8, False)}

# hodnoty, ktore INISS cita aj pod starym nazvom bez jednotky (overene v Ghidre, 3.39)
LEGACY = {
    ('Driver', 'TableTimeoutInterval [ms]'), ('Driver', 'TableTimeoutConst [ms]'), ('Driver', 'TableMinWriteDelay [ms]'),
    ('Driver', 'SyncTimeInterval [s]'), ('Driver', 'PollingInterval [ms]'), ('Driver', 'PollingTimeout [ms]'),
    ('Remote', 'TimeoutInterval [ms]'), ('Remote', 'TimeoutConst [ms]'), ('Remote', 'TimeoutMul [ms/100]'),
    ('OnlyDebug', 'UpdateInterval [s]'),
}

# slovne predvolene hodnoty z docs - urcuje ich program podla okolnosti (RegResolver)
DYNAMIC_DEFAULTS = {'podľa triedy', 'podľa výrobcu', 'skladá sa za behu', '1 / 0'}

TYPES = {'DWORD': 'Dword', 'bool': 'Bool', 'SZ': 'String', 'BINARY': 'Binary', 'COLORREF': 'Color'}
WRITES = {'auto': 'Auto', 'auto+app': 'AutoAndApp', 'app': 'App', 'none': 'None'}

ATTR_RE = r'(\w+)=("[^"]*"|\'[^\']*\'|\{(?:[^{}]|\{[^{}]*\})*\})'
TAG_RE = re.compile(r'<RegKey((?:\s+' + ATTR_RE + r')*)\s*>(.*?)</RegKey>', re.S)


def attrs_of(text):
    out = {}
    for name, value in re.findall(ATTR_RE, text):
        if value[0] in '"\'':
            value = value[1:-1]
        elif re.fullmatch(r'\{"(?:[^"\\]|\\.)*"\}', value):
            # JSX retazec def={"DATA\\"} - escapovanie ako v JavaScripte
            value = json.loads(value[1:-1])
        out[name] = value
    return out


def js_pairs(text):
    """[["a", "b"], ...] z JSX values={[...]}."""
    s = r'("(?:[^"\\]|\\.)*"|\'(?:[^\'\\]|\\.)*\')'
    return [(a[1:-1], b[1:-1]) for a, b in re.findall(r'\[\s*' + s + r'\s*,\s*' + s + r'\s*,?\s*\]', text, re.S)]


def clean_inline(t):
    """Jeden odsek MDX -> obycajny text: bez odkazov, oznaceni, komponentov a odkazov 'vid ...'."""
    link = r'\[[^\]]*\]\([^)]*\)'
    t = re.sub(r'<Tag>([^<]*)</Tag>', r'\1', t)
    t = re.sub(r'<\w+\s[^<>]*/>|</?[a-z]\w*>', '', t)
    # odkazy do dalsej dokumentacie: "(vid [..](..))", "– vid [..](..)", ", vid [..](..)" aj bez odkazu ("vid tvary vyssie")
    t = re.sub(r'\s*\((?:viď|Viď)\b(?:' + link + r'|[^()])*\)', '', t)
    t = re.sub(r'\s*[–,-]\s*(?:viď|Viď)\b(?:' + link + r'|[^.;()])*', '', t)
    t = re.sub(r'(?:^|(?<=[.!?]))\s*Viď\b(?:' + link + r'|[^.])*\.', '', t)
    t = re.sub(link, lambda m: re.match(r'\[([^\]]*)\]', m.group(0)).group(1), t)
    t = re.sub(r'\*\*([^*]+)\*\*', r'\1', t)
    t = re.sub(r'(?<!\w)\*([^*\n]+)\*(?!\w)', r'\1', t)
    t = t.replace('`', '')
    t = re.sub(r'\s+', ' ', t).strip()
    t = re.sub(r'\s+([,.;:)])', r'\1', t)
    return t


def clean(text):
    """MDX -> text pre UI; odseky oddeli novym riadkom, odrazky zachova ako '• '."""
    out = []
    for para in re.split(r'\n\s*\n', text.strip()):
        items = re.split(r'\n\s*[*-]\s+', '\n' + para.strip())
        head = clean_inline(items[0])
        if head:
            out.append(head)
        out += ['• ' + clean_inline(i) for i in items[1:] if clean_inline(i)]
    t = '\n'.join(out)
    return SK_OVERRIDES.get(t, t)


# texty, ktore v docs odkazuju na tabulky alebo obrazky stranky - v okne by nedavali zmysel
SK_OVERRIDES = {
    'Typ protokolu a rodina tabúľ. Vlastné predvolené hodnoty majú triedy 2, 4, 10, 128, 129 a 130; trieda 5 aj všetky ostatné dostanú predvoľby z riadku „iné“ vyššie.':
        'Typ protokolu a rodina tabúľ, napríklad 4 pre tabule ELEN. Určuje aj predvolené hodnoty ostatných nastavení linky: vlastné predvoľby majú triedy 2, 4, 10, 128, 129 a 130, trieda 5 aj všetky ostatné dostanú spoločné.',
    'Komunikačný kanál k tabuliam.':
        r'Komunikačný kanál k tabuliam: sériový port (COM1, COM2…), pomenovaná rúra (\\PIPE\…), mailslot (\\MAILSLOT\…), '
        r'TCP://host:port alebo UDP://host:port. Pred sieťový kanál sa píše číslo komunikačnej linky, napríklad '
        r'3=TCP://host:port – bez neho vyjde linka 0, čo INISS ohlási ako chybu.',
}


def ident(text):
    t = re.sub(r'\[[^\]]*\]', '', text)
    t = re.sub(r'[^0-9A-Za-z]+', '_', t).strip('_')
    return t or 'X'


def cs_str(s):
    return '"' + s.replace('\\', '\\\\').replace('"', '\\"') + '"'


def parse_default(raw, typ):
    """Vrati (C# vyraz RegDefault, text dynamickej predvolby alebo None)."""
    if raw is None:
        return 'RegDefault.None', None
    if re.fullmatch(r'-?\d+', raw):
        return f'RegDefault.FromNumber({int(raw)})', None
    if re.fullmatch(r'0x[0-9A-Fa-f]+', raw):
        return f'RegDefault.FromNumber(unchecked((int)0x{int(raw, 16):08X}))', None
    if raw == '""':
        return 'RegDefault.FromText("")', None
    if raw in DYNAMIC_DEFAULTS or typ != 'String':
        return 'RegDefault.Dynamic', raw
    return f'RegDefault.FromText({cs_str(raw)})', None


def section_intro(body):
    """Prve odseky textu pod nadpisom sekcie (bez komponentov, tabuliek a upozorneni)."""
    paras = []
    for block in re.split(r'\n\s*\n', body):
        b = block.strip()
        if not b:
            continue
        if b.startswith(('<SectionMeta', '<RegPath')):
            continue
        if b.startswith(('<', ':::', '|', '#', '```', '-', '1.')):
            break
        paras.append(clean_inline(b))
        if len(paras) == 2:
            break
    return ' '.join(paras)


def parse(docs):
    text = docs.read_text(encoding='utf-8')
    sections = []
    parts = re.split(r'\n## ', text)
    for part in parts[1:]:
        heading, _, body = part.partition('\n')
        meta = re.search(r'<SectionMeta\s+hive="(\w+)"', body)
        if not meta:
            continue
        name = heading.split(',')[0].strip()
        sec = {'name': name, 'hive': meta.group(1), 'intro': section_intro(body), 'keys': [], 'colors': []}
        regkeys = body.split('\n### ')[0] if name not in ('Driver',) else body
        for m in TAG_RE.finditer(regkeys):
            a = attrs_of(m.group(1))
            desc = clean(m.group(m.lastindex))
            values = js_pairs(a['values']) if 'values' in a else []
            for nm in [n.strip() for n in a['name'].split(',')] if ', ' in a['name'] else [a['name']]:
                sec['keys'].append({
                    'name': nm, 'type': TYPES[a.get('type', 'BINARY')], 'def': a.get('def'),
                    'write': WRITES[a.get('write', 'auto')], 'since': a.get('since'), 'until': a.get('until'),
                    'hive': a.get('hive'), 'tag': a.get('tag'), 'writeOnly': a.get('badge') == 'len zápis',
                    'desc': desc, 'values': [(v, clean(t)) for v, t in values],
                })
        if name == 'Colors':
            for gi, grp in enumerate(re.finditer(r'names:\s*\[(.*?)\],\s*use:\s*"((?:[^"\\]|\\.)*)"', body, re.S)):
                for cn in re.findall(r'"((?:[^"\\]|\\.)*)"', grp.group(1)):
                    sec['colors'].append((cn, gi, clean(grp.group(2))))
        sections.append(sec)

    unused = {}
    m = re.search(r'### Názvy, ktoré nečíta žiadna verzia(.*?)###', text, re.S)
    for row in re.findall(r'^\|\s*`(\w+)`\s*\|(.*)\|\s*$', m.group(1), re.M):
        names = []
        for n in re.findall(r'`([^`]+)`(?:\s*až\s*`([^`]+)`)?', row[1]):
            if n[1]:
                a, b = re.match(r'(\D+)(\d+)', n[0]), re.match(r'(\D+)(\d+)', n[1])
                names += [f'{a.group(1)}{i}' for i in range(int(a.group(2)), int(b.group(2)) + 1)]
            else:
                names.append(n[0])
        unused[row[0]] = names
    m = re.search(r'### Názvy, ktoré program vôbec nepozná(.*?)###', text, re.S)
    obsolete = re.findall(r'`(\w+)`', m.group(1))
    return sections, unused, obsolete


def build(sections, unused, obsolete):
    texts = {}  # kluc resx -> slovensky text
    cs = ['// <auto-generated>', '// Vygenerovane skriptom tools/gen_registry_catalog.py z iniss-tools-docs/docs/iniss/registry.mdx.',
          '// Rucne neupravovat - zmeny patria do dokumentacie, potom spustit generator znova.', '// </auto-generated>',
          '', 'namespace ToolsCore.Iniss.Registry;', '', 'public static partial class RegCatalog', '{',
          '    private static RegSection[] BuildSections() =>', '    [']
    used_ids = set()
    for sec in sections:
        sname = sec['name']
        skey = f'S_{ident(sname)}'
        texts[skey] = sec['intro']
        max_index, two_digit = NUMBERED.get(sname, (-1, False))
        cs.append(f'        new RegSection({cs_str(sname)}, RegHive.{sec["hive"].capitalize()}, RegGroup.{GROUPS[sname]}, '
                  f'{max_index}, {str(two_digit).lower()}, {cs_str(skey)},')
        cs.append('        [')
        if sname == 'Colors':
            for i, (cn, gi, use) in enumerate(sec['colors']):
                kid = f'K_Colors_G{gi}'
                texts[kid] = use
                cs.append(f'            new RegSetting({cs_str(cn)}, RegValueType.Color, RegDefault.Dynamic, RegWriteMode.Auto, {cs_str(kid)}) '
                          f'{{ Kind = RegNameKind.Color, ColorIndex = {i}, DynamicDefaultKey = "D_Colors" }},')
            texts['D_Colors'] = 'farba zabudovaná v programe'
        for k in sec['keys']:
            name = k['name']
            kind = 'Fixed'
            if '<N>' in name:
                kind = 'Indexed'
            elif name.startswith('«'):
                kind = 'Dynamic'
            base = f'K_{ident(sname)}_{ident(name.replace("<N>", "N"))}'
            kid = base
            n = 2
            while kid in used_ids:
                kid = f'{base}{n}'
                n += 1
            used_ids.add(kid)
            texts[kid] = k['desc']
            default, dyn = parse_default(k['def'], k['type'])
            props = []
            if kind != 'Fixed':
                props.append(f'Kind = RegNameKind.{kind}')
            if dyn is not None:
                dkey = f'D_{kid[2:]}'
                texts[dkey] = dyn
                props.append(f'DynamicDefaultKey = {cs_str(dkey)}')
            if k['since']:
                props.append(f'Since = new RegVersion({k["since"].replace(".", ", ")})')
            if k['until']:
                props.append(f'Until = new RegVersion({k["until"].replace(".", ", ")})')
            if k['hive']:
                props.append(f'HiveOverride = RegHive.{k["hive"].capitalize()}')
            if k['writeOnly']:
                props.append('WriteOnly = true')
            if (sname, name) in LEGACY:
                legacy = re.sub(r'\s*\[[^\]]*\]$', '', name)
                props.append(f'LegacyName = {cs_str(legacy)}')
            if k['values']:
                choices = []
                for i, (v, t) in enumerate(k['values']):
                    ckey = f'C_{kid[2:]}_{i}'
                    texts[ckey] = t
                    choices.append(f'new RegChoice({cs_str(v)}, {cs_str(ckey)})')
                props.append('Choices = [' + ', '.join(choices) + ']')
            init = (' { ' + ', '.join(props) + ' }') if props else ''
            cs.append(f'            new RegSetting({cs_str(name)}, RegValueType.{k["type"]}, {default}, RegWriteMode.{k["write"]}, '
                      f'{cs_str(kid)}){init},')
        cs.append('        ]),')
    cs.append('    ];')
    cs.append('')
    cs.append('    private static Dictionary<string, string[]> BuildUnusedNames() => new(StringComparer.OrdinalIgnoreCase)')
    cs.append('    {')
    for s, names in unused.items():
        cs.append(f'        [{cs_str(s)}] = [{", ".join(cs_str(n) for n in names)}],')
    cs.append('    };')
    cs.append('')
    cs.append('    private static string[] BuildObsoleteNames() => [' + ', '.join(cs_str(n) for n in obsolete) + '];')
    cs.append('}')
    return '\n'.join(cs) + '\n', texts


RESX_HEAD = '''<?xml version="1.0" encoding="utf-8"?>
<root>
  <resheader name="resmimetype">
    <value>text/microsoft-resx</value>
  </resheader>
  <resheader name="version">
    <value>2.0</value>
  </resheader>
  <resheader name="reader">
    <value>System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
  <resheader name="writer">
    <value>System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
'''


def write_resx(path, entries, comments=None):
    lines = [RESX_HEAD.rstrip('\n')]
    for key, value in entries.items():
        lines.append(f'  <data name="{key}" xml:space="preserve">')
        lines.append(f'    <value>{escape(value)}</value>')
        if comments and key in comments:
            lines.append(f'    <comment>{escape(comments[key])}</comment>')
        lines.append('  </data>')
    lines.append('</root>')
    path.write_bytes(('
'.join(lines) + '
').encode('utf-8'))


def read_resx(path):
    if not path.exists():
        return {}
    text = path.read_text(encoding='utf-8')
    out = {}
    for m in re.finditer(r'<data name="([^"]+)"[^>]*>\s*<value>(.*?)</value>(?:\s*<comment>(.*?)</comment>)?', text, re.S):
        from xml.sax.saxutils import unescape
        out[m.group(1)] = (unescape(m.group(2)), unescape(m.group(3)) if m.group(3) is not None else None)
    return out


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument('--docs', default=str(ROOT.parent / 'iniss-tools-docs' / 'docs' / 'iniss' / 'registry.mdx'))
    ap.add_argument('--export-sk', help='zapise slovenske texty (kluc -> text) do JSON na preklad')
    ap.add_argument('--import-cs', help='JSON s ceskymi prekladmi (kluc -> text) - zapise RegTexts.cs.resx')
    args = ap.parse_args()

    sections, unused, obsolete = parse(Path(args.docs))
    code, texts = build(sections, unused, obsolete)
    (OUT_DIR / 'RegCatalog.g.cs').write_bytes(code.replace('
', '
').encode('utf-8'))
    write_resx(OUT_DIR / 'RegTexts.resx', texts)
    print(f'{sum(len(s["keys"]) for s in sections)} hodnot v {len(sections)} sekciach, {len(texts)} textov')

    if args.export_sk:
        Path(args.export_sk).write_text(json.dumps(texts, ensure_ascii=False, indent=1), encoding='utf-8')

    cs_path = OUT_DIR / 'RegTexts.cs.resx'
    if args.import_cs:
        tr = json.loads(Path(args.import_cs).read_text(encoding='utf-8'))
        old = read_resx(cs_path)
        entries, comments = {}, {}
        for key, sk in texts.items():
            value = tr.get(key) or (old.get(key) or (None, None))[0]
            if value is None:
                continue
            entries[key] = value
            comments[key] = sk
        write_resx(cs_path, entries, comments)

    cz = read_resx(cs_path)
    missing = [k for k in texts if k not in cz]
    stale = [k for k in texts if k in cz and cz[k][1] is not None and cz[k][1] != texts[k]]
    if missing or stale:
        print(f'CZ: chyba {len(missing)} prekladov, {len(stale)} zastaralych (zmeneny slovensky zdroj)', file=sys.stderr)
        for k in (missing + stale)[:20]:
            print('  ', k, file=sys.stderr)
        sys.exit(1)


if __name__ == '__main__':
    main()
