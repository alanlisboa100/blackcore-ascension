# UnityRO Mobile MMO - Sprint 2 Notes

## What changed

- Role-aware network connection state for Login, Character and Map servers.
- Automatic Character/Map reconnect with bounded backoff and 5-second handshake validation.
- Mobile app background/foreground recovery.
- Packet hooks now support multiple subscribers per packet header.
- Map reconnect resync through `ZC_ACCEPT_ENTER2` and `CZ_NOTIFY_ACTORINIT`.
- Runtime reconnect status overlay with manual retry.
- Runtime mobile HUD with virtual joystick, attack, four skill buttons, inventory and skills shortcuts.
- Safe-area support for notches and system gesture regions.
- Camera-relative joystick movement using short path requests.
- Selected-target reuse for mobile basic attacks.
- Desktop ALT shortcuts fixed to use `Input.GetKeyDown` rather than `Event.current` in `Update`.
- Reusable Addressables cache for small frequently reused assets.
- BGM, entity audio, map audio, cursor and casting VFX moved away from synchronous Addressables waits.
- Splash download-size check is asynchronous.
- Remote config fetch now has retries, timeout and last-known-good cache fallback.
- Extra `NetworkClient` regression tests for multicast packet hooks.
- CI network test filter expanded to all `Tests.Network` tests.
- Editor menu to preview mobile HUD on desktop.
- Android target architectures changed from ARMv7-only to ARMv7 + ARM64.

## How to preview the mobile HUD in Editor

Use: `UnityRO > Mobile HUD > Force In Editor`, then reload `MapScene`.

## Validation performed here

- Static brace/parenthesis balance checks on modified C# files.
- Workflow YAML parsed successfully.
- Package manifest JSON parsed successfully.
- Diff reviewed against Sprint 1.

Unity Editor is not installed in this environment, so this sprint still needs an actual Unity import/compile and Play Mode smoke test before merging.

## Highest-priority next work

1. Convert `SpriteEntityViewer` body/head/equipment/palette Addressables to generation-safe async loading.
2. Remove remaining synchronous UI/mesh Addressables waits.
3. Expand pooling to entities, drops and transient VFX.
4. Add session-expired reconnect handling that returns cleanly to login.
5. Validate Android build in Unity, then add CI APK output.
6. Upgrade Unity in a dedicated branch and migrate gameplay input to the Input System.
