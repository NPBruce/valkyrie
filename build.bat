@echo off
rem read build version
rem read build version
set /p version=<unity\Assets\Resources\version.txt

rem set steam path
IF "%steampath%"=="" SET steampath=%programfiles(x86)%\Steam
IF NOT EXIST "%steampath%" ( 
echo [31m--- ERROR --- Steam path not set : please set Steam path in build.bat [0m
exit /B
)

rem set java path
IF "%JAVA_HOME%"=="" SET JAVA_HOME=%ProgramFiles%\Java\jdk1.8.0_192\bin
IF NOT EXIST "%JAVA_HOME%" ( 
echo [31m--- ERROR --- JAVA_HOME path not set : please set Java home path in build.bat [0m
exit /B
)

SET PATH=%PATH%;%JAVA_HOME%;%ProgramFiles%\Unity\Editor;%ProgramFiles%\7-Zip;%WinDir%/Microsoft.NET/Framework/v4.0.30319;%ProgramFiles(x86)%\NSIS
@echo on

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

rem Because reasons
set Target=

rem Build libraries
msbuild libraries/libraries.sln /nologo /p:Configuration="Release" /p:NoWarn=0108

rem build unity
Unity -batchmode -quit -projectPath "%~dp0unity" -buildWindowsPlayer ..\build\win\valkyrie.exe
rem copy %LOCALAPPDATA%\Unity\Editor\Editor.log %LOCALAPPDATA%\Unity\Editor\Editor_valkyrie-windows.log 

Unity -batchmode -quit -projectPath "%~dp0unity" -buildTarget OSXUniversal -buildOSXUniversalPlayer ..\build\macos\Valkyrie.app
rem copy %LOCALAPPDATA%\Unity\Editor\Editor.log %LOCALAPPDATA%\Unity\Editor\Editor_valkyrie-macos.log 

Unity -batchmode -quit -projectPath "%~dp0unity" -buildLinuxUniversalPlayer ..\build\linux\valkyrie
rem copy %LOCALAPPDATA%\Unity\Editor\Editor.log %LOCALAPPDATA%\Unity\Editor\Editor_valkyrie-linux.log 

Unity -batchmode -quit -projectPath "%~dp0unity" -executeMethod PerformBuild.CommandLineBuildAndroid +buildlocation "%~dp0build\android\Valkyrie-android.apk"
rem copy %LOCALAPPDATA%\Unity\Editor\Editor.log %LOCALAPPDATA%\Unity\Editor\Editor_valkyrie-android.log

rem delete the META-INF from the apk
7z -tzip d "%~dp0build\android\Valkyrie-android.apk" META-INF
rem jarsigner comes form the JDK
jarsigner -verbose -keystore "%~dp0unity\user.keystore" -digestalg SHA1 -sigalg MD5withRSA -storepass valkyrie -signedjar "%~dp0build\Valkyrie-android-%version%.apk" "%~dp0build\android\Valkyrie-android.apk" com.bruce.valkyrie

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
del build\valkyrie-macos-%version%.zip
del build\valkyrie-linux-%version%.tar.gz

rem create windows zip
7z a "%~dp0build\valkyrie-windows-%version%.7z" "%~dp0build\batch\*" -r
rem create windows 7z
7z a "%~dp0build\valkyrie-windows-%version%.zip" "%~dp0build\batch\*" -r
rem create macos zip
7z a "%~dp0build\valkyrie-macos-%version%.zip" "%~dp0build\batchMac\*" -r
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
