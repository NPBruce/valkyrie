del ..\build\valkyrie.zip
rmdir /s /q ..\build\batch
rmdir /s /q .\content\D2E\ffg
mkdir ..\build\batch
xcopy /E /Y ..\build\unity ..\build\batch
copy LICENSE ..\build\batch
copy NOTICE ..\build\batch
copy dotnetzip-license.rtf ..\build\batch
mkdir ..\build\batch\valkyrie_Data\content
mkdir ..\build\batch\valkyrie_Data\valkyrie-questdata
mkdir ..\build\batch\valkyrie_Data\valkyrie-questdata\example
xcopy /E /Y .\content ..\build\batch\valkyrie_Data\content
xcopy /E /Y ..\valkyrie-questdata\example ..\build\batch\valkyrie_Data\valkyrie-questdata\example
set /p version=<Assets\Resources\version.txt

"C:\Program Files\7-Zip\7z.exe" a ..\build\valkyrie-win-%version%.zip ..\build\batch\* -r
