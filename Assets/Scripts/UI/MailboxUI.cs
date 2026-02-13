using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using LottoDefense.Lobby;

namespace LottoDefense.UI
{
    public class MailboxUI : MonoBehaviour
    {
        private Canvas canvas;
        private Font font;
        private GameObject overlay;
        private GameObject listArea;
        private GameObject detailPanel;

        private MailboxManager mailboxManager;

        public static MailboxUI Create(Canvas parentCanvas, Font font)
        {
            GameObject obj = new GameObject("MailboxUI");
            obj.transform.SetParent(parentCanvas.transform, false);
            MailboxUI ui = obj.AddComponent<MailboxUI>();
            ui.canvas = parentCanvas;
            ui.font = font;
            ui.mailboxManager = FindFirstObjectByType<MailboxManager>();
            ui.BuildUI();
            return ui;
        }

        private void BuildUI()
        {
            GameObject panel = MainGameBootstrapper.CreateModalPopup(
                canvas, font, "우편함", 0.9f, 0.75f, out overlay, out Button closeBtn);
            closeBtn.onClick.AddListener(Hide);

            // Claim All button
            GameObject claimAllObj = new GameObject("ClaimAllButton");
            claimAllObj.transform.SetParent(panel.transform, false);
            Image claimAllBg = claimAllObj.AddComponent<Image>();
            claimAllBg.color = LobbyDesignTokens.ButtonPrimary;
            Button claimAllBtn = claimAllObj.AddComponent<Button>();
            claimAllBtn.onClick.AddListener(() =>
            {
                if (mailboxManager != null) mailboxManager.ClaimAll();
                PopulateMailList();
            });

            RectTransform claimAllRect = claimAllObj.GetComponent<RectTransform>();
            claimAllRect.anchorMin = new Vector2(0.55f, 0.78f);
            claimAllRect.anchorMax = new Vector2(0.85f, 0.87f);
            claimAllRect.sizeDelta = Vector2.zero;

            GameObject claimAllTextObj = new GameObject("Text");
            claimAllTextObj.transform.SetParent(claimAllObj.transform, false);
            CreateText(claimAllTextObj, "전체 수령", LobbyDesignTokens.SmallSize, Color.white);
            RectTransform catRect = claimAllTextObj.GetComponent<RectTransform>();
            catRect.anchorMin = Vector2.zero;
            catRect.anchorMax = Vector2.one;
            catRect.sizeDelta = Vector2.zero;

            // Mail list area
            listArea = new GameObject("MailList");
            listArea.transform.SetParent(panel.transform, false);

            RectTransform listRect = listArea.AddComponent<RectTransform>();
            listRect.anchorMin = new Vector2(0.03f, 0.03f);
            listRect.anchorMax = new Vector2(0.97f, 0.76f);
            listRect.sizeDelta = Vector2.zero;

            VerticalLayoutGroup layout = listArea.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.padding = new RectOffset(10, 10, 10, 10);
        }

        public void Show()
        {
            PopulateMailList();
            if (overlay != null) overlay.SetActive(true);
        }

        public void Hide()
        {
            if (detailPanel != null) Destroy(detailPanel);
            if (overlay != null) overlay.SetActive(false);
        }

        private void PopulateMailList()
        {
            // Clear existing
            for (int i = listArea.transform.childCount - 1; i >= 0; i--)
                Destroy(listArea.transform.GetChild(i).gameObject);

            if (mailboxManager == null) return;

            List<MailEntry> mails = mailboxManager.GetAllMails();
            foreach (var mail in mails)
            {
                CreateMailRow(mail);
            }
        }

