using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace LottoDefense.UI
{
    /// <summary>
    /// MainGame 씬 로드 시 자동으로 메인 메뉴 버튼 생성
    /// Unity Editor 메뉴 실행 필요 없음!
    /// </summary>
    public class AutoCreateMainMenu : MonoBehaviour
    {
        private void Awake()
        {
            // 이미 MainMenuUI가 있으면 생성 안 함
            if (FindFirstObjectByType<MainMenuUI>() != null)
            {
                Debug.Log("[AutoCreateMainMenu] MainMenuUI already exists, skipping creation");
                return;
            }

            // MainGame 씬에서만 실행
            if (SceneManager.GetActiveScene().name != "MainGame")
            {
                return;
            }

            Debug.Log("[AutoCreateMainMenu] Creating main menu buttons...");
            CreateMainMenuButtons();
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

            // MainMenuUI GameObject 생성
            GameObject mainMenuObj = new GameObject("MainMenuUI");
            mainMenuObj.transform.SetParent(canvas.transform, false);
            MainMenuUI mainMenuUI = mainMenuObj.AddComponent<MainMenuUI>();

            RectTransform menuRect = mainMenuObj.GetComponent<RectTransform>();
            menuRect.anchorMin = Vector2.zero;
            menuRect.anchorMax = Vector2.one;
            menuRect.sizeDelta = Vector2.zero;

            // SceneNavigator 생성 또는 찾기
            SceneNavigator navigator = FindFirstObjectByType<SceneNavigator>();
            if (navigator == null)
            {
                GameObject navObj = new GameObject("SceneNavigator");
                navigator = navObj.AddComponent<SceneNavigator>();
                DontDestroyOnLoad(navObj);
            }

            // 버튼 생성
            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (defaultFont == null)
                defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

            // 1. 싱글 플레이 버튼 (화면 중앙 위쪽)
            Button singleButton = CreateButton("SinglePlayButton", "싱글 플레이", 
                new Vector2(0.5f, 0.65f), new Vector2(300, 80), 
                new Color(0.2f, 0.6f, 1f), defaultFont, mainMenuObj.transform);
            
            singleButton.onClick.AddListener(() => 
            {
                Debug.Log("[AutoCreateMainMenu] Single play button clicked!");
                navigator.LoadGameScene();
            });

            // 2. 협동 플레이 버튼
            Button coopButton = CreateButton("CoopPlayButton", "협동 플레이", 
                new Vector2(0.5f, 0.5f), new Vector2(300, 80), 
                new Color(0.9f, 0.5f, 0.2f), defaultFont, mainMenuObj.transform);
            
            coopButton.onClick.AddListener(() => 
            {
                Debug.Log("[AutoCreateMainMenu] Coop play button clicked!");
                navigator.ShowMultiplayerLobby();
            });

            // 3. 랭킹 버튼
            Button rankingButton = CreateButton("RankingButton", "랭킹", 
                new Vector2(0.5f, 0.35f), new Vector2(300, 60), 
                new Color(0.3f, 0.7f, 0.3f), defaultFont, mainMenuObj.transform);
            
            rankingButton.onClick.AddListener(() => 
            {
                Debug.Log("[AutoCreateMainMenu] Ranking button clicked!");
                navigator.ShowRankings();
            });

            // 4. 내 기록 버튼
            Button statsButton = CreateButton("MyStatsButton", "내 기록", 
                new Vector2(0.5f, 0.25f), new Vector2(300, 60), 
                new Color(0.7f, 0.3f, 0.7f), defaultFont, mainMenuObj.transform);
            
            statsButton.onClick.AddListener(() => 
            {
                Debug.Log("[AutoCreateMainMenu] Stats button clicked!");
                // TODO: Show stats UI
            });

            // MainMenuUI에 버튼 참조 연결
            var singleField = typeof(MainMenuUI).GetField("singlePlayButton", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var coopField = typeof(MainMenuUI).GetField("coopPlayButton", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var rankingField = typeof(MainMenuUI).GetField("rankingButton", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var statsField = typeof(MainMenuUI).GetField("myStatsButton", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var navField = typeof(MainMenuUI).GetField("sceneNavigator", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            singleField?.SetValue(mainMenuUI, singleButton);
            coopField?.SetValue(mainMenuUI, coopButton);
            rankingField?.SetValue(mainMenuUI, rankingButton);
            statsField?.SetValue(mainMenuUI, statsButton);
            navField?.SetValue(mainMenuUI, navigator);

            Debug.Log("[AutoCreateMainMenu] ✅ 메인 메뉴 버튼 자동 생성 완료!");
            Debug.Log("버튼 4개: 싱글 플레이, 협동 플레이, 랭킹, 내 기록");
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
