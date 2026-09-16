namespace DesktopLife.Creatures.Displays;
public sealed record PopulationSettings(int Cockroaches = 20, int Ants = 20, int Caterpillars = 3, int RoachMin = 60, int RoachMax = 180, int AntMin = 60, int AntMax = 120, int CaterpillarMin = 60, int CaterpillarMax = 140)
{
    [System.Text.Json.Serialization.JsonIgnore]
    public int Flies => 1;
    public const int MaxCockroaches = 500;
    public const int MaxAnts = 500;
    public const int MaxCaterpillars = 100;
    public static void ValidateRange(int min, int max)
    {
        if (min < 10 || max > 300 || min > max) throw new ArgumentOutOfRangeException(nameof(min), "Size must be 10–300%, minimum <= maximum.");
    }
    public void Validate()
    {
        ValidateRange(RoachMin, RoachMax);
        ValidateRange(AntMin, AntMax);
        ValidateRange(CaterpillarMin, CaterpillarMax);
        if (Cockroaches < 0 || Cockroaches > MaxCockroaches) throw new ArgumentOutOfRangeException(nameof(Cockroaches));
        if (Ants < 0 || Ants > MaxAnts) throw new ArgumentOutOfRangeException(nameof(Ants));
        if (Caterpillars < 0 || Caterpillars > MaxCaterpillars) throw new ArgumentOutOfRangeException(nameof(Caterpillars));
    }
}
