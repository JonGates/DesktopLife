using System.Numerics;
using System.Windows;
using System.Windows.Media;
using DesktopLife.Engine.Creatures;
using DesktopLife.Engine.World;
namespace DesktopLife.Rendering;
public sealed class WpfCreatureRenderer : IRenderer
{
    private static readonly DrawingGroup[] FlyingFly = Enumerable.Range(0, 8).Select(i => FlySprite.Create(false, i)).ToArray();
    private static readonly DrawingGroup[] RestingFly = Enumerable.Range(0, 8).Select(i => FlySprite.Create(true, i)).ToArray();
    private static readonly DrawingGroup[] Ants = Enumerable.Range(0, 8).Select(i => SmallInsectSprite.Create(false, i)).ToArray();
    private static readonly DrawingGroup[] Caterpillars = Enumerable.Range(0, 8).Select(i => SmallInsectSprite.Create(true, i)).ToArray();
    private static readonly DrawingGroup[] Roaches = Enumerable.Range(0, 8).Select(CockroachSprite.Create).ToArray();
    private static readonly Dictionary<CreatureKind, DrawingGroup[]> Additional = InsectCatalog.Additional
        .ToDictionary(insect => insect.Kind, insect => Enumerable.Range(0, 8).Select(i => AdditionalInsectSprite.Create(insect, i)).ToArray());
    private static readonly Dictionary<CreatureKind, DrawingGroup[]> Cute = new[] { CreatureKind.Fly, CreatureKind.Ant, CreatureKind.Cockroach, CreatureKind.Caterpillar }
        .Concat(InsectCatalog.Additional.Select(x => x.Kind))
        .ToDictionary(k => k, k => Enumerable.Range(0, 8).Select(i => k is CreatureKind.Fly or CreatureKind.Ant or CreatureKind.Cockroach or CreatureKind.Caterpillar
            ? CuteInsectSprite.Create(k, i) : AdditionalInsectSprite.Create(InsectCatalog.Get(k), i, cute: true)).ToArray());
    private static readonly DrawingGroup[] CuteResting = Enumerable.Range(0, 8).Select(i => CuteInsectSprite.Create(CreatureKind.Fly, i, true)).ToArray();
    private static readonly DrawingGroup SettledFly = FlySprite.Create(true, grooming: false);
    private static readonly DrawingGroup CuteSettledFly = CuteInsectSprite.Create(CreatureKind.Fly, 0, true, false);
    private static readonly Dictionary<(CreatureKind, LocomotionState, bool), DrawingGroup[]> MotionSprites = CreateMotionSprites();
    private static readonly DrawingGroup[][] LadybugAir = new[] { false, true }.Select(cute => Enumerable.Range(0, 64)
        .Select(index => AdditionalInsectSprite.Create(InsectCatalog.Get(CreatureKind.Ladybug), index % 8, cute, LocomotionState.TakingOff, index / 8 / 7f)).ToArray()).ToArray();
    private static readonly Brush AirShadow = FrozenShadow();
    private static Brush FrozenShadow()
    {
        var brush = new SolidColorBrush(Color.FromArgb(42, 28, 38, 30)); brush.Freeze(); return brush;
    }
    private static Dictionary<(CreatureKind, LocomotionState, bool), DrawingGroup[]> CreateMotionSprites()
    {
        var result = new Dictionary<(CreatureKind, LocomotionState, bool), DrawingGroup[]>();
        foreach (var kind in new[] { CreatureKind.Cricket, CreatureKind.Grasshopper })
        foreach (var state in new[] { LocomotionState.JumpPreparing, LocomotionState.Jumping, LocomotionState.JumpLanding })
        foreach (var cute in new[] { false, true })
            result[(kind, state, cute)] = Enumerable.Range(0, 8)
                .Select(frame => AdditionalInsectSprite.Create(InsectCatalog.Get(kind), frame, cute, state, frame / 7f)).ToArray();
        return result;
    }
    public static bool IsGrooming(ICreature creature) => creature.IsResting && creature.RestingSeconds is >= 0.25f and <= 2.65f && creature.RestingSeconds is not (> 1.05f and < 1.4f);
    public CreatureStyle Style { get; set; }
    public static int Frame(ICreature creature)
    {
        if (creature.Kind != CreatureKind.Fly || !creature.IsResting) return (int)(creature.AnimationPhase * 8) & 7;
        // Settle, groom in short bouts, then hold still before departure.
        var t = creature.RestingSeconds;
        if (t < 0.25f || t > 2.65f || t is > 1.05f and < 1.4f) return 0;
        return (int)(t * 14) & 7;
    }
    public void Render(DrawingContext dc, IReadOnlyList<ICreature> creatures, WorldBounds bounds, float time, double scaleX, double scaleY)
    {
        for (var i = 0; i < creatures.Count; i++)
        {
            var creature = creatures[i];
            if (!creature.IsVisible || !bounds.Contains(creature.Position, 100 * creature.Scale)) continue;
            var p = ScreenCoordinates.ToLocal(creature.Position, new Vector2(bounds.Left, bounds.Top), (float)scaleX, (float)scaleY);
            var matrix = Matrix.Identity;
            matrix.Scale(creature.Scale, creature.Scale);
            matrix.Rotate(creature.Rotation * 180 / Math.PI);
            matrix.Scale(1 / scaleX, 1 / scaleY);
            matrix.Translate(p.X, p.Y);
            if (creature.Elevation > 0)
            {
                dc.PushTransform(new MatrixTransform(matrix));
                dc.PushOpacity(Math.Max(0.3, 1 - creature.Elevation / 40));
                var body = InsectCatalog.Get(creature.Kind);
                dc.DrawEllipse(AirShadow, null, new Point(0, 1.5), body.BodyLength * 0.42, body.BodyWidth * 0.44);
                dc.Pop(); dc.Pop();
                matrix.Translate(0, -creature.Elevation * creature.Scale / scaleY);
            }
            var transform = new MatrixTransform(matrix);
            transform.Freeze();
            dc.PushTransform(transform);
            var frame = Frame(creature);
            if (creature.Kind == CreatureKind.Ladybug && creature.MotionState is LocomotionState.TakingOff or LocomotionState.Flying or LocomotionState.Landing)
            {
                var spreadFrame = Math.Clamp((int)Math.Round(creature.WingSpread * 7), 0, 7);
                dc.DrawDrawing(LadybugAir[Style == CreatureStyle.Cute ? 1 : 0][spreadFrame * 8 + ((int)(time * 42) & 7)]);
            }
            else if (MotionSprites.TryGetValue((creature.Kind, creature.MotionState, Style == CreatureStyle.Cute), out var motion))
            {
                var motionFrame = Math.Clamp((int)(creature.MotionProgress * 8), 0, 7);
                dc.DrawDrawing(motion[motionFrame]);
            }
            else if (Style == CreatureStyle.Cute && Cute.TryGetValue(creature.Kind, out var cute))
                dc.DrawDrawing(creature.Kind == CreatureKind.Fly && creature.IsResting ? (IsGrooming(creature) ? CuteResting[frame] : CuteSettledFly) : cute[frame]);
            else if (Additional.TryGetValue(creature.Kind, out var additional))
                dc.DrawDrawing(additional[frame]);
            else
                dc.DrawDrawing(creature.Kind switch
                {
                    CreatureKind.Cockroach => Roaches[frame],
                    CreatureKind.Ant => Ants[frame],
                    CreatureKind.Caterpillar => Caterpillars[frame],
                    _ => creature.IsResting ? (IsGrooming(creature) ? RestingFly[frame] : SettledFly) : FlyingFly[frame]
                });
            dc.Pop();
        }
    }
}
