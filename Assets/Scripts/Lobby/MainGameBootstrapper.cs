using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Reflection;
using LottoDefense.Authentication;
using LottoDefense.UI;
using LottoDefense.Profile;
using LottoDefense.Units;

namespace LottoDefense.Lobby
{
    public class MainGameBootstrapper : MonoBehaviour
    {
        #region Auto-Init
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RegisterSceneCallback()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "MainGame")
            {
                if (FindFirstObjectByType<MainGameBootstrapper>() != null) return;
                GameObject obj = new GameObject("MainGameBootstrapper");
                obj.AddComponent<MainGameBootstrapper>();
                Debug.Log("[MainGameBootstrapper] Auto-created in MainGame");
            }
        }
        #endregion

        #region Fields
        private Canvas mainCanvas;
        private Transform safeAreaRoot;
        private Font defaultFont;

        // Currency displays
        private Text goldText;
        private Text ticketText;

        // Notification badges
        private NotificationBadge dailyRewardBadge;
        private NotificationBadge shopBadge;
        private NotificationBadge questBadge;
        private NotificationBadge mailBadge;

        // Profile
        private ProfileSelectionUI profileSelectionUI;
        private ProfileHeaderDisplay profileHeaderDisplay;

        // Popup references
        private UnitShopUI shopUI;
        private DailyRewardUI dailyRewardUI;
        private LobbyQuestUI questUI;
        private MailboxUI mailboxUI;

        // Managers
        private LobbyDataManager dataManager;
        private UnitShopManager shopManager;
        private DailyRewardManager dailyRewardManager;
        private LobbyQuestManager questManager;
        private MailboxManager mailboxManager;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            defaultFont = GameFont.Get();

            DestroyBrokenCanvases();
            EnsureEventSystem();
            SetupCanvas();
            EnsureManagers();
            CreateUI();
            SubscribeToEvents();
            RefreshAllBadges();

            Debug.Log("[MainGameBootstrapper] Lobby UI initialized");
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }
        #endregion

        #region Setup
        private void DestroyBrokenCanvases()
        {
            Canvas[] existing = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (Canvas c in existing)
            {
                Debug.Log($"[MainGameBootstrapper] Destroying existing Canvas: {c.gameObject.name}");
                Destroy(c.gameObject);
            }
        }

        private void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() == null)
            {
                GameObject esObj = new GameObject("EventSystem");
                esObj.AddComponent<EventSystem>();
                esObj.AddComponent<StandaloneInputModule>();
            }
        }

        private void SetupCanvas()
        {
            GameObject canvasObj = new GameObject("LobbyCanvas");
            mainCanvas = canvasObj.AddComponent<Canvas>();
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            mainCanvas.sortingOrder = 0;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            // Create SafeArea container
            GameObject safeAreaObj = new GameObject("SafeArea");
            safeAreaObj.transform.SetParent(mainCanvas.transform, false);

            RectTransform safeRect = safeAreaObj.AddComponent<RectTransform>();
            safeRect.anchorMin = Vector2.zero;
            safeRect.anchorMax = Vector2.one;
            safeRect.offsetMin = Vector2.zero;
            safeRect.offsetMax = Vector2.zero;

            safeAreaObj.AddComponent<LottoDefense.UI.SafeAreaAdapter>();
            safeAreaRoot = safeAreaObj.transform;
        }

        private void EnsureManagers()
        {
            // LobbyDataManager
            dataManager = FindFirstObjectByType<LobbyDataManager>();
            if (dataManager == null)
            {
                GameObject obj = new GameObject("LobbyDataManager");
                dataManager = obj.AddComponent<LobbyDataManager>();
            }

            // UnitShopManager
            shopManager = FindFirstObjectByType<UnitShopManager>();
            if (shopManager == null)
            {
                GameObject obj = new GameObject("UnitShopManager");
                shopManager = obj.AddComponent<UnitShopManager>();
            }

            // DailyRewardManager
            dailyRewardManager = FindFirstObjectByType<DailyRewardManager>();
            if (dailyRewardManager == null)
            {
                GameObject obj = new GameObject("DailyRewardManager");
                dailyRewardManager = obj.AddComponent<DailyRewardManager>();
            }

            // LobbyQuestManager
            questManager = FindFirstObjectByType<LobbyQuestManager>();
            if (questManager == null)
            {
                GameObject obj = new GameObject("LobbyQuestManager");
                questManager = obj.AddComponent<LobbyQuestManager>();
            }

            // MailboxManager
            mailboxManager = FindFirstObjectByType<MailboxManager>();
            if (mailboxManager == null)
            {
                GameObject obj = new GameObject("MailboxManager");
                mailboxManager = obj.AddComponent<MailboxManager>();
            }

            // Force UserProfileManager early init before UI creation
            var _ = UserProfileManager.Instance;
        }
        #endregion

        #region UI Creation
        private void CreateUI()
        {
            CreateBackground();
            CreateTopBar();
            CreateTitle();
            CreateGameStartButton();
            CreateBottomButtons();
            CreateProfileSelectionUI();
        }

        private void CreateBackground()
        {
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(mainCanvas.transform, false);
            bgObj.transform.SetAsFirstSibling();
            Image bg = bgObj.AddComponent<Image>();
            bg.color = LobbyDesignTokens.Background;
            bg.raycastTarget = true;

            RectTransform rect = bgObj.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
        }

        private void CreateTopBar()
        {
            GameObject barObj = new GameObject("TopBar");
            barObj.transform.SetParent(safeAreaRoot, false);
            Image barBg = barObj.AddComponent<Image>();
            barBg.color = LobbyDesignTokens.TopBarBg;

            RectTransform barRect = barObj.GetComponent<RectTransform>();
            barRect.anchorMin = new Vector2(0f, 1f);
            barRect.anchorMax = new Vector2(1f, 1f);
            barRect.pivot = new Vector2(0.5f, 1f);
            barRect.sizeDelta = new Vector2(0f, LobbyDesignTokens.TopBarHeight);

            CreateProfileSection(barObj.transform);
            CreateCurrencyDisplays(barObj.transform);
            CreateIconButtons(barObj.transform);
        }

        private void CreateProfileSection(Transform parent)
        {
            GameObject profileObj = new GameObject("ProfileSection");
            profileObj.transform.SetParent(parent, false);

            RectTransform profileRect = profileObj.AddComponent<RectTransform>();
            profileRect.anchorMin = new Vector2(0f, 0f);
            profileRect.anchorMax = new Vector2(0.28f, 1f);
            profileRect.sizeDelta = Vector2.zero;

            HorizontalLayoutGroup hlg = profileObj.AddComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.spacing = 6f;
            hlg.padding = new RectOffset(12, 8, 8, 8);
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;

            Image profileBg = profileObj.AddComponent<Image>();
            profileBg.color = new Color(0.96f, 0.93f, 0.88f, 0.85f);
            Sprite rounded = CuteUIHelper.GetRoundedRectSprite(12);
            if (rounded != null)
            {
                profileBg.sprite = rounded;
                profileBg.type = Image.Type.Sliced;
            }

            Button profileButton = profileObj.AddComponent<Button>();
            ColorBlock colors = profileButton.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 0.98f, 0.95f, 1f);
            colors.pressedColor = new Color(0.9f, 0.87f, 0.82f, 1f);
            colors.fadeDuration = 0.08f;
            profileButton.colors = colors;

            // Avatar border
            GameObject avatarBorderObj = new GameObject("AvatarBorder");
            avatarBorderObj.transform.SetParent(profileObj.transform, false);
            RectTransform avatarBorderRect = avatarBorderObj.AddComponent<RectTransform>();
            avatarBorderRect.sizeDelta = new Vector2(56, 56);

            LayoutElement avatarBorderLE = avatarBorderObj.AddComponent<LayoutElement>();
            avatarBorderLE.preferredWidth = 56;
            avatarBorderLE.preferredHeight = 56;

            Image avatarBorderImage = avatarBorderObj.AddComponent<Image>();
            avatarBorderImage.color = Color.white;
            Sprite borderRounded = CuteUIHelper.GetRoundedRectSprite(10);
            if (borderRounded != null)
            {
                avatarBorderImage.sprite = borderRounded;
                avatarBorderImage.type = Image.Type.Sliced;
            }

            // Avatar icon inside border
            GameObject avatarIconObj = new GameObject("AvatarIcon");
            avatarIconObj.transform.SetParent(avatarBorderObj.transform, false);
            RectTransform avatarIconRect = avatarIconObj.AddComponent<RectTransform>();
            avatarIconRect.anchorMin = new Vector2(0.1f, 0.1f);
            avatarIconRect.anchorMax = new Vector2(0.9f, 0.9f);
            avatarIconRect.offsetMin = Vector2.zero;
            avatarIconRect.offsetMax = Vector2.zero;

            Image avatarIcon = avatarIconObj.AddComponent<Image>();
            avatarIcon.sprite = UnitData.CreateCircleSprite(64);
            avatarIcon.color = Color.white;
            avatarIcon.raycastTarget = false;

            // Nickname text
            GameObject nicknameObj = new GameObject("NicknameText");
            nicknameObj.transform.SetParent(profileObj.transform, false);
            RectTransform nicknameRect = nicknameObj.AddComponent<RectTransform>();
            nicknameRect.sizeDelta = new Vector2(100, 40);

            LayoutElement nicknameLE = nicknameObj.AddComponent<LayoutElement>();
            nicknameLE.preferredWidth = 100;
            nicknameLE.preferredHeight = 40;

            Text nicknameText = CreateText(nicknameObj, "Player", LobbyDesignTokens.BodySize, LobbyDesignTokens.TextPrimary);
            nicknameText.alignment = TextAnchor.MiddleLeft;
            nicknameText.fontStyle = FontStyle.Bold;
            nicknameText.raycastTarget = false;
            nicknameText.resizeTextForBestFit = true;
            nicknameText.resizeTextMinSize = 16;
            nicknameText.resizeTextMaxSize = LobbyDesignTokens.BodySize;

            // Wire ProfileHeaderDisplay
            profileHeaderDisplay = profileObj.AddComponent<ProfileHeaderDisplay>();
            SetField(profileHeaderDisplay, "avatarImage", avatarIcon);
            SetField(profileHeaderDisplay, "borderImage", avatarBorderImage);
            SetField(profileHeaderDisplay, "nicknameText", nicknameText);
            SetField(profileHeaderDisplay, "profileButton", profileButton);
        }

        private void CreateCurrencyDisplays(Transform parent)
        {
            // Gold display
            GameObject goldObj = new GameObject("GoldDisplay");
            goldObj.transform.SetParent(parent, false);

            RectTransform goldRect = goldObj.AddComponent<RectTransform>();
            goldRect.anchorMin = new Vector2(0.28f, 0f);
            goldRect.anchorMax = new Vector2(0.46f, 1f);
            goldRect.sizeDelta = Vector2.zero;

            HorizontalLayoutGroup goldLayout = goldObj.AddComponent<HorizontalLayoutGroup>();
            goldLayout.childAlignment = TextAnchor.MiddleCenter;
            goldLayout.spacing = 8f;
            goldLayout.padding = new RectOffset(20, 10, 10, 10);
            goldLayout.childForceExpandWidth = false;
            goldLayout.childForceExpandHeight = false;

            // Gold icon text
            GameObject goldIconObj = new GameObject("GoldIcon");
            goldIconObj.transform.SetParent(goldObj.transform, false);
            Text goldIcon = CreateText(goldIconObj, "G", LobbyDesignTokens.CurrencySize, LobbyDesignTokens.GoldColor);
            goldIcon.fontStyle = FontStyle.Bold;
            LayoutElement goldIconLE = goldIconObj.AddComponent<LayoutElement>();
            goldIconLE.preferredWidth = 40;
            goldIconLE.preferredHeight = 40;

            // Gold amount
            GameObject goldAmountObj = new GameObject("GoldAmount");
            goldAmountObj.transform.SetParent(goldObj.transform, false);
            goldText = CreateText(goldAmountObj, "0", LobbyDesignTokens.CurrencySize, LobbyDesignTokens.GoldColor);
            goldText.alignment = TextAnchor.MiddleLeft;
            LayoutElement goldAmountLE = goldAmountObj.AddComponent<LayoutElement>();
            goldAmountLE.preferredWidth = 120;
            goldAmountLE.preferredHeight = 40;

            // Ticket display
            GameObject ticketObj = new GameObject("TicketDisplay");
            ticketObj.transform.SetParent(parent, false);

            RectTransform ticketRect = ticketObj.AddComponent<RectTransform>();
            ticketRect.anchorMin = new Vector2(0.46f, 0f);
            ticketRect.anchorMax = new Vector2(0.60f, 1f);
            ticketRect.sizeDelta = Vector2.zero;

            HorizontalLayoutGroup ticketLayout = ticketObj.AddComponent<HorizontalLayoutGroup>();
            ticketLayout.childAlignment = TextAnchor.MiddleCenter;
            ticketLayout.spacing = 8f;
            ticketLayout.padding = new RectOffset(10, 10, 10, 10);
            ticketLayout.childForceExpandWidth = false;
            ticketLayout.childForceExpandHeight = false;

            // Ticket icon text
            GameObject ticketIconObj = new GameObject("TicketIcon");
            ticketIconObj.transform.SetParent(ticketObj.transform, false);
            Text ticketIcon = CreateText(ticketIconObj, "T", LobbyDesignTokens.CurrencySize, LobbyDesignTokens.TicketColor);
            ticketIcon.fontStyle = FontStyle.Bold;
            LayoutElement ticketIconLE = ticketIconObj.AddComponent<LayoutElement>();
            ticketIconLE.preferredWidth = 40;
            ticketIconLE.preferredHeight = 40;

            // Ticket amount
            GameObject ticketAmountObj = new GameObject("TicketAmount");
            ticketAmountObj.transform.SetParent(ticketObj.transform, false);
            ticketText = CreateText(ticketAmountObj, "5", LobbyDesignTokens.CurrencySize, LobbyDesignTokens.TicketColor);
            ticketText.alignment = TextAnchor.MiddleLeft;
            LayoutElement ticketAmountLE = ticketAmountObj.AddComponent<LayoutElement>();
            ticketAmountLE.preferredWidth = 80;
            ticketAmountLE.preferredHeight = 40;

            // Refresh values
            RefreshCurrencyDisplay();
        }

        private void CreateIconButtons(Transform parent)
        {
            // Right container
            GameObject rightObj = new GameObject("IconButtons");
            rightObj.transform.SetParent(parent, false);

            RectTransform rightRect = rightObj.AddComponent<RectTransform>();
            rightRect.anchorMin = new Vector2(0.60f, 0f);
            rightRect.anchorMax = new Vector2(1f, 1f);
            rightRect.sizeDelta = Vector2.zero;

            HorizontalLayoutGroup layout = rightObj.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleRight;
            layout.spacing = LobbyDesignTokens.IconSpacing;
            layout.padding = new RectOffset(10, 20, 10, 10);
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            // Spacer to push buttons right
            GameObject spacer = new GameObject("Spacer");
            spacer.transform.SetParent(rightObj.transform, false);
            spacer.AddComponent<RectTransform>();
            LayoutElement spacerLE = spacer.AddComponent<LayoutElement>();
            spacerLE.flexibleWidth = 1;

            // Daily Reward button
            GameObject dailyBtn = CreateIconButton(rightObj.transform, "DailyReward", "Rv");
            dailyBtn.GetComponent<Button>().onClick.AddListener(OnDailyRewardClicked);
            dailyRewardBadge = NotificationBadge.Create(dailyBtn.transform, defaultFont);

            // Shop button
            GameObject shopBtn = CreateIconButton(rightObj.transform, "Shop", "Sh");
            shopBtn.GetComponent<Button>().onClick.AddListener(OnShopClicked);
            shopBadge = NotificationBadge.Create(shopBtn.transform, defaultFont);

            // Quest button
            GameObject questBtn = CreateIconButton(rightObj.transform, "Quest", "Qt");
            questBtn.GetComponent<Button>().onClick.AddListener(OnQuestClicked);
            questBadge = NotificationBadge.Create(questBtn.transform, defaultFont);

            // Synthesis button (조합)
            GameObject synthesisBtn = CreateIconButton(rightObj.transform, "Synthesis", "조합");
            synthesisBtn.GetComponent<Button>().onClick.AddListener(() => {
                Debug.Log("[MainGameBootstrapper] Synthesis clicked - TODO: Show synthesis UI");
            });

            // Mailbox button
            GameObject mailBtn = CreateIconButton(rightObj.transform, "Mailbox", "Ma");
            mailBtn.GetComponent<Button>().onClick.AddListener(OnMailboxClicked);
            mailBadge = NotificationBadge.Create(mailBtn.transform, defaultFont);
        }

        private GameObject CreateIconButton(Transform parent, string name, string label)
        {
            GameObject btnObj = new GameObject(name + "Button");
            btnObj.transform.SetParent(parent, false);

            Image bg = btnObj.AddComponent<Image>();
            bg.color = LobbyDesignTokens.ButtonSecondary;
            Sprite rounded = CuteUIHelper.GetRoundedRectSprite(14);
            if (rounded != null)
            {
                bg.sprite = rounded;
                bg.type = Image.Type.Sliced;
            }

            Shadow shadow = btnObj.AddComponent<Shadow>();
            shadow.effectColor = CuteUIHelper.SoftShadow;
            shadow.effectDistance = new Vector2(1, -2);

            Button btn = btnObj.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 0.98f, 0.95f, 1f);
            colors.pressedColor = new Color(0.85f, 0.82f, 0.78f, 1f);
            colors.fadeDuration = 0.08f;
            btn.colors = colors;

            LayoutElement le = btnObj.AddComponent<LayoutElement>();
            le.preferredWidth = LobbyDesignTokens.IconButtonSize;
            le.preferredHeight = LobbyDesignTokens.IconButtonSize;

            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(btnObj.transform, false);
            Text text = CreateText(labelObj, label, LobbyDesignTokens.IconButtonFontSize, LobbyDesignTokens.TextPrimary);
            text.fontStyle = FontStyle.Bold;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 16;
            text.resizeTextMaxSize = LobbyDesignTokens.IconButtonFontSize;

            RectTransform labelRect = labelObj.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(4, 4);
            labelRect.offsetMax = new Vector2(-4, -4);

            return btnObj;
        }

        private void CreateTitle()
        {
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(safeAreaRoot, false);
            Text titleText = CreateText(titleObj, "LOTTO DEFENSE", LobbyDesignTokens.TitleSize, LobbyDesignTokens.TextPrimary);
            titleText.fontStyle = FontStyle.Bold;

            RectTransform rect = titleObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.05f, 0.46f);
            rect.anchorMax = new Vector2(0.95f, 0.58f);
            rect.sizeDelta = Vector2.zero;
        }

        private void CreateGameStartButton()
        {
            GameObject container = new GameObject("PlayButtonsContainer");
            container.transform.SetParent(safeAreaRoot, false);

            RectTransform containerRect = container.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.08f, 0.22f);
            containerRect.anchorMax = new Vector2(0.92f, 0.42f);
            containerRect.sizeDelta = Vector2.zero;

            VerticalLayoutGroup vlg = container.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 20;
            vlg.padding = new RectOffset(0, 0, 0, 0);
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childAlignment = TextAnchor.MiddleCenter;

            // Row: Single + Coop side by side
            GameObject topRow = new GameObject("PlayRow");
            topRow.transform.SetParent(container.transform, false);
            LayoutElement topRowLE = topRow.AddComponent<LayoutElement>();
            topRowLE.preferredHeight = LobbyDesignTokens.GameStartButtonHeight;

            HorizontalLayoutGroup hlg = topRow.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 20;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            CreatePlayButton(topRow.transform, "SinglePlayButton", "싱글 플레이",
                LobbyDesignTokens.ButtonPrimary, () => OnSinglePlayClicked());
            CreatePlayButton(topRow.transform, "CoopPlayButton", "협동 플레이",
                new Color(0.95f, 0.65f, 0.4f), () => OnCoopPlayClicked());

            // Ranking button (full width below)
            Button rankingBtn = CreatePlayButton(container.transform, "RankingButton", "랭킹",
                LobbyDesignTokens.ButtonSuccess, () => {
                    SceneNavigator nav = FindFirstObjectByType<SceneNavigator>();
                    if (nav != null) nav.ShowRankings();
                });
            LayoutElement rankLE = rankingBtn.gameObject.AddComponent<LayoutElement>();
            rankLE.preferredHeight = 90;
        }

        private Button CreatePlayButton(Transform parent, string name, string label, Color color, UnityEngine.Events.UnityAction onClick)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);
            btnObj.AddComponent<RectTransform>();

            Image bg = btnObj.AddComponent<Image>();
            bg.color = color;
            Sprite rounded = CuteUIHelper.GetRoundedRectSprite(20);
            if (rounded != null)
            {
                bg.sprite = rounded;
                bg.type = Image.Type.Sliced;
            }

            Shadow shadow = btnObj.AddComponent<Shadow>();
            shadow.effectColor = CuteUIHelper.SoftShadow;
            shadow.effectDistance = new Vector2(2, -3);

            Button btn = btnObj.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 0.98f, 0.95f, 1f);
            colors.pressedColor = new Color(0.85f, 0.82f, 0.78f, 1f);
            colors.fadeDuration = 0.08f;
            btn.colors = colors;
            btn.onClick.AddListener(onClick);

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(12, 8);
            textRect.offsetMax = new Vector2(-12, -8);

            Text text = CreateText(textObj, label, LobbyDesignTokens.ButtonFontSize + 8, LobbyDesignTokens.ButtonText);
            text.fontStyle = FontStyle.Bold;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 24;
            text.resizeTextMaxSize = LobbyDesignTokens.ButtonFontSize + 8;

            return btn;
        }

        private void CreateBottomButtons()
        {
            // Bottom row container
            GameObject bottomRow = new GameObject("BottomButtons");
            bottomRow.transform.SetParent(safeAreaRoot, false);

            RectTransform bottomRect = bottomRow.AddComponent<RectTransform>();
            bottomRect.anchorMin = new Vector2(0.08f, 0f);
            bottomRect.anchorMax = new Vector2(0.92f, 0f);
            bottomRect.pivot = new Vector2(0.5f, 0f);
            bottomRect.anchoredPosition = new Vector2(0, 24);
            bottomRect.sizeDelta = new Vector2(0, 60);

            HorizontalLayoutGroup hlg = bottomRow.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 16;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            CreateBottomButton(bottomRow.transform, "SettingsButton", "설정",
                LobbyDesignTokens.ButtonSecondary,
                () => Debug.Log("[MainGameBootstrapper] Settings - Coming soon"));

            CreateBottomButton(bottomRow.transform, "LogoutButton", "로그아웃",
                LobbyDesignTokens.ButtonDanger, OnLogoutClicked);
        }

        private void CreateBottomButton(Transform parent, string name, string label, Color color, UnityEngine.Events.UnityAction onClick)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);

            Image bg = btnObj.AddComponent<Image>();
            bg.color = color;
            Sprite rounded = CuteUIHelper.GetRoundedRectSprite(12);
            if (rounded != null)
            {
                bg.sprite = rounded;
                bg.type = Image.Type.Sliced;
            }

            Shadow shadow = btnObj.AddComponent<Shadow>();
            shadow.effectColor = CuteUIHelper.SoftShadow;
            shadow.effectDistance = new Vector2(1, -2);

            Button btn = btnObj.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 0.98f, 0.95f, 1f);
            colors.pressedColor = new Color(0.85f, 0.82f, 0.78f, 1f);
            colors.fadeDuration = 0.08f;
            btn.colors = colors;
            btn.onClick.AddListener(onClick);

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(8, 4);
            textRect.offsetMax = new Vector2(-8, -4);

            Text text = CreateText(textObj, label, LobbyDesignTokens.BodySize, LobbyDesignTokens.TextPrimary);
            text.fontStyle = FontStyle.Bold;
        }
        private void CreateProfileSelectionUI()
        {
            GameObject overlayObj = new GameObject("ProfileSelectionOverlay");
            overlayObj.transform.SetParent(mainCanvas.transform, false);

            RectTransform overlayRect = overlayObj.AddComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.sizeDelta = Vector2.zero;

            Image overlayImg = overlayObj.AddComponent<Image>();
            overlayImg.color = LobbyDesignTokens.ModalOverlay;
            overlayImg.raycastTarget = true;

            Canvas overlayCanvas = overlayObj.AddComponent<Canvas>();
            overlayCanvas.overrideSorting = true;
            overlayCanvas.sortingOrder = 50;
            overlayObj.AddComponent<GraphicRaycaster>();

            GameObject panelObj = new GameObject("ProfilePanel");
            panelObj.transform.SetParent(overlayObj.transform, false);

            RectTransform panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.05f, 0.3f);
            panelRect.anchorMax = new Vector2(0.95f, 0.75f);
            panelRect.sizeDelta = Vector2.zero;

            Image panelBg = panelObj.AddComponent<Image>();
            panelBg.color = LobbyDesignTokens.ModalPanelBg;
            Sprite panelRounded = CuteUIHelper.GetRoundedRectSprite(24);
            if (panelRounded != null)
            {
                panelBg.sprite = panelRounded;
                panelBg.type = Image.Type.Sliced;
            }

            Shadow panelShadow = panelObj.AddComponent<Shadow>();
            panelShadow.effectColor = CuteUIHelper.SoftShadow;
            panelShadow.effectDistance = new Vector2(3, -4);

            VerticalLayoutGroup panelVLG = panelObj.AddComponent<VerticalLayoutGroup>();
            panelVLG.padding = new RectOffset(8, 8, 12, 12);
            panelVLG.spacing = 8;
            panelVLG.childControlWidth = true;
            panelVLG.childControlHeight = false;
            panelVLG.childForceExpandWidth = true;
            panelVLG.childForceExpandHeight = false;

            // Title row with close button
            GameObject titleRow = new GameObject("TitleRow");
            titleRow.transform.SetParent(panelObj.transform, false);
            LayoutElement titleLE = titleRow.AddComponent<LayoutElement>();
            titleLE.preferredHeight = 44;

            Text titleText = CreateText(titleRow, "프로필 선택", LobbyDesignTokens.HeaderSize, LobbyDesignTokens.TextPrimary);
            titleText.alignment = TextAnchor.MiddleCenter;

            GameObject closeObj = new GameObject("CloseButton");
            closeObj.transform.SetParent(titleRow.transform, false);

            RectTransform closeRect = closeObj.AddComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(1f, 0.5f);
            closeRect.anchorMax = new Vector2(1f, 0.5f);
            closeRect.pivot = new Vector2(1f, 0.5f);
            closeRect.anchoredPosition = Vector2.zero;
            closeRect.sizeDelta = new Vector2(40, 40);

            Image closeBg = closeObj.AddComponent<Image>();
            closeBg.color = LobbyDesignTokens.ButtonClose;
            Sprite closeRounded = CuteUIHelper.GetRoundedRectSprite(10);
            if (closeRounded != null)
            {
                closeBg.sprite = closeRounded;
                closeBg.type = Image.Type.Sliced;
            }

            Button closeButton = closeObj.AddComponent<Button>();

            GameObject closeTextObj = new GameObject("X");
            closeTextObj.transform.SetParent(closeObj.transform, false);
            RectTransform closeTextRect = closeTextObj.AddComponent<RectTransform>();
            closeTextRect.anchorMin = Vector2.zero;
            closeTextRect.anchorMax = Vector2.one;
            closeTextRect.sizeDelta = Vector2.zero;
            CreateText(closeTextObj, "X", 22, Color.white);

            // Avatar grid (scrollable)
            GameObject scrollObj = new GameObject("AvatarScroll");
            scrollObj.transform.SetParent(panelObj.transform, false);
            LayoutElement scrollLE = scrollObj.AddComponent<LayoutElement>();
            scrollLE.flexibleHeight = 1;

            Image scrollBg = scrollObj.AddComponent<Image>();
            scrollBg.color = new Color(0.94f, 0.92f, 0.9f, 0.3f);

            ScrollRect scroll = scrollObj.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scrollObj.AddComponent<RectMask2D>();

            GameObject gridContent = new GameObject("GridContent");
            gridContent.transform.SetParent(scrollObj.transform, false);
            RectTransform gridContentRect = gridContent.AddComponent<RectTransform>();
            gridContentRect.anchorMin = new Vector2(0f, 1f);
            gridContentRect.anchorMax = new Vector2(1f, 1f);
            gridContentRect.pivot = new Vector2(0.5f, 1f);
            gridContentRect.sizeDelta = new Vector2(0, 300);

            GridLayoutGroup gridLayout = gridContent.AddComponent<GridLayoutGroup>();
            gridLayout.cellSize = new Vector2(100, 155);
            gridLayout.spacing = new Vector2(8, 8);
            gridLayout.padding = new RectOffset(4, 4, 6, 6);
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 4;
            gridLayout.childAlignment = TextAnchor.UpperCenter;

            ContentSizeFitter gridFitter = gridContent.AddComponent<ContentSizeFitter>();
            gridFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.content = gridContentRect;

            // Wire ProfileSelectionUI component (only panel, close, grid)
            profileSelectionUI = overlayObj.AddComponent<ProfileSelectionUI>();
            SetField(profileSelectionUI, "panel", overlayObj);
            SetField(profileSelectionUI, "closeButton", closeButton);
            SetField(profileSelectionUI, "avatarGridContainer", gridContent.transform);

            overlayObj.SetActive(false);

            if (profileHeaderDisplay != null)
                profileHeaderDisplay.SetProfileSelectionUI(profileSelectionUI);
        }
        #endregion

        #region Event Handlers
        private void OnGameStartClicked()
        {
            Debug.Log("[MainGameBootstrapper] Game start clicked");
            SceneNavigator nav = FindFirstObjectByType<SceneNavigator>();
            if (nav == null)
            {
                GameObject navObj = new GameObject("SceneNavigator");
                nav = navObj.AddComponent<SceneNavigator>();
            }
            nav.TryStartGame();
        }

        private void OnShopClicked()
        {
            Debug.Log("[MainGameBootstrapper] Shop clicked");
            if (shopUI == null)
                shopUI = UnitShopUI.Create(mainCanvas, defaultFont);
            shopUI.Show();
        }

        private void OnDailyRewardClicked()
        {
            Debug.Log("[MainGameBootstrapper] Daily reward clicked");
            if (dailyRewardUI == null)
                dailyRewardUI = DailyRewardUI.Create(mainCanvas, defaultFont);
            dailyRewardUI.Show();
        }

        private void OnQuestClicked()
        {
            Debug.Log("[MainGameBootstrapper] Quest clicked");
            if (questUI == null)
                questUI = LobbyQuestUI.Create(mainCanvas, defaultFont);
            questUI.Show();
        }

        private void OnMailboxClicked()
        {
            Debug.Log("[MainGameBootstrapper] Mailbox clicked");
            if (mailboxUI == null)
                mailboxUI = MailboxUI.Create(mainCanvas, defaultFont);
            mailboxUI.Show();
        }

        private void OnLogoutClicked()
        {
            Debug.Log("[MainGameBootstrapper] Logout clicked");
            var auth = FindFirstObjectByType<AuthenticationManager>();
            if (auth != null) auth.Logout();
            SceneManager.LoadScene("LoginScene");
        }
        #endregion

        #region Currency Display
        private void RefreshCurrencyDisplay()
        {
            if (dataManager == null) return;
            if (goldText != null) goldText.text = dataManager.Gold.ToString();
            if (ticketText != null) ticketText.text = dataManager.Tickets.ToString();
        }
        #endregion

        #region Badge Refresh
        public void RefreshAllBadges()
        {
            RefreshDailyRewardBadge();
            RefreshShopBadge();
            RefreshQuestBadge();
            RefreshMailBadge();
        }

        private void RefreshDailyRewardBadge()
        {
            if (dailyRewardBadge == null || dailyRewardManager == null) return;
            int claimable = dailyRewardManager.GetClaimableStageCount();
            if (claimable > 0) dailyRewardBadge.Show(claimable);
            else dailyRewardBadge.Hide();
        }

        private void RefreshShopBadge()
        {
            if (shopBadge == null || shopManager == null || dataManager == null) return;
            int affordable = shopManager.GetAffordableUnlockCount();
            if (affordable > 0) shopBadge.Show(affordable);
            else shopBadge.Hide();
        }

        private void RefreshQuestBadge()
        {
            if (questBadge == null || questManager == null) return;
            int claimable = questManager.GetClaimableQuestCount();
            if (claimable > 0) questBadge.Show(claimable);
            else questBadge.Hide();
        }

        private void RefreshMailBadge()
        {
            if (mailBadge == null || mailboxManager == null) return;
            int unclaimed = mailboxManager.GetUnclaimedMailCount();
            if (unclaimed > 0) mailBadge.Show(unclaimed);
            else mailBadge.Hide();
        }
        #endregion

        #region Event Subscription
        private void SubscribeToEvents()
        {
            if (dataManager != null)
            {
                dataManager.OnGoldChanged += HandleGoldChanged;
                dataManager.OnTicketsChanged += HandleTicketsChanged;
                dataManager.OnDataChanged += HandleDataChanged;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (dataManager != null)
            {
                dataManager.OnGoldChanged -= HandleGoldChanged;
                dataManager.OnTicketsChanged -= HandleTicketsChanged;
                dataManager.OnDataChanged -= HandleDataChanged;
            }
        }

        private void HandleGoldChanged(int newGold)
        {
            if (goldText != null) goldText.text = newGold.ToString();
            RefreshShopBadge();
        }

        private void HandleTicketsChanged(int newTickets)
        {
            if (ticketText != null) ticketText.text = newTickets.ToString();
        }

        private void HandleDataChanged()
        {
            RefreshAllBadges();
        }
        #endregion

        #region Helpers
        private static void SetField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName,
                BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(target, value);
        }

        private Text CreateText(GameObject obj, string text, int fontSize, Color color)
        {
            Text t = obj.AddComponent<Text>();
            t.font = defaultFont;
            t.text = text;
            t.fontSize = fontSize;
            t.color = color;
            t.fontStyle = FontStyle.Bold;
            t.alignment = TextAnchor.MiddleCenter;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            return t;
        }

        /// <summary>
        /// Create a simple modal popup overlay (dark background + centered panel).
        /// Returns the panel transform to add content to.
        /// </summary>
        public static GameObject CreateModalPopup(Canvas canvas, Font font, string title, float widthRatio, float heightRatio, out GameObject overlay, out Button closeButton)
        {
            // Overlay
            overlay = new GameObject("ModalOverlay");
            overlay.transform.SetParent(canvas.transform, false);
            Image overlayImg = overlay.AddComponent<Image>();
            overlayImg.color = LobbyDesignTokens.ModalOverlay;
            overlayImg.raycastTarget = true;

            RectTransform overlayRect = overlay.GetComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.sizeDelta = Vector2.zero;

            // Panel
            GameObject panel = new GameObject("Panel");
            panel.transform.SetParent(overlay.transform, false);
            Image panelBg = panel.AddComponent<Image>();
            panelBg.color = LobbyDesignTokens.ModalPanelBg;
            Sprite panelRounded = CuteUIHelper.GetRoundedRectSprite(24);
            if (panelRounded != null)
            {
                panelBg.sprite = panelRounded;
                panelBg.type = Image.Type.Sliced;
            }

            Shadow panelShadow = panel.AddComponent<Shadow>();
            panelShadow.effectColor = CuteUIHelper.SoftShadow;
            panelShadow.effectDistance = new Vector2(3, -4);

            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f - widthRatio / 2f, 0.5f - heightRatio / 2f);
            panelRect.anchorMax = new Vector2(0.5f + widthRatio / 2f, 0.5f + heightRatio / 2f);
            panelRect.sizeDelta = Vector2.zero;

            // Title
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(panel.transform, false);
            Text titleText = titleObj.AddComponent<Text>();
            titleText.font = font;
            titleText.text = title;
            titleText.fontSize = LobbyDesignTokens.HeaderSize;
            titleText.color = LobbyDesignTokens.TextPrimary;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.fontStyle = FontStyle.Bold;
            titleText.raycastTarget = false;

            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0f, 0.88f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.sizeDelta = Vector2.zero;

            // Close button
            GameObject closeObj = new GameObject("CloseButton");
            closeObj.transform.SetParent(panel.transform, false);
            Image closeBg = closeObj.AddComponent<Image>();
            closeBg.color = LobbyDesignTokens.ButtonClose;
            Sprite closeRounded = CuteUIHelper.GetRoundedRectSprite(12);
            if (closeRounded != null)
            {
                closeBg.sprite = closeRounded;
                closeBg.type = Image.Type.Sliced;
            }
            closeButton = closeObj.AddComponent<Button>();

            RectTransform closeRect = closeObj.GetComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(1f, 1f);
            closeRect.anchorMax = new Vector2(1f, 1f);
            closeRect.pivot = new Vector2(1f, 1f);
            closeRect.anchoredPosition = new Vector2(-16f, -16f);
            closeRect.sizeDelta = new Vector2(52f, 52f);

            GameObject closeTextObj = new GameObject("X");
            closeTextObj.transform.SetParent(closeObj.transform, false);
            Text closeText = closeTextObj.AddComponent<Text>();
            closeText.font = font;
            closeText.text = "X";
            closeText.fontSize = 28;
            closeText.color = Color.white;
            closeText.alignment = TextAnchor.MiddleCenter;
            closeText.fontStyle = FontStyle.Bold;
            closeText.raycastTarget = false;

            RectTransform closeTextRect = closeTextObj.GetComponent<RectTransform>();
            closeTextRect.anchorMin = Vector2.zero;
            closeTextRect.anchorMax = Vector2.one;
            closeTextRect.sizeDelta = Vector2.zero;

            overlay.SetActive(false);
            return panel;
        }

        private void OnSinglePlayClicked()
        {
            Debug.Log("[MainGameBootstrapper] Single play button clicked - showing difficulty selection");
            LottoDefense.UI.DifficultySelectionUI.Show(false, OnDifficultySelected);
        }

        private void OnCoopPlayClicked()
        {
            Debug.Log("[MainGameBootstrapper] Coop play button clicked - showing difficulty selection");
            LottoDefense.UI.DifficultySelectionUI.Show(true, OnDifficultySelected);
        }

        private void OnDifficultySelected(LottoDefense.Gameplay.GameDifficulty difficulty)
        {
            Debug.Log($"[MainGameBootstrapper] Difficulty selected: {difficulty}");
            
            // 난이도 저장 (게임 시작 전에 GameplayManager가 설정할 수 있도록)
            PlayerPrefs.SetInt("SelectedDifficulty", (int)difficulty);
            PlayerPrefs.Save();
            
            // 게임 시작
            OnGameStartClicked();
        }
        #endregion
    }
}
