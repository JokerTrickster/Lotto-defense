using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

namespace LottoDefense.Editor
{
    /// <summary>
    /// MainGame 씬의 UI를 완전히 재구성
    /// 기존 버튼들 삭제하고 새로 생성
    /// </summary>
    public class FixMainGameScene : EditorWindow
    {
        [MenuItem("Lotto Defense/Fix MainGame Scene UI")]
        public static void FixUI()
        {
            // MainGame 씬 열기
            Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/MainGame.unity");
            
            Debug.Log("[FixMainGameScene] Cleaning up old UI...");
            
            // 기존 버튼들 모두 삭제
            DeleteOldButtons();
            
            Debug.Log("[FixMainGameScene] Creating new UI...");
            
            // 새 버튼 생성
            CreateNewUI();
            
            // 씬 저장
            EditorSceneManager.SaveScene(scene);
            
            Debug.Log("[FixMainGameScene] ✅ MainGame scene UI fixed!");
            
            EditorUtility.DisplayDialog("Success", 
                "MainGame 씬 UI 수정 완료!\n\n" +
                "- 싱글/협동 플레이: 중앙 나란히\n" +
                "- 랭킹/내 기록: 좌상단\n" +
                "- 게임 시작 버튼: 삭제됨", 
                "OK");
        }
        
        private static void DeleteOldButtons()
        {
            // ⚠️ 기존 UI는 유지! 중복된 메인 메뉴 버튼만 삭제
            string[] menuButtons = new string[]
            {
                "SinglePlayButton",
                "CoopPlayButton",
                "RankingButton",
                "MyStatsButton"
            };
            
            int deletedCount = 0;
            foreach (string btnName in menuButtons)
            {
                GameObject btnObj = GameObject.Find(btnName);
                if (btnObj != null)
                {
                    Debug.Log($"[FixMainGameScene] Deleting duplicate: {btnName}");
                    Object.DestroyImmediate(btnObj);
                    deletedCount++;
                }
            }
            
            Debug.Log($"[FixMainGameScene] Deleted {deletedCount} duplicate menu buttons (기존 UI 유지)");
        }
        
        private static void CreateNewUI()
        {
            // Canvas 찾기 또는 생성
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
                Debug.Log("[FixMainGameScene] Canvas created");
            }
            
            // EventSystem 확인
            if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystemObj = new GameObject("EventSystem");
                eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
            
            // 버튼 생성
            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (defaultFont == null)
                defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            
            // 1. 싱글 플레이 버튼 (중앙 왼쪽)
            Button singleButton = CreateButton("SinglePlayButton", "싱글 플레이", 
                new Vector2(0.5f, 0.5f), new Vector2(250, 100), 
                new Color(0.2f, 0.6f, 1f), defaultFont, canvas.transform);
            singleButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(-140, 0);
            
            // 2. 협동 플레이 버튼 (중앙 오른쪽)
            Button coopButton = CreateButton("CoopPlayButton", "협동 플레이", 
                new Vector2(0.5f, 0.5f), new Vector2(250, 100), 
                new Color(0.9f, 0.5f, 0.2f), defaultFont, canvas.transform);
            coopButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(140, 0);
            
            // 3. 랭킹 버튼 (좌상단)
            Button rankingButton = CreateButton("RankingButton", "랭킹", 
                new Vector2(0f, 1f), new Vector2(120, 50), 
                new Color(0.3f, 0.7f, 0.3f), defaultFont, canvas.transform);
            rankingButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(70, -35);
            rankingButton.GetComponentInChildren<Text>().fontSize = 24;
            
            // 4. 내 기록 버튼 (좌상단, 랭킹 아래)
            Button statsButton = CreateButton("MyStatsButton", "내 기록", 
                new Vector2(0f, 1f), new Vector2(120, 50), 
                new Color(0.7f, 0.3f, 0.7f), defaultFont, canvas.transform);
            statsButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(70, -95);
            statsButton.GetComponentInChildren<Text>().fontSize = 24;
            
            Debug.Log("[FixMainGameScene] 4 buttons created successfully");
        }
        
        private static Button CreateButton(string name, string text, Vector2 anchorPos, 
            Vector2 size, Color color, Font font, Transform parent)
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
