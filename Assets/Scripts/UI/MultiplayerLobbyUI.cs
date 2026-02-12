using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using LottoDefense.Networking;

namespace LottoDefense.UI
{
    /// <summary>
    /// Multiplayer lobby UI overlay for MainGame scene.
    /// Supports room creation, code-based join, and auto-match.
    /// </summary>
    public class MultiplayerLobbyUI : MonoBehaviour
    {
        #region Serialized Fields
        [SerializeField] private GameObject lobbyPanel;
        [SerializeField] private Button closeButton;

        // Server connection
        [SerializeField] private InputField serverUrlInput;
        [SerializeField] private InputField playerNameInput;
        [SerializeField] private Button connectButton;
        [SerializeField] private Text connectionStatusText;

        // Room actions (shown after connection)
        [SerializeField] private GameObject roomActionsPanel;
        [SerializeField] private Button createRoomButton;
        [SerializeField] private Button joinRoomButton;
        [SerializeField] private Button autoMatchButton;
        [SerializeField] private InputField roomCodeInput;

        // Waiting room (shown after room create/join)
        [SerializeField] private GameObject waitingPanel;
        [SerializeField] private Text roomCodeDisplay;
        [SerializeField] private Text waitingStatusText;
        [SerializeField] private Button readyButton;
        [SerializeField] private Button leaveButton;

        // Error display
        [SerializeField] private Text errorText;
        #endregion

        #region Private Fields
        private Font defaultFont;
        private const string DEFAULT_SERVER_URL = "ws://localhost:8080/ws";
        private const string DEFAULT_PLAYER_NAME = "Player";
        #endregion

        #region Unity Lifecycle
        private void Start()
        {
            defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (defaultFont == null)
                defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

            SetupButtonListeners();
            ShowConnectionPanel();
        }

        private void OnEnable()
        {
            if (MultiplayerManager.Instance != null)
            {
                MultiplayerManager.Instance.OnConnected += HandleConnected;
                MultiplayerManager.Instance.OnDisconnected += HandleDisconnected;
                MultiplayerManager.Instance.OnRoomCreated += HandleRoomCreated;
                MultiplayerManager.Instance.OnPlayerJoined += HandlePlayerJoined;
                MultiplayerManager.Instance.OnMatchStart += HandleMatchStart;
                MultiplayerManager.Instance.OnError += HandleError;
            }
        }

        private void OnDisable()
        {
            if (MultiplayerManager.Instance != null)
            {
                MultiplayerManager.Instance.OnConnected -= HandleConnected;
                MultiplayerManager.Instance.OnDisconnected -= HandleDisconnected;
                MultiplayerManager.Instance.OnRoomCreated -= HandleRoomCreated;
                MultiplayerManager.Instance.OnPlayerJoined -= HandlePlayerJoined;
                MultiplayerManager.Instance.OnMatchStart -= HandleMatchStart;
                MultiplayerManager.Instance.OnError -= HandleError;
            }
        }
        #endregion

        #region Setup
        private void SetupButtonListeners()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);

            if (connectButton != null)
                connectButton.onClick.AddListener(OnConnectClicked);

            if (createRoomButton != null)
                createRoomButton.onClick.AddListener(OnCreateRoomClicked);

            if (joinRoomButton != null)
                joinRoomButton.onClick.AddListener(OnJoinRoomClicked);

            if (autoMatchButton != null)
                autoMatchButton.onClick.AddListener(OnAutoMatchClicked);

            if (readyButton != null)
                readyButton.onClick.AddListener(OnReadyClicked);

