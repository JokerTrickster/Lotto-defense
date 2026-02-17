using UnityEngine;
using System;
using System.Collections.Generic;
using LottoDefense.Gameplay;
using LottoDefense.Grid;
using LottoDefense.Units;

namespace LottoDefense.Quests
{
    public class QuestManager : MonoBehaviour
    {
        #region Singleton
        private static QuestManager _instance;

        public static QuestManager Instance
        {
            get
            {
                if (GameplayManager.IsCleaningUp) return null;

                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<QuestManager>();

                    if (_instance == null)
                    {
                        GameObject go = new GameObject("QuestManager");
                        _instance = go.AddComponent<QuestManager>();
                    }
                }
                return _instance;
            }
        }
        #endregion

        #region Events
        public event Action<QuestInstance> OnQuestCompleted;
        #endregion

        #region Fields
        private List<QuestInstance> quests = new List<QuestInstance>();
        #endregion

        #region Properties
        public IReadOnlyList<QuestInstance> Quests => quests;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            SubscribeEvents();
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }
        #endregion

        #region Initialization
        public void InitializeQuests()
        {
            quests.Clear();
            QuestDefinition[] definitions = QuestConfig.GetAllQuests();
            foreach (var def in definitions)
            {
                quests.Add(new QuestInstance(def));
            }
        }
        #endregion

        #region Event Subscriptions
        private void SubscribeEvents()
        {
            var placement = UnitPlacementManager.Instance;
            if (placement != null)
            {
                placement.OnUnitPlaced += OnUnitPlaced;
                placement.OnUnitsSwapped += OnUnitsSwapped;
            }
        }

        private void UnsubscribeEvents()
        {
            if (GameplayManager.IsCleaningUp) return;

            var placement = FindFirstObjectByType<UnitPlacementManager>();
            if (placement != null)
            {
                placement.OnUnitPlaced -= OnUnitPlaced;
                placement.OnUnitsSwapped -= OnUnitsSwapped;
            }
        }
        #endregion

        #region Event Handlers
        private void OnUnitPlaced(Unit unit, Vector2Int gridPos)
        {
            CheckAllQuests();
        }

        private void OnUnitsSwapped(Unit unit1, Unit unit2, Vector2Int pos1, Vector2Int pos2)
        {
            CheckAllQuests();
        }
        #endregion

        #region Quest Check Logic
        public void CheckAllQuests()
        {
            foreach (var quest in quests)
            {
                if (quest.State != QuestState.Hidden) continue;

                bool completed = CheckQuest(quest);
                if (completed)
                {
                    quest.State = QuestState.Completed;
                    Debug.Log($"[QuestManager] Quest completed: {quest.Definition.questId}");
                    OnQuestCompleted?.Invoke(quest);
                }
            }
        }

        private bool CheckQuest(QuestInstance quest)
        {
            switch (quest.Definition.questType)
            {
                case QuestType.CollectUnits:
                    return CheckCollectUnits(quest.Definition);
                case QuestType.PositionUnits:
                    return CheckPositionUnits(quest.Definition);
                default:
                    return false;
            }
        }

        private bool CheckCollectUnits(QuestDefinition def)
        {
            var gridManager = GridManager.Instance;
            if (gridManager == null) return false;

            foreach (var condition in def.conditions)
            {
                int placedCount = CountUnitsOnGrid(condition.unitName);
                if (placedCount < condition.count) return false;
            }
            return true;
        }

        private bool CheckPositionUnits(QuestDefinition def)
        {
            var gridManager = GridManager.Instance;
            if (gridManager == null) return false;

            foreach (var condition in def.conditions)
            {
                if (condition.gridPositions == null) return false;

                foreach (var pos in condition.gridPositions)
                {
                    Unit unit = gridManager.GetUnitAt(pos);
                    if (unit == null || unit.Data.unitName != condition.unitName)
                        return false;
                }
            }
            return true;
        }

        private int CountUnitsOnGrid(string unitName)
        {
            var gridManager = GridManager.Instance;
            if (gridManager == null) return 0;

            int count = 0;
            for (int x = 0; x < GridManager.GRID_WIDTH; x++)
            {
                for (int y = 0; y < GridManager.GRID_HEIGHT; y++)
                {
                    Unit unit = gridManager.GetUnitAt(x, y);
                    if (unit != null && unit.Data.unitName == unitName)
                        count++;
                }
            }
            return count;
        }
        #endregion

        #region Reward
        public bool ClaimReward(string questId)
        {
            foreach (var quest in quests)
            {
                if (quest.Definition.questId == questId && quest.State == QuestState.Completed)
                {
                    quest.State = QuestState.Rewarded;
                    GameplayManager.Instance?.ModifyGold(quest.Definition.goldReward);
                    Debug.Log($"[QuestManager] Reward claimed: {questId}, +{quest.Definition.goldReward} gold");
                    return true;
                }
            }
            return false;
        }
        #endregion
    }
}
