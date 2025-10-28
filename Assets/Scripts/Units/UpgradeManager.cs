using UnityEngine;
using System;
using System.Collections.Generic;
using LottoDefense.Gameplay;

namespace LottoDefense.Units
{
    /// <summary>
    /// Singleton manager responsible for unit upgrade system.
    /// Handles upgrade cost calculation, stat modifications, and gold transactions.
    /// Upgrade formula: Cost = 10 * level^1.5, Attack multiplier = 1.0 + (0.1 * (level - 1))
    /// </summary>
    public class UpgradeManager : MonoBehaviour
    {
        #region Singleton
        private static UpgradeManager _instance;

        /// <summary>
        /// Global access point for the UpgradeManager singleton.
        /// </summary>
        public static UpgradeManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<UpgradeManager>();

                    if (_instance == null)
                    {
                        GameObject go = new GameObject("UpgradeManager");
                        _instance = go.AddComponent<UpgradeManager>();
                    }
                }
                return _instance;
            }
        }
        #endregion

        #region Constants
        private const int MAX_UPGRADE_LEVEL = 10;
        private const float BASE_COST = 10f;
        private const float COST_EXPONENT = 1.5f;
        private const float ATTACK_INCREASE_PER_LEVEL = 0.1f; // 10% per level
        #endregion

        #region Properties
        /// <summary>
        /// Currently selected unit for upgrade (if any).
        /// </summary>
        public Unit SelectedUnit { get; private set; }
        #endregion

        #region Events
        /// <summary>
        /// Fired when a unit is successfully upgraded.
        /// Parameters: unit, newLevel, newAttack
        /// </summary>
        public event Action<Unit, int, int> OnUnitUpgraded;

        /// <summary>
        /// Fired when an upgrade attempt fails.
        /// Parameters: failureReason
        /// </summary>
        public event Action<string> OnUpgradeFailed;

        /// <summary>
        /// Fired when a unit is selected for upgrade.
        /// Parameters: selectedUnit
        /// </summary>
        public event Action<Unit> OnUnitSelected;
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

            Debug.Log("[UpgradeManager] Initialized");
        }
        #endregion

        #region Unit Selection
        /// <summary>
        /// Select a unit for upgrade.
        /// </summary>
        /// <param name="unit">Unit to select</param>
        public void SelectUnit(Unit unit)
        {
            if (unit == null)
            {
                Debug.LogWarning("[UpgradeManager] Cannot select null unit");
                return;
            }

            SelectedUnit = unit;
            OnUnitSelected?.Invoke(unit);
            Debug.Log($"[UpgradeManager] Selected {unit.Data.GetDisplayName()} for upgrade (L{unit.UpgradeLevel})");
        }

        /// <summary>
        /// Clear the current unit selection.
        /// </summary>
        public void DeselectUnit()
        {
            SelectedUnit = null;
            Debug.Log("[UpgradeManager] Unit deselected");
        }
        #endregion

        #region Upgrade Logic
        /// <summary>
        /// Check if a unit can be upgraded.
        /// </summary>
        /// <param name="unit">Unit to check</param>
        /// <returns>True if upgrade is possible</returns>
        public bool CanUpgradeUnit(Unit unit)
        {
            if (unit == null)
                return false;

            // Check if at max level
            if (unit.UpgradeLevel >= MAX_UPGRADE_LEVEL)
                return false;

            // Check if player has enough gold
            int cost = GetUpgradeCost(unit.UpgradeLevel);
            if (GameplayManager.Instance.CurrentGold < cost)
                return false;

            return true;
        }

        /// <summary>
        /// Attempt to upgrade a unit.
        /// </summary>
        /// <param name="unit">Unit to upgrade</param>
        /// <returns>True if upgrade succeeded</returns>
        public bool TryUpgradeUnit(Unit unit)
        {
            if (unit == null)
            {
                OnUpgradeFailed?.Invoke("No unit selected");
                return false;
            }

            // Check max level
            if (unit.UpgradeLevel >= MAX_UPGRADE_LEVEL)
            {
                string reason = $"Unit is already at max level ({MAX_UPGRADE_LEVEL})";
                Debug.LogWarning($"[UpgradeManager] {reason}");
                OnUpgradeFailed?.Invoke(reason);
                return false;
            }

            // Calculate cost
            int cost = GetUpgradeCost(unit.UpgradeLevel);

            // Check gold availability
            if (GameplayManager.Instance.CurrentGold < cost)
            {
                string reason = $"Insufficient gold. Need {cost}, have {GameplayManager.Instance.CurrentGold}";
                Debug.LogWarning($"[UpgradeManager] {reason}");
                OnUpgradeFailed?.Invoke(reason);
                return false;
            }

            // Perform upgrade
            int newLevel = unit.UpgradeLevel + 1;
            float newMultiplier = CalculateAttackMultiplier(newLevel);

            // Deduct gold
            GameplayManager.Instance.ModifyGold(-cost);

            // Apply upgrade to unit
            unit.ApplyUpgrade(newLevel, newMultiplier);

            // Notify listeners
            OnUnitUpgraded?.Invoke(unit, newLevel, unit.CurrentAttack);

            Debug.Log($"[UpgradeManager] Upgraded {unit.Data.GetDisplayName()} to L{newLevel} for {cost} gold");
            return true;
        }
        #endregion

        #region Cost Calculation
        /// <summary>
        /// Calculate the gold cost to upgrade from current level to next level.
        /// Formula: 10 * level^1.5
        /// </summary>
        /// <param name="currentLevel">Current upgrade level</param>
        /// <returns>Gold cost for upgrade</returns>
        public int GetUpgradeCost(int currentLevel)
        {
            if (currentLevel >= MAX_UPGRADE_LEVEL)
                return int.MaxValue; // Cannot upgrade further

            return Mathf.RoundToInt(BASE_COST * Mathf.Pow(currentLevel, COST_EXPONENT));
        }

        /// <summary>
        /// Calculate the attack multiplier for a given level.
        /// Formula: 1.0 + (0.1 * (level - 1))
        /// Level 1: 1.0x, Level 2: 1.1x, Level 5: 1.4x, Level 10: 1.9x
        /// </summary>
        /// <param name="level">Upgrade level</param>
        /// <returns>Attack multiplier</returns>
        public float CalculateAttackMultiplier(int level)
        {
            return 1.0f + (ATTACK_INCREASE_PER_LEVEL * (level - 1));
        }

        /// <summary>
        /// Get comprehensive upgrade stats for a unit.
        /// </summary>
        /// <param name="unit">Unit to analyze</param>
        /// <returns>Upgrade stats structure</returns>
        public UpgradeStats GetUpgradeStats(Unit unit)
        {
            if (unit == null)
            {
                return new UpgradeStats
                {
                    currentLevel = 0,
                    maxLevel = MAX_UPGRADE_LEVEL,
                    isMaxLevel = false,
                    upgradeCost = 0,
                    currentAttack = 0,
                    nextAttack = 0,
                    attackGain = 0,
                    canAfford = false
                };
            }

            int currentLevel = unit.UpgradeLevel;
            int nextLevel = currentLevel + 1;
            bool isMaxLevel = currentLevel >= MAX_UPGRADE_LEVEL;

            int upgradeCost = isMaxLevel ? 0 : GetUpgradeCost(currentLevel);
            int currentAttack = unit.CurrentAttack;
            int nextAttack = isMaxLevel ? currentAttack : Mathf.RoundToInt(unit.Data.attack * CalculateAttackMultiplier(nextLevel));
            int attackGain = nextAttack - currentAttack;
            bool canAfford = GameplayManager.Instance.CurrentGold >= upgradeCost;

            return new UpgradeStats
            {
                currentLevel = currentLevel,
                maxLevel = MAX_UPGRADE_LEVEL,
                isMaxLevel = isMaxLevel,
                upgradeCost = upgradeCost,
                currentAttack = currentAttack,
                nextAttack = nextAttack,
                attackGain = attackGain,
                canAfford = canAfford
            };
        }

        /// <summary>
        /// Get the maximum upgrade level.
        /// </summary>
        public int GetMaxLevel()
        {
            return MAX_UPGRADE_LEVEL;
        }
        #endregion

        #region Debug Utilities
        /// <summary>
        /// Get a summary of the upgrade cost curve.
        /// </summary>
        public string GetUpgradeCostCurve()
        {
            string result = "Upgrade Cost Curve:\n";
            int totalCost = 0;

            for (int level = 1; level < MAX_UPGRADE_LEVEL; level++)
            {
                int cost = GetUpgradeCost(level);
                totalCost += cost;
                float multiplier = CalculateAttackMultiplier(level + 1);
                result += $"L{level}→L{level + 1}: {cost} gold (ATK x{multiplier:F1})\n";
            }

            result += $"\nTotal cost L1→L{MAX_UPGRADE_LEVEL}: {totalCost} gold";
            return result;
        }
        #endregion
    }

    /// <summary>
    /// Data structure containing comprehensive upgrade information for UI display.
    /// </summary>
    [System.Serializable]
    public struct UpgradeStats
    {
        public int currentLevel;
        public int maxLevel;
        public bool isMaxLevel;
        public int upgradeCost;
        public int currentAttack;
        public int nextAttack;
        public int attackGain;
        public bool canAfford;

        public override string ToString()
        {
            if (isMaxLevel)
                return $"Level {currentLevel}/{maxLevel} (MAX) - ATK: {currentAttack}";

            return $"Level {currentLevel}/{maxLevel} - ATK: {currentAttack}→{nextAttack} (+{attackGain}) - Cost: {upgradeCost} gold";
        }
    }
}
