# UnityRO Mobile MMO Modernization

This branch starts the modernization required to turn UnityRO into a maintainable Android/iOS/PC MMORPG client while preserving rAthena protocol compatibility.

## Completed in tranche 1

- Removed per-entity pointer raycasts and centralized pointer targeting in `EntityControl`.
- Reduced gameplay pointer selection to a single physics raycast per frame.
- Added mouse/touch pointer abstraction (`PointerInput`).
- Fixed the Insert key path that relied on `Event.current` from `Update`.
- Added two-finger mobile camera rotate + pinch zoom while reserving one-finger taps for gameplay.
- Replaced legacy socket `BeginReceive/EndReceive` loop with cancellable `ReadAsync`.
- Added graceful remote-close handling and TCP `NoDelay`.
- Hardened packet parsing for TCP fragmentation.
- Fixed packet registration scan so one unannotated type does not abort all later registrations.
- Added safe connection/stream guards to outgoing packets and heartbeat.
- Made heartbeat lifecycle idempotent and added a main-thread disconnect event.
- Added a connection timeout for dead/unreachable endpoints.
- Added scene-safe packet unhooking for all 60 existing packet hooks.
- Removed a duplicate receive-loop start during map-server login.
- Replaced LINQ-heavy outgoing packet assembly with a reusable direct byte buffer.
- Added EditMode regression tests for fragmented, coalesced, fixed and variable packets.
- Added CI execution for EditMode tests before client builds.
- Added pooling for transient combat damage numbers.
- Centralized grid targeting into the same gameplay pointer raycast.
- Made dropped-item Addressables loading asynchronous and lifetime-owned.
- Cached the shared shadow sprite and damage prefab hot paths.
- Fixed `EntityManager.ClearEntities()` so it destroys entity GameObjects rather than only components.
- Implemented server-driven NPC script close/reset instead of crashing on `ZC_CLOSE_SCRIPT`.
- Implemented common live appearance updates (job, hair, weapon/shield, headgear, colors, robe, costume reset) without the previous weapon-change crash.
- Corrected `ZC_SPRITE_CHANGE2 (0x01D7)` from 15 to the rAthena 11-byte wire layout and added a regression test that verifies the following packet is not consumed.

## Next tranches

1. Expand protocol regression tests across login/char/map packets and outgoing packet parity.
2. Add reconnect/session state machine for Wi-Fi/4G changes and app background/foreground.
3. Extend object pools to entities, dropped items, chat lines and transient VFX.
4. Replace remaining synchronous Addressables `WaitForCompletion()` hot paths with async preload/cache/release ownership.
5. Build a mobile HUD (virtual joystick, attack, hotbar, potion, target lock, safe-area scaling).
6. Move gameplay input to Unity Input System after the Unity upgrade.
7. Upgrade the project in a dedicated branch from Unity 2021 LTS to a supported Unity 6 LTS release.
8. Add Android CI build/output handling, then iOS.
9. Add a reconnect overlay and network-state UX using the new disconnect event.
10. Implement missing MMO-facing client systems: party, guild, trade, quests, world map, social and settings.

## Guardrails

- Keep rAthena packet compatibility covered by tests before protocol refactors.
- Keep mobile and desktop controls behind common gameplay actions.
- Do not ship Ragnarok-owned assets/content as original game IP.
- Review AGPL-3.0 obligations before deciding whether the distributed client can remain proprietary.

## Completed in tranche 2

- Added role-aware connection state (`Login`, `Character`, `Map`) and lifecycle states.
- Added automatic Character/Map reconnect with bounded backoff, handshake timeout and server re-auth packet replay.
- Added background/foreground recovery for mobile app lifecycle.
- Made packet hooks multicast so multiple systems can safely listen to the same packet header.
- Added map-scene handling for `ZC_ACCEPT_ENTER2` after reconnect and player reposition/resync.
- Added a runtime network status/retry overlay.
- Added the first functional runtime mobile HUD: virtual joystick, attack, four skill slots, inventory and skills buttons.
- Added safe-area handling for notches/rounded corners/system gesture areas.
- Added camera-relative short-step joystick movement while preserving desktop click-to-move.
- Added persistent selected target support for the mobile attack button.
- Fixed desktop map UI shortcuts that incorrectly relied on `Event.current` from `Update()`.
- Added app-lifetime Addressables cache for small frequently reused assets.
- Removed synchronous Addressables waits from BGM, entity audio, map audio, cursor initialization and casting VFX.
- Removed the splash-screen synchronous Addressables size check.
- Added remote-config retry, timeout and last-known-good cache fallback for unreliable mobile networks.
- Added network hook regression tests and expanded CI network test filtering.
- Added an Editor menu toggle to force the mobile HUD for desktop testing.

## Immediate tranche 3 priorities

1. Convert `SpriteEntityViewer` body/head/equipment/palette loading to generation-safe async loading; this is now the largest remaining spawn-time mobile stall.
2. Convert mesh entity loading and remaining UI Addressables waits.
3. Add entity/item/VFX pooling beyond combat damage numbers.
4. Add explicit reconnect failure UX that can return to login safely when the rAthena session is no longer valid.
5. Add Android development build automation after opening this branch once in Unity and validating package/import migrations.
6. Upgrade Unity in its own branch and move input to the newer Input System after the current controls are behavior-locked.
