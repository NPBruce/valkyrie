---
trigger: always_on
---

# Project structure and logic
- The application is purely based on unities UI system (Canvas, UI elements) for creating the app user interface.
- The unity project is located in folder `unity`.
- There is only one scene in the unity project located at `unity\Assets\Scenes\Game.unity`.

## Assets
Assets are located in folder `unity/Assets`.

### Unity plugins
Unity plugins are located in folder `unity/Assets/Plugins`. The following plugins are used:
- **Firebase**: Google Firebase App and Crashlytics for analytics and crash reporting.
- **Ionic.Zip.Unity**: Library for handling ZIP files.
- **LZ4 Compression**: Library for LZ4 compression.
- **NativeFilePicker**: Native file picker for Android and iOS.
- **StandaloneFileBrowser**: Native file browser for desktop platforms (Windows, macOS, Linux).
- **TextMeshPro**: Advanced text rendering for Unity.

## Code
C# code is located in folder `unity/Assets/Scripts`.

## Unit tests.
Unit tests are located in folder `unity\Assets\UnitTests`.

### Constants
String constants are located in file `unity/Assets/Scripts/ValkyrieConstants.cs`.

### UI components
UI components are located in folder `unity/Assets/Scripts/UI`.

### UI screens
UI screens are located in folder `unity/Assets/Scripts/UI/Screens`.

### Website
The public GitHub website data is located in folder `web` and `index.html`.

## GitHub data

### GitHub actions
GitHub actions are located in folder `.github/workflows/`.

#### GitHub action scripts
GitHub actions scripts are located in folder `valkyrie\workflowScripts`.

### GitHub issue templates
GitHub issues are located in folder `.github/ISSUE_TEMPLATE/`.

## Libraries
Additional c# helper libraries are located in folder `libraries`. Helper libraries are:
- **FFGAppImport**: Imports assets from official FFG apps.
- **ValkyrieTools**: Common helpers and Android JNI utilities.
- **SetVersion**: Updates version numbers in build files.
- **ObbExtract**: Extract files from Android OBB archives.
- **IADBExtract / Injection / MoMInjection**: Tools to extract/convert game data to Valkyrie format.
- **PuzzleGenerator**: Generates puzzle data.

## Resources
Resources are located in folder `unity/Assets/Resources`. Resource data contains:
- External scripts: External files used for different purposes under `unity/Assets/Resources/Scripts/`
- Fonts: Font files under `unity/Assets/Resources/Fonts/`
- Sprites: Icons and other common images under `unity/Assets/Resources/Sprites/`

## Build scripts
There are two build scripts that can by used as alternative for building the application in Unity editor. For more information on build see [Developer guide](https://github.com/NPBruce/valkyrie/wiki/Developer-Guide).
- `workflowScripts/build.bat` is a batch file for Windows.
- `workflowScripts/build.ps1` is a PowerShell script for Windows. This file is used in the GitHub actions build pipeline (`github\workflows\buildAndOptionalRelease.yml`).