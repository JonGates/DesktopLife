using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DesktopLife.Engine.Creatures;

namespace DesktopLife.Rendering;

/// <summary>Cached fish tail/fin and turtle flipper poses. All sprites face right.</summary>
internal static class OceanSprite
{
    private static readonly BitmapSource[] Fish = LoadBodies("ocean-fish-a.png", 4)
        .Concat(LoadBodies("ocean-fish-b.png", 4)).Concat(LoadBodies("ocean-fish-c.png", 4)).ToArray();
    private static readonly BitmapSource Turtle = LoadBodies("ocean-turtle.png", 1)[0];

    public static DrawingGroup CreateTurtleShell(bool cute)
    {
        var group = new DrawingGroup();
        using (var dc = group.Open())
        {
            // Keep the shell at the same body location while head, tail and flippers retract.
            var shell = new EllipseGeometry(new Point(-4, 0), 10.4, 9.6);
            dc.PushClip(shell);
            if (!cute) dc.DrawImage(Turtle, new Rect(-15, -10, 30, 20));
            else
            {
                dc.DrawGeometry(new SolidColorBrush(Color.FromRgb(103, 164, 113)), new Pen(Brushes.DarkOliveGreen, .5), shell);
                dc.DrawEllipse(new SolidColorBrush(Color.FromRgb(162, 209, 127)), new Pen(Brushes.DarkOliveGreen, .5), new(-4, 0), 5, 5);
                for (var i = 0; i < 6; i++)
                {
                    var a = i * Math.PI / 3;
                    dc.DrawLine(new Pen(Brushes.DarkOliveGreen, .5), new(-4 + Math.Cos(a) * 5, Math.Sin(a) * 5), new(-4 + Math.Cos(a) * 10.5, Math.Sin(a) * 10));
                }
            }
            dc.Pop();
        }
        group.Freeze(); return group;
    }

    public static DrawingGroup Create(InsectDefinition definition, int frame, bool cute)
    {
        var result = new DrawingGroup();
        using (var dc = result.Open())
        {
            var beat = Math.Sin(frame * Math.PI / 4);
            if (definition.Kind == CreatureKind.GreenTurtle) DrawTurtle(dc, definition, beat, cute);
            else if (cute) CuteOceanBody.Draw(dc, definition, beat);
            else
            {
                var index = OceanCatalog.Fish.ToList().FindIndex(d => d.Kind == definition.Kind);
                double l = definition.BodyLength, w = definition.BodyWidth;
                var aspect = (double)Fish[index].PixelWidth / Fish[index].PixelHeight;
                l = Math.Min(l, w * aspect); w = l / aspect;
                var bounds = new Rect(-l / 2, -w / 2, l, w);
                var seahorse = definition.Kind == CreatureKind.Seahorse;
                var idol = definition.Kind == CreatureKind.MoorishIdol;
                var tailClip = seahorse ? new Rect(-l, w * 0.22, l * 2, w)
                    : idol ? new Rect(-l, -w * 0.18, l * 0.99, w * 0.46)
                    : new Rect(-l, -w, l * 0.77, w * 2);
                // The tail is articulated around its attachment; body overlaps the seam slightly.
                dc.PushTransform(seahorse ? new RotateTransform(beat * 5, 0, w * 0.22) : new RotateTransform(beat * 13, idol ? -l * 0.015 : -l * 0.25, 0));
                dc.PushClip(new RectangleGeometry(tailClip));
                dc.DrawImage(Fish[index], bounds);
                dc.Pop(); dc.Pop();
                // The idol's long dorsal streamer extends behind its tail; keep it attached to the body.
                dc.PushClip(idol
                    ? new CombinedGeometry(GeometryCombineMode.Exclude, new RectangleGeometry(new Rect(-l, -w, l * 2, w * 2)), new RectangleGeometry(new Rect(-l, -w * 0.18, l * 0.98, w * 0.46)))
                    : new RectangleGeometry(seahorse ? new Rect(-l, -w, l * 2, w * 1.25) : new Rect(-l * 0.26, -w, l * 1.3, w * 2)));
                dc.DrawImage(Fish[index], bounds);
                dc.Pop();
                // A translucent pectoral/dorsal fin pulse adds movement without deforming the body.
                var fin = new SolidColorBrush(Color.FromArgb(65, 189, 221, 216));
                dc.PushTransform(new RotateTransform(beat * 18, seahorse ? -l * 0.14 : l * 0.13, 0));
                dc.DrawEllipse(fin, null, new(seahorse ? -l * 0.2 : l * 0.08, w * 0.12), l * 0.1, w * (0.09 + 0.035 * beat));
                dc.Pop();
            }
        }
        result.Freeze(); return result;
    }

