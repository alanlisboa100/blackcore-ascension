# Black Core: Ascension — Remote APK + Pre-Unity Performance Sprint

## O que foi aplicado

### APK sem PC
- Workflow manual `.github/workflows/android-apk.yml` com GameCI v4 para gerar APK Android em runner remoto.
- Guia `BLACKCORE_REMOTE_APK.md` para Unity Build Automation (rota recomendada para quem só tem celular).
- `tools/blackcore_apk_readiness.py` detecta bloqueadores antes de gastar minutos de build.
- `tools/blackcore_addressables_profile.py` cria modo **local-apk**: bundles Addressables são configurados para entrar no APK em vez de depender de CDN.
- `tools/uba_prebuild_android.sh` pronto para usar como Pre-Build Script no Unity Build Automation.

### Remoção de dependência UnityRO
- Remote.LoadPath antigo foi trocado de `unityro.fra1.digitaloceanspaces.com` para o placeholder seguro `assets.blackcore.example`.
- O modo `local-apk` não usa esse CDN; ele troca os 10 grupos bundled para Local.BuildPath/Local.LoadPath.
- O readiness bloqueia builds remotos enquanto um CDN Black Core real não for configurado.

### Bloqueador visual atual
- Ainda falta `UnityClient/Assets/_Generated/AddressablesAssets`.
- Sem essa biblioteca, o APK pode até compilar partes do Player, mas não teremos mapa/sprites/modelos/efeitos suficientes para um cliente jogável.
- O pipeline agora falha cedo e explica isso em vez de entregar um APK quebrado.

### Performance segura antes do Unity
- Adicionado `AddressableAssetCache<T>.LoadSync()` para reaproveitar handles já carregados.
- `SpriteEntityViewer` usa o cache para SpriteData, atlas, paletas e sombra.
- `RawImageExtensions` usa cache para texturas de interface/login.
- `EffectLoader` reaproveita texturas de VFX.
- `WaitForCompletion()` no `SpriteEntityViewer`: 0.
- Total no cliente caiu para 15; os restantes ficam para uma refatoração async após o primeiro compile Unity.

### Black Core Content Profile
- Criados profiles Renewal/Pre-Renewal que removem a superfície grande de NPCs/quests/jobs legados.
- Mantêm Global Functions, CashShop Functions, mapflags, monster spawns, warps e `scripts_custom.conf`.
- Ferramenta reversível: `Server/tools/blackcore_content_profile.py enable|disable|status`.
- Imports dos dois profiles validados: PASS.
- O profile fica opt-in no pacote para não remover conteúdo de navegação antes do primeiro teste end-to-end.

## Readiness atual

Depois de ativar o profile `local-apk`, existe **1 bloqueador conhecido**:

`Assets/_Generated/AddressablesAssets` ausente.

Esse é o próximo gargalo concreto para gerar um APK jogável.
