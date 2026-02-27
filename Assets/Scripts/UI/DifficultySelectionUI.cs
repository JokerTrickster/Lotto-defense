using UnityEngine;
using UnityEngine.UI;
using System;
using LottoDefense.Gameplay;
using LottoDefense.Lobby;

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
            panelRect.sizeDelta = new Vector2(700, 600);

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

            VerticalLayoutGroup vlg = popupPanel.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(40, 40, 30, 30);
            vlg.spacing = 14;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childAlignment = TextAnchor.UpperCenter;

            // Title
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(popupPanel.transform, false);
            LayoutElement titleLE = titleObj.AddComponent<LayoutElement>();
            titleLE.preferredHeight = 60;

            Text titleText = titleObj.AddComponent<Text>();
            titleText.text = "\uB09C\uC774\uB3C4 \uC120\uD0DD";
            titleText.font = defaultFont;
            titleText.fontSize = 48;
            titleText.color = CuteUIHelper.DarkText;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.fontStyle = FontStyle.Bold;

            // Difficulty buttons
            CreateDifficultyButton(GameDifficulty.Normal, new Color(0.45f, 0.82f, 0.55f), defaultFont);
            CreateDifficultyButton(GameDifficulty.Hard, new Color(0.95f, 0.7f, 0.4f), defaultFont);
            CreateDifficultyButton(GameDifficulty.VeryHard, new Color(0.95f, 0.5f, 0.5f), defaultFont);

            // Cancel button - same size as difficulty buttons for consistency
            CreateCancelButton(defaultFont);
        }

        private void CreateDifficultyButton(GameDifficulty difficulty, Color color, Font font)
        {
            bool isUnlocked = LobbyDataManager.IsDifficultyUnlocked(difficulty);

            Color lockedColor = Color.Lerp(color, new Color(0.75f, 0.73f, 0.7f), 0.6f);

            GameObject btnObj = new GameObject($"{difficulty}Button");
            btnObj.transform.SetParent(popupPanel.transform, false);

            LayoutElement le = btnObj.AddComponent<LayoutElement>();
            le.preferredHeight = 88;

            Image btnImage = btnObj.AddComponent<Image>();
            btnImage.color = isUnlocked ? color : lockedColor;
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
            btn.interactable = isUnlocked;
            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 0.98f, 0.95f);
            colors.pressedColor = new Color(0.85f, 0.82f, 0.78f);
            colors.disabledColor = new Color(0.92f, 0.92f, 0.92f, 1f);
            colors.fadeDuration = 0.08f;
            btn.colors = colors;
            btn.onClick.AddListener(() => OnDifficultyClicked(difficulty));

            if (isUnlocked)
            {
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
                text.resizeTextMinSize = 18;
                text.resizeTextMaxSize = 36;
            }
            else
            {
                GameObject nameObj = new GameObject("Name");
                nameObj.transform.SetParent(btnObj.transform, false);
                RectTransform nameRect = nameObj.AddComponent<RectTransform>();
                nameRect.anchorMin = new Vector2(0f, 0.45f);
                nameRect.anchorMax = new Vector2(1f, 1f);
                nameRect.offsetMin = new Vector2(16, 0);
                nameRect.offsetMax = new Vector2(-16, -4);

                Text nameText = nameObj.AddComponent<Text>();
                nameText.text = GetDifficultyName(difficulty);
                nameText.font = font;
                nameText.fontSize = 32;
                nameText.color = new Color(0.35f, 0.33f, 0.3f);
                nameText.alignment = TextAnchor.MiddleCenter;
                nameText.fontStyle = FontStyle.Bold;
                nameText.raycastTarget = false;

                GameObject hintObj = new GameObject("Hint");
                hintObj.transform.SetParent(btnObj.transform, false);
                RectTransform hintRect = hintObj.AddComponent<RectTransform>();
                hintRect.anchorMin = new Vector2(0f, 0f);
                hintRect.anchorMax = new Vector2(1f, 0.5f);
                hintRect.offsetMin = new Vector2(16, 4);
                hintRect.offsetMax = new Vector2(-16, 0);

                Text hintText = hintObj.AddComponent<Text>();
                hintText.text = GetLockHint(difficulty);
                hintText.font = font;
                hintText.fontSize = 22;
                hintText.color = new Color(0.7f, 0.45f, 0.2f);
                hintText.alignment = TextAnchor.MiddleCenter;
                hintText.fontStyle = FontStyle.Bold;
                hintText.raycastTarget = false;
                hintText.resizeTextForBestFit = true;
                hintText.resizeTextMinSize = 16;
                hintText.resizeTextMaxSize = 22;
            }
        }

        private void CreateCancelButton(Font font)
        {
            GameObject btnObj = new GameObject("CancelButton");
            btnObj.transform.SetParent(popupPanel.transform, false);

            LayoutElement le = btnObj.AddComponent<LayoutElement>();
            le.preferredHeight = 88;

            Image btnImage = btnObj.AddComponent<Image>();
            btnImage.color = new Color(0.75f, 0.72f, 0.68f);
            Sprite cancelRounded = CuteUIHelper.GetRoundedRectSprite(16);
            if (cancelRounded != null)
            {
                btnImage.sprite = cancelRounded;
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
            btn.onClick.AddListener(Close);

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(16, 8);
            textRect.offsetMax = new Vector2(-16, -8);

            Text text = textObj.AddComponent<Text>();
            text.text = "\uCDE8\uC18C";
            text.font = font;
            text.fontSize = 36;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            text.fontStyle = FontStyle.Bold;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 24;
            text.resizeTextMaxSize = 36;
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

        private string GetDifficultyName(GameDifficulty difficulty)
        {
            switch (difficulty)
            {
                case GameDifficulty.Normal: return "보통";
                case GameDifficulty.Hard: return "어려움";
                case GameDifficulty.VeryHard: return "매우 어려움";
                default: return "보통";
            }
        }

        private string GetLockHint(GameDifficulty difficulty)
        {
            switch (difficulty)
            {
                case GameDifficulty.Hard: return "보통 클리어 시 해금";
                case GameDifficulty.VeryHard: return "어려움 클리어 시 해금";
                default: return "잠김";
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
