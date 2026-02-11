using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using LottoDefense.Gameplay;
using LottoDefense.Grid;

namespace LottoDefense.Units
{
    /// <summary>
    /// Automatically detects and synthesizes units when 3 or more of the same type are present.
    /// Provides UI button for triggering auto-synthesis.
    /// </summary>
    public class AutoSynthesisManager : MonoBehaviour
    {
        #region Singleton
        private static AutoSynthesisManager _instance;

        public static AutoSynthesisManager Instance
        {
            get
            {
                if (GameplayManager.IsCleaningUp) return null;

                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<AutoSynthesisManager>();

                    if (_instance == null)
                    {
                        GameObject go = new GameObject("AutoSynthesisManager");
                        _instance = go.AddComponent<AutoSynthesisManager>();
                    }
                }
                return _instance;
            }
        }
        #endregion

        #region Private Fields
        private GameBalanceConfig balanceConfig;
        #endregion

        #region Events
        /// <summary>
        /// Fired when auto-synthesis is performed.
        /// Parameters: unitName, synthesisCount
        /// </summary>
        public event System.Action<string, int> OnAutoSynthesis;
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
        private void Initialize()
        {
            balanceConfig = Resources.Load<GameBalanceConfig>("GameBalanceConfig");
            if (balanceConfig == null)
            {
                Debug.LogError("[AutoSynthesisManager] GameBalanceConfig not found!");
            }

            Debug.Log("[AutoSynthesisManager] Initialized");
        }
        #endregion

        #region Auto Synthesis
        /// <summary>
        /// Scan all placed units and automatically synthesize groups of 2 same units.
        /// </summary>
        /// <returns>Number of synthesis operations performed</returns>
        public int PerformAutoSynthesis()
        {
            if (balanceConfig == null)
            {
                Debug.LogError("[AutoSynthesisManager] GameBalanceConfig not loaded!");
                return 0;
            }

            // Check if we're in Preparation phase
            if (GameplayManager.Instance == null || GameplayManager.Instance.CurrentState != GameState.Preparation)
            {
                Debug.LogWarning("[AutoSynthesisManager] Auto-synthesis only allowed during Preparation phase!");
                return 0;
            }

            // Get all placed units
            if (UnitManager.Instance == null)
            {
                Debug.LogWarning("[AutoSynthesisManager] UnitManager not found!");
                return 0;
            }

            var placedUnits = UnitManager.Instance.GetPlacedUnits();
            if (placedUnits == null || placedUnits.Count == 0)
            {
                Debug.Log("[AutoSynthesisManager] No units to synthesize");
                return 0;
            }

            // Group units by name and upgrade level
            Dictionary<string, List<Unit>> unitGroups = new Dictionary<string, List<Unit>>();
            foreach (var unit in placedUnits)
            {
                if (unit == null || unit.Data == null) continue;

                // Key: "unitName_level" to group same units with same upgrade level
                string key = $"{unit.Data.unitName}_{unit.UpgradeLevel}";

                if (!unitGroups.ContainsKey(key))
                {
                    unitGroups[key] = new List<Unit>();
                }
                unitGroups[key].Add(unit);
            }

            int totalSynthesisCount = 0;

            // Process each group
            foreach (var group in unitGroups)
            {
                string unitName = group.Key.Split('_')[0];
                List<Unit> units = group.Value;

                // Need at least 2 units to synthesize
                while (units.Count >= 2)
                {
                    // Check if recipe exists
                    var recipe = balanceConfig.GetSynthesisRecipe(unitName);
                    if (recipe == null)
                    {
                        Debug.Log($"[AutoSynthesisManager] No recipe for {unitName}, skipping");
                        break;
                    }

                    // Check gold cost
                    if (GameplayManager.Instance != null && recipe.synthesisGoldCost > 0)
                    {
                        if (GameplayManager.Instance.CurrentGold < recipe.synthesisGoldCost)
                        {
                            Debug.LogWarning($"[AutoSynthesisManager] Not enough gold for {unitName} synthesis (need {recipe.synthesisGoldCost})");
                            break;
                        }
                    }

                    // Take first 2 units
                    Unit unit1 = units[0];
                    Unit unit2 = units[1];

                    // Try synthesis using SynthesisManager
                    if (SynthesisManager.Instance != null)
                    {
                        bool success = SynthesisManager.Instance.TrySynthesize(unit1, unit2);
                        if (success)
                        {
                            totalSynthesisCount++;
                            Debug.Log($"[AutoSynthesisManager] Auto-synthesized {unitName} → {recipe.resultUnitName}");

                            // Remove synthesized units from list
                            units.Remove(unit1);
                            units.Remove(unit2);

                            // Fire event
                            OnAutoSynthesis?.Invoke(unitName, totalSynthesisCount);
                        }
                        else
                        {
                            Debug.LogWarning($"[AutoSynthesisManager] Synthesis failed for {unitName}");
                            break;
                        }
                    }
                    else
                    {
                        Debug.LogError("[AutoSynthesisManager] SynthesisManager not found!");
                        break;
                    }
                }
            }

            if (totalSynthesisCount > 0)
            {
                Debug.Log($"[AutoSynthesisManager] Performed {totalSynthesisCount} auto-synthesis operations");
            }
            else
            {
                Debug.Log("[AutoSynthesisManager] No units eligible for auto-synthesis");
            }

            return totalSynthesisCount;
        }

        /// <summary>
        /// Check if auto-synthesis is possible (returns count of possible synthesis operations).
        /// </summary>
        public int GetPossibleSynthesisCount()
        {
            if (UnitManager.Instance == null || balanceConfig == null) return 0;

            var placedUnits = UnitManager.Instance.GetPlacedUnits();
            if (placedUnits == null || placedUnits.Count == 0) return 0;

            // Group units by name and level
            Dictionary<string, int> unitCounts = new Dictionary<string, int>();
            foreach (var unit in placedUnits)
            {
                if (unit == null || unit.Data == null) continue;

                string key = $"{unit.Data.unitName}_{unit.UpgradeLevel}";

                if (!unitCounts.ContainsKey(key))
                {
                    unitCounts[key] = 0;
                }
                unitCounts[key]++;
            }

            int totalPossible = 0;
            foreach (var entry in unitCounts)
            {
                string unitName = entry.Key.Split('_')[0];
                int count = entry.Value;

                // Check if recipe exists
                var recipe = balanceConfig.GetSynthesisRecipe(unitName);
                if (recipe != null)
                {
                    totalPossible += count / 2; // Integer division
                }
            }

            return totalPossible;
        }
        #endregion
    }
}
