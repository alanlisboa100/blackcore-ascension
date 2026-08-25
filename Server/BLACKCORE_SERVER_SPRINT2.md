# Black Core: Ascension — Server Sprint 2

## Gameplay added

- Streamlined Black Core class ascension NPC (`Maya`) using native rAthena Job IDs.
  - Base 10: Iniciado -> Guerreiro / Arcanista / Arqueiro / Devoto / Mercador / Ladino.
  - Job 40: second specialization branches.
  - Base 99 + Job 50: streamlined third-class ascension for the alpha (no forced rebirth loop).
- Starter weapon + 20 Poções Rubras on the first path, once per character.
- `Kira`: storage and Núcleos bank access.
- `Rafa`: server-side party creation fallback while the custom mobile party UI is unfinished.
- `Bia`: starter supply shop.
- Public boss flow with `Ravi` and the **Senhor do Abismo**.
- Quest 90002 **Eco do Abismo** with one-time quest rewards.
- Fixed the previous quest reward flow so completed quests cannot be repeatedly claimed.

## Alpha balance

`conf/import/battle_conf.txt` now defines a moderate alpha profile:

- Base EXP: 3x
- Job EXP: 3x
- MVP EXP: 2x
- Quest EXP: 2.5x
- Common/healing/usable drops: 1.5x
- Equipment drops: 1.25x
- Card drops: 0.75x
- Party even-share bonus: +10% per additional participating member
- Guild creation does not consume Emperium during alpha
- Party share level range: 20

All of these are import overrides and can be tuned without editing rAthena's upstream battle files.

## Phone-friendly VPS deployment

`deploy/docker/` contains:

- `Dockerfile`
- `docker-compose.yml`
- `.env.example`
- `entrypoint.sh`
- deployment README

The stack builds rAthena, starts MariaDB, imports `main.sql` + `logs.sql`, replaces the insecure sample `s1/p1` inter-server account, writes private runtime SQL/network imports, and exposes login/char/map ports.

## Compatibility rules preserved

- Job IDs unchanged.
- Mob IDs unchanged.
- Item IDs unchanged.
- Skill IDs unchanged.
- Map resource names unchanged.
- rAthena packet protocol unchanged.

Black Core names and progression are layered on top of those stable IDs.

## Validation performed

- All `db/import/*.yml` parse successfully.
- Docker Compose YAML parses successfully.
- Every Black Core battle override key exists in the upstream rAthena battle configuration.
- Quest mob IDs/Aegis names and starter/shop item IDs were verified against the Renewal databases.
- All Black Core NPC script files have balanced braces and are included in `npc/scripts_custom.conf`.
- `login-server` and `char-server` compiled successfully in this environment.
- The full `map-server` compilation was started from a clean copy and produced no Black Core-related compiler error, but the large upstream C++ compile exceeded the execution window before linking. No C++ source was modified in Sprint 2; gameplay changes are scripts/config/data imports.

A full runtime script parse still requires MariaDB + map-server startup; the included Docker stack is intended to be the next end-to-end validation environment.
