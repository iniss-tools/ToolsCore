"""Ikona desktopoveho programu INISSTools ako dlazdica s textom (gvd, raw, fnt, gtn, gvx).

Pouzitie: python make_app_icon.py TEXT PISMENO16 TMAVA SVETLA VYSTUP.ico [--compare POVODNA.ico]
Priklad:  python make_app_icon.py GVX X "#5500d4" "#ccaaff" ../../GVXEditor/GVXEditor/gvx.ico

Dlazdica 256 px: okraj v tmavej farbe (rx 13.5), vnutro od 26.5 vo svetlej (rx 5), text Bahnschrift Bold v tmavej
farbe, vyska verzaliek 68 px, stred 128. Ramce 24-64 a 256 px kresli Inkscape z jedneho SVG, 16 px ma jedno pismeno
s vyskou verzalky 152. Vedla ICO vznikne aj PNG 256 px (logo v docs). ICO: 16-64 px 32-bitove BMP, 256 px PNG.
Proti gvd.ico je vysledok vizualne zhodny, lisi sa len vyhladzovanie hran pismen.
"""
import io
import os
import struct
import subprocess
import sys
import tempfile

from PIL import Image

INKSCAPE = r'C:\Program Files\Inkscape\bin\inkscape.exe'
CAP = 0.71  # vyska verzalky Bahnschrift Bold k velkosti pisma


def svg(text, dark, light, inner, cap_height):
    size = 256 - 2 * inner
    font = cap_height / CAP
    baseline = 128 + cap_height / 2
    return f'''<svg xmlns="http://www.w3.org/2000/svg" width="256" height="256" viewBox="0 0 256 256">
  <rect x="0" y="0" width="256" height="256" rx="13.5" fill="{dark}"/>
  <rect x="{inner}" y="{inner}" width="{size}" height="{size}" rx="5" fill="{light}"/>
  <text x="128" y="{baseline}" text-anchor="middle" fill="{dark}"
        style="font-family:Bahnschrift;font-weight:bold;font-size:{font}px;font-kerning:normal">{text}</text>
</svg>'''


def render(source, width, work):
    path = os.path.join(work, f'icon{width}.svg')
    out = os.path.join(work, f'icon{width}.png')
    with open(path, 'w', encoding='utf-8') as f:
        f.write(source)
    subprocess.run([INKSCAPE, path, '--export-type=png', f'--export-filename={out}', f'--export-width={width}'], check=True,
                   capture_output=True)
    return Image.open(out).convert('RGBA')


def bmp_entry(image):
    w, h = image.size
    header = struct.pack('<IiiHHIIiiII', 40, w, h * 2, 1, 32, 0, 0, 0, 0, 0, 0)
    pixels = bytearray()
    for y in range(h - 1, -1, -1):
        for x in range(w):
            r, g, b, a = image.getpixel((x, y))
            pixels += bytes((b, g, r, a))
    stride = ((w + 31) // 32) * 4
    mask = bytes(stride * h)
    return header + bytes(pixels) + mask


def png_entry(image):
    buffer = io.BytesIO()
    image.save(buffer, format='PNG')
    return buffer.getvalue()


def build(text, letter, dark, light):
    with tempfile.TemporaryDirectory() as work:
        frames = {16: render(svg(letter, dark, light, 26.5, 152), 16, work)}
        big = svg(text, dark, light, 26.5, 68)
        for width in (24, 32, 48, 64, 256):
            frames[width] = render(big, width, work)
    return frames


def write_ico(frames, path):
    entries = [(w, bmp_entry(frames[w]) if w < 256 else png_entry(frames[w])) for w in sorted(frames)]
    offset = 6 + 16 * len(entries)
    data = struct.pack('<HHH', 0, 1, len(entries))
    body = b''
    for w, blob in entries:
        data += struct.pack('<BBBBHHII', w % 256, w % 256, 0, 0, 1, 32, len(blob), offset + len(body))
        body += blob
    with open(path, 'wb') as f:
        f.write(data + body)


def read_ico(path):
    d = open(path, 'rb').read()
    frames = {}
    for i in range(struct.unpack_from('<HHH', d, 0)[2]):
        w, h, _, _, _, _, size, off = struct.unpack_from('<BBBBHHII', d, 6 + 16 * i)
        w = w or 256
        blob = d[off:off + size]
        if blob[:4] == b'\x89PNG':
            frames[w] = Image.open(io.BytesIO(blob)).convert('RGBA')
        else:
            image = Image.new('RGBA', (w, w))
            for y in range(w):
                for x in range(w):
                    o = 40 + ((w - 1 - y) * w + x) * 4
                    b, g, r, a = blob[o:o + 4]
                    image.putpixel((x, y), (r, g, b, a))
            frames[w] = image
    return frames


def compare(frames, path):
    other = read_ico(path)
    for w in sorted(frames):
        a, b = frames[w], other.get(w)
        if b is None:
            print(f'{w}: chyba v povodnej')
            continue
        diff = sum(1 for p, q in zip(a.getdata(), b.getdata()) if p != q)
        worst = max((max(abs(x - y) for x, y in zip(p, q)) for p, q in zip(a.getdata(), b.getdata())), default=0)
        print(f'{w:3}: rozdielnych pixelov {diff}, najvacsi rozdiel {worst}')


if __name__ == '__main__':
    text, letter, dark, light, out = sys.argv[1:6]
    frames = build(text, letter, dark, light)
    write_ico(frames, out)
    frames[256].save(os.path.splitext(out)[0] + '_256.png')
    if '--compare' in sys.argv:
        compare(frames, sys.argv[sys.argv.index('--compare') + 1])
