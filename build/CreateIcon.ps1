param(
    [Parameter(Mandatory = $true)]
    [string] $SourcePng,

    [Parameter(Mandatory = $true)]
    [string] $DestinationIco
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName PresentationCore

$sourcePath = (Resolve-Path -LiteralPath $SourcePng).Path
$destinationPath = [System.IO.Path]::GetFullPath($DestinationIco)
[System.IO.Directory]::CreateDirectory([System.IO.Path]::GetDirectoryName($destinationPath)) | Out-Null

$bitmap = [System.Windows.Media.Imaging.BitmapImage]::new()
$bitmap.BeginInit()
$bitmap.CacheOption = [System.Windows.Media.Imaging.BitmapCacheOption]::OnLoad
$bitmap.DecodePixelWidth = 256
$bitmap.UriSource = [Uri]::new($sourcePath)
$bitmap.EndInit()
$bitmap.Freeze()

$pngStream = [System.IO.MemoryStream]::new()
$encoder = [System.Windows.Media.Imaging.PngBitmapEncoder]::new()
$encoder.Frames.Add([System.Windows.Media.Imaging.BitmapFrame]::Create($bitmap))
$encoder.Save($pngStream)
$pngBytes = $pngStream.ToArray()
$pngStream.Dispose()

$fileStream = [System.IO.File]::Create($destinationPath)
$writer = [System.IO.BinaryWriter]::new($fileStream)
try {
    $writer.Write([UInt16] 0)
    $writer.Write([UInt16] 1)
    $writer.Write([UInt16] 1)
    $writer.Write([Byte] 0)
    $writer.Write([Byte] 0)
    $writer.Write([Byte] 0)
    $writer.Write([Byte] 0)
    $writer.Write([UInt16] 1)
    $writer.Write([UInt16] 32)
    $writer.Write([UInt32] $pngBytes.Length)
    $writer.Write([UInt32] 22)
    $writer.Write($pngBytes)
}
finally {
    $writer.Dispose()
    $fileStream.Dispose()
}

Write-Output $destinationPath
