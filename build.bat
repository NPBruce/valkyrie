@echo off
rem read build version
set /p version=<unity\Assets\Resources\version.txt

rem set steam path. You can get it from https://store.steampowered.com/
IF "%steampath%"=="" SET steampath=%programfiles(x86)%\Steam
IF NOT EXIST "%steampath%" ( 
echo [31m--- ERROR --- Steam path not set : please set Steam path in build.bat [0m
exit /B
)

rem set open java development kit path. You can get it from https://developers.redhat.com/products/openjdk/download/
IF "%JDK_HOME%"=="" SET JDK_HOME=%JAVA_HOME%
IF "%JDK_HOME%"=="" SET JDK_HOME=%ProgramFiles%\RedHat\java-1.8.0-openjdk-1.8.0.212-3
IF NOT EXIST "%JDK_HOME%" ( 
echo [31m--- ERROR --- JDK_HOME path not set : please set Java JDK home path in build.bat or create a similar environment variable[0m
exit /B
)

rem set android sdk path. You can get it from https://developer.android.com/studio/.
IF "%ANDROID_SDK_ROOT%"=="" SET ANDROID_SDK_ROOT=%LOCALAPPDATA%\Android\Sdk
IF NOT EXIST "%ANDROID_SDK_ROOT%" ( 
echo [31m--- ERROR --- ANDROID_SDK_ROOT path not set : please set android sdk path in build.bat or create a similar environment variable[0m
exit /B
)

rem set android build tools. The come with the android sdk. Don't use the latest 29.0.0-rc3 as the unity build will fail (if the build tools folder for it exists, manually delete it).
IF "%ANDROID_BUILD_TOOLS%"=="" SET ANDROID_BUILD_TOOLS=%ANDROID_SDK_ROOT%\build-tools\28.0.3
IF NOT EXIST "%ANDROID_BUILD_TOOLS%" (
echo [31m--- ERROR --- ANDROID_BUILD_TOOLS path not set : please set android build tool path in build.bat or create a similar environment variable[0m
exit /B
)

rem set unity editor location
IF "%UNITY_EDITOR_HOME%"=="" SET UNITY_EDITOR_HOME=%ProgramFiles%\Unity\Editor
IF NOT EXIST "%UNITY_EDITOR_HOME%" (
echo [31m--- ERROR --- UNITY_EDITOR_HOME path not set : please set unity editor path in build.bat or create a similar environment variable[0m
exit /B
)

rem find visual studio installation path so we can find the msbuild executable. e.g. in C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\
rem this way we can find whatever year/version of vs is installed.
FOR /D %%B in ("%ProgramFiles%\Microsoft Visual Studio\*") do (SET "VSPATH=%%B")

echo using visual studio path: %VSPATH%

rem you can get NSIS from https://nsis.sourceforge.io/Main_Page
SET PATH=%PATH%;%JDK_HOME%\bin;%UNITY_EDITOR_HOME%;%ProgramFiles%\7-Zip;%VSPATH%\Community\MSBuild\Current\Bin\;%ProgramFiles(x86)%\NSIS;%~dp0libraries\SetVersion\bin\Release;%ANDROID_BUILD_TOOLS%;%localappdata%\NuGet;%ProgramFiles%\Git\bin

rem cleanup
rmdir /s /q build\android
rmdir /s /q build\batch
rmdir /s /q build\batchMac
rmdir /s /q build\batchLinux
rmdir /s /q build\macos
rmdir /s /q build\win
rmdir /s /q build\linux
del build\Valkyrie-android-%version%.apk

rem create base structure
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
POWERSHELL -Command "Invoke-WebRequest -Uri https://github.com/seinsinnes/FSAF/releases/latest/download/fsaf-release.aar -OutFile .\unity\Assets\Plugins\Android\fsaf-release.aar"

rem Because reasons
set Target=

rem install any external packages required by the libraries
winget install -q Microsoft.NuGet -l "%localappdata%\NuGet" --accept-source-agreements --accept-package-agreements
nuget restore libraries/libraries.sln

rem Build libraries
msbuild libraries/libraries.sln /nologo /p:Configuration="Release" /p:NoWarn=0108

rem this will change the version information in the project settings
SetVersion %~dp0

rem build unity
Unity -batchmode -quit -projectPath "%~dp0unity" -buildWindowsPlayer ..\build\win\valkyrie.exe
rem copy %LOCALAPPDATA%\Unity\Editor\Editor.log ..\build\Editor_valkyrie-windows.log 

Unity -batchmode -quit -projectPath "%~dp0unity" -buildTarget OSXUniversal -buildOSXUniversalPlayer ..\build\macos\Valkyrie.app
rem copy %LOCALAPPDATA%\Unity\Editor\Editor.log %LOCALAPPDATA%\Unity\Editor\Editor_valkyrie-macos.log 

Unity -batchmode -quit -projectPath "%~dp0unity" -buildLinuxUniversalPlayer ..\build\linux\valkyrie
rem copy %LOCALAPPDATA%\Unity\Editor\Editor.log %LOCALAPPDATA%\Unity\Editor\Editor_valkyrie-linux.log

Unity -batchmode -quit -projectPath "%~dp0unity" -executeMethod PerformBuild.CommandLineBuildAndroid +buildlocation "%~dp0build\android\Valkyrie-android.apk"
rem copy %LOCALAPPDATA%\Unity\Editor\Editor.log Editor_valkyrie-android.log

rem delete the META-INF from the apk
7z -tzip d "%~dp0build\android\Valkyrie-android.apk" META-INF
rem jarsigner comes form the JDK
jarsigner -keystore "%~dp0unity\user.keystore" -storepass valkyrie -keypass valkyrie "%~dp0build\android\Valkyrie-android.apk" com.bruce.valkyrie
jarsigner -verify -verbose -certs "%~dp0build\android\Valkyrie-android.apk"
rem zipalign comes from the android build tools
zipalign -v 4 "%~dp0build\android\Valkyrie-android.apk" "%~dp0build\Valkyrie-android-%version%.apk"

rem copy lience to win release
copy LICENSE build\batch\LICENSE.txt
copy NOTICE build\batch\NOTICE.txt
copy .NET-Ogg-Vorbis-Encoder-LICENSE build\batch\.NET-Ogg-Vorbis-Encoder-LICENSE.txt
copy dotnetzip-license.rtf build\batch
rem duplicate licence to macos/linux
xcopy /I /E /Y build\batch build\batchMac\Valkyrie.app
xcopy /I /E /Y build\batch build\batchLinux\valkyrie-linux-%version%

rem copy over windows build
xcopy /E /Y build\win build\batch
rem copy over macos build
xcopy /E /Y build\macos build\batchMac
rem copy over linux build
xcopy /E /Y build\linux build\batchLinux\valkyrie-linux-%version%

rem delete previous build
del build\valkyrie-windows-%version%.exe
del build\valkyrie-windows-%version%.zip
del build\valkyrie-windows-%version%.7z
del build\valkyrie-macos-%version%.tar.gz
del build\valkyrie-linux-%version%.tar.gz

rem create windows zip
7z a "%~dp0build\valkyrie-windows-%version%.7z" "%~dp0build\batch\*" -r
rem create windows 7z
7z a "%~dp0build\valkyrie-windows-%version%.zip" "%~dp0build\batch\*" -r
rem create macos tar ball
7z a "%~dp0build\batchMac\valkyrie-macos-%version%.tar" "%~dp0build\batchMac\*" -r
7z a "%~dp0build\valkyrie-macos-%version%.tar.gz" "%~dp0build\batchMac\valkyrie-macos-%version%.tar"
del "%~dp0build\batchMac\valkyrie-macos-%version%.tar"
rem create linux tar ball
7z a "%~dp0build\batchLinux\valkyrie-linux-%version%.tar" "%~dp0build\batchLinux\*" -r
7z a "%~dp0build\valkyrie-linux-%version%.tar.gz" "%~dp0build\batchLinux\valkyrie-linux-%version%.tar"
del "%~dp0build\batchLinux\valkyrie-linux-%version%.tar"
rem move apk
IF EXIST android\test.apk move android\test.apk valkyrie-android-%version%.apk

set /a num=%version:~-1% 2>nul

if "%num%"=="%version:~-1%" (
    makensis /DVERSION=%version% valkyrie.nsi
    goto :EOF
)

makensis /DVERSION=%version% /DPRERELEASE valkyrie.nsi
