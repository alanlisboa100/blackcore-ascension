# Black Core: Ascension — rAthena Server Sprint 1

This repository is customized through rAthena's import/override system wherever possible.
Internal Aegis names, map resource names and numeric IDs remain untouched for protocol/script compatibility.

## Applied
- Character-server branding: `BlackCore-Ascension` / `BlackCore`.
- Branded MOTD.
- `db/import/item_db.yml`: 89 item rows covering all 45 Black Core aliases.
- `db/import/mob_db.yml`: 221 monster rows covering all 63 Black Core aliases.
- `db/import/skill_db.yml`: 55 skill rows covering all 52 Black Core aliases.
- `npc/custom/blackcore/world_core.txt`: Lívia guide/warper, Nina healer, Caio starter quest, login welcome.
- `tools/blackcore_identity_sync.py`: repeatable generator for DB identity overrides after upstream updates.

## World names (display identity)
- prontera → Nova Aurora
- izlude → Porto Prisma
- geffen → Torre Nox
- payon → Vale Cedro
- morocc → Dunas de Ônix
- alberta → Porto Brasa
- aldebaran → Cronovale
- yuno → Altavila
- lighthalzen → Neo Lumen
- einbroch → Forja Rubra
- niflheim → Véu Sombrio
- brasilis → Bravamar

## Currency
The numeric `Zeny` variable stays intact internally. The Black Core client presents it as **Núcleos**. New Black Core scripts should say Núcleos in dialogue but continue using `Zeny` in script expressions.

## Deployment security
Do **not** expose a server with rAthena's sample `s1/p1` inter-server credentials or default SQL credentials. Put private credentials/IPs in deployment-only config and do not commit them.

## First original quest
- **Primeiro Pulso** (`Quest ID 90001`)
- Targets: 10 PORING + 5 LUNATIC in `prt_fild08` (displayed as Campos de Nova Aurora).
- Rewards: 1,500 Núcleos (internal Zeny), 10 Poções Rubras (Red_Potion), Base/Job EXP.
