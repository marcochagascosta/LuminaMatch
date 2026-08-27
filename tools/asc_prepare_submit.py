#!/usr/bin/env python3
"""App Store Connect helper — JWT auth + read-only status (submit is manual by default).

Env:
  ASC_ISSUER_ID
  ASC_KEY_ID
  ASC_PRIVATE_KEY_P8   (PEM contents, including -----BEGIN PRIVATE KEY-----)

Usage:
  python3 tools/asc_prepare_submit.py --dry-run
"""

from __future__ import annotations

import argparse
import json
import os
import sys
import time
import urllib.error
import urllib.request

APP_ID = "6791448071"
BUNDLE = "com.marcosaas.luminamatch"
BASE = "https://api.appstoreconnect.apple.com/v1"


def make_token(issuer: str, key_id: str, pem: str) -> str:
    try:
        import jwt  # PyJWT
    except ImportError as ex:
        raise SystemExit("Install PyJWT: pip install PyJWT cryptography") from ex

    now = int(time.time())
    payload = {
        "iss": issuer,
        "iat": now,
        "exp": now + 20 * 60,
        "aud": "appstoreconnect-v1",
    }
    headers = {"alg": "ES256", "kid": key_id, "typ": "JWT"}
    return jwt.encode(payload, pem, algorithm="ES256", headers=headers)


def api_get(path: str, token: str) -> dict:
    req = urllib.request.Request(
        f"{BASE}{path}",
        headers={"Authorization": f"Bearer {token}", "Accept": "application/json"},
    )
    try:
        with urllib.request.urlopen(req, timeout=60) as resp:
            return json.loads(resp.read().decode("utf-8"))
    except urllib.error.HTTPError as e:
        raise SystemExit(f"HTTP {e.code}: {e.read().decode('utf-8', errors='replace')}") from e


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--dry-run", action="store_true", default=True)
    parser.add_argument("--list-builds", action="store_true")
    args = parser.parse_args()

    issuer = os.environ.get("ASC_ISSUER_ID", "").strip()
    key_id = os.environ.get("ASC_KEY_ID", "").strip()
    pem = os.environ.get("ASC_PRIVATE_KEY_P8", "").strip()
    if not (issuer and key_id and pem):
        print(
            "Missing ASC_ISSUER_ID / ASC_KEY_ID / ASC_PRIVATE_KEY_P8.\n"
            "See docs/IOS_SUBMIT_RUNBOOK.md — upload binary on a stable Mac, then submit in ASC UI."
        )
        sys.exit(1)

    if "BEGIN" not in pem and "\\n" in pem:
        pem = pem.replace("\\n", "\n")

    token = make_token(issuer, key_id, pem)
    app = api_get(f"/apps/{APP_ID}", token)
    attrs = app.get("data", {}).get("attributes", {})
    print("app:", attrs.get("name"), attrs.get("bundleId") or BUNDLE)

    versions = api_get(
        f"/apps/{APP_ID}/appStoreVersions?limit=5&fields[appStoreVersions]=versionString,appStoreState",
        token,
    )
    for v in versions.get("data", []):
        a = v.get("attributes", {})
        print(f"version {a.get('versionString')}: {a.get('appStoreState')} id={v.get('id')}")

    if args.list_builds:
        builds = api_get(
            f"/builds?filter[app]={APP_ID}&limit=10&sort=-uploadedDate"
            "&fields[builds]=version,processingState,uploadedDate",
            token,
        )
        for b in builds.get("data", []):
            a = b.get("attributes", {})
            print(
                f"build {a.get('version')} {a.get('processingState')} "
                f"{a.get('uploadedDate')} id={b.get('id')}"
            )

    print(
        "\nDry-run only: attach build 50 in ASC UI and Submit for Review.\n"
        "Automated submit is intentionally not performed by this script."
    )


if __name__ == "__main__":
    main()
