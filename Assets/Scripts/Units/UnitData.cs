using UnityEngine;

namespace LottoDefense.Units
{
    /// <summary>
    /// Attack pattern type for units.
    /// </summary>
    public enum AttackPattern
    {
        SingleTarget,   // 단일 대상 공격 (기본)
        Splash,         // 범위 공격 (주 대상 + 주변 적)
        AOE,            // 광역 공격 (범위 내 모든 적)
        Pierce,         // 관통 공격 (일직선상 모든 적)
        Chain           // 연쇄 공격 (대상에서 대상으로 튕김)
    }

    /// <summary>
    /// ScriptableObject that defines the data template for a unit type.
    /// Designers can create different unit variants by creating instances of this asset.
    /// All unit stats and visual references are configured here.
    /// </summary>
    [CreateAssetMenu(fileName = "NewUnit", menuName = "Lotto Defense/Unit Data", order = 1)]
    public class UnitData : ScriptableObject
    {
        [Header("Basic Information")]
        [Tooltip("Display name of the unit")]
        public string unitName = "New Unit";

        [Tooltip("Unit type determines combat behavior")]
        public UnitType type = UnitType.Melee;

        [Tooltip("Rarity tier affects stats and drop rate")]
        public Rarity rarity = Rarity.Normal;

        [Header("Combat Stats")]
        [Tooltip("Base attack damage per hit")]
        [Min(1)]
        public int attack = 10;

        [Tooltip("Defense value (damage reduction)")]
        [Min(0)]
        public int defense = 5;

        [Tooltip("Maximum attack range in grid units")]
        [Min(0.1f)]
        public float attackRange = 1.5f;

        [Tooltip("Attacks per second")]
        [Min(0.1f)]
        public float attackSpeed = 1.0f;

        [Header("Attack Pattern")]
        [Tooltip("Attack pattern type (Single, Splash, AOE, Pierce, Chain)")]
        public AttackPattern attackPattern = AttackPattern.SingleTarget;

        [Tooltip("Splash/AOE radius (0 = no splash, only for Splash/AOE patterns)")]
        [Min(0f)]
        public float splashRadius = 0f;

        [Tooltip("Max targets hit (for Pierce/Chain patterns, 0 = unlimited)")]
        [Min(0)]
        public int maxTargets = 1;

        [Tooltip("Damage falloff for splash (% of damage at edge, 100 = no falloff)")]
        [Range(0f, 100f)]
        public float splashDamageFalloff = 50f;

        [Header("Upgrade Settings")]
        [Tooltip("Base cost for first upgrade")]
        [Min(1)]
        public int baseUpgradeCost = 5;

        [Tooltip("Attack damage increase per upgrade level (%)")]
        [Range(0f, 100f)]
        public float attackUpgradePercent = 10f;

        [Tooltip("Attack speed increase per upgrade level (%)")]
        [Range(0f, 100f)]
        public float attackSpeedUpgradePercent = 8f;

        [Tooltip("Maximum upgrade level")]
        [Min(1)]
        public int maxUpgradeLevel = 10;

        [Header("Visual References")]
        [Tooltip("Icon displayed in UI and inventory")]
        public Sprite icon;

        [Tooltip("Prefab instantiated when unit is placed on grid")]
        public GameObject prefab;

        [Header("Skills")]
        [Tooltip("Unit's special skills/abilities")]
        public UnitSkill[] skills = new UnitSkill[0];

        [Header("Optional Metadata")]
        [TextArea(3, 5)]
        [Tooltip("Description text for UI display")]
        public string description = "";

        /// <summary>
        /// Validates that all required fields are properly configured.
        /// Called in Unity Editor to catch configuration errors early.
        /// </summary>
        private void OnValidate()
        {
            // Ensure positive stats
            attack = Mathf.Max(1, attack);
            defense = Mathf.Max(0, defense);
            attackRange = Mathf.Max(0.1f, attackRange);
            attackSpeed = Mathf.Max(0.1f, attackSpeed);

            // Validate attack pattern settings
            splashRadius = Mathf.Max(0f, splashRadius);
            maxTargets = Mathf.Max(0, maxTargets);
            splashDamageFalloff = Mathf.Clamp(splashDamageFalloff, 0f, 100f);

            // Auto-configure based on attack pattern
            if (attackPattern == AttackPattern.SingleTarget)
            {
                maxTargets = 1;
                splashRadius = 0f;
            }
            else if (attackPattern == AttackPattern.Splash || attackPattern == AttackPattern.AOE)
            {
                if (splashRadius <= 0f)
                {
                    splashRadius = 1.5f; // Default splash radius
                }
            }

            // Validate name is not empty
            if (string.IsNullOrWhiteSpace(unitName))
            {
                unitName = $"Unit_{type}_{rarity}";
            }
        }

        /// <summary>
        /// Returns a formatted display name with rarity prefix.
        /// Example: "[Epic] Shadow Archer"
        /// </summary>
        public string GetDisplayName()
        {
            return $"[{rarity}] {unitName}";
        }

        /// <summary>
        /// Calculates DPS (Damage Per Second) based on attack and attack speed.
        /// Useful for balancing and comparison.
        /// </summary>
        public float GetDPS()
        {
            return attack * attackSpeed;
        }

        /// <summary>
        /// Returns a summary string for debugging and logging.
        /// </summary>
        public override string ToString()
        {
            return $"{GetDisplayName()} - Type: {type}, ATK: {attack}, DEF: {defense}, Range: {attackRange}, AS: {attackSpeed}";
        }

        #region Static Visual Helpers
        /// <summary>
        /// Create a circle Sprite from a generated Texture2D.
        /// Used as placeholder visual when no icon/prefab is assigned.
        /// </summary>
        public static Sprite CreateCircleSprite(int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float center = size * 0.5f;
            float radius = center - 1f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center + 0.5f;
                    float dy = y - center + 0.5f;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    tex.SetPixel(x, y, dist <= radius ? Color.white : Color.clear);
                }
            }
            tex.Apply();
            tex.filterMode = FilterMode.Bilinear;

            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        /// <summary>
        /// Get a display color based on unit rarity.
        /// </summary>
        public static Color GetRarityColor(Rarity rarity)
        {
            switch (rarity)
            {
                case Rarity.Normal:    return new Color(0.6f, 0.8f, 1f);       // Light blue
                case Rarity.Rare:      return new Color(0.4f, 0.6f, 1f);       // Blue
                case Rarity.Epic:      return new Color(0.7f, 0.3f, 1f);       // Purple
                case Rarity.Legendary: return new Color(1f, 0.84f, 0f);        // Gold
                default:               return Color.white;
            }
        }
        #endregion
    }
}