        private void CreateMailRow(MailEntry mail)
        {
            bool isRead = mailboxManager.IsRead(mail.id);
            bool isClaimed = mailboxManager.IsClaimed(mail.id);
            bool isReward = mail.type == MailType.Reward;

            GameObject row = new GameObject($"Mail_{mail.id}");
            row.transform.SetParent(listArea.transform, false);

            Image rowBg = row.AddComponent<Image>();
            if (isClaimed || (mail.type == MailType.Notice && isRead))
                rowBg.color = new Color(0.1f, 0.1f, 0.15f, 1f);
            else
                rowBg.color = LobbyDesignTokens.CardBg;

            LayoutElement rowLE = row.AddComponent<LayoutElement>();
            rowLE.preferredHeight = 100;

            Button rowBtn = row.AddComponent<Button>();
            string mailId = mail.id;
            rowBtn.onClick.AddListener(() => ShowMailDetail(mailId));

            // Unread indicator
            if (!isRead)
            {
                GameObject dotObj = new GameObject("UnreadDot");
                dotObj.transform.SetParent(row.transform, false);
                Image dot = dotObj.AddComponent<Image>();
                dot.color = LobbyDesignTokens.BadgeBg;

                RectTransform dotRect = dotObj.GetComponent<RectTransform>();
                dotRect.anchorMin = new Vector2(0.02f, 0.4f);
                dotRect.anchorMax = new Vector2(0.05f, 0.6f);
                dotRect.sizeDelta = Vector2.zero;
            }

            // Title
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(row.transform, false);
            Text titleText = CreateText(titleObj, mail.title, LobbyDesignTokens.BodySize,
                isRead ? LobbyDesignTokens.TextMuted : LobbyDesignTokens.TextPrimary);
            titleText.alignment = TextAnchor.MiddleLeft;
            titleText.fontStyle = isRead ? FontStyle.Normal : FontStyle.Bold;

            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.07f, 0.3f);
            titleRect.anchorMax = new Vector2(0.7f, 0.9f);
            titleRect.sizeDelta = Vector2.zero;

            // Type label
            string typeStr = isReward ? "보상" : "공지";
            Color typeColor = isReward ? LobbyDesignTokens.GoldColor : LobbyDesignTokens.TextSecondary;

            GameObject typeObj = new GameObject("Type");
            typeObj.transform.SetParent(row.transform, false);
            CreateText(typeObj, typeStr, LobbyDesignTokens.SmallSize - 4, typeColor);

            RectTransform typeRect = typeObj.GetComponent<RectTransform>();
            typeRect.anchorMin = new Vector2(0.07f, 0.05f);
            typeRect.anchorMax = new Vector2(0.3f, 0.35f);
            typeRect.sizeDelta = Vector2.zero;

            // Status
            if (isReward)
            {
                string statusStr = isClaimed ? "수령완료" : "미수령";
                Color statusColor = isClaimed ? LobbyDesignTokens.TextMuted : LobbyDesignTokens.TextSuccess;

                GameObject statusObj = new GameObject("Status");
                statusObj.transform.SetParent(row.transform, false);
                CreateText(statusObj, statusStr, LobbyDesignTokens.SmallSize - 2, statusColor);

                RectTransform statusRect = statusObj.GetComponent<RectTransform>();
                statusRect.anchorMin = new Vector2(0.75f, 0.3f);
                statusRect.anchorMax = new Vector2(0.97f, 0.7f);
                statusRect.sizeDelta = Vector2.zero;
            }
        }

        private void ShowMailDetail(string mailId)
        {
            if (detailPanel != null) Destroy(detailPanel);

            MailEntry mail = mailboxManager.GetAllMails().Find(m => m.id == mailId);
            if (mail == null) return;

            // Mark as read
            mailboxManager.MarkRead(mailId);

            // Detail overlay
            detailPanel = new GameObject("MailDetail");
            detailPanel.transform.SetParent(overlay.transform, false);
            Image detailBg = detailPanel.AddComponent<Image>();
            detailBg.color = new Color(0f, 0f, 0f, 0.5f);

            RectTransform detailRect = detailPanel.GetComponent<RectTransform>();
            detailRect.anchorMin = Vector2.zero;
            detailRect.anchorMax = Vector2.one;
            detailRect.sizeDelta = Vector2.zero;

            // Panel
            GameObject panel = new GameObject("Panel");
            panel.transform.SetParent(detailPanel.transform, false);
            Image panelBg = panel.AddComponent<Image>();
            panelBg.color = LobbyDesignTokens.ModalPanelBg;

            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.1f, 0.2f);
            panelRect.anchorMax = new Vector2(0.9f, 0.8f);
            panelRect.sizeDelta = Vector2.zero;

            // Close button
            GameObject closeObj = new GameObject("Close");
            closeObj.transform.SetParent(panel.transform, false);
            Image closeBg = closeObj.AddComponent<Image>();
            closeBg.color = LobbyDesignTokens.ButtonClose;
            Button closeBtn = closeObj.AddComponent<Button>();
            closeBtn.onClick.AddListener(() => { Destroy(detailPanel); PopulateMailList(); });

