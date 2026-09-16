namespace DesktopLife.Engine.Creatures;

public sealed record InsectDefinition(CreatureKind Kind, string ChineseName, string EnglishName,
    float BodyLength, float BodyWidth, float Speed, float Stride, int MaxCount = 100);

public static class InsectCatalog
{
    public static IReadOnlyList<InsectDefinition> Additional { get; } = Array.AsReadOnly<InsectDefinition>([
        new(CreatureKind.Ladybug, "瓢虫", "Ladybug", 10, 7, 22, 8),
        new(CreatureKind.GroundBeetle, "步甲", "Ground beetle", 22, 8, 42, 16),
        new(CreatureKind.Earwig, "蠼螋", "Earwig", 24, 5, 32, 16),
        new(CreatureKind.Silverfish, "衣鱼", "Silverfish", 18, 4, 55, 12),
        new(CreatureKind.Cricket, "蟋蟀", "Cricket", 26, 7, 32, 20),
        new(CreatureKind.Grasshopper, "蚱蜢", "Grasshopper", 38, 8, 26, 26),
        new(CreatureKind.Mantis, "螳螂", "Mantis", 60, 8, 12, 36),
        new(CreatureKind.StickInsect, "竹节虫", "Stick insect", 80, 4, 8, 40)
    ]);

    public static InsectDefinition Get(CreatureKind kind) =>
        Additional.FirstOrDefault(d => d.Kind == kind) ?? throw new ArgumentOutOfRangeException(nameof(kind));
}
