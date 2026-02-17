using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using LottoDefense.Lobby;
using LottoDefense.Gameplay;
using LottoDefense.Units;

namespace LottoDefense.UI
{
    public class UnitShopUI : MonoBehaviour
    {
        private Canvas canvas;
        private Font font;
        private GameObject overlay;
        private GameObject contentArea;
        private GameObject detailPopup;

        private UnitShopManager shopManager;
        private GameBalanceConfig balanceConfig;
        private List<GameObject> cardObjects = new List<GameObject>();

        public static UnitShopUI Create(Canvas parentCanvas, Font font)
        {
            GameObject obj = new GameObject("UnitShopUI");
            obj.transform.SetParent(parentCanvas.transform, false);
            UnitShopUI ui = obj.AddComponent<UnitShopUI>();
            ui.canvas = parentCanvas;
            ui.font = font;
            ui.shopManager = FindFirstObjectByType<UnitShopManager>();
            ui.balanceConfig = Resources.Load<GameBalanceConfig>("GameBalanceConfig");
            if (ui.balanceConfig == null)
            {
                ui.balanceConfig = ScriptableObject.CreateInstance<GameBalanceConfig>();
            }
            ui.BuildUI();
            return ui;
        }

        private void BuildUI()
        {
            GameObject panel = MainGameBootstrapper.CreateModalPopup(
                canvas, font, "상점", 0.9f, 0.75f, out overlay, out Button closeBtn);
            closeBtn.onClick.AddListener(Hide);

            // Clicking overlay background closes the popup
            Button overlayBtn = overlay.AddComponent<Button>();
            overlayBtn.onClick.AddListener(Hide);

            // Content area (scrollable card grid)
            contentArea = new GameObject("Content");
            contentArea.transform.SetParent(panel.transform, false);

            RectTransform contentRect = contentArea.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0.05f, 0.05f);
            contentRect.anchorMax = new Vector2(0.95f, 0.85f);
            contentRect.sizeDelta = Vector2.zero;

            GridLayoutGroup grid = contentArea.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(420, 200);
            grid.spacing = new Vector2(20, 20);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 2;
            grid.childAlignment = TextAnchor.UpperCenter;
            grid.padding = new RectOffset(10, 10, 10, 10);

            PopulateCards();
        }

        private void PopulateCards()
        {
            // Clear existing
            foreach (var card in cardObjects)
                if (card != null) Destroy(card);
            cardObjects.Clear();

            if (shopManager == null || balanceConfig == null) return;

            foreach (var shopItem in shopManager.GetAllShopItems())
            {
                CreateUnitCard(shopItem);
            }
        }

        private void CreateUnitCard(GameBalanceConfig.UnitShopPrice shopItem)
        {
            bool unlocked = shopManager.IsUnlocked(shopItem.unitName);

            GameObject card = new GameObject($"Card_{shopItem.unitName}");
            card.transform.SetParent(contentArea.transform, false);
            cardObjects.Add(card);

            Image cardBg = card.AddComponent<Image>();
            cardBg.color = unlocked ? LobbyDesignTokens.CardBg : LobbyDesignTokens.CardBgLocked;

            Button cardBtn = card.AddComponent<Button>();
            string capturedName = shopItem.unitName;
            cardBtn.onClick.AddListener(() => ShowUnitDetail(capturedName));

            // Unit icon (circle placeholder)
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(card.transform, false);
            Image iconImg = iconObj.AddComponent<Image>();

            // Get rarity color
            var unitBalance = balanceConfig.units.Find(u => u.unitName == shopItem.unitName);
            Color rarityColor = GetRarityColor(unitBalance?.rarity ?? Rarity.Normal);
            iconImg.color = unlocked ? rarityColor : new Color(0.3f, 0.3f, 0.3f, 1f);

            RectTransform iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.05f, 0.2f);
            iconRect.anchorMax = new Vector2(0.35f, 0.8f);
            iconRect.sizeDelta = Vector2.zero;

            // Unit name
            GameObject nameObj = new GameObject("Name");
            nameObj.transform.SetParent(card.transform, false);
            Text nameText = CreateText(nameObj, shopItem.unitName, LobbyDesignTokens.BodySize, LobbyDesignTokens.TextPrimary);
            nameText.alignment = TextAnchor.MiddleLeft;

