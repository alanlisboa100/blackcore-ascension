# Black Core: Ascension — APK sem PC

## Resumo

Você não precisa abrir o Unity no seu aparelho. Porém, como o cliente é um projeto Unity, o build final do APK precisa ser executado pelo Unity Editor/build pipeline em algum lugar.

A rota recomendada para este projeto é:

**Celular / Black Core -> GitHub -> Unity Build Automation -> APK -> celular**

## Opção A — Unity Build Automation (recomendada)

1. Coloque `Client/unityro_names_sprint` em um repositório Git.
2. No Unity Dashboard, abra DevOps > Build Automation.
3. Conecte o repositório GitHub.
4. Crie um target Android.
5. Project subfolder: `UnityClient`.
6. Ative auto-detect da versão pelo `ProjectSettings/ProjectVersion.txt`.
7. Use Unity `2021.3.8f1` no primeiro smoke build; não migre a versão antes de confirmar que abre.
8. Builder: Windows ou macOS disponível para Android.
9. Android SDK: use uma versão oferecida pelo target compatível com Unity 2021.3.
10. Para teste interno, use debug signing. Para publicação, crie e preserve um keystore próprio.
11. Dispare a build no Dashboard e baixe o APK no celular.

### Bloqueador atual

O projeto ainda não contém a biblioteca visual gerada em:

`UnityClient/Assets/_Generated/AddressablesAssets`

Além disso, o Remote.LoadPath foi propositalmente trocado para o placeholder seguro:

`https://assets.blackcore.example/[BuildTarget]`

Isso evita que um APK Black Core tente baixar conteúdo do CDN legado do UnityRO. Antes do APK jogável precisamos escolher uma destas estratégias:

- importar/gerar um **Black Core Asset Pack** local e empacotar os bundles;
- hospedar os bundles Black Core em um CDN nosso e trocar o placeholder;
- para um smoke test técnico, usar apenas assets que tenhamos direito de usar e gerar o catálogo/bundles antes da build.

## Opção B — GitHub Actions + GameCI

Foi adicionado:

`.github/workflows/android-apk.yml`

Ele roda preflight, testes opcionais, Unity 2021.3.8f1 em cloud runner e gera um artifact Android APK.

GameCI exige ativação/licença Unity. Essa opção é boa quando já houver os secrets de licença configurados no GitHub.

## Comando de prontidão local/CI

```bash
python3 tools/blackcore_apk_readiness.py --client UnityClient
```

Em CI use `--ci` para bloquear builds que ainda dependam de CDN placeholder ou não tenham os assets gerados.

## O que o celular faz

Depois de configurado uma vez, o fluxo diário pode ser só:

1. abrir GitHub/Unity Dashboard;
2. disparar build;
3. baixar artifact `.apk`;
4. instalar no Android;
5. testar e mandar os logs/erros para a próxima sprint.
