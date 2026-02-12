using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using LottoDefense.Quests;
using LottoDefense.VFX;

namespace LottoDefense.UI
{
    public class QuestUI : MonoBehaviour
    {
        #region Serialized Fields
        [SerializeField] private GameObject questPanel;
        [SerializeField] private Button closeButton;
        [SerializeField] private Transform contentParent;
        #endregion

        #region Private Fields
        private List<QuestItemUI> questItems = new List<QuestItemUI>();
        private Font cachedFont;
        #endregion

        #region Unity Lifecycle
        private void Start()
        {
            cachedFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (cachedFont == null)
                cachedFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);

            if (questPanel != null)
                questPanel.SetActive(false);
        }

        private void OnEnable()
        {
            var questManager = QuestManager.Instance;
            if (questManager != null)
                questManager.OnQuestCompleted += OnQuestCompleted;
        }

        private void OnDisable()
        {
            var questManager = FindFirstObjectByType<QuestManager>();
            if (questManager != null)
                questManager.OnQuestCompleted -= OnQuestCompleted;
        }
        #endregion

        #region Public Methods
        public void Show()
        {
            if (questPanel != null)
                questPanel.SetActive(true);

            RefreshQuestList();
        }

        public void Hide()
        {
            if (questPanel != null)
                questPanel.SetActive(false);
        }
        #endregion

        #region Private Methods
        private void OnQuestCompleted(QuestInstance quest)
        {
            VFXManager.Instance?.ShowQuestCompletedEffect(quest.Definition.hintText);

            // If panel is visible, refresh it
            if (questPanel != null && questPanel.activeSelf)
                RefreshQuestList();
        }

        private void RefreshQuestList()
        {
            // Clear existing items
            foreach (var item in questItems)
            {
                if (item.Root != null)
                    Destroy(item.Root);
            }
            questItems.Clear();

            var questManager = QuestManager.Instance;
            if (questManager == null || contentParent == null) return;

            foreach (var quest in questManager.Quests)
            {
                CreateQuestItem(quest);
            }
        }

