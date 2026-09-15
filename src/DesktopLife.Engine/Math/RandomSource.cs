namespace DesktopLife.Engine.Math;
public interface IRandomSource { float NextFloat(float min, float max); }
public sealed class RandomSource(int seed) : IRandomSource
{
    private readonly Random _random = new(seed);
    public float NextFloat(float min, float max) => min + (max - min) * _random.NextSingle();
}
