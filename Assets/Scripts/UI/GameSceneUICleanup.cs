using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace LottoDefense.UI
{
    /// <summary>
    /// GameScene 로드 시 메인 메뉴 버튼들을 자동으로 삭제
    /// 게임 플레이 중에는 메인 메뉴 버튼이 보이면 안 됨!
    /// </summary>
    public static class GameSceneUICleanup
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

            Debug.Log("[GameSceneUICleanup] Cleaning up main menu buttons...");

            // 메인 메뉴 버튼들 삭제
            DeleteMainMenuButtons();
        }

        private static void DeleteMainMenuButtons()
        {
            int deletedCount = 0;

            // 버튼 이름으로 찾아서 삭제
            string[] buttonNames = new string[]
            {
                "SinglePlayButton",
                "CoopPlayButton",
                "RankingButton",
                "MyStatsButton",
                "StartGameButton",
                "StartButton"
            };

            foreach (string btnName in buttonNames)
            {
                GameObject btnObj = GameObject.Find(btnName);
                if (btnObj != null)
                {
                    Debug.Log($"[GameSceneUICleanup] Deleting: {btnName}");
                    Object.Destroy(btnObj);
                    deletedCount++;
                }
            }

            // 혹시 남아있는 버튼들도 찾아서 삭제
            Button[] allButtons = Object.FindObjectsByType<Button>(FindObjectsSortMode.None);
            foreach (Button btn in allButtons)
            {
                string name = btn.gameObject.name;
                
                // 게임 UI 버튼은 건너뛰기
                if (name.Contains("Summon") || name.Contains("Sell") || 
                    name.Contains("Upgrade") || (name.Contains("Start") && name.Contains("Wave")))
                {
                    continue;
                }

                // 메인 메뉴 버튼으로 보이는 것들 삭제
                Text btnText = btn.GetComponentInChildren<Text>();
                if (btnText != null)
                {
                    string text = btnText.text;
                    if (text.Contains("싱글") || text.Contains("협동") || 
                        text.Contains("랭킹") || text.Contains("기록") ||
                        text.Contains("게임") && text.Contains("시작"))
                    {
                        Debug.Log($"[GameSceneUICleanup] Deleting button with text: {text}");
                        Object.Destroy(btn.gameObject);
                        deletedCount++;
                    }
                }
            }

            Debug.Log($"[GameSceneUICleanup] ✅ Deleted {deletedCount} main menu buttons");
        }
    }
}
