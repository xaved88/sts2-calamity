"""
Writes placeholder SVGs for hardships and powers into tools/art/src, in one shared style: dark ink outlines, a cursed
palette, objects on a transparent background like vanilla relic icons. Existing SVGs are never overwritten, so any file
can be hand-edited or replaced with real art afterwards.

Usage: python -I tools/art/make_placeholders.py
"""
from pathlib import Path

SRC = Path(__file__).resolve().parent / "src"

INK = "#1d1418"
BONE, CLOTH, LEATHER, DARKLEATHER = "#efe3c8", "#b5a77e", "#8b5a2e", "#4a2a17"
WOOD, DARKWOOD, IRON, DARKIRON, RUST = "#a06a38", "#5c3a1e", "#8a8496", "#4a4454", "#a2502a"
RED, BLOOD, GOLD, GREEN, MOSS, PURPLE, BLUE, ICE, SLIME = (
    "#d8323a", "#8e0f1c", "#e3b341", "#6f9a3c", "#3f5e2a", "#6b3a85", "#3d6fa3", "#9fd3ef", "#8fbf3a")


def stroke(width=8):
    return f'stroke="{INK}" stroke-width="{width}" stroke-linejoin="round" stroke-linecap="round"'


S = stroke()
S5 = stroke(5)

