using UnityEngine;
using System;
using System.Collections.Generic;

namespace LottoDefense.Lobby
{
    public class MailboxManager : MonoBehaviour
    {
        #region Fields
        private List<MailEntry> mails;
        #endregion

        #region Events
        public event Action OnMailStateChanged;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            // Try to load from Resources, fallback to defaults
            MailConfig config = Resources.Load<MailConfig>("MailConfig");
            if (config != null && config.mails.Count > 0)
                mails = config.mails;
            else
                mails = MailConfig.GetDefaultMails();

            Debug.Log($"[MailboxManager] Loaded {mails.Count} mails");
        }
        #endregion

        #region Mail Logic
        public List<MailEntry> GetAllMails()
        {
            return mails;
        }

        public bool IsRead(string mailId)
        {
            return LobbyDataManager.Instance != null && LobbyDataManager.Instance.IsMailRead(mailId);
        }

        public void MarkRead(string mailId)
        {
            if (LobbyDataManager.Instance == null) return;
            LobbyDataManager.Instance.SetMailRead(mailId, true);
            OnMailStateChanged?.Invoke();
        }

        public bool IsClaimed(string mailId)
        {
            return LobbyDataManager.Instance != null && LobbyDataManager.Instance.IsMailClaimed(mailId);
        }

        public bool TryClaim(string mailId)
        {
            if (LobbyDataManager.Instance == null) return false;
            if (IsClaimed(mailId)) return false;

            MailEntry mail = mails.Find(m => m.id == mailId);
            if (mail == null || mail.type != MailType.Reward) return false;

            // Grant attachments
            foreach (var attachment in mail.attachments)
            {
                switch (attachment.itemType)
                {
                    case "gold":
                        LobbyDataManager.Instance.Gold += attachment.amount;
                        break;
                    case "ticket":
                        LobbyDataManager.Instance.Tickets += attachment.amount;
                        break;
                }
            }

            LobbyDataManager.Instance.SetMailClaimed(mailId, true);
            LobbyDataManager.Instance.SetMailRead(mailId, true);
            OnMailStateChanged?.Invoke();

            Debug.Log($"[MailboxManager] Mail '{mail.title}' claimed");
            return true;
        }

        public void ClaimAll()
        {
            foreach (var mail in mails)
            {
                if (mail.type == MailType.Reward && !IsClaimed(mail.id))
                {
                    TryClaim(mail.id);
                }
            }
        }

        public bool HasUnclaimedMail()
        {
            return GetUnclaimedMailCount() > 0;
        }

        public int GetUnclaimedMailCount()
        {
            int count = 0;
            foreach (var mail in mails)
            {
                if (mail.type == MailType.Reward && !IsClaimed(mail.id))
                    count++;
                else if (mail.type == MailType.Notice && !IsRead(mail.id))
                    count++;
            }
            return count;
        }
        #endregion
    }
}
