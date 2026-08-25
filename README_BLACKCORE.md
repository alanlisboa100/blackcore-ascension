# Black Core: Ascension — Full Stack Sprint 2

This bundle contains the customized Unity client and rAthena server.

## Server

Sprint 2 adds streamlined class progression, party bootstrap, storage/bank/services, starter economy tuning, the first public boss encounter, a second original quest, and a Docker/VPS deployment stack designed to be operated from a phone/Termux/Black Core terminal.

See:

- `Server/BLACKCORE_SERVER_NOTES.md`
- `Server/BLACKCORE_SERVER_SPRINT2.md`
- `Server/deploy/docker/README.md`

## Client

The client keeps the previous mobile/network/performance/visual/identity work and no longer depends on UnityRO's old remote configuration host.

For an alpha APK, set the VPS IPv4 in:

`Client/unityro_names_sprint/UnityClient/Assets/AddressableAssets/LocalConfigs.json.txt`

For production, point `remoteConfigLocation` to your own HTTPS JSON endpoint so the server address can change without rebuilding the APK.

See:

`Client/unityro_names_sprint/ServerMigration/BLACKCORE_SERVER_ENDPOINT.md`

## Network ports

- Login: TCP 6900
- Character: TCP 6121
- Map: TCP 5121

MariaDB stays private inside Docker and should not be exposed publicly.
