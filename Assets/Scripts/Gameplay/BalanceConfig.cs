using UnityEngine;

namespace LottoDefense.Gameplay
{
    /// <summary>
    /// ScriptableObject containing all balance configuration for the game.
    /// Includes gold rewards, upgrade costs, difficulty scaling, and gacha probabilities.
    /// Easily adjustable by designers without code changes.
    /// </summary>
    [CreateAssetMenu(fileName = "BalanceConfig", menuName = "Lotto Defense/Balance Config", order = 1)]
    public class BalanceConfig : ScriptableObject
    {
        #region Gold Rewards
        [Header("Gold Rewards")]
        [Tooltip("Minimum gold per monster kill")]
        [SerializeField] private int minGoldPerMonster = 2;

        [Tooltip("Maximum gold per monster kill")]
        [SerializeField] private int maxGoldPerMonster = 5;

        [Tooltip("Gold scaling curve across rounds (X: round normalized 0-1, Y: multiplier)")]
        [SerializeField] private AnimationCurve goldScalingCurve = AnimationCurve.Linear(0f, 1f, 1f, 1.5f);
        #endregion

        #region Upgrade Costs
        [Header("Upgrade Costs")]
        [Tooltip("Base cost for upgrades (formula: base * level^exponent)")]
        [SerializeField] private float upgradeCostBase = 10f;

        [Tooltip("Exponent for upgrade cost scaling (higher = faster cost increase)")]
        [SerializeField] private float upgradeCostExponent = 1.5f;

        [Tooltip("Maximum upgrade level")]
        [SerializeField] private int maxUpgradeLevel = 10;

        [Tooltip("Attack multiplier per level (0.1 = 10% per level)")]
        [SerializeField] private float attackIncreasePerLevel = 0.1f;
        #endregion

        #region Difficulty Scaling
        [Header("Difficulty Scaling")]
        [Tooltip("Base HP multiplier for round 1")]
        [SerializeField] private float hpScalingBase = 1.0f;

        [Tooltip("Maximum HP multiplier for final round")]
        [SerializeField] private float hpScalingMax = 5.0f;

        [Tooltip("Base defense multiplier for round 1")]
        [SerializeField] private float defenseScalingBase = 1.0f;

        [Tooltip("Maximum defense multiplier for final round")]
        [SerializeField] private float defenseScalingMax = 3.0f;

        [Tooltip("HP scaling curve (X: round progress 0-1, Y: multiplier 0-1)")]
        [SerializeField] private AnimationCurve hpScalingCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Tooltip("Defense scaling curve (X: round progress 0-1, Y: multiplier 0-1)")]
        [SerializeField] private AnimationCurve defenseScalingCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        #endregion

        #region Gacha Probabilities
        [Header("Gacha Probabilities")]
        [Tooltip("Probability of Normal rarity (0-1)")]
        [Range(0f, 1f)]
        [SerializeField] private float commonProbability = 0.70f;    // 70%

        [Tooltip("Probability of Rare rarity (0-1)")]
        [Range(0f, 1f)]
        [SerializeField] private float uncommonProbability = 0.20f;  // 20%

        [Tooltip("Probability of Epic rarity (0-1)")]
        [Range(0f, 1f)]
        [SerializeField] private float rareProbability = 0.08f;      // 8%

        [Tooltip("Probability of Legendary rarity (0-1)")]
        [Range(0f, 1f)]
        [SerializeField] private float epicProbability = 0.02f;      // 2%
        #endregion

        #region Game Settings
        [Header("Game Settings")]
        [Tooltip("Total number of rounds to complete for victory")]
        [SerializeField] private int maxRounds = 30;

        [Tooltip("Starting gold amount")]
        [SerializeField] private int startingGold = 50;

        [Tooltip("Starting life points")]
        [SerializeField] private int startingLife = 10;
        #endregion

