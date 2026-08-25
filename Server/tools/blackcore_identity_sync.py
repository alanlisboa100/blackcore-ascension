#!/usr/bin/env python3
"""Generate Black Core: Ascension rAthena import overrides.

Keeps all Aegis/internal names and IDs untouched; only player-facing Name/Description
fields are overridden. This makes upstream rAthena updates much easier to merge.
"""
from pathlib import Path
import yaml

ROOT = Path(__file__).resolve().parents[1]
DB = ROOT / "db"
OUT = DB / "import"
OUT.mkdir(parents=True, exist_ok=True)

ITEM_ALIASES = {
    "Red Potion":"Poção Rubra", "Orange Potion":"Poção Solar", "Yellow Potion":"Poção Dourada",
    "White Potion":"Poção de Luz", "Blue Potion":"Essência Azul", "Green Potion":"Antídoto Verde",
    "Red Herb":"Erva Rubra", "Yellow Herb":"Erva Dourada", "White Herb":"Erva de Luz",
    "Blue Herb":"Erva Azul", "Green Herb":"Erva Verde", "Apple":"Maçã do Vale",
    "Banana":"Banana Dourada", "Carrot":"Cenoura Selvagem", "Meat":"Carne Temperada",
    "Jellopy":"Fragmento Gelatinoso", "Fluff":"Tufo Macio", "Clover":"Trevo do Vale",
    "Feather":"Pluma Leve", "Iron":"Ferro Bruto", "Steel":"Aço Temperado",
    "Phracon":"Minério Bruto", "Emveretarcon":"Liga Arcana", "Fly Wing":"Asa de Salto",
    "Butterfly Wing":"Asa de Retorno", "Knife":"Faca de Campo", "Main Gauche":"Punhal do Vale",
    "Sword":"Espada de Ferro", "Falchion":"Lâmina Curva", "Blade":"Lâmina de Guerra",
    "Katana":"Katana Rubra", "Bow":"Arco de Caça", "Crossbow":"Besta de Caça",
    "Rod":"Cajado Arcano", "Staff":"Cajado de Guerra", "Mace":"Maça de Ferro",
    "Club":"Clava Reforçada", "Axe":"Machado de Ferro", "Spear":"Lança de Guarda",
    "Cotton Shirt":"Camisa de Viajante", "Jacket":"Jaqueta de Couro", "Guard":"Escudo de Guarda",
    "Buckler":"Broquel de Ferro", "Sandals":"Sandálias de Viagem", "Shoes":"Botas de Viagem",
}

MONSTER_ALIASES = {
    "Poring":"Geleia Rosa", "Drops":"Gota Azul", "Poporing":"Lodo Verde", "Marin":"Gota do Mar",
    "Magmaring":"Gota de Magma", "Lunatic":"Coelho Lunar", "Fabre":"Lagartinha Verde",
    "Chonchon":"Mosca do Campo", "Steel Chonchon":"Mosca de Aço", "Rocker":"Grilo do Vale",
    "Picky":"Pintinho Bravo", "Willow":"Salgueiro Vivo", "Elder Willow":"Salgueiro Ancião",
    "Spore":"Cogumelo Bravo", "Poison Spore":"Cogumelo Tóxico", "Mandragora":"Mandragora",
    "Wolf":"Lobo Cinzento", "Desert Wolf":"Lobo do Sertão", "Smokie":"Guaxinim da Mata",
    "Hornet":"Vespa Dourada", "Andre":"Formiga Operária", "Deniro":"Formiga Rubra",
    "Piere":"Formiga Verde", "Thief Bug":"Barata das Ruínas", "Thief Bug Male":"Barata Cascuda",
    "Thief Bug Female":"Barata Rainha", "Tarou":"Rato do Esgoto", "Familiar":"Morcego da Caverna",
    "Skeleton":"Esqueleto Errante", "Soldier Skeleton":"Soldado Ossudo", "Archer Skeleton":"Arqueiro Ossudo",
    "Zombie":"Morto-Vivo", "Ghoul":"Devorador", "Orc Warrior":"Guerreiro Orc",
    "Orc Lady":"Matriarca Orc", "Orc Hero":"Campeão Orc", "Goblin":"Goblin do Vale",
    "Kobold":"Kobold Selvagem", "Condor":"Condor", "Peco Peco":"Ave do Sertão", "Muka":"Cacto Bravo",
    "Golem":"Golem de Pedra", "Metaller":"Besouro Metálico", "Argiope":"Aranha Venenosa",
    "Argos":"Aranha do Deserto", "Side Winder":"Serpente Listrada", "Anacondaq":"Anaconda",
    "Boa":"Jiboia", "Yoyo":"Macaquinho", "Savage Babe":"Javali Jovem", "Savage":"Javali Selvagem",
    "Baphomet":"Senhor do Abismo", "Baphomet Jr.":"Herdeiro do Abismo", "Mistress":"Rainha Vespa",
    "Moonlight Flower":"Raposa Lunar", "Osiris":"Faraó Sombrio", "Doppelganger":"O Reflexo",
    "Drake":"Capitão Espectral", "Eddga":"Tigre Ancestral", "Phreeoni":"Olho da Areia",
    "Maya":"Rainha Formiga", "Stormy Knight":"Cavaleiro da Geada", "Dark Lord":"Lorde das Sombras",
}

