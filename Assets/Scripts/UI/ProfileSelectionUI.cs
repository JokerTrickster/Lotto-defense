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

            foreach (var btn in _avatarButtons)
            {
                if (btn != null)
                    Destroy(btn);
            }
            _avatarButtons.Clear();

            AutoSizeGridCells();

            ProfileAvatarData[] allAvatars = UserProfileManager.Instance.AvailableAvatars;
            if (allAvatars == null || allAvatars.Length == 0)
            {
                Debug.LogWarning("[ProfileSelectionUI] No avatars available");
                return;
            }

            foreach (var avatarData in allAvatars)
            {
                GameObject btnObj = CreateAvatarButton(avatarData);
                if (btnObj != null)
                    _avatarButtons.Add(btnObj);
            }
        }

        private void AutoSizeGridCells()
        {
            GridLayoutGroup grid = avatarGridContainer.GetComponent<GridLayoutGroup>();
            if (grid == null) return;

            Canvas.ForceUpdateCanvases();
            float containerWidth = ((RectTransform)avatarGridContainer).rect.width;
            if (containerWidth <= 0) return;

            int cols = grid.constraintCount > 0 ? grid.constraintCount : 4;
            float usable = containerWidth - grid.padding.left - grid.padding.right
                           - grid.spacing.x * (cols - 1);
            float cellW = Mathf.Floor(usable / cols);
            float cellH = Mathf.Floor(cellW * 1.3f);
            grid.cellSize = new Vector2(cellW, cellH);
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
            bool isUnlocked = UserProfileManager.Instance.CurrentProfile.HasUnlockedAvatar(avatarData.avatarId);
            bool isSelected = UserProfileManager.Instance.SelectedAvatarId == avatarData.avatarId;

            GameObject btnObj = new GameObject($"Avatar_{avatarData.avatarId}");
            btnObj.transform.SetParent(avatarGridContainer, false);

            Image bgImage = btnObj.AddComponent<Image>();
            Color bgColor = isSelected
                ? new Color(1f, 0.92f, 0.7f)
                : new Color(0.95f, 0.93f, 0.9f);
            bgImage.color = bgColor;
            bgImage.sprite = CuteUIHelper.GetRoundedRectSprite(12);
            bgImage.type = Image.Type.Sliced;

            if (isSelected)
            {
                Outline outline = btnObj.AddComponent<Outline>();
                outline.effectColor = new Color(1f, 0.84f, 0f, 1f);
                outline.effectDistance = new Vector2(3, -3);
            }

            // Avatar icon — top portion
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(btnObj.transform, false);
            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            float iconBottom = isUnlocked ? 0.3f : 0.4f;
            iconRect.anchorMin = new Vector2(0.1f, iconBottom);
            iconRect.anchorMax = new Vector2(0.9f, 0.95f);
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;

            Image iconImage = iconObj.AddComponent<Image>();
            iconImage.sprite = avatarData.avatarSprite;
            iconImage.color = isUnlocked ? Color.white : new Color(0.4f, 0.4f, 0.4f);
            iconImage.raycastTarget = false;

            if (!isUnlocked)
            {
                // "잠김" overlay on icon
                GameObject lockObj = new GameObject("LockOverlay");
                lockObj.transform.SetParent(iconObj.transform, false);
                RectTransform lockRect = lockObj.AddComponent<RectTransform>();
                lockRect.anchorMin = Vector2.zero;
                lockRect.anchorMax = Vector2.one;
                lockRect.offsetMin = Vector2.zero;
                lockRect.offsetMax = Vector2.zero;

                Image lockBg = lockObj.AddComponent<Image>();
                lockBg.color = new Color(0, 0, 0, 0.5f);
                lockBg.raycastTarget = false;

                GameObject lockTextObj = new GameObject("LockText");
                lockTextObj.transform.SetParent(lockObj.transform, false);
                RectTransform ltRect = lockTextObj.AddComponent<RectTransform>();
                ltRect.anchorMin = Vector2.zero;
                ltRect.anchorMax = Vector2.one;
                ltRect.offsetMin = Vector2.zero;
                ltRect.offsetMax = Vector2.zero;

                Text lockText = lockTextObj.AddComponent<Text>();
                lockText.text = "잠김";
                lockText.font = GameFont.Get();
                lockText.fontSize = 18;
                lockText.fontStyle = FontStyle.Bold;
                lockText.color = Color.white;
                lockText.alignment = TextAnchor.MiddleCenter;
                lockText.raycastTarget = false;
            }

            // Bottom text area — name always visible
            if (isUnlocked)
            {
                // Unlocked: name (+ "사용중" badge if selected)
                GameObject nameObj = new GameObject("Name");
                nameObj.transform.SetParent(btnObj.transform, false);
                RectTransform nameRect = nameObj.AddComponent<RectTransform>();

                if (isSelected)
                {
                    nameRect.anchorMin = new Vector2(0f, 0.12f);
                    nameRect.anchorMax = new Vector2(1f, 0.32f);
                }
                else
                {
                    nameRect.anchorMin = new Vector2(0f, 0f);
                    nameRect.anchorMax = new Vector2(1f, 0.3f);
                }
                nameRect.offsetMin = new Vector2(2, 0);
                nameRect.offsetMax = new Vector2(-2, 0);

                Text nameText = nameObj.AddComponent<Text>();
                nameText.text = avatarData.avatarName;
                nameText.font = GameFont.Get();
                nameText.fontSize = 14;
                nameText.fontStyle = FontStyle.Bold;
                nameText.color = new Color(0.25f, 0.22f, 0.2f);
                nameText.alignment = TextAnchor.MiddleCenter;
                nameText.raycastTarget = false;
                nameText.resizeTextForBestFit = true;
                nameText.resizeTextMinSize = 10;
                nameText.resizeTextMaxSize = 14;

                if (isSelected)
                {
                    GameObject badgeObj = new GameObject("Badge");
                    badgeObj.transform.SetParent(btnObj.transform, false);
                    RectTransform badgeRect = badgeObj.AddComponent<RectTransform>();
                    badgeRect.anchorMin = new Vector2(0f, 0f);
                    badgeRect.anchorMax = new Vector2(1f, 0.14f);
                    badgeRect.offsetMin = new Vector2(2, 1);
                    badgeRect.offsetMax = new Vector2(-2, 0);

                    Text badgeText = badgeObj.AddComponent<Text>();
                    badgeText.text = "사용중";
                    badgeText.font = GameFont.Get();
                    badgeText.fontSize = 12;
                    badgeText.fontStyle = FontStyle.Bold;
                    badgeText.color = new Color(0.85f, 0.55f, 0f);
                    badgeText.alignment = TextAnchor.MiddleCenter;
                    badgeText.raycastTarget = false;
                }
            }
            else
            {
                // Locked: name + unlock hint
                GameObject nameObj = new GameObject("Name");
                nameObj.transform.SetParent(btnObj.transform, false);
                RectTransform nameRect = nameObj.AddComponent<RectTransform>();
                nameRect.anchorMin = new Vector2(0f, 0.17f);
                nameRect.anchorMax = new Vector2(1f, 0.4f);
                nameRect.offsetMin = new Vector2(2, 0);
                nameRect.offsetMax = new Vector2(-2, 0);

                Text nameText = nameObj.AddComponent<Text>();
                nameText.text = avatarData.avatarName;
                nameText.font = GameFont.Get();
                nameText.fontSize = 13;
                nameText.fontStyle = FontStyle.Bold;
                nameText.color = new Color(0.5f, 0.48f, 0.45f);
                nameText.alignment = TextAnchor.MiddleCenter;
                nameText.raycastTarget = false;
                nameText.resizeTextForBestFit = true;
                nameText.resizeTextMinSize = 9;
                nameText.resizeTextMaxSize = 13;

                string hint = !string.IsNullOrEmpty(avatarData.unlockHint) ? avatarData.unlockHint : "히든 퀘스트";
                GameObject hintObj = new GameObject("Hint");
                hintObj.transform.SetParent(btnObj.transform, false);
                RectTransform hintRect = hintObj.AddComponent<RectTransform>();
                hintRect.anchorMin = new Vector2(0f, 0f);
                hintRect.anchorMax = new Vector2(1f, 0.19f);
                hintRect.offsetMin = new Vector2(2, 1);
                hintRect.offsetMax = new Vector2(-2, 0);

                Text hintText = hintObj.AddComponent<Text>();
                hintText.text = hint;
                hintText.font = GameFont.Get();
                hintText.fontSize = 10;
                hintText.color = new Color(0.8f, 0.6f, 0.3f);
                hintText.alignment = TextAnchor.MiddleCenter;
                hintText.raycastTarget = false;
                hintText.resizeTextForBestFit = true;
                hintText.resizeTextMinSize = 8;
                hintText.resizeTextMaxSize = 11;
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
