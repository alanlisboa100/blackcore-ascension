# Black Core: Ascension — Validation

## Sprint 1 baseline

- `./configure --enable-packetver=20211103`: PASS.
- `make -j2 map`: PASS on the Sprint 1 baseline.
- Map server reached SQL initialization; no local MariaDB was running in that environment.

## Sprint 2 validation

Sprint 2 changes only scripts, data imports, configuration, deployment files, and the full-stack client endpoint fallback. No rAthena C++ source file was changed.

- All `db/import/*.yml`: YAML parse PASS.
- `deploy/docker/docker-compose.yml`: YAML parse PASS.
- `deploy/docker/entrypoint.sh`: `bash -n` PASS.
- Every key in `conf/import/battle_conf.txt` exists in upstream `conf/battle/*.conf`.
- Quest mobs PORING/LUNATIC/BAPHOMET verified in Renewal mob DB.
- Starter/shop item IDs verified in Renewal item DB.
- Black Core NPC scripts are all included by `npc/scripts_custom.conf`.
- Static brace validation for every `npc/custom/blackcore/*.txt`: PASS.
- Quest reward logic audited so quest rewards cannot be claimed repeatedly after `completequest`.
- A fresh Sprint 2 build successfully linked `login-server` and `char-server`. The unchanged upstream `map-server` C++ compilation exceeded the execution window before linking; it produced no Black Core-related compiler error before termination.

## End-to-end check still required

A real run should use the included Docker stack (or another MariaDB installation), then confirm:

1. login/char/map all connect to MariaDB;
2. all Black Core NPC scripts parse at map-server startup;
3. the custom Unity client logs in using the configured VPS endpoint;
4. class changes, party creation, storage/bank, quests and boss kill flow are exercised with test characters.
