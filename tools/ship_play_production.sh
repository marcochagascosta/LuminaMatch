#!/usr/bin/env bash
# Atalho local / CI para promover Play Production.
set -euo pipefail
cd "$(dirname "$0")/.."
if [[ -z "${GOOGLE_APPLICATION_CREDENTIALS_JSON:-}" && -z "${GOOGLE_APPLICATION_CREDENTIALS:-}" ]]; then
  echo "Defina GOOGLE_APPLICATION_CREDENTIALS_JSON (JSON da service account) ou GOOGLE_APPLICATION_CREDENTIALS."
  exit 1
fi
exec python3 tools/play_promote_production.py --yes "$@"
