using UnityEngine;
using UnityEngine.UI;
using LottoDefense.Lobby;

namespace LottoDefense.UI
{
    public class DailyRewardUI : MonoBehaviour
    {
        private Canvas canvas;
        private Font font;
        private GameObject overlay;
        private GameObject contentArea;
        private Text infoText;

        private DailyRewardManager rewardManager;

        public static DailyRewardUI Create(Canvas parentCanvas, Font font)
        {
            GameObject obj = new GameObject("DailyRewardUI");
            obj.transform.SetParent(parentCanvas.transform, false);
            DailyRewardUI ui = obj.AddComponent<DailyRewardUI>();
            ui.canvas = parentCanvas;
            ui.font = font;
            ui.rewardManager = FindFirstObjectByType<DailyRewardManager>();
            ui.BuildUI();
            return ui;
        }

        private void BuildUI()
        {
            GameObject panel = MainGameBootstrapper.CreateModalPopup(
                canvas, font, "일일 보상", 0.9f, 0.55f, out overlay, out Button closeBtn);
            closeBtn.onClick.AddListener(Hide);

            // Progress info
            GameObject infoObj = new GameObject("Info");
            infoObj.transform.SetParent(panel.transform, false);
            infoText = CreateText(infoObj, "", LobbyDesignTokens.BodySize, LobbyDesignTokens.TextSecondary);

            RectTransform infoRect = infoObj.GetComponent<RectTransform>();
            infoRect.anchorMin = new Vector2(0.05f, 0.78f);
            infoRect.anchorMax = new Vector2(0.95f, 0.87f);
            infoRect.sizeDelta = Vector2.zero;

            // Content area for stage cards
            contentArea = new GameObject("Content");
            contentArea.transform.SetParent(panel.transform, false);

            RectTransform contentRect = contentArea.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0.03f, 0.05f);
            contentRect.anchorMax = new Vector2(0.97f, 0.76f);
            contentRect.sizeDelta = Vector2.zero;

            HorizontalLayoutGroup layout = contentArea.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 12f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
            layout.padding = new RectOffset(10, 10, 10, 10);
        }

        public void Show()
        {
            PopulateStages();
            if (overlay != null) overlay.SetActive(true);
        }

        public void Hide()
        {
            if (overlay != null) overlay.SetActive(false);
        }

        private void PopulateStages()
        {
            // Clear existing children
            for (int i = contentArea.transform.childCount - 1; i >= 0; i--)
                Destroy(contentArea.transform.GetChild(i).gameObject);

            if (rewardManager == null) return;

            int clearCount = rewardManager.GetDailyClearCount();

            // Update info text
            if (infoText != null) infoText.text = $"오늘의 클리어: {clearCount}회";

            for (int i = 0; i < rewardManager.GetStageCount(); i++)
            {
                CreateStageCard(i, clearCount);
            }
        }

        private void CreateStageCard(int stageIndex, int currentClears)
        {
            var stage = rewardManager.GetStage(stageIndex);
            if (stage == null) return;

            bool achieved = currentClears >= stage.requiredClears;
            bool claimed = rewardManager.IsStageClaimed(stageIndex);
            bool canClaim = rewardManager.CanClaimStage(stageIndex);

            GameObject card = new GameObject($"Stage_{stageIndex + 1}");
            card.transform.SetParent(contentArea.transform, false);

            Image cardBg = card.AddComponent<Image>();
            if (claimed)
                cardBg.color = new Color(0.15f, 0.3f, 0.15f, 1f);
            else if (achieved)
                cardBg.color = LobbyDesignTokens.CardBgHighlight;
            else
                cardBg.color = LobbyDesignTokens.CardBgLocked;

            // Stage number
            GameObject numObj = new GameObject("StageNum");
            numObj.transform.SetParent(card.transform, false);
            CreateText(numObj, $"{stageIndex + 1}단계", LobbyDesignTokens.SmallSize, LobbyDesignTokens.TextPrimary);

            RectTransform numRect = numObj.GetComponent<RectTransform>();
            numRect.anchorMin = new Vector2(0f, 0.8f);
            numRect.anchorMax = new Vector2(1f, 1f);
            numRect.sizeDelta = Vector2.zero;

            // Required clears
            GameObject reqObj = new GameObject("Required");
            reqObj.transform.SetParent(card.transform, false);
            string reqStr = $"{stage.requiredClears}회";
            CreateText(reqObj, reqStr, LobbyDesignTokens.SmallSize - 4, LobbyDesignTokens.TextMuted);

            RectTransform reqRect = reqObj.GetComponent<RectTransform>();
            reqRect.anchorMin = new Vector2(0f, 0.65f);
            reqRect.anchorMax = new Vector2(1f, 0.8f);
            reqRect.sizeDelta = Vector2.zero;

            // Reward display
            string rewardStr = "";
            if (stage.goldReward > 0) rewardStr += $"+{stage.goldReward}G";
            if (stage.ticketReward > 0)
            {
                if (rewardStr.Length > 0) rewardStr += "\n";
                rewardStr += $"+{stage.ticketReward}T";
            }

            GameObject rewardObj = new GameObject("Reward");
            rewardObj.transform.SetParent(card.transform, false);
            CreateText(rewardObj, rewardStr, LobbyDesignTokens.SmallSize - 2, LobbyDesignTokens.GoldColor);

            RectTransform rewardRect = rewardObj.GetComponent<RectTransform>();
            rewardRect.anchorMin = new Vector2(0f, 0.35f);
            rewardRect.anchorMax = new Vector2(1f, 0.65f);
            rewardRect.sizeDelta = Vector2.zero;

            // Action button / status
            GameObject actionObj = new GameObject("Action");
            actionObj.transform.SetParent(card.transform, false);

            if (claimed)
            {
                CreateText(actionObj, "수령완료", LobbyDesignTokens.SmallSize - 2, LobbyDesignTokens.TextSuccess);
            }
            else if (canClaim)
            {
                Image actionBg = actionObj.AddComponent<Image>();
                actionBg.color = LobbyDesignTokens.ButtonSuccess;
                Button actionBtn = actionObj.AddComponent<Button>();
                int idx = stageIndex;
                actionBtn.onClick.AddListener(() =>
                {
                    rewardManager.TryClaimStage(idx);
                    PopulateStages();
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
                int remaining = stage.requiredClears - currentClears;
                CreateText(actionObj, $"잔여 {remaining}회", LobbyDesignTokens.SmallSize - 4, LobbyDesignTokens.TextMuted);
            }

            RectTransform actionRect = actionObj.GetComponent<RectTransform>();
            if (actionRect == null) actionRect = actionObj.AddComponent<RectTransform>();
            actionRect.anchorMin = new Vector2(0.1f, 0.05f);
            actionRect.anchorMax = new Vector2(0.9f, 0.3f);
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
