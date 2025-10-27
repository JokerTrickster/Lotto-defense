using UnityEngine;
using UnityEngine.SceneManagement;

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
    }
}
