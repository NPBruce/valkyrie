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

function Build-Unity {
    param (
        [string]$PlatformName,
        [string]$OutputPath,
        [string]$LogFile,
        [string]$BuildTarget, # Optional: For -buildTarget
        [string]$BuildMethod, # Optional: For -executeMethod
        [string]$BuildPlayerOption, # Optional: e.g. -buildWindowsPlayer
        [string]$UnityProject,
        [string]$UnityExe
    )

    Write-Log "Building for $PlatformName..."

    # Construct arguments dynamically
    $Args = @("-batchmode", "-quit", "-projectPath", $UnityProject)

    if (![string]::IsNullOrEmpty($BuildTarget)) {
        $Args += "-buildTarget"
        $Args += $BuildTarget
    }

    if (![string]::IsNullOrEmpty($BuildPlayerOption)) {
        $Args += $BuildPlayerOption
        $Args += $OutputPath
    }

    if (![string]::IsNullOrEmpty($BuildMethod)) {
        $Args += "-executeMethod"
        $Args += $BuildMethod
        # Android specific arg for the method
        if ($PlatformName -eq "Android") {
            $Args += "+buildlocation"
            $Args += $OutputPath
        }
    }

    # Add log file argument
    $Args += "-logFile"
    $Args += $LogFile

    Write-Log "Running Unity with args: $Args"

    $UnityProcess = Start-Process -FilePath $UnityExe -ArgumentList $Args -NoNewWindow -PassThru
    $UnityProcess.WaitForExit()

    if ($UnityProcess.ExitCode -ne 0) {
        Write-Log "Unity $PlatformName Build Failed (Exit Code: $($UnityProcess.ExitCode))" -Level "ERROR" -Color "Red"
        if (Test-Path $LogFile) {
            Write-Log "Dumping log file: $LogFile" -Level "ERROR" -Color "Yellow"
            Get-Content $LogFile | Write-Host
        }
        else {
            Write-Log "Log file not found at $LogFile" -Level "ERROR" -Color "Red"
        }
        exit 1
    }

    Write-Log "$PlatformName build completed. Log: $LogFile"
}

# -----------------------------------------------------------------------------
# Sub-Methods
# -----------------------------------------------------------------------------

function Get-BuildVersion {
    param ([string]$ScriptRoot)
    $VersionFile = Join-Path $ScriptRoot "unity\Assets\Resources\version.txt"
    if (Test-Path $VersionFile) {
        $Version = Get-Content $VersionFile -Raw
        return $Version.Trim()
    }
    else {
        Write-Log "ERROR: Version file not found at $VersionFile" -Level "ERROR" -Color "Red"
        exit 1
    }
}

function Get-BuildConfiguration {
    $Config = @{
        BuildWindows = if ($env:BUILD_WINDOWS) { $env:BUILD_WINDOWS } else { "true" }
        BuildMac     = if ($env:BUILD_MAC) { $env:BUILD_MAC } else { "true" }
        BuildLinux   = if ($env:BUILD_LINUX) { $env:BUILD_LINUX } else { "true" }
        BuildAndroid = if ($env:BUILD_ANDROID) { $env:BUILD_ANDROID } else { "true" }
    }

    Write-Log "--- Build Configuration ---" -Color "Cyan"
    $Config.Keys | ForEach-Object { Write-Log ("{0}: {1}" -f $_, $Config[$_]) }

    return $Config
}

