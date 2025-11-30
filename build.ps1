# build.ps1
# PowerShell Build Script for Valkyrie

$ErrorActionPreference = "Stop"
$ScriptRoot = $PSScriptRoot

# -----------------------------------------------------------------------------
# Helper Functions
# -----------------------------------------------------------------------------

function Write-Log {
    param (
        [string]$Message,
        [string]$Level = "INFO",
        [string]$Color = "White"
    )
    $Timestamp = Get-Date -Format "HH:mm:ss.ff"
    Write-Host "[$Timestamp] [$Level] $Message" -ForegroundColor $Color
}

function Invoke-CommandChecked {
    param (
        [scriptblock]$Command,
        [string]$ErrorMessage = "Command failed",
        [string]$LogFile = $null
    )
    
    try {
        & $Command
        if ($LASTEXITCODE -ne 0) {
            throw "$ErrorMessage (Exit Code: $LASTEXITCODE)"
        }
    }
    catch {
        Write-Log "ERROR: $_" -Level "ERROR" -Color "Red"
        if (![string]::IsNullOrEmpty($LogFile) -and (Test-Path $LogFile)) {
            Write-Log "Dumping log file: $LogFile" -Level "ERROR" -Color "Yellow"
            Get-Content $LogFile | Write-Host
        }
        exit 1
    }
}

function Test-PathOrExit {
    param (
        [string]$Path,
        [string]$Name
    )
    if (-not (Test-Path $Path)) {
        Write-Log "ERROR: $Name path not found: $Path" -Level "ERROR" -Color "Red"
        exit 1
    }
}

# -----------------------------------------------------------------------------
# Configuration & Environment Setup
# -----------------------------------------------------------------------------

Write-Log "--- Starting Build Process ---" -Color "Cyan"

# Read Version
$VersionFile = Join-Path $ScriptRoot "unity\Assets\Resources\version.txt"
if (Test-Path $VersionFile) {
    $Version = Get-Content $VersionFile -Raw
    $Version = $Version.Trim()
    Write-Log "Version: $Version" -Color "Green"
} else {
    Write-Log "ERROR: Version file not found at $VersionFile" -Level "ERROR" -Color "Red"
    exit 1
}

# Default Build Flags
$BuildWindows = if ($env:BUILD_WINDOWS) { $env:BUILD_WINDOWS } else { "true" }
$BuildMac = if ($env:BUILD_MAC) { $env:BUILD_MAC } else { "true" }
$BuildLinux = if ($env:BUILD_LINUX) { $env:BUILD_LINUX } else { "true" }
$BuildAndroid = if ($env:BUILD_ANDROID) { $env:BUILD_ANDROID } else { "true" }

Write-Log "--- Build Configuration ---" -Color "Cyan"
Write-Log "BUILD_WINDOWS: $BuildWindows"
Write-Log "BUILD_MAC:     $BuildMac"
Write-Log "BUILD_LINUX:   $BuildLinux"
Write-Log "BUILD_ANDROID: $BuildAndroid"

# Steam Path
$SteamPath = if ($env:steampath) { $env:steampath } else { "${env:ProgramFiles(x86)}\Steam" }
# Test-PathOrExit $SteamPath "Steam" # Commented out in original batch, keeping logic similar but safer

# Android Environment
if ($BuildAndroid -eq "true") {
    Write-Log "Checking Android Environment..."
    
    $JdkHome = if ($env:JDK_HOME) { $env:JDK_HOME } elseif ($env:JAVA_HOME) { $env:JAVA_HOME } else { "$env:ProgramFiles\RedHat\java-1.8.0-openjdk-1.8.0.212-3" }
    Test-PathOrExit $JdkHome "JDK_HOME"
    $env:JDK_HOME = $JdkHome

    $AndroidSdkRoot = if ($env:ANDROID_SDK_ROOT) { $env:ANDROID_SDK_ROOT } else { "$env:LOCALAPPDATA\Android\Sdk" }
    Test-PathOrExit $AndroidSdkRoot "ANDROID_SDK_ROOT"
    $env:ANDROID_SDK_ROOT = $AndroidSdkRoot

    $AndroidBuildTools = if ($env:ANDROID_BUILD_TOOLS) { $env:ANDROID_BUILD_TOOLS } else { Join-Path $AndroidSdkRoot "build-tools\28.0.3" }
    Test-PathOrExit $AndroidBuildTools "ANDROID_BUILD_TOOLS"
    $env:ANDROID_BUILD_TOOLS = $AndroidBuildTools
    
    Write-Log "Android Environment OK."
}

