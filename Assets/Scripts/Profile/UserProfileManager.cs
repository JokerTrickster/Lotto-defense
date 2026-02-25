using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

namespace LottoDefense.Profile
{
    /// <summary>
    /// Singleton manager for user profile data.
    /// Handles loading, saving, nickname changes, avatar selection, and unlocking.
    /// Persists data via PlayerPrefs (can be extended to backend API).
    /// </summary>
    public class UserProfileManager : MonoBehaviour
    {
        private static UserProfileManager _instance;
        public static UserProfileManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("UserProfileManager");
                    _instance = go.AddComponent<UserProfileManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        #region Events
        public event Action<string> OnNicknameChanged;
        public event Action<string> OnAvatarChanged;
        public event Action<string> OnAvatarUnlocked;
        #endregion

        #region Private Fields
        private UserProfile _currentProfile;
        private ProfileAvatarData[] _availableAvatars;
        private const string PROFILE_SAVE_KEY = "UserProfile";
        #endregion

        #region Properties
        public UserProfile CurrentProfile => _currentProfile;
        public string Nickname => _currentProfile?.nickname ?? "Player";
        public string SelectedAvatarId => _currentProfile?.selectedAvatarId ?? "avatar_default";
        public ProfileAvatarData[] AvailableAvatars => _availableAvatars;
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

            LoadAvatarDatabase();
            LoadProfile();
        }
        #endregion

        #region Avatar Database
        /// <summary>
        /// Load all ProfileAvatarData from Resources folder.
        /// </summary>
        private void LoadAvatarDatabase()
        {
            _availableAvatars = Resources.LoadAll<ProfileAvatarData>("Avatars");

            if (_availableAvatars == null || _availableAvatars.Length == 0)
            {
                Debug.LogWarning("[UserProfileManager] No avatar data found in Resources/Avatars. Creating default.");
                _availableAvatars = new ProfileAvatarData[0];
            }
            else
            {
                Debug.Log($"[UserProfileManager] Loaded {_availableAvatars.Length} avatars from database");
            }
        }

        /// <summary>
        /// Get avatar data by ID.
        /// </summary>
        public ProfileAvatarData GetAvatarData(string avatarId)
        {
            if (_availableAvatars == null || _availableAvatars.Length == 0)
                return null;

            return _availableAvatars.FirstOrDefault(a => a.avatarId == avatarId);
        }

        /// <summary>
        /// Get all avatars the user has unlocked.
        /// </summary>
        public List<ProfileAvatarData> GetUnlockedAvatars()
        {
            if (_currentProfile == null || _availableAvatars == null)
                return new List<ProfileAvatarData>();

            return _availableAvatars
                .Where(a => _currentProfile.HasUnlockedAvatar(a.avatarId))
                .ToList();
        }

        /// <summary>
        /// Get all locked avatars the user hasn't unlocked yet.
        /// </summary>
        public List<ProfileAvatarData> GetLockedAvatars()
        {
            if (_currentProfile == null || _availableAvatars == null)
                return new List<ProfileAvatarData>();

            return _availableAvatars
                .Where(a => !_currentProfile.HasUnlockedAvatar(a.avatarId))
                .ToList();
        }
        #endregion

        #region Profile Management
        /// <summary>
        /// Load user profile from PlayerPrefs.
        /// Creates a new default profile if none exists.
        /// </summary>
        private void LoadProfile()
        {
            if (PlayerPrefs.HasKey(PROFILE_SAVE_KEY))
            {
                string json = PlayerPrefs.GetString(PROFILE_SAVE_KEY);
                _currentProfile = JsonUtility.FromJson<UserProfile>(json);
                _currentProfile.ValidateSelectedAvatar();
                Debug.Log($"[UserProfileManager] Loaded profile: {_currentProfile.nickname}");
            }
            else
            {
                _currentProfile = new UserProfile();
                UnlockDefaultAvatars();
                SaveProfile();
                Debug.Log("[UserProfileManager] Created new default profile");
            }
        }

        /// <summary>
        /// Save current profile to PlayerPrefs.
        /// </summary>
        public void SaveProfile()
        {
            if (_currentProfile == null)
            {
                Debug.LogWarning("[UserProfileManager] Cannot save null profile");
                return;
            }

            string json = JsonUtility.ToJson(_currentProfile);
            PlayerPrefs.SetString(PROFILE_SAVE_KEY, json);
            PlayerPrefs.Save();
            Debug.Log("[UserProfileManager] Profile saved");
        }