SKILL_ALIASES = {
    "Bash":"Golpe Brutal", "Magnum Break":"Explosão Rubra", "Provoke":"Desafio", "Endure":"Pele de Ferro",
    "Increase HP Recovery":"Fôlego de Aço", "Sword Mastery":"Domínio da Lâmina",
    "Two-Handed Sword Mastery":"Domínio da Montante", "Bowling Bash":"Corte Tempestuoso",
    "Brandish Spear":"Investida Imperial", "Aura Blade":"Lâmina de Aura", "Fire Bolt":"Faísca Rubra",
    "Cold Bolt":"Estilhaço Gélido", "Lightning Bolt":"Raio Celeste", "Fire Ball":"Orbe de Brasas",
    "Fire Wall":"Muralha Ígnea", "Frost Diver":"Lança de Gelo", "Thunderstorm":"Tempestade Elétrica",
    "Soul Strike":"Impacto Astral", "Safety Wall":"Barreira Arcana", "Stone Curse":"Olhar de Pedra",
    "Napalm Beat":"Pulso Astral", "Double Strafe":"Disparo Duplo", "Arrow Shower":"Chuva de Flechas",
    "Owl's Eye":"Olhar do Falcão", "Vulture's Eye":"Mira Longa", "Improve Concentration":"Foco do Caçador",
    "Blitz Beat":"Ataque do Falcão", "Falcon Assault":"Investida Aérea", "Focused Arrow Strike":"Tiro Perfurante",
    "Heal":"Cura Radiante", "Blessing":"Bênção", "Increase AGI":"Passo Celeste", "Angelus":"Guarda Sagrada",
    "Pneuma":"Véu de Vento", "Holy Light":"Luz Sagrada", "Resurrection":"Retorno da Alma",
    "Sanctuary":"Santuário", "Double Attack":"Golpe Duplo", "Envenom":"Lâmina Venenosa",
    "Hiding":"Passo Sombrio", "Steal":"Mão Leve", "Back Slide":"Recuo Sombrio", "Sonic Blow":"Rajada Sônica",
    "Cloaking":"Véu Sombrio", "Grimtooth":"Presa Sombria", "Mammonite":"Golpe Dourado", "Discount":"Barganha",
    "Overcharge":"Venda Esperta", "Cart Revolution":"Impacto de Carga", "Vending":"Loja de Rua",
    "Teleport":"Salto de Núcleo", "Warp Portal":"Portal do Núcleo",
}


def bodies(paths):
    seen = set()
    for p in paths:
        if not p.exists(): continue
        try:
            data = yaml.safe_load(p.read_text(encoding="utf-8-sig")) or {}
        except Exception:
            continue
        for row in data.get("Body", []) or []:
            if not isinstance(row, dict) or "Id" not in row: continue
            key = (p.name, row["Id"])
            if key in seen: continue
            seen.add(key)
            yield row


