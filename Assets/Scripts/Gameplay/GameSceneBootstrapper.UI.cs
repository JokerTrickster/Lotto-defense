using UnityEngine;
using UnityEngine.UI;
using LottoDefense.UI;
using LottoDefense.Units;
using LottoDefense.Quests;
using LottoDefense.Networking;
using LottoDefense.Profile;

namespace LottoDefense.Gameplay
{
    /// <summary>
    /// Partial class containing all UI creation methods for GameSceneBootstrapper.
    /// </summary>
    public partial class GameSceneBootstrapper
    {
        private void ApplyRoundedSprite(Image image, int radius = 16)
        {
            Sprite rounded = CuteUIHelper.GetRoundedRectSprite(radius);
            if (rounded != null)
            {
                image.sprite = rounded;
                image.type = Image.Type.Sliced;
            }
        }

        #region Countdown UI
        private void EnsureCountdownUI()
        {
            CountdownUI countdown = FindFirstObjectByType<CountdownUI>();
            if (countdown != null) return;

            GameObject countdownObj = new GameObject("CountdownUI");
            countdownObj.transform.SetParent(safeAreaRoot, false);

            RectTransform rect = countdownObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image overlayBg = countdownObj.AddComponent<Image>();
            overlayBg.color = GameSceneDesignTokens.CountdownOverlay;
            overlayBg.raycastTarget = false;

            CanvasGroup canvasGroup = countdownObj.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;

            GameObject textObj = new GameObject("CountdownText");
            textObj.transform.SetParent(countdownObj.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.5f, 0.5f);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.anchoredPosition = Vector2.zero;
            textRect.sizeDelta = new Vector2(400, 300);

            GameObject shadowObj = new GameObject("Shadow");
            shadowObj.transform.SetParent(textObj.transform, false);
            RectTransform shadowRect = shadowObj.AddComponent<RectTransform>();
            shadowRect.anchorMin = Vector2.zero;
            shadowRect.anchorMax = Vector2.one;
            shadowRect.offsetMin = new Vector2(4, -4);
            shadowRect.offsetMax = new Vector2(4, -4);
            Text shadowText = CreateText(shadowObj, "3", GameSceneDesignTokens.CountdownSize, new Color(0.4f, 0.25f, 0.15f, 0.5f));
            shadowText.fontStyle = FontStyle.Bold;
            shadowText.raycastTarget = false;

            Text countdownText = CreateText(textObj, "3", GameSceneDesignTokens.CountdownSize, GameSceneDesignTokens.CountdownText);
            countdownText.fontStyle = FontStyle.Bold;

            Outline outline = textObj.AddComponent<Outline>();
            outline.effectColor = new Color(0.4f, 0.25f, 0.15f, 0.4f);
            outline.effectDistance = new Vector2(3, -3);

            GameObject startTextObj = new GameObject("StartText");
            startTextObj.transform.SetParent(countdownObj.transform, false);

            RectTransform startTextRect = startTextObj.AddComponent<RectTransform>();
            startTextRect.anchorMin = new Vector2(0.5f, 0.5f);
            startTextRect.anchorMax = new Vector2(0.5f, 0.5f);
            startTextRect.anchoredPosition = Vector2.zero;
            startTextRect.sizeDelta = new Vector2(600, 300);

            Text startText = CreateText(startTextObj, "START!", 160, GameSceneDesignTokens.CountdownStartColor);
            startText.fontStyle = FontStyle.Bold;
            startText.raycastTarget = false;

            Outline startOutline = startTextObj.AddComponent<Outline>();
            startOutline.effectColor = new Color(0.4f, 0.25f, 0.15f, 0.4f);
            startOutline.effectDistance = new Vector2(3, -3);

            startTextObj.SetActive(false);

            countdown = countdownObj.AddComponent<CountdownUI>();
            SetField(countdown, "countdownText", countdownText);
            SetField(countdown, "startText", startText);
            SetField(countdown, "canvasGroup", canvasGroup);
        }
        #endregion

        #region Round Start UI
        private void EnsureRoundStartUI()
        {
            RoundStartUI roundStartUI = FindFirstObjectByType<RoundStartUI>();
            if (roundStartUI != null) return;

            GameObject roundStartObj = new GameObject("RoundStartUI");
            roundStartObj.transform.SetParent(safeAreaRoot, false);

            RectTransform rect = roundStartObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            CanvasGroup canvasGroup = roundStartObj.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;

            GameObject textObj = new GameObject("RoundText");
            textObj.transform.SetParent(roundStartObj.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.5f, 0.5f);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.anchoredPosition = Vector2.zero;
            textRect.sizeDelta = new Vector2(600, 200);

            GameObject shadowObj = new GameObject("Shadow");
            shadowObj.transform.SetParent(textObj.transform, false);
            RectTransform shadowRect = shadowObj.AddComponent<RectTransform>();
            shadowRect.anchorMin = Vector2.zero;
            shadowRect.anchorMax = Vector2.one;
            shadowRect.offsetMin = new Vector2(4, -4);
            shadowRect.offsetMax = new Vector2(4, -4);
            Text shadowText = CreateText(shadowObj, "\uB77C\uC6B4\uB4DC 1", 80, new Color(0.4f, 0.25f, 0.15f, 0.5f));
            shadowText.fontStyle = FontStyle.Bold;
            shadowText.raycastTarget = false;

            Text roundText = CreateText(textObj, "\uB77C\uC6B4\uB4DC 1", 80, new Color(0.9f, 0.6f, 0.2f));
            roundText.fontStyle = FontStyle.Bold;

            Outline outline = textObj.AddComponent<Outline>();
            outline.effectColor = new Color(0.4f, 0.25f, 0.15f, 0.4f);
            outline.effectDistance = new Vector2(4, -4);

            RoundStartUI component = roundStartObj.AddComponent<RoundStartUI>();
            SetField(component, "roundText", roundText);
            SetField(component, "canvasGroup", canvasGroup);

        }
        #endregion

        #region Unit Selection UI
        private void EnsureUnitSelectionUI()
        {
            UnitSelectionUI selectionUI = FindFirstObjectByType<UnitSelectionUI>();
            if (selectionUI != null) return;

            GameObject containerObj = new GameObject("UnitSelectionUIContainer");
            containerObj.transform.SetParent(safeAreaRoot, false);

            GameObject selectionPanelObj = new GameObject("UnitSelectionPanel");
            selectionPanelObj.transform.SetParent(safeAreaRoot, false);

            RectTransform panelRect = selectionPanelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.zero;
            panelRect.pivot = new Vector2(0.5f, 0f);
            panelRect.sizeDelta = new Vector2(240f, 42f);

            Image panelBg = selectionPanelObj.AddComponent<Image>();
            panelBg.color = GameSceneDesignTokens.SelectionPanelBg;
            ApplyRoundedSprite(panelBg, 10);

            Outline panelOutline = selectionPanelObj.AddComponent<Outline>();
            panelOutline.effectColor = GameSceneDesignTokens.SelectionPanelBorder;
            panelOutline.effectDistance = new Vector2(2, -2);

            GameObject unitNameObj = new GameObject("UnitNameText");
            unitNameObj.transform.SetParent(selectionPanelObj.transform, false);
            RectTransform nameRect = unitNameObj.AddComponent<RectTransform>();
            nameRect.anchorMin = Vector2.zero;
            nameRect.anchorMax = Vector2.one;
            nameRect.offsetMin = new Vector2(8, 2);
            nameRect.offsetMax = new Vector2(-8, -2);

            Text unitNameText = CreateText(unitNameObj, "\uC720\uB2DB", 18, GameSceneDesignTokens.GoldColor);
            unitNameText.alignment = TextAnchor.MiddleCenter;
            unitNameText.fontStyle = FontStyle.Bold;
            unitNameText.resizeTextForBestFit = true;
            unitNameText.resizeTextMinSize = 14;
            unitNameText.resizeTextMaxSize = 18;

            Outline nameOutline = unitNameObj.AddComponent<Outline>();
            nameOutline.effectColor = new Color(1f, 1f, 1f, 0.4f);
            nameOutline.effectDistance = new Vector2(1, -1);

            UnitSelectionUI component = containerObj.AddComponent<UnitSelectionUI>();
            SetField(component, "selectionPanel", selectionPanelObj);
            SetField(component, "unitNameText", unitNameText);

            selectionPanelObj.SetActive(false);

        }

        private GameObject CreateStatGridRow(Transform parent, string name, float height)
        {
            GameObject row = new GameObject(name);
            row.transform.SetParent(parent, false);
            row.AddComponent<RectTransform>();
            LayoutElement le = row.AddComponent<LayoutElement>();
            le.preferredHeight = height;

            HorizontalLayoutGroup hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 6f;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            return row;
        }

        private Text CreateGridStatText(Transform parent, string placeholder, int fontSize, Color color)
        {
            GameObject obj = new GameObject($"Stat_{placeholder}");
            obj.transform.SetParent(parent, false);
            obj.AddComponent<RectTransform>();
            LayoutElement le = obj.AddComponent<LayoutElement>();
            le.flexibleWidth = 1f;

            Text t = CreateText(obj, placeholder, fontSize, color);
            t.alignment = TextAnchor.MiddleLeft;
            t.fontStyle = FontStyle.Bold;
            t.resizeTextForBestFit = true;
            t.resizeTextMinSize = 16;
            t.resizeTextMaxSize = fontSize;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.supportRichText = true;

            return t;
        }
        #endregion

