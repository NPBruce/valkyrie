mkdir build
rmdir /s /q build\batch
rmdir /s /q build\batchMac
mkdir build\batch
mkdir build\batchMac

copy LICENSE build\batch
copy NOTICE build\batch
copy .NET-Ogg-Vorbis-Encoder-LICENSE build\batch
copy dotnetzip-license.rtf build\batch
xcopy /E /Y build\batch build\batchMac

mkdir build\batch\valkyrie_Data
mkdir build\batch\valkyrie_Data\content
xcopy /E /Y content build\batch\valkyrie_Data\content
rmdir /s /q build\batch\valkyrie_Data\content\D2E\ffg
rmdir /s /q build\batch\valkyrie_Data\content\MoM\ffg

mkdir build\batchMac\macos.app
mkdir build\batchMac\macos.app\Contents
xcopy /E /Y build\batch\valkyrie_Data build\batchMac\Valkyrie.app\Contents

xcopy /E /Y build\win build\batch

xcopy /E /Y build\macos build\batchMac

set /p version=<Assets\Resources\version.txt

del build\valkyrie-win-%version%.zip
del build\valkyrie-macos-%version%.zip
cd build\batch
"C:\Program Files\7-Zip\7z.exe" a ..\valkyrie-win-%version%.zip * -r
cd ..\batchMac
"C:\Program Files\7-Zip\7z.exe" a ..\valkyrie-macos-%version%.zip * -r
cd ..\..
