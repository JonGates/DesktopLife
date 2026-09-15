param([string]$OutputDirectory = "$PSScriptRoot/../src/DesktopLife.App/Assets")
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName PresentationCore, WindowsBase
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$visual = New-Object System.Windows.Media.DrawingVisual
$dc = $visual.RenderOpen()
$background = [System.Windows.Media.BrushConverter]::new().ConvertFromString('#123F3B')
$gold = [System.Windows.Media.BrushConverter]::new().ConvertFromString('#F3BF61')
$light = [System.Windows.Media.BrushConverter]::new().ConvertFromString('#FFF1CF')
$pen = [System.Windows.Media.Pen]::new($gold, 10)
$pen.StartLineCap = 'Round'; $pen.EndLineCap = 'Round'; $pen.LineJoin = 'Round'
$dc.DrawRoundedRectangle($background, $null, [System.Windows.Rect]::new(4,4,248,248), 52,52)
foreach ($path in @('M 106,112 L 78,94 L 68,70','M 104,139 L 65,134 L 48,145','M 107,162 L 78,185 L 70,207','M 150,112 L 178,94 L 188,70','M 152,139 L 191,134 L 208,145','M 149,162 L 178,185 L 186,207','M 116,78 L 101,51','M 140,78 L 155,51')) { $dc.DrawGeometry($null, $pen, [System.Windows.Media.Geometry]::Parse($path)) }
$dc.DrawEllipse($gold,$null,[System.Windows.Point]::new(128,151),36,49)
$dc.DrawEllipse($light,$null,[System.Windows.Point]::new(128,92),25,24)
$dc.DrawLine([System.Windows.Media.Pen]::new($background,7),[System.Windows.Point]::new(128,116),[System.Windows.Point]::new(128,194))
$dc.Close()
$frames = @()
foreach ($size in @(16,24,32,48,64,128,256)) {
    $scaled = New-Object System.Windows.Media.DrawingVisual
    $context = $scaled.RenderOpen()
    $context.PushTransform([System.Windows.Media.ScaleTransform]::new($size/256.0,$size/256.0))
    $context.DrawDrawing($visual.Drawing); $context.Pop(); $context.Close()
    $bitmap = [System.Windows.Media.Imaging.RenderTargetBitmap]::new($size,$size,96,96,[System.Windows.Media.PixelFormats]::Pbgra32)
    $bitmap.Render($scaled)
    $encoder = New-Object System.Windows.Media.Imaging.PngBitmapEncoder
    $encoder.Frames.Add([System.Windows.Media.Imaging.BitmapFrame]::Create($bitmap))
    $memory = New-Object System.IO.MemoryStream
    $encoder.Save($memory)
    $frames += @{ Size=$size; Bytes=$memory.ToArray() }; $memory.Dispose()
    if ($size -eq 256) { [IO.File]::WriteAllBytes((Join-Path $OutputDirectory 'DesktopLife.png'),$frames[-1].Bytes) }
}
$file = [IO.File]::Create((Join-Path $OutputDirectory 'DesktopLife.ico'))
$writer = [IO.BinaryWriter]::new($file)
try {
    $writer.Write([uint16]0); $writer.Write([uint16]1); $writer.Write([uint16]$frames.Count)
    $offset = 6 + 16*$frames.Count
    foreach ($frame in $frames) {
        $dimension = if ($frame.Size -eq 256) {0} else {$frame.Size}
        $writer.Write([byte]$dimension); $writer.Write([byte]$dimension); $writer.Write([byte]0); $writer.Write([byte]0)
        $writer.Write([uint16]1); $writer.Write([uint16]32); $writer.Write([uint32]$frame.Bytes.Length); $writer.Write([uint32]$offset)
        $offset += $frame.Bytes.Length
    }
    foreach ($frame in $frames) { $writer.Write([byte[]]$frame.Bytes) }
} finally { $writer.Dispose() }
