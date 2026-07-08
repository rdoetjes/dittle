#!/bin/bash
# package.sh - Create installers for Dittle
# Run this from the dittle/ directory

APP_NAME="dittle"
VERSION="1.0.0"
PUBLISH_DIR="bin/Release/net10.0"
DIST_DIR="dist"

# Ensure we start fresh
rm -rf "$DIST_DIR"
mkdir -p "$DIST_DIR"

# 1. Linux (.tar.gz)
echo "Packaging Linux..."
dotnet publish dittle.csproj -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true
if [ -d "$PUBLISH_DIR/linux-x64/publish" ]; then
    # Create a temp dir to avoid path nesting in tar
    TEMP_LINUX="temp_linux"
    mkdir -p "$TEMP_LINUX"
    cp -r "$PUBLISH_DIR/linux-x64/publish/"* "$TEMP_LINUX/"
    cd "$TEMP_LINUX"
    tar -czvf "../$DIST_DIR/${APP_NAME}-linux-x64.tar.gz" .
    cd ..
    rm -rf "$TEMP_LINUX"
fi

# 2. MacOS (.app zip)
echo "Packaging MacOS (ARM64 and X64)..."
for arch in "osx-arm64" "osx-x64"; do
    dotnet publish dittle.csproj -c Release -r $arch --self-contained true -p:PublishSingleFile=true
    if [ -d "$PUBLISH_DIR/$arch/publish" ]; then
        # Create the .app bundle structure
        # We name it simply dittle.app for the zip, but distinguish the zip filename
        BUNDLE_NAME="${APP_NAME}.app"
        BUNDLE_PATH="$DIST_DIR/$BUNDLE_NAME"
        rm -rf "$BUNDLE_PATH"
        
        mkdir -p "$BUNDLE_PATH/Contents/MacOS"
        mkdir -p "$BUNDLE_PATH/Contents/Resources"
        
        # Copy the binary and ensure executable bit
        cp "$PUBLISH_DIR/$arch/publish/$APP_NAME" "$BUNDLE_PATH/Contents/MacOS/"
        chmod +x "$BUNDLE_PATH/Contents/MacOS/$APP_NAME"
        
        # Copy libraylib.dylib if it exists in the publish folder
        if [ -f "$PUBLISH_DIR/$arch/publish/libraylib.dylib" ]; then
            cp "$PUBLISH_DIR/$arch/publish/libraylib.dylib" "$BUNDLE_PATH/Contents/MacOS/"
        fi
        
        # Copy resources
        cp -r resources/* "$BUNDLE_PATH/Contents/Resources/"
        
        # Create Info.plist
        cat > "$BUNDLE_PATH/Contents/Info.plist" <<EOF
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleExecutable</key>
    <string>${APP_NAME}</string>
    <key>CFBundleIdentifier</key>
    <string>com.yourdomain.${APP_NAME}</string>
    <key>CFBundleName</key>
    <string>${APP_NAME}</string>
    <key>CFBundleVersion</key>
    <string>${VERSION}</string>
    <key>CFBundleShortVersionString</key>
    <string>${VERSION}</string>
    <key>CFBundlePackageType</key>
    <string>APPL</string>
    <key>LSMinimumSystemVersion</key>
    <string>10.15</string>
</dict>
</plist>
EOF
        # Zip from inside the dist folder to avoid path nesting
        cd "$DIST_DIR"
        zip -ry "${APP_NAME}-${arch}.zip" "$BUNDLE_NAME"
        cd ..
        rm -rf "$BUNDLE_PATH"
    fi
done

# 3. Windows (Zip)
echo "Packaging Windows..."
dotnet publish dittle.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
if [ -d "$PUBLISH_DIR/win-x64/publish" ]; then
    TEMP_WIN="temp_win"
    mkdir -p "$TEMP_WIN"
    cp -r "$PUBLISH_DIR/win-x64/publish/"* "$TEMP_WIN/"
    cd "$TEMP_WIN"
    zip -r "../$DIST_DIR/${APP_NAME}-win-x64.zip" .
    cd ..
    rm -rf "$TEMP_WIN"
fi

echo "Packaging complete! Check the '$DIST_DIR' directory."
