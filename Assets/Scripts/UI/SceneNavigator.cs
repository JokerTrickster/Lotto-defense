using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
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
            StartCoroutine(LoadMainGameCoroutine());
        }

        private IEnumerator LoadMainGameCoroutine()
        {
            // Clean up all gameplay singletons
            GameplayManager.CleanupAllGameplaySingletons();

            // Wait one frame for Destroy() calls to take effect
            yield return null;

            // Now load MainGame scene
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

        /// <summary>준비중: 협동 플레이 (for button binding).</summary>
        public void ShowComingSoonCoop() => ShowComingSoon("협동 플레이");

        /// <summary>준비중: 보스 러시 (for button binding).</summary>
        public void ShowComingSoonBossRush() => ShowComingSoon("보스 러시");
    }
}
