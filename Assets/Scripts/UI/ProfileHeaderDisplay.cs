using UnityEngine;
using UnityEngine.UI;
using LottoDefense.Profile;

namespace LottoDefense.UI
{
    /// <summary>
    /// Component for displaying user profile (avatar + nickname) in game HUD.
    /// Updates automatically when profile changes.
    /// Can be clicked to open ProfileSelectionUI.
    /// </summary>
    public class ProfileHeaderDisplay : MonoBehaviour
    {
        #region Serialized Fields
        [Header("Visual Components")]
        [SerializeField] private Image avatarImage;
        [SerializeField] private Image borderImage;
        [SerializeField] private Text nicknameText;
        [SerializeField] private Button profileButton;
        #endregion

        #region Unity Lifecycle
        private void Start()
        {
            if (profileButton != null)
            {
                profileButton.onClick.AddListener(OnProfileButtonClicked);
            }

            // Subscribe to profile change events
            if (UserProfileManager.Instance != null)
            {
                UserProfileManager.Instance.OnNicknameChanged += HandleNicknameChanged;
                UserProfileManager.Instance.OnAvatarChanged += HandleAvatarChanged;
            }

            RefreshDisplay();
        }

        private void OnDestroy()
        {
            if (profileButton != null)
            {
                profileButton.onClick.RemoveListener(OnProfileButtonClicked);
            }

            // Unsubscribe from profile change events
            if (UserProfileManager.Instance != null)
            {
                UserProfileManager.Instance.OnNicknameChanged -= HandleNicknameChanged;
                UserProfileManager.Instance.OnAvatarChanged -= HandleAvatarChanged;
            }
        }
        #endregion

        #region Display Update
        /// <summary>
        /// Refresh profile display with current user data.
        /// </summary>
        public void RefreshDisplay()
        {
            if (UserProfileManager.Instance == null)
            {
                Debug.LogWarning("[ProfileHeaderDisplay] UserProfileManager not available");
                return;
            }

            UpdateAvatar();
            UpdateNickname();
        }

        private void UpdateAvatar()
        {
            if (avatarImage != null)
            {
                Sprite avatarSprite = UserProfileManager.Instance.GetCurrentAvatarSprite();
                if (avatarSprite != null)
                {
                    avatarImage.sprite = avatarSprite;
                }
            }

            if (borderImage != null)
            {
                Color borderColor = UserProfileManager.Instance.GetCurrentAvatarBorderColor();
                borderImage.color = borderColor;
            }
        }

        private void UpdateNickname()
        {
            if (nicknameText != null)
            {
                nicknameText.text = UserProfileManager.Instance.Nickname;
            }
        }
        #endregion

        #region Event Handlers
        private void HandleNicknameChanged(string newNickname)
        {
            UpdateNickname();
        }

        private void HandleAvatarChanged(string newAvatarId)
        {
            UpdateAvatar();
        }

        private void OnProfileButtonClicked()
        {
            // Find and open ProfileSelectionUI
            ProfileSelectionUI profileUI = FindFirstObjectByType<ProfileSelectionUI>();
            if (profileUI != null)
            {
                profileUI.Show();
            }
            else
            {
                Debug.LogWarning("[ProfileHeaderDisplay] ProfileSelectionUI not found in scene");
            }
        }
        #endregion

        #region Public API
        /// <summary>
        /// Set avatar sprite directly (for custom scenarios).
        /// </summary>
        public void SetAvatarSprite(Sprite sprite)
        {
            if (avatarImage != null)
            {
                avatarImage.sprite = sprite;
            }
        }

        /// <summary>
        /// Set nickname text directly (for custom scenarios).
        /// </summary>
        public void SetNickname(string nickname)
        {
            if (nicknameText != null)
            {
                nicknameText.text = nickname;
            }
        }
        #endregion
    }
}
