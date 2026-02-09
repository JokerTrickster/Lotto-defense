using UnityEngine;
using System.Collections.Generic;
using LottoDefense.Units;
using LottoDefense.Monsters;

namespace LottoDefense.Gameplay
{
    /// <summary>
    /// 중앙화된 게임 밸런스 설정 파일.
    /// 모든 유닛, 몬스터, 난이도, 확률, 게임 룰을 한 곳에서 관리합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "GameBalanceConfig", menuName = "LottoDefense/Game Balance Config")]
    public class GameBalanceConfig : ScriptableObject
    {
        #region Unit Balance
        [System.Serializable]
        public class UnitBalance
        {
            [Header("기본 정보")]
            public string unitName;
            public Rarity rarity;

            [Header("전투 스탯")]
            [Tooltip("기본 공격력")]
            public int attack = 10;

            [Tooltip("초당 공격 횟수")]
            public float attackSpeed = 1.0f;

            [Tooltip("공격 사거리 (유닛 단위)")]
            public float attackRange = 3.0f;

            [Header("업그레이드")]
            [Tooltip("업그레이드 비용")]
            public int upgradeCost = 5;
        }

        [Header("=== 유닛 밸런스 ===")]
        public List<UnitBalance> units = new List<UnitBalance>
        {
            // Normal 유닛 (25%)
            new UnitBalance { unitName = "기본 궁수", rarity = Rarity.Normal, attack = 10, attackSpeed = 1.0f, attackRange = 3.0f, upgradeCost = 5 },
            new UnitBalance { unitName = "검사", rarity = Rarity.Normal, attack = 15, attackSpeed = 0.8f, attackRange = 1.5f, upgradeCost = 5 },

            // Rare 유닛 (25%)
            new UnitBalance { unitName = "강화 궁수", rarity = Rarity.Rare, attack = 20, attackSpeed = 1.2f, attackRange = 4.0f, upgradeCost = 10 },
            new UnitBalance { unitName = "마법사", rarity = Rarity.Rare, attack = 30, attackSpeed = 0.6f, attackRange = 5.0f, upgradeCost = 10 },

            // Epic 유닛 (25%)
            new UnitBalance { unitName = "저격수", rarity = Rarity.Epic, attack = 50, attackSpeed = 0.5f, attackRange = 6.0f, upgradeCost = 20 },
            new UnitBalance { unitName = "대마법사", rarity = Rarity.Epic, attack = 60, attackSpeed = 0.7f, attackRange = 5.0f, upgradeCost = 20 },

            // Legendary 유닛 (25%)
            new UnitBalance { unitName = "드래곤 아처", rarity = Rarity.Legendary, attack = 100, attackSpeed = 1.5f, attackRange = 7.0f, upgradeCost = 50 },
            new UnitBalance { unitName = "대현자", rarity = Rarity.Legendary, attack = 150, attackSpeed = 1.0f, attackRange = 6.0f, upgradeCost = 50 }
        };
        #endregion

        #region Monster Balance
        [System.Serializable]
        public class MonsterBalance
        {
            [Header("기본 정보")]
            public string monsterName;
            public MonsterType type;

            [Header("전투 스탯")]
            [Tooltip("최대 체력")]
            public int maxHealth = 100;

            [Tooltip("공격력 (현재 미사용)")]
            public int attack = 10;

            [Tooltip("방어력 (피해 감소)")]
            public int defense = 5;

            [Tooltip("이동 속도 (유닛/초)")]
            public float moveSpeed = 2.0f;

            [Header("보상")]
            [Tooltip("처치 시 골드")]
            public int goldReward = 10;

            [Header("스케일링")]
            [Tooltip("라운드당 체력 증가율 (1.1 = 10% 증가)")]
            public float healthScaling = 1.1f;

            [Tooltip("라운드당 방어력 증가율 (1.05 = 5% 증가)")]
            public float defenseScaling = 1.05f;
        }

        [Header("=== 몬스터 밸런스 ===")]
        public List<MonsterBalance> monsters = new List<MonsterBalance>
        {
            new MonsterBalance
            {
                monsterName = "기본 몬스터",
                type = MonsterType.Normal,
                maxHealth = 100,
                attack = 10,
                defense = 5,
                moveSpeed = 2.0f,
                goldReward = 10,
                healthScaling = 1.1f,
                defenseScaling = 1.05f
            },
            new MonsterBalance
            {
                monsterName = "빠른 몬스터",
                type = MonsterType.Fast,
                maxHealth = 70,
                attack = 8,
                defense = 3,
                moveSpeed = 4.0f,
                goldReward = 8,
                healthScaling = 1.08f,
                defenseScaling = 1.03f
            },
            new MonsterBalance
            {
                monsterName = "탱크 몬스터",
                type = MonsterType.Tank,
                maxHealth = 200,
                attack = 15,
                defense = 10,
                moveSpeed = 1.5f,
                goldReward = 15,
                healthScaling = 1.12f,
                defenseScaling = 1.06f
            }
        };
        #endregion

