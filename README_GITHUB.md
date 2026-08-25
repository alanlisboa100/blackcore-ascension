# Black Core: Ascension — repository layout

This repository is a monorepo:

- `Client/unityro_names_sprint/UnityClient` — Unity 2021.3 client
- `Server` — rAthena-based Black Core server
- `.github/workflows/android-apk.yml` — remote Android APK workflow

## Android build

The workflow expects Unity licensing secrets when using GameCI:

- `UNITY_LICENSE`
- `UNITY_EMAIL`
- `UNITY_PASSWORD`

The preflight intentionally blocks a build if the generated Addressables content is missing.

For Unity Build Automation, set the Unity project path to:

`Client/unityro_names_sprint/UnityClient`

and target Android.