            if (leaveButton != null)
                leaveButton.onClick.AddListener(OnLeaveClicked);
        }
        #endregion

        #region Show/Hide
        public void Show()
        {
            gameObject.SetActive(true);

            // Subscribe to MultiplayerManager events if it exists now
            if (MultiplayerManager.Instance != null)
            {
                // Re-subscribe in case OnEnable didn't fire with valid Instance
                OnDisable();
                OnEnable();

                // Restore UI state based on current multiplayer state
                switch (MultiplayerManager.Instance.CurrentState)
                {
                    case MultiplayerState.InLobby:
                        ShowRoomActions();
                        break;
                    case MultiplayerState.InRoom:
                        ShowWaitingRoom(MultiplayerManager.Instance.RoomCode);
                        break;
                    default:
                        ShowConnectionPanel();
                        break;
                }
            }
            else
            {
                ShowConnectionPanel();
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
        #endregion

        #region UI State
        private void ShowConnectionPanel()
        {
            if (roomActionsPanel != null) roomActionsPanel.SetActive(false);
            if (waitingPanel != null) waitingPanel.SetActive(false);

            if (serverUrlInput != null)
            {
                serverUrlInput.gameObject.SetActive(true);
                if (string.IsNullOrEmpty(serverUrlInput.text))
                    serverUrlInput.text = DEFAULT_SERVER_URL;
            }
            if (playerNameInput != null)
            {
                playerNameInput.gameObject.SetActive(true);
                if (string.IsNullOrEmpty(playerNameInput.text))
                    playerNameInput.text = DEFAULT_PLAYER_NAME;
            }
            if (connectButton != null) connectButton.gameObject.SetActive(true);
            if (connectionStatusText != null) connectionStatusText.text = "";
            ClearError();
        }

        private void ShowRoomActions()
        {
            if (serverUrlInput != null) serverUrlInput.gameObject.SetActive(false);
            if (playerNameInput != null) playerNameInput.gameObject.SetActive(false);
            if (connectButton != null) connectButton.gameObject.SetActive(false);
            if (waitingPanel != null) waitingPanel.SetActive(false);

            if (roomActionsPanel != null) roomActionsPanel.SetActive(true);
            if (connectionStatusText != null)
                connectionStatusText.text = "서버 연결됨";
            ClearError();
        }

        private void ShowWaitingRoom(string roomCode)
        {
            if (serverUrlInput != null) serverUrlInput.gameObject.SetActive(false);
            if (playerNameInput != null) playerNameInput.gameObject.SetActive(false);
            if (connectButton != null) connectButton.gameObject.SetActive(false);
            if (roomActionsPanel != null) roomActionsPanel.SetActive(false);

            if (waitingPanel != null) waitingPanel.SetActive(true);
            if (roomCodeDisplay != null) roomCodeDisplay.text = $"방 코드: {roomCode}";
            if (waitingStatusText != null) waitingStatusText.text = "상대를 기다리는 중...";
            if (readyButton != null) readyButton.interactable = false;
            ClearError();
        }
        #endregion

        #region Button Handlers
        private void OnConnectClicked()
        {
            string url = serverUrlInput != null ? serverUrlInput.text : DEFAULT_SERVER_URL;
            string playerName = playerNameInput != null ? playerNameInput.text : DEFAULT_PLAYER_NAME;

            if (string.IsNullOrEmpty(url))
            {
                ShowError("서버 URL을 입력하세요");
                return;
            }

            // Create MultiplayerManager if it doesn't exist
            EnsureMultiplayerManager();

            if (connectionStatusText != null)
                connectionStatusText.text = "연결 중...";

            if (connectButton != null)
                connectButton.interactable = false;

            MultiplayerManager.Instance.ConnectToServer(url, playerName);
        }

        private void OnCreateRoomClicked()
        {
            if (MultiplayerManager.Instance == null) return;
            MultiplayerManager.Instance.CreateRoom();
        }

        private void OnJoinRoomClicked()
        {
            if (MultiplayerManager.Instance == null) return;

            string code = roomCodeInput != null ? roomCodeInput.text : "";
            if (string.IsNullOrEmpty(code))
            {
                ShowError("방 코드를 입력하세요");
                return;
            }

            MultiplayerManager.Instance.JoinRoom(code);
        }

        private void OnAutoMatchClicked()
        {
            if (MultiplayerManager.Instance == null) return;
            if (waitingStatusText != null) waitingStatusText.text = "매칭 중...";
            MultiplayerManager.Instance.RequestAutoMatch();
        }

        private void OnReadyClicked()
        {
            if (MultiplayerManager.Instance == null) return;
            MultiplayerManager.Instance.SendReady();
            if (readyButton != null) readyButton.interactable = false;
            if (waitingStatusText != null) waitingStatusText.text = "준비 완료! 게임 시작 대기중...";
        }

        private void OnLeaveClicked()
        {
            if (MultiplayerManager.Instance != null)
            {
                MultiplayerManager.Instance.DisconnectFromServer();
            }
            ShowConnectionPanel();
            if (connectButton != null) connectButton.interactable = true;
        }
        #endregion

        #region Event Handlers
        private void HandleConnected()
        {
            if (connectButton != null) connectButton.interactable = true;
            ShowRoomActions();
        }

        private void HandleDisconnected(string reason)
        {
            if (connectButton != null) connectButton.interactable = true;
            ShowConnectionPanel();
            if (connectionStatusText != null)
                connectionStatusText.text = $"연결 끊김: {reason}";
        }

        private void HandleRoomCreated(string roomCode)
        {
            ShowWaitingRoom(roomCode);
        }

        private void HandlePlayerJoined(string playerName)
        {
            if (waitingStatusText != null)
                waitingStatusText.text = $"{playerName} 참가! 준비 버튼을 눌러주세요.";
            if (readyButton != null) readyButton.interactable = true;
        }

        private void HandleMatchStart()
        {
            Debug.Log("[MultiplayerLobbyUI] Match starting - loading GameScene");
            Hide();
            SceneManager.LoadScene("GameScene");
        }

        private void HandleError(string error)
        {
            ShowError(error);
            if (connectButton != null) connectButton.interactable = true;
        }
        #endregion

        #region Helpers
        private void EnsureMultiplayerManager()
        {
            if (MultiplayerManager.Instance == null)
            {
                GameObject go = new GameObject("MultiplayerManager");
                go.AddComponent<MultiplayerManager>();
            }

            // Ensure events are subscribed
            OnDisable();
            OnEnable();
        }

        private void ShowError(string message)
        {
            if (errorText != null)
            {
                errorText.text = message;
                errorText.color = new Color(1f, 0.3f, 0.3f);
            }
        }

        private void ClearError()
        {
            if (errorText != null)
                errorText.text = "";
        }
        #endregion

        #region Static Factory
        /// <summary>
        /// Create the lobby UI dynamically in the MainGame scene.
        /// Called by SceneNavigator.
        /// </summary>
        public static MultiplayerLobbyUI CreateInCanvas(Canvas parentCanvas)
        {
            if (parentCanvas == null)
            {
                Debug.LogError("[MultiplayerLobbyUI] No parent canvas provided");
                return null;
            }

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            // Full-screen overlay
            GameObject root = new GameObject("MultiplayerLobbyUI");
            root.transform.SetParent(parentCanvas.transform, false);

            RectTransform rootRect = root.AddComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            // Dark background
            Image bgImage = root.AddComponent<Image>();
            bgImage.color = new Color(0f, 0f, 0f, 0.85f);

            // Main panel
            GameObject panel = new GameObject("Panel");
            panel.transform.SetParent(root.transform, false);

            RectTransform panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = new Vector2(700, 900);

            Image panelBg = panel.AddComponent<Image>();
            panelBg.color = new Color(0.12f, 0.14f, 0.2f, 0.98f);

            Outline panelOutline = panel.AddComponent<Outline>();
            panelOutline.effectColor = new Color(0.4f, 0.5f, 0.8f, 0.6f);
            panelOutline.effectDistance = new Vector2(2, -2);

            // Title
            Text titleText = CreateUIText(panel.transform, "Title",
                "멀티플레이", 42, new Color(1f, 0.9f, 0.4f), font,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -30), new Vector2(600, 60));
            titleText.fontStyle = FontStyle.Bold;

            // Close button (top right)
            Button closeBtn = CreateUIButton(panel.transform, "CloseButton", "X",
                new Color(0.8f, 0.2f, 0.2f), font,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-15, -15), new Vector2(50, 50));

            // --- Connection Section ---
            // Server URL input
            InputField serverInput = CreateUIInputField(panel.transform, "ServerUrlInput",
                "ws://localhost:8080/ws", font,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0, -120), new Vector2(560, 50));

            // Player name input
            InputField nameInput = CreateUIInputField(panel.transform, "PlayerNameInput",
                "Player", font,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0, -185), new Vector2(560, 50));

            // Connect button
            Button connectBtn = CreateUIButton(panel.transform, "ConnectButton", "접속",
                new Color(0.2f, 0.5f, 0.8f), font,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f),
                new Vector2(0, -250), new Vector2(560, 60));

            // Connection status
            Text statusText = CreateUIText(panel.transform, "ConnectionStatus",
                "", 20, Color.white, font,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -310), new Vector2(560, 30));

            // --- Room Actions Panel (hidden initially) ---
            GameObject roomActions = new GameObject("RoomActionsPanel");
            roomActions.transform.SetParent(panel.transform, false);
            RectTransform raRect = roomActions.AddComponent<RectTransform>();
            raRect.anchorMin = new Vector2(0, 0);
            raRect.anchorMax = new Vector2(1, 1);
            raRect.offsetMin = new Vector2(40, 200);
            raRect.offsetMax = new Vector2(-40, -120);

            VerticalLayoutGroup raLayout = roomActions.AddComponent<VerticalLayoutGroup>();
            raLayout.spacing = 16;
            raLayout.padding = new RectOffset(10, 10, 10, 10);
            raLayout.childControlWidth = true;
            raLayout.childControlHeight = false;
            raLayout.childForceExpandWidth = true;
            raLayout.childForceExpandHeight = false;

            // Create Room button
            Button createBtn = CreateLayoutButton(roomActions.transform, "CreateRoomButton",
                "방 만들기", new Color(0.2f, 0.6f, 0.3f), font, 70);

            // Room code input + Join button row
            GameObject joinRow = new GameObject("JoinRow");
            joinRow.transform.SetParent(roomActions.transform, false);
            joinRow.AddComponent<RectTransform>();
            LayoutElement joinRowLE = joinRow.AddComponent<LayoutElement>();
            joinRowLE.preferredHeight = 60;

            HorizontalLayoutGroup joinHL = joinRow.AddComponent<HorizontalLayoutGroup>();
            joinHL.spacing = 8;
            joinHL.childControlWidth = true;
            joinHL.childControlHeight = true;
            joinHL.childForceExpandWidth = true;
            joinHL.childForceExpandHeight = true;

            InputField codeInput = CreateLayoutInputField(joinRow.transform, "RoomCodeInput",
                "방 코드 입력", font);

            Button joinBtn = CreateLayoutButton(joinRow.transform, "JoinRoomButton",
                "참가", new Color(0.3f, 0.5f, 0.7f), font, 0);

            // Auto-match button
            Button autoMatchBtn = CreateLayoutButton(roomActions.transform, "AutoMatchButton",
                "자동 매칭", new Color(0.6f, 0.4f, 0.8f), font, 70);

            roomActions.SetActive(false);

            // --- Waiting Panel (hidden initially) ---
            GameObject waitingPnl = new GameObject("WaitingPanel");
            waitingPnl.transform.SetParent(panel.transform, false);
            RectTransform wpRect = waitingPnl.AddComponent<RectTransform>();
            wpRect.anchorMin = new Vector2(0, 0);
            wpRect.anchorMax = new Vector2(1, 1);
            wpRect.offsetMin = new Vector2(40, 200);
            wpRect.offsetMax = new Vector2(-40, -120);

            VerticalLayoutGroup wpLayout = waitingPnl.AddComponent<VerticalLayoutGroup>();
            wpLayout.spacing = 20;
            wpLayout.padding = new RectOffset(10, 10, 30, 10);
            wpLayout.childControlWidth = true;
            wpLayout.childControlHeight = false;
            wpLayout.childForceExpandWidth = true;
            wpLayout.childForceExpandHeight = false;
            wpLayout.childAlignment = TextAnchor.UpperCenter;

            // Room code display
            Text codeDisplay = CreateLayoutText(waitingPnl.transform, "RoomCodeDisplay",
                "방 코드: ----", 36, new Color(1f, 0.9f, 0.4f), font, 50);
            codeDisplay.fontStyle = FontStyle.Bold;

            // Waiting status
            Text waitStatus = CreateLayoutText(waitingPnl.transform, "WaitingStatus",
                "상대를 기다리는 중...", 24, Color.white, font, 40);

            // Ready button
            Button readyBtn = CreateLayoutButton(waitingPnl.transform, "ReadyButton",
                "준비 완료", new Color(0.2f, 0.7f, 0.3f), font, 70);

            // Leave button
            Button leaveBtn = CreateLayoutButton(waitingPnl.transform, "LeaveButton",
                "나가기", new Color(0.7f, 0.2f, 0.2f), font, 60);

            waitingPnl.SetActive(false);

            // Error text (bottom area)
            Text errText = CreateUIText(panel.transform, "ErrorText",
                "", 18, new Color(1f, 0.3f, 0.3f), font,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 40), new Vector2(560, 40));

            // Add component and wire references
            MultiplayerLobbyUI lobbyUI = root.AddComponent<MultiplayerLobbyUI>();

            SetField(lobbyUI, "lobbyPanel", root);
            SetField(lobbyUI, "closeButton", closeBtn);
            SetField(lobbyUI, "serverUrlInput", serverInput);
            SetField(lobbyUI, "playerNameInput", nameInput);
            SetField(lobbyUI, "connectButton", connectBtn);
            SetField(lobbyUI, "connectionStatusText", statusText);
            SetField(lobbyUI, "roomActionsPanel", roomActions);
            SetField(lobbyUI, "createRoomButton", createBtn);
            SetField(lobbyUI, "joinRoomButton", joinBtn);
            SetField(lobbyUI, "autoMatchButton", autoMatchBtn);
            SetField(lobbyUI, "roomCodeInput", codeInput);
            SetField(lobbyUI, "waitingPanel", waitingPnl);
            SetField(lobbyUI, "roomCodeDisplay", codeDisplay);
            SetField(lobbyUI, "waitingStatusText", waitStatus);
            SetField(lobbyUI, "readyButton", readyBtn);
            SetField(lobbyUI, "leaveButton", leaveBtn);
            SetField(lobbyUI, "errorText", errText);

            return lobbyUI;
        }
        #endregion

        #region UI Creation Helpers
        private static Text CreateUIText(Transform parent, string name, string text, int fontSize,
            Color color, Font font, Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);

            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;

            Text t = obj.AddComponent<Text>();
            t.font = font;
            t.text = text;
            t.fontSize = fontSize;
            t.color = color;
            t.alignment = TextAnchor.MiddleCenter;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            return t;
        }

        private static Text CreateLayoutText(Transform parent, string name, string text, int fontSize,
            Color color, Font font, float height)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            obj.AddComponent<RectTransform>();

            LayoutElement le = obj.AddComponent<LayoutElement>();
            le.preferredHeight = height;

            Text t = obj.AddComponent<Text>();
            t.font = font;
            t.text = text;
            t.fontSize = fontSize;
            t.color = color;
            t.alignment = TextAnchor.MiddleCenter;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            return t;
        }

        private static Button CreateUIButton(Transform parent, string name, string text,
            Color bgColor, Font font, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
            Vector2 pos, Vector2 size)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);

            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;

            Image img = obj.AddComponent<Image>();
            img.color = bgColor;

            Button btn = obj.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f);
            colors.pressedColor = new Color(0.7f, 0.7f, 0.7f);
            colors.fadeDuration = 0.1f;
            btn.colors = colors;

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(obj.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            Text t = textObj.AddComponent<Text>();
            t.font = font;
            t.text = text;
            t.fontSize = 28;
            t.color = Color.white;
            t.alignment = TextAnchor.MiddleCenter;
            t.fontStyle = FontStyle.Bold;
            t.raycastTarget = false;

            return btn;
        }

        private static Button CreateLayoutButton(Transform parent, string name, string text,
            Color bgColor, Font font, float height)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            obj.AddComponent<RectTransform>();

            if (height > 0)
            {
                LayoutElement le = obj.AddComponent<LayoutElement>();
                le.preferredHeight = height;
            }

            Image img = obj.AddComponent<Image>();
            img.color = bgColor;

            Button btn = obj.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f);
            colors.pressedColor = new Color(0.7f, 0.7f, 0.7f);
            colors.fadeDuration = 0.1f;
            btn.colors = colors;

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(obj.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            Text t = textObj.AddComponent<Text>();
            t.font = font;
            t.text = text;
            t.fontSize = 26;
            t.color = Color.white;
            t.alignment = TextAnchor.MiddleCenter;
            t.fontStyle = FontStyle.Bold;
            t.raycastTarget = false;

            return btn;
        }

        private static InputField CreateUIInputField(Transform parent, string name, string placeholder,
            Font font, Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);

            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;

            Image bg = obj.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.22f, 0.3f, 1f);

            // Text child
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(obj.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10, 2);
            textRect.offsetMax = new Vector2(-10, -2);

            Text inputText = textObj.AddComponent<Text>();
            inputText.font = font;
            inputText.text = "";
            inputText.fontSize = 22;
            inputText.color = Color.white;
            inputText.alignment = TextAnchor.MiddleLeft;
            inputText.supportRichText = false;

            // Placeholder child
            GameObject phObj = new GameObject("Placeholder");
            phObj.transform.SetParent(obj.transform, false);
            RectTransform phRect = phObj.AddComponent<RectTransform>();
            phRect.anchorMin = Vector2.zero;
            phRect.anchorMax = Vector2.one;
            phRect.offsetMin = new Vector2(10, 2);
            phRect.offsetMax = new Vector2(-10, -2);

            Text phText = phObj.AddComponent<Text>();
            phText.font = font;
            phText.text = placeholder;
            phText.fontSize = 22;
            phText.color = new Color(0.5f, 0.5f, 0.6f);
            phText.alignment = TextAnchor.MiddleLeft;
            phText.fontStyle = FontStyle.Italic;

            InputField inputField = obj.AddComponent<InputField>();
            inputField.textComponent = inputText;
            inputField.placeholder = phText;
            inputField.text = placeholder;

            return inputField;
        }

        private static InputField CreateLayoutInputField(Transform parent, string name,
            string placeholder, Font font)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            obj.AddComponent<RectTransform>();

            Image bg = obj.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.22f, 0.3f, 1f);

            // Text child
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(obj.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10, 2);
            textRect.offsetMax = new Vector2(-10, -2);

            Text inputText = textObj.AddComponent<Text>();
            inputText.font = font;
            inputText.text = "";
            inputText.fontSize = 22;
            inputText.color = Color.white;
            inputText.alignment = TextAnchor.MiddleLeft;
            inputText.supportRichText = false;

            // Placeholder child
            GameObject phObj = new GameObject("Placeholder");
            phObj.transform.SetParent(obj.transform, false);
            RectTransform phRect = phObj.AddComponent<RectTransform>();
            phRect.anchorMin = Vector2.zero;
            phRect.anchorMax = Vector2.one;
            phRect.offsetMin = new Vector2(10, 2);
            phRect.offsetMax = new Vector2(-10, -2);

            Text phText = phObj.AddComponent<Text>();
            phText.font = font;
            phText.text = placeholder;
            phText.fontSize = 22;
            phText.color = new Color(0.5f, 0.5f, 0.6f);
            phText.alignment = TextAnchor.MiddleLeft;
            phText.fontStyle = FontStyle.Italic;

            InputField inputField = obj.AddComponent<InputField>();
            inputField.textComponent = inputText;
            inputField.placeholder = phText;

            return inputField;
        }

        private static void SetField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(target, value);
        }
        #endregion
    }
}
