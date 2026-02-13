using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using LottoDefense.Lobby;

namespace LottoDefense.UI
{
    public class LobbyQuestUI : MonoBehaviour
    {
        private Canvas canvas;
        private Font font;
        private GameObject overlay;
        private GameObject contentArea;
        private LobbyQuestManager questManager;
        private bool showDaily = false;

        // Tab buttons for state tracking
        private Image permanentTabImg;
        private Image dailyTabImg;

        public static LobbyQuestUI Create(Canvas parentCanvas, Font font)
        {
            GameObject obj = new GameObject("LobbyQuestUI");
            obj.transform.SetParent(parentCanvas.transform, false);
            LobbyQuestUI ui = obj.AddComponent<LobbyQuestUI>();
            ui.canvas = parentCanvas;
            ui.font = font;
            ui.questManager = FindFirstObjectByType<LobbyQuestManager>();
            ui.BuildUI();
            return ui;
        }

        private void BuildUI()
        {
            GameObject panel = MainGameBootstrapper.CreateModalPopup(
                canvas, font, "퀘스트", 0.9f, 0.75f, out overlay, out Button closeBtn);
            closeBtn.onClick.AddListener(Hide);

            // Tab bar
            GameObject tabBar = new GameObject("TabBar");
            tabBar.transform.SetParent(panel.transform, false);

            RectTransform tabRect = tabBar.AddComponent<RectTransform>();
            tabRect.anchorMin = new Vector2(0.05f, 0.78f);
            tabRect.anchorMax = new Vector2(0.7f, 0.87f);
            tabRect.sizeDelta = Vector2.zero;

            HorizontalLayoutGroup tabLayout = tabBar.AddComponent<HorizontalLayoutGroup>();
            tabLayout.spacing = 10;
            tabLayout.childForceExpandWidth = true;
            tabLayout.childForceExpandHeight = true;

            // Permanent tab
            GameObject permTab = new GameObject("PermanentTab");
            permTab.transform.SetParent(tabBar.transform, false);
            permanentTabImg = permTab.AddComponent<Image>();
            permanentTabImg.color = LobbyDesignTokens.TabActive;
            Button permBtn = permTab.AddComponent<Button>();
            permBtn.onClick.AddListener(() => { showDaily = false; RefreshTabs(); PopulateQuests(); });

            GameObject permTextObj = new GameObject("Text");
            permTextObj.transform.SetParent(permTab.transform, false);
            Text permText = CreateText(permTextObj, "영구", LobbyDesignTokens.SmallSize, Color.white);
            RectTransform ptRect = permTextObj.GetComponent<RectTransform>();
            ptRect.anchorMin = Vector2.zero;
            ptRect.anchorMax = Vector2.one;
            ptRect.sizeDelta = Vector2.zero;

            // Daily tab
            GameObject dailyTab = new GameObject("DailyTab");
            dailyTab.transform.SetParent(tabBar.transform, false);
            dailyTabImg = dailyTab.AddComponent<Image>();
            dailyTabImg.color = LobbyDesignTokens.TabInactive;
            Button dailyBtn = dailyTab.AddComponent<Button>();
            dailyBtn.onClick.AddListener(() => { showDaily = true; RefreshTabs(); PopulateQuests(); });

            GameObject dailyTextObj = new GameObject("Text");
            dailyTextObj.transform.SetParent(dailyTab.transform, false);
            Text dailyText = CreateText(dailyTextObj, "일일", LobbyDesignTokens.SmallSize, Color.white);
            RectTransform dtRect = dailyTextObj.GetComponent<RectTransform>();
            dtRect.anchorMin = Vector2.zero;
            dtRect.anchorMax = Vector2.one;
            dtRect.sizeDelta = Vector2.zero;

            // Content area (scrollable quest list)
            contentArea = new GameObject("Content");
            contentArea.transform.SetParent(panel.transform, false);

            RectTransform contentRect = contentArea.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0.03f, 0.03f);
            contentRect.anchorMax = new Vector2(0.97f, 0.76f);
            contentRect.sizeDelta = Vector2.zero;

            VerticalLayoutGroup layout = contentArea.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 12f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.padding = new RectOffset(10, 10, 10, 10);
        }

        private void RefreshTabs()
        {
            if (permanentTabImg != null)
                permanentTabImg.color = showDaily ? LobbyDesignTokens.TabInactive : LobbyDesignTokens.TabActive;
            if (dailyTabImg != null)
                dailyTabImg.color = showDaily ? LobbyDesignTokens.TabActive : LobbyDesignTokens.TabInactive;
        }

        public void Show()
        {
            // Refresh daily quest progress on open
            if (questManager != null)
                questManager.RefreshDailyQuestProgress();

            showDaily = false;
            RefreshTabs();
            PopulateQuests();
            if (overlay != null) overlay.SetActive(true);
        }

        public void Hide()
        {
            if (overlay != null) overlay.SetActive(false);
        }

        private void PopulateQuests()
        {
            // Clear existing
            for (int i = contentArea.transform.childCount - 1; i >= 0; i--)
                Destroy(contentArea.transform.GetChild(i).gameObject);

            if (questManager == null) return;

            List<LobbyQuestDefinition> quests = LobbyQuestConfig.GetAllQuests();
            foreach (var quest in quests)
            {
                if (quest.isDaily != showDaily) continue;
                CreateQuestRow(quest);
            }
        }

