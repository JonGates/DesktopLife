using System.Numerics;
using DesktopLife.Creatures.Displays;
using DesktopLife.Engine.Creatures;
using DesktopLife.Engine.Math;
using DesktopLife.Engine.World;

namespace DesktopLife.Creatures.Tests;

public class InsectLocomotionTests
{
    private static string State(CrawlingInsect insect) => insect.MotionState.ToString();

    [Theory]
    [InlineData(CreatureKind.Cricket, "JumpPreparing", "Jumping", "JumpLanding")]
    [InlineData(CreatureKind.Grasshopper, "JumpPreparing", "Jumping", "JumpLanding")]
    [InlineData(CreatureKind.Ladybug, "TakingOff", "Flying", "Landing")]
    public void AutonomousMotionCompletesInOrder(CreatureKind kind, string first, string airborne, string last)
    {
        var insect = new CrawlingInsect(new(200, 200), kind);
        var states = new List<string> { State(insect) };
        float peakHeight = 0, peakWings = 0;
        for (var frame = 0; frame < 1000; frame++)
        {
            var previousPhase = insect.AnimationPhase;
            var previousState = insect.MotionState;
            var previousRotation = insect.Rotation;
            insect.Update(0.02f, Context());
            Assert.InRange(insect.MotionProgress, 0, 1);
            Assert.InRange(insect.Elevation, 0, kind == CreatureKind.Ladybug ? 12 : 20);
            Assert.InRange(insect.WingSpread, 0, 1);
            Assert.InRange(MathF.Abs(insect.Rotation - previousRotation), 0, 0.241f);
            if (previousState != LocomotionState.Walking) Assert.Equal(previousPhase, insect.AnimationPhase);
            peakHeight = MathF.Max(peakHeight, insect.Elevation);
            peakWings = MathF.Max(peakWings, insect.WingSpread);
            if (states[^1] != State(insect)) states.Add(State(insect));
            if (states.Count == 5) break;
        }
        Assert.Equal(new[] { "Walking", first, airborne, last, "Walking" }, states);
        Assert.True(peakHeight > 10);
        Assert.Equal(kind == CreatureKind.Ladybug ? 1 : 0, peakWings);
        Assert.Equal(0, insect.Elevation);
        Assert.Equal(0, insect.WingSpread);
    }

    [Theory]
    [InlineData(CreatureKind.Cricket)]
    [InlineData(CreatureKind.Grasshopper)]
    public void NearbyMouseTriggersEarlierJumpButCannotRepeatWithoutCooldown(CreatureKind kind)
    {
        var insect = new CrawlingInsect(new(300, 300), kind);
        insect.Update(0.02f, Context(mouse: insect.Position - new Vector2(20, 0)));
        Assert.Equal(LocomotionState.JumpPreparing, insect.MotionState);
        for (var i = 0; i < 100 && insect.MotionState != LocomotionState.Walking; i++)
            insect.Update(0.02f, Context(mouse: insect.Position - new Vector2(20, 0)));
        Assert.Equal(LocomotionState.Walking, insect.MotionState);
        for (var i = 0; i < 100; i++)
        {
            insect.Update(0.02f, Context(mouse: insect.Position - new Vector2(20, 0)));
            Assert.Equal(LocomotionState.Walking, insect.MotionState);
        }
        for (var i = 0; i < 35 && insect.MotionState == LocomotionState.Walking; i++)
            insect.Update(0.02f, Context(mouse: insect.Position - new Vector2(20, 0)));
        Assert.Equal(LocomotionState.JumpPreparing, insect.MotionState);
    }

