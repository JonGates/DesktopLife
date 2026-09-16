namespace DesktopLife.Engine.Creatures;

public static class OceanCatalog
{
    public static InsectDefinition Turtle { get; } = new(CreatureKind.GreenTurtle, "绿海龟", "Green turtle", 30, 20, 48, 36, 1);
    public static IReadOnlyList<InsectDefinition> Fish { get; } = Array.AsReadOnly<InsectDefinition>([
        new(CreatureKind.Clownfish, "小丑鱼", "Clownfish", 32, 18, 44, 20),
        new(CreatureKind.BlueTang, "蓝倒吊", "Blue tang", 42, 23, 64, 25),
        new(CreatureKind.YellowTang, "黄倒吊", "Yellow tang", 36, 26, 55, 23),
        new(CreatureKind.Butterflyfish, "蝴蝶鱼", "Butterflyfish", 38, 28, 38, 25),
        new(CreatureKind.Angelfish, "神仙鱼", "Angelfish", 45, 32, 34, 30),
        new(CreatureKind.Lionfish, "狮子鱼", "Lionfish", 48, 42, 24, 32),
        new(CreatureKind.Pufferfish, "河豚", "Pufferfish", 40, 30, 28, 24),
        new(CreatureKind.Seahorse, "海马", "Seahorse", 18, 36, 14, 12),
        new(CreatureKind.MandarinFish, "麒麟鱼", "Mandarin fish", 25, 15, 22, 15),
        new(CreatureKind.RoyalGramma, "皇家草莓鱼", "Royal gramma", 26, 13, 42, 18),
        new(CreatureKind.MoorishIdol, "镰鱼", "Moorish idol", 52, 34, 46, 28),
        new(CreatureKind.Wrasse, "六线隆头鱼", "Six-line wrasse", 40, 15, 72, 22)
    ]);
    public static bool IsOcean(CreatureKind kind) => kind == CreatureKind.GreenTurtle || Fish.Any(d => d.Kind == kind);
    public static InsectDefinition Get(CreatureKind kind) => kind == CreatureKind.GreenTurtle ? Turtle :
        Fish.FirstOrDefault(d => d.Kind == kind) ?? throw new ArgumentOutOfRangeException(nameof(kind));
}
