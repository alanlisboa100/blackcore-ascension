#!/usr/bin/env bash
set -euo pipefail

if [[ $# -ne 1 ]]; then
  echo "Uso: GH_TOKEN=... ./tools/push_to_github.sh OWNER/REPO"
  exit 2
fi

if [[ -z "${GH_TOKEN:-}" ]]; then
  echo "GH_TOKEN não definido. O token nunca deve ser salvo neste repositório."
  exit 2
fi

repo="$1"
remote="https://github.com/${repo}.git"

askpass="$(mktemp)"
trap 'rm -f "$askpass"' EXIT
cat > "$askpass" <<'ASK'
#!/usr/bin/env sh
case "$1" in
  *Username*) printf '%s\n' "x-access-token" ;;
  *Password*) printf '%s\n' "$GH_TOKEN" ;;
  *) printf '\n' ;;
esac
ASK
chmod 700 "$askpass"

if git remote get-url origin >/dev/null 2>&1; then
  git remote set-url origin "$remote"
else
  git remote add origin "$remote"
fi

GIT_ASKPASS="$askpass" GIT_TERMINAL_PROMPT=0 git push -u origin HEAD:main