function Setup-Environment {
    param ([hashtable]$Config, [string]$ScriptRoot)

    # Android Environment
    if ($Config.BuildAndroid -eq "true") {
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

    $UnityExe = Join-Path $UnityEditorHome "Unity.exe"
    if (-not (Test-Path $UnityExe)) {
        Write-Log "ERROR: Unity.exe not found at $UnityExe" -Level "ERROR" -Color "Red"
        exit 1
    }
    Write-Log "Unity Executable found: $UnityExe"

    # Visual Studio (MSBuild)
    $VsPath = Get-ChildItem "$env:ProgramFiles\Microsoft Visual Studio\*" -Directory | Select-Object -First 1 -ExpandProperty FullName
    if (-not $VsPath) {
        Write-Log "ERROR: Visual Studio installation not found." -Level "ERROR" -Color "Red"
        exit 1
    }
    Write-Log "Using Visual Studio path: $VsPath"

    # SetVersion Path
    $SetVersionPath = Join-Path $ScriptRoot "libraries\SetVersion\bin\Release"
    Write-Log "SetVersion Path: $SetVersionPath"

    # Update PATH
    $env:PATH = "$env:PATH;$JdkHome\bin;$UnityEditorHome;$env:ProgramFiles\7-Zip;$VsPath\Community\MSBuild\Current\Bin\;${env:ProgramFiles(x86)}\NSIS;$SetVersionPath;$AndroidBuildTools;$env:localappdata\NuGet;$env:ProgramFiles\Git\bin"
    Write-Log "Updated PATH: $env:PATH"

    return $UnityExe
}

function Initialize-BuildDirectories {
    param ([string]$ScriptRoot, [string]$Version)

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

    return $BuildDir
}

function Install-Dependencies {
    param ([string]$ScriptRoot)

    Write-Log "Restoring NuGet packages..."
    Invoke-CommandChecked { winget install -q Microsoft.NuGet -l "$env:localappdata\NuGet" --accept-source-agreements --accept-package-agreements } "Winget install failed"
    Invoke-CommandChecked { nuget restore "$ScriptRoot\libraries\libraries.sln" } "NuGet restore failed"
}

function Build-Libraries {
    param ([string]$ScriptRoot)

    Write-Log "Building libraries..."
    Invoke-CommandChecked { msbuild "$ScriptRoot\libraries\libraries.sln" /nologo /p:Configuration="Release" /p:NoWarn=0108 } "MSBuild failed"
    Write-Log "Libraries built."

    Write-Log "Updating version info..."
    Invoke-CommandChecked { & "SetVersion" $ScriptRoot } "SetVersion failed"

    if (Test-Path "$ScriptRoot\unity\Assets\Plugins\UnityEngine.dll") {
        Remove-Item "$ScriptRoot\unity\Assets\Plugins\UnityEngine.dll" -Force
    }
}

function Package-Artifacts {
    param ([string]$BuildDir, [string]$Version, [hashtable]$Config)

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

    if ($Config.BuildWindows -eq "true") {
        Write-Log "Zipping Windows build..."
        Invoke-CommandChecked { 7z a "$BuildDir\valkyrie-windows-$Version.7z" "$BuildDir\batch\*" -r } "7z Windows 7z failed"
        Invoke-CommandChecked { 7z a "$BuildDir\valkyrie-windows-$Version.zip" "$BuildDir\batch\*" -r } "7z Windows zip failed"
    }

    if ($Config.BuildMac -eq "true") {
        Write-Log "Compressing macOS build..."
        $MacTar = "$BuildDir\batchMac\valkyrie-macos-$Version.tar"
        Invoke-CommandChecked { 7z a $MacTar "$BuildDir\batchMac\*" -r } "7z macOS tar failed"
        Invoke-CommandChecked { 7z a "$BuildDir\valkyrie-macos-$Version.tar.gz" $MacTar } "7z macOS gzip failed"
        Remove-Item $MacTar
    }

    if ($Config.BuildLinux -eq "true") {
        Write-Log "Compressing Linux build..."
        $LinuxTar = "$BuildDir\batchLinux\valkyrie-linux-$Version.tar"
        Invoke-CommandChecked { 7z a $LinuxTar "$BuildDir\batchLinux\*" -r } "7z Linux tar failed"
        Invoke-CommandChecked { 7z a "$BuildDir\valkyrie-linux-$Version.tar.gz" $LinuxTar } "7z Linux gzip failed"
        Remove-Item $LinuxTar
    }


}

function Create-Installer {
    param ([string]$Version)

    Write-Log "Creating Installer..."
    $LastDigit = $Version.Substring($Version.Length - 1)
    $Num = $LastDigit -as [int]

    if ($Num -eq $LastDigit) {
        Invoke-CommandChecked { makensis /DVERSION=$Version valkyrie.nsi } "Makensis Release failed"
        Write-Log "Installer created (Release)."
    }
    else {
        Invoke-CommandChecked { makensis /DVERSION=$Version /DPRERELEASE valkyrie.nsi } "Makensis Pre-release failed"
        Write-Log "Installer created (Pre-release)."
    }
}

# -----------------------------------------------------------------------------
# Main Execution Flow
# -----------------------------------------------------------------------------

Write-Log "--- Starting Build Process ---" -Color "Cyan"

$Version = Get-BuildVersion -ScriptRoot $ScriptRoot
Write-Log "Internal Version: $Version" -Color "Green"

if ($env:PACKAGE_VERSION) {
    $OutputVersion = $env:PACKAGE_VERSION
    Write-Log "Output Version (Overridden): $OutputVersion" -Color "Magenta"
}
else {
    $OutputVersion = $Version
    Write-Log "Output Version: $OutputVersion" -Color "Green"
}

$Config = Get-BuildConfiguration

$UnityExe = Setup-Environment -Config $Config -ScriptRoot $ScriptRoot

$BuildDir = Initialize-BuildDirectories -ScriptRoot $ScriptRoot -Version $Version

Install-Dependencies -ScriptRoot $ScriptRoot

Build-Libraries -ScriptRoot $ScriptRoot

# Unity Builds
Write-Log "--- Starting Unity Builds ---" -Color "Cyan"
$UnityProject = Join-Path $ScriptRoot "unity"
$UnityProject = Join-Path $ScriptRoot "unity"

if ($Config.BuildWindows -eq "true") {
    Build-Unity -PlatformName "Windows" `
        -OutputPath "$BuildDir\win\valkyrie.exe" `
        -LogFile "$BuildDir\Editor_valkyrie-windows.log" `
        -BuildPlayerOption "-buildWindowsPlayer" `
        -UnityProject $UnityProject `
        -UnityExe $UnityExe
}

if ($Config.BuildMac -eq "true") {
    Build-Unity -PlatformName "macOS" `
        -OutputPath "$BuildDir\macos\Valkyrie.app" `
        -LogFile "$BuildDir\Editor_valkyrie-macos.log" `
        -BuildTarget "OSXUniversal" `
        -BuildPlayerOption "-buildOSXUniversalPlayer" `
        -UnityProject $UnityProject `
        -UnityExe $UnityExe
}

if ($Config.BuildLinux -eq "true") {
    Build-Unity -PlatformName "Linux" `
        -OutputPath "$BuildDir\linux\valkyrie" `
        -LogFile "$BuildDir\Editor_valkyrie-linux.log" `
        -BuildPlayerOption "-buildLinux64Player" `
        -UnityProject $UnityProject `
        -UnityExe $UnityExe
}

if ($Config.BuildAndroid -eq "true") {
    $ApkPath = "$BuildDir\android\Valkyrie-android.apk"

    Build-Unity -PlatformName "Android" `
        -OutputPath $ApkPath `
        -LogFile "$BuildDir\Editor_valkyrie-android.log" `
        -BuildMethod "PerformBuild.CommandLineBuildAndroid" `
        -UnityProject $UnityProject `
        -UnityExe $UnityExe

    Write-Log "Processing APK..."
    Invoke-CommandChecked { 7z -tzip d $ApkPath META-INF } "7z failed to delete META-INF"

    Write-Log "Signing APK..."
    $Keystore = Join-Path $UnityProject "user.keystore"
    Invoke-CommandChecked {
        jarsigner -keystore $Keystore -storepass valkyrie -keypass valkyrie $ApkPath com.bruce.valkyrie
    } "Jarsigner failed"
    Invoke-CommandChecked { jarsigner -verify -verbose -certs $ApkPath } "Jarsigner verify failed"

    Write-Log "Aligning APK..."
    $AlignedApk = "$BuildDir\Valkyrie-android-$OutputVersion.apk"
    Invoke-CommandChecked { zipalign -v 4 $ApkPath $AlignedApk } "Zipalign failed"
    Write-Log "Android post-processing complete."
}

Package-Artifacts -BuildDir $BuildDir -Version $OutputVersion -Config $Config

Create-Installer -Version $OutputVersion

Write-Log "--- Build Process Complete ---" -Color "Green"
