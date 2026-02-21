param (
    [Parameter(Mandatory = $true)]
    [string]$RepoUrl,

    [Parameter(Mandatory = $true)]
    [string]$IssueNumber,

    [Parameter(Mandatory = $true)]
    [string]$RunnerRepo
)

# Extract owner and repo from URL
if ($RepoUrl -match "https://github\.com/([^/]+)/([^/]+)") {
    $Owner = $Matches[1]
    $Repo = $Matches[2]
}
else {
    Write-Error "Invalid GitHub Repository URL: $RepoUrl"
    exit 1
}

$Headers = @{
    "Authorization" = "token $env:GH_TOKEN"
    "Accept"        = "application/vnd.github.v3+json"
    "User-Agent"    = "Valkyrie-Actions"
}

$Results = @()

# ------------------------------------------------------------------
# Repository-level checks
# ------------------------------------------------------------------
if ($Repo -match "-") {
    $Results += "- ⚠️ repository name contains a hyphen; consider using dots instead"
}

# Fetch the full tree
try {
    $TreeUrl = "https://api.github.com/repos/$Owner/$Repo/git/trees/HEAD?recursive=1"
    $TreeResponse = Invoke-RestMethod -Uri $TreeUrl -Headers $Headers -Method Get
    
    # In PowerShell, $TreeResponse.tree is an array of objects
    $Paths = @($TreeResponse.tree | Select-Object -ExpandProperty path)
}
catch {
    $Results += "- ❌ error accessing repository tree: $_"
    $Paths = @()
}

if ($Paths.Count -gt 0) {
    # ------------------------------------------------------------------
    # Generic presence checks
    # ------------------------------------------------------------------
    $Exts = @("ini", "valkyrie")
    foreach ($Ext in $Exts) {
        $Matches = @($Paths | Where-Object { $_.ToLower().EndsWith(".$Ext") })
        if ($Matches.Count -eq 0) {
            $Results += "- ❌ no *.$Ext file found"
        }
        else {
            $Results += "- ✅ $($Matches.Count) *.$Ext file(s) found:`n" + ($Matches -join "`n")
        }
    }

    # Check for image file (jpg or png)
    $ImageMatches = @($Paths | Where-Object { $_.ToLower().EndsWith(".jpg") -or $_.ToLower().EndsWith(".png") })
    if ($ImageMatches.Count -eq 0) {
        $Results += "- ❌ no *.jpg or *.png image file found"
    }
    else {
        $Results += "- ✅ $($ImageMatches.Count) image file(s) (*.jpg, *.png) found:`n" + ($ImageMatches -join "`n")
    }

    # ------------------------------------------------------------------
    # Detailed checks specific to .ini files
    # ------------------------------------------------------------------
    $IniPaths = @($Paths | Where-Object { $_.ToLower().EndsWith(".ini") })
    foreach ($IniPath in $IniPaths) {
        # Standard string matching for paths since it uses forward slashes in Git Tree
        $Base = [System.IO.Path]::GetFileName($IniPath)
        
        if ($Base -match "(?i)EditorScenario|EditorScenariusz") {
            $Results += "- ⚠️ ini file '$IniPath' has a generic filename; please rename it"
        }

        try {
            $FileUrl = "https://api.github.com/repos/$Owner/$Repo/contents/$IniPath"
            $FileResponse = Invoke-RestMethod -Uri $FileUrl -Headers $Headers -Method Get
            $Content = [System.Text.Encoding]::UTF8.GetString([Convert]::FromBase64String($FileResponse.content))

            if ($Content -match "(?im)^Language\s*=\s*(.*)$") {
                $Lang = $Matches[1].Trim()
                if ($Lang.ToLower() -eq "english") {
                    $Results += "- ⚠️ ini file '$IniPath' has default language set to English"
                }
            }
        }
        catch {
            $Results += "- ⚠️ unable to read ini file '$IniPath': $_"
        }
    }

    # ------------------------------------------------------------------
    # Filename checks for .valkyrie
    # ------------------------------------------------------------------
    $ValkPaths = @($Paths | Where-Object { $_.ToLower().EndsWith(".valkyrie") })
    foreach ($ValkPath in $ValkPaths) {
        $Base = [System.IO.Path]::GetFileName($ValkPath)
        if ($Base -match "(?i)EditorScenario|EditorScenariusz") {
            $Results += "- ⚠️ .valkyrie file '$ValkPath' has a generic filename; please rename it"
        }
    }
}

# ------------------------------------------------------------------
# Comment results on issue
# ------------------------------------------------------------------
$CommentBody = "**Scenario repository validation**`n`nRepository: $RepoUrl`n`n" + ($Results -join "`n")
$CommentUrl = "https://api.github.com/repos/$RunnerRepo/issues/$IssueNumber/comments"

$BodyJson = @{
    body = $CommentBody
} | ConvertTo-Json

try {
    Invoke-RestMethod -Uri $CommentUrl -Headers $Headers -Method Post -Body $BodyJson -ContentType "application/json" | Out-Null
    Write-Host "Successfully posted comment to issue #$IssueNumber"
}
catch {
    Write-Error "Failed to post comment: $_"
}