        private void CreateQuestItem(QuestInstance quest)
        {
            // Row container
            GameObject row = new GameObject("QuestItem_" + quest.Definition.questId);
            row.transform.SetParent(contentParent, false);

            RectTransform rowRect = row.AddComponent<RectTransform>();
            rowRect.sizeDelta = new Vector2(620, 90);

            Image rowBg = row.AddComponent<Image>();

            // Status-dependent colors
            Color bgColor;
            switch (quest.State)
            {
                case QuestState.Completed:
                    bgColor = GameSceneDesignTokens.QuestCompletedBg;
                    break;
                case QuestState.Rewarded:
                    bgColor = GameSceneDesignTokens.QuestRewardedBg;
                    break;
                default:
                    bgColor = GameSceneDesignTokens.QuestHiddenBg;
                    break;
            }
            rowBg.color = bgColor;

            // Quest text
            GameObject textObj = new GameObject("QuestText");
            textObj.transform.SetParent(row.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0, 0);
            textRect.anchorMax = new Vector2(0.65f, 1);
            textRect.offsetMin = new Vector2(15, 5);
            textRect.offsetMax = new Vector2(0, -5);

            Text questText = textObj.AddComponent<Text>();
            questText.font = cachedFont;
            questText.fontSize = 22;
            questText.alignment = TextAnchor.MiddleLeft;
            questText.horizontalOverflow = HorizontalWrapMode.Wrap;
            questText.verticalOverflow = VerticalWrapMode.Overflow;
            questText.raycastTarget = false;

            switch (quest.State)
            {
                case QuestState.Hidden:
                    questText.text = $"??? ({quest.Definition.hintText})";
                    questText.color = GameSceneDesignTokens.QuestHiddenText;
                    break;
                case QuestState.Completed:
                    questText.text = quest.Definition.descriptionText;
                    questText.color = GameSceneDesignTokens.QuestCompletedText;
                    break;
                case QuestState.Rewarded:
                    questText.text = quest.Definition.descriptionText;
                    questText.color = GameSceneDesignTokens.QuestRewardedText;
                    break;
            }

            // Reward area (right side)
            GameObject rewardArea = new GameObject("RewardArea");
            rewardArea.transform.SetParent(row.transform, false);
            RectTransform rewardRect = rewardArea.AddComponent<RectTransform>();
            rewardRect.anchorMin = new Vector2(0.65f, 0);
            rewardRect.anchorMax = new Vector2(1, 1);
            rewardRect.offsetMin = new Vector2(5, 8);
            rewardRect.offsetMax = new Vector2(-10, -8);

            if (quest.State == QuestState.Completed)
            {
                // "보상 받기" button
                Image rewardBtnBg = rewardArea.AddComponent<Image>();
                rewardBtnBg.color = GameSceneDesignTokens.QuestRewardBtnBg;

                Button rewardBtn = rewardArea.AddComponent<Button>();
                ColorBlock colors = rewardBtn.colors;
                colors.normalColor = Color.white;
                colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
                colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
                colors.fadeDuration = 0.1f;
                rewardBtn.colors = colors;

                GameObject btnTextObj = new GameObject("BtnText");
                btnTextObj.transform.SetParent(rewardArea.transform, false);
                RectTransform btnTextRect = btnTextObj.AddComponent<RectTransform>();
                btnTextRect.anchorMin = Vector2.zero;
                btnTextRect.anchorMax = Vector2.one;
                btnTextRect.offsetMin = Vector2.zero;
                btnTextRect.offsetMax = Vector2.zero;

                Text btnText = btnTextObj.AddComponent<Text>();
                btnText.font = cachedFont;
                btnText.text = $"보상 받기\n+{quest.Definition.goldReward}G";
                btnText.fontSize = 20;
                btnText.color = Color.white;
                btnText.alignment = TextAnchor.MiddleCenter;
                btnText.fontStyle = FontStyle.Bold;
                btnText.raycastTarget = false;

                string questId = quest.Definition.questId;
                rewardBtn.onClick.AddListener(() => OnClaimReward(questId));
            }
            else
            {
                // Gold reward text (non-clickable)
                Text rewardText = rewardArea.AddComponent<Text>();
                rewardText.font = cachedFont;
                rewardText.alignment = TextAnchor.MiddleCenter;
                rewardText.raycastTarget = false;

                if (quest.State == QuestState.Rewarded)
                {
                    rewardText.text = "완료";
                    rewardText.fontSize = 24;
                    rewardText.color = GameSceneDesignTokens.QuestRewardedText;
                    rewardText.fontStyle = FontStyle.Bold;
                }
                else
                {
                    rewardText.text = $"+{quest.Definition.goldReward}G";
                    rewardText.fontSize = 22;
                    rewardText.color = GameSceneDesignTokens.GoldColor;
                }
            }

            questItems.Add(new QuestItemUI(row));
        }

        private void OnClaimReward(string questId)
        {
            var questManager = QuestManager.Instance;
            if (questManager == null) return;

            // Look up gold reward before claiming (state changes after claim)
            int goldReward = 0;
            foreach (var quest in questManager.Quests)
            {
                if (quest.Definition.questId == questId && quest.State == QuestState.Completed)
                {
                    goldReward = quest.Definition.goldReward;
                    break;
                }
            }

            if (questManager.ClaimReward(questId))
            {
                VFXManager.Instance?.ShowRewardClaimedEffect(goldReward);
                RefreshQuestList();
            }
        }
        #endregion

        #region Helper Classes
        private class QuestItemUI
        {
            public GameObject Root { get; private set; }

            public QuestItemUI(GameObject root)
            {
                Root = root;
            }
        }
        #endregion
    }
}