            RectTransform closeRect = closeObj.GetComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(1f, 1f);
            closeRect.anchorMax = new Vector2(1f, 1f);
            closeRect.pivot = new Vector2(1f, 1f);
            closeRect.anchoredPosition = new Vector2(-10f, -10f);
            closeRect.sizeDelta = new Vector2(50f, 50f);

            GameObject closeTextObj = new GameObject("X");
            closeTextObj.transform.SetParent(closeObj.transform, false);
            CreateText(closeTextObj, "X", LobbyDesignTokens.BodySize, Color.white);
            RectTransform ctRect = closeTextObj.GetComponent<RectTransform>();
            ctRect.anchorMin = Vector2.zero;
            ctRect.anchorMax = Vector2.one;
            ctRect.sizeDelta = Vector2.zero;

            // Title
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(panel.transform, false);
            Text titleText = CreateText(titleObj, mail.title, LobbyDesignTokens.SubHeaderSize, LobbyDesignTokens.TextPrimary);
            titleText.fontStyle = FontStyle.Bold;

            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.05f, 0.82f);
            titleRect.anchorMax = new Vector2(0.85f, 0.95f);
            titleRect.sizeDelta = Vector2.zero;

            // Body
            GameObject bodyObj = new GameObject("Body");
            bodyObj.transform.SetParent(panel.transform, false);
            Text bodyText = CreateText(bodyObj, mail.body, LobbyDesignTokens.SmallSize, LobbyDesignTokens.TextSecondary);
            bodyText.alignment = TextAnchor.UpperLeft;
            bodyText.horizontalOverflow = HorizontalWrapMode.Wrap;

            RectTransform bodyRect = bodyObj.GetComponent<RectTransform>();
            bodyRect.anchorMin = new Vector2(0.08f, 0.35f);
            bodyRect.anchorMax = new Vector2(0.92f, 0.8f);
            bodyRect.sizeDelta = Vector2.zero;

            // Attachments display
            if (mail.attachments != null && mail.attachments.Count > 0)
            {
                string attachStr = "첨부: ";
                foreach (var att in mail.attachments)
                {
                    attachStr += att.itemType == "gold" ? $"+{att.amount}G " : $"+{att.amount}T ";
                }

                GameObject attachObj = new GameObject("Attachments");
                attachObj.transform.SetParent(panel.transform, false);
                CreateText(attachObj, attachStr, LobbyDesignTokens.SmallSize, LobbyDesignTokens.GoldColor);

                RectTransform attachRect = attachObj.GetComponent<RectTransform>();
                attachRect.anchorMin = new Vector2(0.08f, 0.22f);
                attachRect.anchorMax = new Vector2(0.92f, 0.35f);
                attachRect.sizeDelta = Vector2.zero;
            }

            // Claim button (for reward mails)
            if (mail.type == MailType.Reward)
            {
                bool claimed = mailboxManager.IsClaimed(mailId);

                GameObject actionObj = new GameObject("ClaimButton");
                actionObj.transform.SetParent(panel.transform, false);
                Image actionBg = actionObj.AddComponent<Image>();
                actionBg.color = claimed ? LobbyDesignTokens.ButtonDisabled : LobbyDesignTokens.ButtonSuccess;

                RectTransform actionRect = actionObj.GetComponent<RectTransform>();
                actionRect.anchorMin = new Vector2(0.2f, 0.05f);
                actionRect.anchorMax = new Vector2(0.8f, 0.18f);
                actionRect.sizeDelta = Vector2.zero;

                if (!claimed)
                {
                    Button actionBtn = actionObj.AddComponent<Button>();
                    string capturedId = mailId;
                    actionBtn.onClick.AddListener(() =>
                    {
                        mailboxManager.TryClaim(capturedId);
                        Destroy(detailPanel);
                        PopulateMailList();
                    });
                }

                string btnLabel = claimed ? "수령완료" : "수령";
                GameObject btnTextObj = new GameObject("Text");
                btnTextObj.transform.SetParent(actionObj.transform, false);
                CreateText(btnTextObj, btnLabel, LobbyDesignTokens.ButtonFontSize, Color.white);
                RectTransform btRect = btnTextObj.GetComponent<RectTransform>();
                btRect.anchorMin = Vector2.zero;
                btRect.anchorMax = Vector2.one;
                btRect.sizeDelta = Vector2.zero;
            }
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
