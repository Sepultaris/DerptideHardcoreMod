namespace DerptideHardcore
{
    public class Settings
    {
        public bool TokenDrop { get; set; } = true;

        public bool HcPointSystemEnabled { get; set; } = true;

        public bool DeleteHcToons { get; set; } = true;

        public float TokenDropChance { get; set; } = 0.3f;

        public uint TokenWCID { get; set; } = 420711;

        public uint HalfLifeWCID { get; set; } = 420712;

        public float HardcoreDamageBonus { get; set; } = 0.15f;

        public float HcMobDamageBoost { get; set; } = 0.3f;
    }
}


