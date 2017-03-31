rem cleanup
rmdir /s /q build\batch
rmdir /s /q build\batchMac

rem create structure
mkdir build
mkdir build\batch
mkdir build\batch\valkyrie_Data
mkdir build\batch\valkyrie_Data\content
mkdir build\batchMac
mkdir build\batchMac\macos.app
mkdir build\batchMac\macos.app\Contents

rem copy lience to win release
copy LICENSE build\batch\LICENSE.txt
copy NOTICE build\batch\NOTICE.txt
copy .NET-Ogg-Vorbis-Encoder-LICENSE build\batch\.NET-Ogg-Vorbis-Encoder-LICENSE.txt
copy dotnetzip-license.rtf build\batch
rem duplicate licence to macos
xcopy /E /Y build\batch build\batchMac\Valkyrie.app

rem copy content from source to win release
xcopy /E /Y content build\batch\valkyrie_Data\content
rem remove imported content
rmdir /s /q build\batch\valkyrie_Data\content\D2E\ffg
rmdir /s /q build\batch\valkyrie_Data\content\MoM\ffg
rem copy content from win to macos
xcopy /E /Y build\batch\valkyrie_Data build\batchMac\Valkyrie.app\Contents

rem copy over windows build
xcopy /E /Y build\win build\batch
rem copy over macos build
xcopy /E /Y build\macos build\batchMac

rem read build version
set /p version=<Assets\Resources\version.txt

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
