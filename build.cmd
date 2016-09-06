mkdir build
del build\valkyrie.zip
rmdir /s /q build\batch
rmdir /s /q content\D2E\ffg
mkdir build\batch
xcopy /E /Y build\unity build\batch
copy LICENSE build\batch
copy NOTICE build\batch
copy dotnetzip-license.rtf build\batch
mkdir build\batch\valkyrie_Data\content
mkdir build\batch\valkyrie_Data\quests
xcopy /E /Y content build\batch\valkyrie_Data\content
xcopy /E /Y quests build\batch\valkyrie_Data\quests
set /p version=<Assets\Resources\version.txt

del build\valkyrie-win-%version%.zip
cd build\batch
"C:\Program Files\7-Zip\7z.exe" a ..\valkyrie-win-%version%.zip * -r
cd ..\..
