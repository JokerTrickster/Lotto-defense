using UnityEngine;
using UnityEngine.SceneManagement;
using LottoDefense.UI;

namespace LottoDefense.Gameplay
{
    /// <summary>
    /// GameScene 로드 시 자동으로 배속 컨트롤러와 UI 생성
    /// </summary>
    public static class GameSpeedBootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // GameScene일 때만 실행
            if (scene.name != "GameScene")
            {
                return;
            }

            Debug.Log("[GameSpeedBootstrapper] Setting up game speed controls...");

            // GameSpeedController 생성
            if (GameSpeedController.Instance == null)
            {
                GameObject controllerObj = new GameObject("GameSpeedController");
                controllerObj.AddComponent<GameSpeedController>();
                Debug.Log("[GameSpeedBootstrapper] GameSpeedController created");
            }

            // GameSpeedButton UI 생성
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                // 이미 배속 버튼이 있는지 확인
                GameSpeedButton existingButton = Object.FindFirstObjectByType<GameSpeedButton>();
                if (existingButton == null)
                {
                    GameObject btnObj = new GameObject("GameSpeedButton");
                    btnObj.transform.SetParent(canvas.transform, false);
                    btnObj.AddComponent<GameSpeedButton>();
                    Debug.Log("[GameSpeedBootstrapper] GameSpeedButton UI created");
                }
            }
            else
            {
                Debug.LogWarning("[GameSpeedBootstrapper] No Canvas found, cannot create speed button UI");
            }
        }
    }
}
