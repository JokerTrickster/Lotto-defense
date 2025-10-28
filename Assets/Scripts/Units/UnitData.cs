using UnityEngine;

namespace LottoDefense.Units
{
    /// <summary>
    /// Unit type categories for combat behavior.
    /// </summary>
    public enum UnitType
    {
        Melee,
        Ranged,
        Debuffer
    }

    /// <summary>
    /// Unit rarity tiers for gacha system.
    /// </summary>
    public enum UnitRarity
    {
        Normal,
        Rare,
        Epic,
        Legendary
    }

    /// <summary>
    /// ScriptableObject template defining unit stats and properties.
    /// Used for gacha system and unit instantiation.
    /// </summary>
    [CreateAssetMenu(fileName = "NewUnit", menuName = "Lotto Defense/Unit Data", order = 1)]
    public class UnitData : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Display name of the unit")]
        public string unitName = "New Unit";

        [Tooltip("Unit type determining combat behavior")]
        public UnitType unitType = UnitType.Melee;

        [Tooltip("Rarity tier affecting gacha probability")]
        public UnitRarity rarity = UnitRarity.Normal;

        [Header("Combat Stats")]
        [Tooltip("Attack damage value")]
        [Min(1)]
        public int attack = 10;

        [Tooltip("Defense value reducing incoming damage")]
        [Min(0)]
        public int defense = 5;

        [Tooltip("Attack range in grid cells (Melee: 1, Ranged: 3, Debuffer: 2)")]
        [Min(0.5f)]
        public float attackRange = 1.0f;

        [Tooltip("Attacks per second")]
        [Min(0.1f)]
        public float attackSpeed = 1.0f;

        [Header("Visuals")]
        [Tooltip("Unit icon for UI")]
        public Sprite icon;

        [Tooltip("Unit prefab for grid placement")]
        public GameObject prefab;

        [Header("Gacha Probability")]
        [Tooltip("Weight for gacha selection (Normal:50, Rare:30, Epic:15, Legendary:5)")]
        [Min(1)]
        public int gachaWeight = 50;

        #region Validation
        private void OnValidate()
        {
            // Ensure stats are positive
            attack = Mathf.Max(1, attack);
            defense = Mathf.Max(0, defense);
            attackRange = Mathf.Max(0.5f, attackRange);
            attackSpeed = Mathf.Max(0.1f, attackSpeed);

            // Auto-set gacha weights based on rarity
            switch (rarity)
            {
                case UnitRarity.Normal:
                    gachaWeight = 50;
                    break;
                case UnitRarity.Rare:
                    gachaWeight = 30;
                    break;
                case UnitRarity.Epic:
                    gachaWeight = 15;
                    break;
                case UnitRarity.Legendary:
                    gachaWeight = 5;
                    break;
            }
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Get the cooldown time between attacks in seconds.
        /// </summary>
        public float GetAttackCooldown()
        {
            return 1f / attackSpeed;
        }

        /// <summary>
        /// Get color associated with rarity tier.
        /// </summary>
        public Color GetRarityColor()
        {
            switch (rarity)
            {
                case UnitRarity.Normal:
                    return Color.white;
                case UnitRarity.Rare:
                    return new Color(0.2f, 0.6f, 1f); // Blue
                case UnitRarity.Epic:
                    return new Color(0.6f, 0.2f, 1f); // Purple
                case UnitRarity.Legendary:
                    return new Color(1f, 0.8f, 0.2f); // Gold
                default:
                    return Color.white;
            }
        }
        #endregion
    }
}