    [Theory]
    [InlineData(CreatureKind.Cricket)]
    [InlineData(CreatureKind.Grasshopper)]
    [InlineData(CreatureKind.Ladybug)]
    public void AirborneMotionCrossesConnectedNegativeScreensButNeverGaps(CreatureKind kind)
    {
        foreach (var gap in new[] { 0, 2, 40 })
        {
            var layout = new DesktopLayout([new("left", new(-400, -400, 400, 800)), new("right", new(gap, -400, 400, 800))]);
            var insect = new CrawlingInsect(new(kind == CreatureKind.Ladybug ? -300 : -1, -100), kind);
            var crossedInAir = false;
            for (var i = 0; i < 1000; i++)
            {
                var before = insect.Position;
                insect.Update(0.02f, Context(layout, kind == CreatureKind.Ladybug ? null : insect.Position - new Vector2(20, 0)));
                Assert.True(layout.Contains(insect.Position));
                Assert.InRange(Vector2.Distance(before, insect.Position), 0, 3.61f);
                if (gap > 0) Assert.True(insect.Position.X < 0);
                if (insect.Elevation > 0 && insect.Position.X > 0) crossedInAir = true;
            }
            if (gap == 0) Assert.True(crossedInAir);
        }
    }

    [Theory]
    [InlineData(CreatureKind.Cricket)]
    [InlineData(CreatureKind.Grasshopper)]
    [InlineData(CreatureKind.Ladybug)]
    public void ResizePreservesAirborneIndividualAndHotplugResetsItSafely(CreatureKind kind)
    {
        var simulation = new DisplaySimulation(8);
        simulation.Synchronize([new("old", new(0, 0, 2000, 2000))]);
        simulation.SetPopulation(new(0, 0, 0, Additional: new() { [kind] = new(1, 100, 100) }));
        var insect = Assert.IsType<CrawlingInsect>(simulation.World.Manager.Creatures.Single(c => c.Kind == kind));
        insect.Relocate(new(300, 300));
        AdvanceUntilAirborne(insect);
        var id = insect.Id;
        var height = insect.Elevation;
        var state = insect.MotionState;
        var position = insect.Position;
        simulation.SetPopulation(new(0, 0, 0, Additional: new() { [kind] = new(1, 200, 200) }));
        Assert.Same(insect, simulation.World.Manager.Creatures.Single(c => c.Kind == kind));
        Assert.Equal(2, insect.Scale);
        Assert.Equal(height, insect.Elevation);
        Assert.Equal(state, insect.MotionState);
        Assert.Equal(position, insect.Position);
        simulation.Synchronize([new("new", new(-2000, -2000, 800, 800))]);
        Assert.Equal(id, insect.Id);
        Assert.True(simulation.Layout.Contains(insect.Position));
        Assert.Equal(LocomotionState.Walking, insect.MotionState);
        Assert.Equal(Vector2.Zero, insect.Velocity);
        Assert.Equal(0, insect.Elevation);
        Assert.Equal(0, insect.WingSpread);
        insect.Update(0.02f, Context(simulation.Layout));
        Assert.Equal(LocomotionState.Walking, insect.MotionState);
    }

    [Theory]
    [InlineData(CreatureKind.Cricket)]
    [InlineData(CreatureKind.Grasshopper)]
    [InlineData(CreatureKind.Ladybug)]
    public void InvalidOrPausedUpdatesDoNotAdvanceMotionAndLargeDeltaIsClamped(CreatureKind kind)
    {
        var insect = new CrawlingInsect(new(300, 300), kind);
        AdvanceUntilAirborne(insect);
        var state = (insect.Position, insect.Rotation, insect.MotionState, insect.MotionProgress, insect.Elevation, insect.WingSpread, insect.AnimationPhase);
        var world = new SimulationWorld(new(0, 0, 2000, 2000), new MidpointRandom(), [insect]);
        foreach (var dt in new[] { 0, -1, float.NaN, float.PositiveInfinity, float.NegativeInfinity })
        {
            insect.Update(dt, Context() with { TotalTime = 100000, ElapsedSeconds = 100000 });
            world.Update(dt, Vector2.Zero);
            Assert.Equal(state, (insect.Position, insect.Rotation, insect.MotionState, insect.MotionProgress, insect.Elevation, insect.WingSpread, insect.AnimationPhase));
        }
        var reference = new CrawlingInsect(new(300, 300), kind);
        AdvanceUntilAirborne(reference);
        reference.Update(0.05f, Context());
        insect.Update(100000, Context() with { TotalTime = 100000, ElapsedSeconds = 100000 });
        Assert.Equal(state.Item3, insect.MotionState);
        Assert.Equal(reference.Position, insect.Position);
        Assert.Equal(reference.MotionProgress, insect.MotionProgress);
        Assert.Equal(reference.Elevation, insect.Elevation);
        Assert.Equal(reference.WingSpread, insect.WingSpread);
    }

