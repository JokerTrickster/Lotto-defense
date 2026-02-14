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
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            balanceConfig = Resources.Load<GameBalanceConfig>("GameBalanceConfig");
            if (balanceConfig == null)
            {
                Debug.LogWarning("[UnitShopManager] GameBalanceConfig asset not found, using code defaults");
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
            return balanceConfig.unitShopPrices;
        }
        #endregion
    }
}
