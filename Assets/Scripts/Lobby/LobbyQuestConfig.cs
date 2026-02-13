using System.Collections.Generic;

namespace LottoDefense.Lobby
{
    public enum LobbyQuestType
    {
        GameClear,
        RoundReach,
        UnitUnlock,
        Synthesis,
        Upgrade
    }

    [System.Serializable]
    public class QuestReward
    {
        public string itemType; // "gold" or "ticket"
        public int amount;

        public QuestReward(string type, int amt)
        {
            itemType = type;
            amount = amt;
        }
    }

    [System.Serializable]
    public class LobbyQuestDefinition
    {
        public string id;
        public string title;
        public string description;
        public LobbyQuestType type;
        public int targetCount;
        public List<QuestReward> rewards;
        public bool isDaily;
    }

    public static class LobbyQuestConfig
    {
        public static List<LobbyQuestDefinition> GetAllQuests()
        {
            return new List<LobbyQuestDefinition>
            {
                // Permanent quests
                new LobbyQuestDefinition
                {
                    id = "Q001",
                    title = "첫 승리",
                    description = "게임 1회 클리어",
                    type = LobbyQuestType.GameClear,
                    targetCount = 1,
                    rewards = new List<QuestReward> { new QuestReward("gold", 100) },
                    isDaily = false
                },
                new LobbyQuestDefinition
                {
                    id = "Q002",
                    title = "견습 용사",
                    description = "게임 5회 클리어",
                    type = LobbyQuestType.GameClear,
                    targetCount = 5,
                    rewards = new List<QuestReward> { new QuestReward("ticket", 2) },
                    isDaily = false
                },
                new LobbyQuestDefinition
                {
                    id = "Q003",
                    title = "숙련된 지휘관",
                    description = "게임 20회 클리어",
                    type = LobbyQuestType.GameClear,
                    targetCount = 20,
                    rewards = new List<QuestReward> { new QuestReward("gold", 500) },
                    isDaily = false
                },
                new LobbyQuestDefinition
                {
                    id = "Q004",
                    title = "수집가",
                    description = "유닛 전종 해금 (4종)",
                    type = LobbyQuestType.UnitUnlock,
                    targetCount = 4,
                    rewards = new List<QuestReward>
                    {
                        new QuestReward("ticket", 5),
                        new QuestReward("gold", 1000)
                    },
                    isDaily = false
                },
                new LobbyQuestDefinition
                {
                    id = "Q005",
                    title = "조합의 달인",
                    description = "조합 50회 수행",
                    type = LobbyQuestType.Synthesis,
                    targetCount = 50,
                    rewards = new List<QuestReward> { new QuestReward("gold", 300) },
                    isDaily = false
                },

                // Daily quests
                new LobbyQuestDefinition
                {
                    id = "Q006",
                    title = "일일: 오늘의 전투",
                    description = "게임 3회 클리어",
                    type = LobbyQuestType.GameClear,
                    targetCount = 3,
                    rewards = new List<QuestReward> { new QuestReward("gold", 30) },
                    isDaily = true
                },
                new LobbyQuestDefinition
                {
                    id = "Q007",
                    title = "일일: 강화 훈련",
                    description = "업그레이드 5회 수행",
                    type = LobbyQuestType.Upgrade,
                    targetCount = 5,
                    rewards = new List<QuestReward> { new QuestReward("ticket", 1) },
                    isDaily = true
                }
            };
        }
    }
}
