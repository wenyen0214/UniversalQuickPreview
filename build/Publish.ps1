param(
    [string] $Version = '0.1.0',
    [string] $Runtime = 'win-x64',
    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Release',
    [switch] $SkipInstaller
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$artifactsRoot = [System.IO.Path]::GetFullPath((Join-Path $repositoryRoot 'artifacts'))
$publishDirectory = [System.IO.Path]::GetFullPath((Join-Path $artifactsRoot "publish\$Runtime"))
$installerDirectory = [System.IO.Path]::GetFullPath((Join-Path $artifactsRoot 'installer'))
$portableArchive = [System.IO.Path]::GetFullPath(
    (Join-Path $artifactsRoot "UniversalQuickPreview-$Version-portable-$Runtime.zip"))

function Assert-ArtifactsPath([string] $Path) {
    $prefix = $artifactsRoot.TrimEnd('\') + '\'
    if (-not $Path.StartsWith($prefix, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing to modify a path outside the artifacts directory: $Path"
    }
}

Assert-ArtifactsPath $publishDirectory
Assert-ArtifactsPath $installerDirectory
Assert-ArtifactsPath $portableArchive

if (Test-Path -LiteralPath $publishDirectory) {
    Remove-Item -LiteralPath $publishDirectory -Recurse -Force
}
if (Test-Path -LiteralPath $portableArchive) {
    Remove-Item -LiteralPath $portableArchive -Force
}

New-Item -ItemType Directory -Force -Path $publishDirectory, $installerDirectory | Out-Null

dotnet publish (Join-Path $repositoryRoot 'src\UniversalQuickPreview.App\UniversalQuickPreview.App.csproj') `
    --configuration $Configuration `
    --runtime $Runtime `
    --self-contained true `
    --output $publishDirectory `
    -p:Version=$Version `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:DebugType=None `
    -p:DebugSymbols=false
if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed with exit code $LASTEXITCODE."
}

Compress-Archive -Path (Join-Path $publishDirectory '*') -DestinationPath $portableArchive -CompressionLevel Optimal
Write-Output "Portable archive: $portableArchive"

if ($SkipInstaller) {
    return
}

$compilerCandidates = @(
    (Get-Command 'ISCC.exe' -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Source),
    "$env:LOCALAPPDATA\Programs\Inno Setup 7\ISCC.exe",
    "$env:ProgramFiles\Inno Setup 7\ISCC.exe",
    "$env:ProgramFiles(x86)\Inno Setup 7\ISCC.exe",
    "$env:ProgramFiles(x86)\Inno Setup 6\ISCC.exe",
    "$env:ProgramFiles\Inno Setup 6\ISCC.exe"
) | Where-Object { $_ -and (Test-Path -LiteralPath $_) }
$compiler = $compilerCandidates | Select-Object -First 1
if (-not $compiler) {
    throw 'Inno Setup was not found. Install Inno Setup 7 or run with -SkipInstaller.'
}

& $compiler "/DMyAppVersion=$Version" "/DPublishDir=$publishDirectory" "/DOutputDir=$installerDirectory" `
    (Join-Path $repositoryRoot 'installer\UniversalQuickPreview.iss')
if ($LASTEXITCODE -ne 0) {
    throw "Inno Setup failed with exit code $LASTEXITCODE."
}

Write-Output "Installer directory: $installerDirectory"
