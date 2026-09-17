using DesktopLife.Engine.Creatures;

namespace DesktopLife.Creatures.Displays;

public sealed record SpeciesPopulation(int Count = 0, int MinPercent = 80, int MaxPercent = 120);

public sealed record PopulationSettings(int Cockroaches = 20, int Ants = 20, int Caterpillars = 3, int RoachMin = 60, int RoachMax = 180, int AntMin = 60, int AntMax = 120, int CaterpillarMin = 60, int CaterpillarMax = 140, Dictionary<CreatureKind, SpeciesPopulation>? Additional = null, Habitat Habitat = Habitat.Forest, Dictionary<CreatureKind, SpeciesPopulation>? Ocean = null, int RainLevel = 3)
{
    public SpeciesPopulation GetAdditional(CreatureKind kind) =>
        Additional is not null && Additional.TryGetValue(kind, out var value) ? value : new();
    public SpeciesPopulation GetOcean(CreatureKind kind) =>
        Ocean is not null && Ocean.TryGetValue(kind, out var value) ? value : new(2, 80, 120);
    [System.Text.Json.Serialization.JsonIgnore]
    public int Flies => Habitat == Habitat.Forest ? 1 : 0;
    public const int MaxCockroaches = 500;
    public const int MaxAnts = 500;
    public const int MaxCaterpillars = 100;
    public static void ValidateRange(int min, int max)
    {
        if (min < 10 || max > 300 || min > max) throw new ArgumentOutOfRangeException(nameof(min), "Size must be 10–300%, minimum <= maximum.");
    }
    public void Validate()
    {
        if (RainLevel is < 1 or > 5) throw new ArgumentOutOfRangeException(nameof(RainLevel));
        if (!Enum.IsDefined(Habitat)) throw new ArgumentOutOfRangeException(nameof(Habitat));
        ValidateRange(RoachMin, RoachMax);
        ValidateRange(AntMin, AntMax);
        ValidateRange(CaterpillarMin, CaterpillarMax);
        if (Cockroaches < 0 || Cockroaches > MaxCockroaches) throw new ArgumentOutOfRangeException(nameof(Cockroaches));
        if (Ants < 0 || Ants > MaxAnts) throw new ArgumentOutOfRangeException(nameof(Ants));
        if (Caterpillars < 0 || Caterpillars > MaxCaterpillars) throw new ArgumentOutOfRangeException(nameof(Caterpillars));
        foreach (var (kind, value) in Additional ?? [])
        {
            var definition = InsectCatalog.Get(kind);
            if (value is null) throw new ArgumentException("Species population cannot be null.", nameof(Additional));
            if (value.Count < 0 || value.Count > definition.MaxCount) throw new ArgumentOutOfRangeException(nameof(Additional));
            ValidateRange(value.MinPercent, value.MaxPercent);
        }
        foreach (var (kind, value) in Ocean ?? [])
        {
            if (!OceanCatalog.Fish.Any(d => d.Kind == kind)) throw new ArgumentOutOfRangeException(nameof(Ocean));
            if (value is null) throw new ArgumentException("Species population cannot be null.", nameof(Ocean));
            if (value.Count < 0 || value.Count > 100) throw new ArgumentOutOfRangeException(nameof(Ocean));
            ValidateRange(value.MinPercent, value.MaxPercent);
        }
    }
}
