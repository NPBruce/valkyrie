param (
    [Parameter(Mandatory = $true)]
    [string]$RepoUrl,

    [Parameter(Mandatory = $false)]
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
    "Accept"     = "application/vnd.github.v3+json"
    "User-Agent" = "Valkyrie-Actions"
}
if ($env:GH_TOKEN) {
    $Headers["Authorization"] = "token $env:GH_TOKEN"
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
    $IsContentPack = @($Paths | Where-Object { $_.ToLower().EndsWith(".valkyriecontentpack") -or [System.IO.Path]::GetFileName($_).ToLower() -eq "content_pack.ini" }).Count -gt 0
    
    if ($IsContentPack) {
        $Exts = @("ini", "valkyriecontentpack")
        $TargetManifest = "contentPacksManifest.ini"
        $TargetTag = "Content Pack"
    }
    else {
        $Exts = @("ini", "valkyrie")
        $TargetManifest = "manifest.ini"
        $TargetTag = "Scenario"
    }

    foreach ($Ext in $Exts) {
        $MatchedFiles = @($Paths | Where-Object { $_.ToLower().EndsWith(".$Ext") })
        if ($MatchedFiles.Count -eq 0) {
            $Results += "- ❌ no *.$Ext file found"
        }
        else {
            $Results += "- ✅ $($MatchedFiles.Count) *.$Ext file(s) found:`n" + ($MatchedFiles -join "`n")
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

            if ($Content -match "(?im)^\s*\[([^\]]+)\]") {
                # Check Language
                if ($Content -match "(?im)^(?:defaultlanguage|Language)\s*=\s*(.*)$") {
                    $Lang = $Matches[1].Trim()
                    if ($Lang.ToLower() -eq "english") {
                        $Results += "- ℹ️ ini file '$IniPath' has default language set to English (Make sure this is correct for your scenario)"
                    }
                    else {
                        $Results += "- ✅ Default language set to '$Lang'"
                    }
                }
                else {
                    $Results += "- ⚠️ ini file '$IniPath' is missing a 'defaultlanguage' property."
                }

                if ($IsContentPack) {
                    if (-not ($Content -match "(?im)^name\.$Lang\s*=\s*.+$")) {
                        $Results += "- ❌ Content pack '$IniPath' is missing 'name.$Lang' property required for UI display."
                    }
                    else {
                        $Results += "- ✅ Content pack 'name.$Lang' is defined."
                    }
                    if (-not ($Content -match "(?im)^description\.$Lang\s*=\s*.+$")) {
                        $Results += "- ❌ Content pack '$IniPath' is missing 'description.$Lang' property required for UI display."
                    }
                    else {
                        $Results += "- ✅ Content pack 'description.$Lang' is defined."
                    }
                }
                else {
                    # Check Packs
                    if ($Content -match "(?im)^packs\s*=\s*(.*)$") {
                        $Packs = $Matches[1].Trim()
                        $Results += "- ℹ️ Required Expansions (packs) defined in '$IniPath': **$Packs**"
                    }
                    else {
                        $Results += "- ℹ️ No 'packs' property defined in '$IniPath' (Base game only)"
                    }
                }

                # Check Image
                if ($Content -match "(?im)^image\s*=\s*(.*)$") {
                    $Image = $Matches[1].Trim().Replace('"', '')
                    $ImageMatch = @($Paths | Where-Object { $_ -eq $Image -or $_.EndsWith("/$Image") -or $_.EndsWith("\$Image") })
                    if ($ImageMatch.Count -gt 0) {
                        $Results += "- ✅ Thumbnail image '$Image' found in repository."
                    }
                    else {
                        $Results += "- ⚠️ Thumbnail image '$Image' defined in '$IniPath', but not found in repository."
                    }
                }
                else {
                    $Results += "- ℹ️ No thumbnail 'image' defined in '$IniPath'. Consider adding one to improve scenario visibility."
                }

                # Check Type and Duplicates
                $Type = ""
                if ($Content -match "(?im)^type\s*=\s*(.*)$") {
                    $Type = $Matches[1].Trim()
                    $ValidTypes = if ($IsContentPack) { @("MoMCustom", "D2ECustom") } else { @("MoM", "D2E") }
                    
                    if ($Type -notin $ValidTypes) {
                        $Results += "- ❌ Invalid 'type=$Type' in '$IniPath'. Must be one of: " + ($ValidTypes -join ", ") + "."
                        $Type = ""
                    }
                    else {
                        # Map custom types back to base folder names for manifest lookup
                        $ManifestDirType = if ($Type -eq "MoMCustom") { "MoM" } elseif ($Type -eq "D2ECustom") { "D2E" } else { $Type }
                        $Results += "- ✅ $TargetTag type identified as '$Type'"
                    }
                }
                else {
                    $Results += "- ❌ Missing 'type' property in '$IniPath'. Must define 'type'."
                }

                if ($Type) {
                    $ScenarioId = ""
                    if ($IsContentPack) {
                        $ValkPathsTmp = @($Paths | Where-Object { $_.ToLower().EndsWith(".valkyriecontentpack") })
                        if ($ValkPathsTmp.Count -gt 0) {
                            $ValkFile = $ValkPathsTmp[0]
                            $ValkBase = [System.IO.Path]::GetFileName($ValkFile)
                            $ScenarioId = $ValkBase.Substring(0, $ValkBase.Length - 20)
                        }
                        else {
                            $BaseNameTmp = [System.IO.Path]::GetFileName($IniPath)
                            $ScenarioId = $BaseNameTmp.Substring(0, $BaseNameTmp.Length - 4)
                        }
                    }
                    else {
                        $ValkPathsTmp = @($Paths | Where-Object { $_.ToLower().EndsWith(".valkyrie") })
                        if ($ValkPathsTmp.Count -gt 0) {
                            $ValkFile = $ValkPathsTmp[0]
                            $ValkBase = [System.IO.Path]::GetFileName($ValkFile)
                            $ScenarioId = $ValkBase.Substring(0, $ValkBase.Length - 9)
                        }
                        else {
                            $BaseNameTmp = [System.IO.Path]::GetFileName($IniPath)
                            $ScenarioId = $BaseNameTmp.Substring(0, $BaseNameTmp.Length - 4)
                        }
                    }

                    try {
                        $ManifestUrl = "https://raw.githubusercontent.com/NPBruce/valkyrie-store/master/$ManifestDirType/$TargetManifest"
                        $StoreManifest = Invoke-RestMethod -Uri $ManifestUrl -Method Get
                        $SafeId = [regex]::Escape($ScenarioId)
                        if ($StoreManifest -match "(?im)^\[$SafeId\]") {
                            $Results += "- ❌ $TargetTag ID '$ScenarioId' already exists in the valkyrie-store $ManifestDirType $TargetManifest. Please rename your files to ensure a unique ID."
                        }
                        else {
                            $Results += "- ✅ $TargetTag ID '$ScenarioId' is unique for $Type."
                            
                            $DefaultBranch = ""
                            try {
                                $RepoInfoUrl = "https://api.github.com/repos/$Owner/$Repo"
                                $RepoInfoResponse = Invoke-RestMethod -Uri $RepoInfoUrl -Headers $Headers -Method Get
                                $DefaultBranch = $RepoInfoResponse.default_branch
                            }
                            catch {
                                $DefaultBranch = "master"
                            }
                            
                            $Results += "`n---`n💡 **For Valkyrie Admins:** You can copy the following block to add this $TargetTag for Valkyrie $TargetTag repository:`n"
                            $Results += "``````ini`n[$ScenarioId]`nexternal=https://raw.githubusercontent.com/$Owner/$Repo/$DefaultBranch/`n```````n"
                            $Results += "[👉 Quick Edit $ManifestDirType $TargetManifest](https://github.com/NPBruce/valkyrie-store/edit/master/$ManifestDirType/$TargetManifest)`n---"
                        }
                    }
                    catch {
                        $Results += "- ⚠️ Could not fetch $ManifestDirType $TargetManifest from valkyrie-store to check for duplicate ID: $_"
                    }
                }
            }
            else {
                if ($Content -match "(?im)^Language\s*=\s*(.*)$") {
                    $Lang = $Matches[1].Trim()
                    if ($Lang.ToLower() -eq "english") {
                        $Results += "- ⚠️ ini file '$IniPath' has default language set to English"
                    }
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
    $ValkPaths = @($Paths | Where-Object { $_.ToLower().EndsWith(".valkyrie") -or $_.ToLower().EndsWith(".valkyriecontentpack") })
    foreach ($ValkPath in $ValkPaths) {
        $Base = [System.IO.Path]::GetFileName($ValkPath)
        if ($Base -match "(?i)EditorScenario|EditorScenariusz") {
            $Results += "- ⚠️ Container file '$ValkPath' has a generic filename; please rename it"
        }
    }
}

# ------------------------------------------------------------------
# Output results
# ------------------------------------------------------------------
$CommentBody = "**Repository validation**`n`nRepository: $RepoUrl`n`n" + ($Results -join "`n")

# Always write to the GitHub Actions Console
Write-Host "========================================="
Write-Host "Validation Results for $RepoUrl"
Write-Host "========================================="
Write-Host $CommentBody
Write-Host "========================================="

# If an Issue Number was provided (i.e. not a manual dispatch), post a comment
if ($IssueNumber) {
    $CommentUrl = "https://api.github.com/repos/$RunnerRepo/issues/$IssueNumber/comments"

    $BodyJson = @{
        body = $CommentBody
    } | ConvertTo-Json

    try {
        Invoke-RestMethod -Uri $CommentUrl -Headers $Headers -Method Post -Body $BodyJson -ContentType "application/json" | Out-Null
        Write-Host "Successfully posted comment to issue #$IssueNumber"
    }
    catch {
        Write-Error "Failed to post comment to #$IssueNumber : $_"
    }
}
else {
    Write-Host "No IssueNumber provided. Skipping GitHub issue comment."
}
