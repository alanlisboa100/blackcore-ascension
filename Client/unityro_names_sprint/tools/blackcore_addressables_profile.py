#!/usr/bin/env python3
"""Switch Unity Addressables between Black Core local-APK and remote-CDN modes.

This script edits serialized Addressables YAML using the project's existing
profile variable IDs. It is intended for CI/pre-build use with Unity 2021.3 /
Addressables 1.20.5.
"""
from pathlib import Path
import argparse
import re
import sys

LOCAL_BUILD = '1f43ebfdc58322b4e8f11a51e850c94e'
LOCAL_LOAD = '8881bc1892fd6e44fb5e5fdab983fdc6'
REMOTE_BUILD = '82bcd6853212fd640922892571e1b5d3'
REMOTE_LOAD = '3c157dbb8b129934c84ded1c33dc7f4d'


def replace_setting(text, key, value):
    return re.sub(rf'(^\s*{re.escape(key)}:\s*)\d+', rf'\g<1>{value}', text, flags=re.M)


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument('mode', choices=['local-apk', 'remote-cdn', 'status'])
    ap.add_argument('--client', default='UnityClient')
    args = ap.parse_args()

    root = Path(args.client).resolve()
    settings = root / 'Assets' / 'AddressableAssetsData' / 'AddressableAssetSettings.asset'
    schema_dir = root / 'Assets' / 'AddressableAssetsData' / 'AssetGroups' / 'Schemas'
    if not settings.exists() or not schema_dir.exists():
        raise SystemExit('Addressables settings not found')

    schemas = list(schema_dir.glob('*_BundledAssetGroupSchema.asset'))
    remote_count = sum(REMOTE_LOAD in p.read_text(errors='ignore') for p in schemas)
    local_count = sum(LOCAL_LOAD in p.read_text(errors='ignore') for p in schemas)

    if args.mode == 'status':
        print(f'Bundled schemas: {len(schemas)} | local={local_count} | remote={remote_count}')
        return 0

    txt = settings.read_text(encoding='utf-8')
    if args.mode == 'local-apk':
        # Addressables 1.20 enum: Preferences=0, BuildWithPlayer=1, DoNotBuild=2.
        txt = replace_setting(txt, 'm_BuildAddressablesWithPlayerBuild', 1)
        txt = replace_setting(txt, 'm_BuildRemoteCatalog', 0)
        settings.write_text(txt, encoding='utf-8')
        for p in schemas:
            s = p.read_text(encoding='utf-8')
            s = s.replace(REMOTE_BUILD, LOCAL_BUILD).replace(REMOTE_LOAD, LOCAL_LOAD)
            p.write_text(s, encoding='utf-8')
        print(f'Black Core Addressables -> LOCAL APK ({len(schemas)} groups)')
    else:
        txt = replace_setting(txt, 'm_BuildAddressablesWithPlayerBuild', 1)
        txt = replace_setting(txt, 'm_BuildRemoteCatalog', 1)
        settings.write_text(txt, encoding='utf-8')
        for p in schemas:
            s = p.read_text(encoding='utf-8')
            s = s.replace(LOCAL_BUILD, REMOTE_BUILD).replace(LOCAL_LOAD, REMOTE_LOAD)
            p.write_text(s, encoding='utf-8')
        print(f'Black Core Addressables -> REMOTE CDN ({len(schemas)} groups)')
    return 0


if __name__ == '__main__':
    sys.exit(main())
