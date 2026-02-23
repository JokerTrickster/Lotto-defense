using UnityEngine;
using UnityEngine.UI;
using LottoDefense.UI;
using LottoDefense.Units;
using LottoDefense.Quests;
using LottoDefense.Networking;

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

        private Text CreateInfoStatText(Transform parent, string placeholder)
        {
            GameObject obj = new GameObject($"Info_{placeholder}");
            obj.transform.SetParent(parent, false);

            Text t = CreateText(obj, placeholder, GameSceneDesignTokens.UnitInfoDetailSize, CuteUIHelper.DarkText);
            t.alignment = TextAnchor.MiddleLeft;
            t.fontStyle = FontStyle.Bold;
            t.supportRichText = true;
            t.resizeTextForBestFit = true;
            t.resizeTextMinSize = 9;
            t.resizeTextMaxSize = GameSceneDesignTokens.UnitInfoDetailSize;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Truncate;

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

            HorizontalLayoutGroup hLayout = bottomPanelObj.AddComponent<HorizontalLayoutGroup>();
            hLayout.padding = new RectOffset(8, 8, 6, 6);
            hLayout.spacing = 8;
            hLayout.childControlWidth = true;
            hLayout.childControlHeight = true;
            hLayout.childForceExpandWidth = true;
            hLayout.childForceExpandHeight = true;

            // ========== LEFT COLUMN (35%): Unit Info ==========
            GameObject leftCol = new GameObject("LeftColumn");
            leftCol.transform.SetParent(bottomPanelObj.transform, false);

            LayoutElement leftLE = leftCol.AddComponent<LayoutElement>();
            leftLE.flexibleWidth = 0.35f;

            Image leftBg = leftCol.AddComponent<Image>();
            leftBg.color = new Color(0.96f, 0.93f, 0.88f, 0.7f);
            leftBg.raycastTarget = false;
            ApplyRoundedSprite(leftBg, 12);

            GameObject statsContainer = new GameObject("StatsContainer");
            statsContainer.transform.SetParent(leftCol.transform, false);
            RectTransform statsContainerRect = statsContainer.AddComponent<RectTransform>();
            statsContainerRect.anchorMin = Vector2.zero;
            statsContainerRect.anchorMax = Vector2.one;
            statsContainerRect.offsetMin = new Vector2(6, 4);
            statsContainerRect.offsetMax = new Vector2(-6, -4);

            HorizontalLayoutGroup statsHLayout = statsContainer.AddComponent<HorizontalLayoutGroup>();
            statsHLayout.spacing = 8;
            statsHLayout.padding = new RectOffset(4, 4, 2, 2);
            statsHLayout.childControlWidth = false;
            statsHLayout.childControlHeight = true;
            statsHLayout.childForceExpandWidth = false;
            statsHLayout.childForceExpandHeight = true;

            // Portrait
            GameObject portraitObj = new GameObject("Portrait");
            portraitObj.transform.SetParent(statsContainer.transform, false);
            LayoutElement portraitLE = portraitObj.AddComponent<LayoutElement>();
            portraitLE.preferredWidth = 70;
            portraitLE.preferredHeight = 70;

            Image portraitBg = portraitObj.AddComponent<Image>();
            portraitBg.color = new Color(0.92f, 0.88f, 0.82f, 0.85f);
            portraitBg.raycastTarget = false;
            ApplyRoundedSprite(portraitBg, 10);

            Outline portraitOutline = portraitObj.AddComponent<Outline>();
            portraitOutline.effectColor = CuteUIHelper.WarmBorder;
            portraitOutline.effectDistance = new Vector2(1, -1);

            GameObject portraitCircleObj = new GameObject("PortraitCircle");
            portraitCircleObj.transform.SetParent(portraitObj.transform, false);
            RectTransform circleRect = portraitCircleObj.AddComponent<RectTransform>();
            circleRect.anchorMin = new Vector2(0.1f, 0.1f);
            circleRect.anchorMax = new Vector2(0.9f, 0.9f);
            circleRect.offsetMin = Vector2.zero;
            circleRect.offsetMax = Vector2.zero;

            Image portraitImage = portraitCircleObj.AddComponent<Image>();
            portraitImage.sprite = UnitData.CreateCircleSprite(64);
            portraitImage.color = Color.white;
            portraitImage.raycastTarget = false;

            // Stats area (right of portrait)
            GameObject statsAreaObj = new GameObject("StatsArea");
            statsAreaObj.transform.SetParent(statsContainer.transform, false);
            LayoutElement statsLE = statsAreaObj.AddComponent<LayoutElement>();
            statsLE.flexibleWidth = 1f;

            VerticalLayoutGroup statsVLayout = statsAreaObj.AddComponent<VerticalLayoutGroup>();
            statsVLayout.spacing = 3f;
            statsVLayout.childControlWidth = true;
            statsVLayout.childControlHeight = true;
            statsVLayout.childForceExpandWidth = true;
            statsVLayout.childForceExpandHeight = false;

            // Name row
            GameObject nameRowObj = new GameObject("InfoNameRow");
            nameRowObj.transform.SetParent(statsAreaObj.transform, false);
            LayoutElement nameRowLE = nameRowObj.AddComponent<LayoutElement>();
            nameRowLE.preferredHeight = 24f;

            Text infoNameText = CreateText(nameRowObj, "", GameSceneDesignTokens.UnitInfoNameSize, CuteUIHelper.DarkText);
            infoNameText.alignment = TextAnchor.MiddleLeft;
            infoNameText.fontStyle = FontStyle.Bold;
            infoNameText.horizontalOverflow = HorizontalWrapMode.Wrap;
            infoNameText.verticalOverflow = VerticalWrapMode.Truncate;
            infoNameText.resizeTextForBestFit = true;
            infoNameText.resizeTextMinSize = 12;
            infoNameText.resizeTextMaxSize = GameSceneDesignTokens.UnitInfoNameSize;

            // ATK | SPD | RNG
            GameObject statsRow1Obj = new GameObject("StatsRow1");
            statsRow1Obj.transform.SetParent(statsAreaObj.transform, false);
            LayoutElement statsRow1LE = statsRow1Obj.AddComponent<LayoutElement>();
            statsRow1LE.preferredHeight = 20f;

            HorizontalLayoutGroup statsRow1H = statsRow1Obj.AddComponent<HorizontalLayoutGroup>();
            statsRow1H.spacing = 6f;
            statsRow1H.childControlWidth = true;
            statsRow1H.childControlHeight = true;
            statsRow1H.childForceExpandWidth = true;
            statsRow1H.childForceExpandHeight = true;

            Text infoAtkText = CreateInfoStatText(statsRow1Obj.transform, "ATK");
            infoAtkText.color = GameSceneDesignTokens.UnitInfoAttack;
            Text infoSpdText = CreateInfoStatText(statsRow1Obj.transform, "SPD");
            infoSpdText.color = GameSceneDesignTokens.UnitInfoSpeed;
            Text infoRngText = CreateInfoStatText(statsRow1Obj.transform, "RNG");
            infoRngText.color = GameSceneDesignTokens.UnitInfoRange;

            // TYPE | DEF
            GameObject statsRow2Obj = new GameObject("StatsRow2");
            statsRow2Obj.transform.SetParent(statsAreaObj.transform, false);
            LayoutElement statsRow2LE = statsRow2Obj.AddComponent<LayoutElement>();
            statsRow2LE.preferredHeight = 20f;

            HorizontalLayoutGroup statsRow2H = statsRow2Obj.AddComponent<HorizontalLayoutGroup>();
            statsRow2H.spacing = 6f;
            statsRow2H.childControlWidth = true;
            statsRow2H.childControlHeight = true;
            statsRow2H.childForceExpandWidth = true;
            statsRow2H.childForceExpandHeight = true;

            Text infoPatternText = CreateInfoStatText(statsRow2Obj.transform, "TYPE");
            infoPatternText.color = GameSceneDesignTokens.UnitInfoPatternColor;
            Text infoDefText = CreateInfoStatText(statsRow2Obj.transform, "DEF");
            infoDefText.color = GameSceneDesignTokens.UnitInfoDefense;

            // SKILL + Mana bar
            GameObject skillRowObj = new GameObject("SkillRow");
            skillRowObj.transform.SetParent(statsAreaObj.transform, false);
            LayoutElement skillRowLE = skillRowObj.AddComponent<LayoutElement>();
            skillRowLE.preferredHeight = 22f;

            HorizontalLayoutGroup skillRowH = skillRowObj.AddComponent<HorizontalLayoutGroup>();
            skillRowH.spacing = 4f;
            skillRowH.childControlWidth = false;
            skillRowH.childControlHeight = true;
            skillRowH.childForceExpandWidth = false;
            skillRowH.childForceExpandHeight = true;

            GameObject skillTextObj = new GameObject("SkillText");
            skillTextObj.transform.SetParent(skillRowObj.transform, false);
            LayoutElement skillTextLE = skillTextObj.AddComponent<LayoutElement>();
            skillTextLE.preferredWidth = 120f;

            Text infoSkillText = CreateText(skillTextObj, "", 13, GameSceneDesignTokens.UnitInfoSkillColor);
            infoSkillText.alignment = TextAnchor.MiddleLeft;
            infoSkillText.horizontalOverflow = HorizontalWrapMode.Wrap;
            infoSkillText.verticalOverflow = VerticalWrapMode.Truncate;
            infoSkillText.resizeTextForBestFit = true;
            infoSkillText.resizeTextMinSize = 10;
            infoSkillText.resizeTextMaxSize = 13;
            infoSkillText.supportRichText = true;

            GameObject manaContainerObj = new GameObject("ManaBarContainer");
            manaContainerObj.transform.SetParent(skillRowObj.transform, false);
            LayoutElement manaContainerLE = manaContainerObj.AddComponent<LayoutElement>();
            manaContainerLE.preferredWidth = 80f;
            manaContainerLE.preferredHeight = 14f;

            Image manaBarBgImg = manaContainerObj.AddComponent<Image>();
            manaBarBgImg.color = GameSceneDesignTokens.ManaBarBg;
            manaBarBgImg.raycastTarget = false;
            ApplyRoundedSprite(manaBarBgImg, 6);

            Outline manaBarOutline = manaContainerObj.AddComponent<Outline>();
            manaBarOutline.effectColor = CuteUIHelper.WarmBorder;
            manaBarOutline.effectDistance = new Vector2(1, -1);

            GameObject manaFillObj = new GameObject("ManaBarFill");
            manaFillObj.transform.SetParent(manaContainerObj.transform, false);
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

            // --- emptyStateText: visible when no unit selected ---
            GameObject emptyStateObj = new GameObject("EmptyStateText");
            emptyStateObj.transform.SetParent(leftCol.transform, false);
            RectTransform emptyRect = emptyStateObj.AddComponent<RectTransform>();
            emptyRect.anchorMin = Vector2.zero;
            emptyRect.anchorMax = Vector2.one;
            emptyRect.offsetMin = Vector2.zero;
            emptyRect.offsetMax = Vector2.zero;

            Text emptyText = CreateText(emptyStateObj, "\uC720\uB2DB\uC744 \uC120\uD0DD\uD558\uC138\uC694", 16, new Color(0.6f, 0.55f, 0.5f, 0.8f));
            emptyText.alignment = TextAnchor.MiddleCenter;

            UnitInfoPanel infoPanel = leftCol.AddComponent<UnitInfoPanel>();
            SetField(infoPanel, "portraitImage", portraitImage);
            SetField(infoPanel, "unitNameText", infoNameText);
            SetField(infoPanel, "attackText", infoAtkText);
            SetField(infoPanel, "speedText", infoSpdText);
            SetField(infoPanel, "rangeText", infoRngText);
            SetField(infoPanel, "patternText", infoPatternText);
            SetField(infoPanel, "defenseText", infoDefText);
            SetField(infoPanel, "skillText", infoSkillText);
            SetField(infoPanel, "manaBarFill", manaFillImg);
            SetField(infoPanel, "manaBarContainer", manaContainerObj);
            SetField(infoPanel, "statsContainer", statsContainer);
            SetField(infoPanel, "emptyStateText", emptyText);

            statsContainer.SetActive(false);
            emptyStateObj.SetActive(true);

            // ========== RIGHT COLUMN (65%): 3x2 Button Grid ==========
            GameObject rightCol = new GameObject("RightColumn");
            rightCol.transform.SetParent(bottomPanelObj.transform, false);

            LayoutElement rightLE = rightCol.AddComponent<LayoutElement>();
            rightLE.flexibleWidth = 0.65f;

            VerticalLayoutGroup rightVLayout = rightCol.AddComponent<VerticalLayoutGroup>();
            rightVLayout.spacing = 8;
            rightVLayout.padding = new RectOffset(4, 4, 6, 6);
            rightVLayout.childControlWidth = true;
            rightVLayout.childControlHeight = true;
            rightVLayout.childForceExpandWidth = true;
            rightVLayout.childForceExpandHeight = true;

            // Row 1: Summon | Auto Synth | Sell
            GameObject row1 = new GameObject("ButtonRow1");
            row1.transform.SetParent(rightCol.transform, false);
            HorizontalLayoutGroup row1H = row1.AddComponent<HorizontalLayoutGroup>();
            row1H.spacing = 8;
            row1H.childControlWidth = true;
            row1H.childControlHeight = true;
            row1H.childForceExpandWidth = true;
            row1H.childForceExpandHeight = true;

            GameObject summonBtnObj = CreateCommandButton(row1.transform, "SummonButton",
                "\uC18C\uD658 5G", GameSceneDesignTokens.SummonButtonBg, GameSceneDesignTokens.SummonButtonBorder);
            Button summonBtn = summonBtnObj.GetComponent<Button>();
            Text summonBtnText = summonBtnObj.GetComponentInChildren<Text>();
            summonBtnText.supportRichText = true;

            GameObject autoSynthBtnObj = CreateCommandButton(row1.transform, "AutoSynthButton",
                "\uC790\uB3D9\uC870\uD569(0)", GameSceneDesignTokens.AutoSynthBtnBg, GameSceneDesignTokens.AutoSynthBtnBorder);
            Button autoSynthBtn = autoSynthBtnObj.GetComponent<Button>();
            Text autoSynthBtnText = autoSynthBtnObj.GetComponentInChildren<Text>();

            GameObject sellBtnObj = CreateCommandButton(row1.transform, "SellButton",
                "\uD310\uB9E4", GameSceneDesignTokens.SellBtnBg, GameSceneDesignTokens.SellBtnBorder);
            Button sellBtn = sellBtnObj.GetComponent<Button>();
            Text sellBtnText = sellBtnObj.GetComponentInChildren<Text>();

            // Row 2: Attack UP | Speed UP | Synthesis
            GameObject row2 = new GameObject("ButtonRow2");
            row2.transform.SetParent(rightCol.transform, false);
            HorizontalLayoutGroup row2H = row2.AddComponent<HorizontalLayoutGroup>();
            row2H.spacing = 8;
            row2H.childControlWidth = true;
            row2H.childControlHeight = true;
            row2H.childForceExpandWidth = true;
            row2H.childForceExpandHeight = true;

            GameObject atkUpBtnObj = CreateCommandButton(row2.transform, "AttackUpgradeButton",
                "\uACF5\uACA9\u2191", GameSceneDesignTokens.AttackUpBtnBg, GameSceneDesignTokens.AttackUpBtnBorder);
            Button atkUpBtn = atkUpBtnObj.GetComponent<Button>();
            Text atkUpBtnText = atkUpBtnObj.GetComponentInChildren<Text>();

            GameObject spdUpBtnObj = CreateCommandButton(row2.transform, "AttackSpeedUpgradeButton",
                "\uACF5\uC18D\u2191", GameSceneDesignTokens.SpeedUpBtnBg, GameSceneDesignTokens.SpeedUpBtnBorder);
            Button spdUpBtn = spdUpBtnObj.GetComponent<Button>();
            Text spdUpBtnText = spdUpBtnObj.GetComponentInChildren<Text>();

            GameObject synthBtnObj = CreateCommandButton(row2.transform, "SynthesisButton",
                "\uC870\uD569", GameSceneDesignTokens.SynthFloatBtnBg, GameSceneDesignTokens.SynthFloatBtnBorder);
            Button synthBtn = synthBtnObj.GetComponent<Button>();
            Text synthBtnText = synthBtnObj.GetComponentInChildren<Text>();
            synthBtnText.color = GameSceneDesignTokens.SynthFloatBtnText;

            // ========== Wire GameBottomUI component ==========
            GameBottomUI component = bottomPanelObj.AddComponent<GameBottomUI>();
            SetField(component, "panel", bottomPanelObj);
            SetField(component, "summonButton", summonBtn);
            SetField(component, "autoSynthesisButton", autoSynthBtn);
            SetField(component, "sellButton", sellBtn);
            SetField(component, "attackUpgradeButton", atkUpBtn);
            SetField(component, "attackSpeedUpgradeButton", spdUpBtn);
            SetField(component, "synthesisButton", synthBtn);
            SetField(component, "summonButtonText", summonBtnText);
            SetField(component, "autoSynthesisButtonText", autoSynthBtnText);
            SetField(component, "sellButtonText", sellBtnText);
            SetField(component, "attackUpgradeButtonText", atkUpBtnText);
            SetField(component, "attackSpeedUpgradeButtonText", spdUpBtnText);
            SetField(component, "synthesisButtonText", synthBtnText);

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
            textRect.offsetMin = new Vector2(6, 4);
            textRect.offsetMax = new Vector2(-6, -4);

            Text buttonText = CreateText(textObj, text, textSize, CuteUIHelper.DarkText);
            buttonText.alignment = TextAnchor.MiddleCenter;
            buttonText.fontStyle = FontStyle.Bold;
            buttonText.resizeTextForBestFit = true;
            buttonText.resizeTextMinSize = 12;
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

            // ---- Row 2: LIFE | GOLD | MONSTERS | UNITS ----
            GameObject row2 = CreateHUDRow(hudObj.transform, "Row_Stats", 52);
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
            labelText.fontStyle = FontStyle.Normal;
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

        #region Opponent Status UI (Multiplayer)
        private void EnsureOpponentStatusUI()
        {
            if (MultiplayerManager.Instance == null || !MultiplayerManager.Instance.IsMultiplayer)
                return;

            if (FindFirstObjectByType<OpponentStatusUI>() != null)
                return;

            OpponentStatusUI.CreateOnCanvas(mainCanvas, defaultFont);
        }
        #endregion
    }
}