RELICS = {
    "nettle_shirt": f'''
  <path d="M84 52 L112 40 Q128 58 144 40 L172 52 L212 92 L188 116 L174 102 L174 214 L82 214 L82 102 L68 116 L44 92 Z" fill="#7d9b4c" {S}/>
  <g fill="{GREEN}" {S5}><path d="M96 130 l-12 -8 l14 -2z"/><path d="M150 150 l14 -6 l-4 14z"/><path d="M110 180 l-14 4 l8 -12z"/><path d="M146 104 l10 -10 l2 14z"/><path d="M124 196 l6 12 l-12 -2z"/></g>''',
    "lumpy_cot": f'''
  <path d="M40 150 L216 150 L216 176 L40 176 Z" fill="{WOOD}" {S}/>
  <path d="M48 176 L48 214 M208 176 L208 214" {S}/>
  <path d="M44 150 C60 112 86 136 104 116 C124 96 140 132 162 112 C184 94 204 120 212 150 Z" fill="{CLOTH}" {S}/>
  <path d="M72 132 c6 -6 14 -6 18 2 M136 124 c6 -8 14 -6 18 2" fill="none" {S5}/>''',
    "heavy_pack": f'''
  <path d="M70 80 C70 50 186 50 186 80 L196 206 C196 220 60 220 60 206 Z" fill="{LEATHER}" {S}/>
  <path d="M74 112 L182 112 L182 150 L74 150 Z" fill="{DARKLEATHER}" {S}/>
  <path d="M100 62 C100 30 156 30 156 62" fill="none" {S}/>
  <rect x="114" y="122" width="28" height="18" rx="4" fill="{GOLD}" {S5}/>
  <path d="M96 180 l64 0" {S5}/>''',
    "leaky_flask": f'''
  <path d="M106 36 L150 36 L150 90 C200 110 206 196 128 212 C50 196 56 110 106 90 Z" fill="#cfe7ef" {S}/>
  <path d="M68 150 C80 196 176 196 188 150 C160 160 96 160 68 150 Z" fill="{RED}" opacity="0.9"/>
  <path d="M146 112 l-10 18 l12 8 l-8 16" fill="none" {S5}/>
  <path d="M180 196 C176 210 196 210 192 196 L186 184 Z" fill="{RED}" {S5}/>
  <rect x="98" y="24" width="60" height="18" rx="4" fill="{DARKWOOD}" {S}/>''',
    "flea_ridden_blanket": f'''
  <path d="M40 92 L216 76 L216 196 L40 212 Z" fill="#8a6fa6" {S}/>
  <path d="M40 132 L216 116 M40 172 L216 156" stroke="#5b4a6e" stroke-width="10"/>
  <path d="M40 92 L216 76 L216 196 L40 212 Z" fill="none" {S}/>
  <g fill="{INK}"><circle cx="86" cy="118" r="5"/><circle cx="150" cy="104" r="5"/><circle cx="120" cy="160" r="5"/><circle cx="182" cy="146" r="5"/><circle cx="76" cy="186" r="5"/><circle cx="170" cy="184" r="5"/></g>''',
    "rusted_hinge": f'''
  <path d="M40 80 L120 80 L120 176 L40 176 Z" fill="{IRON}" {S}/>
  <path d="M136 80 L216 80 L216 176 L136 176 Z" fill="{IRON}" {S}/>
  <rect x="116" y="60" width="24" height="136" rx="10" fill="{DARKIRON}" {S}/>
  <g fill="{DARKIRON}"><circle cx="66" cy="106" r="7"/><circle cx="66" cy="150" r="7"/><circle cx="190" cy="106" r="7"/><circle cx="190" cy="150" r="7"/></g>
  <g fill="{RUST}" opacity="0.85"><ellipse cx="92" cy="130" rx="18" ry="12"/><ellipse cx="170" cy="160" rx="14" ry="9"/><ellipse cx="128" cy="96" rx="10" ry="16"/></g>''',
    "bloodied_signpost": f'''
  <rect x="116" y="70" width="24" height="152" fill="{DARKWOOD}" {S}/>
  <path d="M52 52 L188 52 L216 82 L188 112 L52 112 Z" fill="{WOOD}" {S}/>
  <path d="M104 72 C104 52 140 50 140 70 C140 84 122 86 122 98" fill="none" stroke="{INK}" stroke-width="10" stroke-linecap="round"/>
  <circle cx="122" cy="106" r="2" fill="{INK}" stroke="{INK}" stroke-width="8"/>
  <path d="M60 100 C80 120 70 150 90 160 C96 140 104 124 98 108 Z" fill="{BLOOD}"/>
  <path d="M170 60 C178 80 160 92 176 104" fill="none" stroke="{BLOOD}" stroke-width="8" stroke-linecap="round"/>''',
    "spiteful_totem": f'''
  <path d="M80 40 L176 40 L176 216 L80 216 Z" fill="{WOOD}" {S}/>
  <path d="M80 128 L176 128" {S}/>
  <path d="M92 70 L120 82 M164 70 L136 82" {S}/>
  <path d="M96 92 L120 96 L118 108 L96 104 Z M160 92 L136 96 L138 108 L160 104 Z" fill="{RED}" {S5}/>
  <path d="M100 172 L156 172 L146 196 L110 196 Z" fill="{INK}"/>
  <path d="M108 172 l6 10 l6 -10 l6 10 l6 -10 l6 10 l6 -10" fill="none" stroke="{BONE}" stroke-width="4"/>
  <path d="M60 140 L80 150 L80 120 Z M196 140 L176 150 L176 120 Z" fill="{DARKWOOD}" {S5}/>''',
    "briar_crown": f'''
  <ellipse cx="128" cy="140" rx="90" ry="44" fill="none" stroke="{INK}" stroke-width="30"/>
  <ellipse cx="128" cy="140" rx="90" ry="44" fill="none" stroke="{MOSS}" stroke-width="16"/>
  <g fill="{BONE}" {S5}><path d="M48 118 l-14 -22 l26 10z"/><path d="M92 98 l-4 -28 l18 20z"/><path d="M146 96 l8 -28 l10 26z"/><path d="M196 110 l22 -18 l-6 26z"/><path d="M200 168 l22 10 l-24 6z"/><path d="M120 186 l0 26 l-14 -22z"/><path d="M64 172 l-22 12 l10 -22z"/></g>''',
    "war_drum": f'''
  <path d="M56 100 L200 100 L188 200 L68 200 Z" fill="{RED}" {S}/>
  <ellipse cx="128" cy="100" rx="72" ry="24" fill="{BONE}" {S}/>
  <path d="M68 116 L100 196 L132 116 L164 196 L196 116" fill="none" stroke="{GOLD}" stroke-width="6"/>
  <path d="M68 200 C100 212 156 212 188 200" fill="none" {S}/>
  <path d="M150 36 L104 92 M210 52 L148 98" stroke="{INK}" stroke-width="16" stroke-linecap="round"/>
  <path d="M150 36 L104 92 M210 52 L148 98" stroke="{WOOD}" stroke-width="8" stroke-linecap="round"/>
  <circle cx="150" cy="36" r="11" fill="{BONE}" {S5}/><circle cx="210" cy="52" r="11" fill="{BONE}" {S5}/>''',
    "mourning_bell": f'''
  <path d="M128 40 C80 40 76 100 72 150 C70 176 52 182 48 196 L208 196 C204 182 186 176 184 150 C180 100 176 40 128 40 Z" fill="#6f6a7c" {S}/>
  <path d="M60 180 L196 180" stroke="{DARKIRON}" stroke-width="10"/>
  <rect x="114" y="22" width="28" height="22" rx="8" fill="{DARKIRON}" {S5}/>
  <circle cx="128" cy="210" r="16" fill="{DARKIRON}" {S}/>
  <path d="M98 70 C92 100 92 130 94 160" fill="none" stroke="#a9a3b8" stroke-width="8" stroke-linecap="round"/>''',
    "martyrs_fuse": f'''
  <circle cx="112" cy="150" r="66" fill="#3b3644" {S}/>
  <rect x="134" y="66" width="36" height="28" rx="6" transform="rotate(35 152 80)" fill="{DARKIRON}" {S}/>
  <path d="M164 70 C188 48 196 70 214 46" fill="none" {S}/>
  <path d="M214 46 l-8 -20 l14 12 l8 -18 l2 22 l18 -4 l-16 14 l14 12 l-20 -6 l-6 18 z" fill="{GOLD}" stroke="{RED}" stroke-width="4" stroke-linejoin="round"/>
  <path d="M80 120 C76 134 76 150 82 162" fill="none" stroke="#6a6378" stroke-width="10" stroke-linecap="round"/>''',
    "wardens_seal": f'''
  <path d="M128 36 C150 50 178 34 190 58 C214 66 206 96 222 112 C214 136 228 160 204 176 C200 204 170 200 154 218 C128 210 104 226 88 206 C60 206 58 178 38 164 C42 138 28 116 48 98 C50 70 80 68 96 46 C108 44 118 38 128 36 Z" fill="{BLOOD}" {S}/>
  <circle cx="128" cy="128" r="58" fill="{RED}" {S5}/>
  <path d="M128 84 L156 98 L156 132 C156 152 140 166 128 172 C116 166 100 152 100 132 L100 98 Z" fill="{BLOOD}" {S5}/>
  <path d="M128 104 L128 152 M112 124 L144 124" stroke="{GOLD}" stroke-width="7" stroke-linecap="round"/>''',
    "crooked_scale": f'''
  <path d="M128 50 L128 200" {S}/>
  <path d="M84 210 L172 210" stroke="{INK}" stroke-width="14" stroke-linecap="round"/>
  <path d="M84 210 L172 210" stroke="{GOLD}" stroke-width="6" stroke-linecap="round"/>
  <path d="M44 78 L212 112" stroke="{INK}" stroke-width="14" stroke-linecap="round"/>
  <path d="M44 78 L212 112" stroke="{GOLD}" stroke-width="6" stroke-linecap="round"/>
  <path d="M44 78 L26 136 M44 78 L62 136 M212 112 L194 170 M212 112 L230 170" stroke="{INK}" stroke-width="4"/>
  <path d="M20 136 C24 154 64 154 68 136 Z" fill="{GOLD}" {S5}/>
  <path d="M188 170 C192 188 232 188 236 170 Z" fill="{GOLD}" {S5}/>
  <circle cx="128" cy="44" r="12" fill="{GOLD}" {S5}/>''',
    "bramble_signpost": f'''
  <rect x="116" y="70" width="24" height="152" fill="{DARKWOOD}" {S}/>
  <path d="M68 52 L204 52 L204 112 L68 112 L40 82 Z" fill="{WOOD}" {S}/>
  <path d="M120 72 C120 52 156 50 156 70 C156 84 138 86 138 98" fill="none" stroke="{INK}" stroke-width="10" stroke-linecap="round"/>
  <circle cx="138" cy="106" r="2" fill="{INK}" stroke="{INK}" stroke-width="8"/>
  <path d="M100 222 C80 190 160 176 120 150 C90 130 170 120 150 96 C140 80 180 66 196 40" fill="none" stroke="{INK}" stroke-width="16" stroke-linecap="round"/>
  <path d="M100 222 C80 190 160 176 120 150 C90 130 170 120 150 96 C140 80 180 66 196 40" fill="none" stroke="{MOSS}" stroke-width="8" stroke-linecap="round"/>
  <g fill="{BONE}" {S5}><path d="M98 190 l-14 -4 l10 -10z"/><path d="M146 166 l14 0 l-8 12z"/><path d="M112 140 l-14 -6 l12 -8z"/><path d="M168 74 l14 -2 l-6 12z"/></g>''',
    "lead_bookmark": f'''
  <path d="M36 70 C80 56 112 64 128 80 C144 64 176 56 220 70 L220 200 C176 186 144 194 128 210 C112 194 80 186 36 200 Z" fill="{BONE}" {S}/>
  <path d="M128 80 L128 210" {S5}/>
  <path d="M60 96 L110 92 M60 120 L110 116 M60 144 L110 140 M146 92 L196 96 M146 116 L196 120" stroke="#9c8f74" stroke-width="5" stroke-linecap="round"/>
  <path d="M152 40 L188 40 L188 150 L170 136 L152 150 Z" fill="#6e6a72" {S}/>''',
    "pollen_sachet": f'''
  <path d="M96 100 C40 120 52 214 128 214 C204 214 216 120 160 100 Z" fill="#d8c27a" {S}/>
  <path d="M90 88 C108 76 148 76 166 88 L160 104 C144 98 112 98 96 104 Z" fill="#b39a4a" {S5}/>
  <path d="M110 76 C100 50 126 40 128 64" fill="none" stroke="{GREEN}" stroke-width="6"/>
  <g fill="{GOLD}" stroke="#a87f1a" stroke-width="2"><circle cx="150" cy="56" r="8"/><circle cx="176" cy="40" r="6"/><circle cx="190" cy="70" r="7"/><circle cx="168" cy="84" r="5"/><circle cx="206" cy="46" r="5"/></g>''',
    "thistle_grip": f'''
  <path d="M128 220 C126 180 132 150 128 120" fill="none" stroke="{INK}" stroke-width="16" stroke-linecap="round"/>
  <path d="M128 220 C126 180 132 150 128 120" fill="none" stroke="{GREEN}" stroke-width="8" stroke-linecap="round"/>
  <path d="M128 178 C96 166 80 182 66 160 C92 150 110 156 128 170 Z M128 158 C160 146 176 160 192 140 C166 132 146 138 128 150 Z" fill="{GREEN}" {S5}/>
  <path d="M92 120 C92 86 164 86 164 120 C164 140 92 140 92 120 Z" fill="{GREEN}" {S}/>
  <path d="M96 100 C96 50 112 30 128 24 C144 30 160 50 160 100 Z" fill="{PURPLE}" {S}/>
  <path d="M112 96 L106 50 M128 96 L128 36 M144 96 L150 50" stroke="#a477c0" stroke-width="5" stroke-linecap="round"/>''',
    "puffball_spore": f'''
  <path d="M84 150 L172 150 L160 214 L96 214 Z" fill="{BONE}" {S}/>
  <path d="M40 152 C34 80 222 80 216 152 Z" fill="#c7b38a" {S}/>
  <g fill="#9c8660"><circle cx="90" cy="124" r="9"/><circle cx="132" cy="110" r="11"/><circle cx="172" cy="128" r="8"/></g>
  <g fill="{SLIME}" opacity="0.75"><circle cx="70" cy="56" r="20"/><circle cx="104" cy="40" r="24"/><circle cx="146" cy="44" r="20"/><circle cx="180" cy="62" r="16"/></g>''',
    "ironbark_sap": f'''
  <path d="M60 60 L176 44 L196 196 L80 212 Z" fill="#6b5844" {S}/>
  <path d="M84 80 C96 120 80 160 96 196 M120 66 C134 110 118 150 134 200 M156 56 C168 100 154 150 172 196" fill="none" stroke="#3e3226" stroke-width="7" stroke-linecap="round"/>
  <path d="M188 104 C176 130 200 150 206 170 C214 150 224 128 206 104 Z" fill="{GOLD}" {S5}/>
  <path d="M198 186 C192 200 214 204 210 190 L204 176 Z" fill="{GOLD}" {S5}/>''',
    "blighted_seeds": f'''
  <g {S}>
    <ellipse cx="88" cy="110" rx="28" ry="40" transform="rotate(-20 88 110)" fill="#5a4a2e"/>
    <ellipse cx="164" cy="96" rx="26" ry="38" transform="rotate(25 164 96)" fill="#4a3d26"/>
    <ellipse cx="124" cy="178" rx="30" ry="40" transform="rotate(8 124 178)" fill="#5f5031"/>
  </g>
  <g fill="{SLIME}" opacity="0.8"><circle cx="80" cy="96" r="9"/><circle cx="172" cy="110" r="8"/><circle cx="116" cy="168" r="10"/><circle cx="136" cy="196" r="6"/></g>
  <path d="M94 76 C110 60 104 44 120 36 M170 62 C180 48 176 34 188 28" fill="none" stroke="#3a2f20" stroke-width="5" stroke-linecap="round"/>''',
    "damp_powder_horn": f'''
  <path d="M40 80 C80 70 150 90 200 170 L216 164 L222 196 L196 200 C150 130 90 116 52 120 Z" fill="{BONE}" {S}/>
  <path d="M70 76 L74 122 M110 82 L112 124" stroke="#a8977a" stroke-width="6"/>
  <rect x="26" y="74" width="28" height="52" rx="8" fill="{DARKWOOD}" {S}/>
  <g fill="{BLUE}" {S5}><path d="M150 52 C140 72 164 76 160 58 L154 40 Z"/><path d="M186 72 C178 88 198 90 196 76 L190 62 Z"/><path d="M118 40 C112 54 130 56 128 44 L122 30 Z"/></g>''',
    "bilge_bucket": f'''
  <path d="M60 82 L196 82 L178 212 L78 212 Z" fill="{WOOD}" {S}/>
  <path d="M66 126 L190 126 M72 172 L184 172" stroke="{DARKIRON}" stroke-width="10"/>
  <ellipse cx="128" cy="82" rx="68" ry="18" fill="{SLIME}" {S}/>
  <path d="M90 92 C86 120 104 126 100 96 Z M160 94 C164 130 146 136 150 98 Z" fill="{SLIME}" {S5}/>
  <path d="M60 82 C60 30 196 30 196 82" fill="none" {S}/>''',
    "rotten_crate": f'''
  <path d="M44 74 L212 74 L212 206 L44 206 Z" fill="{WOOD}" {S}/>
  <path d="M44 118 L212 118 M44 162 L212 162" {S5}/>
  <path d="M44 74 L212 206 M212 74 L44 206" stroke="{DARKWOOD}" stroke-width="10"/>
  <path d="M44 74 L212 74 L212 206 L44 206 Z" fill="none" {S}/>
  <g fill="{SLIME}" opacity="0.85"><ellipse cx="80" cy="100" rx="22" ry="12"/><ellipse cx="176" cy="150" rx="26" ry="14"/><ellipse cx="110" cy="190" rx="18" ry="10"/></g>
  <path d="M150 56 C156 40 172 50 166 34" fill="none" stroke="{MOSS}" stroke-width="6" stroke-linecap="round"/>''',
    "cracked_hull_plank": f'''
  <path d="M30 96 L226 70 L226 140 L30 166 Z" fill="{WOOD}" {S}/>
  <path d="M30 120 L226 94" stroke="{DARKWOOD}" stroke-width="5"/>
  <g fill="{DARKIRON}"><circle cx="52" cy="110" r="7"/><circle cx="204" cy="90" r="7"/></g>
  <path d="M120 84 L134 108 L118 122 L136 148" fill="none" {S}/>
  <g fill="{BLUE}" {S5}><path d="M128 158 C116 182 144 188 140 166 L134 150 Z"/><path d="M104 182 C96 200 118 204 116 188 L110 176 Z"/><path d="M150 194 C142 212 164 216 162 200 L156 188 Z"/></g>''',
    "rusted_buckler": f'''
  <circle cx="128" cy="128" r="90" fill="{IRON}" {S}/>
  <circle cx="128" cy="128" r="28" fill="{DARKIRON}" {S}/>
  <g fill="{RUST}" opacity="0.9"><ellipse cx="80" cy="96" rx="26" ry="18"/><ellipse cx="170" cy="168" rx="30" ry="20"/><ellipse cx="164" cy="80" rx="14" ry="10"/><ellipse cx="86" cy="170" rx="16" ry="12"/></g>
  <g fill="{INK}"><circle cx="70" cy="140" r="8"/><circle cx="182" cy="124" r="7"/><circle cx="124" cy="196" r="6"/></g>''',
    "tiny_pouch": f'''
  <g transform="translate(128 150) scale(1.7) translate(-128 -150)">
  <path d="M106 120 C76 132 82 186 128 186 C174 186 180 132 150 120 Z" fill="{LEATHER}" {S}/>
  <path d="M102 112 C114 104 142 104 154 112 L150 124 C138 120 118 120 106 124 Z" fill="{DARKLEATHER}" {S5}/>
  <path d="M140 110 C156 100 168 112 160 124" fill="none" stroke="{GOLD}" stroke-width="5"/>
  <circle cx="128" cy="154" r="10" fill="{GOLD}" {S5}/>
  </g>''',
    "hive_nectar": f'''
  <path d="M60 90 L196 90 L188 208 C150 220 106 220 68 208 Z" fill="#e8a83a" {S}/>
  <path d="M60 90 C60 70 196 70 196 90" fill="#c98a24" {S}/>
  <path d="M72 120 L184 120 M74 156 L182 156 M78 190 L178 190" stroke="#b9782a" stroke-width="6"/>
  <path d="M104 92 C100 118 120 126 116 96 Z M150 92 C154 124 136 130 140 98 Z" fill="#f6c650" {S5}/>
  <g transform="translate(184 52)" {S5}><ellipse cx="0" cy="0" rx="16" ry="11" fill="{GOLD}"/><path d="M-6 -10 L-6 10 M4 -10 L4 10" stroke="{INK}" stroke-width="4"/><ellipse cx="-6" cy="-16" rx="10" ry="7" fill="#e8f4fb"/><ellipse cx="8" cy="-16" rx="10" ry="7" fill="#e8f4fb"/></g>''',
    "dull_whetstone": f'''
  <path d="M40 140 C40 100 70 86 128 86 C186 86 216 100 216 140 C216 176 186 188 128 188 C70 188 40 176 40 140 Z" fill="#8f8a84" {S}/>
  <path d="M60 120 C100 108 160 108 196 120" fill="none" stroke="#b0aba5" stroke-width="10" stroke-linecap="round"/>
  <path d="M76 160 L116 154 M140 166 L182 156" stroke="#6b6660" stroke-width="6" stroke-linecap="round"/>
  <path d="M150 40 L196 100" stroke="{INK}" stroke-width="18" stroke-linecap="round"/>
  <path d="M150 40 L196 100" stroke="#c9c4be" stroke-width="10" stroke-linecap="round"/>''',
    "clouded_orb": f'''
  <circle cx="128" cy="116" r="80" fill="#4a5e8a" {S}/>
  <g fill="#c9cbd6" opacity="0.9"><ellipse cx="100" cy="100" rx="40" ry="18"/><ellipse cx="150" cy="130" rx="44" ry="20"/><ellipse cx="116" cy="150" rx="30" ry="14"/></g>
  <circle cx="128" cy="116" r="80" fill="none" {S}/>
  <path d="M80 210 L176 210 L160 186 L96 186 Z" fill="{DARKWOOD}" {S}/>''',
    "trembling_gauntlet": f'''
  <path d="M84 216 L84 140 L70 104 L84 98 L98 128 L98 70 L116 70 L118 120 L120 56 L140 56 L140 120 L144 66 L162 68 L160 126 L170 92 L188 98 L174 158 L172 216 Z" fill="{IRON}" {S}/>
  <path d="M84 176 L172 176" stroke="{DARKIRON}" stroke-width="8"/>
  <path d="M44 90 C36 110 36 130 44 150 M210 90 C218 110 218 130 210 150 M30 74 C18 106 18 140 30 170" fill="none" stroke="{INK}" stroke-width="6" stroke-linecap="round"/>''',
    "blue_skull": f'''
  <path d="M128 40 C70 40 46 80 50 124 C52 146 64 156 74 162 L74 200 L182 200 L182 162 C192 156 204 146 206 124 C210 80 186 40 128 40 Z" fill="{BLUE}" {S}/>
  <g fill="{INK}"><ellipse cx="96" cy="124" rx="20" ry="22"/><ellipse cx="160" cy="124" rx="20" ry="22"/><path d="M128 146 L116 168 L140 168 Z"/></g>
  <g fill="{ICE}"><circle cx="96" cy="124" r="6"/><circle cx="160" cy="124" r="6"/></g>
  <path d="M94 200 L94 184 M116 200 L116 184 M140 200 L140 184 M162 200 L162 184" {S5}/>''',
    "iron_manacle": f'''
  <circle cx="92" cy="140" r="52" fill="none" stroke="{INK}" stroke-width="34"/>
  <circle cx="92" cy="140" r="52" fill="none" stroke="{IRON}" stroke-width="20"/>
  <path d="M142 128 L180 116 M180 116 C196 104 216 116 210 134 C206 150 186 152 178 140" fill="none" stroke="{INK}" stroke-width="22" stroke-linecap="round"/>
  <path d="M142 128 L180 116 M180 116 C196 104 216 116 210 134 C206 150 186 152 178 140" fill="none" stroke="{DARKIRON}" stroke-width="10" stroke-linecap="round"/>
  <rect x="78" y="70" width="28" height="28" rx="4" fill="{DARKIRON}" {S5}/>''',
    "frozen_ember": f'''
  <path d="M128 30 C150 70 196 90 190 150 C186 196 150 218 128 218 C106 218 70 196 66 150 C60 90 106 70 128 30 Z" fill="{ICE}" {S}/>
  <path d="M128 92 C140 116 162 128 158 160 C154 184 138 192 128 192 C118 192 102 184 98 160 C94 128 116 116 128 92 Z" fill="#e8743a" {S5}/>
  <path d="M86 110 L70 92 M174 116 L192 98 M80 176 L62 186 M178 178 L196 190" stroke="#e8f6fd" stroke-width="7" stroke-linecap="round"/>''',
    "doom_clock": f'''
  <circle cx="128" cy="132" r="84" fill="#3a2a4a" {S}/>
  <circle cx="128" cy="132" r="64" fill="#d9cfe6" {S5}/>
  <path d="M128 132 L128 86 M128 132 L160 150" stroke="{INK}" stroke-width="9" stroke-linecap="round"/>
  <g fill="{INK}"><circle cx="128" cy="78" r="5"/><circle cx="182" cy="132" r="5"/><circle cx="128" cy="186" r="5"/><circle cx="74" cy="132" r="5"/></g>
  <path d="M76 52 L60 34 M180 52 L196 34" stroke="{INK}" stroke-width="12" stroke-linecap="round"/>
  <circle cx="128" cy="132" r="9" fill="{PURPLE}" {S5}/>''',
    "guttering_brazier": f'''
  <path d="M60 120 L196 120 L176 176 L80 176 Z" fill="{DARKIRON}" {S}/>
  <path d="M96 176 L80 218 M160 176 L176 218 M128 176 L128 218" {S}/>
  <path d="M84 122 C70 90 104 82 98 54 C124 72 120 96 132 92 C140 70 150 60 148 40 C176 66 186 100 172 122 Z" fill="#e8743a" {S5}/>
  <path d="M110 120 C104 104 120 96 118 82 C132 96 132 108 140 106 C146 96 152 100 150 120 Z" fill="{GOLD}"/>
  <path d="M190 140 C204 152 204 170 194 182 C186 170 184 156 190 140 Z" fill="#d9cfa0" {S5}/>''',
    "ballast_box": f'''
  <path d="M48 96 L208 96 L208 208 L48 208 Z" fill="{DARKIRON}" {S}/>
  <path d="M40 80 L216 80 L216 104 L40 104 Z" fill="{IRON}" {S}/>
  <path d="M48 150 L208 150" stroke="{INK}" stroke-width="6"/>
  <g fill="{IRON}" {S5}><circle cx="70" cy="126" r="7"/><circle cx="186" cy="126" r="7"/><circle cx="70" cy="180" r="7"/><circle cx="186" cy="180" r="7"/></g>
  <path d="M100 56 L156 56 L170 80 L86 80 Z" fill="#5c5866" {S}/>''',
    "bound_locket": f'''
  <path d="M80 40 C100 70 156 70 176 40" fill="none" stroke="{GOLD}" stroke-width="8" stroke-linecap="round"/>
  <path d="M128 70 C70 70 60 130 76 160 C92 194 128 214 128 214 C128 214 164 194 180 160 C196 130 186 70 128 70 Z" fill="{GOLD}" {S}/>
  <path d="M128 90 C92 90 86 130 96 150 C106 172 128 186 128 186 C128 186 150 172 160 150 C170 130 164 90 128 90 Z" fill="{BLOOD}" {S5}/>
  <path d="M64 120 L192 160 M64 160 L192 120" stroke="{INK}" stroke-width="8" stroke-linecap="round"/>
  <path d="M64 120 L192 160 M64 160 L192 120" stroke="#6e6a72" stroke-width="4" stroke-linecap="round"/>''',
    "broken_glasses": f'''
  <circle cx="80" cy="132" r="44" fill="#cfe7ef" {S}/>
  <circle cx="176" cy="132" r="44" fill="#cfe7ef" {S}/>
  <path d="M124 128 C128 118 128 118 132 128" fill="none" {S}/>
  <path d="M36 120 L14 100 M220 120 L242 100" {S}/>
  <path d="M60 104 L84 130 L70 146 L94 170 M152 100 L176 128" fill="none" stroke="{INK}" stroke-width="4"/>
  <path d="M168 150 L200 140 L186 168 Z" fill="#9fc4d1" {S5}/>''',
    "hungry_jar": f'''
  <path d="M80 70 L176 70 L184 96 C214 120 214 196 190 210 L66 210 C42 196 42 120 72 96 Z" fill="#9db7a8" {S}/>
  <rect x="72" y="50" width="112" height="24" rx="6" fill="{DARKWOOD}" {S}/>
  <path d="M84 150 C96 132 112 160 128 144 C144 128 160 160 172 146 L172 196 L84 196 Z" fill="#3a2f3a"/>
  <g fill="{BONE}"><path d="M98 168 l8 -14 l8 14 z"/><path d="M118 168 l8 -14 l8 14 z"/><path d="M138 168 l8 -14 l8 14 z"/></g>
  <g fill="{RED}"><circle cx="108" cy="130" r="6"/><circle cx="150" cy="128" r="6"/></g>''',
    "kudzu_cutting": f'''
  <path d="M128 222 C118 180 140 150 124 110 C112 80 140 58 132 30" fill="none" stroke="{INK}" stroke-width="16" stroke-linecap="round"/>
  <path d="M128 222 C118 180 140 150 124 110 C112 80 140 58 132 30" fill="none" stroke="{MOSS}" stroke-width="8" stroke-linecap="round"/>
  <g fill="{GREEN}" {S5}>
    <path d="M128 176 C96 176 76 150 82 130 C110 132 126 152 128 176 Z"/>
    <path d="M128 140 C158 142 180 120 176 98 C148 98 132 118 128 140 Z"/>
    <path d="M128 86 C102 84 88 64 92 46 C114 48 126 66 128 86 Z"/>
  </g>''',
    "soggy_bandolier": f'''
  <path d="M40 60 C100 120 156 150 222 196" stroke="{INK}" stroke-width="40" stroke-linecap="round" fill="none"/>
  <path d="M40 60 C100 120 156 150 222 196" stroke="{LEATHER}" stroke-width="28" stroke-linecap="round" fill="none"/>
  <g {S5}>
    <rect x="70" y="86" width="22" height="40" rx="6" fill="#a8d0e0" transform="rotate(38 81 106)"/>
    <rect x="112" y="116" width="22" height="40" rx="6" fill="{RED}" transform="rotate(34 123 136)"/>
    <rect x="156" y="144" width="22" height="40" rx="6" fill="{GREEN}" transform="rotate(30 167 164)"/>
  </g>
  <g fill="{BLUE}" {S5}><path d="M90 160 C82 176 102 180 100 166 L94 152 Z"/><path d="M140 190 C132 206 152 210 150 196 L144 182 Z"/></g>''',
    "smugglers_padlock": f'''
  <path d="M84 110 L84 80 C84 30 172 30 172 80 L172 110" fill="none" stroke="{INK}" stroke-width="30"/>
  <path d="M84 110 L84 80 C84 30 172 30 172 80 L172 110" fill="none" stroke="{IRON}" stroke-width="16"/>
  <rect x="56" y="104" width="144" height="110" rx="16" fill="{GOLD}" {S}/>
  <circle cx="128" cy="148" r="14" fill="{INK}"/>
  <path d="M128 156 L120 190 L136 190 Z" fill="{INK}"/>''',
    "shieldbreaker_spike": f'''
  <path d="M128 24 L156 150 L128 232 L100 150 Z" fill="{IRON}" {S}/>
  <path d="M128 24 L128 232" stroke="{DARKIRON}" stroke-width="5"/>
  <path d="M60 140 L196 140 L196 164 L60 164 Z" fill="{DARKIRON}" {S}/>
  <g fill="#c9c4be" {S5}><path d="M40 92 l24 10 l-10 18z"/><path d="M196 196 l20 18 l-24 4z"/><path d="M200 80 l20 -14 l-2 22z"/></g>''',
    "vakuus_sextant": f'''
  <path d="M52 196 C100 224 156 224 204 196" fill="none" stroke="{INK}" stroke-width="22"/>
  <path d="M52 196 C100 224 156 224 204 196" fill="none" stroke="{GOLD}" stroke-width="12"/>
  <path d="M128 40 L52 196 M128 40 L204 196" stroke="{INK}" stroke-width="18" stroke-linecap="round"/>
  <path d="M128 40 L52 196 M128 40 L204 196" stroke="{GOLD}" stroke-width="8" stroke-linecap="round"/>
  <path d="M128 40 L170 198" stroke="{INK}" stroke-width="8" stroke-linecap="round"/>
  <circle cx="128" cy="40" r="14" fill="{GOLD}" {S5}/>
  <g transform="translate(184 76)"><circle r="22" fill="#d9c27a" {S5}/><circle cx="-7" cy="-4" r="4" fill="{INK}"/><circle cx="7" cy="-4" r="4" fill="{INK}"/><path d="M-8 8 Q0 14 8 8" fill="none" stroke="{INK}" stroke-width="3"/></g>''',
    "trial_writ": f'''
  <path d="M60 50 L196 50 L196 200 L60 200 Z" fill="#e6d6b0" {S}/>
  <path d="M60 50 C40 50 40 80 60 80 M196 200 C216 200 216 170 196 170" fill="none" {S}/>
  <path d="M84 84 L172 84 M84 108 L172 108 M84 132 L150 132" stroke="#9c8f74" stroke-width="6" stroke-linecap="round"/>
  <circle cx="160" cy="170" r="26" fill="{BLOOD}" {S5}/>
  <path d="M148 160 L152 150 L160 158 L168 150 L172 160 Z" fill="{GOLD}"/>''',
    "fogbound_lantern": f'''
  <path d="M100 44 L156 44 L150 64 L106 64 Z" fill="{DARKIRON}" {S}/>
  <path d="M88 64 L168 64 L176 188 L80 188 Z" fill="#7d8a96" {S}/>
  <path d="M100 84 L156 84 L160 172 L96 172 Z" fill="#c9d3dc"/>
  <path d="M80 188 L176 188 L168 210 L88 210 Z" fill="{DARKIRON}" {S}/>
  <path d="M128 24 C112 24 112 44 128 44 C144 44 144 24 128 24" fill="none" {S5}/>
  <g fill="#dfe4ea" opacity="0.85"><ellipse cx="60" cy="130" rx="36" ry="16"/><ellipse cx="196" cy="110" rx="40" ry="16"/><ellipse cx="180" cy="200" rx="40" ry="14"/><ellipse cx="70" cy="210" rx="34" ry="12"/></g>''',
}

