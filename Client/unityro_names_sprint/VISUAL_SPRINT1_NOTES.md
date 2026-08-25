# Black Core: Ascension — Visual Sprint 1

Working title and runtime visual identity applied on top of UnityRO Mobile Sprint 2.

## Applied

- Working product identity: **Black Core: Ascension** / studio **Black Core**.
- Runtime URP cinematic volume with ACES tonemapping, subtle Bloom, color grading, vignette and desktop-only film grain.
- Mobile FXAA / desktop SMAA enabled through URP camera data.
- Map mood detection (city / forest / desert / frost / dark / neutral) controlling color, contrast, saturation and lightweight exponential fog.
- Water tint adjusted by map mood.
- Tri-light ambient illumination for stronger separation on ground, houses, props and foliage.
- Mobile-friendly directional shadow tuning.
- Legacy environment polish using MaterialPropertyBlock: foliage receives a subtle cool-green lift; houses/roof/wood a subtle warm lift; stone/castle a slight cool lift.
- Visible-only micro sway for grass/leaves/plants/bushes/flowers; existing animated model nodes are excluded.
- Procedural weapon trail on weapon sprite viewers during attack motions.
- Critical damage type, critical color treatment, combat flash and local-player camera impulse.
- Casting gets an additional rotating ground ring using the existing casting texture/material.
- STR effect materials are cached and released instead of being recreated repeatedly.
- New Black Core mobile HUD palette, button outlines, brand badge and joystick colors.
- Inventory / stats / equipment / skills / escape windows now open/close with a short fade + scale animation.
- Splash status and menu overlay carry the Black Core: Ascension working identity.

## Important

- The Unity assembly name remains `UnityRO` because changing it would break serialized scene/prefab script references. Branding is intentionally separated from technical assembly names.
- This sprint does not replace Ragnarok-owned visual assets. A commercial original game still needs its own characters, maps, monsters, UI art, sounds, music and other protected content.
- `Black Core: Ascension` is a working title, not trademark clearance.
- Visual values are intentionally conservative until they can be inspected in the Unity Editor and on a real Android device.

## Next visual pass after first Unity run

1. Fix any Unity 2021/URP compile or shader warnings found by the Editor.
2. Capture Android screenshots from city, field, dungeon and combat.
3. Tune bloom/fog/contrast per device.
4. Replace the runtime brand badge with authored logo art.
5. Build original UI sprite atlas and icon family.
6. Create original class silhouettes, weapons and spell VFX.
7. Replace environment textures/models biome-by-biome.
8. Add authored emissive materials for portals, crystals, torches and rare loot.

## Names follow-up
A follow-up Names Sprint adds persistent Black Core nameplates, Brazilian/MMO character-name suggestions, deterministic Brazilian NPC service names, companion nicknames, and presentation-only Portuguese/original aliases for common monsters/bosses. See `NAMES_SPRINT_NOTES.md`.
