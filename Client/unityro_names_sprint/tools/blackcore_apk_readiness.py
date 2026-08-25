#!/usr/bin/env python3
import argparse
import pathlib
import re
import sys


def main():
    p = argparse.ArgumentParser()
    p.add_argument('--client', required=True)
    p.add_argument('--ci', action='store_true', help='Exit non-zero for blockers')
    args = p.parse_args()

    root = pathlib.Path(args.client).resolve()
    blockers = []
    warnings = []

    version_file = root / 'ProjectSettings' / 'ProjectVersion.txt'
    if not version_file.exists():
        blockers.append('ProjectSettings/ProjectVersion.txt ausente')
    else:
        version = version_file.read_text(errors='ignore')
        if '2021.3.8f1' not in version:
            warnings.append('Versão Unity diferente de 2021.3.8f1; revisar configuração remota')

    build_settings = root / 'ProjectSettings' / 'EditorBuildSettings.asset'
    if not build_settings.exists():
        blockers.append('EditorBuildSettings.asset ausente')
    else:
        txt = build_settings.read_text(errors='ignore')
        enabled = len(re.findall(r'- enabled: 1', txt))
        if enabled < 5:
            blockers.append(f'Apenas {enabled} cenas habilitadas no build')

    settings = root / 'Assets' / 'AddressableAssetsData' / 'AddressableAssetSettings.asset'
    if not settings.exists():
        blockers.append('AddressableAssetSettings.asset ausente')
    else:
        txt = settings.read_text(errors='ignore')
        schema_dir = root / 'Assets' / 'AddressableAssetsData' / 'AssetGroups' / 'Schemas'
        remote_load_id = '3c157dbb8b129934c84ded1c33dc7f4d'
        remote_groups = 0
        if schema_dir.exists():
            for schema in schema_dir.glob('*_BundledAssetGroupSchema.asset'):
                if remote_load_id in schema.read_text(errors='ignore'):
                    remote_groups += 1
        if 'unityro.fra1.digitaloceanspaces.com' in txt and remote_groups:
            blockers.append('Addressables remotos ainda apontam para o CDN legado do UnityRO')
        if 'assets.blackcore.example' in txt and remote_groups:
            blockers.append('CDN Black Core ainda não foi configurado (placeholder .example)')

    generated = root / 'Assets' / '_Generated' / 'AddressablesAssets'
    if not generated.exists():
        blockers.append('Assets/_Generated/AddressablesAssets ausente: biblioteca visual ainda não foi gerada/importada')
    else:
        count = sum(1 for f in generated.rglob('*') if f.is_file() and not f.name.endswith('.meta'))
        if count < 100:
            warnings.append(f'Biblioteca visual contém somente {count} arquivos; confirmar se está completa')

    ps = root / 'ProjectSettings' / 'ProjectSettings.asset'
    if ps.exists():
        txt = ps.read_text(errors='ignore')
        if 'Android: com.blackcore.ascension' not in txt:
            blockers.append('Application Identifier Android não é com.blackcore.ascension')
        if 'AndroidTargetArchitectures: 3' not in txt:
            warnings.append('Arquiteturas Android não estão no preset ARMv7 + ARM64 esperado')

    print('=== BLACK CORE APK READINESS ===')
    for w in warnings:
        print('WARN:', w)
    for b in blockers:
        print('BLOCKER:', b)
    print(f'Warnings: {len(warnings)} | Blockers: {len(blockers)}')

    if args.ci and blockers:
        return 2
    return 0


if __name__ == '__main__':
    sys.exit(main())
