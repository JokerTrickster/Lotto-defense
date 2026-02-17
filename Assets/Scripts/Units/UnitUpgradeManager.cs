using UnityEngine;
using System.Collections.Generic;
using LottoDefense.Gameplay;
using LottoDefense.Grid;

namespace LottoDefense.Units
{
    /// <summary>
    /// Manages unit stat upgrades (Attack and Attack Speed).
    /// Each upgrade level costs gold and increases the stat by a multiplier.
    /// </summary>
    public class UnitUpgradeManager : MonoBehaviour
    {
        #region Singleton
        private static UnitUpgradeManager _instance;

        public static UnitUpgradeManager Instance
        {
            get
            {
                if (GameplayManager.IsCleaningUp) return null;

                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<UnitUpgradeManager>();

                    if (_instance == null)
                    {
                        GameObject go = new GameObject("UnitUpgradeManager");
                        _instance = go.AddComponent<UnitUpgradeManager>();
                    }
                }
                return _instance;
            }
        }
        #endregion

        #region (No longer used - now uses UnitData settings)
        // Legacy constants - kept for reference
        // Now uses UnitData.maxUpgradeLevel, attackUpgradePercent, attackSpeedUpgradePercent
        #endregion

        #region Rarity-Wide Upgrade Storage
        private readonly Dictionary<Rarity, int> rarityAttackLevels = new Dictionary<Rarity, int>();
        private readonly Dictionary<Rarity, int> raritySpeedLevels = new Dictionary<Rarity, int>();
        #endregion

        #region Session Tracking
        private int sessionUpgradeCount;
        public int SessionUpgradeCount => sessionUpgradeCount;
        #endregion

