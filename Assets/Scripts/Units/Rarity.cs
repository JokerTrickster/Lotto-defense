namespace LottoDefense.Units
{
    /// <summary>
    /// Defines the four rarity tiers for units in the gacha system.
    /// Rarity affects base stats, drop rates, and visual presentation.
    /// </summary>
    public enum Rarity
    {
        /// <summary>
        /// Common units with basic stats.
        /// Drop rate: 50%
        /// </summary>
        Normal,

        /// <summary>
        /// Uncommon units with improved stats.
        /// Drop rate: 30%
        /// </summary>
        Rare,

        /// <summary>
        /// High-tier units with strong stats.
        /// Drop rate: 15%
        /// </summary>
        Epic,

        /// <summary>
        /// Premium units with exceptional stats.
        /// Drop rate: 5%
        /// </summary>
        Legendary
    }
}
