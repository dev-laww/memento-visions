#!/bin/sh

# Determine the Godot executable from the PATH
if command -v godot >/dev/null 2>&1; then
    GODOT_CMD="godot"
elif command -v godot.exe >/dev/null 2>&1; then
    GODOT_CMD="godot.exe"
else
    echo "Godot executable not found in PATH. Please install Godot or add it to your PATH."
    exit 1
fi


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

echo "Godot editor opened."
