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

rem Build libraries
C:/Windows/Microsoft.NET/Framework/v3.5/msbuild.exe libraries/libraries.sln /nologo /p:Configuration="Release"
