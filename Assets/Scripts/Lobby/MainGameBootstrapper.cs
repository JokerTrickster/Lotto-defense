using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using LottoDefense.Authentication;
using LottoDefense.UI;

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
            defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (defaultFont == null)
                defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

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
            // Top bar container
            GameObject barObj = new GameObject("TopBar");
            barObj.transform.SetParent(safeAreaRoot, false);
            Image barBg = barObj.AddComponent<Image>();
            barBg.color = LobbyDesignTokens.TopBarBg;

            RectTransform barRect = barObj.GetComponent<RectTransform>();
            barRect.anchorMin = new Vector2(0f, 1f);
            barRect.anchorMax = new Vector2(1f, 1f);
            barRect.pivot = new Vector2(0.5f, 1f);
            barRect.sizeDelta = new Vector2(0f, LobbyDesignTokens.TopBarHeight);

            // Left section: currencies
            CreateCurrencyDisplays(barObj.transform);

            // Right section: icon buttons
            CreateIconButtons(barObj.transform);
        }

        private void CreateCurrencyDisplays(Transform parent)
        {
            // Gold display
            GameObject goldObj = new GameObject("GoldDisplay");
            goldObj.transform.SetParent(parent, false);

            RectTransform goldRect = goldObj.AddComponent<RectTransform>();
            goldRect.anchorMin = new Vector2(0f, 0f);
            goldRect.anchorMax = new Vector2(0.25f, 1f);
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
            ticketRect.anchorMin = new Vector2(0.25f, 0f);
            ticketRect.anchorMax = new Vector2(0.5f, 1f);
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
            rightRect.anchorMin = new Vector2(0.5f, 0f);
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

            // Mailbox button
            GameObject mailBtn = CreateIconButton(rightObj.transform, "Mailbox", "Ma");
            mailBtn.GetComponent<Button>().onClick.AddListener(OnMailboxClicked);
            mailBadge = NotificationBadge.Create(mailBtn.transform, defaultFont);

            // Ranking button (랭킹)
            GameObject rankingBtn = CreateIconButton(rightObj.transform, "Ranking", "랭킹");
            rankingBtn.GetComponent<Button>().onClick.AddListener(() => {
                Debug.Log("[MainGameBootstrapper] Ranking clicked - TODO: Show rankings");
            });
            // 랭킹 버튼은 약간 크게
            LayoutElement rankingLE = rankingBtn.GetComponent<LayoutElement>();
            rankingLE.preferredWidth = 120;
            rankingLE.preferredHeight = 60;
            // 텍스트 크기 조정
            Text rankingText = rankingBtn.GetComponentInChildren<Text>();
            if (rankingText != null) rankingText.fontSize = 32;
        }

        private GameObject CreateIconButton(Transform parent, string name, string label)
        {
            GameObject btnObj = new GameObject(name + "Button");
            btnObj.transform.SetParent(parent, false);

            Image bg = btnObj.AddComponent<Image>();
            bg.color = LobbyDesignTokens.ButtonSecondary;

            Button btn = btnObj.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.9f, 0.95f, 1f, 1f);
            colors.pressedColor = new Color(0.55f, 0.55f, 0.55f, 1f);
            colors.fadeDuration = 0.06f;
            btn.colors = colors;

            LayoutElement le = btnObj.AddComponent<LayoutElement>();
            le.preferredWidth = LobbyDesignTokens.IconButtonSize;
            le.preferredHeight = LobbyDesignTokens.IconButtonSize;

            // Label
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(btnObj.transform, false);
            Text text = CreateText(labelObj, label, LobbyDesignTokens.IconButtonFontSize, LobbyDesignTokens.TextPrimary);
            text.fontStyle = FontStyle.Bold;

            RectTransform labelRect = labelObj.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.sizeDelta = Vector2.zero;

            return btnObj;
        }

        private void CreateTitle()
        {
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(safeAreaRoot, false);
            Text titleText = CreateText(titleObj, "LOTTO DEFENSE", LobbyDesignTokens.TitleSize, LobbyDesignTokens.TextPrimary);
            titleText.fontStyle = FontStyle.Bold;

            RectTransform rect = titleObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.05f, 0.45f);
            rect.anchorMax = new Vector2(0.95f, 0.6f);
            rect.sizeDelta = Vector2.zero;
        }

        private void CreateGameStartButton()
        {
            // ===== 싱글 플레이 버튼 (중앙 왼쪽) =====
            GameObject singleBtn = new GameObject("SinglePlayButton");
            singleBtn.transform.SetParent(safeAreaRoot, false);

            RectTransform singleRect = singleBtn.AddComponent<RectTransform>();
            singleRect.anchorMin = new Vector2(0.5f, 0.4f);
            singleRect.anchorMax = new Vector2(0.5f, 0.4f);
            singleRect.pivot = new Vector2(0.5f, 0.5f);
            singleRect.anchoredPosition = new Vector2(-220, 0); // 왼쪽으로
            singleRect.sizeDelta = new Vector2(400, 150);

            Image singleBg = singleBtn.AddComponent<Image>();
            singleBg.color = new Color(0.2f, 0.6f, 1f); // 파란색

            Button singleButton = singleBtn.AddComponent<Button>();
            ColorBlock singleColors = singleButton.colors;
            singleColors.normalColor = Color.white;
            singleColors.highlightedColor = new Color(0.9f, 0.95f, 1f, 1f);
            singleColors.pressedColor = new Color(0.55f, 0.55f, 0.55f, 1f);
            singleColors.fadeDuration = 0.06f;
            singleButton.colors = singleColors;
            singleButton.onClick.AddListener(OnGameStartClicked); // 기존 동작 유지

            GameObject singleTextObj = new GameObject("Text");
            singleTextObj.transform.SetParent(singleBtn.transform, false);
            Text singleText = CreateText(singleTextObj, "싱글 플레이", 48, LobbyDesignTokens.ButtonText);
            singleText.fontStyle = FontStyle.Bold;

            RectTransform singleTextRect = singleTextObj.GetComponent<RectTransform>();
            singleTextRect.anchorMin = Vector2.zero;
            singleTextRect.anchorMax = Vector2.one;
            singleTextRect.sizeDelta = Vector2.zero;

            // ===== 협동 플레이 버튼 (중앙 오른쪽) =====
            GameObject coopBtn = new GameObject("CoopPlayButton");
            coopBtn.transform.SetParent(safeAreaRoot, false);

            RectTransform coopRect = coopBtn.AddComponent<RectTransform>();
            coopRect.anchorMin = new Vector2(0.5f, 0.4f);
            coopRect.anchorMax = new Vector2(0.5f, 0.4f);
            coopRect.pivot = new Vector2(0.5f, 0.5f);
            coopRect.anchoredPosition = new Vector2(220, 0); // 오른쪽으로
            coopRect.sizeDelta = new Vector2(400, 150);

            Image coopBg = coopBtn.AddComponent<Image>();
            coopBg.color = new Color(0.9f, 0.5f, 0.2f); // 주황색

            Button coopButton = coopBtn.AddComponent<Button>();
            ColorBlock coopColors = coopButton.colors;
            coopColors.normalColor = Color.white;
            coopColors.highlightedColor = new Color(1f, 0.95f, 0.9f, 1f);
            coopColors.pressedColor = new Color(0.55f, 0.55f, 0.55f, 1f);
            coopColors.fadeDuration = 0.06f;
            coopButton.colors = coopColors;
            coopButton.onClick.AddListener(() => {
                Debug.Log("[MainGameBootstrapper] Coop play clicked - TODO: Show multiplayer lobby");
            });

            GameObject coopTextObj = new GameObject("Text");
            coopTextObj.transform.SetParent(coopBtn.transform, false);
            Text coopText = CreateText(coopTextObj, "협동 플레이", 48, LobbyDesignTokens.ButtonText);
            coopText.fontStyle = FontStyle.Bold;

            RectTransform coopTextRect = coopTextObj.GetComponent<RectTransform>();
            coopTextRect.anchorMin = Vector2.zero;
            coopTextRect.anchorMax = Vector2.one;
            coopTextRect.sizeDelta = Vector2.zero;
        }

        private void CreateBottomButtons()
        {
            // Settings button (bottom left) - placeholder
            GameObject settingsObj = new GameObject("SettingsButton");
            settingsObj.transform.SetParent(safeAreaRoot, false);

            Image settingsBg = settingsObj.AddComponent<Image>();
            settingsBg.color = LobbyDesignTokens.ButtonSecondary;

            Button settingsBtn = settingsObj.AddComponent<Button>();
            settingsBtn.onClick.AddListener(() => Debug.Log("[MainGameBootstrapper] Settings - Coming soon"));

            RectTransform settingsRect = settingsObj.GetComponent<RectTransform>();
            settingsRect.anchorMin = new Vector2(0f, 0f);
            settingsRect.anchorMax = new Vector2(0f, 0f);
            settingsRect.pivot = new Vector2(0f, 0f);
            settingsRect.anchoredPosition = new Vector2(20f, 20f);
            settingsRect.sizeDelta = new Vector2(120f, 50f);

            GameObject settingsTextObj = new GameObject("Text");
            settingsTextObj.transform.SetParent(settingsObj.transform, false);
            CreateText(settingsTextObj, "설정", LobbyDesignTokens.BodySize, LobbyDesignTokens.TextPrimary);
            RectTransform stRect = settingsTextObj.GetComponent<RectTransform>();
            stRect.anchorMin = Vector2.zero;
            stRect.anchorMax = Vector2.one;
            stRect.sizeDelta = Vector2.zero;

            // Logout button (bottom right)
            GameObject logoutObj = new GameObject("LogoutButton");
            logoutObj.transform.SetParent(safeAreaRoot, false);

            Image logoutBg = logoutObj.AddComponent<Image>();
            logoutBg.color = LobbyDesignTokens.ButtonDanger;

            Button logoutBtn = logoutObj.AddComponent<Button>();
            logoutBtn.onClick.AddListener(OnLogoutClicked);

            RectTransform logoutRect = logoutObj.GetComponent<RectTransform>();
            logoutRect.anchorMin = new Vector2(1f, 0f);
            logoutRect.anchorMax = new Vector2(1f, 0f);
            logoutRect.pivot = new Vector2(1f, 0f);
            logoutRect.anchoredPosition = new Vector2(-20f, 20f);
            logoutRect.sizeDelta = new Vector2(140f, 50f);

            GameObject logoutTextObj = new GameObject("Text");
            logoutTextObj.transform.SetParent(logoutObj.transform, false);
            CreateText(logoutTextObj, "로그아웃", LobbyDesignTokens.BodySize, LobbyDesignTokens.TextPrimary);
            RectTransform ltRect = logoutTextObj.GetComponent<RectTransform>();
            ltRect.anchorMin = Vector2.zero;
            ltRect.anchorMax = Vector2.one;
            ltRect.sizeDelta = Vector2.zero;
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
        private Text CreateText(GameObject obj, string text, int fontSize, Color color)
        {
            Text t = obj.AddComponent<Text>();
            t.font = defaultFont;
            t.text = text;
            t.fontSize = fontSize;
            t.color = color;
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
            closeButton = closeObj.AddComponent<Button>();

            RectTransform closeRect = closeObj.GetComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(1f, 1f);
            closeRect.anchorMax = new Vector2(1f, 1f);
            closeRect.pivot = new Vector2(1f, 1f);
            closeRect.anchoredPosition = new Vector2(-10f, -10f);
            closeRect.sizeDelta = new Vector2(60f, 60f);

            GameObject closeTextObj = new GameObject("X");
            closeTextObj.transform.SetParent(closeObj.transform, false);
            Text closeText = closeTextObj.AddComponent<Text>();
            closeText.font = font;
            closeText.text = "X";
            closeText.fontSize = LobbyDesignTokens.SubHeaderSize;
            closeText.color = Color.white;
            closeText.alignment = TextAnchor.MiddleCenter;
            closeText.raycastTarget = false;

            RectTransform closeTextRect = closeTextObj.GetComponent<RectTransform>();
            closeTextRect.anchorMin = Vector2.zero;
            closeTextRect.anchorMax = Vector2.one;
            closeTextRect.sizeDelta = Vector2.zero;

            overlay.SetActive(false);
            return panel;
        }
        #endregion
    }
}
