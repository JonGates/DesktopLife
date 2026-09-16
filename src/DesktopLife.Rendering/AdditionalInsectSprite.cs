using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DesktopLife.Engine.Creatures;

namespace DesktopLife.Rendering;

/// <summary>Distinct body textures and articulated appendages; all heads face +X.</summary>
internal static class AdditionalInsectSprite
{
    private static readonly BitmapSource[] Bodies = LoadAtlas("small-insect-bodies.png")
        .Concat(LoadAtlas("long-insect-bodies.png")).ToArray();

    public static DrawingGroup Create(InsectDefinition insect, int frame)
    {
        var kind = insect.Kind;
        var index = InsectCatalog.Additional.ToList().FindIndex(x => x.Kind == kind);
        var length = (double)insect.BodyLength;
        var width = (double)insect.BodyWidth;
        var group = new DrawingGroup();
        using (var dc = group.Open())
        {
            var color = kind switch
            {
                CreatureKind.Ladybug => Color.FromRgb(40, 33, 25),
                CreatureKind.GroundBeetle => Color.FromRgb(54, 48, 32),
                CreatureKind.Silverfish => Color.FromRgb(153, 155, 151),
                CreatureKind.Grasshopper => Color.FromRgb(88, 112, 43),
                CreatureKind.Mantis => Color.FromRgb(85, 123, 48),
                CreatureKind.StickInsect => Color.FromRgb(111, 81, 45),
                _ => Color.FromRgb(78, 47, 28)
            };
            var pen = new Pen(new SolidColorBrush(color), Math.Clamp(length / 45, 0.4, 1.0))
                { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round, LineJoin = PenLineJoin.Round };
            var fine = new Pen(pen.Brush, Math.Clamp(length / 100, 0.24, 0.5));
            for (var side = -1; side <= 1; side += 2)
            {
                for (var pair = 0; pair < 3; pair++)
                {
                    var phase = (frame / 8.0 + pair * 0.5 + (side > 0 ? 0.5 : 0)) % 1;
                    var stroke = phase < 0.65 ? 1 - 2 * phase / 0.65 : -Math.Cos((phase - 0.65) / 0.35 * Math.PI);
                    var stride = insect.Stride * 0.325;
                    var rootX = length * (0.25 - pair * 0.085);
                    var reach = length * (0.22 - pair * 0.2);
                    var span = Math.Max(width * 0.9, length * 0.24);
                    if (kind == CreatureKind.StickInsect) { rootX = length * (0.34 - pair * 0.12); span = length * 0.28; }
                    if (kind == CreatureKind.Mantis) { rootX = pair == 0 ? length * 0.34 : length * (0.05 - (pair - 1) * 0.14); span = length * 0.26; }
                    var root = new Point(rootX, side * width * 0.2);
                    var knee = new Point(rootX + reach * 0.6 + stroke * stride * 0.35, side * span * 0.6);
                    var foot = new Point(rootX + reach + stroke * stride, side * span);
                    if (kind == CreatureKind.Mantis && pair == 0)
                    {
                        // Forelegs remain folded in the characteristic prey-catching posture.
                        knee = new(rootX + length * 0.13, side * width * 1.05);
                        foot = new(rootX - length * 0.03 + stroke * 0.4, side * width * 0.68);
                        dc.DrawLine(new Pen(pen.Brush, 1.4), root, knee);
                        dc.DrawLine(pen, knee, foot);
                        for (var tooth = 1; tooth <= 4; tooth++)
                        {
                            var p = Lerp(knee, foot, tooth / 5.0);
                            dc.DrawLine(fine, p, new(p.X - 0.4, p.Y - side * 0.9));
                        }
                        continue;
                    }
                    if (pair == 2 && kind is CreatureKind.Cricket or CreatureKind.Grasshopper)
                    {
                        // Enlarged femur folds back, followed by a thin, spiny tibia.
                        knee = new(rootX - length * 0.3 + stroke, side * length * 0.24);
                        foot = new(rootX + length * 0.1 + stroke * stride, side * length * 0.33);
                        dc.DrawLine(new Pen(pen.Brush, length * 0.075) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round }, root, knee);
                        dc.DrawLine(pen, knee, foot);
                        for (var thorn = 1; thorn <= 5; thorn++)
                        {
                            var p = Lerp(knee, foot, thorn / 6.0);
                            dc.DrawLine(fine, p, new(p.X - 0.6, p.Y + side * 0.9));
                        }
                    }
                    else { dc.DrawLine(pen, root, knee); dc.DrawLine(pen, knee, foot); }
                    dc.DrawLine(fine, foot, new(foot.X - 0.8, foot.Y + side * 0.4));
                }
                var antennaLength = length * (kind switch
                {
                    CreatureKind.Ladybug => 0.18,
                    CreatureKind.Grasshopper => 0.28,
                    CreatureKind.Cricket => 0.95,
                    CreatureKind.Silverfish => 0.65,
                    CreatureKind.StickInsect => 0.4,
                    _ => 0.42
                });
                var sweep = Math.Sin(frame * Math.PI / 4 + side * 0.5) * 0.7;
                Curve(dc, fine, new(length * 0.43, side * width * 0.16),
                    new(length * 0.52 + antennaLength * 0.5, side * (width * 0.55 + sweep)),
                    new(length * 0.48 + antennaLength, side * (width * 0.9 + sweep)));
                if (kind == CreatureKind.Earwig)
                    Curve(dc, pen, new(-length * 0.43, side * width * 0.3), new(-length * 0.83, side * width * 0.9), new(-length * 0.74, side * width * 0.09));
                if (kind is CreatureKind.Cricket or CreatureKind.Silverfish)
                    dc.DrawLine(fine, new(-length * 0.44, side * width * 0.2), new(-length * (kind == CreatureKind.Silverfish ? 1.03 : 0.67), side * width * 1.05));
            }
            if (kind == CreatureKind.Silverfish)
                dc.DrawLine(fine, new(-length * 0.4, 0), new(-length * 1.16, 0));
            dc.DrawImage(Bodies[index], new Rect(-length / 2, -width / 2, length, width));
        }
        group.Freeze();
        return group;
    }

    private static Point Lerp(Point a, Point b, double t) => new(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t);
    private static void Curve(DrawingContext dc, Pen pen, Point from, Point bend, Point to)
    {
        var geometry = new StreamGeometry();
        using (var path = geometry.Open()) { path.BeginFigure(from, false, false); path.QuadraticBezierTo(bend, to, true, false); }
        geometry.Freeze(); dc.DrawGeometry(null, pen, geometry);
    }

    private static BitmapSource[] LoadAtlas(string file)
    {
        using var stream = typeof(AdditionalInsectSprite).Assembly.GetManifestResourceStream("DesktopLife.Rendering.Assets." + file)
            ?? throw new InvalidOperationException("Missing additional insect atlas: " + file);
        var decoded = BitmapDecoder.Create(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad).Frames[0];
        var atlas = new FormatConvertedBitmap(decoded, PixelFormats.Bgra32, null, 0);
        var stride = atlas.PixelWidth * 4;
        var pixels = new byte[stride * atlas.PixelHeight];
        atlas.CopyPixels(pixels, stride, 0);
        var result = new BitmapSource[4];
        // Generated atlases have clear transparent gutters, but rows need not be exactly equal.
        var bands = new List<(int Top, int Bottom)>();
        var start = -1;
        for (var y = 0; y <= atlas.PixelHeight; y++)
        {
            var occupied = false;
            if (y < atlas.PixelHeight)
                for (var x = 0; x < atlas.PixelWidth && !occupied; x++)
                    occupied = pixels[y * stride + x * 4 + 3] > 16;
            if (occupied && start < 0) start = y;
            if (!occupied && start >= 0) { bands.Add((start, y)); start = -1; }
        }
        if (bands.Count != 4) throw new InvalidOperationException("Atlas requires four separate bodies: " + file);
        for (var row = 0; row < 4; row++)
        {
            var top = Math.Max(0, bands[row].Top - 4);
            var bottom = Math.Min(atlas.PixelHeight, bands[row].Bottom + 4);
            var left = atlas.PixelWidth; var right = -1; var first = bottom; var last = -1;
            for (var y = top; y < bottom; y++)
            for (var x = 0; x < atlas.PixelWidth; x++)
                if (pixels[y * stride + x * 4 + 3] > 16)
                { left = Math.Min(left, x); right = Math.Max(right, x); first = Math.Min(first, y); last = Math.Max(last, y); }
            if (right < left || first <= top || last >= bottom - 1 || left == 0 || right == atlas.PixelWidth - 1)
                throw new InvalidOperationException("Atlas must contain four isolated transparent body rows: " + file);
            left = Math.Max(0, left - 3); right = Math.Min(atlas.PixelWidth - 1, right + 3);
            first = Math.Max(top, first - 3); last = Math.Min(bottom - 1, last + 3);
            var body = new CroppedBitmap(atlas, new Int32Rect(left, first, right - left + 1, last - first + 1));
            body.Freeze(); result[row] = body;
        }
        return result;
    }
}
