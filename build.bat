rem read build version
set /p version=<unity\Assets\Resources\version.txt

rem cleanup
rmdir /s /q build\batch
rmdir /s /q build\batchMac
rmdir /s /q build\batchLinux
rmdir /s /q build\macos
rmdir /s /q build\win
rmdir /s /q build\linux

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
mkdir build\android

rem Because reasons
set Target=

rem Build libraries
C:/Windows/Microsoft.NET/Framework/v4.0.30319/msbuild.exe libraries/libraries.sln /nologo /p:Configuration="Release" /p:NoWarn=0108

rem build unity
"%ProgramFiles%\Unity\Editor\Unity.exe" -batchmode -quit -projectPath %~dp0unity -buildWindowsPlayer ..\build\win\valkyrie.exe
"%ProgramFiles%\Unity\Editor\Unity.exe" -batchmode -quit -projectPath %~dp0unity -buildOSXPlayer ..\build\macos\Valkyrie.app
"%ProgramFiles%\Unity\Editor\Unity.exe" -batchmode -quit -projectPath %~dp0unity -buildLinuxUniversalPlayer ..\build\linux\valkyrie
"%ProgramFiles%\Unity\Editor\Unity.exe" -batchmode -quit -projectPath %~dp0unity -executeMethod PerformBuild.CommandLineBuildAndroid +buildlocation ..\build\android\

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
del build\valkyrie-macos-%version%.zip
del build\valkyrie-linux-%version%.tar.gz

rem create windows zip
cd build\batch
"C:\Program Files\7-Zip\7z.exe" a ..\valkyrie-windows-%version%.zip * -r
rem create macos zip
cd ..\batchMac
"C:\Program Files\7-Zip\7z.exe" a ..\valkyrie-macos-%version%.zip * -r
rem create linux tar ball
cd ..\batchLinux
"C:\Program Files\7-Zip\7z.exe" a ..\valkyrie-linux-%version%.tar * -r
cd ..
"C:\Program Files\7-Zip\7z.exe" a valkyrie-linux-%version%.tar.gz valkyrie-linux-%version%.tar
del valkyrie-linux-%version%.tar
rem move apk
move android\test.apk valkyrie-android-%version%.apk

cd ..

set /a num=%version:~-1% 2>nul

if "%num%"=="%version:~-1%" (
    "C:\Program Files (x86)\NSIS\makensis" /DVERSION=%version% valkyrie.nsi
    goto :EOF
)

"C:\Program Files (x86)\NSIS\makensis" /DVERSION=%version% /DPRERELEASE valkyrie.nsi