    [Theory]
    [InlineData(CreatureKind.Ant)]
    [InlineData(CreatureKind.Caterpillar)]
    [InlineData(CreatureKind.GroundBeetle)]
    [InlineData(CreatureKind.Earwig)]
    [InlineData(CreatureKind.Silverfish)]
    [InlineData(CreatureKind.Mantis)]
    [InlineData(CreatureKind.StickInsect)]
    public void OtherWalkersStayOnGround(CreatureKind kind)
    {
        var insect = new CrawlingInsect(new(300, 300), kind);
        for (var i = 0; i < 1500; i++)
        {
            insect.Update(0.02f, Context());
            Assert.Equal(LocomotionState.Walking, insect.MotionState);
            Assert.Equal(0, insect.Elevation);
            Assert.Equal(0, insect.WingSpread);
        }
    }

    private static void AdvanceUntilAirborne(CrawlingInsect insect)
    {
        for (var i = 0; i < 1000 && insect.Elevation <= 0; i++) insect.Update(0.02f, Context());
        Assert.True(insect.Elevation > 0);
    }

    [Fact]
    public void LadybugOpensWingsBeforeLiftingAndFoldsOnlyAfterTouchdown()
    {
        var insect = new CrawlingInsect(new(300, 300), CreatureKind.Ladybug);
        var openedOnGround = false;
        var foldedOnGround = false;
        var sawLanding = false;
        for (var i = 0; i < 1500; i++)
        {
            insect.Update(0.01f, Context());
            if (insect.MotionState == LocomotionState.TakingOff && insect.Elevation == 0 && insect.WingSpread > 0)
                openedOnGround = true;
            if (insect.Elevation > 0) Assert.Equal(1, insect.WingSpread);
            if (insect.MotionState == LocomotionState.Landing)
            {
                sawLanding = true;
                if (insect.Elevation == 0 && insect.WingSpread is > 0 and < 1) foldedOnGround = true;
            }
            if (sawLanding && insect.MotionState == LocomotionState.Walking) break;
        }
        Assert.True(openedOnGround);
        Assert.True(foldedOnGround);
    }

    [Theory]
    [InlineData(CreatureKind.Cricket)]
    [InlineData(CreatureKind.Grasshopper)]
    public void JumpLandingSettlesWithoutSlidingAtAirborneSpeed(CreatureKind kind)
    {
        var insect = new CrawlingInsect(new(300, 300), kind);
        for (var i = 0; i < 1000 && insect.MotionState != LocomotionState.JumpLanding; i++)
            insect.Update(0.01f, Context());
        Assert.Equal(LocomotionState.JumpLanding, insect.MotionState);
        var touchdown = insect.Position;
        while (insect.MotionState == LocomotionState.JumpLanding)
        {
            insect.Update(0.01f, Context());
            Assert.InRange(insect.Velocity.Length(), 0, InsectCatalog.Get(kind).Speed + 0.01f);
        }
        Assert.InRange(Vector2.Distance(touchdown, insect.Position), 0, 5);
    }

    private static CreatureContext Context(DesktopLayout? layout = null, Vector2? mouse = null) =>
        new(new(mouse ?? new(-10000, -10000), Vector2.Zero, 0, false, TimeSpan.Zero),
            layout?.Bounds ?? new(0, 0, 2000, 2000), 0, new MidpointRandom(), Layout: layout);

    private sealed class MidpointRandom : IRandomSource
    {
        public float NextFloat(float min, float max) => (min + max) / 2;
    }
}
