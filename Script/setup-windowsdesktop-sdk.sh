#!/usr/bin/env bash
set -euo pipefail

LIST=$(dotnet --list-sdks 2>/dev/null || true)
[ -n "$LIST" ] || { echo "Error: .NET SDK not found."; exit 1; }

# Default: the SDK that dotnet resolves here (honours global.json)
SDK=${1:-$(dotnet --version 2>/dev/null || true)}
[ -n "$SDK" ] || { echo "Error: Unable to determine the .NET SDK version."; exit 1; }

ROOT=$(dirname "$(awk -v v="$SDK" '$1==v{gsub(/[\[\]]/,"",$2);print $2;exit}' <<<"$LIST")")
DST="$ROOT/sdk/$SDK/Sdks"
TMP="$HOME/.dotnet-windowsdesktop/$SDK"
ZIP="$TMP/win.zip"
URL="https://builds.dotnet.microsoft.com/dotnet/Sdk/$SDK/dotnet-sdk-$SDK-win-x64.zip"

echo "SDK $SDK"
[[ -d "$DST/Microsoft.NET.Sdk.WindowsDesktop" ]] && { echo "Already injected"; exit 0; }

command -v curl >/dev/null || { echo "Error: curl not installed."; exit 1; }
command -v unzip >/dev/null || { echo "Error: unzip not installed."; exit 1; }
mkdir -p "$TMP"

if [[ -s "$ZIP" ]]; then
  echo "Using existing $ZIP"
else
  echo ">>> Downloading..."
  curl -fL --retry 3 -o "$ZIP" "$URL"
fi

echo ">>> Extracting..."
unzip -qo "$ZIP" -d "$TMP"

SRC=$(find "$TMP" -type d -name Microsoft.NET.Sdk.WindowsDesktop -print -quit)
[ -n "$SRC" ] || { echo "Error: WindowsDesktop SDK not found in archive"; exit 1; }

echo ">>> Copying to $DST (sudo)..."
sudo mkdir -p "$DST"
sudo cp -r "$SRC" "$DST/"

echo "Done: $DST/Microsoft.NET.Sdk.WindowsDesktop"
