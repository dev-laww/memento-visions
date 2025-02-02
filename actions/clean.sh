#!/bin/bash

# Determine the Godot executable from the PATH
if command -v godot >/dev/null 2>&1; then
    GODOT_CMD="godot"
elif command -v godot.exe >/dev/null 2>&1; then
    GODOT_CMD="godot.exe"
else
    echo "Godot executable not found in PATH. Please install Godot or add it to your PATH."
    exit 1
fi

# Function to close Godot based on OS
close_godot() {
    OS=$(uname -s)
    case "$OS" in
        Linux|Darwin)
            echo "Closing Godot editor on $OS..."
            pkill -f "$GODOT_CMD" 2>/dev/null
            ;;
        CYGWIN*|MINGW*|MSYS*)
            echo "Closing Godot editor on Windows..."
            taskkill //IM godot.exe //F 2>/dev/null
            ;;
        *)
            echo "Unknown OS: $OS. Please close the Godot editor manually."
            ;;
    esac
}

echo "Closing Godot editor..."
close_godot

echo "Cleaning up the project directory..."
rm -rf .godot .vs build data gdunit4_testadapter results build

echo "Cleaning solution..."
dotnet clean

echo "Building solution..."
dotnet build

echo "Reopening Godot editor..."
OS=$(uname -s)
case "$OS" in
    Linux|Darwin)
        # Run Godot in a subshell using nohup with full redirection:
        (
          nohup "$GODOT_CMD" . --editor </dev/null >/dev/null 2>&1 &
          disown
        )
        ;;
    CYGWIN*|MINGW*|MSYS*)
        # On Windows (Git Bash, MSYS), backgrounding is usually sufficient.
        "$GODOT_CMD" . --editor </dev/null >/dev/null 2>&1 &
        ;;
    *)
        # Fallback for unknown OS: try backgrounding the process.
        "$GODOT_CMD" . --editor </dev/null >/dev/null 2>&1 &
        ;;
esac

echo "Godot editor reopened."
