using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using LottoDefense.Profile;

namespace LottoDefense.UI
{
    /// <summary>
    /// UI popup for selecting avatar and changing nickname.
    /// Displays available avatars in a scrollable grid with locked/unlocked states.
    /// </summary>
    public class ProfileSelectionUI : MonoBehaviour
    {
        #region Serialized Fields
        [Header("Panel")]
        [SerializeField] private GameObject panel;
        [SerializeField] private Button closeButton;

        [Header("Nickname Section")]
        [SerializeField] private InputField nicknameInput;
        [SerializeField] private Button saveNicknameButton;
        [SerializeField] private Text nicknameErrorText;

        [Header("Avatar Section")]
        [SerializeField] private Transform avatarGridContainer;
        [SerializeField] private GameObject avatarButtonPrefab;
        [SerializeField] private Text selectedAvatarNameText;

        [Header("Preview Section")]
        [SerializeField] private Image previewAvatarImage;
        [SerializeField] private Image previewBorderImage;
        [SerializeField] private Text previewNicknameText;
        #endregion

        #region Private Fields
        private List<GameObject> _avatarButtons = new List<GameObject>();
        private string _pendingAvatarId;
        private bool _listenersAttached;
        #endregion

        #region Unity Lifecycle
        private void OnDestroy()
        {
            if (closeButton != null)
                closeButton.onClick.RemoveListener(Hide);

            if (saveNicknameButton != null)
                saveNicknameButton.onClick.RemoveListener(OnSaveNicknameClicked);

            if (nicknameInput != null)
                nicknameInput.onValueChanged.RemoveListener(OnNicknameInputChanged);
        }
        #endregion

        #region Public Methods
        public void Show()
        {
            if (panel == null) return;

            EnsureListeners();
            panel.SetActive(true);
            RefreshUI();
        }

        private void EnsureListeners()
        {
            if (_listenersAttached) return;
            _listenersAttached = true;

            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);

            if (saveNicknameButton != null)
                saveNicknameButton.onClick.AddListener(OnSaveNicknameClicked);

            if (nicknameInput != null)
                nicknameInput.onValueChanged.AddListener(OnNicknameInputChanged);
        }

        public void Hide()
        {
            if (panel == null) return;
            panel.SetActive(false);
        }
        #endregion

        #region UI Refresh
        private void RefreshUI()
        {
            RefreshNicknameSection();
            RefreshAvatarGrid();
            RefreshPreview();
        }

        private void RefreshNicknameSection()
        {
            if (nicknameInput != null)
            {
                nicknameInput.text = UserProfileManager.Instance.Nickname;
            }

            if (nicknameErrorText != null)
            {
                nicknameErrorText.gameObject.SetActive(false);
            }
        }

        private void RefreshAvatarGrid()
        {
            if (avatarGridContainer == null)
            {
                Debug.LogWarning("[ProfileSelectionUI] Avatar grid container not assigned");
                return;
            }

            // Clear existing buttons
            foreach (var btn in _avatarButtons)
            {
                if (btn != null)
                    Destroy(btn);
            }
            _avatarButtons.Clear();

            // Get all avatars from UserProfileManager
            ProfileAvatarData[] allAvatars = UserProfileManager.Instance.AvailableAvatars;
            if (allAvatars == null || allAvatars.Length == 0)
            {
                Debug.LogWarning("[ProfileSelectionUI] No avatars available");
                return;
            }

            // Create button for each avatar
            foreach (var avatarData in allAvatars)
            {
                GameObject btnObj = CreateAvatarButton(avatarData);
                if (btnObj != null)
                    _avatarButtons.Add(btnObj);
            }
        }

        private GameObject CreateAvatarButton(ProfileAvatarData avatarData)
        {
            if (avatarButtonPrefab != null)
            {
                // Use prefab if available
                GameObject btnObj = Instantiate(avatarButtonPrefab, avatarGridContainer);
                ConfigureAvatarButton(btnObj, avatarData);
                return btnObj;
            }
            else
            {
                // Create button programmatically
                return CreateAvatarButtonProgrammatically(avatarData);
            }
        }

        private GameObject CreateAvatarButtonProgrammatically(ProfileAvatarData avatarData)
        {
            GameObject btnObj = new GameObject($"Avatar_{avatarData.avatarId}");
            btnObj.transform.SetParent(avatarGridContainer, false);

            RectTransform rect = btnObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(100, 140);

            LayoutElement le = btnObj.AddComponent<LayoutElement>();
            le.preferredWidth = 100;
            le.preferredHeight = 140;
            le.minWidth = 100;
            le.minHeight = 140;

            // Background image
            Image bgImage = btnObj.AddComponent<Image>();
            bgImage.color = avatarData.borderColor;
            bgImage.sprite = CuteUIHelper.GetRoundedRectSprite(12);
            bgImage.type = Image.Type.Sliced;

            // Avatar icon (top portion)
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(btnObj.transform, false);
            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.1f, 0.3f);
            iconRect.anchorMax = new Vector2(0.9f, 0.95f);
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;

            Image iconImage = iconObj.AddComponent<Image>();
            iconImage.sprite = avatarData.avatarSprite;
            iconImage.color = Color.white;
            iconImage.raycastTarget = false;

