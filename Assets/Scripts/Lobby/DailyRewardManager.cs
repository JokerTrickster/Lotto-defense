using UnityEngine;
using System;
using LottoDefense.Gameplay;

namespace LottoDefense.Lobby
{
    public class DailyRewardManager : MonoBehaviour
    {
        #region Fields
        private GameBalanceConfig balanceConfig;
        #endregion

        #region Events
        public event Action OnRewardClaimed;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            balanceConfig = Resources.Load<GameBalanceConfig>("GameBalanceConfig");
            if (balanceConfig == null)
                Debug.LogError("[DailyRewardManager] GameBalanceConfig not found!");
        }
        #endregion

        #region Reward Logic
        public int GetDailyClearCount()
        {
            return LobbyDataManager.Instance != null ? LobbyDataManager.Instance.DailyClearCount : 0;
        }

        public bool CanClaimStage(int stageIndex)
        {
            if (balanceConfig == null || LobbyDataManager.Instance == null) return false;
            if (stageIndex < 0 || stageIndex >= balanceConfig.dailyRewardStages.Count) return false;

            var stage = balanceConfig.dailyRewardStages[stageIndex];
            int clears = LobbyDataManager.Instance.DailyClearCount;

            // Check if requirement met
            if (clears < stage.requiredClears) return false;

            // Check if already claimed (bitmask)
            int claimed = LobbyDataManager.Instance.DailyClaimedStages;
            return (claimed & (1 << stageIndex)) == 0;
        }

        public bool IsStageClaimed(int stageIndex)
        {
            if (LobbyDataManager.Instance == null) return false;
            int claimed = LobbyDataManager.Instance.DailyClaimedStages;
            return (claimed & (1 << stageIndex)) != 0;
        }

        public bool TryClaimStage(int stageIndex)
        {
            if (!CanClaimStage(stageIndex)) return false;

            var stage = balanceConfig.dailyRewardStages[stageIndex];

            // Grant rewards
            if (stage.goldReward > 0)
                LobbyDataManager.Instance.Gold += stage.goldReward;
            if (stage.ticketReward > 0)
                LobbyDataManager.Instance.Tickets += stage.ticketReward;

            // Mark as claimed
            int claimed = LobbyDataManager.Instance.DailyClaimedStages;
            claimed |= (1 << stageIndex);
            LobbyDataManager.Instance.DailyClaimedStages = claimed;

            OnRewardClaimed?.Invoke();
            Debug.Log($"[DailyRewardManager] Claimed stage {stageIndex + 1}: +{stage.goldReward}G, +{stage.ticketReward}T");
            return true;
        }

        public bool HasClaimableRewards()
        {
            return GetClaimableStageCount() > 0;
        }

        public int GetClaimableStageCount()
        {
            if (balanceConfig == null) return 0;
            int count = 0;
            for (int i = 0; i < balanceConfig.dailyRewardStages.Count; i++)
            {
                if (CanClaimStage(i)) count++;
            }
            return count;
        }

        public int GetStageCount()
        {
            return balanceConfig != null ? balanceConfig.dailyRewardStages.Count : 0;
        }

        public GameBalanceConfig.DailyRewardStage GetStage(int index)
        {
            if (balanceConfig == null || index < 0 || index >= balanceConfig.dailyRewardStages.Count)
                return null;
            return balanceConfig.dailyRewardStages[index];
        }
        #endregion
    }
}
