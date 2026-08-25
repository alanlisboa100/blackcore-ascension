# Identity Sprint — Black Core: Ascension

## Applied
- Added `BlackCoreLoreService` as the presentation identity layer.
- Rebranded major Ragnarok map names while preserving map resource names and map protocol values.
- Rebranded base/advanced class display names through `JobHelper.GetJobName`.
- Rebranded a curated starter set of common combat skills while preserving skill IDs/tags.
- Rebranded common starter consumables, materials, weapons and equipment while preserving item IDs and resource names.
- Rebranded visible `Zeny` terminology as `Núcleos`; money values remain the same server field.
- NPC dialogue now translates known legacy place/currency terms before display.
- Named legacy NPCs now receive stable Brazilian in-world identities without changing server names.
- Unknown legacy monsters receive deterministic Black Core creature aliases so old names do not leak into nameplates.
- Minimap now shows the Black Core region name above the map.
- Character selection shows Black Core region names.
- NPC dialog buttons use `Continuar` / `Fechar`.
- Added EditMode tests for maps, classes, skills, items and dialogue rebranding.
- Added `ServerMigration/BLACKCORE_RATHENA_IDENTITY_PLAN.md` for the later server-side content pass.

## Deliberately not changed
- Numeric job/skill/item IDs.
- Packet layouts.
- Map filenames / Addressable addresses.
- Player-selected character names.
- Server-side NPC script identifiers.

Those remain stable for rAthena compatibility.
