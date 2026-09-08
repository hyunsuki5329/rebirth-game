param(
    [ValidateSet('EditMode','PlayMode','Method')][string]$Mode = 'EditMode',
    [string]$Tag = 'latest',
    [string]$Method = ''
)
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
$validationRoot = Join-Path $projectRoot '.validation'
$resultRoot = Join-Path $projectRoot 'TestResults'
New-Item -ItemType Directory -Force $validationRoot, $resultRoot | Out-Null
# Establish source GUIDs before either Editor imports newly written game assets.
$gameAssets = Join-Path $projectRoot 'Assets/_Game'
foreach ($asset in @((Get-Item $gameAssets)) + @(Get-ChildItem $gameAssets -Recurse)) {
    if ($asset.Name.EndsWith('.meta')) { continue }
    $metaPath = $asset.FullName + '.meta'
    if (Test-Path -LiteralPath $metaPath) { continue }
    $meta = "fileFormatVersion: 2`nguid: $([guid]::NewGuid().ToString('N'))`n"
    if ($asset.PSIsContainer) { $meta += "folderAsset: yes`nDefaultImporter:`n  externalObjects: {}`n  userData: `n  assetBundleName: `n  assetBundleVariant: `n" }
    try {
        $stream = [System.IO.File]::Open($metaPath, [System.IO.FileMode]::CreateNew)
        $writer = [System.IO.StreamWriter]::new($stream)
        try { $writer.Write($meta) } finally { $writer.Dispose() }
    } catch [System.IO.IOException] {
        if (-not (Test-Path -LiteralPath $metaPath)) { throw }
    }
}
foreach ($folder in @('Assets','Packages','ProjectSettings')) {
    & robocopy (Join-Path $projectRoot $folder) (Join-Path $validationRoot $folder) /E /XO /NFL /NDL /NJH /NJS /NP
    if ($LASTEXITCODE -ge 8) { throw "Copy failed: $folder" }
}
# Reuse resolved package source; never share the mutable AssetDatabase with the open Editor.
& robocopy (Join-Path $projectRoot 'Library/PackageCache') (Join-Path $validationRoot 'Library/PackageCache') /E /XO /NFL /NDL /NJH /NJS /NP
if ($LASTEXITCODE -ge 8) { throw 'Package copy failed' }
$unityExe = 'D:\Unity editor\6000.3.23f1\Editor\Unity.exe'
$log = Join-Path $resultRoot "$Tag.log"
$xml = Join-Path $resultRoot "$Tag.xml"
$argsList = @('-batchmode','-nographics','-projectPath', ('"' + $validationRoot + '"'), '-logFile', ('"' + $log + '"'))
if ($Mode -eq 'Method') { $argsList += @('-executeMethod', $Method, '-quit') }
else { $argsList += @('-runTests','-testPlatform',$Mode,'-testResults',('"' + $xml + '"')) }
$process = Start-Process -FilePath $unityExe -ArgumentList $argsList -WindowStyle Hidden -PassThru
$process.Id | Set-Content (Join-Path $resultRoot "$Tag.pid")
Write-Output "Started Unity PID $($process.Id). Log: $log"