        private void CreateQuestRow(LobbyQuestDefinition quest)
        {
            int progress = questManager.GetProgress(quest);
            bool completed = questManager.IsCompleted(quest);
            bool claimed = questManager.IsClaimed(quest);
            bool canClaim = questManager.CanClaim(quest);

            GameObject row = new GameObject($"Quest_{quest.id}");
            row.transform.SetParent(contentArea.transform, false);

            Image rowBg = row.AddComponent<Image>();
            rowBg.color = claimed ? new Color(0.12f, 0.2f, 0.12f, 1f) : LobbyDesignTokens.CardBg;

            LayoutElement rowLE = row.AddComponent<LayoutElement>();
            rowLE.preferredHeight = 120;

            // Title
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(row.transform, false);
            Text titleText = CreateText(titleObj, quest.title, LobbyDesignTokens.BodySize, LobbyDesignTokens.TextPrimary);
            titleText.alignment = TextAnchor.MiddleLeft;

            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.03f, 0.55f);
            titleRect.anchorMax = new Vector2(0.6f, 0.95f);
            titleRect.sizeDelta = Vector2.zero;

            // Progress text
            string progressStr = $"{Mathf.Min(progress, quest.targetCount)}/{quest.targetCount}";
            GameObject progressObj = new GameObject("Progress");
            progressObj.transform.SetParent(row.transform, false);
            CreateText(progressObj, progressStr, LobbyDesignTokens.SmallSize, LobbyDesignTokens.TextSecondary);

            RectTransform progressRect = progressObj.GetComponent<RectTransform>();
            progressRect.anchorMin = new Vector2(0.03f, 0.1f);
            progressRect.anchorMax = new Vector2(0.35f, 0.5f);
            progressRect.sizeDelta = Vector2.zero;

            // Progress bar
            GameObject barBgObj = new GameObject("BarBg");
            barBgObj.transform.SetParent(row.transform, false);
            Image barBg = barBgObj.AddComponent<Image>();
            barBg.color = LobbyDesignTokens.ProgressBg;

            RectTransform barBgRect = barBgObj.GetComponent<RectTransform>();
            barBgRect.anchorMin = new Vector2(0.35f, 0.2f);
            barBgRect.anchorMax = new Vector2(0.62f, 0.4f);
            barBgRect.sizeDelta = Vector2.zero;

            GameObject barFillObj = new GameObject("BarFill");
            barFillObj.transform.SetParent(barBgObj.transform, false);
            Image barFill = barFillObj.AddComponent<Image>();
            barFill.color = completed ? LobbyDesignTokens.ProgressComplete : LobbyDesignTokens.ProgressFill;

            float fillAmount = quest.targetCount > 0 ? Mathf.Clamp01((float)progress / quest.targetCount) : 0f;
            RectTransform barFillRect = barFillObj.GetComponent<RectTransform>();
            barFillRect.anchorMin = Vector2.zero;
            barFillRect.anchorMax = new Vector2(fillAmount, 1f);
            barFillRect.sizeDelta = Vector2.zero;

            // Reward display
            string rewardStr = "";
            foreach (var reward in quest.rewards)
            {
                if (rewardStr.Length > 0) rewardStr += " ";
                rewardStr += reward.itemType == "gold" ? $"+{reward.amount}G" : $"+{reward.amount}T";
            }

            GameObject rewardObj = new GameObject("Reward");
            rewardObj.transform.SetParent(row.transform, false);
            CreateText(rewardObj, rewardStr, LobbyDesignTokens.SmallSize - 2, LobbyDesignTokens.GoldColor);

            RectTransform rewardRect = rewardObj.GetComponent<RectTransform>();
            rewardRect.anchorMin = new Vector2(0.63f, 0.1f);
            rewardRect.anchorMax = new Vector2(0.78f, 0.5f);
            rewardRect.sizeDelta = Vector2.zero;

            // Claim button / status
            GameObject actionObj = new GameObject("Action");
            actionObj.transform.SetParent(row.transform, false);

            if (claimed)
            {
                CreateText(actionObj, "수령완료", LobbyDesignTokens.SmallSize - 2, LobbyDesignTokens.TextSuccess);
            }
            else if (canClaim)
            {
                Image actionBg = actionObj.AddComponent<Image>();
                actionBg.color = LobbyDesignTokens.ButtonSuccess;
                Button actionBtn = actionObj.AddComponent<Button>();
                string questId = quest.id;
                actionBtn.onClick.AddListener(() =>
                {
                    var q = LobbyQuestConfig.GetAllQuests().Find(x => x.id == questId);
                    if (q != null) questManager.TryClaimReward(q);
                    PopulateQuests();
                });

                GameObject btnTextObj = new GameObject("Text");
                btnTextObj.transform.SetParent(actionObj.transform, false);
                CreateText(btnTextObj, "수령", LobbyDesignTokens.SmallSize - 2, Color.white);
                RectTransform btRect = btnTextObj.GetComponent<RectTransform>();
                btRect.anchorMin = Vector2.zero;
                btRect.anchorMax = Vector2.one;
                btRect.sizeDelta = Vector2.zero;
            }
            else
            {
                CreateText(actionObj, "진행중", LobbyDesignTokens.SmallSize - 2, LobbyDesignTokens.TextMuted);
            }

            RectTransform actionRect = actionObj.GetComponent<RectTransform>();
            if (actionRect == null) actionRect = actionObj.AddComponent<RectTransform>();
            actionRect.anchorMin = new Vector2(0.78f, 0.15f);
            actionRect.anchorMax = new Vector2(0.97f, 0.85f);
            actionRect.sizeDelta = Vector2.zero;
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
    }
}
