using UnityEngine;
using System.Collections.Generic;

namespace LottoDefense.Lobby
{
    public enum MailType
    {
        Notice,
        Reward
    }

    [System.Serializable]
    public class MailAttachment
    {
        public string itemType; // "gold" or "ticket"
        public int amount;

        public MailAttachment(string type, int amt)
        {
            itemType = type;
            amount = amt;
        }
    }

    [System.Serializable]
    public class MailEntry
    {
        public string id;
        public string title;
        public string body;
        public MailType type;
        public List<MailAttachment> attachments;
    }

    [CreateAssetMenu(fileName = "MailConfig", menuName = "LottoDefense/Mail Config")]
    public class MailConfig : ScriptableObject
    {
        public List<MailEntry> mails = new List<MailEntry>();

        /// <summary>
        /// Get default mails when no ScriptableObject asset exists.
        /// </summary>
        public static List<MailEntry> GetDefaultMails()
        {
            return new List<MailEntry>
            {
                new MailEntry
                {
                    id = "mail_welcome",
                    title = "환영합니다!",
                    body = "로또 디펜스에 오신 것을 환영합니다!\n게임을 즐겨주세요.",
                    type = MailType.Notice,
                    attachments = new List<MailAttachment>()
                },
                new MailEntry
                {
                    id = "mail_launch_reward",
                    title = "런칭 기념 보상",
                    body = "로또 디펜스 런칭을 기념하여 보상을 지급합니다.\n감사합니다!",
                    type = MailType.Reward,
                    attachments = new List<MailAttachment>
                    {
                        new MailAttachment("gold", 200),
                        new MailAttachment("ticket", 3)
                    }
                },
                new MailEntry
                {
                    id = "mail_update_notice",
                    title = "업데이트 안내",
                    body = "새로운 유닛과 스테이지가 추가될 예정입니다.\n기대해 주세요!",
                    type = MailType.Reward,
                    attachments = new List<MailAttachment>
                    {
                        new MailAttachment("gold", 50)
                    }
                }
            };
        }
    }
}
