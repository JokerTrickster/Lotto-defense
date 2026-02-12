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
                    "collect_archer_3",
                    "궁수의 부대",
                    "기본 궁수 3마리 배치",
                    QuestType.CollectUnits,
                    new QuestCondition[] { new QuestCondition("기본 궁수", 3) },
                    30
                ),
                new QuestDefinition(
                    "collect_swordsman_3",
                    "검의 군단",
                    "검사 3마리 배치",
                    QuestType.CollectUnits,
                    new QuestCondition[] { new QuestCondition("검사", 3) },
                    30
                ),
                new QuestDefinition(
                    "collect_mage_2",
                    "마법의 힘",
                    "마법사 2마리 배치",
                    QuestType.CollectUnits,
                    new QuestCondition[] { new QuestCondition("마법사", 2) },
                    50
                ),
                new QuestDefinition(
                    "collect_sniper_2",
                    "정밀 사격",
                    "저격수 2마리 배치",
                    QuestType.CollectUnits,
                    new QuestCondition[] { new QuestCondition("저격수", 2) },
                    50
                ),
                new QuestDefinition(
                    "collect_mixed_army",
                    "다양한 전력",
                    "기본 궁수 2마리 + 검사 2마리 배치",
                    QuestType.CollectUnits,
                    new QuestCondition[]
                    {
                        new QuestCondition("기본 궁수", 2),
                        new QuestCondition("검사", 2)
                    },
                    40
                ),

                // PositionUnits quests
                new QuestDefinition(
                    "position_mage_corners",
                    "마법진 배치",
                    "마법사를 (0,0)과 (7,3)에 배치",
                    QuestType.PositionUnits,
                    new QuestCondition[]
                    {
                        new QuestCondition("마법사", new Vector2Int[] { new Vector2Int(0, 0), new Vector2Int(7, 3) })
                    },
                    80
                ),
                new QuestDefinition(
                    "position_archer_frontline",
                    "전방 궁수대",
                    "기본 궁수를 (0,0), (0,1), (0,2)에 배치",
                    QuestType.PositionUnits,
                    new QuestCondition[]
                    {
                        new QuestCondition("기본 궁수", new Vector2Int[] { new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(0, 2) })
                    },
                    60
                ),
            };
        }
    }
}
