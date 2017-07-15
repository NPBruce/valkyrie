;NSIS Modern User Interface
;Basic Example Script
;Written by Joost Verburg

;--------------------------------
;Include Modern UI

  !include "MUI2.nsh"

;--------------------------------
;General

  ;Name and file
  Name "Valkyrie ${VERSION} Installer"
  OutFile "build\valkyrie-windows-${VERSION}.exe"

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
  RequestExecutionLevel admin

  !define MUI_ICON web\logo.ico

;--------------------------------
;Variables

  Var StartMenuFolder

;--------------------------------
;Interface Settings

  !define MUI_ABORTWARNING

;--------------------------------
;Pages

  !insertmacro MUI_PAGE_DIRECTORY
  
  ;Start Menu Folder Page Configuration
  !define MUI_STARTMENUPAGE_REGISTRY_ROOT "HKCU"
  !define MUI_STARTMENUPAGE_REGISTRY_VALUENAME "Start Menu Folder"
  !ifdef PRERELEASE
    !define MUI_STARTMENUPAGE_REGISTRY_KEY "Software\Valkyrie-${VERSION}"
    !define MUI_STARTMENUPAGE_DEFAULTFOLDER "Valkyrie ${VERSION}"
  !endif
  !ifndef PRERELEASE
    !define MUI_STARTMENUPAGE_REGISTRY_KEY "Software\Valkyrie"
    !define MUI_STARTMENUPAGE_DEFAULTFOLDER "Valkyrie"
  !endif

  !insertmacro MUI_PAGE_STARTMENU Application $StartMenuFolder

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
  
  SetOverwrite on
  File /r build\batch\*.*
  
  ;Store installation folder
  !ifdef PRERELEASE
    WriteRegStr HKCU "Software\Valkyrie-${VERSION}" "" $INSTDIR
  !endif

  !ifndef PRERELEASE
    WriteRegStr HKCU "Software\Valkyrie" "" $INSTDIR
  !endif

  ;Create uninstaller
  WriteUninstaller "$INSTDIR\Uninstall.exe"

  
  !insertmacro MUI_STARTMENU_WRITE_BEGIN Application
    
    ;Create shortcuts
    CreateDirectory "$SMPROGRAMS\$StartMenuFolder"
    CreateShortcut "$SMPROGRAMS\$StartMenuFolder\Uninstall.lnk" "$INSTDIR\Uninstall.exe"
    !ifdef PRERELEASE
      CreateShortcut "$SMPROGRAMS\$StartMenuFolder\Valkyrie-${VERSION}.lnk" "$INSTDIR\Valkyrie.exe"
    !endif
    !ifndef PRERELEASE
      CreateShortcut "$SMPROGRAMS\$StartMenuFolder\Valkyrie.lnk" "$INSTDIR\Valkyrie.exe"
    !endif
  
  !insertmacro MUI_STARTMENU_WRITE_END

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

  !insertmacro MUI_STARTMENU_GETFOLDER Application $StartMenuFolder

  RMDir /r /REBOOTOK "$SMPROGRAMS\$StartMenuFolder"

SectionEnd