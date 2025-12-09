@echo off
setlocal EnableDelayedExpansion
rem read build version
set /p version=<unity\Assets\Resources\version.txt

echo [%TIME%] --- Starting Build Process ---
echo [%TIME%] Version: %version%

rem set default build flags if not set
IF "%BUILD_WINDOWS%"=="" SET BUILD_WINDOWS=true
IF "%BUILD_MAC%"=="" SET BUILD_MAC=true
IF "%BUILD_LINUX%"=="" SET BUILD_LINUX=true
IF "%BUILD_ANDROID%"=="" SET BUILD_ANDROID=true

echo [%TIME%] --- Build Configuration ---
echo [%TIME%] BUILD_WINDOWS: %BUILD_WINDOWS%
echo [%TIME%] BUILD_MAC:     %BUILD_MAC%
echo [%TIME%] BUILD_LINUX:   %BUILD_LINUX%
echo [%TIME%] BUILD_ANDROID: %BUILD_ANDROID%

rem set steam path. You can get it from https://store.steampowered.com/
rem is steam needed for building?
goto comment
IF "%steampath%"=="" SET steampath=%programfiles(x86)%\Steam
IF NOT EXIST "%steampath%" ( 
echo  [31m--- ERROR --- Steam path not set : please set Steam path in build.bat  [0m
exit /B 1
)
:comment

if "%BUILD_ANDROID%"=="true" (
    echo [%TIME%] Checking Android Environment...
    rem set open java development kit path. You can get it from https://developers.redhat.com/products/openjdk/download/
    IF "%JDK_HOME%"=="" SET JDK_HOME=%JAVA_HOME%
    IF "%JDK_HOME%"=="" SET JDK_HOME=%ProgramFiles%\RedHat\java-1.8.0-openjdk-1.8.0.212-3
    IF NOT EXIST "%JDK_HOME%" ( 
    echo  [31m--- ERROR --- JDK_HOME path not set : please set Java JDK home path in build.bat or create a similar environment variable [0m
    exit /B 1
    )

    rem set android sdk path. You can get it from https://developer.android.com/studio/.
    IF "%ANDROID_SDK_ROOT%"=="" SET ANDROID_SDK_ROOT=%LOCALAPPDATA%\Android\Sdk
    IF NOT EXIST "%ANDROID_SDK_ROOT%" ( 
    echo  [31m--- ERROR --- ANDROID_SDK_ROOT path not set : please set android sdk path in build.bat or create a similar environment variable [0m
    exit /B 1
    )

    rem set android build tools. The come with the android sdk. Don't use the latest 29.0.0-rc3 as the unity build will fail (if the build tools folder for it exists, manually delete it).
    IF "%ANDROID_BUILD_TOOLS%"=="" SET ANDROID_BUILD_TOOLS=%ANDROID_SDK_ROOT%\build-tools\28.0.3
    IF NOT EXIST "%ANDROID_BUILD_TOOLS%" (
    echo  [31m--- ERROR --- ANDROID_BUILD_TOOLS path not set : please set android build tool path in build.bat or create a similar environment variable [0m
    exit /B 1
    )
    echo [%TIME%] Android Environment OK.
)

rem set unity editor location
IF "%UNITY_EDITOR_HOME%"=="" SET UNITY_EDITOR_HOME=%ProgramFiles%\Unity\Editor
IF NOT EXIST "%UNITY_EDITOR_HOME%" (
echo  [31m--- ERROR --- UNITY_EDITOR_HOME path not set : please set unity editor path in build.bat or create a similar environment variable [0m
exit /B 1
)

rem find visual studio installation path so we can find the msbuild executable. e.g. in C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\
rem this way we can find whatever year/version of vs is installed.
FOR /D %%B in ("%ProgramFiles%\Microsoft Visual Studio\*") do (SET "VSPATH=%%B")

echo [%TIME%] Using Visual Studio path: %VSPATH%

rem you can get NSIS from https://nsis.sourceforge.io/Main_Page
SET PATH=%PATH%;%JDK_HOME%\bin;%UNITY_EDITOR_HOME%;%ProgramFiles%\7-Zip;%VSPATH%\Community\MSBuild\Current\Bin\;%ProgramFiles(x86)%\NSIS;%~dp0libraries\SetVersion\bin\Release;%ANDROID_BUILD_TOOLS%;%localappdata%\NuGet;%ProgramFiles%\Git\bin

rem cleanup
echo [%TIME%] Cleaning up previous builds...
rmdir /s /q build\android
rmdir /s /q build\batch
rmdir /s /q build\batchMac
rmdir /s /q build\batchLinux
rmdir /s /q build\macos
rmdir /s /q build\win
rmdir /s /q build\linux
del build\Valkyrie-android-%version%.apk

rem create base structure
echo [%TIME%] Creating build directories...
mkdir build
mkdir build\batch
mkdir build\batchMac
mkdir build\batchLinux
mkdir build\batch\valkyrie_Data
mkdir build\batch\valkyrie_Data\content
mkdir build\batchMac\Valkyrie.app
mkdir build\batchMac\Valkyrie.app\Contents
mkdir build\batchLinux\valkyrie-linux-%version%
mkdir build\batchLinux\valkyrie-linux-%version%\valkyrie_Data
mkdir build\batchLinux\valkyrie-linux-%version%\valkyrie_Data\content

mkdir build\win
mkdir build\macos
mkdir build\macos\Valkyrie.app
mkdir build\linux

rem download the latest version of Android Storage Access Framework library.
echo [%TIME%] Downloading Android dependencies...
POWERSHELL -Command "Invoke-WebRequest -Uri https://github.com/seinsinnes/FSAF/releases/latest/download/fsaf-release.aar -OutFile .\unity\Assets\Plugins\Android\fsaf-release.aar"

rem Because reasons
set Target=

rem install any external packages required by the libraries
echo [%TIME%] Restoring NuGet packages...
winget install -q Microsoft.NuGet -l "%localappdata%\NuGet" --accept-source-agreements --accept-package-agreements
nuget restore libraries/libraries.sln

rem Build libraries
echo [%TIME%] Building libraries...
msbuild libraries/libraries.sln /nologo /p:Configuration="Release" /p:NoWarn=0108
echo [%TIME%] Libraries built.

rem this will change the version information in the project settings
echo [%TIME%] Updating version info...
SetVersion %~dp0

del ".\unity\Assets\Plugins\UnityEngine.dll"

rem build unity
echo [%TIME%] --- Starting Unity Builds ---

if "%BUILD_WINDOWS%"=="true" (
    echo [%TIME%] Building for Windows...
    Unity -batchmode -quit -projectPath "%~dp0unity" -buildWindowsPlayer ..\build\win\valkyrie.exe
    set BUILD_STATUS=!ERRORLEVEL!
    copy %LOCALAPPDATA%\Unity\Editor\Editor.log .\build\Editor_valkyrie-windows.log
    if !BUILD_STATUS! NEQ 0 (
        echo [31m--- ERROR --- Unity Windows Build Failed [0m
        type .\build\Editor_valkyrie-windows.log
        exit /b 1
    )
    echo [%TIME%] Windows build complete. Log: .\build\Editor_valkyrie-windows.log
)

if "%BUILD_MAC%"=="true" (
    echo [%TIME%] Building for macOS...
    Unity -batchmode -quit -projectPath "%~dp0unity" -buildTarget OSXUniversal -buildOSXUniversalPlayer ..\build\macos\Valkyrie.app
    set BUILD_STATUS=!ERRORLEVEL!
    copy %LOCALAPPDATA%\Unity\Editor\Editor.log .\build\Editor_valkyrie-macos.log
    if !BUILD_STATUS! NEQ 0 (
        echo [31m--- ERROR --- Unity macOS Build Failed [0m
        type .\build\Editor_valkyrie-macos.log
        exit /b 1
    )
    echo [%TIME%] macOS build complete. Log: .\build\Editor_valkyrie-macos.log
)

if "%BUILD_LINUX%"=="true" (
    echo [%TIME%] Building for Linux...
    Unity -batchmode -quit -projectPath "%~dp0unity" -buildLinux64Player ..\build\linux\valkyrie
    set BUILD_STATUS=!ERRORLEVEL!
    copy %LOCALAPPDATA%\Unity\Editor\Editor.log .\build\Editor_valkyrie-linux.log
    if !BUILD_STATUS! NEQ 0 (
        echo [31m--- ERROR --- Unity Linux Build Failed [0m
        type .\build\Editor_valkyrie-linux.log
        exit /b 1
    )
    echo [%TIME%] Linux build complete. Log: .\build\Editor_valkyrie-linux.log
)

if "%BUILD_ANDROID%"=="true" (
    echo [%TIME%] Building for Android...
    Unity -batchmode -quit -projectPath "%~dp0unity" -executeMethod PerformBuild.CommandLineBuildAndroid +buildlocation "%~dp0build\android\Valkyrie-android.apk"
    set BUILD_STATUS=!ERRORLEVEL!
    copy %LOCALAPPDATA%\Unity\Editor\Editor.log .\build\Editor_valkyrie-android.log
    echo [%TIME%] Android build finished with status: !BUILD_STATUS!. Log: .\build\Editor_valkyrie-android.log
    
    if !BUILD_STATUS! NEQ 0 (
        echo  [31m--- ERROR --- Unity Build Failed [0m
        type .\build\Editor_valkyrie-android.log
        exit /b 1
    )

    rem delete the META-INF from the apk
    echo [%TIME%] Processing APK...
    7z -tzip d "%~dp0build\android\Valkyrie-android.apk" META-INF
    rem jarsigner comes form the JDK
    echo [%TIME%] Signing APK...
    jarsigner -keystore "%~dp0unity\user.keystore" -storepass valkyrie -keypass valkyrie "%~dp0build\android\Valkyrie-android.apk" com.bruce.valkyrie
    jarsigner -verify -verbose -certs "%~dp0build\android\Valkyrie-android.apk"
    rem zipalign comes from the android build tools
    echo [%TIME%] Aligning APK...
    zipalign -v 4 "%~dp0build\android\Valkyrie-android.apk" "%~dp0build\Valkyrie-android-%version%.apk"
    echo [%TIME%] Android post-processing complete.
)

rem copy lience to win release
echo [%TIME%] Copying licenses...
copy LICENSE build\batch\LICENSE.txt
copy NOTICE build\batch\NOTICE.txt
copy .NET-Ogg-Vorbis-Encoder-LICENSE build\batch\.NET-Ogg-Vorbis-Encoder-LICENSE.txt
copy dotnetzip-license.rtf build\batch
rem duplicate licence to macos/linux
xcopy /I /E /Y build\batch build\batchMac\Valkyrie.app
xcopy /I /E /Y build\batch build\batchLinux\valkyrie-linux-%version%

rem copy over windows build
echo [%TIME%] Packaging Windows build...
xcopy /E /Y build\win build\batch
rem copy over macos build
echo [%TIME%] Packaging macOS build...
xcopy /E /Y build\macos build\batchMac
rem copy over linux build
echo [%TIME%] Packaging Linux build...
xcopy /E /Y build\linux build\batchLinux\valkyrie-linux-%version%

rem delete previous build
echo [%TIME%] Removing old packages...
del build\valkyrie-windows-%version%.exe
del build\valkyrie-windows-%version%.zip
del build\valkyrie-windows-%version%.7z
del build\valkyrie-macos-%version%.tar.gz
del build\valkyrie-linux-%version%.tar.gz

rem create windows zip
if "%BUILD_WINDOWS%"=="true" (
    echo [%TIME%] Zipping Windows build...
    7z a "%~dp0build\valkyrie-windows-%version%.7z" "%~dp0build\batch\*" -r
    rem create windows 7z
    7z a "%~dp0build\valkyrie-windows-%version%.zip" "%~dp0build\batch\*" -r
)
rem create macos tar ball
if "%BUILD_MAC%"=="true" (
    echo [%TIME%] Compressing macOS build...
    7z a "%~dp0build\batchMac\valkyrie-macos-%version%.tar" "%~dp0build\batchMac\*" -r
    7z a "%~dp0build\valkyrie-macos-%version%.tar.gz" "%~dp0build\batchMac\valkyrie-macos-%version%.tar"
    del "%~dp0build\batchMac\valkyrie-macos-%version%.tar"
)
rem create linux tar ball
if "%BUILD_LINUX%"=="true" (
    echo [%TIME%] Compressing Linux build...
    7z a "%~dp0build\batchLinux\valkyrie-linux-%version%.tar" "%~dp0build\batchLinux\*" -r
    7z a "%~dp0build\valkyrie-linux-%version%.tar.gz" "%~dp0build\batchLinux\valkyrie-linux-%version%.tar"
    del "%~dp0build\batchLinux\valkyrie-linux-%version%.tar"
)
rem move apk
IF EXIST android\test.apk move android\test.apk valkyrie-android-%version%.apk

set /a num=%version:~-1% 2>nul

echo [%TIME%] Creating Installer...
if "%num%"=="%version:~-1%" (
    makensis /DVERSION=%version% valkyrie.nsi
    echo [%TIME%] Installer created (Release).
    goto :EOF
)

makensis /DVERSION=%version% /DPRERELEASE valkyrie.nsi
echo [%TIME%] Installer created (Pre-release).
echo [%TIME%] --- Build Process Complete ---
