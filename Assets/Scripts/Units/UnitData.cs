using UnityEngine;

namespace LottoDefense.Units
{
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

        [Header("Visual References")]
        [Tooltip("Icon displayed in UI and inventory")]
        public Sprite icon;

        [Tooltip("Prefab instantiated when unit is placed on grid")]
        public GameObject prefab;

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