# Card portraits: 500x380 (rendered at 1000x760, the vanilla portrait size).
CARDS = {
    "unholy_pact": f'''
  <rect width="500" height="380" fill="#1a0c12"/>
  <circle cx="250" cy="190" r="150" fill="#3a0d18"/>
  <path d="M250 70 L285 160 L380 160 L303 215 L332 305 L250 250 L168 305 L197 215 L120 160 L215 160 Z" fill="none" stroke="{RED}" stroke-width="8"/>
  <path d="M130 330 C170 270 210 240 245 200" fill="none" stroke="{BONE}" stroke-width="22" stroke-linecap="round"/>
  <path d="M370 330 C330 270 290 240 255 200" fill="none" stroke="{PURPLE}" stroke-width="22" stroke-linecap="round"/>
  <circle cx="250" cy="196" r="16" fill="{GOLD}"/>''',
    "weight": f'''
  <rect width="500" height="380" fill="#2a2730"/>
  <path d="M160 150 L340 150 L380 320 L120 320 Z" fill="{DARKIRON}" stroke="{INK}" stroke-width="10" stroke-linejoin="round"/>
  <path d="M215 150 C215 90 285 90 285 150" fill="none" stroke="{INK}" stroke-width="26"/>
  <path d="M215 150 C215 90 285 90 285 150" fill="none" stroke="{IRON}" stroke-width="12"/>
  <path d="M215 230 L285 230 M250 230 L250 290" stroke="#c9c4be" stroke-width="16" stroke-linecap="round"/>''',
    "gnawing": f'''
  <rect width="500" height="380" fill="{INK}"/>
  <path d="M60 120 C140 60 360 60 440 120 L400 150 L370 120 L340 160 L310 120 L280 165 L250 120 L220 165 L190 120 L160 160 L130 120 L100 150 Z" fill="{BONE}" stroke="#3a2f3a" stroke-width="6"/>
  <path d="M60 270 C140 330 360 330 440 270 L400 240 L370 270 L340 230 L310 270 L280 225 L250 270 L220 225 L190 270 L160 230 L130 270 L100 240 Z" fill="{BONE}" stroke="#3a2f3a" stroke-width="6"/>
  <ellipse cx="250" cy="195" rx="120" ry="30" fill="{BLOOD}"/>''',
    "kudzu": f'''
  <rect width="500" height="380" fill="#1f2a1a"/>
  <g fill="none" stroke="{MOSS}" stroke-width="18" stroke-linecap="round">
    <path d="M0 300 C120 240 160 340 260 260 C340 200 400 280 500 220"/>
    <path d="M40 0 C80 120 20 200 120 260"/><path d="M460 20 C420 120 480 180 400 260"/>
  </g>
  <g fill="{GREEN}" {S5}>
    <path d="M150 270 C120 230 140 200 170 200 C180 230 170 250 150 270 Z"/><path d="M300 230 C330 190 370 200 370 230 C340 240 320 240 300 230 Z"/>
    <path d="M90 140 C60 110 80 80 110 90 C112 120 104 130 90 140 Z"/><path d="M420 150 C450 120 470 140 460 170 C440 170 430 165 420 150 Z"/>
  </g>''',
}

