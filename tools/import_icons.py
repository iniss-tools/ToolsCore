"""
Import ikon SVG do banky ToolsCore (ToolsCore/Icons/Svg).

Zdroj: Fluent UI System Icons (MIT) - github.com/microsoft/fluentui-system-icons, assets/<nazov>/SVG, styl Regular
(obrysy), Filled (vyplnene - stavove ikony ako chyba a varovanie) alebo duotone - farebny obrys Regular nad vyplnou
Filled v tej istej farbe s nizkym krytim (ako farebne ikony Visual Studia, napr. zlaty priecinok, modra disketa).

Fluent ma kazdu ikonu nakreslenu zvlast pre velkosti 16, 20, 24, 28 a 32 bodov (ciary hrubky 1 presne na bodoch),
banka preto obsahuje vsetky, ktore zdroj ma: nazov_<velkost>.svg a variant pre tmave pozadie nazov_<velkost>_dark.svg.
Ikony su jednofarebne - prefarbia sa na sivu (svetla #6C707E, tmava #CED0D6) alebo na akcent (napr. zelene spustenie).

Novu ikonu pridat do ICONS a spustit (z korena repozitara ToolsCore):
  python tools/import_icons.py [nazov ...]
Bez argumentov stiahne vsetky ikony z ICONS. Odznaky kresli tools/gen_badges.py.
"""
import re
import sys
import urllib.error
import urllib.parse
import urllib.request
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
OUT_DIR = ROOT / 'ToolsCore' / 'Icons' / 'Svg'

FLUENT = 'https://raw.githubusercontent.com/microsoft/fluentui-system-icons/main/assets/'
FLUENT_COLOR = '#212121'
NOTICE = '<!-- Fluent UI System Icons, Copyright (c) 2020 Microsoft Corporation. Use of this source code is governed by the MIT license. -->\n'
SIZES = (16, 20, 24, 28, 32)

# farby (svetla, tmava) - rovnake ako odznaky
COLORS = {
    'gray': ('#6C707E', '#CED0D6'),
    'green': ('#208A3C', '#57965C'),
    'red': ('#DB3B4B', '#DB5C5C'),
    'blue': ('#4682FA', '#548AF7'),
    'yellow': ('#E09000', '#F2C55C'),
    'orange': ('#E66D17', '#E08855'),
    'gold': ('#C08A1E', '#DCB67A'),
}

# krytie vyplne dvojfarebnej ikony (duotone)
TINT = '0.3'

# nazov v banke -> (nazov priecinka v assets, farba[, styl regular/filled/duotone])
ICONS = {
    'save': ('Save', 'blue', 'duotone'),
    'folder': ('Folder', 'gold', 'duotone'),
    'folder-open': ('Folder Open', 'gold', 'duotone'),
    'import': ('Arrow Download', 'blue'),
    'history': ('History', 'blue', 'duotone'),
    'undo': ('Arrow Undo', 'gray'),
    'redo': ('Arrow Redo', 'gray'),
    'add': ('Add', 'green'),
    'edit': ('Edit', 'gray'),
    'delete': ('Delete', 'red', 'duotone'),
    'search': ('Search', 'gray'),
    'document-search': ('Document Search', 'gray'),
    'settings': ('Settings', 'gray'),
    'globe': ('Globe', 'blue', 'duotone'),
    'info': ('Info', 'blue', 'filled'),
    'run': ('Play', 'green'),
    'stop': ('Stop', 'red'),
    'kill': ('Dismiss Circle', 'red'),
    'rerun': ('Arrow Clockwise', 'green'),
    'train': ('Vehicle Subway', 'gray'),
    'building': ('Building Government', 'gray'),
    'table': ('Table', 'gray'),
    'calendar': ('Calendar LTR', 'blue', 'duotone'),
    'calendar-sparkle': ('Calendar Sparkle', 'blue', 'duotone'),
    'back': ('Arrow Circle Left', 'blue'),
    'forward': ('Arrow Circle Right', 'blue'),
    'move': ('Folder Arrow Right', 'gold', 'duotone'),
    'link': ('Link', 'gray'),
    'text-wrap': ('Text Wrap', 'gray'),
    'speaker': ('Speaker 2', 'blue'),
    'document': ('Document', 'gray'),
    'local-language': ('Local Language', 'gray'),
    'text-font': ('Text Font', 'gray'),
    'export': ('Arrow Upload', 'blue'),
    'exit': ('Arrow Exit', 'gray'),
    'flowchart': ('Flowchart', 'gray'),
    'code': ('Code', 'gray'),
    'rename': ('Rename', 'gray'),
    'save-all': ('Save Multiple', 'blue', 'duotone'),
    'wrench': ('Wrench', 'gray'),
    'eye': ('Eye', 'gray'),
    'text-indent': ('Text Indent Increase LTR', 'gray'),
    'checkmark': ('Checkmark', 'green'),
    'arrow-up': ('Arrow Up', 'gray'),
    'arrow-down': ('Arrow Down', 'gray'),
    'arrow-reset': ('Arrow Counterclockwise', 'blue'),
    'document-copy': ('Document Copy', 'gray'),
    'document-checkmark': ('Document Checkmark', 'gray'),
    'clock': ('Clock', 'gray'),
    'color': ('Color', 'gray'),
    'button': ('Button', 'gray'),
    'math-formula': ('Math Formula', 'blue'),
    'number-symbol': ('Number Symbol', 'green'),
    'flash': ('Flash', 'yellow'),
    'departure': ('Arrow Export LTR', 'green'),
    'pass-through': ('Arrow Right', 'orange'),
    'arrival': ('Arrow Import', 'red'),
    'pause': ('Pause', 'blue'),
    'send': ('Send', 'blue'),
    'pin': ('Pin', 'gray'),
    'filter': ('Filter', 'gray'),
    'target': ('Target', 'gray'),
    'link-dismiss': ('Link Dismiss', 'gray'),
    'road-cone': ('Road Cone', 'orange'),
    'status-error': ('Error Circle', 'red', 'filled'),
    'status-warning': ('Warning', 'yellow', 'filled'),
    'status-info': ('Info', 'blue', 'filled'),
    'status-ok': ('Checkmark Circle', 'green', 'filled'),
    'status-question': ('Question Circle', 'blue', 'filled'),
}


