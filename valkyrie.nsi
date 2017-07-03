;NSIS Modern User Interface
;Basic Example Script
;Written by Joost Verburg

;--------------------------------
;Include Modern UI

  !include "MUI2.nsh"

;--------------------------------
;General

  ;Name and file
  Name "Valkyrie Installer"
  OutFile "build\Valkyrie-${VERSION}.exe"

  !ifdef PRERELEASE
    ;Default installation folder
    InstallDir "$PROGRAMFILES\Valkyrie-${VERSION}"

    ;Get installation folder from registry if available
    InstallDirRegKey HKCU "Software\Valkyrie-${VERSION}" InstallLocation
  !endif

  !ifndef PRERELEASE
    ;Default installation folder
    InstallDir "$PROGRAMFILES\Valkyrie"
  
    ;Get installation folder from registry if available
    InstallDirRegKey HKCU "Software\Valkyrie" InstallLocation
  !endif

  ;Request application privileges for Windows Vista
  RequestExecutionLevel user

;--------------------------------
;Interface Settings

  !define MUI_ABORTWARNING

;--------------------------------
;Pages

  !insertmacro MUI_PAGE_DIRECTORY
  !insertmacro MUI_PAGE_INSTFILES
  
  !insertmacro MUI_UNPAGE_CONFIRM
  !insertmacro MUI_UNPAGE_INSTFILES
  
;--------------------------------
;Languages
 
  !insertmacro MUI_LANGUAGE "English"

;--------------------------------
;Installer Sections

Section "Dummy Section" SecDummy

  SetOutPath "$INSTDIR"
  
  File /r ..\batch\*.*
  
  ;Store installation folder
  !ifdef PRERELEASE
    WriteRegStr HKCU "Software\Valkyrie-${VERSION}" "" $INSTDIR
  !endif

  !ifndef PRERELEASE
    WriteRegStr HKCU "Software\Valkyrie" "" $INSTDIR
  !endif

  ;Create uninstaller
  WriteUninstaller "$INSTDIR\Uninstall.exe"

SectionEnd

;--------------------------------
;Descriptions

  ;Language strings
  LangString DESC_SecDummy ${LANG_ENGLISH} "A test section."

  ;Assign language strings to sections
  !insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
    !insertmacro MUI_DESCRIPTION_TEXT ${SecDummy} $(DESC_SecDummy)
  !insertmacro MUI_FUNCTION_DESCRIPTION_END

;--------------------------------
;Uninstaller Section

Section "Uninstall"

  RMDir /r /REBOOTOK "$INSTDIR"

  !ifdef PRERELEASE
    DeleteRegKey HKCU "Software\Valkyrie-${VERSION}"
  !endif

  !ifndef PRERELEASE
    DeleteRegKey HKCU "Software\Valkyrie"
  !endif

SectionEnd