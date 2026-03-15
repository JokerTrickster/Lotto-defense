using UnityEngine;
using UnityEngine.SceneManagement;
using LottoDefense.UI;

namespace LottoDefense.Login
{
    /// <summary>
    /// 로그인 씬 자동 생성 및 초기화
    /// RuntimeInitializeOnLoadMethod로 LoginScene 로드 시 자동 실행
    /// </summary>
    public class LoginSceneBootstrapper : MonoBehaviour
    {
        #region Auto-Initialization
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void OnSceneLoaded()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.name == "LoginScene" || activeScene.buildIndex == 0)
            {
                EnsureLoginUI();
            }
        }
        #endregion

        #region UI Setup
        private static void EnsureLoginUI()
        {
            // LoginUI가 없으면 생성
            if (FindFirstObjectByType<LoginUI>() == null)
            {
                GameObject loginObj = new GameObject("LoginUI");
                loginObj.AddComponent<LoginUI>();
                Debug.Log("[LoginSceneBootstrapper] LoginUI created");
            }
        }
        #endregion
    }
}
