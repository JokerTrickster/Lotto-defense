using UnityEngine;
using System.Collections.Generic;
using LottoDefense.Quests;

namespace LottoDefense.Profile
{
    /// <summary>
    /// Manager for handling hidden profile unlocks based on quest completion.
    /// Listens to QuestManager events and unlocks avatars when conditions are met.
    /// </summary>
    public class ProfileUnlockManager : MonoBehaviour
    {
        private static ProfileUnlockManager _instance;
        public static ProfileUnlockManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("ProfileUnlockManager");
                    _instance = go.AddComponent<ProfileUnlockManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

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

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }
        #endregion

        #region Event Subscriptions
        private void SubscribeToEvents()
        {
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.OnQuestCompleted += HandleQuestCompleted;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.OnQuestCompleted -= HandleQuestCompleted;
            }
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Called when a quest is completed.
        /// Check if any avatar should be unlocked based on this quest.
        /// </summary>
        private void HandleQuestCompleted(QuestInstance completedQuest)
        {
            if (completedQuest == null || completedQuest.Definition == null)
            {
                Debug.LogWarning("[ProfileUnlockManager] Quest completed but definition is null");
                return;
            }

            string questId = completedQuest.Definition.id;
            Debug.Log($"[ProfileUnlockManager] Quest completed: {questId}");

            CheckAndUnlockAvatars(questId);
        }
        #endregion

        #region Avatar Unlock Logic
        /// <summary>
        /// Check all avatars to see if any should be unlocked based on the completed quest.
        /// </summary>
        private void CheckAndUnlockAvatars(string completedQuestId)
        {
            if (UserProfileManager.Instance == null)
            {
                Debug.LogWarning("[ProfileUnlockManager] UserProfileManager not available");
                return;
            }

            ProfileAvatarData[] allAvatars = UserProfileManager.Instance.AvailableAvatars;
            if (allAvatars == null || allAvatars.Length == 0)
            {
                return;
            }

            foreach (var avatar in allAvatars)
            {
                // Skip if already unlocked
                if (UserProfileManager.Instance.CurrentProfile.HasUnlockedAvatar(avatar.avatarId))
                    continue;

                // Check if this avatar requires the completed quest
                if (!string.IsNullOrEmpty(avatar.requiredQuestId) &&
                    avatar.requiredQuestId == completedQuestId)
                {
                    UnlockAvatar(avatar);
                }
            }
        }

        /// <summary>
        /// Unlock an avatar and show notification to user.
        /// </summary>
        private void UnlockAvatar(ProfileAvatarData avatar)
        {
            bool success = UserProfileManager.Instance.UnlockAvatar(avatar.avatarId);

            if (success)
            {
                Debug.Log($"[ProfileUnlockManager] Unlocked hidden avatar: {avatar.avatarName}");
                ShowUnlockNotification(avatar);
            }
        }

        /// <summary>
        /// Show a notification that a hidden avatar has been unlocked.
        /// </summary>
        private void ShowUnlockNotification(ProfileAvatarData avatar)
        {
            // TODO: Create a nice popup notification UI
            // For now, just log
            Debug.Log($"🎉 Hidden Avatar Unlocked: {avatar.avatarName}!");

            // Could integrate with a notification system here
            // Example: NotificationManager.Instance.ShowAvatarUnlock(avatar);
        }
        #endregion

        #region Manual Unlock (Dev/Testing)
        /// <summary>
        /// Manually unlock an avatar by ID (for testing/debugging).
        /// </summary>
        public void ManualUnlockAvatar(string avatarId)
        {
            if (UserProfileManager.Instance == null)
            {
                Debug.LogWarning("[ProfileUnlockManager] UserProfileManager not available");
                return;
            }

            ProfileAvatarData avatar = UserProfileManager.Instance.GetAvatarData(avatarId);
            if (avatar == null)
            {
                Debug.LogWarning($"[ProfileUnlockManager] Avatar not found: {avatarId}");
                return;
            }

            UnlockAvatar(avatar);
        }

        /// <summary>
        /// Check all quests and unlock any avatars whose conditions are already met.
        /// Useful for retroactive unlocks or save migration.
        /// </summary>
        public void CheckAllQuestsForUnlocks()
        {
            if (QuestManager.Instance == null || UserProfileManager.Instance == null)
            {
                Debug.LogWarning("[ProfileUnlockManager] Managers not available");
                return;
            }

            var completedQuests = QuestManager.Instance.Quests;
            foreach (var quest in completedQuests)
            {
                if (quest != null && quest.IsCompleted && quest.Definition != null)
                {
                    CheckAndUnlockAvatars(quest.Definition.id);
                }
            }
        }
        #endregion

        #region Achievement-Based Unlocks
        /// <summary>
        /// Unlock avatars based on achievement (future implementation).
        /// </summary>
        public void CheckAchievementUnlock(string achievementId)
        {
            if (UserProfileManager.Instance == null)
            {
                return;
            }

            ProfileAvatarData[] allAvatars = UserProfileManager.Instance.AvailableAvatars;
            if (allAvatars == null || allAvatars.Length == 0)
            {
                return;
            }

            foreach (var avatar in allAvatars)
            {
                // Skip if already unlocked
                if (UserProfileManager.Instance.CurrentProfile.HasUnlockedAvatar(avatar.avatarId))
                    continue;

                // Check if this avatar requires the achievement
                if (!string.IsNullOrEmpty(avatar.requiredAchievement) &&
                    avatar.requiredAchievement == achievementId)
                {
                    UnlockAvatar(avatar);
                }
            }
        }
        #endregion
    }
}
