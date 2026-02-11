using UnityEngine;
using System.Collections.Generic;
using LottoDefense.Gameplay;
using LottoDefense.Grid;

namespace LottoDefense.Units
{
    /// <summary>
    /// Manages unit synthesis: combining 2 same units into 1 upgraded unit.
    /// Uses GameBalanceConfig recipes for synthesis rules.
    /// </summary>
    public class SynthesisManager : MonoBehaviour
    {
        #region Singleton
        private static SynthesisManager _instance;

        /// <summary>
        /// Global access point for the SynthesisManager singleton.
        /// </summary>
        public static SynthesisManager Instance
        {
            get
            {
                if (GameplayManager.IsCleaningUp) return null;

                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<SynthesisManager>();

                    if (_instance == null)
                    {
                        GameObject go = new GameObject("SynthesisManager");
                        _instance = go.AddComponent<SynthesisManager>();
                    }
                }
                return _instance;
            }
        }
        #endregion

        #region Private Fields
        private GameBalanceConfig balanceConfig;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            Initialize();
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize SynthesisManager with balance configuration.
        /// </summary>
        private void Initialize()
        {
            // Load balance config
            balanceConfig = Resources.Load<GameBalanceConfig>("GameBalanceConfig");
            if (balanceConfig == null)
            {
                Debug.LogError("[SynthesisManager] GameBalanceConfig not found in Resources!");
            }

            Debug.Log("[SynthesisManager] Initialized");
        }
        #endregion

        #region Synthesis Logic
        /// <summary>
        /// Try to synthesize units based on two selected units.
        /// Requires 2 units with the same name to synthesize.
        /// </summary>
        /// <param name="unit1">First unit for synthesis</param>
        /// <param name="unit2">Second unit for synthesis</param>
        /// <returns>True if synthesis succeeded, false otherwise</returns>
        public bool TrySynthesize(Unit unit1, Unit unit2)
        {
            if (unit1 == null || unit1.Data == null || unit2 == null || unit2.Data == null)
            {
                Debug.LogWarning("[SynthesisManager] Cannot synthesize null units");
                return false;
            }

            if (balanceConfig == null)
            {
                Debug.LogError("[SynthesisManager] GameBalanceConfig not loaded!");
                return false;
            }

            // Both units must be the same type
            if (unit1.Data.unitName != unit2.Data.unitName)
            {
                Debug.LogWarning($"[SynthesisManager] Units must be same type: {unit1.Data.unitName} != {unit2.Data.unitName}");
                return false;
            }

            // Get synthesis recipe
            var recipe = balanceConfig.GetSynthesisRecipe(unit1.Data.unitName);
            if (recipe == null)
            {
                Debug.LogWarning($"[SynthesisManager] No synthesis recipe for {unit1.Data.unitName}");
                return false;
            }

            // Get result unit data
            UnitData resultData = Resources.Load<UnitData>($"Units/{recipe.resultUnitName}");
            if (resultData == null)
            {
                Debug.LogError($"[SynthesisManager] Result unit data not found: Units/{recipe.resultUnitName}");
                return false;
            }

            // Check gold cost (if any)
            if (recipe.synthesisGoldCost > 0)
            {
                if (GameplayManager.Instance == null || GameplayManager.Instance.CurrentGold < recipe.synthesisGoldCost)
                {
                    Debug.LogWarning($"[SynthesisManager] Not enough gold: {recipe.synthesisGoldCost} required");
                    return false;
                }

                // Deduct gold
                GameplayManager.Instance.ModifyGold(-recipe.synthesisGoldCost);
            }

            // Perform synthesis
            List<Unit> sourceUnits = new List<Unit> { unit1, unit2 };
            PerformSynthesis(sourceUnits, resultData);

            Debug.Log($"[SynthesisManager] Synthesis successful: {unit1.Data.unitName} x2 → {recipe.resultUnitName}");
            return true;
        }

        /// <summary>
        /// Perform the actual synthesis: remove 2 units, create 1 result unit.
        /// </summary>
        private void PerformSynthesis(List<Unit> sourceUnits, UnitData resultData)
        {
            if (sourceUnits.Count < 2) return;

            // Get position of first unit to place result unit
            Vector2Int resultPosition = sourceUnits[0].GridPosition;

            // Remove all source units
            for (int i = 0; i < sourceUnits.Count; i++)
            {
                Unit unit = sourceUnits[i];

                // Remove from grid
                if (GridManager.Instance != null)
                {
                    GridManager.Instance.RemoveUnit(unit.GridPosition);
                }

                // Destroy GameObject
                Destroy(unit.gameObject);
            }

            // Create result unit at the first unit's position
            if (UnitManager.Instance != null)
            {
                UnitManager.Instance.PlaceUnit(resultData, resultPosition);
                Debug.Log($"[SynthesisManager] Created {resultData.unitName} at {resultPosition}");
            }
        }
        #endregion

        #region Debugging
        /// <summary>
        /// Get synthesis statistics for debugging.
        /// </summary>
        public string GetStats()
        {
            if (balanceConfig == null) return "No config loaded";

            int recipeCount = 0;
            foreach (var recipe in balanceConfig.synthesisRecipes)
            {
                if (recipe != null) recipeCount++;
            }

            return $"Loaded {recipeCount} synthesis recipes";
        }
        #endregion
    }
}
