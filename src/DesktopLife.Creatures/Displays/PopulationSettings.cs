namespace DesktopLife.Creatures.Displays;
public sealed record PopulationSettings(int Flies = 1, int Cockroaches = 20)
{
    public const int MaxFlies = 20;
    public const int MaxCockroaches = 500;
    public void Validate()
    {
        if (Flies < 0 || Flies > MaxFlies) throw new ArgumentOutOfRangeException(nameof(Flies));
        if (Cockroaches < 0 || Cockroaches > MaxCockroaches) throw new ArgumentOutOfRangeException(nameof(Cockroaches));
    }
}
