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

            [Header("스킬")]
            [Tooltip("이 유닛의 스킬 ID 목록 (스킬 프리셋 섹션 참조)")]
            public List<string> skillIds = new List<string>();
        }

        [System.Serializable]
        public class SkillBalance
        {
            [Header("스킬 기본 정보")]
            public string skillName;
            public string description;
            public SkillType skillType;

            [Header("쿨다운 설정")]
            [Tooltip("쿨다운 시간 (초, 0 = 쿨다운 없음)")]
            public float cooldownDuration = 10f;

            [Tooltip("스킬 최초 사용 가능 시간 (초)")]
            public float initialCooldown = 0f;

            [Header("효과 배율")]
            [Tooltip("데미지 배율 (1.5 = 150%)")]
            public float damageMultiplier = 1.0f;

            [Tooltip("범위 배율 (2.0 = 200%)")]
            public float rangeMultiplier = 1.0f;

            [Tooltip("공격속도 배율 (1.5 = 150% 빠름)")]
            public float attackSpeedMultiplier = 1.0f;

            [Header("효과 범위")]
            [Tooltip("효과 지속시간 (초, 버프용)")]
            public float effectDuration = 0f;

            [Tooltip("영향받는 대상 수 (0 = 단일)")]
            public int targetCount = 0;

            [Tooltip("AOE 범위 (0 = AOE 없음)")]
            public float aoeRadius = 0f;
        }

        [Header("=== 유닛 밸런스 ===")]
        public List<UnitBalance> units = new List<UnitBalance>
        {
            // Normal 유닛 - Warrior (근접 전사)
            new UnitBalance
            {
                unitName = "Warrior",
                rarity = Rarity.Normal,
                attack = 10,
                attackSpeed = 1.0f,
                attackRange = 1.5f,
                upgradeCost = 5,
                skillIds = new List<string> { "battle_frenzy", "critical_strike", "war_cry" }
            },

            // Rare 유닛 - Archer (원거리 궁수)
            new UnitBalance
            {
                unitName = "Archer",
                rarity = Rarity.Rare,
                attack = 15,
                attackSpeed = 1.2f,
                attackRange = 3.0f,
                upgradeCost = 10,
                skillIds = new List<string> { "double_shot", "sniper", "arrow_rain" }
            },

            // Epic 유닛 - Mage (광역 마법사)
            new UnitBalance
            {
                unitName = "Mage",
                rarity = Rarity.Epic,
                attack = 25,
                attackSpeed = 0.8f,
                attackRange = 4.0f,
                upgradeCost = 20,
                skillIds = new List<string> { "area_attack", "chain_lightning", "meteor" }
            },

            // Legendary 유닛 - Dragon Knight (전설 기사)
            new UnitBalance
            {
                unitName = "Dragon Knight",
                rarity = Rarity.Legendary,
                attack = 40,
                attackSpeed = 0.7f,
                attackRange = 2.0f,
                upgradeCost = 50,
                skillIds = new List<string> { "berserker", "area_attack", "rapid_fire", "dragon_fury" }
            },

            // Legendary 유닛 - Phoenix (불사조)
            new UnitBalance
            {
                unitName = "Phoenix",
                rarity = Rarity.Legendary,
                attack = 50,
                attackSpeed = 0.6f,
                attackRange = 5.0f,
                upgradeCost = 60,
                skillIds = new List<string> { "area_attack", "chain_lightning", "critical_strike", "phoenix_flame" }
            }
        };
        #endregion

        #region Skill Presets
        [Header("=== 스킬 프리셋 (재사용 가능한 스킬 템플릿) ===")]
        [Tooltip("스킬 ID를 키로 사용하여 여러 유닛이 같은 스킬을 공유할 수 있습니다")]
        public List<SkillPreset> skillPresets = new List<SkillPreset>
        {
            // ===== OnHit 스킬 =====
            new SkillPreset
            {
                skillId = "critical_strike",
                skill = new SkillBalance
                {
                    skillName = "크리티컬 스트라이크",
                    description = "공격 시 2배 데미지",
                    skillType = SkillType.OnHit,
                    cooldownDuration = 3f,
                    damageMultiplier = 2.0f
                }
            },
            new SkillPreset
            {
                skillId = "double_shot",
                skill = new SkillBalance
                {
                    skillName = "더블 샷",
                    description = "공격 시 추가로 1번 더 공격",
                    skillType = SkillType.OnHit,
                    cooldownDuration = 5f,
                    damageMultiplier = 0.5f,
                    targetCount = 1
                }
            },
            new SkillPreset
            {
                skillId = "piercing_arrow",
                skill = new SkillBalance
                {
                    skillName = "관통 화살",
                    description = "공격이 2명의 적을 관통",
                    skillType = SkillType.OnHit,
                    cooldownDuration = 4f,
                    damageMultiplier = 1.0f,
                    targetCount = 2
                }
            },

            // ===== OnKill 스킬 =====
            new SkillPreset
            {
                skillId = "chain_lightning",
                skill = new SkillBalance
                {
                    skillName = "연쇄 번개",
                    description = "처치 시 주변 적 3명에게 50% 데미지",
                    skillType = SkillType.OnKill,
                    cooldownDuration = 2f,
                    damageMultiplier = 0.5f,
                    targetCount = 3,
                    aoeRadius = 2.0f
                }
            },
            new SkillPreset
            {
                skillId = "battle_frenzy",
                skill = new SkillBalance
                {
                    skillName = "전투 광기",
                    description = "처치 시 3초간 공속 50% 증가",
                    skillType = SkillType.OnKill,
                    cooldownDuration = 1f,
                    attackSpeedMultiplier = 1.5f,
                    effectDuration = 3f
                }
            },
            new SkillPreset
            {
                skillId = "gold_rush",
                skill = new SkillBalance
                {
                    skillName = "골드 러시",
                    description = "처치 시 추가 골드 +3",
                    skillType = SkillType.OnKill,
                    cooldownDuration = 0f,
                    damageMultiplier = 1.0f
                }
            },

            // ===== Passive 스킬 =====
            new SkillPreset
            {
                skillId = "sniper",
                skill = new SkillBalance
                {
                    skillName = "저격수",
                    description = "사거리 +50%",
                    skillType = SkillType.Passive,
                    cooldownDuration = 0f,
                    rangeMultiplier = 1.5f
                }
            },
            new SkillPreset
            {
                skillId = "berserker",
                skill = new SkillBalance
                {
                    skillName = "버서커",
                    description = "공격력 +30%",
                    skillType = SkillType.Passive,
                    cooldownDuration = 0f,
                    damageMultiplier = 1.3f
                }
            },
            new SkillPreset
            {
                skillId = "rapid_fire",
                skill = new SkillBalance
                {
                    skillName = "속사",
                    description = "공격속도 +50%",
                    skillType = SkillType.Passive,
                    cooldownDuration = 0f,
                    attackSpeedMultiplier = 1.5f
                }
            },
            new SkillPreset
            {
                skillId = "area_attack",
                skill = new SkillBalance
                {
                    skillName = "광역 공격",
                    description = "공격이 범위 1.5 내 모든 적 명중",
                    skillType = SkillType.Passive,
                    cooldownDuration = 0f,
                    aoeRadius = 1.5f,
                    targetCount = 5
                }
            },

            // ===== Active 스킬 (마나 충전 후 자동 발동) =====
            new SkillPreset
            {
                skillId = "war_cry",
                skill = new SkillBalance
                {
                    skillName = "전사의 함성",
                    description = "3초간 공격력 2배",
                    skillType = SkillType.Active,
                    cooldownDuration = 10f,
                    damageMultiplier = 2.0f,
                    effectDuration = 3f
                }
            },
            new SkillPreset
            {
                skillId = "arrow_rain",
                skill = new SkillBalance
                {
                    skillName = "화살 비",
                    description = "4초간 공격속도 2배",
                    skillType = SkillType.Active,
                    cooldownDuration = 12f,
                    attackSpeedMultiplier = 2.0f,
                    effectDuration = 4f
                }
            },
            new SkillPreset
            {
                skillId = "meteor",
                skill = new SkillBalance
                {
                    skillName = "메테오",
                    description = "5초간 공격력 3배",
                    skillType = SkillType.Active,
                    cooldownDuration = 15f,
                    damageMultiplier = 3.0f,
                    effectDuration = 5f
                }
            },
            new SkillPreset
            {
                skillId = "dragon_fury",
                skill = new SkillBalance
                {
                    skillName = "용의 분노",
                    description = "5초간 공격력 2.5배 + 공속 1.5배",
                    skillType = SkillType.Active,
                    cooldownDuration = 15f,
                    damageMultiplier = 2.5f,
                    attackSpeedMultiplier = 1.5f,
                    effectDuration = 5f
                }
            },
            new SkillPreset
            {
                skillId = "phoenix_flame",
                skill = new SkillBalance
                {
                    skillName = "불사조의 불꽃",
                    description = "6초간 공격력 3배 + 공속 2배",
                    skillType = SkillType.Active,
                    cooldownDuration = 12f,
                    damageMultiplier = 3.0f,
                    attackSpeedMultiplier = 2.0f,
                    effectDuration = 6f
                }
            }
        };

        [System.Serializable]
        public class SkillPreset
        {
            [Tooltip("스킬 고유 ID (유닛이 참조할 때 사용)")]
            public string skillId;

            [Tooltip("스킬 밸런스 데이터")]
            public SkillBalance skill;
        }
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
            public int maxRounds = 10;

            [Header("스케일링 커브")]
            [Tooltip("라운드별 체력 배율 (X: 0~1 정규화된 라운드, Y: 배율)")]
            public AnimationCurve healthScaling = AnimationCurve.Linear(0f, 1f, 1f, 5f);

            [Tooltip("라운드별 방어력 배율 (X: 0~1 정규화된 라운드, Y: 배율)")]
            public AnimationCurve defenseScaling = AnimationCurve.Linear(0f, 1f, 1f, 3f);
        }

        [Header("=== 난이도 밸런스 ===")]
        public DifficultyBalance difficulty = new DifficultyBalance
        {
            maxRounds = 10
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

        #region Synthesis System
        [System.Serializable]
        public class SynthesisRecipe
        {
            [Tooltip("합성할 유닛 이름 (2개 필요)")]
            public string sourceUnitName;

            [Tooltip("합성 결과 유닛 이름")]
            public string resultUnitName;

            [Tooltip("합성 비용 (골드)")]
            public int synthesisGoldCost = 0;
        }

        [Header("=== 합성 시스템 ===")]
        [Tooltip("유닛 합성 레시피 (같은 유닛 2개 → 상위 유닛)")]
        public List<SynthesisRecipe> synthesisRecipes = new List<SynthesisRecipe>
        {
            // Normal → Rare
            new SynthesisRecipe
            {
                sourceUnitName = "Warrior",
                resultUnitName = "Archer",
                synthesisGoldCost = 0
            },

            // Rare → Epic
            new SynthesisRecipe
            {
                sourceUnitName = "Archer",
                resultUnitName = "Mage",
                synthesisGoldCost = 0
            },

            // Epic → Legendary
            new SynthesisRecipe
            {
                sourceUnitName = "Mage",
                resultUnitName = "Dragon Knight",
                synthesisGoldCost = 0
            },

            // Legendary → Legendary (Dragon Knight → Phoenix)
            new SynthesisRecipe
            {
                sourceUnitName = "Dragon Knight",
                resultUnitName = "Phoenix",
                synthesisGoldCost = 0
            }
        };

        [Header("=== 판매 시스템 ===")]
        [Tooltip("유닛 판매 시 획득 골드 (기본값, 등급별 미지정 시 사용)")]
        public int unitSellGold = 3;

        [Tooltip("Normal 등급 판매 골드")]
        public int sellGoldNormal = 3;
        [Tooltip("Rare 등급 판매 골드")]
        public int sellGoldRare = 8;
        [Tooltip("Epic 등급 판매 골드")]
        public int sellGoldEpic = 20;
        [Tooltip("Legendary 등급 판매 골드")]
        public int sellGoldLegendary = 50;
        #endregion

        #region Unit Shop Prices
        [System.Serializable]
        public class UnitShopPrice
        {
            public string unitName;
            public int goldCost;
        }

        [Header("=== 유닛 상점 가격 ===")]
        public List<UnitShopPrice> unitShopPrices = new List<UnitShopPrice>
        {
            new UnitShopPrice { unitName = "Warrior", goldCost = 0 },
            new UnitShopPrice { unitName = "Archer", goldCost = 100 },
            new UnitShopPrice { unitName = "Mage", goldCost = 500 },
            new UnitShopPrice { unitName = "Dragon Knight", goldCost = 2000 },
            new UnitShopPrice { unitName = "Phoenix", goldCost = 3000 }
        };

        public int GetUnitUnlockCost(string unitName)
        {
            var price = unitShopPrices.Find(p => p.unitName == unitName);
            return price != null ? price.goldCost : 0;
        }
        #endregion

        #region Daily Reward Stages
        [System.Serializable]
        public class DailyRewardStage
        {
            public int requiredClears;
            public int goldReward;
            public int ticketReward;
        }

        [Header("=== 일일 보상 단계 ===")]
        public List<DailyRewardStage> dailyRewardStages = new List<DailyRewardStage>
        {
            new DailyRewardStage { requiredClears = 1, goldReward = 50, ticketReward = 0 },
            new DailyRewardStage { requiredClears = 3, goldReward = 0, ticketReward = 1 },
            new DailyRewardStage { requiredClears = 5, goldReward = 150, ticketReward = 0 },
            new DailyRewardStage { requiredClears = 10, goldReward = 200, ticketReward = 2 }
        };
        #endregion

        #region Game Result Rewards
        [System.Serializable]
        public class GameResultReward
        {
            public int minRound;
            public int maxRound;
            public int goldReward;
        }

        [Header("=== 게임 결과 보상 ===")]
        public List<GameResultReward> gameResultRewards = new List<GameResultReward>
        {
            new GameResultReward { minRound = 0, maxRound = 3, goldReward = 10 },
            new GameResultReward { minRound = 4, maxRound = 6, goldReward = 30 },
            new GameResultReward { minRound = 7, maxRound = 9, goldReward = 60 },
            new GameResultReward { minRound = 10, maxRound = 999, goldReward = 100 }
        };

        public int GetGameResultGold(int roundReached)
        {
            for (int i = gameResultRewards.Count - 1; i >= 0; i--)
            {
                if (roundReached >= gameResultRewards[i].minRound)
                    return gameResultRewards[i].goldReward;
            }
            return 10;
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// 등급별 판매 골드 반환.
        /// </summary>
        public int GetSellGold(Rarity rarity)
        {
            return rarity switch
            {
                Rarity.Normal => sellGoldNormal,
                Rarity.Rare => sellGoldRare,
                Rarity.Epic => sellGoldEpic,
                Rarity.Legendary => sellGoldLegendary,
                _ => unitSellGold
            };
        }

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
        /// 스킬 ID로 스킬 밸런스 데이터 가져오기
        /// </summary>
        public SkillBalance GetSkillById(string skillId)
        {
            var preset = skillPresets.Find(p => p.skillId == skillId);
            return preset?.skill;
        }

        /// <summary>
        /// 유닛의 모든 스킬 가져오기
        /// </summary>
        public List<SkillBalance> GetUnitSkills(UnitBalance unitBalance)
        {
            var skills = new List<SkillBalance>();
            if (unitBalance.skillIds != null)
            {
                foreach (var skillId in unitBalance.skillIds)
                {
                    var skill = GetSkillById(skillId);
                    if (skill != null)
                    {
                        skills.Add(skill);
                    }
                    else
                    {
                        Debug.LogWarning($"[GameBalanceConfig] 스킬 ID '{skillId}'를 찾을 수 없습니다!");
                    }
                }
            }
            return skills;
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

        /// <summary>
        /// 합성 레시피 찾기
        /// </summary>
        public SynthesisRecipe GetSynthesisRecipe(string sourceUnitName)
        {
            return synthesisRecipes.Find(r => r.sourceUnitName == sourceUnitName);
        }

        /// <summary>
        /// 합성 가능 여부 확인
        /// </summary>
        public bool CanSynthesize(string unitName)
        {
            return GetSynthesisRecipe(unitName) != null;
        }
        #endregion
    }
}
