using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using LottoDefense.Gameplay;

namespace LottoDefense.UI
{
    /// <summary>
    /// 난이도 선택 팝업 UI
    /// </summary>
    public class DifficultySelectionUI : MonoBehaviour
    {
        private Canvas canvas;
        private GameObject popupPanel;
        private Action<GameDifficulty> onDifficultySelected;
        private bool isCoopMode;

        /// <summary>
        /// 난이도 선택 팝업 표시
        /// </summary>
        public static void Show(bool isCoopMode, Action<GameDifficulty> callback)
        {
            // Canvas 찾기
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[DifficultySelectionUI] No Canvas found!");
                return;
            }

            // UI 생성
            GameObject uiObj = new GameObject("DifficultySelectionUI");
            uiObj.transform.SetParent(canvas.transform, false);
            DifficultySelectionUI ui = uiObj.AddComponent<DifficultySelectionUI>();
            ui.Initialize(canvas, isCoopMode, callback);
        }

        private void Initialize(Canvas targetCanvas, bool coop, Action<GameDifficulty> callback)
        {
            canvas = targetCanvas;
            isCoopMode = coop;
            onDifficultySelected = callback;
            CreatePopup();
        }

        private void CreatePopup()
        {
            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (defaultFont == null)
                defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

            // 배경 (어두운 오버레이)
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(canvas.transform, false);
            Image bgImage = bgObj.AddComponent<Image>();
            bgImage.color = new Color(0f, 0f, 0f, 0.7f);
            bgImage.raycastTarget = true;

            RectTransform bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;

            // 팝업 패널
            popupPanel = new GameObject("DifficultyPopup");
            popupPanel.transform.SetParent(bgObj.transform, false);

            RectTransform panelRect = popupPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = new Vector2(800, 600);

            Image panelImage = popupPanel.AddComponent<Image>();
            panelImage.color = new Color(0.2f, 0.2f, 0.25f);

            // 제목
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(popupPanel.transform, false);
            Text titleText = titleObj.AddComponent<Text>();
            titleText.text = "난이도 선택";
            titleText.font = defaultFont;
            titleText.fontSize = 56;
            titleText.color = Color.white;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.fontStyle = FontStyle.Bold;

            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.1f, 0.7f);
            titleRect.anchorMax = new Vector2(0.9f, 0.9f);
            titleRect.sizeDelta = Vector2.zero;

            // 난이도 버튼 3개
            CreateDifficultyButton(GameDifficulty.Normal, new Vector2(0, 80), new Color(0.3f, 0.7f, 0.3f), defaultFont);
            CreateDifficultyButton(GameDifficulty.Hard, new Vector2(0, -40), new Color(1f, 0.6f, 0.2f), defaultFont);
            CreateDifficultyButton(GameDifficulty.VeryHard, new Vector2(0, -160), new Color(0.9f, 0.2f, 0.2f), defaultFont);

            // 취소 버튼
            CreateCancelButton(defaultFont);
        }

        private void CreateDifficultyButton(GameDifficulty difficulty, Vector2 position, Color color, Font font)
        {
            GameObject btnObj = new GameObject($"{difficulty}Button");
            btnObj.transform.SetParent(popupPanel.transform, false);

            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.5f, 0.5f);
            btnRect.anchorMax = new Vector2(0.5f, 0.5f);
            btnRect.pivot = new Vector2(0.5f, 0.5f);
            btnRect.anchoredPosition = position;
            btnRect.sizeDelta = new Vector2(600, 100);

            Image btnImage = btnObj.AddComponent<Image>();
            btnImage.color = color;

            Button btn = btnObj.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.2f, 1.2f, 1.2f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f);
            btn.colors = colors;
            btn.onClick.AddListener(() => OnDifficultyClicked(difficulty));

            // 버튼 텍스트
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            Text text = textObj.AddComponent<Text>();
            text.text = GetDifficultyDescription(difficulty);
            text.font = font;
            text.fontSize = 40;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            text.fontStyle = FontStyle.Bold;

            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
        }

        private void CreateCancelButton(Font font)
        {
            GameObject btnObj = new GameObject("CancelButton");
            btnObj.transform.SetParent(popupPanel.transform, false);

            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.5f, 0.05f);
            btnRect.anchorMax = new Vector2(0.5f, 0.05f);
            btnRect.pivot = new Vector2(0.5f, 0f);
            btnRect.anchoredPosition = Vector2.zero;
            btnRect.sizeDelta = new Vector2(300, 80);

            Image btnImage = btnObj.AddComponent<Image>();
            btnImage.color = new Color(0.5f, 0.5f, 0.5f);

            Button btn = btnObj.AddComponent<Button>();
            btn.onClick.AddListener(Close);

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            Text text = textObj.AddComponent<Text>();
            text.text = "취소";
            text.font = font;
            text.fontSize = 36;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;

            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
        }

        private string GetDifficultyDescription(GameDifficulty difficulty)
        {
            switch (difficulty)
            {
                case GameDifficulty.Normal:
                    return "보통\n기본 난이도";
                case GameDifficulty.Hard:
                    return "어려움\n체력 +50%, 방어력 +30%";
                case GameDifficulty.VeryHard:
                    return "매우 어려움\n체력 +100%, 방어력 +50%";
                default:
                    return "보통";
            }
        }

        private void OnDifficultyClicked(GameDifficulty difficulty)
        {
            Debug.Log($"[DifficultySelectionUI] Selected: {difficulty}");
            onDifficultySelected?.Invoke(difficulty);
            Close();
        }

        private void Close()
        {
            Destroy(transform.parent.gameObject); // 배경 포함 삭제
            Destroy(gameObject);
        }
    }
}