        #region Events
        /// <summary>
        /// Fired when a unit is upgraded.
        /// Parameters: unit, upgradeType, newLevel
        /// </summary>
        public event System.Action<Unit, UpgradeType, int> OnUnitUpgraded;
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

        }
        #endregion

        #region Upgrade Logic
        /// <summary>
        /// Upgrade attack level for ALL units of the same rarity.
        /// Cost is based on the selected unit's current rarity-wide level.
        /// </summary>
        public bool UpgradeAttack(Unit unit)
        {
            if (unit == null)
            {
                Debug.LogWarning("[UnitUpgradeManager] Cannot upgrade null unit");
                return false;
            }

            Rarity rarity = unit.Data.rarity;
            int currentLevel = GetRarityAttackLevel(rarity);

            // Check max level
            int maxLevel = unit.Data != null ? unit.Data.maxUpgradeLevel : 10;
            if (currentLevel >= maxLevel)
            {
                Debug.LogWarning($"[UnitUpgradeManager] {rarity} attack already at max level {maxLevel}");
                return false;
            }

            if (!CanUpgrade())
            {
                Debug.LogWarning("[UnitUpgradeManager] GameplayManager not available for upgrade");
                return false;
            }

            int cost = GetUpgradeCost(unit, UpgradeType.Attack);

            if (GameplayManager.Instance == null || GameplayManager.Instance.CurrentGold < cost)
            {
                Debug.LogWarning($"[UnitUpgradeManager] Not enough gold for attack upgrade (need {cost})");
                return false;
            }

            // Deduct gold
            GameplayManager.Instance.ModifyGold(-cost);

            // Increment rarity-wide level
            int newLevel = currentLevel + 1;
            rarityAttackLevels[rarity] = newLevel;

            // Apply to ALL units of this rarity on the grid
            ApplyAttackLevelToAllUnits(rarity, newLevel);

            sessionUpgradeCount++;

            OnUnitUpgraded?.Invoke(unit, UpgradeType.Attack, newLevel);

            return true;
        }

        /// <summary>
        /// Upgrade attack speed level for ALL units of the same rarity.
        /// </summary>
        public bool UpgradeAttackSpeed(Unit unit)
        {
            if (unit == null)
            {
                Debug.LogWarning("[UnitUpgradeManager] Cannot upgrade null unit");
                return false;
            }

            Rarity rarity = unit.Data.rarity;
            int currentLevel = GetRaritySpeedLevel(rarity);

            int maxLevel = unit.Data != null ? unit.Data.maxUpgradeLevel : 10;
            if (currentLevel >= maxLevel)
            {
                Debug.LogWarning($"[UnitUpgradeManager] {rarity} attack speed already at max level {maxLevel}");
                return false;
            }

            if (!CanUpgrade())
            {
                Debug.LogWarning("[UnitUpgradeManager] GameplayManager not available for upgrade");
                return false;
            }

            int cost = GetUpgradeCost(unit, UpgradeType.AttackSpeed);

            if (GameplayManager.Instance == null || GameplayManager.Instance.CurrentGold < cost)
            {
                Debug.LogWarning($"[UnitUpgradeManager] Not enough gold for attack speed upgrade (need {cost})");
                return false;
            }

            // Deduct gold
            GameplayManager.Instance.ModifyGold(-cost);

            // Increment rarity-wide level
            int newLevel = currentLevel + 1;
            raritySpeedLevels[rarity] = newLevel;

            // Apply to ALL units of this rarity on the grid
            ApplySpeedLevelToAllUnits(rarity, newLevel);

            sessionUpgradeCount++;

            OnUnitUpgraded?.Invoke(unit, UpgradeType.AttackSpeed, newLevel);

            return true;
        }
        #endregion

        #region Cost Calculation
        /// <summary>
        /// Calculate upgrade cost based on rarity-wide level.
        /// Uses UnitData.baseUpgradeCost as base cost.
        /// </summary>
        public int GetUpgradeCost(Unit unit, UpgradeType upgradeType)
        {
            if (unit == null || unit.Data == null) return 999999;

            Rarity rarity = unit.Data.rarity;
            int currentLevel = upgradeType == UpgradeType.Attack
                ? GetRarityAttackLevel(rarity)
                : GetRaritySpeedLevel(rarity);

            int baseCost = unit.Data.baseUpgradeCost;

            // Cost increases with level: baseCost * (1 + level * 0.5)
            int cost = Mathf.RoundToInt(baseCost * (1f + currentLevel * 0.5f));

            return cost;
        }

        /// <summary>
        /// Get attack upgrade multiplier for a level.
        /// Uses UnitData.attackUpgradePercent if available.
        /// </summary>
        public float GetAttackMultiplier(int level, UnitData data = null)
        {
            float percentPerLevel = data != null ? data.attackUpgradePercent / 100f : 0.1f;
            return 1f + (level * percentPerLevel);
        }

        /// <summary>
        /// Get attack speed upgrade multiplier for a level.
        /// Uses UnitData.attackSpeedUpgradePercent if available.
        /// </summary>
        public float GetAttackSpeedMultiplier(int level, UnitData data = null)
        {
            float percentPerLevel = data != null ? data.attackSpeedUpgradePercent / 100f : 0.08f;
            return 1f + (level * percentPerLevel);
        }
        #endregion

        #region Rarity Level Access
        /// <summary>
        /// Get current attack upgrade level for a rarity.
        /// </summary>
        public int GetRarityAttackLevel(Rarity rarity)
        {
            return rarityAttackLevels.TryGetValue(rarity, out int level) ? level : 0;
        }

        /// <summary>
        /// Get current attack speed upgrade level for a rarity.
        /// </summary>
        public int GetRaritySpeedLevel(Rarity rarity)
        {
            return raritySpeedLevels.TryGetValue(rarity, out int level) ? level : 0;
        }

        /// <summary>
        /// Apply existing rarity upgrades to a newly placed unit.
        /// Call this after unit.Initialize().
        /// </summary>
        public void ApplyRarityUpgrades(Unit unit)
        {
            if (unit == null || unit.Data == null) return;

            Rarity rarity = unit.Data.rarity;
            int atkLevel = GetRarityAttackLevel(rarity);
            int spdLevel = GetRaritySpeedLevel(rarity);

            if (atkLevel > 0)
            {
                unit.SetAttackUpgradeLevel(atkLevel);
            }
            if (spdLevel > 0)
            {
                unit.SetAttackSpeedUpgradeLevel(spdLevel);
            }
        }
        #endregion

        #region Apply to All Units
        private void ApplyAttackLevelToAllUnits(Rarity rarity, int level)
        {
            if (GridManager.Instance == null) return;

            for (int x = 0; x < GridManager.GRID_WIDTH; x++)
            {
                for (int y = 0; y < GridManager.GRID_HEIGHT; y++)
                {
                    Unit unit = GridManager.Instance.GetUnitAt(x, y);
                    if (unit != null && unit.Data != null && unit.Data.rarity == rarity)
                    {
                        unit.SetAttackUpgradeLevel(level);
                    }
                }
            }
        }

        private void ApplySpeedLevelToAllUnits(Rarity rarity, int level)
        {
            if (GridManager.Instance == null) return;

            for (int x = 0; x < GridManager.GRID_WIDTH; x++)
            {
                for (int y = 0; y < GridManager.GRID_HEIGHT; y++)
                {
                    Unit unit = GridManager.Instance.GetUnitAt(x, y);
                    if (unit != null && unit.Data != null && unit.Data.rarity == rarity)
                    {
                        unit.SetAttackSpeedUpgradeLevel(level);
                    }
                }
            }
        }
        #endregion

        #region Helper Methods
        private bool CanUpgrade()
        {
            return GameplayManager.Instance != null;
        }
        #endregion
    }

    /// <summary>
    /// Type of upgrade.
    /// </summary>
    public enum UpgradeType
    {
        Attack,
        AttackSpeed
    }
}
