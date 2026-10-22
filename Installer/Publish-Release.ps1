<#
.SYNOPSIS
Zverejni instalator programu INISSTools ako vydanie v jeho repozitari na GitHube.

.DESCRIPTION
Pouziva ho workflow vydania (ToolsCore/.github/workflows/release-app.yml) po Build-Installer.ps1, s tokenom workflowu
(GITHUB_TOKEN) - ine tokeny netreba.

K vydaniu znacky vX.Y prilozi instalator a update.json - zaznam pre kontrolu aktualizacii (verzia, stranka vydania,
co je nove, instalator s adresou, SHA-256 a velkostou). Program ho cita z poslednej verzie repozitara
(.../releases/latest/download/update.json, adresa je v zostave - AppRelease.props).

Text vydania je text uz zalozeneho vydania (vytvorene na GitHube), inak sprava anotovanej znacky, inak zoznam
commitov od predchadzajucej znacky. Nove vydanie vznikne najprv ako koncept a zverejni sa az so subormi; za posledne
sa oznaci, len ak nie je starsie nez doterajsie posledne (oprava starsej verzie ho neprebije).

S -DryRun len vypise text vydania a update.json, na GitHub nic neposle (na lokalnu skusku).
#>
param(
    # nazov programu = kluc v update.json (GVDEditor, TableSimulator...)
    [Parameter(Mandatory = $true)] [string] $Program,
    # znacka vX.Y v repozitari programu
    [Parameter(Mandatory = $true)] [string] $Tag,
    # repozitar programu vlastnik/nazov
    [Parameter(Mandatory = $true)] [string] $Repo,
    # pracovna kopia repozitara programu (text znacky a commity)
    [Parameter(Mandatory = $true)] [string] $SourceDir,
    # installer.json z Build-Installer.ps1; update.json sa zapise vedla neho
    [Parameter(Mandatory = $true)] [string] $InstallerInfo,
    # pracovne kopie zavislosti, ktorych commity sa uvedu vo vydani
    [string[]] $Dependencies = @(),
    [switch] $DryRun
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
[Console]::OutputEncoding = [Text.UTF8Encoding]::new($false)
$OutputEncoding = [Text.UTF8Encoding]::new($false)

if ($Tag -notmatch '^v((0|[1-9]\d*)\.(0|[1-9]\d*)(\.(0|[1-9]\d*))?)$') { throw "Znacka '$Tag' nie je vydanie vX.Y alebo vX.Y.Z" }
$version = $Matches[1]
# -File odovzda zoznam ako jeden retazec s ciarkami
$Dependencies = @($Dependencies | ForEach-Object { $_ -split '[,\s]+' } | Where-Object { $_ })
$installer = Get-Content $InstallerInfo -Raw -Encoding UTF8 | ConvertFrom-Json
$token = $env:GITHUB_TOKEN
if (-not $DryRun -and -not $token) { throw 'Chyba token GITHUB_TOKEN' }

function Invoke-GitHub {
    param([string] $Method, [string] $Path, $Body = $null)
    $headers = @{ Authorization = "Bearer $token"; Accept = 'application/vnd.github+json'; 'X-GitHub-Api-Version' = '2022-11-28' }
    $request = @{ Method = $Method; Uri = "https://api.github.com/$Path"; Headers = $headers; UseBasicParsing = $true }
    if ($null -ne $Body) {
        $request.Body = [Text.Encoding]::UTF8.GetBytes(($Body | ConvertTo-Json -Depth 10 -Compress))
        $request.ContentType = 'application/json; charset=utf-8'
    }
    try {
        $response = Invoke-WebRequest @request
        return [pscustomobject]@{ Status = [int]$response.StatusCode; Data = $(if ($response.Content) { $response.Content | ConvertFrom-Json }) }
    }
    catch {
        $status = $_.Exception.Response.StatusCode
        if ($null -eq $status) { throw }
        return [pscustomobject]@{ Status = [int]$status; Data = $null }
    }
}

# Vydanie so znackou vratane konceptu - koncept z preruseneho behu sa pouzije znova.
function Find-Release {
    $response = Invoke-GitHub GET "repos/$Repo/releases?per_page=100"
    if ($response.Status -ne 200) { throw "Vydania $Repo sa nepodarilo nacitat ($($response.Status))" }
    return @($response.Data) | Where-Object { $null -ne $_ -and $_.tag_name -eq $Tag } | Select-Object -First 1
}

function Add-Asset($Release, [string] $Path, [string] $Name) {
    # rovnomenny subor z predchadzajuceho behu sa nahradi
    foreach ($asset in @($Release.assets)) {
        if ($asset.name -eq $Name) { Invoke-GitHub DELETE "repos/$Repo/releases/assets/$($asset.id)" | Out-Null }
    }
    $uploadUrl = $Release.upload_url -replace '\{.*\}$', ''
    $headers = @{ Authorization = "Bearer $token"; Accept = 'application/vnd.github+json'; 'X-GitHub-Api-Version' = '2022-11-28' }
    Invoke-RestMethod -Method Post -Uri "$uploadUrl`?name=$([Uri]::EscapeDataString($Name))" -Headers $headers `
        -ContentType 'application/octet-stream' -InFile $Path -UseBasicParsing | Out-Null
}

function ConvertTo-Version([string] $Text) {
    $parts = @($Text.TrimStart('v').Split('.')) + @('0', '0')
    return [version]($parts[0..2] -join '.')
}

# Posledne vydanie sa zmeni, len ak toto nie je starsie (oprava starsej verzie vznikne neskor, ale nema ho prebit).
function Get-MakeLatest {
    $latest = Invoke-GitHub GET "repos/$Repo/releases/latest"
    if ($latest.Status -eq 404) { return 'true' }
    if ($latest.Status -ne 200) { throw "Posledne vydanie $Repo sa nepodarilo nacitat ($($latest.Status))" }
    if ($latest.Data.tag_name -notmatch '^v\d') { return 'true' }
    if ((ConvertTo-Version $version) -ge (ConvertTo-Version $latest.Data.tag_name)) { return 'true' }
    return 'false'
}

function Get-TagNotes {
    $git = @('-C', $SourceDir)
    $type = (& git @git for-each-ref "refs/tags/$Tag" --format='%(objecttype)') | Select-Object -First 1
    if ($type -eq 'tag') {
        $text = (& git @git for-each-ref "refs/tags/$Tag" --format='%(contents)') -join "`n"
        $signature = $text.IndexOf('-----BEGIN ')
        if ($signature -ge 0) { $text = $text.Substring(0, $signature) }
        if ($text.Trim()) { return $text.Trim() }
    }
    # prve vydanie nema predchadzajucu znacku - git vtedy skonci chybou (Windows PowerShell z nej robi vynimku)
    $ErrorActionPreference = 'Continue'
    $previous = (& git @git describe --tags --abbrev=0 --match 'v[0-9]*' "$Tag^" 2>$null) | Select-Object -First 1
    $found = $LASTEXITCODE -eq 0 -and $previous
    $ErrorActionPreference = 'Stop'
    if (-not $found) { return 'First release.' }
    $commits = @(& git @git log --no-merges --format='- %s' "$previous..$Tag")
    if ($commits.Count -eq 0) { return "No changes since $previous." }
    return "Changes since ${previous}:`n`n" + ($commits -join "`n")
}

