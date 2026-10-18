"""
Generator odznakov banky ikon ToolsCore (ToolsCore/Icons/Badges).

Odznak sa kresli do praveho dolneho rohu ikony (polovica sirky), pre kazdu velkost z mriezky ikon Fluent zvlast,
aby boli ciary na celych bodoch: nazov_<velkost>.svg a nazov_<velkost>_dark.svg. Farby su rovnake ako v importe ikon.

Pouzitie (z korena repozitara ToolsCore):
  python tools/gen_badges.py
"""
import math
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
OUT_DIR = ROOT / 'ToolsCore' / 'Icons' / 'Badges'
SIZES = (16, 20, 24, 28, 32)
HEAD = '<!-- Odznak banky ikon ToolsCore (tools/gen_badges.py), pravy dolny roh ikony. -->\n'

GRAY = ('#6C707E', '#CED0D6')
GREEN = ('#208A3C', '#57965C')
RED = ('#DB3B4B', '#DB5C5C')
BLUE = ('#4682FA', '#548AF7')


def num(v: float) -> str:
    return f'{v:.2f}'.rstrip('0').rstrip('.')


def thickness(n: int) -> int:
    # hrubka kriza a plusu - 2 body do velkosti 24, potom 3
    return 2 if n <= 24 else 3


def corner(n: int) -> tuple[float, float, float]:
    """Oblast odznaku (zaciatok, koniec, stred); pri neparnej hrubke o bod mensia, aby bol stred na pol bode."""
    b = n // 2
    t = thickness(n)
    start = n - b + (1 if t % 2 else 0)
    return start, n, (start + n) / 2


def add(n: int, c: str) -> str:
    s, e, m = corner(n)
    h = thickness(n) / 2
    d = (f'M{num(m - h)} {num(s)}H{num(m + h)}V{num(m - h)}H{num(e)}V{num(m + h)}H{num(m + h)}V{num(e)}'
         f'H{num(m - h)}V{num(m + h)}H{num(s)}V{num(m - h)}H{num(m - h)}Z')
    return f'<path d="{d}" fill="{c}"/>'


def delete(n: int, c: str) -> str:
    s, e, _ = corner(n)
    t = thickness(n)
    a, b = s + t / 2, e - t / 2
    return (f'<path d="M{num(a)} {num(a)}L{num(b)} {num(b)}M{num(b)} {num(a)}L{num(a)} {num(b)}" '
            f'stroke="{c}" stroke-width="{t}" stroke-linecap="round"/>')


def edit(n: int, c: str) -> str:
    b = n // 2
    s2 = math.sqrt(0.5)
    tip = (n - b + 0.5, n - 0.5)
    length, half, cone = 1.125 * b, 0.16 * b, 0.3 * b

    def at(along: float, side: float) -> tuple[float, float]:
        return tip[0] + (along + side) * s2, tip[1] + (side - along) * s2

    points = [tip, at(cone, half), at(length, half), at(length, -half), at(cone, -half)]
    return '<path d="M' + 'L'.join(f'{num(x)} {num(y)}' for x, y in points) + f'Z" fill="{c}"/>'


def copy(n: int, c: str) -> str:
    b = n // 2
    stroke = 1 if n <= 24 else 2
    back = round(0.6 * b)
    front = round(0.75 * b)
    o = n - b + stroke / 2
    return (f'<rect x="{num(o)}" y="{num(o)}" width="{back}" height="{back}" rx="1" stroke="{c}" stroke-width="{stroke}"/>\n'
            f'<rect x="{n - front}" y="{n - front}" width="{front}" height="{front}" rx="1" fill="{c}"/>')


def settings(n: int, c: str) -> str:
    b = n // 2
    center = n - b / 2
    outer, inner, hole = b / 2, 0.72 * b / 2, 0.29 * b / 2
    teeth = 6 if n <= 24 else 8
    step = 360 / teeth
    points = []
    for k in range(teeth):
        a = k * step
        for angle, radius in ((a - 0.35 * step, inner), (a - 0.18 * step, outer), (a + 0.18 * step, outer), (a + 0.35 * step, inner)):
            r = math.radians(angle)
            points.append((center + radius * math.cos(r), center + radius * math.sin(r)))
    d = 'M' + 'L'.join(f'{num(x)} {num(y)}' for x, y in points) + 'Z'
    d += (f'M{num(center + hole)} {num(center)}A{num(hole)} {num(hole)} 0 1 0 {num(center - hole)} {num(center)}'
          f'A{num(hole)} {num(hole)} 0 1 0 {num(center + hole)} {num(center)}Z')
    return f'<path fill-rule="evenodd" d="{d}" fill="{c}"/>'


BADGES = {'add': (add, GREEN), 'delete': (delete, RED), 'edit': (edit, BLUE), 'copy': (copy, BLUE), 'settings': (settings, GRAY)}


def main() -> None:
    OUT_DIR.mkdir(parents=True, exist_ok=True)
    for old in OUT_DIR.glob('*.svg'):
        old.unlink()
    for name, (draw, colors) in BADGES.items():
        for n in SIZES:
            for color, suffix in zip(colors, ('', '_dark')):
                svg = (HEAD + f'<svg width="{n}" height="{n}" viewBox="0 0 {n} {n}" fill="none" xmlns="http://www.w3.org/2000/svg">\n'
                       + draw(n, color) + '\n</svg>\n')
                (OUT_DIR / f'{name}_{n}{suffix}.svg').write_bytes(svg.replace('\n', '\r\n').encode('utf-8'))
    print(f'{len(BADGES) * len(SIZES) * 2} odznakov')


if __name__ == '__main__':
    main()