POWERS = {
    "sensitive_power": f'''
  <circle cx="128" cy="128" r="70" fill="#e8b4a0" {S}/>
  <path d="M128 40 L128 80 M128 176 L128 216 M40 128 L80 128 M176 128 L216 128 M66 66 L94 94 M162 162 L190 190 M190 66 L162 94 M94 162 L66 190" stroke="{RED}" stroke-width="16" stroke-linecap="round"/>
  <circle cx="128" cy="128" r="22" fill="{RED}" {S}/>''',
    "seized_power": f'''
  <circle cx="128" cy="128" r="76" fill="{IRON}" {S}/>
  <g fill="{RUST}"><ellipse cx="100" cy="100" rx="26" ry="18"/><ellipse cx="160" cy="160" rx="28" ry="18"/></g>
  <path d="M86 128 L170 128" stroke="{INK}" stroke-width="22" stroke-linecap="round"/>
  <path d="M86 128 L170 128" stroke="{DARKIRON}" stroke-width="12" stroke-linecap="round"/>''',
    "damp_power": f'''
  <path d="M128 30 C150 80 196 110 196 156 C196 196 164 222 128 222 C92 222 60 196 60 156 C60 110 106 80 128 30 Z" fill="{BLUE}" {S}/>
  <path d="M96 156 C96 176 108 190 124 194" fill="none" stroke="#a9cdf0" stroke-width="12" stroke-linecap="round"/>''',
    "rage_threshold_power": f'''
  <path d="M128 36 C78 36 56 72 60 112 C62 132 72 140 82 146 L82 180 L174 180 L174 146 C184 140 194 132 196 112 C200 72 178 36 128 36 Z" fill="{BLUE}" {S}/>
  <g fill="{INK}"><ellipse cx="102" cy="110" rx="18" ry="20"/><ellipse cx="154" cy="110" rx="18" ry="20"/></g>
  <path d="M40 210 L216 210" stroke="{RED}" stroke-width="16" stroke-linecap="round"/>
  <path d="M128 196 L128 222" stroke="{INK}" stroke-width="10"/>''',
    # Trials: one glyph per Slay the Spire 1 boss. Also used as the boss badge on the map.
    "split_power": f'''
  <path d="M40 200 C30 140 80 100 104 104 C128 108 120 150 112 200 Z" fill="{SLIME}" {S}/>
  <path d="M144 200 C136 150 128 108 152 104 C176 100 226 140 216 200 Z" fill="{SLIME}" {S}/>
  <path d="M128 60 L128 220" stroke="{INK}" stroke-width="8" stroke-dasharray="14 10"/>
  <g fill="{INK}"><circle cx="76" cy="150" r="7"/><circle cx="182" cy="150" r="7"/></g>''',
    "inferno_power": f'''
  <path d="M128 30 C150 70 196 90 190 150 C186 196 150 220 128 220 C106 220 70 196 66 150 C60 90 106 70 128 30 Z" fill="#7cd36a" {S}/>
  <path d="M128 96 C140 120 160 132 156 162 C152 186 138 194 128 194 C118 194 104 186 100 162 C96 132 116 120 128 96 Z" fill="#c9f2b0"/>
  <g fill="{INK}"><ellipse cx="110" cy="150" rx="8" ry="12"/><ellipse cx="146" cy="150" rx="8" ry="12"/></g>''',
    "mode_shift_power": f'''
  <path d="M128 30 L208 60 L208 130 C208 180 170 210 128 228 C86 210 48 180 48 130 L48 60 Z" fill="#7a8ca8" {S}/>
  <circle cx="128" cy="128" r="40" fill="{DARKIRON}" {S}/>
  <g fill="{DARKIRON}" {S5}><rect x="118" y="70" width="20" height="20"/><rect x="118" y="166" width="20" height="20"/><rect x="70" y="118" width="20" height="20"/><rect x="166" y="118" width="20" height="20"/></g>
  <circle cx="128" cy="128" r="14" fill="#9fd3ef"/>''',
    "second_wind_power": f'''
  <path d="M60 110 L80 50 L104 86 L128 40 L152 86 L176 50 L196 110 Z" fill="{GOLD}" {S}/>
  <path d="M128 120 L128 226" stroke="{INK}" stroke-width="22" stroke-linecap="round"/>
  <path d="M128 120 L128 226" stroke="#c9c4be" stroke-width="10" stroke-linecap="round"/>
  <path d="M90 140 L166 140" stroke="{INK}" stroke-width="18" stroke-linecap="round"/>
  <path d="M90 140 L166 140" stroke="{GOLD}" stroke-width="8" stroke-linecap="round"/>''',
    "stasis_power": f'''
  <circle cx="128" cy="128" r="84" fill="#b5803e" {S}/>
  <circle cx="128" cy="128" r="54" fill="#8c5a24" {S5}/>
  <circle cx="128" cy="128" r="22" fill="#6fd0f0" {S5}/>
  <path d="M128 44 L128 74 M128 182 L128 212 M44 128 L74 128 M182 128 L212 128" stroke="{INK}" stroke-width="8"/>''',
    "mega_debuff_power": f'''
  <path d="M110 120 L146 120 L138 226 L118 226 Z" fill="{DARKWOOD}" {S}/>
  <path d="M128 24 C150 54 180 70 176 104 C172 128 150 134 128 134 C106 134 84 128 80 104 C76 70 106 54 128 24 Z" fill="#a86ad8" {S}/>
  <path d="M128 66 C138 82 150 92 148 108 C146 120 136 122 128 122 C120 122 110 120 108 108 C106 92 118 82 128 66 Z" fill="#e0c7f4"/>''',
    "square_and_circle_power": f'''
  <rect x="34" y="70" width="104" height="104" fill="#5f8fd0" transform="rotate(-8 86 122)" {S}/>
  <circle cx="170" cy="142" r="58" fill="#e05a4a" {S}/>
  <g fill="{GOLD}"><circle cx="86" cy="122" r="12"/><circle cx="170" cy="142" r="12"/></g>''',
    "time_warp_power": f'''
  <circle cx="128" cy="128" r="88" fill="#3d5a8a" {S}/>
  <path d="M128 128 C128 100 160 100 160 128 C160 170 96 170 96 128 C96 76 180 76 180 128 C180 196 76 196 76 128" fill="none" stroke="#c9e3f6" stroke-width="10" stroke-linecap="round"/>
  <path d="M128 50 L128 70 M206 128 L186 128" stroke="{INK}" stroke-width="8"/>''',
    "rebirth_power": f'''
  <path d="M128 70 C96 40 40 50 24 100 C60 92 80 110 90 140 C100 170 118 196 128 220 C138 196 156 170 166 140 C176 110 196 92 232 100 C216 50 160 40 128 70 Z" fill="#3a2a4a" {S}/>
  <path d="M128 90 L140 130 L128 196 L116 130 Z" fill="#e8d66a" {S5}/>
  <g fill="#c4162a"><circle cx="112" cy="104" r="6"/><circle cx="144" cy="104" r="6"/></g>''',
}

