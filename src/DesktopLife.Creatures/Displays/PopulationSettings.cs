namespace DesktopLife.Creatures.Displays;
public sealed record PopulationSettings(int Cockroaches = 20, int Ants = 20, int Caterpillars = 3)
{
    [System.Text.Json.Serialization.JsonIgnore]
    public int Flies => 1;
    public const int MaxCockroaches = 500;
    public const int MaxAnts = 500;
    public const int MaxCaterpillars = 100;
    public void Validate()
    {
        if (Cockroaches < 0 || Cockroaches > MaxCockroaches) throw new ArgumentOutOfRangeException(nameof(Cockroaches));
        if (Ants < 0 || Ants > MaxAnts) throw new ArgumentOutOfRangeException(nameof(Ants));
        if (Caterpillars < 0 || Caterpillars > MaxCaterpillars) throw new ArgumentOutOfRangeException(nameof(Caterpillars));
    }
}
