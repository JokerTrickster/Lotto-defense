using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using LottoDefense.Gameplay;
using LottoDefense.Lobby;

namespace LottoDefense.UI
{
    public class SceneNavigator : MonoBehaviour
    {
        public void LoadScene(string sceneName)
        {
            Debug.Log($"Loading scene: {sceneName}");
            SceneManager.LoadScene(sceneName);
        }

        public void LoadGameScene()
        {
            Debug.Log("Loading GameScene...");
            SceneManager.LoadScene("GameScene");
        }

        public void LoadMainGame()
        {
            Debug.Log("Loading MainGame...");
            // Cleanup then load synchronously. Destroy() is deferred to end-of-frame,
            // so LoadScene executes before GameCanvas (our parent) is actually destroyed.
            // Using a coroutine here would fail because CleanupAllGameplaySingletons()
            // destroys GameCanvas, which kills this MonoBehaviour and its coroutines.
            GameplayManager.CleanupAllGameplaySingletons();
            SceneManager.LoadScene("MainGame");
        }

        public void LoadLoginScene()
        {
            Debug.Log("Loading LoginScene...");
            SceneManager.LoadScene("LoginScene");
        }

        public void QuitGame()
        {
            Debug.Log("Quitting game...");
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        /// <summary>
        /// Show "준비중" feedback for modes not yet implemented.
        /// </summary>
        public void ShowComingSoon(string modeName)
        {
            Debug.Log($"[MainMenu] 준비중: {modeName}");
        }

        /// <summary>Open multiplayer lobby (legacy name kept for existing button binding).</summary>
        public void ShowComingSoonCoop() => ShowMultiplayerLobby();

        /// <summary>Open multiplayer lobby overlay (for button binding).</summary>
        public void ShowMultiplayerLobby()
        {
            Debug.Log("[SceneNavigator] Opening multiplayer lobby");

            // Find existing lobby UI (may be inactive)
            MultiplayerLobbyUI lobbyUI = FindFirstObjectByType<MultiplayerLobbyUI>(FindObjectsInactive.Include);

            if (lobbyUI == null)
            {
                // Create lobby UI dynamically in current scene's canvas
                Canvas canvas = FindFirstObjectByType<Canvas>();
                if (canvas == null)
                {
                    Debug.LogError("[SceneNavigator] No Canvas found to host MultiplayerLobbyUI");
                    return;
                }
                lobbyUI = MultiplayerLobbyUI.CreateInCanvas(canvas);
            }

            lobbyUI.Show();
        }

        /// <summary>준비중: 보스 러시 (for button binding).</summary>
        public void ShowComingSoonBossRush() => ShowComingSoon("보스 러시");

        /// <summary>
        /// Try to start a game by deducting 1 entry ticket.
        /// Shows insufficient tickets popup if not enough.
        /// </summary>
        public void TryStartGame()
        {
            // Free entry for testing - skip ticket check
            Debug.Log("[SceneNavigator] Starting game (free entry mode)");
            SceneManager.LoadScene("GameScene");
        }

        private void ShowInsufficientTicketsPopup()
        {
            Debug.Log("[SceneNavigator] Insufficient tickets");

            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            // Simple popup overlay
            GameObject popupOverlay = new GameObject("InsufficientTicketsPopup");
            popupOverlay.transform.SetParent(canvas.transform, false);

            Image overlayBg = popupOverlay.AddComponent<Image>();
            overlayBg.color = new Color(0f, 0f, 0f, 0.7f);
            overlayBg.raycastTarget = true;

            RectTransform overlayRect = popupOverlay.GetComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.sizeDelta = Vector2.zero;

            // Panel
            GameObject panel = new GameObject("Panel");
            panel.transform.SetParent(popupOverlay.transform, false);
            Image panelBg = panel.AddComponent<Image>();
            panelBg.color = new Color(0.12f, 0.12f, 0.22f, 1f);

            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.15f, 0.35f);
            panelRect.anchorMax = new Vector2(0.85f, 0.65f);
            panelRect.sizeDelta = Vector2.zero;

            // Message
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            GameObject msgObj = new GameObject("Message");
            msgObj.transform.SetParent(panel.transform, false);
            Text msgText = msgObj.AddComponent<Text>();
            msgText.font = font;
            msgText.text = "입장권이 부족합니다\n\n일일 보상, 퀘스트, 우편함에서\n입장권을 획득할 수 있습니다.";
            msgText.fontSize = 28;
            msgText.color = Color.white;
            msgText.alignment = TextAnchor.MiddleCenter;
            msgText.raycastTarget = false;

            RectTransform msgRect = msgObj.GetComponent<RectTransform>();
            msgRect.anchorMin = new Vector2(0.05f, 0.3f);
            msgRect.anchorMax = new Vector2(0.95f, 0.9f);
            msgRect.sizeDelta = Vector2.zero;

            // OK button
            GameObject okObj = new GameObject("OKButton");
            okObj.transform.SetParent(panel.transform, false);
            Image okBg = okObj.AddComponent<Image>();
            okBg.color = new Color(0.26f, 0.52f, 0.96f, 1f);

            Button okBtn = okObj.AddComponent<Button>();
            okBtn.onClick.AddListener(() => Destroy(popupOverlay));

            RectTransform okRect = okObj.GetComponent<RectTransform>();
            okRect.anchorMin = new Vector2(0.25f, 0.05f);
            okRect.anchorMax = new Vector2(0.75f, 0.25f);
            okRect.sizeDelta = Vector2.zero;

            GameObject okTextObj = new GameObject("Text");
            okTextObj.transform.SetParent(okObj.transform, false);
            Text okText = okTextObj.AddComponent<Text>();
            okText.font = font;
            okText.text = "확인";
            okText.fontSize = 30;
            okText.color = Color.white;
            okText.alignment = TextAnchor.MiddleCenter;
            okText.raycastTarget = false;

            RectTransform okTextRect = okTextObj.GetComponent<RectTransform>();
            okTextRect.anchorMin = Vector2.zero;
            okTextRect.anchorMax = Vector2.one;
            okTextRect.sizeDelta = Vector2.zero;
        }
    }
}