            // Avatar name text (bottom portion)
            GameObject nameObj = new GameObject("Name");
            nameObj.transform.SetParent(btnObj.transform, false);
            RectTransform nameRect = nameObj.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0f, 0f);
            nameRect.anchorMax = new Vector2(1f, 0.28f);
            nameRect.offsetMin = new Vector2(4, 2);
            nameRect.offsetMax = new Vector2(-4, 0);

            Text nameText = nameObj.AddComponent<Text>();
            nameText.text = avatarData.avatarName;
            nameText.font = GameFont.Get();
            nameText.fontSize = 16;
            nameText.color = Color.white;
            nameText.alignment = TextAnchor.MiddleCenter;
            nameText.raycastTarget = false;
            nameText.resizeTextForBestFit = true;
            nameText.resizeTextMinSize = 10;
            nameText.resizeTextMaxSize = 16;

            bool isUnlocked = UserProfileManager.Instance.CurrentProfile.HasUnlockedAvatar(avatarData.avatarId);
            if (!isUnlocked)
            {
                // Dark overlay on icon area
                GameObject lockObj = new GameObject("Lock");
                lockObj.transform.SetParent(btnObj.transform, false);
                RectTransform lockRect = lockObj.AddComponent<RectTransform>();
                lockRect.anchorMin = new Vector2(0f, 0.28f);
                lockRect.anchorMax = Vector2.one;
                lockRect.offsetMin = Vector2.zero;
                lockRect.offsetMax = Vector2.zero;

                Image lockBg = lockObj.AddComponent<Image>();
                lockBg.color = new Color(0, 0, 0, 0.7f);

                GameObject lockIconObj = new GameObject("LockIcon");
                lockIconObj.transform.SetParent(lockObj.transform, false);
                RectTransform lockIconRect = lockIconObj.AddComponent<RectTransform>();
                lockIconRect.anchorMin = new Vector2(0.2f, 0.4f);
                lockIconRect.anchorMax = new Vector2(0.8f, 0.85f);
                lockIconRect.offsetMin = Vector2.zero;
                lockIconRect.offsetMax = Vector2.zero;

                Text lockIcon = lockIconObj.AddComponent<Text>();
                lockIcon.text = "잠김";
                lockIcon.font = GameFont.Get();
                lockIcon.fontSize = 20;
                lockIcon.color = Color.white;
                lockIcon.alignment = TextAnchor.MiddleCenter;

                // Unlock hint below lock icon
                string hint = !string.IsNullOrEmpty(avatarData.unlockHint) ? avatarData.unlockHint : "히든 퀘스트";
                nameText.text = hint;
                nameText.color = new Color(1f, 0.8f, 0.5f);
            }

            // Selection indicator
            bool isSelected = UserProfileManager.Instance.SelectedAvatarId == avatarData.avatarId;
            if (isSelected)
            {
                Outline outline = btnObj.AddComponent<Outline>();
                outline.effectColor = new Color(1f, 0.84f, 0f, 1f);
                outline.effectDistance = new Vector2(4, -4);
            }

            Button button = btnObj.AddComponent<Button>();
            button.interactable = isUnlocked;

            string capturedAvatarId = avatarData.avatarId;
            button.onClick.AddListener(() => OnAvatarButtonClicked(capturedAvatarId));

            return btnObj;
        }

        private void ConfigureAvatarButton(GameObject btnObj, ProfileAvatarData avatarData)
        {
            // Configure prefab instance with avatar data
            // This method assumes the prefab has standard child structure

            Image iconImage = btnObj.transform.Find("Icon")?.GetComponent<Image>();
            if (iconImage != null)
            {
                iconImage.sprite = avatarData.avatarSprite;
            }

            Button button = btnObj.GetComponent<Button>();
            if (button != null)
            {
                bool isUnlocked = UserProfileManager.Instance.CurrentProfile.HasUnlockedAvatar(avatarData.avatarId);
                button.interactable = isUnlocked;

                string capturedAvatarId = avatarData.avatarId;
                button.onClick.AddListener(() => OnAvatarButtonClicked(capturedAvatarId));
            }
        }

        private void RefreshPreview()
        {
            if (previewAvatarImage != null)
            {
                previewAvatarImage.sprite = UserProfileManager.Instance.GetCurrentAvatarSprite();
            }

            if (previewBorderImage != null)
            {
                previewBorderImage.color = UserProfileManager.Instance.GetCurrentAvatarBorderColor();
            }

            if (previewNicknameText != null)
            {
                previewNicknameText.text = UserProfileManager.Instance.Nickname;
            }
        }
        #endregion

        #region Event Handlers
        private void OnNicknameInputChanged(string value)
        {
            if (nicknameErrorText != null)
            {
                nicknameErrorText.gameObject.SetActive(false);
            }
        }

        private void OnSaveNicknameClicked()
        {
            if (nicknameInput == null) return;

            string newNickname = nicknameInput.text.Trim();
            bool success = UserProfileManager.Instance.SetNickname(newNickname);

            if (success)
            {
                if (nicknameErrorText != null)
                {
                    nicknameErrorText.gameObject.SetActive(false);
                }
                RefreshPreview();
                Debug.Log("[ProfileSelectionUI] Nickname saved successfully");
            }
            else
            {
                if (nicknameErrorText != null)
                {
                    nicknameErrorText.text = "닉네임은 2-12자여야 합니다";
                    nicknameErrorText.gameObject.SetActive(true);
                }
            }
        }

        private void OnAvatarButtonClicked(string avatarId)
        {
            bool success = UserProfileManager.Instance.SelectAvatar(avatarId);

            if (success)
            {
                _pendingAvatarId = avatarId;
                RefreshAvatarGrid(); // Refresh to update selection indicator
                RefreshPreview();

                ProfileAvatarData avatarData = UserProfileManager.Instance.GetAvatarData(avatarId);
                if (selectedAvatarNameText != null && avatarData != null)
                {
                    selectedAvatarNameText.text = $"선택됨: {avatarData.avatarName}";
                }

                Debug.Log($"[ProfileSelectionUI] Avatar selected: {avatarId}");
            }
            else
            {
                Debug.LogWarning($"[ProfileSelectionUI] Failed to select avatar: {avatarId}");
            }
        }
        #endregion
    }
}