# Unity Editor
$UnityEditorHome = if ($env:UNITY_EDITOR_HOME) { $env:UNITY_EDITOR_HOME } else { "$env:ProgramFiles\Unity\Editor" }
Test-PathOrExit $UnityEditorHome "UNITY_EDITOR_HOME"

# Visual Studio (MSBuild)
$VsPath = Get-ChildItem "$env:ProgramFiles\Microsoft Visual Studio\*" -Directory | Select-Object -First 1 -ExpandProperty FullName
if (-not $VsPath) {
    Write-Log "ERROR: Visual Studio installation not found." -Level "ERROR" -Color "Red"
    exit 1
}
Write-Log "Using Visual Studio path: $VsPath"

# Update PATH
$env:PATH = "$env:PATH;$JdkHome\bin;$UnityEditorHome;$env:ProgramFiles\7-Zip;$VsPath\Community\MSBuild\Current\Bin\;${env:ProgramFiles(x86)}\NSIS;$ScriptRoot\libraries\SetVersion\bin\Release;$AndroidBuildTools;$env:localappdata\NuGet;$env:ProgramFiles\Git\bin"

# -----------------------------------------------------------------------------
# Cleanup & Directory Structure
# -----------------------------------------------------------------------------

Write-Log "Cleaning up previous builds..."
$BuildDir = Join-Path $ScriptRoot "build"
$DirsToClean = @("android", "batch", "batchMac", "batchLinux", "macos", "win", "linux")

foreach ($Dir in $DirsToClean) {
    $Path = Join-Path $BuildDir $Dir
    if (Test-Path $Path) { Remove-Item $Path -Recurse -Force -ErrorAction SilentlyContinue }
}
if (Test-Path "$BuildDir\Valkyrie-android-$Version.apk") { Remove-Item "$BuildDir\Valkyrie-android-$Version.apk" -Force }

Write-Log "Creating build directories..."
New-Item -ItemType Directory -Force -Path "$BuildDir\batch\valkyrie_Data\content" | Out-Null
New-Item -ItemType Directory -Force -Path "$BuildDir\batchMac\Valkyrie.app\Contents" | Out-Null
New-Item -ItemType Directory -Force -Path "$BuildDir\batchLinux\valkyrie-linux-$Version\valkyrie_Data\content" | Out-Null
New-Item -ItemType Directory -Force -Path "$BuildDir\win" | Out-Null
New-Item -ItemType Directory -Force -Path "$BuildDir\macos\Valkyrie.app" | Out-Null
New-Item -ItemType Directory -Force -Path "$BuildDir\linux" | Out-Null

# -----------------------------------------------------------------------------
# Dependencies & Libraries
# -----------------------------------------------------------------------------

Write-Log "Downloading Android dependencies..."
$FsafUrl = "https://github.com/seinsinnes/FSAF/releases/latest/download/fsaf-release.aar"
$FsafOut = Join-Path $ScriptRoot "unity\Assets\Plugins\Android\fsaf-release.aar"
Invoke-WebRequest -Uri $FsafUrl -OutFile $FsafOut

Write-Log "Restoring NuGet packages..."
Invoke-CommandChecked { winget install -q Microsoft.NuGet -l "$env:localappdata\NuGet" --accept-source-agreements --accept-package-agreements } "Winget install failed"
Invoke-CommandChecked { nuget restore "$ScriptRoot\libraries\libraries.sln" } "NuGet restore failed"

Write-Log "Building libraries..."
Invoke-CommandChecked { msbuild "$ScriptRoot\libraries\libraries.sln" /nologo /p:Configuration="Release" /p:NoWarn=0108 } "MSBuild failed"
Write-Log "Libraries built."