        /// <summary>
        /// Reset profile to default state (for testing/debugging).
        /// </summary>
        public void ResetProfile()
        {
            PlayerPrefs.DeleteKey(PROFILE_SAVE_KEY);
            _currentProfile = new UserProfile();
            UnlockDefaultAvatars();
            SaveProfile();
            Debug.Log("[UserProfileManager] Profile reset to default");
        }
        #endregion

        #region Nickname Management
        /// <summary>
        /// Change user's nickname. Must be 2-12 characters.
        /// </summary>
        public bool SetNickname(string newNickname)
        {
            if (string.IsNullOrWhiteSpace(newNickname))
            {
                Debug.LogWarning("[UserProfileManager] Nickname cannot be empty");
                return false;
            }

            if (newNickname.Length < 2 || newNickname.Length > 12)
            {
                Debug.LogWarning("[UserProfileManager] Nickname must be 2-12 characters");
                return false;
            }

            _currentProfile.nickname = newNickname.Trim();
            SaveProfile();
            OnNicknameChanged?.Invoke(_currentProfile.nickname);
            Debug.Log($"[UserProfileManager] Nickname changed to: {_currentProfile.nickname}");
            return true;
        }
        #endregion

        #region Avatar Management
        /// <summary>
        /// Select a new avatar (must be unlocked).
        /// </summary>
        public bool SelectAvatar(string avatarId)
        {
            if (!_currentProfile.HasUnlockedAvatar(avatarId))
            {
                Debug.LogWarning($"[UserProfileManager] Cannot select locked avatar: {avatarId}");
                return false;
            }

            ProfileAvatarData avatarData = GetAvatarData(avatarId);
            if (avatarData == null)
            {
                Debug.LogWarning($"[UserProfileManager] Avatar not found in database: {avatarId}");
                return false;
            }

            _currentProfile.selectedAvatarId = avatarId;
            SaveProfile();
            OnAvatarChanged?.Invoke(avatarId);
            Debug.Log($"[UserProfileManager] Avatar changed to: {avatarData.avatarName}");
            return true;
        }

        /// <summary>
        /// Unlock a new avatar for the user.
        /// </summary>
        public bool UnlockAvatar(string avatarId)
        {
            if (_currentProfile.HasUnlockedAvatar(avatarId))
            {
                Debug.Log($"[UserProfileManager] Avatar already unlocked: {avatarId}");
                return false;
            }

            ProfileAvatarData avatarData = GetAvatarData(avatarId);
            if (avatarData == null)
            {
                Debug.LogWarning($"[UserProfileManager] Cannot unlock unknown avatar: {avatarId}");
                return false;
            }

            _currentProfile.UnlockAvatar(avatarId);
            SaveProfile();
            OnAvatarUnlocked?.Invoke(avatarId);
            Debug.Log($"[UserProfileManager] Avatar unlocked: {avatarData.avatarName}");
            return true;
        }

        /// <summary>
        /// Unlock all default avatars (called on first load).
        /// </summary>
        private void UnlockDefaultAvatars()
        {
            if (_availableAvatars == null) return;

            foreach (var avatar in _availableAvatars)
            {
                if (avatar.isDefaultUnlocked)
                {
                    _currentProfile.UnlockAvatar(avatar.avatarId);
                }
            }
        }

        /// <summary>
        /// Check if user can unlock an avatar based on quest/achievement requirements.
        /// </summary>
        public bool CanUnlockAvatar(string avatarId)
        {
            ProfileAvatarData avatarData = GetAvatarData(avatarId);
            if (avatarData == null || _currentProfile.HasUnlockedAvatar(avatarId))
                return false;

            // If no requirements, can unlock
            if (avatarData.isDefaultUnlocked)
                return true;

            // Check quest requirement
            if (!string.IsNullOrEmpty(avatarData.requiredQuestId))
            {
                // TODO: Integrate with QuestManager to check completion
                // For now, return false
                return false;
            }

            // Check achievement requirement
            if (!string.IsNullOrEmpty(avatarData.requiredAchievement))
            {
                // TODO: Integrate with AchievementManager
                return false;
            }

            return false;
        }
        #endregion

        #region Public API
        /// <summary>
        /// Get the currently selected avatar sprite.
        /// </summary>
        public Sprite GetCurrentAvatarSprite()
        {
            ProfileAvatarData avatarData = GetAvatarData(_currentProfile.selectedAvatarId);
            return avatarData?.avatarSprite;
        }

        /// <summary>
        /// Get the currently selected avatar border color.
        /// </summary>
        public Color GetCurrentAvatarBorderColor()
        {
            ProfileAvatarData avatarData = GetAvatarData(_currentProfile.selectedAvatarId);
            return avatarData?.borderColor ?? Color.white;
        }
        #endregion
    }
}
