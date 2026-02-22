using UnityEngine;
using UnityEngine.SceneManagement;

namespace LottoDefense.UI
{
    /// <summary>
    /// 게임 시작 시 자동으로 MainGame 씬에 메인 메뉴 생성
    /// Unity Editor 메뉴 실행 필요 없음!
    /// </summary>
    public static class MainMenuBootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // MainGame 씬일 때만 실행
            if (scene.name != "MainGame")
            {
                return;
            }

            // ⚠️ 기존 UI는 건드리지 않음! StartButton 유지!

            // 이미 AutoCreateMainMenu가 있으면 실행 안 함
            if (Object.FindFirstObjectByType<AutoCreateMainMenu>() != null)
            {
                return;
            }

            // 이미 싱글/협동 버튼이 있으면 실행 안 함
            if (GameObject.Find("SinglePlayButton") != null)
            {
                Debug.Log("[MainMenuBootstrapper] Main menu buttons already exist");
                return;
            }

            Debug.Log("[MainMenuBootstrapper] Creating main menu buttons...");

            // AutoCreateMainMenu GameObject 생성
            GameObject bootstrapObj = new GameObject("AutoCreateMainMenu");
            bootstrapObj.AddComponent<AutoCreateMainMenu>();
        }
    }
}