        #region Game Bottom UI
        private void EnsureGameBottomUI()
        {
            GameBottomUI bottomUI = FindFirstObjectByType<GameBottomUI>();
            if (bottomUI != null) return;

            float panelH = GameSceneDesignTokens.CommandPanelHeight;
            float btnH = GameSceneDesignTokens.CommandButtonHeight;

            GameObject bottomPanelObj = new GameObject("GameBottomUI");
            bottomPanelObj.transform.SetParent(safeAreaRoot, false);

            RectTransform panelRect = bottomPanelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0, 0);
            panelRect.anchorMax = new Vector2(1, 0);
            panelRect.pivot = new Vector2(0.5f, 0);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = new Vector2(0, panelH);

            Image panelBg = bottomPanelObj.AddComponent<Image>();
            panelBg.color = CuteUIHelper.CreamBg;
            ApplyRoundedSprite(panelBg, 20);

            Shadow panelShadow = bottomPanelObj.AddComponent<Shadow>();
            panelShadow.effectColor = CuteUIHelper.SoftShadow;
            panelShadow.effectDistance = new Vector2(0, 3);

            // Vertical layout: unit info on top, buttons below
            VerticalLayoutGroup mainVLayout = bottomPanelObj.AddComponent<VerticalLayoutGroup>();
            mainVLayout.padding = new RectOffset(10, 10, 4, 4);
            mainVLayout.spacing = 4;
            mainVLayout.childControlWidth = true;
            mainVLayout.childControlHeight = true;
            mainVLayout.childForceExpandWidth = true;
            mainVLayout.childForceExpandHeight = false;

            // ========== TOP: Unit Info – compact 2-column grid ==========
            GameObject infoRow = new GameObject("UnitInfoRow");
            infoRow.transform.SetParent(bottomPanelObj.transform, false);
            LayoutElement infoRowLE = infoRow.AddComponent<LayoutElement>();
            infoRowLE.preferredHeight = GameSceneDesignTokens.UnitInfoPanelHeight;

            Image infoBg = infoRow.AddComponent<Image>();
            infoBg.color = new Color(0.96f, 0.93f, 0.88f, 0.7f);
            infoBg.raycastTarget = false;
            ApplyRoundedSprite(infoBg, 12);

            GameObject statsContainer = new GameObject("StatsContainer");
            statsContainer.transform.SetParent(infoRow.transform, false);
            RectTransform statsContainerRect = statsContainer.AddComponent<RectTransform>();
            statsContainerRect.anchorMin = Vector2.zero;
            statsContainerRect.anchorMax = Vector2.one;
            statsContainerRect.offsetMin = new Vector2(6, 2);
            statsContainerRect.offsetMax = new Vector2(-6, -2);

            VerticalLayoutGroup infoVLayout = statsContainer.AddComponent<VerticalLayoutGroup>();
            infoVLayout.spacing = 3f;
            infoVLayout.padding = new RectOffset(4, 4, 4, 4);
            infoVLayout.childControlWidth = true;
            infoVLayout.childControlHeight = true;
            infoVLayout.childForceExpandWidth = true;
            infoVLayout.childForceExpandHeight = false;

            // --- Row 0: Portrait + Name ---
            float rowH = GameSceneDesignTokens.UnitInfoStatRowHeight;
            int statFont = GameSceneDesignTokens.UnitInfoDetailSize;

            GameObject nameRow = new GameObject("NameRow");
            nameRow.transform.SetParent(statsContainer.transform, false);
            nameRow.AddComponent<RectTransform>();
            LayoutElement nameRowLE = nameRow.AddComponent<LayoutElement>();
            nameRowLE.preferredHeight = 44f;

            HorizontalLayoutGroup nameHLG = nameRow.AddComponent<HorizontalLayoutGroup>();
            nameHLG.spacing = 10f;
            nameHLG.childControlWidth = true;
            nameHLG.childControlHeight = true;
            nameHLG.childForceExpandWidth = false;
            nameHLG.childForceExpandHeight = true;

            GameObject portraitObj = new GameObject("Portrait");
            portraitObj.transform.SetParent(nameRow.transform, false);
            LayoutElement portraitLE = portraitObj.AddComponent<LayoutElement>();
            portraitLE.preferredWidth = 44;
            portraitLE.preferredHeight = 44;
            portraitLE.minWidth = 44;
            portraitLE.minHeight = 44;

            Image portraitBg = portraitObj.AddComponent<Image>();
            portraitBg.color = new Color(0.92f, 0.88f, 0.82f, 0.85f);
            portraitBg.raycastTarget = false;
            ApplyRoundedSprite(portraitBg, 10);
            portraitObj.AddComponent<UnityEngine.UI.AspectRatioFitter>().aspectMode = UnityEngine.UI.AspectRatioFitter.AspectMode.HeightControlsWidth;

            GameObject portraitCircleObj = new GameObject("PortraitCircle");
            portraitCircleObj.transform.SetParent(portraitObj.transform, false);
            RectTransform circleRect = portraitCircleObj.AddComponent<RectTransform>();
            circleRect.anchorMin = new Vector2(0.08f, 0.08f);
            circleRect.anchorMax = new Vector2(0.92f, 0.92f);
            circleRect.offsetMin = Vector2.zero;
            circleRect.offsetMax = Vector2.zero;

            Image portraitImage = portraitCircleObj.AddComponent<Image>();
            portraitImage.sprite = UnitData.CreateCircleSprite(64);
            portraitImage.color = Color.white;
            portraitImage.raycastTarget = false;

            GameObject nameTextObj = new GameObject("NameText");
            nameTextObj.transform.SetParent(nameRow.transform, false);
            nameTextObj.AddComponent<RectTransform>();
            LayoutElement nameTextLE = nameTextObj.AddComponent<LayoutElement>();
            nameTextLE.flexibleWidth = 1f;

            Text infoNameText = CreateText(nameTextObj, "", GameSceneDesignTokens.UnitInfoNameSize, CuteUIHelper.DarkText);
            infoNameText.alignment = TextAnchor.MiddleLeft;
            infoNameText.fontStyle = FontStyle.Bold;
            infoNameText.resizeTextForBestFit = true;
            infoNameText.resizeTextMinSize = 20;
            infoNameText.resizeTextMaxSize = GameSceneDesignTokens.UnitInfoNameSize;
            infoNameText.horizontalOverflow = HorizontalWrapMode.Wrap;
            infoNameText.verticalOverflow = VerticalWrapMode.Truncate;

            // --- Row 1: ATK | SPD (includes upgrade level) ---
            GameObject gridRow1 = CreateStatGridRow(statsContainer.transform, "GridRow1", rowH);
            Text infoAtkText = CreateGridStatText(gridRow1.transform, "ATK", statFont, GameSceneDesignTokens.UnitInfoAttack);
            Text infoSpdText = CreateGridStatText(gridRow1.transform, "SPD", statFont, GameSceneDesignTokens.UnitInfoSpeed);

            // --- Row 2: RNG | TYPE | DEF ---
            GameObject gridRow2 = CreateStatGridRow(statsContainer.transform, "GridRow2", rowH);
            Text infoRngText = CreateGridStatText(gridRow2.transform, "RNG", statFont, GameSceneDesignTokens.UnitInfoRange);
            Text infoPatternText = CreateGridStatText(gridRow2.transform, "TYPE", statFont, GameSceneDesignTokens.UnitInfoPatternColor);
            Text infoDefText = CreateGridStatText(gridRow2.transform, "DEF", statFont, GameSceneDesignTokens.UnitInfoDefense);

            // --- Row 3: SKILL + Mana bar ---
            GameObject skillRow = CreateStatGridRow(statsContainer.transform, "SkillRow", rowH);
            Text infoSkillText = CreateGridStatText(skillRow.transform, "SKILL", statFont, GameSceneDesignTokens.UnitInfoSkillColor);

            GameObject manaRowObj = new GameObject("ManaBarContainer");
            manaRowObj.transform.SetParent(skillRow.transform, false);
            manaRowObj.AddComponent<RectTransform>();
            LayoutElement manaRowLE = manaRowObj.AddComponent<LayoutElement>();
            manaRowLE.flexibleWidth = 0.6f;
            manaRowLE.preferredHeight = 14f;

            Image manaBarBgImg = manaRowObj.AddComponent<Image>();
            manaBarBgImg.color = GameSceneDesignTokens.ManaBarBg;
            manaBarBgImg.raycastTarget = false;
            ApplyRoundedSprite(manaBarBgImg, 4);

            GameObject manaFillObj = new GameObject("ManaBarFill");
            manaFillObj.transform.SetParent(manaRowObj.transform, false);
            RectTransform manaFillRect = manaFillObj.AddComponent<RectTransform>();
            manaFillRect.anchorMin = Vector2.zero;
            manaFillRect.anchorMax = Vector2.one;
            manaFillRect.offsetMin = new Vector2(2, 2);
            manaFillRect.offsetMax = new Vector2(-2, -2);

