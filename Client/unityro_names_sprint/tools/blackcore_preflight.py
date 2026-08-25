#!/usr/bin/env python3
"""Black Core client preflight checks that do not require the Unity Editor."""
from pathlib import Path
import argparse, collections, json, re, sys

LEGACY_VISIBLE = ("Ragnarok", "UnityRO", "rAthena", "Prontera", "Geffen", "Payon", "Morroc", "Zeny", "Kafra", "Emperium")

def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--client", default="UnityClient")
    ap.add_argument("--require-generated-assets", action="store_true")
    args = ap.parse_args()
    client = Path(args.client).resolve()
    assets = client / "Assets"
    errors, warnings = [], []

    if not assets.is_dir():
        print(f"ERROR client Assets not found: {assets}")
        return 2

    # JSON integrity.
    for p in client.rglob("*.json"):
        try:
            json.loads(p.read_text(encoding="utf-8-sig"))
        except Exception as exc:
            errors.append(f"Invalid JSON: {p.relative_to(client)}: {exc}")

    # C# meta integrity and duplicate GUIDs.
    for p in assets.rglob("*.cs"):
        if not Path(str(p) + ".meta").exists():
            errors.append(f"Missing .meta: {p.relative_to(client)}")

    guid_files = collections.defaultdict(list)
    known_guids = set()
    for p in assets.rglob("*.meta"):
        text = p.read_text(errors="ignore")
        m = re.search(r"^guid:\s*([0-9a-f]{32})", text, re.M)
        if m:
            known_guids.add(m.group(1))
            guid_files[m.group(1)].append(p)
    for guid, files in guid_files.items():
        if len(files) > 1:
            errors.append(f"Duplicate GUID {guid}: " + ", ".join(str(x.relative_to(client)) for x in files))

    # User-facing serialized legacy strings.
    for p in list(assets.rglob("*.unity")) + list(assets.rglob("*.prefab")):
        text = p.read_text(errors="ignore")
        for term in LEGACY_VISIBLE:
            if re.search(rf"(^|\s)(m_text|m_Text):.*{re.escape(term)}", text, re.I | re.M):
                errors.append(f"Visible legacy term '{term}': {p.relative_to(client)}")

    # Persistent UI method names should at least exist in source.
    method_locations = collections.defaultdict(list)
    for p in list(assets.rglob("*.unity")) + list(assets.rglob("*.prefab")):
        text = p.read_text(errors="ignore")
        for name in re.findall(r"m_MethodName:\s*([^\r\n]*)", text):
            name = name.strip()
            if name:
                method_locations[name].append(p)
    all_cs = "\n".join(p.read_text(errors="ignore") for p in assets.rglob("*.cs"))
    for name, files in method_locations.items():
        if name in {"SetActive", "Play", "Stop", "Pause"}:
            continue
        if not re.search(rf"\b{re.escape(name)}\s*\(", all_cs):
            errors.append(f"Serialized UI method not found: {name} ({files[0].relative_to(client)})")

    # Generated art/addressable payload. Groups in this repo are mostly catalog references;
    # a playable build needs the generated payload restored/generated first.
    generated = assets / "_Generated" / "AddressablesAssets"
    generated_files = sum(1 for p in generated.rglob("*") if p.is_file()) if generated.exists() else 0
    if generated_files == 0:
        msg = "Generated Addressables art payload is absent (Assets/_Generated/AddressablesAssets)."
        (errors if args.require_generated_assets else warnings).append(msg)

    print(f"Black Core preflight: {client}")
    print(f"Errors: {len(errors)} | Warnings: {len(warnings)}")
    for item in errors:
        print("ERROR", item)
    for item in warnings:
        print("WARN ", item)
    return 1 if errors else 0

if __name__ == "__main__":
    raise SystemExit(main())
