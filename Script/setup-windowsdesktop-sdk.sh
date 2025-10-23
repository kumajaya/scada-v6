#!/usr/bin/env bash
# setup-windowsdesktop-sdk.sh
# Purpose : Inject Microsoft.NET.Sdk.WindowsDesktop into Linux SDK (unofficial)
# Author  : Ketut Kumajaya

set -euo pipefail

SDK_VERSION=${1:-$(dotnet --list-sdks | grep '^8\.' | tail -1 | cut -d' ' -f1)}
DOTNET_ROOT=$(dirname "$(dotnet --list-sdks | grep '^8\.' | tail -1 | awk '{gsub(/[\[\]]/,"",$2); print $2}' | xargs)")
LINUX_SDK_PATH="$DOTNET_ROOT/sdk/$SDK_VERSION/Sdks"
EXTRACT_DIR="$HOME/.dotnet-windowsdesktop/$SDK_VERSION"
SRC_SDK="$EXTRACT_DIR/sdk/$SDK_VERSION/Sdks/Microsoft.NET.Sdk.WindowsDesktop"
KEY_FILE="$SRC_SDK/targets/Microsoft.NET.Sdk.WindowsDesktop.targets"

ZIP_URL="https://dotnetcli.azureedge.net/dotnet/Sdk/$SDK_VERSION/dotnet-sdk-$SDK_VERSION-win-x64.zip"
TEMP_DIR=$(mktemp -d)
trap 'rm -rf "$TEMP_DIR"' EXIT
TEMP_ZIP="$TEMP_DIR/dotnet-sdk-win-$SDK_VERSION.zip"

echo ">>> Detected SDK version: $SDK_VERSION"
echo ">>> DOTNET_ROOT: $DOTNET_ROOT"

[ -d "$LINUX_SDK_PATH" ] || { echo "Error: Linux SDK path not found: $LINUX_SDK_PATH"; exit 1; }
command -v unzip >/dev/null || { echo "Error: unzip not found. Install it first."; exit 1; }

if [ -f "$KEY_FILE" ]; then
  echo ">>> Skipping download, WindowsDesktop SDK already detected."
else
  echo ">>> Downloading Windows .NET SDK ZIP..."
  curl -sSL -o "$TEMP_ZIP" "$ZIP_URL" || { echo "Error: curl failed."; exit 1; }
  echo ">>> Extracting to $EXTRACT_DIR..."
  mkdir -p "$EXTRACT_DIR"
  unzip -q "$TEMP_ZIP" -d "$EXTRACT_DIR"
fi

[ -f "$KEY_FILE" ] || { echo "Error: WindowsDesktop SDK not found after extraction."; exit 1; }

if [ -d "$LINUX_SDK_PATH/Microsoft.NET.Sdk.WindowsDesktop" ]; then
  echo ">>> WindowsDesktop SDK already present at $LINUX_SDK_PATH, skipping copy."
else
  echo ">>> Copying WindowsDesktop SDK into Linux SDK path (sudo required)..."
  sudo mkdir -p "$LINUX_SDK_PATH"
  sudo cp -r --backup=numbered "$SRC_SDK" "$LINUX_SDK_PATH/"
  echo ">>> Injection complete."
fi

echo ">>> Success: WindowsDesktop SDK available at $LINUX_SDK_PATH/Microsoft.NET.Sdk.WindowsDesktop"