            RectTransform nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.4f, 0.55f);
            nameRect.anchorMax = new Vector2(0.95f, 0.85f);
            nameRect.sizeDelta = Vector2.zero;

            // Status text
            GameObject statusObj = new GameObject("Status");
            statusObj.transform.SetParent(card.transform, false);

            string statusStr;
            Color statusColor;
            if (unlocked)
            {
                statusStr = "보유중";
                statusColor = LobbyDesignTokens.TextSuccess;
            }
            else if (shopItem.goldCost == 0)
            {
                statusStr = "무료";
                statusColor = LobbyDesignTokens.TextSuccess;
            }
            else
            {
                statusStr = $"{shopItem.goldCost}G";
                statusColor = LobbyDesignTokens.GoldColor;
            }

            Text statusText = CreateText(statusObj, statusStr, LobbyDesignTokens.SmallSize, statusColor);
            statusText.alignment = TextAnchor.MiddleLeft;

            RectTransform statusRect = statusObj.GetComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(0.4f, 0.15f);
            statusRect.anchorMax = new Vector2(0.95f, 0.5f);
            statusRect.sizeDelta = Vector2.zero;

            // Rarity label
            if (unitBalance != null)
            {
                GameObject rarityObj = new GameObject("Rarity");
                rarityObj.transform.SetParent(card.transform, false);
                string rarityStr = unitBalance.rarity.ToString();
                Text rarityText = CreateText(rarityObj, rarityStr, LobbyDesignTokens.SmallSize - 4, rarityColor);
                rarityText.alignment = TextAnchor.UpperRight;

                RectTransform rarityRect = rarityObj.GetComponent<RectTransform>();
                rarityRect.anchorMin = new Vector2(0.6f, 0.7f);
                rarityRect.anchorMax = new Vector2(0.95f, 0.95f);
                rarityRect.sizeDelta = Vector2.zero;
            }
        }

        private void ShowUnitDetail(string unitName)
        {
            if (detailPopup != null) Destroy(detailPopup);

            var unitBalance = balanceConfig?.units.Find(u => u.unitName == unitName);
            if (unitBalance == null) return;

            bool unlocked = shopManager.IsUnlocked(unitName);
            int cost = shopManager.GetUnlockPrice(unitName);

            // Detail overlay
            detailPopup = new GameObject("DetailPopup");
            detailPopup.transform.SetParent(overlay.transform, false);
            Image detailOverlay = detailPopup.AddComponent<Image>();
            detailOverlay.color = new Color(0f, 0f, 0f, 0.5f);

            RectTransform detailRect = detailPopup.GetComponent<RectTransform>();
            detailRect.anchorMin = Vector2.zero;
            detailRect.anchorMax = Vector2.one;
            detailRect.sizeDelta = Vector2.zero;

            // Detail panel
            GameObject detailPanel = new GameObject("DetailPanel");
            detailPanel.transform.SetParent(detailPopup.transform, false);
            Image panelBg = detailPanel.AddComponent<Image>();
            panelBg.color = LobbyDesignTokens.ModalPanelBg;

            RectTransform panelRect = detailPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.1f, 0.15f);
            panelRect.anchorMax = new Vector2(0.9f, 0.85f);
            panelRect.sizeDelta = Vector2.zero;

            // Close detail button
            GameObject closeObj = new GameObject("CloseDetail");
            closeObj.transform.SetParent(detailPanel.transform, false);
            Image closeBg = closeObj.AddComponent<Image>();
            closeBg.color = LobbyDesignTokens.ButtonClose;
            Button closeBtn = closeObj.AddComponent<Button>();
            closeBtn.onClick.AddListener(() => { if (detailPopup != null) Destroy(detailPopup); });

            RectTransform closeRect = closeObj.GetComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(1f, 1f);
            closeRect.anchorMax = new Vector2(1f, 1f);
            closeRect.pivot = new Vector2(1f, 1f);
            closeRect.anchoredPosition = new Vector2(-10f, -10f);
            closeRect.sizeDelta = new Vector2(50f, 50f);

            GameObject closeTextObj = new GameObject("X");
            closeTextObj.transform.SetParent(closeObj.transform, false);
            Text closeText = CreateText(closeTextObj, "X", LobbyDesignTokens.BodySize, Color.white);
            RectTransform ctRect = closeTextObj.GetComponent<RectTransform>();
            ctRect.anchorMin = Vector2.zero;
            ctRect.anchorMax = Vector2.one;
            ctRect.sizeDelta = Vector2.zero;

            // Unit name + rarity
            Color rarityColor = GetRarityColor(unitBalance.rarity);

            GameObject headerObj = new GameObject("Header");
            headerObj.transform.SetParent(detailPanel.transform, false);
            Text header = CreateText(headerObj, $"{unitName}", LobbyDesignTokens.SubHeaderSize, LobbyDesignTokens.TextPrimary);
            header.fontStyle = FontStyle.Bold;

            RectTransform headerRect = headerObj.GetComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0.05f, 0.85f);
            headerRect.anchorMax = new Vector2(0.85f, 0.95f);
            headerRect.sizeDelta = Vector2.zero;

            // Rarity
            GameObject rarityLabelObj = new GameObject("RarityLabel");
            rarityLabelObj.transform.SetParent(detailPanel.transform, false);
            Text rarityLabel = CreateText(rarityLabelObj, unitBalance.rarity.ToString(), LobbyDesignTokens.BodySize, rarityColor);

            RectTransform rlRect = rarityLabelObj.GetComponent<RectTransform>();
            rlRect.anchorMin = new Vector2(0.05f, 0.78f);
            rlRect.anchorMax = new Vector2(0.95f, 0.85f);
            rlRect.sizeDelta = Vector2.zero;

            // Stats
            string statsStr = $"공격력: {unitBalance.attack}\n공속: {unitBalance.attackSpeed:F1} /초\n사거리: {unitBalance.attackRange:F1}";

            // Add skill info
            if (balanceConfig != null && unitBalance.skillIds != null && unitBalance.skillIds.Count > 0)
            {
                statsStr += "\n\n스킬:";
                foreach (string skillId in unitBalance.skillIds)
                {
                    var skill = balanceConfig.GetSkillById(skillId);
                    if (skill != null)
                        statsStr += $"\n  {skill.skillName} - {skill.description}";
                }
            }

            // Add synthesis info
            var recipe = balanceConfig?.GetSynthesisRecipe(unitName);
            if (recipe != null)
            {
                statsStr += $"\n\n조합: {recipe.sourceUnitName} x2 -> {recipe.resultUnitName}";
            }
            // Check if this unit is a synthesis result
            foreach (var r in balanceConfig.synthesisRecipes)
            {
                if (r.resultUnitName == unitName)
                {
                    statsStr += $"\n조합 방법: {r.sourceUnitName} x2";
                    break;
                }
            }

            GameObject statsObj = new GameObject("Stats");
            statsObj.transform.SetParent(detailPanel.transform, false);
            Text statsText = CreateText(statsObj, statsStr, LobbyDesignTokens.SmallSize, LobbyDesignTokens.TextSecondary);
            statsText.alignment = TextAnchor.UpperLeft;
            statsText.horizontalOverflow = HorizontalWrapMode.Wrap;
            statsText.verticalOverflow = VerticalWrapMode.Truncate;

            RectTransform statsRect = statsObj.GetComponent<RectTransform>();
            statsRect.anchorMin = new Vector2(0.08f, 0.2f);
            statsRect.anchorMax = new Vector2(0.92f, 0.76f);
            statsRect.sizeDelta = Vector2.zero;

            // Unlock / Owned button
            GameObject actionObj = new GameObject("ActionButton");
            actionObj.transform.SetParent(detailPanel.transform, false);
            Image actionBg = actionObj.AddComponent<Image>();

            if (unlocked)
            {
                actionBg.color = LobbyDesignTokens.ButtonDisabled;
                GameObject actionTextObj = new GameObject("Text");
                actionTextObj.transform.SetParent(actionObj.transform, false);
                CreateText(actionTextObj, "보유중", LobbyDesignTokens.ButtonFontSize, LobbyDesignTokens.TextPrimary);
                RectTransform atRect = actionTextObj.GetComponent<RectTransform>();
                atRect.anchorMin = Vector2.zero;
                atRect.anchorMax = Vector2.one;
                atRect.sizeDelta = Vector2.zero;
            }
            else
            {
                bool canAfford = shopManager.CanAfford(unitName);
                actionBg.color = canAfford ? LobbyDesignTokens.ButtonSuccess : LobbyDesignTokens.ButtonDisabled;

                Button actionBtn = actionObj.AddComponent<Button>();
                string capturedName = unitName;
                actionBtn.onClick.AddListener(() =>
                {
                    if (shopManager.TryUnlockUnit(capturedName))
                    {
                        if (detailPopup != null) Destroy(detailPopup);
                        PopulateCards();
                    }
                });
                actionBtn.interactable = canAfford;

                string btnLabel = cost == 0 ? "무료 해금" : $"해금 ({cost}G)";
                GameObject actionTextObj = new GameObject("Text");
                actionTextObj.transform.SetParent(actionObj.transform, false);
                CreateText(actionTextObj, btnLabel, LobbyDesignTokens.ButtonFontSize, LobbyDesignTokens.ButtonText);
                RectTransform atRect = actionTextObj.GetComponent<RectTransform>();
                atRect.anchorMin = Vector2.zero;
                atRect.anchorMax = Vector2.one;
                atRect.sizeDelta = Vector2.zero;
            }

            RectTransform actionRect = actionObj.GetComponent<RectTransform>();
            actionRect.anchorMin = new Vector2(0.15f, 0.05f);
            actionRect.anchorMax = new Vector2(0.85f, 0.16f);
            actionRect.sizeDelta = Vector2.zero;
        }

        public void Show()
        {
            PopulateCards();
            if (overlay != null) overlay.SetActive(true);
        }

        public void Hide()
        {
            if (detailPopup != null) Destroy(detailPopup);
            if (overlay != null) overlay.SetActive(false);
        }

        private Text CreateText(GameObject obj, string text, int fontSize, Color color)
        {
            Text t = obj.AddComponent<Text>();
            t.font = font;
            t.text = text;
            t.fontSize = fontSize;
            t.color = color;
            t.alignment = TextAnchor.MiddleCenter;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            return t;
        }

        private Color GetRarityColor(Rarity rarity)
        {
            return rarity switch
            {
                Rarity.Normal => LobbyDesignTokens.RarityNormal,
                Rarity.Rare => LobbyDesignTokens.RarityRare,
                Rarity.Epic => LobbyDesignTokens.RarityEpic,
                Rarity.Legendary => LobbyDesignTokens.RarityLegendary,
                _ => Color.white
            };
        }
    }
}
