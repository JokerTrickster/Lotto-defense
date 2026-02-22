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
            // 제거할 버튼들
            string[] buttonsToRemove = new string[]
            {
                // 중복 방지
                "SinglePlayButton",
                "CoopPlayButton",
                "RankingButton",
                "MyStatsButton",
                // "게임 시작" 버튼 제거
                "StartGameButton",
                "게임 시작"
            };
            
            int deletedCount = 0;
            foreach (string btnName in buttonsToRemove)
            {
                GameObject btnObj = GameObject.Find(btnName);
                if (btnObj != null)
                {
                    Debug.Log($"[AutoCreateMainMenu] Removing: {btnName}");
                    Destroy(btnObj);
                    deletedCount++;
                }
            }
            
            if (deletedCount > 0)
            {
                Debug.Log($"[AutoCreateMainMenu] Removed {deletedCount} buttons");
            }
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
                
                // CanvasScaler 설정 (모바일 대응)
                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920); // 모바일 세로 해상도
                scaler.matchWidthOrHeight = 0.5f; // Width와 Height 균형
                
                canvasObj.AddComponent<GraphicRaycaster>();
                Debug.Log("[AutoCreateMainMenu] Canvas created with mobile-friendly scaler");
            }
            else
            {
                // 기존 Canvas의 Scaler 설정 업데이트
                CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
                if (scaler != null)
                {
                    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    scaler.referenceResolution = new Vector2(1080, 1920);
                    scaler.matchWidthOrHeight = 0.5f;
                    Debug.Log("[AutoCreateMainMenu] Updated existing CanvasScaler for mobile");
                }
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

            // 1. 싱글 플레이 버튼 (중앙 왼쪽, 크게)
            Button singleButton = CreateButton("SinglePlayButton", "싱글 플레이", 
                new Vector2(0.5f, 0.5f), new Vector2(400, 150),
                new Color(0.2f, 0.6f, 1f), defaultFont, canvas.transform);
            
            RectTransform singleRect = singleButton.GetComponent<RectTransform>();
            singleRect.anchoredPosition = new Vector2(-220, -50); // 약간 아래로
            
            singleButton.onClick.AddListener(() => 
            {
                Debug.Log("[AutoCreateMainMenu] Single play button clicked!");
                navigator.LoadGameScene();
            });

            // 2. 협동 플레이 버튼 (중앙 오른쪽, 크게)
            Button coopButton = CreateButton("CoopPlayButton", "협동 플레이", 
                new Vector2(0.5f, 0.5f), new Vector2(400, 150),
                new Color(0.9f, 0.5f, 0.2f), defaultFont, canvas.transform);
            
            RectTransform coopRect = coopButton.GetComponent<RectTransform>();
            coopRect.anchoredPosition = new Vector2(220, -50); // 약간 아래로
            
            coopButton.onClick.AddListener(() => 
            {
                Debug.Log("[AutoCreateMainMenu] Coop play button clicked!");
                navigator.ShowMultiplayerLobby();
            });

            // 3. 랭킹 버튼 (헤더 우측)
            Button rankingButton = CreateButton("RankingButton", "랭킹", 
                new Vector2(1f, 1f), new Vector2(120, 60),
                new Color(0.3f, 0.7f, 0.3f), defaultFont, canvas.transform);
            
            RectTransform rankingRect = rankingButton.GetComponent<RectTransform>();
            rankingRect.anchoredPosition = new Vector2(-20, -40); // 헤더 우측 상단
            
            Text rankingText = rankingButton.GetComponentInChildren<Text>();
            if (rankingText != null) rankingText.fontSize = 32;
            
            rankingButton.onClick.AddListener(() => 
            {
                Debug.Log("[AutoCreateMainMenu] Ranking button clicked!");
                navigator.ShowRankings();
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
            btnText.fontSize = 48; // 모바일에서 보기 쉽게 크게
            btnText.color = Color.white;
            btnText.alignment = TextAnchor.MiddleCenter;
            btnText.fontStyle = FontStyle.Bold;
            btnText.resizeTextForBestFit = true; // 자동 크기 조정
            btnText.resizeTextMinSize = 24;
            btnText.resizeTextMaxSize = 48;

            return btn;
        }
    }
}
