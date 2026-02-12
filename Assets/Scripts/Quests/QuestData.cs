using UnityEngine;
using System;

namespace LottoDefense.Quests
{
    public enum QuestType
    {
        CollectUnits,
        PositionUnits
    }

    public enum QuestState
    {
        Hidden,
        Completed,
        Rewarded
    }

    [Serializable]
    public class QuestCondition
    {
        public string unitName;
        public int count;
        public Vector2Int[] gridPositions;

        public QuestCondition(string unitName, int count)
        {
            this.unitName = unitName;
            this.count = count;
            this.gridPositions = null;
        }

        public QuestCondition(string unitName, Vector2Int[] gridPositions)
        {
            this.unitName = unitName;
            this.count = gridPositions.Length;
            this.gridPositions = gridPositions;
        }
    }

    [Serializable]
    public class QuestDefinition
    {
        public string questId;
        public string hintText;
        public string descriptionText;
        public QuestType questType;
        public QuestCondition[] conditions;
        public int goldReward;

        public QuestDefinition(string questId, string hintText, string descriptionText,
            QuestType questType, QuestCondition[] conditions, int goldReward)
        {
            this.questId = questId;
            this.hintText = hintText;
            this.descriptionText = descriptionText;
            this.questType = questType;
            this.conditions = conditions;
            this.goldReward = goldReward;
        }
    }

    public class QuestInstance
    {
        public QuestDefinition Definition { get; private set; }
        public QuestState State { get; set; }

        public QuestInstance(QuestDefinition definition)
        {
            Definition = definition;
            State = QuestState.Hidden;
        }
    }
}
