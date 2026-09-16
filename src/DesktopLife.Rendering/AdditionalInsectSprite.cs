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

    public static DrawingGroup Create(InsectDefinition insect, int frame, bool cute = false,
        LocomotionState motion = LocomotionState.Walking, float progress = 0)
    {
        var kind = insect.Kind;
        var index = InsectCatalog.Additional.ToList().FindIndex(x => x.Kind == kind);
        var length = (double)insect.BodyLength;
        var width = (double)insect.BodyWidth;
        var group = new DrawingGroup();
        using (var dc = group.Open())
        {
            var jumping = motion is LocomotionState.JumpPreparing or LocomotionState.Jumping or LocomotionState.JumpLanding;
            var compression = motion == LocomotionState.JumpPreparing ? progress * 0.12 : motion == LocomotionState.JumpLanding ? Math.Sin(progress * Math.PI) * 0.14 : 0;
            dc.PushTransform(new ScaleTransform(1 + compression * 0.25, 1 - compression));
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
            if (cute) pen.Brush = new SolidColorBrush(Color.FromRgb(91, 108, 86));
            var fine = new Pen(pen.Brush, Math.Clamp(length / 100, 0.24, 0.5));
            for (var side = -1; side <= 1; side += 2)
            {
                for (var pair = 0; pair < 3; pair++)
                {
                    if (jumping)
                    {
                        DrawJumpLeg(dc, pen.Brush, pair, side, length, width, motion, progress);
                        continue;
                    }
                    var phase = (frame / 8.0 + pair * 0.5 + (side > 0 ? 0.5 : 0)) % 1;
                    var stroke = phase < 0.65 ? 1 - 2 * phase / 0.65 : -Math.Cos((phase - 0.65) / 0.35 * Math.PI);
                    if (kind == CreatureKind.Mantis)
                    {
                        DrawMantisLeg(dc, pen.Brush, pair, side, stroke, length, width);
                        continue;
                    }
                    var stride = insect.Stride * 0.325;
                    var rootX = length * (0.25 - pair * 0.085);
                    var reach = length * (0.22 - pair * 0.2);
                    var span = Math.Max(width * 0.9, length * 0.24);
                    if (kind == CreatureKind.StickInsect) { rootX = length * (0.34 - pair * 0.12); span = length * 0.28; }
                    if (kind == CreatureKind.Ladybug)
                    {
                        // Coxae stay beneath the thorax; only short knees and tarsi peek past the elytra.
                        rootX = length * (0.22 - pair * 0.12);
                        reach = length * (0.12 - pair * 0.12);
                        span = width * (pair == 1 ? 0.58 : 0.51);
                    }
                    var root = new Point(rootX, side * width * 0.2);
                    var knee = new Point(rootX + reach * 0.6 + stroke * stride * 0.35, side * span * 0.6);
                    var foot = new Point(rootX + reach + stroke * stride, side * span);
                    if (kind == CreatureKind.Ladybug && motion is LocomotionState.Flying or LocomotionState.TakingOff or LocomotionState.Landing)
                    {
                        var tuck = motion == LocomotionState.TakingOff ? progress : motion == LocomotionState.Landing ? 1 - progress : 1;
                        knee = Lerp(knee, new(rootX - 0.5, side * width * 0.28), tuck);
                        foot = Lerp(foot, new(rootX - 1, side * width * 0.4), tuck);
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
                if (kind == CreatureKind.Ladybug)
                {
                    // Short clubbed antennae, separate from the six walking legs.
                    var tip = new Point(length * 0.56, side * (width * 0.23 + sweep * 0.18));
                    Curve(dc, fine, new(length * 0.43, side * width * 0.12), new(length * 0.55, side * width * 0.16), tip);
                    dc.DrawEllipse(pen.Brush, null, tip, 0.24, 0.19);
                }
                else Curve(dc, fine, new(length * 0.43, side * width * 0.16),
                    new(length * 0.52 + antennaLength * 0.5, side * (width * 0.55 + sweep)),
                    new(length * 0.48 + antennaLength, side * (width * 0.9 + sweep)));
                if (kind == CreatureKind.Earwig)
                    Curve(dc, pen, new(-length * 0.43, side * width * 0.3), new(-length * 0.83, side * width * 0.9), new(-length * 0.74, side * width * 0.09));
                if (kind is CreatureKind.Cricket or CreatureKind.Silverfish)
                    dc.DrawLine(fine, new(-length * 0.44, side * width * 0.2), new(-length * (kind == CreatureKind.Silverfish ? 1.03 : 0.67), side * width * 1.05));
            }
            if (kind == CreatureKind.Silverfish)
                dc.DrawLine(fine, new(-length * 0.4, 0), new(-length * 1.16, 0));
            if (kind == CreatureKind.Ladybug && motion is LocomotionState.TakingOff or LocomotionState.Flying or LocomotionState.Landing)
                DrawFlyingLadybug(dc, insect, frame, cute, motion == LocomotionState.TakingOff ? progress : motion == LocomotionState.Landing ? 1 - progress : 1);
            else if (cute) CuteAdditionalBody.Draw(dc, insect);
            else dc.DrawImage(Bodies[index], new Rect(-length / 2, -width / 2, length, width));
            dc.Pop();
        }
        group.Freeze();
        return group;
    }

    private static void DrawJumpLeg(DrawingContext dc, Brush brush, int pair, int side, double length, double width, LocomotionState motion, double t)
    {
        var root = new Point(length * (0.25 - pair * 0.085), side * width * 0.2);
        var back = pair == 2;
        var knee = new Point(root.X + length * (back ? -0.3 : 0.12 - pair * 0.12), side * length * (back ? 0.24 : 0.14));
        var foot = new Point(root.X + length * (back ? 0.1 : 0.22 - pair * 0.2), side * length * (back ? 0.33 : 0.24));
        var tuckedKnee = new Point(root.X - length * (back ? 0.22 : 0.035), side * width * (back ? 0.9 : 0.5));
        var tuckedFoot = new Point(root.X - length * 0.08, side * width * 0.75);
        if (motion == LocomotionState.JumpPreparing)
        {
            knee = Lerp(knee, tuckedKnee, t * 0.45);
            foot = Lerp(foot, tuckedFoot, t * 0.75);
        }
        else if (motion == LocomotionState.Jumping)
        {
            if (back && t < 0.22)
            {
                knee = Lerp(new(root.X - length * 0.29, side * width * 0.95), tuckedKnee, t / 0.22);
                foot = Lerp(new(root.X - length * 0.6, side * width * 1.1), tuckedFoot, t / 0.22);
            }
            else { knee = tuckedKnee; foot = tuckedFoot; }
            // Reach out before touching down.
            if (t > 0.8)
            {
                var reach = (t - 0.8) / 0.2;
                knee = Lerp(knee, new(root.X + length * (back ? -0.26 : 0.1), side * length * 0.2), reach);
                foot = Lerp(foot, new(root.X + length * (back ? 0.08 : 0.16 - pair * 0.17), side * length * 0.28), reach);
            }
        }
        var upper = new Pen(brush, back ? length * 0.07 : 0.7) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };
        var lower = new Pen(brush, back ? 0.65 : 0.45) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };
        dc.DrawLine(upper, root, knee); dc.DrawLine(lower, knee, foot);
        dc.DrawLine(new Pen(brush, 0.25), foot, new(foot.X - 0.8, foot.Y + side * 0.35));
    }

    private static void DrawFlyingLadybug(DrawingContext dc, InsectDefinition insect, int frame, bool cute, double spread)
    {
        double length = insect.BodyLength, width = insect.BodyWidth;
        void Body()
        {
            if (cute) CuteAdditionalBody.Draw(dc, insect);
            else dc.DrawImage(Bodies[0], new Rect(-length / 2, -width / 2, length, width));
        }
        if (spread < 0.015) { Body(); return; }
        var abdomen = new SolidColorBrush(cute ? Color.FromRgb(114, 132, 114) : Color.FromRgb(49, 39, 31));
        dc.DrawEllipse(abdomen, null, new(-length * 0.13, 0), length * 0.36, width * 0.39);
        // Hindwings unfold under the rigid elytra and beat independently in flight.
        for (var side = -1; side <= 1; side += 2)
        {
            var beat = Math.Sin(frame * Math.PI / 4);
            dc.PushTransform(new RotateTransform(-side * (28 + beat * 18) * spread, length * 0.17, 0));
            var wing = new SolidColorBrush(Color.FromArgb((byte)(spread * (cute ? 155 : 110)), 218, 235, 240));
            var vein = new Pen(new SolidColorBrush(Color.FromArgb((byte)(spread * 130), 133, 153, 154)), 0.18);
            dc.DrawEllipse(wing, vein, new(-length * 0.32, side * width * 0.36 * spread), length * 0.7 * spread, width * 0.38 * spread);
            dc.DrawLine(vein, new(length * 0.16, 0), new(-length * 0.91 * spread, side * width * 0.43 * spread));
            dc.Pop();
            dc.PushTransform(new RotateTransform(-side * 62 * spread, length * 0.24, 0));
            dc.PushClip(new RectangleGeometry(new Rect(-length * 0.52, side < 0 ? -width * 0.55 : 0, length * 0.76, width * 0.55)));
            Body();
            dc.Pop(); dc.Pop();
        }
        // Head and pronotum remain anchored while the two shell halves open.
        dc.PushClip(new RectangleGeometry(new Rect(length * 0.24, -width, length * 0.5, width * 2)));
        Body(); dc.Pop();
    }

    private static void DrawMantisLeg(DrawingContext dc, Brush brush, int pair, int side, double stroke, double length, double width)
    {
        Pen Line(double thickness) => new(brush, thickness) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };
        if (pair == 0)
        {
            // Coxa leads back from the prothorax; femur extends forward and tibia folds against it.
            var root = new Point(length * 0.32, side * width * 0.12);
            var hip = new Point(length * 0.22, side * width * 0.46);
            var knee = new Point(length * 0.39 + stroke * 0.25, side * width * 0.88);
            var folded = new Point(length * 0.27, side * width * 0.63);
            dc.DrawLine(Line(0.58), root, hip);
            dc.DrawLine(Line(1.0), hip, knee);
            dc.DrawLine(Line(0.62), knee, folded);
            dc.DrawLine(Line(0.28), folded, new(folded.X + length * 0.035, folded.Y - side * 0.35));
            for (var tooth = 1; tooth <= 5; tooth++)
            {
                var p = Lerp(hip, knee, tooth / 6.0);
                dc.DrawLine(Line(0.23), p, new(p.X + 0.25, p.Y + side * 0.48));
            }
            return;
        }
        var rootX = length * (pair == 1 ? 0.06 : -0.02);
        var rootPoint = new Point(rootX, side * width * 0.18);
        var hipPoint = new Point(rootX + (pair == 1 ? 0.9 : -1), side * width * 0.42);
        var kneePoint = new Point(rootX + length * (pair == 1 ? 0.09 : -0.14) + stroke * 0.7, side * length * 0.14);
        var ankle = new Point(rootX + length * (pair == 1 ? 0.14 : -0.23) + stroke * 4.55, side * length * (pair == 1 ? 0.23 : 0.25));
        dc.DrawLine(Line(0.6), rootPoint, hipPoint);
        dc.DrawLine(Line(0.65), hipPoint, kneePoint);
        dc.DrawLine(Line(0.42), kneePoint, ankle);
        dc.DrawLine(Line(0.25), ankle, new(ankle.X - 1.6, ankle.Y + side * 0.4));
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