        #region Validation
        /// <summary>
        /// Validate configuration values in editor.
        /// </summary>
        private void OnValidate()
        {
            // Clamp gold values
            minGoldPerMonster = Mathf.Max(1, minGoldPerMonster);
            maxGoldPerMonster = Mathf.Max(minGoldPerMonster, maxGoldPerMonster);

            // Clamp upgrade values
            upgradeCostBase = Mathf.Max(1f, upgradeCostBase);
            upgradeCostExponent = Mathf.Max(1f, upgradeCostExponent);
            maxUpgradeLevel = Mathf.Clamp(maxUpgradeLevel, 1, 20);
            attackIncreasePerLevel = Mathf.Max(0f, attackIncreasePerLevel);

            // Clamp difficulty scaling
            hpScalingBase = Mathf.Max(1f, hpScalingBase);
            hpScalingMax = Mathf.Max(hpScalingBase, hpScalingMax);
            defenseScalingBase = Mathf.Max(1f, defenseScalingBase);
            defenseScalingMax = Mathf.Max(defenseScalingBase, defenseScalingMax);

            // Validate gacha probabilities sum to 1.0
            float totalProbability = commonProbability + uncommonProbability + rareProbability + epicProbability;
            if (Mathf.Abs(totalProbability - 1f) > 0.01f)
            {
                Debug.LogWarning($"[BalanceConfig] Gacha probabilities sum to {totalProbability:F2} instead of 1.0! Auto-normalizing...");
                NormalizeProbabilities();
            }

            // Clamp game settings
            maxRounds = Mathf.Max(1, maxRounds);
            startingGold = Mathf.Max(0, startingGold);
            startingLife = Mathf.Max(1, startingLife);
        }

        /// <summary>
        /// Normalize gacha probabilities to sum to 1.0.
        /// </summary>
        private void NormalizeProbabilities()
        {
            float total = commonProbability + uncommonProbability + rareProbability + epicProbability;
            if (total > 0f)
            {
                commonProbability /= total;
                uncommonProbability /= total;
                rareProbability /= total;
                epicProbability /= total;
            }
        }
        #endregion

        #region Gold Reward API
        /// <summary>
        /// Get gold reward for killing a monster in the specified round.
        /// </summary>
        /// <param name="roundNumber">Current round (1-based)</param>
        /// <returns>Random gold amount scaled by round</returns>
        public int GetGoldReward(int roundNumber)
        {
            // Base random gold
            int baseGold = Random.Range(minGoldPerMonster, maxGoldPerMonster + 1);

            // Apply round scaling
            float roundProgress = Mathf.Clamp01((float)(roundNumber - 1) / (maxRounds - 1));
            float scalingMultiplier = goldScalingCurve.Evaluate(roundProgress);

            int scaledGold = Mathf.RoundToInt(baseGold * scalingMultiplier);
            return Mathf.Max(1, scaledGold);
        }
        #endregion

        #region Upgrade Cost API
        /// <summary>
        /// Calculate the gold cost to upgrade from current level to next level.
        /// Formula: base * level^exponent
        /// </summary>
        /// <param name="currentLevel">Current upgrade level</param>
        /// <returns>Gold cost for upgrade</returns>
        public int GetUpgradeCost(int currentLevel)
        {
            if (currentLevel >= maxUpgradeLevel)
                return int.MaxValue; // Cannot upgrade further

            return Mathf.RoundToInt(upgradeCostBase * Mathf.Pow(currentLevel, upgradeCostExponent));
        }

        /// <summary>
        /// Calculate the attack multiplier for a given level.
        /// Formula: 1.0 + (increasePerLevel * (level - 1))
        /// </summary>
        /// <param name="level">Upgrade level</param>
        /// <returns>Attack multiplier</returns>
        public float GetAttackMultiplier(int level)
        {
            return 1.0f + (attackIncreasePerLevel * (level - 1));
        }

        /// <summary>
        /// Get total cost to upgrade from level 1 to max level.
        /// </summary>
        /// <returns>Total gold cost</returns>
        public int GetTotalUpgradeCost()
        {
            int total = 0;
            for (int level = 1; level < maxUpgradeLevel; level++)
            {
                total += GetUpgradeCost(level);
            }
            return total;
        }
        #endregion

        #region Difficulty Scaling API
        /// <summary>
        /// Get HP multiplier for the specified round.
        /// </summary>
        /// <param name="roundNumber">Current round (1-based)</param>
        /// <returns>HP multiplier</returns>
        public float GetHPMultiplier(int roundNumber)
        {
            float roundProgress = Mathf.Clamp01((float)(roundNumber - 1) / (maxRounds - 1));
            float curveValue = hpScalingCurve.Evaluate(roundProgress);
            return Mathf.Lerp(hpScalingBase, hpScalingMax, curveValue);
        }

