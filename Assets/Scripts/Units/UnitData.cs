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
    }
}