Write-Log "Updating version info..."
Invoke-CommandChecked { & "SetVersion" $ScriptRoot } "SetVersion failed"

if (Test-Path "$ScriptRoot\unity\Assets\Plugins\UnityEngine.dll") {
    Remove-Item "$ScriptRoot\unity\Assets\Plugins\UnityEngine.dll" -Force
}

# -----------------------------------------------------------------------------
# Unity Builds
# -----------------------------------------------------------------------------

Write-Log "--- Starting Unity Builds ---" -Color "Cyan"
$UnityProject = Join-Path $ScriptRoot "unity"
$EditorLog = "$env:LOCALAPPDATA\Unity\Editor\Editor.log"

if ($BuildWindows -eq "true") {
    Write-Log "Building for Windows..."
    $WinBuildLog = "$BuildDir\Editor_valkyrie-windows.log"
    Invoke-CommandChecked {
        Unity -batchmode -quit -projectPath $UnityProject -buildWindowsPlayer "$BuildDir\win\valkyrie.exe"
    } "Unity Windows Build Failed"
    Copy-Item $EditorLog $WinBuildLog
    Write-Log "Windows build complete. Log: $WinBuildLog"
}

if ($BuildMac -eq "true") {
    Write-Log "Building for macOS..."
    $MacBuildLog = "$BuildDir\Editor_valkyrie-macos.log"
    Invoke-CommandChecked {
        Unity -batchmode -quit -projectPath $UnityProject -buildTarget OSXUniversal -buildOSXUniversalPlayer "$BuildDir\macos\Valkyrie.app"
    } "Unity macOS Build Failed"
    Copy-Item $EditorLog $MacBuildLog
    Write-Log "macOS build complete. Log: $MacBuildLog"
}

if ($BuildLinux -eq "true") {
    Write-Log "Building for Linux..."
    $LinuxBuildLog = "$BuildDir\Editor_valkyrie-linux.log"
    Invoke-CommandChecked {
        Unity -batchmode -quit -projectPath $UnityProject -buildLinux64Player "$BuildDir\linux\valkyrie"
    } "Unity Linux Build Failed"
    Copy-Item $EditorLog $LinuxBuildLog
    Write-Log "Linux build complete. Log: $LinuxBuildLog"
}

if ($BuildAndroid -eq "true") {
    Write-Log "Building for Android..."
    $AndroidBuildLog = "$BuildDir\Editor_valkyrie-android.log"
    $ApkPath = "$BuildDir\android\Valkyrie-android.apk"
    
    Invoke-CommandChecked {
        Unity -batchmode -quit -projectPath $UnityProject -executeMethod PerformBuild.CommandLineBuildAndroid +buildlocation $ApkPath
    } "Unity Android Build Failed" -LogFile $AndroidBuildLog
    
    Copy-Item $EditorLog $AndroidBuildLog
    Write-Log "Android build finished. Log: $AndroidBuildLog"

    Write-Log "Processing APK..."
    Invoke-CommandChecked { 7z -tzip d $ApkPath META-INF } "7z failed to delete META-INF"
    
    Write-Log "Signing APK..."
    $Keystore = Join-Path $UnityProject "user.keystore"
    Invoke-CommandChecked {
        jarsigner -keystore $Keystore -storepass valkyrie -keypass valkyrie $ApkPath com.bruce.valkyrie
    } "Jarsigner failed"
    Invoke-CommandChecked { jarsigner -verify -verbose -certs $ApkPath } "Jarsigner verify failed"
    
    Write-Log "Aligning APK..."
    $AlignedApk = "$BuildDir\Valkyrie-android-$Version.apk"
    Invoke-CommandChecked { zipalign -v 4 $ApkPath $AlignedApk } "Zipalign failed"
    Write-Log "Android post-processing complete."
}

# -----------------------------------------------------------------------------
# Packaging
# -----------------------------------------------------------------------------

