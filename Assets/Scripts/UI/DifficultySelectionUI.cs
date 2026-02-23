using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using LottoDefense.Gameplay;

namespace LottoDefense.UI
{
    public class DifficultySelectionUI : MonoBehaviour
    {
        private Canvas canvas;
        private GameObject popupPanel;
        private GameObject backgroundOverlay;
        private Action<GameDifficulty> onDifficultySelected;
        private bool isCoopMode;

        public static void Show(bool isCoopMode, Action<GameDifficulty> callback)
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[DifficultySelectionUI] No Canvas found!");
                return;
            }

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
            Font defaultFont = GameFont.Get();

            // Full-screen overlay
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(canvas.transform, false);
            backgroundOverlay = bgObj;
            Image bgImage = bgObj.AddComponent<Image>();
            bgImage.color = CuteUIHelper.WarmOverlay;
            bgImage.raycastTarget = true;

            RectTransform bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;

            // Popup panel
            popupPanel = new GameObject("DifficultyPopup");
            popupPanel.transform.SetParent(bgObj.transform, false);

            RectTransform panelRect = popupPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = new Vector2(700, 520);

            Image panelImage = popupPanel.AddComponent<Image>();
            panelImage.color = CuteUIHelper.PeachBg;
            Sprite rounded = CuteUIHelper.GetRoundedRectSprite(24);
            if (rounded != null)
            {
                panelImage.sprite = rounded;
                panelImage.type = Image.Type.Sliced;
            }

            Shadow panelShadow = popupPanel.AddComponent<Shadow>();
            panelShadow.effectColor = new Color(0.4f, 0.3f, 0.2f, 0.3f);
            panelShadow.effectDistance = new Vector2(4, -4);

            // Vertical layout: title, difficulty options, cancel in one border
            VerticalLayoutGroup vlg = popupPanel.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(40, 40, 24, 24);
            vlg.spacing = 12;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childAlignment = TextAnchor.UpperCenter;

            // Title
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(popupPanel.transform, false);
            LayoutElement titleLE = titleObj.AddComponent<LayoutElement>();
            titleLE.preferredHeight = 56;

            Text titleText = titleObj.AddComponent<Text>();
            titleText.text = "난이도 선택";
            titleText.font = defaultFont;
            titleText.fontSize = 48;
            titleText.color = CuteUIHelper.DarkText;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.fontStyle = FontStyle.Bold;

            // Difficulty buttons (all same size via layout)
            CreateDifficultyButton(GameDifficulty.Normal, new Color(0.45f, 0.82f, 0.55f), defaultFont);
            CreateDifficultyButton(GameDifficulty.Hard, new Color(0.95f, 0.7f, 0.4f), defaultFont);
            CreateDifficultyButton(GameDifficulty.VeryHard, new Color(0.95f, 0.5f, 0.5f), defaultFont);

            // Small gap above cancel so it sits clearly below difficulty options
            GameObject spacer = new GameObject("Spacer");
            spacer.transform.SetParent(popupPanel.transform, false);
            LayoutElement spacerLE = spacer.AddComponent<LayoutElement>();
            spacerLE.preferredHeight = 8;
            spacerLE.flexibleHeight = 0;

            // Cancel button – stays up, no flexible space below
            CreateCancelButton(defaultFont);
        }

        private void CreateDifficultyButton(GameDifficulty difficulty, Color color, Font font)
        {
            GameObject btnObj = new GameObject($"{difficulty}Button");
            btnObj.transform.SetParent(popupPanel.transform, false);

            LayoutElement le = btnObj.AddComponent<LayoutElement>();
            le.preferredHeight = 88;

            Image btnImage = btnObj.AddComponent<Image>();
            btnImage.color = color;
            Sprite btnRounded = CuteUIHelper.GetRoundedRectSprite(16);
            if (btnRounded != null)
            {
                btnImage.sprite = btnRounded;
                btnImage.type = Image.Type.Sliced;
            }

            Shadow btnShadow = btnObj.AddComponent<Shadow>();
            btnShadow.effectColor = CuteUIHelper.SoftShadow;
            btnShadow.effectDistance = new Vector2(2, -3);

            Button btn = btnObj.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 0.98f, 0.95f);
            colors.pressedColor = new Color(0.85f, 0.82f, 0.78f);
            colors.fadeDuration = 0.08f;
            btn.colors = colors;
            btn.onClick.AddListener(() => OnDifficultyClicked(difficulty));

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(16, 8);
            textRect.offsetMax = new Vector2(-16, -8);

            Text text = textObj.AddComponent<Text>();
            text.text = GetDifficultyDescription(difficulty);
            text.font = font;
            text.fontSize = 36;
            text.color = CuteUIHelper.DarkText;
            text.alignment = TextAnchor.MiddleCenter;
            text.fontStyle = FontStyle.Bold;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 20;
            text.resizeTextMaxSize = 36;
        }

        private void CreateCancelButton(Font font)
        {
            GameObject btnObj = new GameObject("CancelButton");
            btnObj.transform.SetParent(popupPanel.transform, false);

            LayoutElement le = btnObj.AddComponent<LayoutElement>();
            le.preferredHeight = 64;

            Image btnImage = btnObj.AddComponent<Image>();
            btnImage.color = new Color(0.82f, 0.78f, 0.75f);
            Sprite cancelRounded = CuteUIHelper.GetRoundedRectSprite(14);
            if (cancelRounded != null)
            {
                btnImage.sprite = cancelRounded;
                btnImage.type = Image.Type.Sliced;
            }

            Button btn = btnObj.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 0.98f, 0.95f);
            colors.pressedColor = new Color(0.85f, 0.82f, 0.78f);
            colors.fadeDuration = 0.08f;
            btn.colors = colors;
            btn.onClick.AddListener(Close);

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(8, 4);
            textRect.offsetMax = new Vector2(-8, -4);

            Text text = textObj.AddComponent<Text>();
            text.text = "취소";
            text.font = font;
            text.fontSize = 34;
            text.color = CuteUIHelper.DarkText;
            text.alignment = TextAnchor.MiddleCenter;
            text.fontStyle = FontStyle.Bold;
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
            if (backgroundOverlay != null)
                Destroy(backgroundOverlay);
            Destroy(gameObject);
        }
    }
}
