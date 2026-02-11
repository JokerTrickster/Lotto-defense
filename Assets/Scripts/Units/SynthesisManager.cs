using UnityEngine;
using System.Collections.Generic;
using LottoDefense.Gameplay;
using LottoDefense.Grid;

namespace LottoDefense.Units
{
    /// <summary>
    /// Manages unit synthesis: combining 3 same units into 1 upgraded unit.
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
        /// Try to synthesize units based on the selected unit.
        /// Requires 3 units with the same name to synthesize.
        /// </summary>
        /// <param name="selectedUnit">The unit that was clicked for synthesis</param>
        /// <returns>True if synthesis succeeded, false otherwise</returns>
        public bool TrySynthesize(Unit selectedUnit)
        {
            if (selectedUnit == null || selectedUnit.Data == null)
            {
                Debug.LogWarning("[SynthesisManager] Cannot synthesize null unit");
                return false;
            }

            if (balanceConfig == null)
            {
                Debug.LogError("[SynthesisManager] GameBalanceConfig not loaded!");
                return false;
            }

            // Get synthesis recipe
            var recipe = balanceConfig.GetSynthesisRecipe(selectedUnit.Data.unitName);
            if (recipe == null)
            {
                Debug.LogWarning($"[SynthesisManager] No synthesis recipe for {selectedUnit.Data.unitName}");
                return false;
            }

            // Find all units with the same name
            List<Unit> sameUnits = FindSameUnits(selectedUnit.Data.unitName);

            if (sameUnits.Count < 3)
            {
                Debug.LogWarning($"[SynthesisManager] Not enough units for synthesis: {sameUnits.Count}/3");
                return false;
            }

            // Get result unit data
            UnitData resultData = Resources.Load<UnitData>($"UnitData/{recipe.resultUnitName}");
            if (resultData == null)
            {
                Debug.LogError($"[SynthesisManager] Result unit data not found: {recipe.resultUnitName}");
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
            PerformSynthesis(sameUnits, resultData);

            Debug.Log($"[SynthesisManager] Synthesis successful: {selectedUnit.Data.unitName} × 3 → {recipe.resultUnitName}");
            return true;
        }

        /// <summary>
        /// Find all units with the specified name.
        /// </summary>
        private List<Unit> FindSameUnits(string unitName)
        {
            if (UnitManager.Instance == null) return new List<Unit>();

            var placedUnits = UnitManager.Instance.GetPlacedUnits();
            List<Unit> sameUnits = new List<Unit>();

            foreach (var unit in placedUnits)
            {
                if (unit != null && unit.Data != null && unit.Data.unitName == unitName)
                {
                    sameUnits.Add(unit);
                }
            }

            return sameUnits;
        }

        /// <summary>
        /// Perform the actual synthesis: remove 3 units, create 1 result unit.
        /// </summary>
        private void PerformSynthesis(List<Unit> sourceUnits, UnitData resultData)
        {
            if (sourceUnits.Count < 3) return;

            // Get position of first unit to place result unit
            Vector2Int resultPosition = sourceUnits[0].GridPosition;

            // Remove all 3 source units
            for (int i = 0; i < 3; i++)
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