    private static void DrawTurtle(DrawingContext dc, InsectDefinition definition, double beat, bool cute)
    {
        double l = definition.BodyLength, w = definition.BodyWidth;
        var flipper = new SolidColorBrush(cute ? Color.FromRgb(131, 191, 122) : Color.FromRgb(89, 119, 57));
        var outline = new Pen(new SolidColorBrush(Color.FromRgb(53, 91, 59)), 0.45);
        for (var side = -1; side <= 1; side += 2)
        for (var pair = 0; pair < 2; pair++)
        {
            var x = l * (pair == 0 ? 0.15 : -0.3);
            dc.PushTransform(new RotateTransform(side * beat * (pair == 0 ? 20 : 10), x, side * w * 0.22));
            var fin = new StreamGeometry();
            using (var path = fin.Open())
            {
                path.BeginFigure(new(x, side * w * 0.2), true, true);
                path.BezierTo(new(x + l * 0.12, side * w * 0.46), new(x - l * 0.12, side * w * (pair == 0 ? 0.86 : 0.62)), new(x - l * 0.24, side * w * (pair == 0 ? 0.8 : 0.55)), true, false);
                path.QuadraticBezierTo(new(x - l * 0.12, side * w * 0.33), new(x, side * w * 0.2), true, false);
            }
            dc.DrawGeometry(flipper, outline, fin); dc.Pop();
        }
        dc.DrawLine(new Pen(flipper, 1.4), new(-l * 0.45, 0), new(-l * 0.57, 0));
        if (!cute) { dc.DrawImage(Turtle, new Rect(-l / 2, -w / 2, l, w)); return; }
        dc.DrawEllipse(flipper, outline, new(l * 0.34, 0), l * 0.16, w * 0.22);
        dc.DrawEllipse(new SolidColorBrush(Color.FromRgb(103, 164, 113)), outline, new(-l * 0.12, 0), l * 0.34, w * 0.46);
        dc.DrawEllipse(new SolidColorBrush(Color.FromRgb(162, 209, 127)), outline, new(-l * 0.12, 0), l * 0.16, w * 0.25);
        for (var i = 0; i < 6; i++)
        {
            var a = i * Math.PI / 3;
            dc.DrawLine(outline, new(-l * 0.12 + Math.Cos(a) * l * 0.16, Math.Sin(a) * w * 0.25), new(-l * 0.12 + Math.Cos(a) * l * 0.32, Math.Sin(a) * w * 0.43));
        }
        foreach (var side in new[] { -1, 1 })
        {
            dc.DrawEllipse(Brushes.Ivory, null, new(l * 0.38, side * w * 0.12), 1.4, 1.4);
            dc.DrawEllipse(Brushes.Black, null, new(l * 0.4, side * w * 0.12), 0.65, 0.8);
        }
    }

    private static BitmapSource[] LoadBodies(string name, int count)
    {
        using var stream = typeof(OceanSprite).Assembly.GetManifestResourceStream("DesktopLife.Rendering.Assets." + name)
            ?? throw new InvalidOperationException("Missing ocean texture: " + name);
        var decoded = BitmapDecoder.Create(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad).Frames[0];
        var bitmap = new FormatConvertedBitmap(decoded, PixelFormats.Bgra32, null, 0);
        var stride = bitmap.PixelWidth * 4;
        var pixels = new byte[stride * bitmap.PixelHeight]; bitmap.CopyPixels(pixels, stride, 0);
        // Fin tips can overlap in Y without the silhouettes touching. Extract connected
        // alpha islands, not horizontal rows, so no neighbouring fish leaks into a crop.
        var width = bitmap.PixelWidth; var height = bitmap.PixelHeight;
        var labels = new int[width * height];
        var bodies = new List<(int Label, int Left, int Top, int Right, int Bottom)>();
        var pending = new Stack<int>(); var label = 0;
        for (var start = 0; start < labels.Length; start++)
        {
            if (labels[start] != 0 || pixels[start * 4 + 3] <= 16) continue;
            label++; labels[start] = label; pending.Push(start);
            var left = width; var right = 0; var top = height; var bottom = 0; var area = 0;
            while (pending.TryPop(out var p))
            {
                var x = p % width; var y = p / width; area++;
                left = Math.Min(left, x); right = Math.Max(right, x);
                top = Math.Min(top, y); bottom = Math.Max(bottom, y);
                Visit(x > 0 ? p - 1 : -1); Visit(x + 1 < width ? p + 1 : -1);
                Visit(p - width); Visit(p + width);
            }
            if (area > 500) bodies.Add((label, left, top, right, bottom));
            void Visit(int p)
            {
                if (p < 0 || p >= labels.Length || labels[p] != 0 || pixels[p * 4 + 3] <= 16) return;
                labels[p] = label; pending.Push(p);
            }
        }
        if (bodies.Count != count) throw new InvalidOperationException($"{name}: expected {count} isolated bodies, got {bodies.Count}");
        return bodies.OrderBy(b => b.Top).Select(b =>
        {
            if (b.Left <= 0 || b.Right >= width - 1 || b.Top <= 0 || b.Bottom >= height - 1)
                throw new InvalidOperationException("Ocean body touches image edge: " + name);
            var w = b.Right - b.Left + 1; var h = b.Bottom - b.Top + 1;
            var cropped = new byte[w * h * 4];
            for (var y = 0; y < h; y++)
            for (var x = 0; x < w; x++)
            {
                var source = (y + b.Top) * width + x + b.Left;
                if (labels[source] == b.Label) Buffer.BlockCopy(pixels, source * 4, cropped, (y * w + x) * 4, 4);
            }
            var body = BitmapSource.Create(w, h, 96, 96, PixelFormats.Bgra32, null, cropped, w * 4);
            body.Freeze(); return body;
        }).ToArray();
    }
}
