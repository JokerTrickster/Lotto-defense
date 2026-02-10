using UnityEngine;
using LottoDefense.Gameplay;

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

            Debug.Log("[UnitUpgradeManager] Initialized");
        }
        #endregion

        #region Upgrade Logic
        /// <summary>
        /// Upgrade unit's attack level.
        /// </summary>
        public bool UpgradeAttack(Unit unit)
        {
            if (unit == null)
            {
                Debug.LogWarning("[UnitUpgradeManager] Cannot upgrade null unit");
                return false;
            }

            // Check max level (from UnitData)
            int maxLevel = unit.Data != null ? unit.Data.maxUpgradeLevel : 10;
            if (unit.AttackUpgradeLevel >= maxLevel)
            {
                Debug.LogWarning($"[UnitUpgradeManager] {unit.Data.GetDisplayName()} attack already at max level {maxLevel}");
                return false;
            }

            // Check if in Preparation phase
            if (!CanUpgrade())
            {
                Debug.LogWarning("[UnitUpgradeManager] Can only upgrade during Preparation phase");
                return false;
            }

            // Calculate cost
            int cost = GetUpgradeCost(unit, UpgradeType.Attack);

            // Check gold
            if (GameplayManager.Instance == null || GameplayManager.Instance.CurrentGold < cost)
            {
                Debug.LogWarning($"[UnitUpgradeManager] Not enough gold for attack upgrade (need {cost})");
                return false;
            }

            // Deduct gold
            GameplayManager.Instance.ModifyGold(-cost);

            // Upgrade
            unit.UpgradeAttack();

            Debug.Log($"[UnitUpgradeManager] Upgraded {unit.Data.GetDisplayName()} attack to level {unit.AttackUpgradeLevel}");

            // Fire event
            OnUnitUpgraded?.Invoke(unit, UpgradeType.Attack, unit.AttackUpgradeLevel);

            return true;
        }

        /// <summary>
        /// Upgrade unit's attack speed level.
        /// </summary>
        public bool UpgradeAttackSpeed(Unit unit)
        {
            if (unit == null)
            {
                Debug.LogWarning("[UnitUpgradeManager] Cannot upgrade null unit");
                return false;
            }

            // Check max level (from UnitData)
            int maxLevel = unit.Data != null ? unit.Data.maxUpgradeLevel : 10;
            if (unit.AttackSpeedUpgradeLevel >= maxLevel)
            {
                Debug.LogWarning($"[UnitUpgradeManager] {unit.Data.GetDisplayName()} attack speed already at max level {maxLevel}");
                return false;
            }

            // Check if in Preparation phase
            if (!CanUpgrade())
            {
                Debug.LogWarning("[UnitUpgradeManager] Can only upgrade during Preparation phase");
                return false;
            }

            // Calculate cost
            int cost = GetUpgradeCost(unit, UpgradeType.AttackSpeed);

            // Check gold
            if (GameplayManager.Instance == null || GameplayManager.Instance.CurrentGold < cost)
            {
                Debug.LogWarning($"[UnitUpgradeManager] Not enough gold for attack speed upgrade (need {cost})");
                return false;
            }

            // Deduct gold
            GameplayManager.Instance.ModifyGold(-cost);

            // Upgrade
            unit.UpgradeAttackSpeed();

            Debug.Log($"[UnitUpgradeManager] Upgraded {unit.Data.GetDisplayName()} attack speed to level {unit.AttackSpeedUpgradeLevel}");

            // Fire event
            OnUnitUpgraded?.Invoke(unit, UpgradeType.AttackSpeed, unit.AttackSpeedUpgradeLevel);

            return true;
        }
        #endregion

        #region Cost Calculation
        /// <summary>
        /// Calculate upgrade cost based on UnitData settings and current level.
        /// Uses UnitData.baseUpgradeCost as base cost.
        /// </summary>
        public int GetUpgradeCost(Unit unit, UpgradeType upgradeType)
        {
            if (unit == null || unit.Data == null) return 999999;

            int currentLevel = upgradeType == UpgradeType.Attack ? unit.AttackUpgradeLevel : unit.AttackSpeedUpgradeLevel;

            // Use base cost from UnitData
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

        #region Helper Methods
        /// <summary>
        /// Check if upgrades are allowed (Preparation phase only).
        /// </summary>
        private bool CanUpgrade()
        {
            return GameplayManager.Instance != null &&
                   GameplayManager.Instance.CurrentState == GameState.Preparation;
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