            Image manaFillImg = manaFillObj.AddComponent<Image>();
            manaFillImg.color = GameSceneDesignTokens.ManaBarFill;
            manaFillImg.type = Image.Type.Filled;
            manaFillImg.fillMethod = Image.FillMethod.Horizontal;
            manaFillImg.fillAmount = 0f;
            manaFillImg.raycastTarget = false;

            // Empty state text
            GameObject emptyStateObj = new GameObject("EmptyStateText");
            emptyStateObj.transform.SetParent(infoRow.transform, false);
            RectTransform emptyRect = emptyStateObj.AddComponent<RectTransform>();
            emptyRect.anchorMin = Vector2.zero;
            emptyRect.anchorMax = Vector2.one;
            emptyRect.offsetMin = Vector2.zero;
            emptyRect.offsetMax = Vector2.zero;

            Text emptyText = CreateText(emptyStateObj, "\uC720\uB2DB\uC744 \uC120\uD0DD\uD558\uC138\uC694", 24, new Color(0.6f, 0.55f, 0.5f, 0.8f));
            emptyText.alignment = TextAnchor.MiddleCenter;

            UnitInfoPanel infoPanel = infoRow.AddComponent<UnitInfoPanel>();
            SetField(infoPanel, "portraitImage", portraitImage);
            SetField(infoPanel, "unitNameText", infoNameText);
            SetField(infoPanel, "attackText", infoAtkText);
            SetField(infoPanel, "speedText", infoSpdText);
            SetField(infoPanel, "rangeText", infoRngText);
            SetField(infoPanel, "patternText", infoPatternText);
            SetField(infoPanel, "defenseText", infoDefText);
            SetField(infoPanel, "skillText", infoSkillText);
            SetField(infoPanel, "manaBarFill", manaFillImg);
            SetField(infoPanel, "manaBarContainer", manaRowObj);
            SetField(infoPanel, "statsContainer", statsContainer);
            SetField(infoPanel, "emptyStateText", emptyText);

            statsContainer.SetActive(false);
            emptyStateObj.SetActive(true);

            // ========== BOTTOM: 2 rows of buttons ==========
            // Row 1: Summon + Sell (primary actions, larger tap targets)
            GameObject buttonRow1 = new GameObject("ButtonRow1");
            buttonRow1.transform.SetParent(bottomPanelObj.transform, false);
            LayoutElement buttonRow1LE = buttonRow1.AddComponent<LayoutElement>();
            buttonRow1LE.preferredHeight = btnH;

            HorizontalLayoutGroup row1H = buttonRow1.AddComponent<HorizontalLayoutGroup>();
            row1H.spacing = 12;
            row1H.childControlWidth = true;
            row1H.childControlHeight = true;
            row1H.childForceExpandWidth = true;
            row1H.childForceExpandHeight = true;

            GameObject summonBtnObj = CreateCommandButton(buttonRow1.transform, "SummonButton",
                "\uC18C\uD658 5G", GameSceneDesignTokens.SummonButtonBg, GameSceneDesignTokens.SummonButtonBorder);
            Button summonBtn = summonBtnObj.GetComponent<Button>();
            Text summonBtnText = summonBtnObj.GetComponentInChildren<Text>();
            summonBtnText.supportRichText = true;

            GameObject sellBtnObj = CreateCommandButton(buttonRow1.transform, "SellButton",
                "\uD310\uB9E4", GameSceneDesignTokens.SellBtnBg, GameSceneDesignTokens.SellBtnBorder);
            Button sellBtn = sellBtnObj.GetComponent<Button>();
            Text sellBtnText = sellBtnObj.GetComponentInChildren<Text>();

            // Row 2: Attack Upgrade + Speed Upgrade
            GameObject buttonRow2 = new GameObject("ButtonRow2");
            buttonRow2.transform.SetParent(bottomPanelObj.transform, false);
            LayoutElement buttonRow2LE = buttonRow2.AddComponent<LayoutElement>();
            buttonRow2LE.preferredHeight = btnH;

            HorizontalLayoutGroup row2H = buttonRow2.AddComponent<HorizontalLayoutGroup>();
            row2H.spacing = 12;
            row2H.childControlWidth = true;
            row2H.childControlHeight = true;
            row2H.childForceExpandWidth = true;
            row2H.childForceExpandHeight = true;

            GameObject atkUpBtnObj = CreateCommandButton(buttonRow2.transform, "AttackUpgradeButton",
                "\uACF5\uACA9\u2191", GameSceneDesignTokens.AttackUpBtnBg, GameSceneDesignTokens.AttackUpBtnBorder);
            Button atkUpBtn = atkUpBtnObj.GetComponent<Button>();
            Text atkUpBtnText = atkUpBtnObj.GetComponentInChildren<Text>();

            GameObject spdUpBtnObj = CreateCommandButton(buttonRow2.transform, "AttackSpeedUpgradeButton",
                "\uACF5\uC18D\u2191", GameSceneDesignTokens.SpeedUpBtnBg, GameSceneDesignTokens.SpeedUpBtnBorder);
            Button spdUpBtn = spdUpBtnObj.GetComponent<Button>();
            Text spdUpBtnText = spdUpBtnObj.GetComponentInChildren<Text>();

            // ========== Wire GameBottomUI component ==========
            GameBottomUI component = bottomPanelObj.AddComponent<GameBottomUI>();
            SetField(component, "panel", bottomPanelObj);
            SetField(component, "summonButton", summonBtn);
            SetField(component, "sellButton", sellBtn);
            SetField(component, "attackUpgradeButton", atkUpBtn);
            SetField(component, "attackSpeedUpgradeButton", spdUpBtn);
            SetField(component, "summonButtonText", summonBtnText);
            SetField(component, "sellButtonText", sellBtnText);
            SetField(component, "attackUpgradeButtonText", atkUpBtnText);
            SetField(component, "attackSpeedUpgradeButtonText", spdUpBtnText);

            component.SetUnitInfoPanel(infoPanel);

            bottomPanelObj.SetActive(true);
            component.EnsureListeners();

        }

        private GameObject CreateCommandButton(Transform parent, string name, string text, Color bgColor, Color borderColor)
        {
            float btnH = GameSceneDesignTokens.CommandButtonHeight;
            int textSize = GameSceneDesignTokens.CommandButtonTextSize;

            GameObject buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(parent, false);

            LayoutElement layoutElement = buttonObj.AddComponent<LayoutElement>();
            layoutElement.minHeight = btnH - 5;
            layoutElement.preferredHeight = btnH;

            Image buttonBg = buttonObj.AddComponent<Image>();
            buttonBg.color = bgColor;
            ApplyRoundedSprite(buttonBg, 12);

            Shadow buttonShadow = buttonObj.AddComponent<Shadow>();
            buttonShadow.effectColor = CuteUIHelper.SoftShadow;
            buttonShadow.effectDistance = new Vector2(2, -3);

            Button button = buttonObj.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 0.98f, 0.95f, 1f);
            colors.pressedColor = new Color(0.85f, 0.82f, 0.78f, 1f);
            colors.disabledColor = GameSceneDesignTokens.ActionBtnDisabled;
            colors.fadeDuration = 0.08f;
            button.colors = colors;

            // Inner highlight strip (top half for soft bevel)
            GameObject highlightObj = new GameObject("Highlight");
            highlightObj.transform.SetParent(buttonObj.transform, false);
            RectTransform hlRect = highlightObj.AddComponent<RectTransform>();
            hlRect.anchorMin = new Vector2(0, 0.5f);
            hlRect.anchorMax = Vector2.one;
            hlRect.offsetMin = new Vector2(4, 0);
            hlRect.offsetMax = new Vector2(-4, -3);
            Image hlImg = highlightObj.AddComponent<Image>();
            hlImg.color = new Color(1f, 1f, 1f, 0.18f);
            hlImg.raycastTarget = false;
            ApplyRoundedSprite(hlImg, 8);

            // Button text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(4, 2);
            textRect.offsetMax = new Vector2(-4, -2);

            Text buttonText = CreateText(textObj, text, textSize, CuteUIHelper.DarkText);
            buttonText.alignment = TextAnchor.MiddleCenter;
            buttonText.fontStyle = FontStyle.Bold;
            buttonText.resizeTextForBestFit = true;
            buttonText.resizeTextMinSize = 20;
            buttonText.resizeTextMaxSize = textSize;
            buttonText.supportRichText = true;

