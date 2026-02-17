using UnityEngine;

namespace LottoDefense.Gameplay
{
    /// <summary>
    /// Difficulty multipliers for HP and Defense stats at a specific round.
    /// </summary>
    [System.Serializable]
    public struct DifficultyMultipliers
    {
        public float hpMultiplier;
        public float defenseMultiplier;
    }

    /// <summary>
    /// ScriptableObject defining difficulty progression curve across all rounds (default 5).
    /// Uses AnimationCurves for flexible, tunable difficulty scaling.
    /// </summary>
    [CreateAssetMenu(fileName = "DifficultyConfig", menuName = "Lotto Defense/Difficulty Config", order = 50)]
    public class DifficultyConfig : ScriptableObject
    {
        #region Inspector Fields
        [Header("Difficulty Curves")]
        [Tooltip("HP multiplier curve from round 1 to 30")]
        [SerializeField] private AnimationCurve hpCurve = AnimationCurve.Linear(0f, 1f, 1f, 5f);

        [Tooltip("Defense multiplier curve from round 1 to 30")]
        [SerializeField] private AnimationCurve defenseCurve = AnimationCurve.Linear(0f, 1f, 1f, 3f);

        [Header("Base Multipliers")]
        [Tooltip("Base HP multiplier applied to curve evaluation")]
        [SerializeField] private float baseHpMultiplier = 1.0f;

        [Tooltip("Base Defense multiplier applied to curve evaluation")]
        [SerializeField] private float baseDefenseMultiplier = 1.0f;

        [Header("Round Settings")]
        [Tooltip("Maximum number of rounds in the game")]
        [SerializeField] private int maxRounds = 5;
        #endregion

        #region Properties
        /// <summary>
        /// Maximum number of rounds in the game.
        /// </summary>
        public int MaxRounds => maxRounds;
        #endregion

        #region Public Methods
        /// <summary>
        /// Get difficulty multipliers for a specific round.
        /// </summary>
        /// <param name="round">Round number (1-based)</param>
        /// <returns>Difficulty multipliers for HP and Defense</returns>
        public DifficultyMultipliers GetMultipliersForRound(int round)
        {
            // Clamp round to valid range
            int clampedRound = Mathf.Clamp(round, 1, maxRounds);

            // Normalize round to 0-1 range for curve evaluation
            float normalizedRound = maxRounds > 1 ? (clampedRound - 1f) / (maxRounds - 1f) : 0f;

            return new DifficultyMultipliers
            {
                hpMultiplier = baseHpMultiplier * hpCurve.Evaluate(normalizedRound),
                defenseMultiplier = baseDefenseMultiplier * defenseCurve.Evaluate(normalizedRound)
            };
        }

        /// <summary>
        /// Preview multipliers for all rounds (useful for debugging).
        /// </summary>
        public void LogDifficultyProgression()
        {
            for (int i = 1; i <= maxRounds; i++)
            {
                var multipliers = GetMultipliersForRound(i);
            }
        }
        #endregion

        #region Validation
        /// <summary>
        /// Validate configuration values.
        /// </summary>
        private void OnValidate()
        {
            baseHpMultiplier = Mathf.Max(0.1f, baseHpMultiplier);
            baseDefenseMultiplier = Mathf.Max(0.1f, baseDefenseMultiplier);
            maxRounds = Mathf.Max(1, maxRounds);
        }
        #endregion
    }
}
