using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using LottoDefense.Gameplay;

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
    }
}
