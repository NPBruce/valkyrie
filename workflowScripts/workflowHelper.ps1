function Get-UnityVersion {
    $projectVersionFile = "$env:GITHUB_WORKSPACE/unity/ProjectSettings/ProjectVersion.txt"
    $unityVersion = Get-Content $projectVersionFile | Select-String "m_EditorVersion:" | ForEach-Object { $_.ToString().Split(":")[1].Trim() }
    echo "UNITY_VERSION=$unityVersion" | Out-File -FilePath $env:GITHUB_ENV -Append
    Write-Host "Unity Version: $unityVersion"
}

function Get-ReleaseVersion {
    $versionFile = "$env:GITHUB_WORKSPACE/unity/Assets/Resources/version.txt"
    $version = Get-Content $versionFile
    $customName = $env:CUSTOM_RELEASE_NAME

    if (-not [string]::IsNullOrWhiteSpace($customName)) {
        if ($customName -match '^[a-zA-Z0-9]+$') {
            Write-Host "Using Custom Release Name: $customName"
            echo "RELEASE_NAME=$customName" | Out-File -FilePath $env:GITHUB_ENV -Append
        }
        else {
            Write-Error "Custom Release Name '$customName' is invalid. Must be alphanumeric only."
            exit 1
        }
    }
    else {
        Write-Host "Using Version from file: $version"
        echo "RELEASE_NAME=$version" | Out-File -FilePath $env:GITHUB_ENV -Append
    }
    # Build_Version is kept for backward compatibility if other scripts use it, 
    # but we rely on RELEASE_NAME for artifacts/tags now.
    echo "Build_Version=$version" | Out-File -FilePath $env:GITHUB_ENV -Append
}

function Run-UnityTests {
    $UnityExe = "C:/Program Files/Unity/Editor/Unity.exe"
    $LogFile = "$env:GITHUB_WORKSPACE/test-results.log"
    $ResultsFile = "$env:GITHUB_WORKSPACE/test-results.xml"

    $argList = @(
        "-batchmode",
        "-nographics",
        "-projectPath", "$env:GITHUB_WORKSPACE/unity",
        "-runTests",
        "-testPlatform", "EditMode",
        "-testResults", "$ResultsFile",
        "-logFile", "$LogFile"
    )

    $process = Start-Process -FilePath $UnityExe -ArgumentList $argList -PassThru -NoNewWindow
    $process.WaitForExit()
    $exitCode = $process.ExitCode

    if (Test-Path $ResultsFile) {
        [xml]$xml = Get-Content $ResultsFile
        $failedNodes = $xml.SelectNodes("//test-case[@result='Failed']")
        if ($failedNodes.Count -gt 0) {
            Write-Host "::error::Found $($failedNodes.Count) failed tests!"
            foreach ($node in $failedNodes) {
                $testName = $node.fullname
                $msg = $node.failure.message
                $stacktrace = $node.failure.'stack-trace'
                
                Write-Host "::group::$testName"
                Write-Host "::error file=$ResultsFile,title=Test Failed::$msg"
                Write-Host $stacktrace
                Write-Host "::endgroup::"
            }
            exit 1 # Fail the job
        }
        else {
            Write-Host "All tests passed."
        }
    }
    else {
        Write-Host "::error::Test results file not found at $ResultsFile"
        if (Test-Path $LogFile) {
            Get-Content $LogFile -Tail 50
        }
        exit 1
    }

    if ($exitCode -ne 0) {
        # If unity exited with error but we didn't catch failed tests above (e.g. crash)
        exit $exitCode
    }
}

function Remove-ConflictingDLL {
    if (Test-Path "unity/Assets/Plugins/UnityEngine.dll") {
        Remove-Item "unity/Assets/Plugins/UnityEngine.dll" -Force
    }
}
