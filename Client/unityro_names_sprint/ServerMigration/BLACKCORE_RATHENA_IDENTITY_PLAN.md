# Black Core: Ascension — rAthena identity migration

This client currently keeps rAthena IDs and resource names unchanged and rebrands only player-facing text.
When the rAthena server repository is added, mirror these names server-side instead of changing numeric IDs.

## Currency
- Internal: Zeny / status value remains unchanged during compatibility phase.
- Display: Núcleos.
- Later server pass: update NPC/shop dialogue strings and website/admin terminology, not the numeric money field.

## Core regions
| Internal map prefix | Black Core display name |
| --- | --- |
| prontera | Nova Aurora |
| izlude | Porto Prisma |
| geffen | Torre Nox |
| payon | Vale Cedro |
| morocc | Dunas de Ônix |
| alberta | Porto Brasa |
| aldebaran | Cronovale |
| yuno/juno | Altavila |
| lighthalzen | Neo Lumen |
| einbroch | Forja Rubra |
| comodo | Costa Lunar |
| umbala | Raiz Antiga |
| niflheim | Véu Sombrio |
| brasilis | Bravamar |

## Starter class identity
| rAthena job | Black Core class |
| --- | --- |
| Novice | Iniciado |
| Swordman | Guerreiro |
| Mage | Arcanista |
| Archer | Arqueiro |
| Acolyte | Devoto |
| Merchant | Mercador |
| Thief | Ladino |

## Server migration rules
1. Keep map filenames and job/skill/item numeric IDs stable until custom assets/protocol are complete.
2. Rename NPC display names and dialogue scripts in `npc/`.
3. Rename quest titles/descriptions where the server owns the text.
4. Rename item display descriptions in the client/server data pipeline while keeping item IDs.
5. Rename skill descriptions/tooltips while keeping skill IDs.
6. Replace Ragnarok-specific lore, trademarks, art, sounds and maps with original Black Core assets before commercial release.
7. Only after compatibility tests, consider custom internal identifiers for brand-new content.

## Source of truth
Client presentation mappings currently live in:
`UnityClient/Assets/Scripts/Identity/BlackCoreLoreService.cs`

When rAthena is imported, generate server-side rename patches from that catalog to prevent drift.
