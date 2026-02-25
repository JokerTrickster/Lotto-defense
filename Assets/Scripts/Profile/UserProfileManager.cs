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
                Debug.Log("[UserProfileManager] No avatar assets found, generating procedural avatars");
                _availableAvatars = GenerateProceduralAvatars();
            }
            else
            {
                Debug.Log($"[UserProfileManager] Loaded {_availableAvatars.Length} avatars from database");
            }
        }

        private ProfileAvatarData[] GenerateProceduralAvatars()
        {
            var avatarDefs = new[]
            {
                new { id = "avatar_default",    name = "기본",       color = new Color(0.55f, 0.75f, 0.95f), border = Color.white,                        unlocked = true,  quest = "",                        hint = "",                         rarity = AvatarRarity.Common },
                new { id = "avatar_red",        name = "붉은 전사",  color = new Color(0.95f, 0.45f, 0.45f), border = new Color(1f, 0.6f, 0.6f),          unlocked = true,  quest = "",                        hint = "",                         rarity = AvatarRarity.Common },
                new { id = "avatar_green",      name = "숲의 수호자",color = new Color(0.45f, 0.85f, 0.55f), border = new Color(0.6f, 1f, 0.7f),          unlocked = true,  quest = "",                        hint = "",                         rarity = AvatarRarity.Common },
                new { id = "avatar_warrior",    name = "전사의 혼",  color = new Color(0.85f, 0.65f, 0.35f), border = new Color(1f, 0.84f, 0f),           unlocked = false, quest = "collect_warrior_3",       hint = "전사 3마리를 배치하세요",  rarity = AvatarRarity.Uncommon },
                new { id = "avatar_archer",     name = "명사수",     color = new Color(0.35f, 0.85f, 0.85f), border = new Color(0.4f, 1f, 1f),            unlocked = false, quest = "collect_archer_3",        hint = "궁수 3마리를 배치하세요",  rarity = AvatarRarity.Uncommon },
                new { id = "avatar_mage",       name = "마법사의 눈",color = new Color(0.7f, 0.45f, 0.95f),  border = new Color(0.8f, 0.55f, 1f),         unlocked = false, quest = "collect_mage_2",          hint = "마법사 2마리를 배치하세요", rarity = AvatarRarity.Rare },
                new { id = "avatar_dragon",     name = "용기사",     color = new Color(0.95f, 0.65f, 0.2f),  border = new Color(1f, 0.84f, 0f),           unlocked = false, quest = "collect_dragon_knight_2", hint = "용기사 2마리를 배치하세요", rarity = AvatarRarity.Legendary },
            };

            var result = new ProfileAvatarData[avatarDefs.Length];
            for (int i = 0; i < avatarDefs.Length; i++)
            {
                var def = avatarDefs[i];
                ProfileAvatarData avatar = ScriptableObject.CreateInstance<ProfileAvatarData>();
                avatar.avatarId = def.id;
                avatar.avatarName = def.name;
                avatar.avatarSprite = CreateProceduralAvatarSprite(64, def.color);
                avatar.borderColor = def.border;
                avatar.isDefaultUnlocked = def.unlocked;
                avatar.requiredQuestId = def.quest;
                avatar.unlockHint = def.hint;
                avatar.rarity = def.rarity;
                result[i] = avatar;
            }

            Debug.Log($"[UserProfileManager] Generated {result.Length} procedural avatars");
            return result;
        }

        private static Sprite CreateProceduralAvatarSprite(int size, Color baseColor)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float center = size * 0.5f;
            float outerRadius = center - 1f;
            float innerRadius = outerRadius * 0.7f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center + 0.5f;
                    float dy = y - center + 0.5f;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    if (dist > outerRadius)
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                    else if (dist > innerRadius)
                    {
                        float t = (dist - innerRadius) / (outerRadius - innerRadius);
                        Color ringColor = Color.Lerp(baseColor * 0.8f, baseColor, t);
                        ringColor.a = 1f;
                        tex.SetPixel(x, y, ringColor);
                    }
                    else
                    {
                        float t = dist / innerRadius;
                        Color fill = Color.Lerp(Color.white * 0.95f, baseColor, t * 0.6f);
                        fill.a = 1f;
                        tex.SetPixel(x, y, fill);
                    }
                }
            }

            tex.Apply();
            tex.filterMode = FilterMode.Bilinear;
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
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
            if (_currentProfile == null) return null;
            ProfileAvatarData avatarData = GetAvatarData(_currentProfile.selectedAvatarId);
            return avatarData?.avatarSprite;
        }

        /// <summary>
        /// Get the currently selected avatar border color.
        /// </summary>
        public Color GetCurrentAvatarBorderColor()
        {
            if (_currentProfile == null) return Color.white;
            ProfileAvatarData avatarData = GetAvatarData(_currentProfile.selectedAvatarId);
            return avatarData?.borderColor ?? Color.white;
        }
        #endregion
    }
}