            return buttonObj;
        }
        #endregion

        #region HUD
        private void EnsureGameHUD()
        {
            if (FindFirstObjectByType<GameHUD>() != null) return;

            GameObject hudObj = new GameObject("GameHUD");
            hudObj.transform.SetParent(safeAreaRoot, false);

            RectTransform hudRect = hudObj.AddComponent<RectTransform>();
            hudRect.anchorMin = new Vector2(0, 1);
            hudRect.anchorMax = new Vector2(1, 1);
            hudRect.pivot = new Vector2(0.5f, 1);
            hudRect.anchoredPosition = Vector2.zero;
            hudRect.sizeDelta = new Vector2(0, GameSceneDesignTokens.HudHeight);

            Image hudBg = hudObj.AddComponent<Image>();
            hudBg.color = GameSceneDesignTokens.HudBackground;
            hudBg.raycastTarget = true;
            ApplyRoundedSprite(hudBg, 20);

            Shadow hudShadow = hudObj.AddComponent<Shadow>();
            hudShadow.effectColor = CuteUIHelper.SoftShadow;
            hudShadow.effectDistance = new Vector2(0, -3);

            VerticalLayoutGroup vlayout = hudObj.AddComponent<VerticalLayoutGroup>();
            int padH = Mathf.RoundToInt(GameSceneDesignTokens.HudPaddingH);
            int padV = Mathf.RoundToInt(GameSceneDesignTokens.HudPaddingV);
            vlayout.padding = new RectOffset(padH, padH, padV, padV);
            vlayout.spacing = 6;
            vlayout.childForceExpandWidth = true;
            vlayout.childForceExpandHeight = false;
            vlayout.childControlWidth = true;
            vlayout.childControlHeight = true;

            // ---- Row 1: ROUND | PHASE | TIMER ----
            GameObject row1 = CreateHUDRow(hudObj.transform, "Row_Top", 48);
            Text roundText = CreateStatWithLabel(row1.transform, "Round", "ROUND", "R1",
                GameSceneDesignTokens.StatLabel, GameSceneDesignTokens.RoundColor,
                GameSceneDesignTokens.StatLabelSize, GameSceneDesignTokens.StatValueSize);

            Text phaseText = CreateStatWithLabel(row1.transform, "Phase", "PHASE", "COUNTDOWN",
                GameSceneDesignTokens.StatLabel, GameSceneDesignTokens.PhaseColor,
                GameSceneDesignTokens.StatLabelSize, GameSceneDesignTokens.PhaseTextSize);

            Text timeText = CreateStatWithLabel(row1.transform, "Time", "TIME", "00:00",
                GameSceneDesignTokens.StatLabel, GameSceneDesignTokens.TimeColor,
                GameSceneDesignTokens.StatLabelSize, GameSceneDesignTokens.StatValueSize);

            CreateDivider(hudObj.transform);

            // ---- Row 2: PROFILE | LIFE | GOLD | MONSTERS | UNITS ----
            GameObject row2 = CreateHUDRow(hudObj.transform, "Row_Stats", 52);

            CreateInlineProfileHeader(row2.transform);

            Text lifeText = CreateStatCard(row2.transform, "Life", "\u2665", "10",
                GameSceneDesignTokens.LifeColor);
            Text goldText = CreateStatCard(row2.transform, "Gold", "\u2666", "30",
                GameSceneDesignTokens.GoldColor);
            Text monsterText = CreateStatCard(row2.transform, "Monster", "\u25C6", "0",
                GameSceneDesignTokens.MonsterColor);
            Text unitText = CreateStatCard(row2.transform, "Unit", "\u25A0", "0",
                GameSceneDesignTokens.UnitColor);

            GameHUD hud = hudObj.AddComponent<GameHUD>();
            SetField(hud, "roundText", roundText);
            SetField(hud, "phaseText", phaseText);
            SetField(hud, "timeText", timeText);
            SetField(hud, "monsterText", monsterText);
            SetField(hud, "goldText", goldText);
            SetField(hud, "unitText", unitText);
            SetField(hud, "lifeText", lifeText);

            RectTransform[] statContainers = new RectTransform[]
            {
                roundText.transform.parent.GetComponent<RectTransform>(),
                phaseText.transform.parent.GetComponent<RectTransform>(),
                timeText.transform.parent.GetComponent<RectTransform>(),
                lifeText.transform.parent.GetComponent<RectTransform>(),
                goldText.transform.parent.GetComponent<RectTransform>(),
                monsterText.transform.parent.GetComponent<RectTransform>(),
                unitText.transform.parent.GetComponent<RectTransform>()
            };
            SetField(hud, "statContainers", statContainers);

            // Tooltip panel
            GameObject tooltipObj = new GameObject("TooltipPanel");
            tooltipObj.transform.SetParent(safeAreaRoot, false);

            RectTransform tooltipRect = tooltipObj.AddComponent<RectTransform>();
            tooltipRect.anchorMin = new Vector2(0, 1);
            tooltipRect.anchorMax = new Vector2(1, 1);
            tooltipRect.pivot = new Vector2(0.5f, 1);
            tooltipRect.anchoredPosition = new Vector2(0, -GameSceneDesignTokens.HudHeight);
            tooltipRect.sizeDelta = new Vector2(0, 44);

            Image tooltipBg = tooltipObj.AddComponent<Image>();
            tooltipBg.color = new Color(1f, 0.97f, 0.92f, 0.96f);
            tooltipBg.raycastTarget = true;
            ApplyRoundedSprite(tooltipBg, 10);

            GameObject tooltipTextObj = new GameObject("Text");
            tooltipTextObj.transform.SetParent(tooltipObj.transform, false);
            RectTransform textRect = tooltipTextObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(16, 4);
            textRect.offsetMax = new Vector2(-16, -4);

            Text tooltipTextComp = CreateText(tooltipTextObj, "", 24, CuteUIHelper.DarkText);
            tooltipTextComp.alignment = TextAnchor.MiddleCenter;

            tooltipObj.SetActive(false);

            SetField(hud, "tooltipPanel", tooltipObj);
            SetField(hud, "tooltipText", tooltipTextComp);
        }

        private GameObject CreateHUDRow(Transform parent, string name, float height)
        {
            GameObject row = new GameObject(name);
            row.transform.SetParent(parent, false);

            RectTransform rect = row.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, height);

            HorizontalLayoutGroup layout = row.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 10;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childAlignment = TextAnchor.MiddleCenter;

            return row;
        }

        private Text CreateStatWithLabel(Transform parent, string name, string label, string value,
            Color labelColor, Color valueColor, int labelSize, int valueSize)
        {
            GameObject container = new GameObject(name);
            container.transform.SetParent(parent, false);
            container.AddComponent<RectTransform>();

            VerticalLayoutGroup vl = container.AddComponent<VerticalLayoutGroup>();
            vl.spacing = 0;
            vl.childForceExpandWidth = true;
            vl.childForceExpandHeight = true;
            vl.childControlWidth = true;
            vl.childControlHeight = true;
            vl.childAlignment = TextAnchor.MiddleCenter;

            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(container.transform, false);
            labelObj.AddComponent<RectTransform>();
            Text labelText = CreateText(labelObj, label, labelSize, labelColor);
            labelText.fontStyle = FontStyle.Bold;
            labelText.raycastTarget = false;

            GameObject valueObj = new GameObject("Value");
            valueObj.transform.SetParent(container.transform, false);
            valueObj.AddComponent<RectTransform>();
            Text valueText = CreateText(valueObj, value, valueSize, valueColor);
            valueText.fontStyle = FontStyle.Bold;
            valueText.raycastTarget = false;

            return valueText;
        }

        private Text CreateStatCard(Transform parent, string name, string icon, string value, Color accentColor)
        {
            GameObject card = new GameObject(name);
            card.transform.SetParent(parent, false);

            RectTransform cardRect = card.AddComponent<RectTransform>();

            Image cardBg = card.AddComponent<Image>();
            cardBg.color = GameSceneDesignTokens.HudStatCardBg;
            cardBg.raycastTarget = false;
            ApplyRoundedSprite(cardBg, 10);

            HorizontalLayoutGroup hl = card.AddComponent<HorizontalLayoutGroup>();
            int pad = Mathf.RoundToInt(GameSceneDesignTokens.StatCardPadding);
            hl.padding = new RectOffset(pad + 4, pad + 4, pad, pad);
            hl.spacing = 4;
            hl.childForceExpandWidth = false;
            hl.childForceExpandHeight = true;
            hl.childControlWidth = true;
            hl.childControlHeight = true;
            hl.childAlignment = TextAnchor.MiddleCenter;

            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(card.transform, false);
            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            LayoutElement iconLE = iconObj.AddComponent<LayoutElement>();
            iconLE.preferredWidth = 32;
            Text iconText = CreateText(iconObj, icon, 26, accentColor);
            iconText.fontStyle = FontStyle.Bold;
            iconText.raycastTarget = false;

            GameObject valueObj = new GameObject("Value");
            valueObj.transform.SetParent(card.transform, false);
            valueObj.AddComponent<RectTransform>();
            LayoutElement valueLE = valueObj.AddComponent<LayoutElement>();
            valueLE.flexibleWidth = 1;
            Text valueText = CreateText(valueObj, value, GameSceneDesignTokens.StatValueSize, CuteUIHelper.DarkText);
            valueText.fontStyle = FontStyle.Bold;
            valueText.raycastTarget = false;

            return valueText;
        }

        private void CreateDivider(Transform parent)
        {
            GameObject divider = new GameObject("Divider");
            divider.transform.SetParent(parent, false);

            RectTransform divRect = divider.AddComponent<RectTransform>();
            divRect.sizeDelta = new Vector2(0, 1);

            LayoutElement le = divider.AddComponent<LayoutElement>();
            le.preferredHeight = 1;
            le.flexibleWidth = 1;

            Image divImg = divider.AddComponent<Image>();
            divImg.color = GameSceneDesignTokens.HudBorder;
            divImg.raycastTarget = false;
        }

        private void CreateInlineProfileHeader(Transform parent)
        {
            GameObject profileContainer = new GameObject("ProfileInline");
            profileContainer.transform.SetParent(parent, false);

            LayoutElement profileLE = profileContainer.AddComponent<LayoutElement>();
            profileLE.preferredWidth = 140;
            profileLE.minWidth = 100;

            Image profileBg = profileContainer.AddComponent<Image>();
            profileBg.color = new Color(0.96f, 0.93f, 0.88f, 0.85f);
            ApplyRoundedSprite(profileBg, 10);

            Button profileButton = profileContainer.AddComponent<Button>();
            ColorBlock colors = profileButton.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 0.98f, 0.95f, 1f);
            colors.pressedColor = new Color(0.9f, 0.87f, 0.82f, 1f);
            profileButton.colors = colors;

            HorizontalLayoutGroup hlg = profileContainer.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 4;
            hlg.padding = new RectOffset(4, 4, 3, 3);
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childAlignment = TextAnchor.MiddleLeft;

            // Avatar border
            GameObject avatarBorderObj = new GameObject("AvatarBorder");
            avatarBorderObj.transform.SetParent(profileContainer.transform, false);
            RectTransform avatarBorderRect = avatarBorderObj.AddComponent<RectTransform>();
            avatarBorderRect.sizeDelta = new Vector2(36, 36);

            LayoutElement avatarBorderLE = avatarBorderObj.AddComponent<LayoutElement>();
            avatarBorderLE.preferredWidth = 36;
            avatarBorderLE.minWidth = 36;
            avatarBorderLE.preferredHeight = 36;

            Image avatarBorderImage = avatarBorderObj.AddComponent<Image>();
            avatarBorderImage.color = Color.white;
            ApplyRoundedSprite(avatarBorderImage, 6);

            // Avatar icon
            GameObject avatarIconObj = new GameObject("AvatarIcon");
            avatarIconObj.transform.SetParent(avatarBorderObj.transform, false);
            RectTransform avatarIconRect = avatarIconObj.AddComponent<RectTransform>();
            avatarIconRect.anchorMin = new Vector2(0.1f, 0.1f);
            avatarIconRect.anchorMax = new Vector2(0.9f, 0.9f);
            avatarIconRect.offsetMin = Vector2.zero;
            avatarIconRect.offsetMax = Vector2.zero;

            Image avatarIcon = avatarIconObj.AddComponent<Image>();
            avatarIcon.sprite = UnitData.CreateCircleSprite(32);
            avatarIcon.color = Color.white;
            avatarIcon.raycastTarget = false;

            // Nickname text
            GameObject nicknameObj = new GameObject("NicknameText");
            nicknameObj.transform.SetParent(profileContainer.transform, false);
            LayoutElement nicknameLE = nicknameObj.AddComponent<LayoutElement>();
            nicknameLE.flexibleWidth = 1;

            Text nicknameText = CreateText(nicknameObj, "Player", 18, CuteUIHelper.DarkText);
            nicknameText.alignment = TextAnchor.MiddleLeft;
            nicknameText.fontStyle = FontStyle.Bold;
            nicknameText.raycastTarget = false;
            nicknameText.resizeTextForBestFit = true;
            nicknameText.resizeTextMinSize = 12;
            nicknameText.resizeTextMaxSize = 18;

            ProfileHeaderDisplay profileDisplay = profileContainer.AddComponent<ProfileHeaderDisplay>();
            SetField(profileDisplay, "avatarImage", avatarIcon);
            SetField(profileDisplay, "borderImage", avatarBorderImage);
            SetField(profileDisplay, "nicknameText", nicknameText);
            SetField(profileDisplay, "profileButton", profileButton);
        }

        private void EnsureProfileSelectionUI()
        {
            if (FindFirstObjectByType<ProfileSelectionUI>() != null) return;

            Canvas canvas = GetComponentInChildren<Canvas>();
            if (canvas == null) canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            GameObject overlayObj = new GameObject("ProfileSelectionOverlay");
            overlayObj.transform.SetParent(canvas.transform, false);

            RectTransform overlayRect = overlayObj.AddComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.sizeDelta = Vector2.zero;

            Image overlayImg = overlayObj.AddComponent<Image>();
            overlayImg.color = new Color(0f, 0f, 0f, 0.6f);
            overlayImg.raycastTarget = true;

            Canvas overlayCanvas = overlayObj.AddComponent<Canvas>();
            overlayCanvas.overrideSorting = true;
            overlayCanvas.sortingOrder = 100;
            overlayObj.AddComponent<GraphicRaycaster>();

            GameObject panelObj = new GameObject("ProfilePanel");
            panelObj.transform.SetParent(overlayObj.transform, false);

            RectTransform panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.05f, 0.3f);
            panelRect.anchorMax = new Vector2(0.95f, 0.75f);
            panelRect.sizeDelta = Vector2.zero;

            Image panelBg = panelObj.AddComponent<Image>();
            panelBg.color = new Color(0.98f, 0.96f, 0.93f, 1f);
            ApplyRoundedSprite(panelBg, 24);

            VerticalLayoutGroup panelVLG = panelObj.AddComponent<VerticalLayoutGroup>();
            panelVLG.padding = new RectOffset(8, 8, 12, 12);
            panelVLG.spacing = 8;
            panelVLG.childControlWidth = true;
            panelVLG.childControlHeight = false;
            panelVLG.childForceExpandWidth = true;
            panelVLG.childForceExpandHeight = false;

            // Title
            GameObject titleRow = new GameObject("TitleRow");
            titleRow.transform.SetParent(panelObj.transform, false);
            LayoutElement titleLE = titleRow.AddComponent<LayoutElement>();
            titleLE.preferredHeight = 44;

            Text titleText = CreateText(titleRow, "프로필 선택", 28, CuteUIHelper.DarkText);
            titleText.alignment = TextAnchor.MiddleCenter;

            // Close button
            GameObject closeObj = new GameObject("CloseButton");
            closeObj.transform.SetParent(titleRow.transform, false);
            RectTransform closeRect = closeObj.AddComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(1f, 0.5f);
            closeRect.anchorMax = new Vector2(1f, 0.5f);
            closeRect.pivot = new Vector2(1f, 0.5f);
            closeRect.anchoredPosition = Vector2.zero;
            closeRect.sizeDelta = new Vector2(40, 40);

            Image closeBg = closeObj.AddComponent<Image>();
            closeBg.color = new Color(0.95f, 0.5f, 0.5f, 1f);
            ApplyRoundedSprite(closeBg, 10);

            Button closeButton = closeObj.AddComponent<Button>();

            GameObject closeTextObj = new GameObject("X");
            closeTextObj.transform.SetParent(closeObj.transform, false);
            RectTransform closeTextRect = closeTextObj.AddComponent<RectTransform>();
            closeTextRect.anchorMin = Vector2.zero;
            closeTextRect.anchorMax = Vector2.one;
            closeTextRect.sizeDelta = Vector2.zero;
            CreateText(closeTextObj, "X", 22, Color.white);

            // Avatar grid (scrollable)
            GameObject scrollObj = new GameObject("AvatarScroll");
            scrollObj.transform.SetParent(panelObj.transform, false);
            LayoutElement scrollLE = scrollObj.AddComponent<LayoutElement>();
            scrollLE.flexibleHeight = 1;

            Image scrollBg = scrollObj.AddComponent<Image>();
            scrollBg.color = new Color(0.94f, 0.92f, 0.9f, 0.3f);

            ScrollRect scroll = scrollObj.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scrollObj.AddComponent<RectMask2D>();

            GameObject gridContent = new GameObject("GridContent");
            gridContent.transform.SetParent(scrollObj.transform, false);
            RectTransform gcRect = gridContent.AddComponent<RectTransform>();
            gcRect.anchorMin = new Vector2(0f, 1f);
            gcRect.anchorMax = new Vector2(1f, 1f);
            gcRect.pivot = new Vector2(0.5f, 1f);
            gcRect.sizeDelta = new Vector2(0, 300);

            GridLayoutGroup gridLayout = gridContent.AddComponent<GridLayoutGroup>();
            gridLayout.cellSize = new Vector2(90, 140);
            gridLayout.spacing = new Vector2(8, 8);
            gridLayout.padding = new RectOffset(4, 4, 6, 6);
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 4;
            gridLayout.childAlignment = TextAnchor.UpperCenter;

            ContentSizeFitter gridFitter = gridContent.AddComponent<ContentSizeFitter>();
            gridFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.content = gcRect;

            // Wire ProfileSelectionUI (only panel, close, grid)
            ProfileSelectionUI selUI = overlayObj.AddComponent<ProfileSelectionUI>();
            SetField(selUI, "panel", overlayObj);
            SetField(selUI, "closeButton", closeButton);
            SetField(selUI, "avatarGridContainer", gridContent.transform);

            overlayObj.SetActive(false);

            ProfileHeaderDisplay headerDisplay = FindFirstObjectByType<ProfileHeaderDisplay>();
            if (headerDisplay != null)
                headerDisplay.SetProfileSelectionUI(selUI);
        }
        #endregion

        #region Buttons
        private void EnsureSynthesisGuideButton()
        {
            GameObject btnObj = new GameObject("SynthesisGuideButton");
            btnObj.transform.SetParent(safeAreaRoot, false);

            float btnSize = GameSceneDesignTokens.UtilityButtonSize;
            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(1, 1);
            btnRect.anchorMax = new Vector2(1, 1);
            btnRect.pivot = new Vector2(1, 1);
            btnRect.anchoredPosition = new Vector2(-12, -(GameSceneDesignTokens.HudHeight + 8));
            btnRect.sizeDelta = new Vector2(btnSize, btnSize);

            Image btnImage = btnObj.AddComponent<Image>();
            btnImage.color = new Color(0.55f, 0.75f, 0.95f, 1f);
            ApplyRoundedSprite(btnImage, 14);

            Shadow btnShadow = btnObj.AddComponent<Shadow>();
            btnShadow.effectColor = CuteUIHelper.SoftShadow;
            btnShadow.effectDistance = new Vector2(2, -2);

            Button button = btnObj.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 0.98f, 0.95f, 1f);
            colors.pressedColor = new Color(0.85f, 0.82f, 0.78f, 1f);
            colors.fadeDuration = 0.1f;
            button.colors = colors;

            EnsureSynthesisGuideUI();

            button.onClick.AddListener(() => {
                SynthesisGuideUI guide = FindFirstObjectByType<SynthesisGuideUI>(FindObjectsInactive.Include);
                if (guide != null)
                {
                    guide.Show();
                }
            });

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            Text btnText = CreateText(textObj, "?", 28, CuteUIHelper.DarkText);
            btnText.fontStyle = FontStyle.Bold;
        }

        private void EnsureSynthesisGuideUI()
        {
            SynthesisGuideUI existingUI = FindFirstObjectByType<SynthesisGuideUI>();
            if (existingUI != null) return;

            GameObject guideObj = new GameObject("SynthesisGuideUI");
            guideObj.transform.SetParent(safeAreaRoot, false);

            RectTransform rect = guideObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Canvas overrideCanvas = guideObj.AddComponent<Canvas>();
            overrideCanvas.overrideSorting = true;
            overrideCanvas.sortingOrder = 200;
            guideObj.AddComponent<GraphicRaycaster>();

            Image bgImage = guideObj.AddComponent<Image>();
            bgImage.color = CuteUIHelper.WarmOverlay;

            // Main panel
            GameObject panelObj = new GameObject("Panel");
            panelObj.transform.SetParent(guideObj.transform, false);

            RectTransform panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = new Vector2(700, 850);

            Image panelImage = panelObj.AddComponent<Image>();
            panelImage.color = CuteUIHelper.PeachBg;
            ApplyRoundedSprite(panelImage, 24);

            Shadow panelShadow = panelObj.AddComponent<Shadow>();
            panelShadow.effectColor = new Color(0.4f, 0.3f, 0.2f, 0.3f);
            panelShadow.effectDistance = new Vector2(4, -4);

            // Close button (top right)
            GameObject closeBtn = new GameObject("CloseButton");
            closeBtn.transform.SetParent(panelObj.transform, false);
            RectTransform closeBtnRect = closeBtn.AddComponent<RectTransform>();
            closeBtnRect.anchorMin = new Vector2(1f, 1f);
            closeBtnRect.anchorMax = new Vector2(1f, 1f);
            closeBtnRect.pivot = new Vector2(1f, 1f);
            closeBtnRect.anchoredPosition = new Vector2(-16, -16);
            closeBtnRect.sizeDelta = new Vector2(52, 52);

            Image closeBtnImage = closeBtn.AddComponent<Image>();
            closeBtnImage.color = new Color(0.95f, 0.5f, 0.5f, 1f);
            ApplyRoundedSprite(closeBtnImage, 12);

            Button closeBtnButton = closeBtn.AddComponent<Button>();

            GameObject closeBtnText = new GameObject("Text");
            closeBtnText.transform.SetParent(closeBtn.transform, false);
            RectTransform closeBtnTextRect = closeBtnText.AddComponent<RectTransform>();
            closeBtnTextRect.anchorMin = Vector2.zero;
            closeBtnTextRect.anchorMax = Vector2.one;
            closeBtnTextRect.offsetMin = Vector2.zero;
            closeBtnTextRect.offsetMax = Vector2.zero;

            Text closeBtnTextComponent = CreateText(closeBtnText, "X", 28, Color.white);
            closeBtnTextComponent.fontStyle = FontStyle.Bold;

            // Title
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(panelObj.transform, false);
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0, -24);
            titleRect.sizeDelta = new Vector2(580, 56);

            Text titleText = CreateText(titleObj, "\uD569\uC131 \uB808\uC2DC\uD53C", 42, new Color(0.7f, 0.5f, 0.15f));
            titleText.fontStyle = FontStyle.Bold;

            // Page number
            GameObject pageNumObj = new GameObject("PageNumber");
            pageNumObj.transform.SetParent(panelObj.transform, false);
            RectTransform pageNumRect = pageNumObj.AddComponent<RectTransform>();
            pageNumRect.anchorMin = new Vector2(0.5f, 1f);
            pageNumRect.anchorMax = new Vector2(0.5f, 1f);
            pageNumRect.pivot = new Vector2(0.5f, 1f);
            pageNumRect.anchoredPosition = new Vector2(0, -90);
            pageNumRect.sizeDelta = new Vector2(200, 30);

            Text pageNumText = CreateText(pageNumObj, "1 / 6", 24, CuteUIHelper.DarkText);

            // Source unit section (left side)
            GameObject sourceSection = CreateUnitDisplaySection(panelObj.transform, "Source", new Vector2(-200, -250), "\uC18C\uC2A4 \uC720\uB2DB");

            // Arrow
            GameObject arrowObj = new GameObject("Arrow");
            arrowObj.transform.SetParent(panelObj.transform, false);
            RectTransform arrowRect = arrowObj.AddComponent<RectTransform>();
            arrowRect.anchorMin = new Vector2(0.5f, 0.5f);
            arrowRect.anchorMax = new Vector2(0.5f, 0.5f);
            arrowRect.pivot = new Vector2(0.5f, 0.5f);
            arrowRect.anchoredPosition = new Vector2(0, -50);
            arrowRect.sizeDelta = new Vector2(100, 50);

            Text arrowText = CreateText(arrowObj, "\u2192", 50, new Color(0.9f, 0.6f, 0.2f));

            // Result unit section (right side)
            GameObject resultSection = CreateUnitDisplaySection(panelObj.transform, "Result", new Vector2(200, -250), "\uACB0\uACFC \uC720\uB2DB");

            // Required count text
            GameObject reqCountObj = new GameObject("RequiredCount");
            reqCountObj.transform.SetParent(panelObj.transform, false);
            RectTransform reqCountRect = reqCountObj.AddComponent<RectTransform>();
            reqCountRect.anchorMin = new Vector2(0.5f, 0);
            reqCountRect.anchorMax = new Vector2(0.5f, 0);
            reqCountRect.pivot = new Vector2(0.5f, 0);
            reqCountRect.anchoredPosition = new Vector2(0, 150);
            reqCountRect.sizeDelta = new Vector2(600, 40);

            Text reqCountText = CreateText(reqCountObj, "2\uAC1C \uD544\uC694", 28, new Color(0.8f, 0.55f, 0.15f));
            reqCountText.fontStyle = FontStyle.Bold;

            // Synthesis info text
            GameObject infoObj = new GameObject("SynthesisInfo");
            infoObj.transform.SetParent(panelObj.transform, false);
            RectTransform infoRect = infoObj.AddComponent<RectTransform>();
            infoRect.anchorMin = new Vector2(0.5f, 0);
            infoRect.anchorMax = new Vector2(0.5f, 0);
            infoRect.pivot = new Vector2(0.5f, 0);
            infoRect.anchoredPosition = new Vector2(0, 100);
            infoRect.sizeDelta = new Vector2(600, 60);

            Text infoText = CreateText(infoObj, "\uAC19\uC740 \uC720\uB2DB 2\uAC1C\uB97C \uBAA8\uC544\uC11C \uD569\uC131\uD558\uC138\uC694!", 20, CuteUIHelper.DarkText);

            // Previous page button (left arrow)
            GameObject prevBtn = CreatePageButton(panelObj.transform, "PrevButton", new Vector2(50, 400), "\u25C0");

            // Next page button (right arrow)
            GameObject nextBtn = CreatePageButton(panelObj.transform, "NextButton", new Vector2(650, 400), "\u25B6");

            SynthesisGuideUI guideUI = guideObj.AddComponent<SynthesisGuideUI>();

            var guidePanelField = typeof(SynthesisGuideUI).GetField("guidePanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var closeButtonField = typeof(SynthesisGuideUI).GetField("closeButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var prevPageButtonField = typeof(SynthesisGuideUI).GetField("prevPageButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var nextPageButtonField = typeof(SynthesisGuideUI).GetField("nextPageButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var pageNumberTextField = typeof(SynthesisGuideUI).GetField("pageNumberText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var recipeTitleTextField = typeof(SynthesisGuideUI).GetField("recipeTitleText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var sourceUnitIconField = typeof(SynthesisGuideUI).GetField("sourceUnitIcon", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var sourceUnitNameTextField = typeof(SynthesisGuideUI).GetField("sourceUnitNameText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var sourceUnitStatsTextField = typeof(SynthesisGuideUI).GetField("sourceUnitStatsText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var resultUnitIconField = typeof(SynthesisGuideUI).GetField("resultUnitIcon", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var resultUnitNameTextField = typeof(SynthesisGuideUI).GetField("resultUnitNameText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var resultUnitStatsTextField = typeof(SynthesisGuideUI).GetField("resultUnitStatsText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var requiredCountTextField = typeof(SynthesisGuideUI).GetField("requiredCountText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var synthesisInfoTextField = typeof(SynthesisGuideUI).GetField("synthesisInfoText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            guidePanelField?.SetValue(guideUI, guideObj);
            closeButtonField?.SetValue(guideUI, closeBtnButton);
            prevPageButtonField?.SetValue(guideUI, prevBtn.GetComponent<Button>());
            nextPageButtonField?.SetValue(guideUI, nextBtn.GetComponent<Button>());
            pageNumberTextField?.SetValue(guideUI, pageNumText);
            recipeTitleTextField?.SetValue(guideUI, titleText);

            var sourceIcon = sourceSection.transform.Find("Icon")?.GetComponent<Image>();
            var sourceName = sourceSection.transform.Find("Name")?.GetComponent<Text>();
            var sourceStats = sourceSection.transform.Find("Stats")?.GetComponent<Text>();
            sourceUnitIconField?.SetValue(guideUI, sourceIcon);
            sourceUnitNameTextField?.SetValue(guideUI, sourceName);
            sourceUnitStatsTextField?.SetValue(guideUI, sourceStats);

            var resultIcon = resultSection.transform.Find("Icon")?.GetComponent<Image>();
            var resultName = resultSection.transform.Find("Name")?.GetComponent<Text>();
            var resultStats = resultSection.transform.Find("Stats")?.GetComponent<Text>();
            resultUnitIconField?.SetValue(guideUI, resultIcon);
            resultUnitNameTextField?.SetValue(guideUI, resultName);
            resultUnitStatsTextField?.SetValue(guideUI, resultStats);

            requiredCountTextField?.SetValue(guideUI, reqCountText);
            synthesisInfoTextField?.SetValue(guideUI, infoText);

            guideObj.SetActive(false);
        }

        private GameObject CreateUnitDisplaySection(Transform parent, string name, Vector2 position, string label)
        {
            GameObject section = new GameObject(name + "Section");
            section.transform.SetParent(parent, false);

            RectTransform sectionRect = section.AddComponent<RectTransform>();
            sectionRect.anchorMin = new Vector2(0.5f, 0.5f);
            sectionRect.anchorMax = new Vector2(0.5f, 0.5f);
            sectionRect.pivot = new Vector2(0.5f, 0.5f);
            sectionRect.anchoredPosition = position;
            sectionRect.sizeDelta = new Vector2(250, 350);

            // Icon
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(section.transform, false);
            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 1f);
            iconRect.anchorMax = new Vector2(0.5f, 1f);
            iconRect.pivot = new Vector2(0.5f, 1f);
            iconRect.anchoredPosition = new Vector2(0, -10);
            iconRect.sizeDelta = new Vector2(120, 120);

            Image iconImage = iconObj.AddComponent<Image>();
            iconImage.color = Color.white;

            // Name
            GameObject nameObj = new GameObject("Name");
            nameObj.transform.SetParent(section.transform, false);
            RectTransform nameRect = nameObj.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.5f, 1f);
            nameRect.anchorMax = new Vector2(0.5f, 1f);
            nameRect.pivot = new Vector2(0.5f, 1f);
            nameRect.anchoredPosition = new Vector2(0, -145);
            nameRect.sizeDelta = new Vector2(240, 40);

            Text nameText = CreateText(nameObj, label, 22, CuteUIHelper.DarkText);
            nameText.fontStyle = FontStyle.Bold;

            // Stats
            GameObject statsObj = new GameObject("Stats");
            statsObj.transform.SetParent(section.transform, false);
            RectTransform statsRect = statsObj.AddComponent<RectTransform>();
            statsRect.anchorMin = new Vector2(0.5f, 1f);
            statsRect.anchorMax = new Vector2(0.5f, 1f);
            statsRect.pivot = new Vector2(0.5f, 1f);
            statsRect.anchoredPosition = new Vector2(0, -195);
            statsRect.sizeDelta = new Vector2(240, 140);

            Text statsText = CreateText(statsObj, "\uACF5\uACA9\uB825: ?\n\uACF5\uACA9\uC18D\uB3C4: ?\n\uC0AC\uAC70\uB9AC: ?\nDPS: ?", 18, CuteUIHelper.DarkText);

            return section;
        }

        private GameObject CreatePageButton(Transform parent, string name, Vector2 position, string text)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);

            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.anchorMin = Vector2.zero;
            btnRect.anchorMax = Vector2.zero;
            btnRect.pivot = new Vector2(0.5f, 0.5f);
            btnRect.anchoredPosition = position;
            btnRect.sizeDelta = new Vector2(80, 80);

            Image btnImage = btnObj.AddComponent<Image>();
            btnImage.color = new Color(0.8f, 0.75f, 0.85f, 1f);
            ApplyRoundedSprite(btnImage, 14);

            Button button = btnObj.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 0.98f, 0.95f, 1f);
            colors.pressedColor = new Color(0.85f, 0.82f, 0.78f, 1f);
            button.colors = colors;

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            Text btnText = CreateText(textObj, text, 36, CuteUIHelper.DarkText);
            btnText.fontStyle = FontStyle.Bold;

            return btnObj;
        }

        private void EnsureGameResultUI()
        {
            GameResultUI resultUI = FindFirstObjectByType<GameResultUI>();
            if (resultUI != null) return;

            GameObject resultObj = new GameObject("GameResultUI");
            resultObj.transform.SetParent(safeAreaRoot, false);

            RectTransform rect = resultObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Canvas overrideCanvas = resultObj.AddComponent<Canvas>();
            overrideCanvas.overrideSorting = true;
            overrideCanvas.sortingOrder = 200;
            resultObj.AddComponent<GraphicRaycaster>();

            CanvasGroup canvasGroup = resultObj.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            Image bgImage = resultObj.AddComponent<Image>();
            bgImage.color = new Color(0.3f, 0.2f, 0.15f, 0.75f);

            // Result panel
            GameObject panelObj = new GameObject("ResultPanel");
            panelObj.transform.SetParent(resultObj.transform, false);
            panelObj.SetActive(false);

            RectTransform panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = new Vector2(700, 580);

            Image panelImage = panelObj.AddComponent<Image>();
            panelImage.color = CuteUIHelper.PeachBg;
            ApplyRoundedSprite(panelImage, 24);

            Shadow panelShadow = panelObj.AddComponent<Shadow>();
            panelShadow.effectColor = new Color(0.4f, 0.3f, 0.2f, 0.3f);
            panelShadow.effectDistance = new Vector2(4, -4);

            // Title text
            GameObject titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(panelObj.transform, false);

            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0, -70);
            titleRect.sizeDelta = new Vector2(600, 90);

            Text titleText = CreateText(titleObj, "\uAC8C\uC784 \uC624\uBC84", 52, CuteUIHelper.DarkText);
            titleText.fontStyle = FontStyle.Bold;

            // Round text
            GameObject roundObj = new GameObject("RoundText");
            roundObj.transform.SetParent(panelObj.transform, false);

            RectTransform roundRect = roundObj.AddComponent<RectTransform>();
            roundRect.anchorMin = new Vector2(0.5f, 0.5f);
            roundRect.anchorMax = new Vector2(0.5f, 0.5f);
            roundRect.anchoredPosition = new Vector2(0, 50);
            roundRect.sizeDelta = new Vector2(500, 60);

            Text roundText = CreateText(roundObj, "\uB3C4\uB2EC \uB77C\uC6B4\uB4DC: 1", 36, new Color(0.4f, 0.35f, 0.3f));

            // Contribution text
            GameObject contribObj = new GameObject("ContributionText");
            contribObj.transform.SetParent(panelObj.transform, false);

            RectTransform contribRect = contribObj.AddComponent<RectTransform>();
            contribRect.anchorMin = new Vector2(0.5f, 0.5f);
            contribRect.anchorMax = new Vector2(0.5f, 0.5f);
            contribRect.anchoredPosition = new Vector2(0, -30);
            contribRect.sizeDelta = new Vector2(500, 60);

            Text contribText = CreateText(contribObj, "\uAE30\uC5EC\uB3C4: 0\uC810", 36, new Color(0.85f, 0.6f, 0.15f));

            // Reward text
            GameObject rewardObj = new GameObject("RewardText");
            rewardObj.transform.SetParent(panelObj.transform, false);

            RectTransform rewardRect = rewardObj.AddComponent<RectTransform>();
            rewardRect.anchorMin = new Vector2(0.5f, 0.5f);
            rewardRect.anchorMax = new Vector2(0.5f, 0.5f);
            rewardRect.anchoredPosition = new Vector2(0, -100);
            rewardRect.sizeDelta = new Vector2(500, 50);

            Text rewardText = CreateText(rewardObj, "", 32, new Color(0.3f, 0.75f, 0.4f));

            // Confirm button
            GameObject btnObj = new GameObject("ConfirmButton");
            btnObj.transform.SetParent(panelObj.transform, false);

            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.5f, 0f);
            btnRect.anchorMax = new Vector2(0.5f, 0f);
            btnRect.anchoredPosition = new Vector2(0, 70);
            btnRect.sizeDelta = new Vector2(400, 90);

            Image btnImage = btnObj.AddComponent<Image>();
            btnImage.color = new Color(0.45f, 0.82f, 0.55f);
            ApplyRoundedSprite(btnImage, 16);

            Shadow btnShadow = btnObj.AddComponent<Shadow>();
            btnShadow.effectColor = CuteUIHelper.SoftShadow;
            btnShadow.effectDistance = new Vector2(2, -3);

            Button button = btnObj.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 0.98f, 0.95f, 1f);
            colors.pressedColor = new Color(0.85f, 0.82f, 0.78f, 1f);
            colors.fadeDuration = 0.08f;
            button.colors = colors;

            GameObject btnTextObj = new GameObject("Text");
            btnTextObj.transform.SetParent(btnObj.transform, false);

            RectTransform btnTextRect = btnTextObj.AddComponent<RectTransform>();
            btnTextRect.anchorMin = Vector2.zero;
            btnTextRect.anchorMax = Vector2.one;
            btnTextRect.offsetMin = new Vector2(8, 4);
            btnTextRect.offsetMax = new Vector2(-8, -4);

            Text btnText = CreateText(btnTextObj, "\uD655\uC778", 38, CuteUIHelper.DarkText);
            btnText.fontStyle = FontStyle.Bold;

            GameResultUI resultUIComponent = resultObj.AddComponent<GameResultUI>();

            SetField(resultUIComponent, "canvasGroup", canvasGroup);
            SetField(resultUIComponent, "resultPanel", panelObj);
            SetField(resultUIComponent, "titleText", titleText);
            SetField(resultUIComponent, "roundText", roundText);
            SetField(resultUIComponent, "contributionText", contribText);
            SetField(resultUIComponent, "confirmButton", button);
            SetField(resultUIComponent, "confirmButtonText", btnText);
            SetField(resultUIComponent, "rewardText", rewardText);

        }
        #endregion

        #region Quest System
        private void EnsureQuestManager()
        {
            if (FindFirstObjectByType<QuestManager>() == null)
                new GameObject("QuestManager").AddComponent<QuestManager>();

            QuestManager.Instance?.InitializeQuests();
        }

        private void EnsureQuestUI()
        {
            QuestUI existingUI = FindFirstObjectByType<QuestUI>();
            if (existingUI != null) return;

            GameObject questObj = new GameObject("QuestUI");
            questObj.transform.SetParent(safeAreaRoot, false);

            RectTransform rect = questObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Canvas overrideCanvas = questObj.AddComponent<Canvas>();
            overrideCanvas.overrideSorting = true;
            overrideCanvas.sortingOrder = 200;
            questObj.AddComponent<GraphicRaycaster>();

            Image bgImage = questObj.AddComponent<Image>();
            bgImage.color = CuteUIHelper.WarmOverlay;

            // Main panel (centered)
            GameObject panelObj = new GameObject("Panel");
            panelObj.transform.SetParent(questObj.transform, false);

            RectTransform panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = new Vector2(700, 850);

            Image panelImage = panelObj.AddComponent<Image>();
            panelImage.color = GameSceneDesignTokens.QuestPanelBg;
            ApplyRoundedSprite(panelImage, 24);

            Shadow panelShadow = panelObj.AddComponent<Shadow>();
            panelShadow.effectColor = new Color(0.4f, 0.3f, 0.2f, 0.3f);
            panelShadow.effectDistance = new Vector2(4, -4);

            // Close button (top right) - 52x52 to match utility buttons
            GameObject closeBtn = new GameObject("CloseButton");
            closeBtn.transform.SetParent(panelObj.transform, false);
            RectTransform closeBtnRect = closeBtn.AddComponent<RectTransform>();
            closeBtnRect.anchorMin = new Vector2(1f, 1f);
            closeBtnRect.anchorMax = new Vector2(1f, 1f);
            closeBtnRect.pivot = new Vector2(1f, 1f);
            closeBtnRect.anchoredPosition = new Vector2(-16, -16);
            closeBtnRect.sizeDelta = new Vector2(52, 52);

            Image closeBtnImage = closeBtn.AddComponent<Image>();
            closeBtnImage.color = new Color(0.95f, 0.5f, 0.5f, 1f);
            ApplyRoundedSprite(closeBtnImage, 12);

            Button closeBtnButton = closeBtn.AddComponent<Button>();

            GameObject closeBtnText = new GameObject("Text");
            closeBtnText.transform.SetParent(closeBtn.transform, false);
            RectTransform closeBtnTextRect = closeBtnText.AddComponent<RectTransform>();
            closeBtnTextRect.anchorMin = Vector2.zero;
            closeBtnTextRect.anchorMax = Vector2.one;
            closeBtnTextRect.offsetMin = Vector2.zero;
            closeBtnTextRect.offsetMax = Vector2.zero;

            Text closeBtnTextComponent = CreateText(closeBtnText, "X", 28, Color.white);
            closeBtnTextComponent.fontStyle = FontStyle.Bold;

            // Title
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(panelObj.transform, false);
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0, -24);
            titleRect.sizeDelta = new Vector2(580, 56);

            Text titleText = CreateText(titleObj, "\uD788\uB4E0 \uD038\uC2A4\uD2B8", 42, new Color(0.7f, 0.5f, 0.15f));
            titleText.fontStyle = FontStyle.Bold;

            // Scroll content area
            GameObject contentArea = new GameObject("ContentArea");
            contentArea.transform.SetParent(panelObj.transform, false);
            RectTransform contentRect = contentArea.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 0);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.offsetMin = new Vector2(20, 20);
            contentRect.offsetMax = new Vector2(-20, -100);

            VerticalLayoutGroup vlg = contentArea.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 10;
            vlg.padding = new RectOffset(10, 10, 10, 10);
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childAlignment = TextAnchor.UpperCenter;

            ContentSizeFitter csf = contentArea.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            QuestUI questUI = questObj.AddComponent<QuestUI>();
            SetField(questUI, "questPanel", questObj);
            SetField(questUI, "closeButton", closeBtnButton);
            SetField(questUI, "contentParent", contentArea.transform);

            questObj.SetActive(false);
        }

        private void EnsureQuestButton()
        {
            EnsureQuestManager();
            EnsureQuestUI();

            GameObject btnObj = new GameObject("QuestButton");
            btnObj.transform.SetParent(safeAreaRoot, false);

            float btnSize = GameSceneDesignTokens.UtilityButtonSize;
            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(1, 1);
            btnRect.anchorMax = new Vector2(1, 1);
            btnRect.pivot = new Vector2(1, 1);
            btnRect.anchoredPosition = new Vector2(-12, -(GameSceneDesignTokens.HudHeight + 8 + btnSize + 8));
            btnRect.sizeDelta = new Vector2(btnSize, btnSize);

            Image btnImage = btnObj.AddComponent<Image>();
            btnImage.color = GameSceneDesignTokens.QuestBtnBg;
            ApplyRoundedSprite(btnImage, 14);

            Shadow btnShadow = btnObj.AddComponent<Shadow>();
            btnShadow.effectColor = CuteUIHelper.SoftShadow;
            btnShadow.effectDistance = new Vector2(2, -2);

            Button button = btnObj.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 0.98f, 0.95f, 1f);
            colors.pressedColor = new Color(0.85f, 0.82f, 0.78f, 1f);
            colors.fadeDuration = 0.1f;
            button.colors = colors;

            button.onClick.AddListener(() => {
                QuestUI questUI = FindFirstObjectByType<QuestUI>(FindObjectsInactive.Include);
                if (questUI != null)
                {
                    questUI.Show();
                }
            });

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            Text btnText = CreateText(textObj, "Q", 28, CuteUIHelper.DarkText);
            btnText.fontStyle = FontStyle.Bold;
        }
        #endregion

        #region Game Speed Button
        private void EnsureGameSpeedButton()
        {
            if (FindFirstObjectByType<GameSpeedButton>() != null) return;

            // Ensure controller exists
            if (GameSpeedController.Instance == null)
            {
                new GameObject("GameSpeedController").AddComponent<GameSpeedController>();
            }

            GameObject speedObj = new GameObject("GameSpeedButton");
            speedObj.transform.SetParent(safeAreaRoot, false);
            speedObj.AddComponent<GameSpeedButton>();
        }
        #endregion
    }
}