# Map icons (dark ink; the game tints them). Trial: a crowned sigil.
MAP = {
    "trial": f'''
  <path d="M30 56 L46 22 L64 44 L82 16 L100 44 L118 22 L134 56 Z" fill="#2a1f24" stroke="#2a1f24" stroke-width="4" stroke-linejoin="round"/>
  <circle cx="82" cy="98" r="34" fill="none" stroke="#2a1f24" stroke-width="12"/>
  <path d="M82 76 L82 120 M60 98 L104 98" stroke="#2a1f24" stroke-width="10" stroke-linecap="round"/>''',
}

# Event portraits: 860x403 (rendered at 3440x1613, the vanilla portrait size).
EVENTS = {
    "trial_event": f'''
  <rect width="860" height="403" fill="#1b1622"/>
  <path d="M0 403 L0 300 C200 250 320 330 430 280 C560 220 680 300 860 260 L860 403 Z" fill="#2a2333"/>
  <g opacity="0.85">
    <circle cx="160" cy="170" r="60" fill="{SLIME}"/>
    <path d="M300 230 L320 120 L360 230 Z" fill="#7cd36a"/>
    <path d="M410 90 L450 60 L490 90 L490 170 L410 170 Z" fill="#7a8ca8"/>
    <circle cx="590" cy="150" r="56" fill="#b5803e"/>
    <rect x="680" y="110" width="90" height="90" fill="#5f8fd0"/>
  </g>
  <path d="M370 40 L390 0 L410 30 L430 0 L450 30 L470 0 L490 40 Z" fill="{GOLD}"/>''',
}


def write(kind: str, name: str, body: str, view_box: str = "0 0 256 256") -> bool:
    path = SRC / kind / f"{name}.svg"
    if path.exists():
        return False
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(
        f'<svg xmlns="http://www.w3.org/2000/svg" viewBox="{view_box}">\n'
        f'  <!-- Placeholder ({name}); generated by tools/art/make_placeholders.py, safe to hand-edit. -->{body}\n</svg>\n',
        encoding="utf-8")
    return True


if __name__ == "__main__":
    made = ([n for n, b in RELICS.items() if write("relics", n, b)]
            + [n for n, b in POWERS.items() if write("powers", n, b)]
            + [n for n, b in CARDS.items() if write("cards", n, b, "0 0 500 380")]
            + [n for n, b in MAP.items() if write("map", n, b, "0 0 164 140")]
            + [n for n, b in EVENTS.items() if write("events", n, b, "0 0 860 403")])
    print(f"wrote {len(made)} new SVGs: {', '.join(made) or '(none)'}")
