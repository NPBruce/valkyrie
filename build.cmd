del ..\build\valkyrie.zip
rmdir /s /q ..\build\batch
mkdir ..\build\batch
xcopy /E /Y ..\build\unity ..\build\batch
copy LICENSE ..\build\batch
copy NOTICE ..\build\batch
copy dotnetzip-license.rtf ..\build\batch
mkdir ..\build\batch\valkyrie_Data\valkyrie-contentpacks
mkdir ..\build\batch\valkyrie_Data\valkyrie-contentpacks\D2E
mkdir ..\build\batch\valkyrie_Data\valkyrie-questdata
mkdir ..\build\batch\valkyrie_Data\valkyrie-questdata\example
xcopy /E /Y ..\valkyrie-contentpacks\D2E ..\build\batch\valkyrie_Data\valkyrie-contentpacks\D2E
xcopy /E /Y ..\valkyrie-questdata\example ..\build\batch\valkyrie_Data\valkyrie-questdata\example
set /p version=<Assets\Resources\version.txt

"C:\Program Files\7-Zip\7z.exe" a ..\build\valkyrie-win-%version%.zip ..\build\batch\* -r
