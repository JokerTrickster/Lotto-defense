using UnityEngine;
using System;
using System.Collections.Generic;
using LottoDefense.Gameplay;

namespace LottoDefense.Lobby
{
    public class UnitShopManager : MonoBehaviour
    {
        #region Fields
        private GameBalanceConfig balanceConfig;
        #endregion

        #region Events
        public event Action<string> OnUnitPurchased;
        public event Action<string, int> OnUnitLeveledUp;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            balanceConfig = Resources.Load<GameBalanceConfig>("GameBalanceConfig");
            if (balanceConfig == null)
            {
                balanceConfig = ScriptableObject.CreateInstance<GameBalanceConfig>();
            }
        }
        #endregion

        #region Shop Logic
        public bool IsUnlocked(string unitName)
        {
            return LobbyDataManager.Instance != null && LobbyDataManager.Instance.IsUnitUnlocked(unitName);
        }

        public int GetUnlockPrice(string unitName)
        {
            if (balanceConfig == null) return 0;
            var price = balanceConfig.unitShopPrices.Find(p => p.unitName == unitName);
            return price != null ? price.goldCost : 0;
        }

        public bool CanAfford(string unitName)
        {
            if (LobbyDataManager.Instance == null) return false;
            int cost = GetUnlockPrice(unitName);
            return LobbyDataManager.Instance.Gold >= cost;
        }

        public bool TryUnlockUnit(string unitName)
        {
            if (IsUnlocked(unitName))
            {
                Debug.LogWarning($"[UnitShopManager] {unitName} already unlocked");
                return false;
            }

            int cost = GetUnlockPrice(unitName);
            if (LobbyDataManager.Instance == null || LobbyDataManager.Instance.Gold < cost)
            {
                Debug.LogWarning($"[UnitShopManager] Not enough gold for {unitName} (need {cost})");
                return false;
            }

            LobbyDataManager.Instance.Gold -= cost;
            LobbyDataManager.Instance.UnlockUnit(unitName);

            OnUnitPurchased?.Invoke(unitName);
            Debug.Log($"[UnitShopManager] Unlocked {unitName} for {cost} gold");
            return true;
        }

        public int GetAffordableUnlockCount()
        {
            if (LobbyDataManager.Instance == null || balanceConfig == null) return 0;

            int count = 0;
            foreach (var price in balanceConfig.unitShopPrices)
            {
                if (!IsUnlocked(price.unitName) && LobbyDataManager.Instance.Gold >= price.goldCost)
                    count++;
            }
            return count;
        }

        public List<GameBalanceConfig.UnitShopPrice> GetAllShopItems()
        {
            if (balanceConfig == null) return new List<GameBalanceConfig.UnitShopPrice>();
            return balanceConfig.unitShopPrices ?? new List<GameBalanceConfig.UnitShopPrice>();
        }
        #endregion

        #region Unit Level Up
        public int GetUnitLevel(string unitName)
        {
            if (LobbyDataManager.Instance == null) return 1;
            return LobbyDataManager.Instance.GetUnitLevel(unitName);
        }

        public int GetMaxLevel()
        {
            return balanceConfig != null ? balanceConfig.unitLevelConfig.maxLevel : 10;
        }

        public int GetLevelUpCost(string unitName)
        {
            if (balanceConfig == null) return int.MaxValue;
            var unitBalance = balanceConfig.units.Find(u => u.unitName == unitName);
            if (unitBalance == null) return int.MaxValue;
            int currentLevel = GetUnitLevel(unitName);
            return balanceConfig.GetLevelUpCost(unitBalance.rarity, currentLevel);
        }

        public bool CanAffordLevelUp(string unitName)
        {
            if (LobbyDataManager.Instance == null) return false;
            if (!IsUnlocked(unitName)) return false;
            if (GetUnitLevel(unitName) >= GetMaxLevel()) return false;
            return LobbyDataManager.Instance.Gold >= GetLevelUpCost(unitName);
        }

        public bool TryLevelUpUnit(string unitName)
        {
            if (!CanAffordLevelUp(unitName)) return false;

            int cost = GetLevelUpCost(unitName);
            int newLevel = GetUnitLevel(unitName) + 1;

            LobbyDataManager.Instance.Gold -= cost;
            LobbyDataManager.Instance.SetUnitLevel(unitName, newLevel);

            OnUnitLeveledUp?.Invoke(unitName, newLevel);
            Debug.Log($"[UnitShopManager] {unitName} leveled up to {newLevel} for {cost} gold");
            return true;
        }
        #endregion
    }
}