def build_by_field(paths, field):
    result = {}
    for row in bodies(paths):
        val = row.get(field)
        if isinstance(val, str):
            result.setdefault(val.casefold(), []).append(row)
    return result

def dump_db(path, dbtype, version, rows, comment):
    header = f"# {comment}\n# Generated by tools/blackcore_identity_sync.py. Keep IDs/Aegis names unchanged.\n\n"
    payload = {"Header":{"Type":dbtype,"Version":version}, "Body":rows}
    text = yaml.safe_dump(payload, sort_keys=False, allow_unicode=True, width=120)
    path.write_text(header + text, encoding="utf-8")

item_paths = [DB/"re"/"item_db.yml", DB/"re"/"item_db_usable.yml", DB/"re"/"item_db_equip.yml", DB/"re"/"item_db_etc.yml", DB/"item_db.yml"]
mob_paths = [DB/"re"/"mob_db.yml", DB/"mob_db.yml"]
skill_paths = [DB/"re"/"skill_db.yml", DB/"skill_db.yml"]
items = build_by_field(item_paths, "Name")
mobs = build_by_field(mob_paths, "Name")
skills = build_by_field(skill_paths, "Description")

item_rows=[]; missing_items=[]
for old,new in ITEM_ALIASES.items():
    rows=items.get(old.casefold(), [])
    if rows:
        item_rows.extend({"Id":row["Id"], "Name":new} for row in rows)
    else: missing_items.append(old)

mob_rows=[]; missing_mobs=[]
for old,new in MONSTER_ALIASES.items():
    rows=mobs.get(old.casefold(), [])
    if rows:
        mob_rows.extend({"Id":row["Id"], "Name":new, "JapaneseName":new} for row in rows)
    else: missing_mobs.append(old)

skill_rows=[]; missing_skills=[]
for old,new in SKILL_ALIASES.items():
    rows=skills.get(old.casefold(), [])
    if rows:
        skill_rows.extend({"Id":row["Id"], "Description":new} for row in rows)
    else: missing_skills.append(old)

# De-duplicate by numeric ID if root/mode databases expose the same row more than once.
def unique_rows(rows):
    out={}
    for row in rows:
        out[row["Id"]]=row
    return list(out.values())
item_rows=unique_rows(item_rows)
mob_rows=unique_rows(mob_rows)
skill_rows=unique_rows(skill_rows)

dump_db(OUT/"item_db.yml", "ITEM_DB", 3, sorted(item_rows,key=lambda r:r["Id"]), "Black Core item display-name overrides")
dump_db(OUT/"mob_db.yml", "MOB_DB", 5, sorted(mob_rows,key=lambda r:r["Id"]), "Black Core monster display-name overrides")
dump_db(OUT/"skill_db.yml", "SKILL_DB", 5, sorted(skill_rows,key=lambda r:r["Id"]), "Black Core skill display-name overrides")

report = ROOT/"BLACKCORE_IDENTITY_REPORT.md"
report.write_text(
    "# Black Core Identity Sync Report\n\n"
    f"- Item rows renamed: **{len(item_rows)}** (aliases covered: {len(ITEM_ALIASES)-len(missing_items)}/{len(ITEM_ALIASES)})\n"
    f"- Monster rows renamed: **{len(mob_rows)}** (aliases covered: {len(MONSTER_ALIASES)-len(missing_mobs)}/{len(MONSTER_ALIASES)})\n"
    f"- Skill rows renamed: **{len(skill_rows)}** (aliases covered: {len(SKILL_ALIASES)-len(missing_skills)}/{len(SKILL_ALIASES)})\n\n"
    "## Missing aliases\n\n"
    f"- Items: {', '.join(missing_items) if missing_items else 'none'}\n"
    f"- Monsters: {', '.join(missing_mobs) if missing_mobs else 'none'}\n"
    f"- Skills: {', '.join(missing_skills) if missing_skills else 'none'}\n",
    encoding="utf-8"
)
print(report.read_text(encoding="utf-8"))
