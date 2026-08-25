#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
python3 tools/blackcore_addressables_profile.py local-apk --client UnityClient
python3 tools/blackcore_preflight.py --client UnityClient
python3 tools/blackcore_apk_readiness.py --client UnityClient --ci
