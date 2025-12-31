# AGENTS Guidelines for This Repository
This repository contains a Unity application located in the root of this repository. When
working on the project interactively with an agent (e.g. the Codex CLI) please follow
the guidelines below so that the development experience continues to work smoothly.

## Development environment
Coding is planned to happen in a Windows environment. Therefore commands like -grep will not work.

## Localization files
Localization files are located in `Assets/StreamingAssets/text/`.
- `Localization.English.txt` is the master file.
- The format is `KEY,Value`.
- When adding new text:
    1. Add the `KEY,English Value` to `Localization.English.txt`.
    2. Add a translated version `KEY,Translated Value` to *all* other relevant files (`Localization.German.txt`, `Localization.French.txt`, `Localization.Spanish.txt`, `Localization.Italian.txt`, etc.). Failing to do so will result in missing text for users of those languages.
    3. In C# code, use `new StringKey("val", "KEY")` to reference the text.
    4. For commonly used keys, add a static reference in `Assets/Scripts/Content/CommonStringKeys.cs`.