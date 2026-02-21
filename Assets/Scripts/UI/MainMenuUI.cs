using UnityEngine;
using UnityEngine.UI;

namespace LottoDefense.UI
{
    /// <summary>
    /// MainGame 씬의 메인 메뉴 UI (싱글 플레이, 협동 플레이 버튼).
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Button singlePlayButton;
        [SerializeField] private Button coopPlayButton;
        [SerializeField] private Button rankingButton;
        [SerializeField] private Button myStatsButton;
        [SerializeField] private Button settingsButton;

        [Header("References")]
        [SerializeField] private SceneNavigator sceneNavigator;

        private void Start()
        {
            // SceneNavigator 찾기
            if (sceneNavigator == null)
            {
                sceneNavigator = FindFirstObjectByType<SceneNavigator>();
                if (sceneNavigator == null)
                {
                    GameObject navObj = new GameObject("SceneNavigator");
                    sceneNavigator = navObj.AddComponent<SceneNavigator>();
                }
            }

            // 버튼 이벤트 연결
            SetupButtons();

            // UI가 없으면 자동 생성
            if (singlePlayButton == null)
            {
                CreateUI();
            }
        }

        private void SetupButtons()
        {
            if (singlePlayButton != null)
                singlePlayButton.onClick.AddListener(OnSinglePlayClicked);

            if (coopPlayButton != null)
                coopPlayButton.onClick.AddListener(OnCoopPlayClicked);

            if (rankingButton != null)
                rankingButton.onClick.AddListener(OnRankingClicked);

            if (myStatsButton != null)
                myStatsButton.onClick.AddListener(OnMyStatsClicked);

            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettingsClicked);
        }

        private void OnSinglePlayClicked()
        {
            Debug.Log("[MainMenuUI] 싱글 플레이 시작!");
            if (sceneNavigator != null)
            {
                sceneNavigator.LoadGameScene();
            }
        }

        private void OnCoopPlayClicked()
        {
            Debug.Log("[MainMenuUI] 협동 플레이 로비 열기!");
            if (sceneNavigator != null)
            {
                sceneNavigator.ShowMultiplayerLobby();
            }
        }

        private void OnRankingClicked()
        {
            Debug.Log("[MainMenuUI] 랭킹 열기!");
            if (sceneNavigator != null)
            {
                sceneNavigator.ShowRankings();
            }
        }

        private void OnMyStatsClicked()
        {
            Debug.Log("[MainMenuUI] 내 기록 열기!");
            // StatsUI 표시
            var statsUI = FindFirstObjectByType<LottoDefense.Backend.UI.StatsUI>(FindObjectsInactive.Include);
            if (statsUI != null)
            {
                statsUI.Show();
            }
            else
            {
                Debug.LogWarning("[MainMenuUI] StatsUI not found");
            }
        }

        private void OnSettingsClicked()
        {
            Debug.Log("[MainMenuUI] 설정 열기!");
        }

        /// <summary>
        /// UI가 없으면 자동으로 생성 (에디터에서 수동으로 만드는 것 권장).
        /// </summary>
        private void CreateUI()
        {
            Debug.LogWarning("[MainMenuUI] UI elements not assigned. Please assign in Inspector or create manually.");
            
            // 자동 생성은 복잡하므로 에디터에서 수동으로 만드는 것을 권장
            // 임시로 빈 GameObject만 생성
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
            }

            // 간단한 텍스트 안내
            GameObject textObj = new GameObject("PlaceholderText");
            textObj.transform.SetParent(canvas.transform, false);
            Text text = textObj.AddComponent<Text>();
            text.text = "MainMenuUI: Please assign UI elements in Inspector\n\n버튼을 Inspector에서 연결하거나\nEditor → Create Main Menu Buttons 실행";
            text.fontSize = 24;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            RectTransform rt = textObj.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
        }
    }
}
