#!/bin/bash

# Prevent packing of resource fork files on macOS (./._xxxxx)
export COPYFILE_DISABLE=true

set -e

pushd "${0%/*}" # Go to script directory
BUILD_DIR="$(pwd)"
TEMP_DIR="$BUILD_DIR/Temp"
OUTPUT_DIR="$BUILD_DIR/Output"

rm -rf "$TEMP_DIR"
mkdir -p -m 777 "$TEMP_DIR"

rm -rf "$OUTPUT_DIR"
mkdir -p "$OUTPUT_DIR"

################################################################################
# Application
################################################################################
PROJECT_NAMES="
    Own.BlockchainExplorer.Api
    Own.BlockchainExplorer.Scanner
"

for PROJECT_NAME in $PROJECT_NAMES; do
    echo "--- Building project: $PROJECT_NAME"

    PROJECT_SOURCE_DIR="../Source/$PROJECT_NAME"
    PROJECT_TEMP_DIR="$TEMP_DIR/$PROJECT_NAME"

    pushd "$PROJECT_SOURCE_DIR"
    mkdir -p "$PROJECT_TEMP_DIR"
    dotnet publish -c Release -o "$PROJECT_TEMP_DIR"
    popd

    pushd "$PROJECT_TEMP_DIR"
    mv appsettings.json appsettings.json.local
    tar czf "$OUTPUT_DIR/$PROJECT_NAME.tar.gz" --exclude={appsettings.json,*.pdb} *
    popd
done

################################################################################
# Database
################################################################################
pushd "../Database"
tar czf "$OUTPUT_DIR/Database.tar.gz" *
popd

################################################################################
# Finalize
################################################################################
popd # Go back to caller directory

# Show build output
echo "--- Build output in $OUTPUT_DIR"
ls -lh "$OUTPUT_DIR"