Write-Log "Copying licenses..."
Copy-Item "LICENSE" "$BuildDir\batch\LICENSE.txt"
Copy-Item "NOTICE" "$BuildDir\batch\NOTICE.txt"
Copy-Item ".NET-Ogg-Vorbis-Encoder-LICENSE" "$BuildDir\batch\.NET-Ogg-Vorbis-Encoder-LICENSE.txt"
Copy-Item "dotnetzip-license.rtf" "$BuildDir\batch"

Write-Log "Duplicating licenses to macOS/Linux..."
Copy-Item "$BuildDir\batch\*" "$BuildDir\batchMac\Valkyrie.app" -Recurse -Force
Copy-Item "$BuildDir\batch\*" "$BuildDir\batchLinux\valkyrie-linux-$Version" -Recurse -Force

Write-Log "Packaging builds..."
Copy-Item "$BuildDir\win\*" "$BuildDir\batch" -Recurse -Force
Copy-Item "$BuildDir\macos\*" "$BuildDir\batchMac" -Recurse -Force
Copy-Item "$BuildDir\linux\*" "$BuildDir\batchLinux\valkyrie-linux-$Version" -Recurse -Force

Write-Log "Removing old packages..."
Remove-Item "$BuildDir\valkyrie-windows-$Version.exe" -ErrorAction SilentlyContinue
Remove-Item "$BuildDir\valkyrie-windows-$Version.zip" -ErrorAction SilentlyContinue
Remove-Item "$BuildDir\valkyrie-windows-$Version.7z" -ErrorAction SilentlyContinue
Remove-Item "$BuildDir\valkyrie-macos-$Version.tar.gz" -ErrorAction SilentlyContinue
Remove-Item "$BuildDir\valkyrie-linux-$Version.tar.gz" -ErrorAction SilentlyContinue

if ($BuildWindows -eq "true") {
    Write-Log "Zipping Windows build..."
    Invoke-CommandChecked { 7z a "$BuildDir\valkyrie-windows-$Version.7z" "$BuildDir\batch\*" -r } "7z Windows 7z failed"
    Invoke-CommandChecked { 7z a "$BuildDir\valkyrie-windows-$Version.zip" "$BuildDir\batch\*" -r } "7z Windows zip failed"
}

if ($BuildMac -eq "true") {
    Write-Log "Compressing macOS build..."
    $MacTar = "$BuildDir\batchMac\valkyrie-macos-$Version.tar"
    Invoke-CommandChecked { 7z a $MacTar "$BuildDir\batchMac\*" -r } "7z macOS tar failed"
    Invoke-CommandChecked { 7z a "$BuildDir\valkyrie-macos-$Version.tar.gz" $MacTar } "7z macOS gzip failed"
    Remove-Item $MacTar
}

if ($BuildLinux -eq "true") {
    Write-Log "Compressing Linux build..."
    $LinuxTar = "$BuildDir\batchLinux\valkyrie-linux-$Version.tar"
    Invoke-CommandChecked { 7z a $LinuxTar "$BuildDir\batchLinux\*" -r } "7z Linux tar failed"
    Invoke-CommandChecked { 7z a "$BuildDir\valkyrie-linux-$Version.tar.gz" $LinuxTar } "7z Linux gzip failed"
    Remove-Item $LinuxTar
}

if (Test-Path "android\test.apk") {
    Move-Item "android\test.apk" "valkyrie-android-$Version.apk" -Force
}

# -----------------------------------------------------------------------------
# Installer
# -----------------------------------------------------------------------------

Write-Log "Creating Installer..."
$LastDigit = $Version.Substring($Version.Length - 1)
$Num = $LastDigit -as [int]

if ($Num -eq $LastDigit) {
    Invoke-CommandChecked { makensis /DVERSION=$Version valkyrie.nsi } "Makensis Release failed"
    Write-Log "Installer created (Release)."
} else {
    Invoke-CommandChecked { makensis /DVERSION=$Version /DPRERELEASE valkyrie.nsi } "Makensis Pre-release failed"
    Write-Log "Installer created (Pre-release)."
}

Write-Log "--- Build Process Complete ---" -Color "Green"