def fetch(url: str) -> str | None:
    try:
        with urllib.request.urlopen(url, timeout=30) as response:
            return response.read().decode('utf-8')
    except urllib.error.HTTPError as e:
        if e.code == 404:
            return None
        raise


def check(svg: str, url: str) -> str:
    # banka obsahuje len jednoduche tvary - nic, co by sa nacitavalo zvonka alebo spustalo
    if '<svg' not in svg or re.search(r'<script|<image|<foreignObject|href\s*=|<!ENTITY', svg, re.I):
        raise ValueError(f'neocakavany obsah SVG: {url}')
    if FLUENT_COLOR not in svg:
        raise ValueError(f'ikona bez farby {FLUENT_COLOR}: {url}')
    return svg.replace('\r\n', '\n').strip() + '\n'


def write(file: str, svg: str) -> None:
    OUT_DIR.mkdir(parents=True, exist_ok=True)
    # CRLF ako ostatne textove subory repozitara
    (OUT_DIR / file).write_bytes(svg.replace('\n', '\r\n').encode('utf-8'))


def url_of(folder: str, size: int, style: str) -> str:
    snake = folder.lower().replace(' ', '_')
    return FLUENT + urllib.parse.quote(folder) + f'/SVG/ic_fluent_{snake}_{size}_{style}.svg'


def inner(svg: str) -> str:
    return re.search(r'<svg[^>]*>(.*)</svg>', svg, re.S).group(1).strip()


def import_icon(name: str, folder: str, color: str, style: str = 'regular') -> None:
    light, dark = COLORS[color]
    found = []
    for size in SIZES:
        url = url_of(folder, size, 'filled' if style == 'filled' else 'regular')
        svg = fetch(url)
        if svg is None:
            continue
        svg = check(svg, url)
        fill = None
        if style == 'duotone':
            fill_url = url_of(folder, size, 'filled')
            fill = fetch(fill_url)
            fill = inner(check(fill, fill_url)) if fill else None
        for c, suffix in ((light, ''), (dark, '_dark')):
            out = svg.replace(FLUENT_COLOR, c)
            if fill is not None:
                # vypln pod obrysom - rovnaky tvar v tej istej farbe s nizkym krytim
                head = re.search(r'<svg[^>]*>', out).group(0)
                tinted = fill.replace(f'"{FLUENT_COLOR}"', f'"{c}" fill-opacity="{TINT}"')
                out = f'{head}\n{tinted}\n{inner(out)}\n</svg>\n'
            write(f'{name}_{size}{suffix}.svg', NOTICE + out)
        found.append(size)
    if not found:
        raise ValueError(f'ikona neexistuje: {folder}')
    print(f'{name} <- {folder} ({style}): {", ".join(map(str, found))}')


def main() -> None:
    names = sys.argv[1:] or list(ICONS)
    for name in names:
        folder, color, *style = ICONS[name]
        for old in OUT_DIR.glob(f'{name}_*.svg'):
            old.unlink()
        import_icon(name, folder, color, *style)


if __name__ == '__main__':
    main()
