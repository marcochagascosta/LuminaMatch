#!/usr/bin/env python3
"""Promote the latest completed alpha/internal release of Lumina Match to production.

Requires Application Default Credentials (or GOOGLE_APPLICATION_CREDENTIALS)
with scope https://www.googleapis.com/auth/androidpublisher
and access to Play Console developer 6604076546202815303.

Usage:
  python3 tools/play_promote_production.py --dry-run
  python3 tools/play_promote_production.py --yes
"""

from __future__ import annotations

import argparse
import json
import os
import sys
import urllib.error
import urllib.request

PACKAGE = "com.marcosaas.luminamatch"
SCOPE = "https://www.googleapis.com/auth/androidpublisher"
BASE = f"https://androidpublisher.googleapis.com/androidpublisher/v3/applications/{PACKAGE}"


def get_token() -> str:
    # Prefer google-auth if installed; else metadata / gcloud.
    try:
        from google.auth import default
        from google.auth.transport.requests import Request

        creds, _ = default(scopes=[SCOPE])
        creds.refresh(Request())
        return creds.token
    except Exception:
        pass

    # gcloud user ADC
    try:
        import subprocess

        out = subprocess.check_output(
            [
                "gcloud",
                "auth",
                "application-default",
                "print-access-token",
            ],
            text=True,
        ).strip()
        if out:
            return out
    except Exception as ex:
        raise SystemExit(
            "No Play API credentials. Set GOOGLE_APPLICATION_CREDENTIALS or run "
            f"`gcloud auth application-default login --scopes={SCOPE}`.\n({ex})"
        ) from ex
    raise SystemExit("Empty access token")


def api(method: str, url: str, token: str, body: dict | None = None) -> dict:
    data = None if body is None else json.dumps(body).encode("utf-8")
    req = urllib.request.Request(
        url,
        data=data,
        method=method,
        headers={
            "Authorization": f"Bearer {token}",
            "Content-Type": "application/json",
        },
    )
    try:
        with urllib.request.urlopen(req, timeout=120) as resp:
            raw = resp.read().decode("utf-8")
            return json.loads(raw) if raw else {}
    except urllib.error.HTTPError as e:
        err = e.read().decode("utf-8", errors="replace")
        raise SystemExit(f"HTTP {e.code} {url}\n{err}") from e


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--dry-run", action="store_true", help="List tracks only")
    parser.add_argument("--yes", action="store_true", help="Commit the production edit")
    parser.add_argument(
        "--source-track",
        default="alpha",
        choices=("alpha", "internal"),
        help="Track to copy release from (default: alpha)",
    )
    parser.add_argument(
        "--user-fraction",
        type=float,
        default=1.0,
        help="Staged rollout fraction (1.0 = full)",
    )
    args = parser.parse_args()

    token = get_token()
    edit = api("POST", f"{BASE}/edits", token, {})
    edit_id = edit["id"]
    print(f"edit={edit_id}")

    for track in ("internal", "alpha", "beta", "production"):
        try:
            info = api("GET", f"{BASE}/edits/{edit_id}/tracks/{track}", token)
            releases = info.get("releases") or []
            summary = []
            for r in releases:
                summary.append(
                    {
                        "name": r.get("name"),
                        "status": r.get("status"),
                        "versionCodes": r.get("versionCodes"),
                    }
                )
            print(f"track {track}: {json.dumps(summary, ensure_ascii=False)}")
        except SystemExit as e:
            print(f"track {track}: (unavailable) {e}")

    if args.dry_run:
        api("DELETE", f"{BASE}/edits/{edit_id}", token)
        print("dry-run: edit discarded")
        return

    if not args.yes:
        api("DELETE", f"{BASE}/edits/{edit_id}", token)
        print("Refusing to promote without --yes (edit discarded).")
        sys.exit(2)

    src = api("GET", f"{BASE}/edits/{edit_id}/tracks/{args.source_track}", token)
    releases = src.get("releases") or []
    if not releases:
        raise SystemExit(f"No releases on source track {args.source_track}")

    # Prefer completed release with highest version code.
    def vmax(rel: dict) -> int:
        codes = [int(c) for c in (rel.get("versionCodes") or [])]
        return max(codes) if codes else -1

    best = max(releases, key=vmax)
    codes = best.get("versionCodes") or []
    if not codes:
        raise SystemExit("Source release has no versionCodes")

    status = "completed" if args.user_fraction >= 1.0 else "inProgress"
    body = {
        "track": "production",
        "releases": [
            {
                "name": best.get("name") or f"Promote {codes}",
                "status": status,
                "versionCodes": codes,
                **(
                    {"userFraction": args.user_fraction}
                    if status == "inProgress"
                    else {}
                ),
                "releaseNotes": best.get("releaseNotes")
                or [
                    {
                        "language": "pt-BR",
                        "text": "Lumina Match 0.1.41 — match-3, boosters, loja e anúncios.",
                    }
                ],
            }
        ],
    }
    api("PUT", f"{BASE}/edits/{edit_id}/tracks/production", token, body)
    validated = api("POST", f"{BASE}/edits/{edit_id}:validate", token, {})
    print("validate:", json.dumps(validated, ensure_ascii=False)[:500])
    committed = api("POST", f"{BASE}/edits/{edit_id}:commit", token, {})
    print("commit:", json.dumps(committed, ensure_ascii=False)[:500])
    print("OK — production track updated. Review Play Console for rollout.")


if __name__ == "__main__":
    # Allow JSON service account via env without writing a long-lived file.
    raw = os.environ.get("GOOGLE_APPLICATION_CREDENTIALS_JSON")
    if raw and not os.environ.get("GOOGLE_APPLICATION_CREDENTIALS"):
        path = "/tmp/lumina-play-sa.json"
        with open(path, "w", encoding="utf-8") as f:
            f.write(raw)
        os.environ["GOOGLE_APPLICATION_CREDENTIALS"] = path
    main()
