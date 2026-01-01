# AGENTS Guidelines for this repository
This repository contains a Unity engine application located in the root of this repository. When working on the project interactively with an agent (e.g. the Codex CLI) please follow the guidelines below so that the development experience continues to work smoothly.

## Purpose of the application
Valkyrie is a community developed scenario app for the board games Descent Second Edition and Mansions of Madness Second Edition.

- The app can be used to create and play user generated scenarios.
- The app requires to import data from the official Fantasy Flight games Descent Road to Legend or Mansions of Madness apps for Android or Steam or alternatively import data from a zip file.
- Custom scenarios are hosted on GitHub and then downloaded to the user device as a container file.
- Content creators can create scenarios that contain custom content such as new tiles and characters that are not included as default content in Valkyrie. These content packs can be created for Descent 2nd Edition and Mansions of Madness 2nd Edition. Those content packs can later be published on GitHub to be available in Valkyrie automatically. Using this method Valkyrie can be expanded with other games as well as long as they are based on similar systems as Descent 2nd Edition and Mansions of Madness (e.g. a DOOM: The Board Game mod is already available as custom content pack).

## Application wiki
The online documentation for the application can be found here: https://github.com/NPBruce/valkyrie/wiki

## Development environment
Coding is intended to happen in a Windows environment.

- This is mainly due to the build.bat and build.ps1 files which help with building the application for different operating systems.
- Some Unix-style commands (for example, `grep`) may not be available or function as expected.
- When providing scripts or command-line instructions, prefer cross-platform or Windows-compatible solutions where possible.

## Unity version
Unity version can be found here: unity\ProjectSettings\ProjectVersion.txt

Before implementing or suggesting any Unity-related changes, always verify compatibility with the current Unity version specified in this file.

## Target operating systems
The application is designed to run on the following operating systems:
- Windows
- Mac
- Linux
- Android

## Localization files
Localization files are located in `Assets/StreamingAssets/text/`.
- `Localization.English.txt` is the master file.
- The format is `KEY,Value`.
- When adding new text:
    1. Add the `KEY,English Value` to `Localization.English.txt`.
    2. Add a translated version `KEY,Translated Value` to *all* other relevant files (`Localization.German.txt`, `Localization.French.txt`, `Localization.Spanish.txt`, `Localization.Italian.txt`, etc.). Failing to do so will result in missing text for users of those languages.
    3. In C# code, use `new StringKey("val", "KEY")` to reference the text.
    4. For commonly used keys, add a static reference in `Assets/Scripts/Content/CommonStringKeys.cs`.