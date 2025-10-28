using UnityEngine;

namespace LottoDefense.Monsters
{
    /// <summary>
    /// Monster type classification for different behavioral patterns.
    /// </summary>
    public enum MonsterType
    {
        Normal,
        Fast,
        Tank,
        Boss
    }

    /// <summary>
    /// ScriptableObject containing all designer-editable monster statistics and properties.
    /// Used as templates for spawning monsters with specific characteristics.
    /// </summary>
    [CreateAssetMenu(fileName = "MonsterData", menuName = "Lotto Defense/Monster Data", order = 1)]
    public class MonsterData : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Display name for this monster type")]
        public string monsterName = "Monster";

        [Tooltip("Visual sprite for this monster")]
        public Sprite sprite;

        [Tooltip("Prefab used for instantiation (optional, can use generic)")]
        public GameObject prefab;

        [Tooltip("Monster classification type")]
        public MonsterType type = MonsterType.Normal;

        [Header("Combat Stats")]
        [Tooltip("Base maximum health points")]
        [Min(1)]
        public int maxHealth = 100;

        [Tooltip("Base attack power (for future features)")]
        [Min(0)]
        public int attack = 10;

        [Tooltip("Base defense value (damage reduction)")]
        [Min(0)]
        public int defense = 5;

        [Header("Movement")]
        [Tooltip("Movement speed in units per second")]
        [Min(0.1f)]
        public float moveSpeed = 2.0f;

        [Header("Rewards")]
        [Tooltip("Gold awarded when monster is killed")]
        [Min(1)]
        public int goldReward = 10;

        [Header("Scaling (Applied per round)")]
        [Tooltip("Health multiplier per round (e.g., 1.1 = +10% per round)")]
        [Min(1.0f)]
        public float healthScaling = 1.1f;

        [Tooltip("Defense multiplier per round")]
        [Min(1.0f)]
        public float defenseScaling = 1.05f;

        /// <summary>
        /// Calculate scaled health for a specific round.
        /// </summary>
        /// <param name="round">Current round number (1-based)</param>
        /// <returns>Scaled health value</returns>
        public int GetScaledHealth(int round)
        {
            if (round <= 1)
                return maxHealth;

            float multiplier = Mathf.Pow(healthScaling, round - 1);
            return Mathf.RoundToInt(maxHealth * multiplier);
        }

        /// <summary>
        /// Calculate scaled defense for a specific round.
        /// </summary>
        /// <param name="round">Current round number (1-based)</param>
        /// <returns>Scaled defense value</returns>
        public int GetScaledDefense(int round)
        {
            if (round <= 1)
                return defense;

            float multiplier = Mathf.Pow(defenseScaling, round - 1);
            return Mathf.RoundToInt(defense * multiplier);
        }

        /// <summary>
        /// Validate monster data on enable.
        /// </summary>
        private void OnValidate()
        {
            maxHealth = Mathf.Max(1, maxHealth);
            attack = Mathf.Max(0, attack);
            defense = Mathf.Max(0, defense);
            moveSpeed = Mathf.Max(0.1f, moveSpeed);
            goldReward = Mathf.Max(1, goldReward);
            healthScaling = Mathf.Max(1.0f, healthScaling);
            defenseScaling = Mathf.Max(1.0f, defenseScaling);
        }
    }
}
