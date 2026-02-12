using UnityEngine;

namespace LottoDefense.Quests
{
    public static class QuestConfig
    {
        public static QuestDefinition[] GetAllQuests()
        {
            return new QuestDefinition[]
            {
                // CollectUnits quests
                new QuestDefinition(
                    "collect_warrior_3",
                    "전사의 부대",
                    "Warrior 3마리 배치",
                    QuestType.CollectUnits,
                    new QuestCondition[] { new QuestCondition("Warrior", 3) },
                    30
                ),
                new QuestDefinition(
                    "collect_archer_3",
                    "궁수의 군단",
                    "Archer 3마리 배치",
                    QuestType.CollectUnits,
                    new QuestCondition[] { new QuestCondition("Archer", 3) },
                    30
                ),
                new QuestDefinition(
                    "collect_mage_2",
                    "마법의 힘",
                    "Mage 2마리 배치",
                    QuestType.CollectUnits,
                    new QuestCondition[] { new QuestCondition("Mage", 2) },
                    50
                ),
                new QuestDefinition(
                    "collect_dragon_knight_2",
                    "전설의 기사단",
                    "Dragon Knight 2마리 배치",
                    QuestType.CollectUnits,
                    new QuestCondition[] { new QuestCondition("Dragon Knight", 2) },
                    100
                ),
                new QuestDefinition(
                    "collect_mixed_army",
                    "다양한 전력",
                    "Warrior 2마리 + Archer 2마리 배치",
                    QuestType.CollectUnits,
                    new QuestCondition[]
                    {
                        new QuestCondition("Warrior", 2),
                        new QuestCondition("Archer", 2)
                    },
                    40
                ),

                // PositionUnits quests
                new QuestDefinition(
                    "position_mage_corners",
                    "마법진 배치",
                    "Mage를 (0,0)과 (7,3)에 배치",
                    QuestType.PositionUnits,
                    new QuestCondition[]
                    {
                        new QuestCondition("Mage", new Vector2Int[] { new Vector2Int(0, 0), new Vector2Int(7, 3) })
                    },
                    80
                ),
                new QuestDefinition(
                    "position_warrior_frontline",
                    "전방 전사대",
                    "Warrior를 (0,0), (0,1), (0,2)에 배치",
                    QuestType.PositionUnits,
                    new QuestCondition[]
                    {
                        new QuestCondition("Warrior", new Vector2Int[] { new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(0, 2) })
                    },
                    60
                ),
            };
        }
    }
}
