# Pointing the mobile client to your Black Core server

For an alpha build, edit:

`UnityClient/Assets/AddressableAssets/LocalConfigs.json.txt`

Set `fallbackLoginServer` to the public IPv4 of the VPS running the Black Core rAthena stack. Keep port `6900` unless you intentionally changed it.

Example:

```json
{
  "remoteConfigLocation": "",
  "fallbackLoginServer": "203.0.113.20",
  "fallbackLoginPort": "6900",
  "fallbackUseSameIpForEveryServer": true
}
```

For production, host `RemoteConfigs/RemoteConfigs.json` behind HTTPS and put that URL in `remoteConfigLocation`. This lets you move the game server without rebuilding the APK.

The client no longer depends on the old UnityRO DigitalOcean Spaces configuration endpoint.
