# Black Core: Ascension — Names Sprint

## What changed

- Added `BlackCoreNameService` as a presentation-only identity layer.
- Server/player identity is preserved; no packet, GID, quest or rAthena name is mutated.
- Player/character nameplates remain visible and use Black Core type colors.
- NPC and monster nameplates remain visible alongside players.
- Character creation now starts with a Brazilian/MMO-style suggested name such as `CaioBrasa`, `LunaNox` or `MayaLume`.
- Added a public `SuggestCharacterName()` hook so a future Dice/Random button can request another name without changing networking code.
- Generic service NPCs receive deterministic Brazilian display names, e.g. `Livia — Atendente`, `Caio — Ferreiro`.
- Common starter monsters and classic bosses receive original Portuguese/Black-Core-facing display aliases, e.g. `Geleia Rosa`, `Coelho Lunar`, `Lobo do Sertao`, `Senhor do Abismo`.
- Pets/companions with generic server names receive friendly Brazilian nicknames (`Pingo`, `Tico`, `Nina`, `Juca`, etc.).
- Added EditMode tests to protect player-name preservation, deterministic generation and common monster aliases.

## Nameplate visual language

- Local player: Black Core cyan + bold.
- Other players: cool white.
- NPCs: warm gold.
- Monsters: danger coral/red.
- Pets/companions: green.
- Dark outline is applied for readability over bright maps.

## Important behavior

Aliases are display-only. `Entity.Status.name` remains the original server value. This is deliberate: rAthena scripts and future server-side content can continue using their canonical names safely while the client evolves into the Black Core IP.

## Next recommended step

Move the alias/name pools into a remotely updateable JSON/Addressables table once the rAthena content fork is available, then mirror the final names into the server databases/scripts so quest text, monster DB, UI and client labels all use the same Black Core canon.
