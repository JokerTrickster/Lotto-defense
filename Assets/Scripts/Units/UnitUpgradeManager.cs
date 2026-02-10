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

        #region Constants
        private const int MAX_ATTACK_UPGRADE_LEVEL = 10;
        private const int MAX_ATTACK_SPEED_UPGRADE_LEVEL = 10;
        private const float ATTACK_UPGRADE_MULTIPLIER = 0.1f; // +10% per level
        private const float ATTACK_SPEED_UPGRADE_MULTIPLIER = 0.08f; // +8% per level
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

            // Check max level
            if (unit.AttackUpgradeLevel >= MAX_ATTACK_UPGRADE_LEVEL)
            {
                Debug.LogWarning($"[UnitUpgradeManager] {unit.Data.GetDisplayName()} attack already at max level {MAX_ATTACK_UPGRADE_LEVEL}");
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

            // Check max level
            if (unit.AttackSpeedUpgradeLevel >= MAX_ATTACK_SPEED_UPGRADE_LEVEL)
            {
                Debug.LogWarning($"[UnitUpgradeManager] {unit.Data.GetDisplayName()} attack speed already at max level {MAX_ATTACK_SPEED_UPGRADE_LEVEL}");
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
        /// Calculate upgrade cost based on rarity and current level.
        /// </summary>
        public int GetUpgradeCost(Unit unit, UpgradeType upgradeType)
        {
            if (unit == null || unit.Data == null) return 999999;

            int currentLevel = upgradeType == UpgradeType.Attack ? unit.AttackUpgradeLevel : unit.AttackSpeedUpgradeLevel;

            // Base cost by rarity
            int baseCost = 0;
            switch (unit.Data.rarity)
            {
                case Rarity.Normal:
                    baseCost = 5;
                    break;
                case Rarity.Rare:
                    baseCost = 10;
                    break;
                case Rarity.Epic:
                    baseCost = 20;
                    break;
                case Rarity.Legendary:
                    baseCost = 50;
                    break;
            }

            // Cost increases with level: baseCost * (1 + level * 0.5)
            int cost = Mathf.RoundToInt(baseCost * (1f + currentLevel * 0.5f));

            return cost;
        }

        /// <summary>
        /// Get attack upgrade multiplier for a level.
        /// </summary>
        public float GetAttackMultiplier(int level)
        {
            return 1f + (level * ATTACK_UPGRADE_MULTIPLIER);
        }

        /// <summary>
        /// Get attack speed upgrade multiplier for a level.
        /// </summary>
        public float GetAttackSpeedMultiplier(int level)
        {
            return 1f + (level * ATTACK_SPEED_UPGRADE_MULTIPLIER);
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