        /// <summary>
        /// Get defense multiplier for the specified round.
        /// </summary>
        /// <param name="roundNumber">Current round (1-based)</param>
        /// <returns>Defense multiplier</returns>
        public float GetDefenseMultiplier(int roundNumber)
        {
            float roundProgress = Mathf.Clamp01((float)(roundNumber - 1) / (maxRounds - 1));
            float curveValue = defenseScalingCurve.Evaluate(roundProgress);
            return Mathf.Lerp(defenseScalingBase, defenseScalingMax, curveValue);
        }
        #endregion

        #region Gacha API
        /// <summary>
        /// Get gacha probabilities as array [Common, Uncommon, Rare, Epic].
        /// </summary>
        /// <returns>Probability array</returns>
        public float[] GetGachaProbabilities()
        {
            return new float[]
            {
                commonProbability,
                uncommonProbability,
                rareProbability,
                epicProbability
            };
        }

        /// <summary>
        /// Validate that gacha probabilities sum to 1.0.
        /// </summary>
        /// <returns>True if valid</returns>
        public bool ValidateProbabilities()
        {
            float total = commonProbability + uncommonProbability + rareProbability + epicProbability;
            return Mathf.Abs(total - 1f) < 0.01f;
        }
        #endregion

        #region Game Settings API
        /// <summary>
        /// Get maximum number of rounds.
        /// </summary>
        public int MaxRounds => maxRounds;

        /// <summary>
        /// Get starting gold amount.
        /// </summary>
        public int StartingGold => startingGold;

        /// <summary>
        /// Get starting life points.
        /// </summary>
        public int StartingLife => startingLife;

        /// <summary>
        /// Get maximum upgrade level.
        /// </summary>
        public int MaxUpgradeLevel => maxUpgradeLevel;
        #endregion

        #region Debug Utilities
        /// <summary>
        /// Generate a balance report for debugging.
        /// </summary>
        /// <returns>Formatted balance report string</returns>
        public string GenerateBalanceReport()
        {
            System.Text.StringBuilder report = new System.Text.StringBuilder();

            report.AppendLine("=== BALANCE CONFIGURATION REPORT ===");
            report.AppendLine();

            report.AppendLine("GOLD REWARDS:");
            report.AppendLine($"  Base Range: {minGoldPerMonster}-{maxGoldPerMonster} gold/monster");
            for (int round = 1; round <= maxRounds; round += 5)
            {
                int goldReward = GetGoldReward(round);
                report.AppendLine($"  Round {round}: ~{goldReward} gold/monster");
            }
            report.AppendLine();

            report.AppendLine("UPGRADE COSTS:");
            for (int level = 1; level <= maxUpgradeLevel && level <= 10; level++)
            {
                int cost = GetUpgradeCost(level);
                float multiplier = GetAttackMultiplier(level + 1);
                report.AppendLine($"  L{level}→L{level + 1}: {cost} gold (ATK x{multiplier:F2})");
            }
            report.AppendLine($"  Total L1→L{maxUpgradeLevel}: {GetTotalUpgradeCost()} gold");
            report.AppendLine();

            report.AppendLine("DIFFICULTY SCALING:");
            for (int round = 1; round <= maxRounds; round += 5)
            {
                float hpMult = GetHPMultiplier(round);
                float defMult = GetDefenseMultiplier(round);
                report.AppendLine($"  Round {round}: HP x{hpMult:F2}, DEF x{defMult:F2}");
            }
            report.AppendLine();

            report.AppendLine("GACHA PROBABILITIES:");
            report.AppendLine($"  Common: {commonProbability * 100f:F1}%");
            report.AppendLine($"  Uncommon: {uncommonProbability * 100f:F1}%");
            report.AppendLine($"  Rare: {rareProbability * 100f:F1}%");
            report.AppendLine($"  Epic: {epicProbability * 100f:F1}%");
            report.AppendLine($"  Total: {(commonProbability + uncommonProbability + rareProbability + epicProbability) * 100f:F1}%");
            report.AppendLine();

            report.AppendLine("GAME SETTINGS:");
            report.AppendLine($"  Max Rounds: {maxRounds}");
            report.AppendLine($"  Starting Gold: {startingGold}");
            report.AppendLine($"  Starting Life: {startingLife}");
            report.AppendLine($"  Max Upgrade Level: {maxUpgradeLevel}");

            return report.ToString();
        }
        #endregion
    }
}