        #region Difficulty Balance
        [System.Serializable]
        public class DifficultyBalance
        {
            [Header("라운드 설정")]
            [Tooltip("최대 라운드 수")]
            public int maxRounds = 5;

            [Header("스케일링 커브")]
            [Tooltip("라운드별 체력 배율 (X: 0~1 정규화된 라운드, Y: 배율)")]
            public AnimationCurve healthScaling = AnimationCurve.Linear(0f, 1f, 1f, 5f);

            [Tooltip("라운드별 방어력 배율 (X: 0~1 정규화된 라운드, Y: 배율)")]
            public AnimationCurve defenseScaling = AnimationCurve.Linear(0f, 1f, 1f, 3f);
        }

        [Header("=== 난이도 밸런스 ===")]
        public DifficultyBalance difficulty = new DifficultyBalance
        {
            maxRounds = 5
        };
        #endregion

        #region Spawn Rates
        [System.Serializable]
        public class SpawnRates
        {
            [Header("유닛 등급 확률 (합계 100%)")]
            [Range(0f, 100f)]
            [Tooltip("일반 등급 확률")]
            public float normalRate = 25f;

            [Range(0f, 100f)]
            [Tooltip("희귀 등급 확률")]
            public float rareRate = 25f;

            [Range(0f, 100f)]
            [Tooltip("영웅 등급 확률")]
            public float epicRate = 25f;

            [Range(0f, 100f)]
            [Tooltip("전설 등급 확률")]
            public float legendaryRate = 25f;
        }

        [Header("=== 소환 확률 ===")]
        public SpawnRates spawnRates = new SpawnRates
        {
            normalRate = 25f,
            rareRate = 25f,
            epicRate = 25f,
            legendaryRate = 25f
        };
        #endregion

        #region Game Rules
        [System.Serializable]
        public class GameRules
        {
            [Header("타이밍")]
            [Tooltip("준비 시간 (초)")]
            public int preparationTime = 15;

            [Tooltip("전투 시간 (초)")]
            public int combatTime = 30;

            [Header("경제")]
            [Tooltip("시작 골드")]
            public int startingGold = 30;

            [Tooltip("유닛 소환 비용")]
            public int summonCost = 5;

            [Header("몬스터")]
            [Tooltip("게임 오버 몬스터 수")]
            public int maxMonsterCount = 100;

            [Tooltip("스폰 속도 (초당 몬스터 수)")]
            public float spawnRate = 2.0f;
        }

        [Header("=== 게임 룰 ===")]
        public GameRules gameRules = new GameRules
        {
            preparationTime = 15,
            combatTime = 30,
            startingGold = 30,
            summonCost = 5,
            maxMonsterCount = 100,
            spawnRate = 2.0f
        };
        #endregion

        #region Helper Methods
        /// <summary>
        /// 특정 등급의 유닛 리스트 가져오기
        /// </summary>
        public List<UnitBalance> GetUnitsByRarity(Rarity rarity)
        {
            return units.FindAll(u => u.rarity == rarity);
        }

        /// <summary>
        /// 특정 타입의 몬스터 가져오기
        /// </summary>
        public MonsterBalance GetMonsterByType(MonsterType type)
        {
            return monsters.Find(m => m.type == type);
        }

        /// <summary>
        /// 등급별 확률 검증 (합계가 100%인지)
        /// </summary>
        public bool ValidateSpawnRates()
        {
            float total = spawnRates.normalRate + spawnRates.rareRate +
                         spawnRates.epicRate + spawnRates.legendaryRate;
            return Mathf.Approximately(total, 100f);
        }

        /// <summary>
        /// 에디터에서 값 변경 시 검증
        /// </summary>
        private void OnValidate()
        {
            // 확률 합계 검증
            float total = spawnRates.normalRate + spawnRates.rareRate +
                         spawnRates.epicRate + spawnRates.legendaryRate;

            if (!Mathf.Approximately(total, 100f))
            {
                Debug.LogWarning($"[GameBalanceConfig] 소환 확률 합계가 100%가 아닙니다! (현재: {total}%)");
            }

            // 난이도 커브 검증
            if (difficulty.healthScaling == null || difficulty.healthScaling.length == 0)
            {
                Debug.LogWarning("[GameBalanceConfig] 체력 스케일링 커브가 비어있습니다!");
                difficulty.healthScaling = AnimationCurve.Linear(0f, 1f, 1f, 5f);
            }

            if (difficulty.defenseScaling == null || difficulty.defenseScaling.length == 0)
            {
                Debug.LogWarning("[GameBalanceConfig] 방어력 스케일링 커브가 비어있습니다!");
                difficulty.defenseScaling = AnimationCurve.Linear(0f, 1f, 1f, 3f);
            }
        }
        #endregion
    }
}