function Get-BuiltFrom {
    $dirs = @($SourceDir) + $Dependencies
    $items = foreach ($dir in $dirs) {
        $sha = (& git -C $dir rev-parse --short=10 HEAD) | Select-Object -First 1
        '`{0}` {1}' -f (Split-Path $dir -Leaf), $sha
    }
    return 'Built from ' + ($items -join ', ') + '.'
}

# Text vydania: vydanie zalozene na GitHube ma prednost pred znackou.
$release = if ($DryRun) { $null } else { Find-Release }
$notes = if ($null -ne $release -and $release.body -and $release.body.Trim()) { $release.body.Trim() } else { Get-TagNotes }
$body = $notes + "`n`n---`n" + (Get-BuiltFrom)
$title = "$Program $version"
Write-Host "Vydanie $Repo $Tag - $title"
Write-Host $body

# Zaznam pre kontrolu aktualizacii - rovnaky tvar ako index (ToolsCore.Iniss UpdateIndex), len s tymto programom.
$update = [ordered]@{
    schema   = 1
    programs = [ordered]@{
        $Program = [ordered]@{
            version   = $version
            published = [DateTime]::UtcNow.ToString('yyyy-MM-ddTHH:mm:ssZ', [Globalization.CultureInfo]::InvariantCulture)
            page      = "https://github.com/$Repo/releases/tag/$Tag"
            notes     = $notes
            installer = [ordered]@{
                url    = "https://github.com/$Repo/releases/download/$Tag/$($installer.name)"
                sha256 = $installer.sha256
                size   = [long]$installer.size
            }
        }
    }
}
$updatePath = Join-Path (Split-Path -Parent (Resolve-Path $InstallerInfo)) 'update.json'
[IO.File]::WriteAllText($updatePath, ($update | ConvertTo-Json -Depth 10) + "`n", [Text.UTF8Encoding]::new($false))
Write-Host (Get-Content $updatePath -Raw -Encoding UTF8)
if ($DryRun) { return }

if ($null -eq $release) {
    $created = Invoke-GitHub POST "repos/$Repo/releases" @{ tag_name = $Tag; name = $title; body = $body; draft = $true }
    if ($created.Status -ne 201) { throw "Vydanie $Repo $Tag sa nepodarilo zalozit ($($created.Status))" }
    $release = $created.Data
}
Add-Asset $release $installer.path $installer.name
Add-Asset $release $updatePath 'update.json'
if ($release.draft) {
    $published = Invoke-GitHub PATCH "repos/$Repo/releases/$($release.id)" @{ draft = $false; make_latest = (Get-MakeLatest) }
    if ($published.Status -ne 200) { throw "Vydanie $Repo $Tag sa nepodarilo zverejnit ($($published.Status))" }
}
Write-Host "Vydanie zverejnene: https://github.com/$Repo/releases/tag/$Tag"
