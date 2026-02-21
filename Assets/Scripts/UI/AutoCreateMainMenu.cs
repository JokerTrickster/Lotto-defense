using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace LottoDefense.UI
{
    /// <summary>
    /// MainGame 씬 로드 시 자동으로 메인 메뉴 버튼 생성
    /// Unity Editor 메뉴 실행 필요 없음!
    /// </summary>
    [DefaultExecutionOrder(-100)] // 다른 스크립트보다 먼저 실행
    public class AutoCreateMainMenu : MonoBehaviour
    {
        private void Awake()
        {
            // MainGame 씬에서만 실행
            if (SceneManager.GetActiveScene().name != "MainGame")
            {
                Destroy(gameObject);
                return;
            }

            Debug.Log("[AutoCreateMainMenu] Starting UI cleanup and creation...");
            
            // 모든 기존 버튼 삭제 (StartGameButton, 이상한 버튼들 등)
            CleanupOldUI();
            
            // 새 버튼 생성
            CreateMainMenuButtons();
            
            // 생성 후 자신은 삭제
            Destroy(gameObject);
        }
        
        private void CleanupOldUI()
        {
            // 모든 버튼 찾아서 삭제 (SinglePlayButton, CoopPlayButton 제외)
            Button[] allButtons = FindObjectsByType<Button>(FindObjectsSortMode.None);
            int deletedCount = 0;
            
            foreach (Button btn in allButtons)
            {
                string name = btn.gameObject.name;
                // 이미 생성한 새 버튼은 건너뛰기
                if (name == "SinglePlayButton" || name == "CoopPlayButton" ||
                    name == "RankingButton" || name == "MyStatsButton")
                {
                    continue;
                }
                
                Debug.Log($"[AutoCreateMainMenu] Deleting old button: {name}");
                Destroy(btn.gameObject);
                deletedCount++;
            }
            
            Debug.Log($"[AutoCreateMainMenu] Deleted {deletedCount} old buttons");
        }

        private void CreateMainMenuButtons()
        {
            // Canvas 찾기 또는 생성
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
                Debug.Log("[AutoCreateMainMenu] Canvas created");
            }

            // SceneNavigator 생성 또는 찾기
            SceneNavigator navigator = FindFirstObjectByType<SceneNavigator>();
            if (navigator == null)
            {
                GameObject navObj = new GameObject("SceneNavigator");
                navigator = navObj.AddComponent<SceneNavigator>();
                DontDestroyOnLoad(navObj);
            }

            // 버튼 생성 (StartGameButton은 이미 Bootstrapper에서 삭제됨)
            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (defaultFont == null)
                defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

            // 1. 싱글 플레이 버튼 (화면 중앙, 왼쪽)
            Button singleButton = CreateButton("SinglePlayButton", "싱글 플레이", 
                new Vector2(0.5f, 0.5f), new Vector2(250, 100), 
                new Color(0.2f, 0.6f, 1f), defaultFont, canvas.transform);
            
            // 왼쪽으로 이동 (-140px)
            RectTransform singleRect = singleButton.GetComponent<RectTransform>();
            singleRect.anchoredPosition = new Vector2(-140, 0);
            
            singleButton.onClick.AddListener(() => 
            {
                Debug.Log("[AutoCreateMainMenu] Single play button clicked!");
                navigator.LoadGameScene();
            });

            // 2. 협동 플레이 버튼 (화면 중앙, 오른쪽)
            Button coopButton = CreateButton("CoopPlayButton", "협동 플레이", 
                new Vector2(0.5f, 0.5f), new Vector2(250, 100), 
                new Color(0.9f, 0.5f, 0.2f), defaultFont, canvas.transform);
            
            // 오른쪽으로 이동 (+140px)
            RectTransform coopRect = coopButton.GetComponent<RectTransform>();
            coopRect.anchoredPosition = new Vector2(140, 0);
            
            coopButton.onClick.AddListener(() => 
            {
                Debug.Log("[AutoCreateMainMenu] Coop play button clicked!");
                navigator.ShowMultiplayerLobby();
            });

            // 3. 랭킹 버튼 (왼쪽 상단)
            Button rankingButton = CreateButton("RankingButton", "랭킹", 
                new Vector2(0f, 1f), new Vector2(120, 50), 
                new Color(0.3f, 0.7f, 0.3f), defaultFont, canvas.transform);
            
            // 왼쪽 상단 모서리에 배치 (여백 10px)
            RectTransform rankingRect = rankingButton.GetComponent<RectTransform>();
            rankingRect.anchoredPosition = new Vector2(70, -35);
            
            // 텍스트 크기 줄이기
            Text rankingText = rankingButton.GetComponentInChildren<Text>();
            if (rankingText != null) rankingText.fontSize = 24;
            
            rankingButton.onClick.AddListener(() => 
            {
                Debug.Log("[AutoCreateMainMenu] Ranking button clicked!");
                navigator.ShowRankings();
            });

            // 4. 내 기록 버튼 (왼쪽 상단, 랭킹 버튼 아래)
            Button statsButton = CreateButton("MyStatsButton", "내 기록", 
                new Vector2(0f, 1f), new Vector2(120, 50), 
                new Color(0.7f, 0.3f, 0.7f), defaultFont, canvas.transform);
            
            // 랭킹 버튼 아래에 배치 (여백 5px)
            RectTransform statsRect = statsButton.GetComponent<RectTransform>();
            statsRect.anchoredPosition = new Vector2(70, -95);
            
            // 텍스트 크기 줄이기
            Text statsText = statsButton.GetComponentInChildren<Text>();
            if (statsText != null) statsText.fontSize = 24;
            
            statsButton.onClick.AddListener(() => 
            {
                Debug.Log("[AutoCreateMainMenu] Stats button clicked!");
            });

            Debug.Log("[AutoCreateMainMenu] ✅ 메인 메뉴 버튼 자동 생성 완료!");
            Debug.Log("레이아웃: 싱글/협동 중앙 나란히, 랭킹/내기록 좌상단");
        }

        private Button CreateButton(string name, string text, Vector2 anchorPos, Vector2 size, Color color, Font font, Transform parent)
        {
            // 버튼 GameObject
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);

            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.anchorMin = anchorPos;
            btnRect.anchorMax = anchorPos;
            btnRect.pivot = new Vector2(0.5f, 0.5f);
            btnRect.anchoredPosition = Vector2.zero;
            btnRect.sizeDelta = size;

            Image btnImage = btnObj.AddComponent<Image>();
            btnImage.color = color;

            Button btn = btnObj.AddComponent<Button>();

            // 버튼 텍스트
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            Text btnText = textObj.AddComponent<Text>();
            btnText.text = text;
            btnText.font = font;
            btnText.fontSize = 32;
            btnText.color = Color.white;
            btnText.alignment = TextAnchor.MiddleCenter;
            btnText.fontStyle = FontStyle.Bold;

            return btn;
        }
    }
}
