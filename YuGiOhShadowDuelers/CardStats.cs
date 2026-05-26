namespace YuGiOhShadowDuelers
{
    public enum CardType { SliferTheSkyDragon, WingedDragonOfRa, DarkMagician, DarkMagicianGirl }

    public class CardStats
    {
        public string Name         { get; set; }
        public string Description  { get; set; }
        public int    MaxHP        { get; set; }
        public int    DamageMin    { get; set; }
        public int    DamageMax    { get; set; }
        public double DodgeChance  { get; set; }   // 0.0 - 1.0
        public string SpritePrefix { get; set; }   // "slifer", "ra", "dm", "dmg"

        // Kaiba is always the same (machine)
        public static readonly CardStats Kaiba = new()
        {
            Name         = "Seto Kaiba",
            MaxHP        = 1000,
            DamageMin    = 10,
            DamageMax    = 120,
            DodgeChance  = 0.20,
            SpritePrefix = "kaiba"
        };

        // ── PLAYER CARDS ──────────────────────────────────────────────────────
        // Rules: player may have more DamageMax than Kaiba but LESS dodge chance
        // All players start at 1000 HP per professor requirements

        public static readonly CardStats Slifer = new()
        {
            Name         = "Slifer el Dragón del Cielo",
            Description  = "¡Poder del rayo! Daño devastador pero menos evasión.",
            MaxHP        = 1000,
            DamageMin    = 10,
            DamageMax    = 130,   // más daño que Kaiba
            DodgeChance  = 0.15,  // menos dodge que Kaiba (20%)
            SpritePrefix = "slifer"
        };

        public static readonly CardStats Ra = new()
        {
            Name         = "El Dragón Alado de Ra",
            Description  = "Fuerza divina equilibrada. Buen daño y resistencia.",
            MaxHP        = 1000,
            DamageMin    = 15,
            DamageMax    = 125,   // más daño que Kaiba
            DodgeChance  = 0.12,  // menos dodge que Kaiba
            SpritePrefix = "ra"
        };

        public static readonly CardStats DarkMagician = new()
        {
            Name         = "Mago Oscuro",
            Description  = "Magia antigua. Daño alto y magia oscura.",
            MaxHP        = 1000,
            DamageMin    = 10,
            DamageMax    = 135,   // mayor daño
            DodgeChance  = 0.10,  // menor dodge
            SpritePrefix = "dm"
        };

        public static readonly CardStats DarkMagicianGirl = new()
        {
            Name         = "Maga Oscura",
            Description  = "Veloz y mágica. Daño moderado pero más golpes seguros.",
            MaxHP        = 1000,
            DamageMin    = 20,
            DamageMax    = 122,   // más daño que Kaiba
            DodgeChance  = 0.18,  // menor dodge que Kaiba
            SpritePrefix = "dmg"
        };

        public static CardStats FromCardType(CardType t) => t switch
        {
            CardType.SliferTheSkyDragon    => Slifer,
            CardType.WingedDragonOfRa      => Ra,
            CardType.DarkMagician          => DarkMagician,
            CardType.DarkMagicianGirl      => DarkMagicianGirl,
            _                              => Slifer
        };
    }
}
