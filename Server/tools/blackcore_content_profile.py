#!/usr/bin/env python3
"""Switch rAthena NPC loading between full legacy content and Black Core alpha profile.

The tool changes only the primary scripts_main.conf files and keeps backups.
It does not touch IDs, DBs or C++ protocol code.
"""
from pathlib import Path
import argparse
import shutil
import sys

ROOT = Path(__file__).resolve().parents[1]
PROFILES = {
    ROOT / 'npc' / 're' / 'scripts_main.conf': ROOT / 'npc' / 'blackcore' / 'scripts_main.conf',
    ROOT / 'npc' / 'pre-re' / 'scripts_main.conf': ROOT / 'npc' / 'blackcore' / 'scripts_main_pre_re.conf',
}
TARGETS = list(PROFILES)


def enable():
    for target in TARGETS:
        profile = PROFILES[target]
        if not profile.exists():
            raise SystemExit(f'Profile not found: {profile}')
        data = profile.read_text(encoding='utf-8')
        backup = target.with_name('scripts_main.legacy.conf')
        if target.exists() and not backup.exists():
            shutil.copy2(target, backup)
        target.write_text(data, encoding='utf-8')
        print(f'BLACKCORE profile enabled: {target.relative_to(ROOT)}')


def disable():
    for target in TARGETS:
        backup = target.with_name('scripts_main.legacy.conf')
        if not backup.exists():
            print(f'WARN: no backup for {target.relative_to(ROOT)}')
            continue
        shutil.copy2(backup, target)
        print(f'Legacy profile restored: {target.relative_to(ROOT)}')


def status():
    marker = 'Black Core: Ascension - Alpha Content Profile'
    for target in TARGETS:
        active = target.exists() and marker in target.read_text(encoding='utf-8', errors='ignore')
        print(f'{target.relative_to(ROOT)}: {"BLACKCORE" if active else "LEGACY/FULL"}')


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument('mode', choices=['enable', 'disable', 'status'])
    args = ap.parse_args()
    {'enable': enable, 'disable': disable, 'status': status}[args.mode]()
    return 0


if __name__ == '__main__':
    sys.exit(main())
