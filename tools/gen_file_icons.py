"""
Generator ikon typov suborov banky ikon ToolsCore (ToolsCore/Icons/Svg): dokument Fluent so stitkom pripony.

Stitok je farebny obdlznik v dolnej casti ikony s priponou z bodoveho pisma 3x5 (pri 32 bodoch 6x10), aby bola
pripona citatelna aj v 16 bodoch ako pri ikonach suborov Visual Studia. Obrys dokumentu sa berie z ikony `document`
banky - najprv spustit tools/import_icons.py.

Pouzitie (z korena repozitara ToolsCore):
  python tools/gen_file_icons.py
"""
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SVG_DIR = ROOT / 'ToolsCore' / 'Icons' / 'Svg'
SIZES = (16, 20, 24, 28, 32)
HEAD = '<!-- Ikona suboru banky ikon ToolsCore (tools/gen_file_icons.py) - obrys dokumentu z Fluent UI System Icons (MIT). -->\n'
LETTER_COLOR = '#FFFFFF'

# nazov ikony -> (text stitku, farba svetla, farba tmava)
FILES = {
    'file-ewa': ('EWA', '#E66D17', '#E08855'),
    'file-wav': ('WAV', '#4682FA', '#548AF7'),
}

# bodove pismo 3x5
GLYPHS = {
    'A': ['.#.', '#.#', '###', '#.#', '#.#'],
    'E': ['###', '#..', '##.', '#..', '###'],
    'V': ['#.#', '#.#', '#.#', '#.#', '.#.'],
    'W': ['#.#', '#.#', '#.#', '###', '#.#'],
}


def inner(svg: str) -> str:
    return re.search(r'<svg[^>]*>(.*)</svg>', svg, re.S).group(1).strip()


def label(text: str, n: int, color: str) -> str:
    s = 2 if n >= 32 else 1
    width, height = (len(text) * 4 + 1) * s, 7 * s
    x0 = (n - width) // 2
    y0 = n - height - (n - 16) // 4
    parts = [f'<rect x="{x0}" y="{y0}" width="{width}" height="{height}" rx="{s}" fill="{color}"/>']
    for i, ch in enumerate(text):
        for row, line in enumerate(GLYPHS[ch]):
            for col, bit in enumerate(line):
                if bit == '#':
                    parts.append(f'<rect x="{x0 + s * (1 + i * 4 + col)}" y="{y0 + s * (1 + row)}" width="{s}" height="{s}" fill="{LETTER_COLOR}"/>')
    return '\n'.join(parts)


def main() -> None:
    for name, (text, light, dark) in FILES.items():
        for n in SIZES:
            for color, suffix in ((light, ''), (dark, '_dark')):
                source = SVG_DIR / f'document_{n}{suffix}.svg'
                if not source.exists():
                    continue
                document = inner(source.read_text(encoding='utf-8'))
                svg = (HEAD + f'<svg width="{n}" height="{n}" viewBox="0 0 {n} {n}" fill="none" xmlns="http://www.w3.org/2000/svg">\n'
                       + document + '\n' + label(text, n, color) + '\n</svg>\n')
                (SVG_DIR / f'{name}_{n}{suffix}.svg').write_bytes(svg.replace('\n', '\r\n').encode('utf-8'))
        print(name)


if __name__ == '__main__':
    main()
