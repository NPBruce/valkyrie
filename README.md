![Valkyrie](https://raw.githubusercontent.com/NPBruce/valkyrie/master/web/banner.png)

Valkyrie custom scenario tool for Fantasy Flight Board Games.

- Website: https://npbruce.github.io/valkyrie/
- Information: https://github.com/NPBruce/valkyrie/wiki
- Discord server for community support: https://discord.gg/yrVeSVt

## Purpose of the application
Valkyrie is a community developed scenario builder for the board games Descent Second Edition and Mansions of Madness Second Edition.

- The app can be used to create and play user generated scenarios.
- The app requires to import data from the official Fantasy Flight games Descent Road to Legend or Mansions of Madness apps for Android or Steam or alternatively import data from a zip file.
- Custom scenarios are hosted on GitHub and then downloaded to the user device as a container file.
- Content creators can create scenarios that contain custom content such as new tiles and characters that are not included as default content in Valkyrie. These content packs can be created for Descent 2nd Edition and Mansions of Madness 2nd Edition. Those content packs can later be published on GitHub to be available in Valkyrie automatically. Using this method Valkyrie can be expanded with other games as well as long as they are based on similar systems as Descent 2nd Edition and Mansions of Madness (e.g. a DOOM: The Board Game mod is already available as custom content pack).

## Project structure and logic
- The application is purely based on unities UI system (Canvas, UI elements) for creating the app user interface.
- The unity project is located in folder `unity`.
- There is only one scene in the unity project located at `unity\Assets\Scenes\Game.unity`.

### Assets
Assets are located in folder `unity/Assets`.

### Unity plugins
Unity plugins are located in folder `unity/Assets/Plugins`. The following plugins are used:
- **Firebase**: Google Firebase App and Crashlytics for analytics and crash reporting.
- **Ionic.Zip.Unity**: Library for handling ZIP files.
- **LZ4 Compression**: Library for LZ4 compression.
- **NativeFilePicker**: Native file picker for Android and iOS.
- **StandaloneFileBrowser**: Native file browser for desktop platforms (Windows, macOS, Linux).
- **TextMeshPro**: Advanced text rendering for Unity.

### Code
C# code is located in folder `unity/Assets/Scripts`.

### Unit tests.
Unit tests are located in folder `unity\Assets\UnitTests`.

#### Constants
String constants are located in file `unity/Assets/Scripts/ValkyrieConstants.cs`.

#### UI components
UI components are located in folder `unity/Assets/Scripts/UI`.

#### UI screens
UI screens are located in folder `unity/Assets/Scripts/UI/Screens`.

### Website
The public GitHub website data is located in folder `web` and `index.html`.

### GitHub data

#### GitHub actions
GitHub actions are located in folder `.github/workflows/`.

#### GitHub issue templates
GitHub issues are located in folder `.github/ISSUE_TEMPLATE/`.

### Libraries
Additional c# helper libraries are located in folder `libraries`. Helper libraries are:
- **FFGAppImport**: Imports assets from official FFG apps.
- **ValkyrieTools**: Common helpers and Android JNI utilities.
- **SetVersion**: Updates version numbers in build files.
- **ObbExtract**: Extract files from Android OBB archives.
- **IADBExtract / Injection / MoMInjection**: Tools to extract/convert game data to Valkyrie format.
- **PuzzleGenerator**: Generates puzzle data.

#### Resources
Resources are located in folder `unity/Assets/Resources`. Resource data contains:
- External scripts: External files used for different purposes under `unity/Assets/Resources/Scripts/`
- Fonts: Font files under `unity/Assets/Resources/Fonts/`
- Sprites: Icons and other common images under `unity/Assets/Resources/Sprites/`

### Build scripts
There are two build scripts:
- `build.bat` is a batch file for Windows.
- `build.ps1` is a PowerShell script for Windows. This file is used in the GitHub actions build pipeline (`github\workflows\buildAndOptionalRelease.yml`).