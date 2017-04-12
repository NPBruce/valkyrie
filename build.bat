rem cleanup
rmdir /s /q build\batch
rmdir /s /q build\batchMac

rem create base structure
mkdir build
mkdir build\batch
mkdir build\batchMac

rem I clean macos because I don't trust unity
rmdir /s /q build\macos
mkdir build\macos
mkdir build\macos\Valkyrie.app

rem Because reasons
set Target=

rem Build libraries
C:/Windows/Microsoft.NET/Framework/v4.0.30319/msbuild.exe libraries/libraries.sln /nologo /p:Configuration="Release" /p:NoWarn=0108

rem build unity
"%ProgramFiles%\Unity\Editor\Unity.exe" -batchmode -quit -projectPath %~dp0unity -buildWindowsPlayer ..\build\win\valkyrie.exe
"%ProgramFiles%\Unity\Editor\Unity.exe" -batchmode -quit -projectPath %~dp0unity -buildOSXPlayer ..\build\macos\Valkyrie.app

rem copy lience to win release
copy LICENSE build\batch\LICENSE.txt
copy NOTICE build\batch\NOTICE.txt
copy .NET-Ogg-Vorbis-Encoder-LICENSE build\batch\.NET-Ogg-Vorbis-Encoder-LICENSE.txt
copy dotnetzip-license.rtf build\batch
rem duplicate licence to macos
xcopy /I /E /Y build\batch build\batchMac\Valkyrie.app

rem create inner structure
mkdir build\batch\valkyrie_Data
mkdir build\batch\valkyrie_Data\content
mkdir build\batchMac
mkdir build\batchMac\Valkyrie.app
mkdir build\batchMac\Valkyrie.app\Contents

rem copy content from source to win release
xcopy /E /Y content build\batch\valkyrie_Data\content
rem remove imported content
rmdir /s /q build\batch\valkyrie_Data\content\D2E\ffg
rmdir /s /q build\batch\valkyrie_Data\content\MoM\ffg
rem copy content from win to macos
xcopy /I /E /Y build\batch\valkyrie_Data build\batchMac\Valkyrie.app\Contents

rem copy over windows build
xcopy /E /Y build\win build\batch
rem copy over macos build
xcopy /E /Y build\macos build\batchMac

rem read build version
set /p version=<unity\Assets\Resources\version.txt

rem delete previous build
del build\valkyrie-win-%version%.zip
del build\valkyrie-macos-%version%.zip

rem create windows zip
cd build\batch
"C:\Program Files\7-Zip\7z.exe" a ..\valkyrie-win-%version%.zip * -r
rem create macos zip
cd ..\batchMac
"C:\Program Files\7-Zip\7z.exe" a ..\valkyrie-macos-%version%.zip * -r
cd ..\..
