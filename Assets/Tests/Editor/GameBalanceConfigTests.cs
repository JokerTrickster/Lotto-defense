using NUnit.Framework;
using UnityEngine;
using LottoDefense.Gameplay;
using LottoDefense.Units;
using LottoDefense.Monsters;

namespace LottoDefense.Tests
{
    public class GameBalanceConfigTests
    {
        private GameBalanceConfig config;

        [SetUp]
        public void Setup()
        {
            config = ScriptableObject.CreateInstance<GameBalanceConfig>();
        }

        [TearDown]
        public void TearDown()
        {
            if (config != null) Object.DestroyImmediate(config);
        }

        #region GetLevelUpCost
        [Test]
        public void GetLevelUpCost_NormalRarity_Level1_ReturnsBaseCost()
        {
            // Normal base cost = 20, level 1 → 20 * max(1, 1) = 20
            Assert.AreEqual(20, config.GetLevelUpCost(Rarity.Normal, 1),
                "Normal rarity level 1 cost should be 20");
        }

        [Test]
        public void GetLevelUpCost_NormalRarity_Level5_ReturnsScaledCost()
        {
            // 20 * max(1, 5) = 20 * 5 = 100
            Assert.AreEqual(100, config.GetLevelUpCost(Rarity.Normal, 5),
                "Normal rarity level 5 cost should be 100");
        }

        [Test]
        public void GetLevelUpCost_LegendaryRarity_Level3_ReturnsScaledCost()
        {
            // Legendary base = 200, level 3 → 200 * 3 = 600
            Assert.AreEqual(600, config.GetLevelUpCost(Rarity.Legendary, 3),
                "Legendary rarity level 3 cost should be 600");
        }

        [Test]
        public void GetLevelUpCost_AllRarities_Level1_HaveCorrectBaseCosts()
        {
            Assert.AreEqual(20, config.GetLevelUpCost(Rarity.Normal, 1), "Normal base cost");
            Assert.AreEqual(50, config.GetLevelUpCost(Rarity.Rare, 1), "Rare base cost");
            Assert.AreEqual(100, config.GetLevelUpCost(Rarity.Epic, 1), "Epic base cost");
            Assert.AreEqual(200, config.GetLevelUpCost(Rarity.Legendary, 1), "Legendary base cost");
        }

        [Test]
        public void GetLevelUpCost_ZeroLevel_ClampsToMinimum()
        {
            // max(1, 0) = 1, so cost = 20 * 1 = 20
            Assert.AreEqual(20, config.GetLevelUpCost(Rarity.Normal, 0),
                "Level 0 should clamp to minimum via max(1, 0)");
        }

        [Test]
        public void GetLevelUpCost_NegativeLevel_ClampsToMinimum()
        {
            // max(1, -5) = 1, so cost = 20 * 1 = 20
            Assert.AreEqual(20, config.GetLevelUpCost(Rarity.Normal, -5),
                "Negative level should clamp to 1");
        }
        #endregion

        #region GetLevelAttackMultiplier
        [Test]
        public void GetLevelAttackMultiplier_Level1_Returns1()
        {
            // 1 + 0.1 * (max(1,1) - 1) = 1 + 0 = 1.0
            Assert.AreEqual(1.0f, config.GetLevelAttackMultiplier(1), 0.001f,
                "Level 1 attack multiplier should be 1.0");
        }

        [Test]
        public void GetLevelAttackMultiplier_Level5_ReturnsCorrectValue()
        {
            // 1 + 0.1 * (5 - 1) = 1 + 0.4 = 1.4
            Assert.AreEqual(1.4f, config.GetLevelAttackMultiplier(5), 0.001f,
                "Level 5 attack multiplier should be 1.4");
        }

        [Test]
        public void GetLevelAttackMultiplier_Level10_ReturnsCorrectValue()
        {
            // 1 + 0.1 * (10 - 1) = 1 + 0.9 = 1.9
            Assert.AreEqual(1.9f, config.GetLevelAttackMultiplier(10), 0.001f,
                "Level 10 attack multiplier should be 1.9");
        }

        [Test]
        public void GetLevelAttackMultiplier_LevelZero_ClampsToLevel1()
        {
            // 1 + 0.1 * (max(1,0) - 1) = 1 + 0 = 1.0
            Assert.AreEqual(1.0f, config.GetLevelAttackMultiplier(0), 0.001f,
                "Level 0 should clamp to level 1 (multiplier 1.0)");
        }
        #endregion

        #region GetLevelSpeedMultiplier
        [Test]
        public void GetLevelSpeedMultiplier_Level1_Returns1()
        {
            // 1 + 0.05 * (max(1,1) - 1) = 1.0
            Assert.AreEqual(1.0f, config.GetLevelSpeedMultiplier(1), 0.001f,
                "Level 1 speed multiplier should be 1.0");
        }

        [Test]
        public void GetLevelSpeedMultiplier_Level10_ReturnsCorrectValue()
        {
            // 1 + 0.05 * (10 - 1) = 1 + 0.45 = 1.45
            Assert.AreEqual(1.45f, config.GetLevelSpeedMultiplier(10), 0.001f,
                "Level 10 speed multiplier should be 1.45");
        }
        #endregion

        #region GetSellGold
        [Test]
        public void GetSellGold_AllRarities_ReturnCorrectValues()
        {
            Assert.AreEqual(3, config.GetSellGold(Rarity.Normal), "Normal sell gold");
            Assert.AreEqual(8, config.GetSellGold(Rarity.Rare), "Rare sell gold");
            Assert.AreEqual(20, config.GetSellGold(Rarity.Epic), "Epic sell gold");
            Assert.AreEqual(50, config.GetSellGold(Rarity.Legendary), "Legendary sell gold");
        }

        [Test]
        public void GetSellGold_HigherRarity_IsWorthMore()
        {
            int normal = config.GetSellGold(Rarity.Normal);
            int rare = config.GetSellGold(Rarity.Rare);
            int epic = config.GetSellGold(Rarity.Epic);
            int legendary = config.GetSellGold(Rarity.Legendary);

            Assert.Less(normal, rare, "Normal should sell for less than Rare");
            Assert.Less(rare, epic, "Rare should sell for less than Epic");
            Assert.Less(epic, legendary, "Epic should sell for less than Legendary");
        }
        #endregion

        #region GetGameResultGold
        [Test]
        public void GetGameResultGold_Round0To3_Returns10()
        {
            Assert.AreEqual(10, config.GetGameResultGold(0), "Round 0 reward");
            Assert.AreEqual(10, config.GetGameResultGold(1), "Round 1 reward");
            Assert.AreEqual(10, config.GetGameResultGold(3), "Round 3 reward");
        }

        [Test]
        public void GetGameResultGold_Round4To6_Returns30()
        {
            Assert.AreEqual(30, config.GetGameResultGold(4), "Round 4 reward");
            Assert.AreEqual(30, config.GetGameResultGold(5), "Round 5 reward");
            Assert.AreEqual(30, config.GetGameResultGold(6), "Round 6 reward");
        }

        [Test]
        public void GetGameResultGold_Round7To9_Returns60()
        {
            Assert.AreEqual(60, config.GetGameResultGold(7), "Round 7 reward");
            Assert.AreEqual(60, config.GetGameResultGold(9), "Round 9 reward");
        }

        [Test]
        public void GetGameResultGold_Round10Plus_Returns100()
        {
            Assert.AreEqual(100, config.GetGameResultGold(10), "Round 10 reward");
            Assert.AreEqual(100, config.GetGameResultGold(30), "Round 30 reward");
            Assert.AreEqual(100, config.GetGameResultGold(999), "Round 999 reward");
        }

        [Test]
        public void GetGameResultGold_HigherRound_HigherReward()
        {
            int r1 = config.GetGameResultGold(1);
            int r5 = config.GetGameResultGold(5);
            int r8 = config.GetGameResultGold(8);
            int r10 = config.GetGameResultGold(10);

            Assert.LessOrEqual(r1, r5, "Round 5 should reward >= round 1");
            Assert.LessOrEqual(r5, r8, "Round 8 should reward >= round 5");
            Assert.LessOrEqual(r8, r10, "Round 10 should reward >= round 8");
        }
        #endregion

        #region ValidateSpawnRates
        [Test]
        public void ValidateSpawnRates_Default_ReturnsTrue()
        {
            Assert.IsTrue(config.ValidateSpawnRates(),
                "Default spawn rates (25/25/25/25) should sum to 100%");
        }

        [Test]
        public void ValidateSpawnRates_InvalidSum_ReturnsFalse()
        {
            config.spawnRates.normalRate = 30f;
            Assert.IsFalse(config.ValidateSpawnRates(),
                "Spawn rates summing to != 100 should return false");
        }
        #endregion

        #region Synthesis
        [Test]
        public void GetSynthesisRecipe_Warrior_ReturnsArcher()
        {
            var recipe = config.GetSynthesisRecipe("Warrior");
            Assert.IsNotNull(recipe, "Warrior should have a synthesis recipe");
            Assert.AreEqual("Archer", recipe.resultUnitName,
                "Warrior synthesis result should be Archer");
            Assert.AreEqual(0, recipe.synthesisGoldCost,
                "Warrior synthesis should cost 0 gold");
        }

        [Test]
        public void GetSynthesisRecipe_FullChain_IsCorrect()
        {
            Assert.AreEqual("Archer", config.GetSynthesisRecipe("Warrior").resultUnitName);
            Assert.AreEqual("Mage", config.GetSynthesisRecipe("Archer").resultUnitName);
            Assert.AreEqual("DragonKnight", config.GetSynthesisRecipe("Mage").resultUnitName);
            Assert.AreEqual("Phoenix", config.GetSynthesisRecipe("DragonKnight").resultUnitName);
        }

        [Test]
        public void GetSynthesisRecipe_Phoenix_ReturnsNull()
        {
            var recipe = config.GetSynthesisRecipe("Phoenix");
            Assert.IsNull(recipe, "Phoenix (top of chain) should have no synthesis recipe");
        }

        [Test]
        public void CanSynthesize_ValidUnit_ReturnsTrue()
        {
            Assert.IsTrue(config.CanSynthesize("Warrior"));
            Assert.IsTrue(config.CanSynthesize("Archer"));
            Assert.IsTrue(config.CanSynthesize("Mage"));
            Assert.IsTrue(config.CanSynthesize("DragonKnight"));
        }

        [Test]
        public void CanSynthesize_TopUnit_ReturnsFalse()
        {
            Assert.IsFalse(config.CanSynthesize("Phoenix"),
                "Phoenix cannot be synthesized further");
        }

        [Test]
        public void CanSynthesize_NonexistentUnit_ReturnsFalse()
        {
            Assert.IsFalse(config.CanSynthesize("InvalidUnit"),
                "Unknown unit name should return false");
        }
        #endregion

        #region GetUnitUnlockCost
        [Test]
        public void GetUnitUnlockCost_Warrior_IsFree()
        {
            Assert.AreEqual(0, config.GetUnitUnlockCost("Warrior"),
                "Warrior should be free to unlock");
        }

        [Test]
        public void GetUnitUnlockCost_AllUnits_CorrectPrices()
        {
            Assert.AreEqual(0, config.GetUnitUnlockCost("Warrior"));
            Assert.AreEqual(100, config.GetUnitUnlockCost("Archer"));
            Assert.AreEqual(500, config.GetUnitUnlockCost("Mage"));
            Assert.AreEqual(2000, config.GetUnitUnlockCost("DragonKnight"));
            Assert.AreEqual(3000, config.GetUnitUnlockCost("Phoenix"));
        }

        [Test]
        public void GetUnitUnlockCost_UnknownUnit_ReturnsZero()
        {
            Assert.AreEqual(0, config.GetUnitUnlockCost("NonExistent"),
                "Unknown unit should return 0 cost");
        }

        [Test]
        public void GetUnitUnlockCost_HigherRarity_CostsMore()
        {
            int warrior = config.GetUnitUnlockCost("Warrior");
            int archer = config.GetUnitUnlockCost("Archer");
            int mage = config.GetUnitUnlockCost("Mage");
            int dk = config.GetUnitUnlockCost("DragonKnight");
            int phoenix = config.GetUnitUnlockCost("Phoenix");

            Assert.LessOrEqual(warrior, archer);
            Assert.LessOrEqual(archer, mage);
            Assert.LessOrEqual(mage, dk);
            Assert.LessOrEqual(dk, phoenix);
        }
        #endregion

        #region GetSkillById
        [Test]
        public void GetSkillById_ValidId_ReturnsSkill()
        {
            var skill = config.GetSkillById("critical_strike");
            Assert.IsNotNull(skill, "critical_strike should exist");
            Assert.AreEqual("크리티컬 스트라이크", skill.skillName);
            Assert.AreEqual(SkillType.OnHit, skill.skillType);
            Assert.AreEqual(2.0f, skill.damageMultiplier, 0.01f);
        }

        [Test]
        public void GetSkillById_InvalidId_ReturnsNull()
        {
            var skill = config.GetSkillById("nonexistent_skill");
            Assert.IsNull(skill, "Invalid skill ID should return null");
        }

        [Test]
        public void GetSkillById_AllPresets_AreReachable()
        {
            string[] expectedIds = {
                "critical_strike", "double_shot", "piercing_arrow",
                "chain_lightning", "battle_frenzy", "gold_rush",
                "sniper", "berserker", "rapid_fire", "area_attack",
                "war_cry", "arrow_rain", "meteor", "dragon_fury", "phoenix_flame"
            };

            foreach (var id in expectedIds)
            {
                Assert.IsNotNull(config.GetSkillById(id),
                    $"Skill preset '{id}' should be found");
            }
        }
        #endregion

        #region GetMonsterByType
        [Test]
        public void GetMonsterByType_Normal_ReturnsCorrectStats()
        {
            var monster = config.GetMonsterByType(MonsterType.Normal);
            Assert.IsNotNull(monster);
            Assert.AreEqual(100, monster.maxHealth);
            Assert.AreEqual(5, monster.defense);
            Assert.AreEqual(10, monster.goldReward);
        }

        [Test]
        public void GetMonsterByType_Fast_HasHigherSpeed()
        {
            var normal = config.GetMonsterByType(MonsterType.Normal);
            var fast = config.GetMonsterByType(MonsterType.Fast);
            Assert.Greater(fast.moveSpeed, normal.moveSpeed,
                "Fast monster should be faster than Normal");
        }

        [Test]
        public void GetMonsterByType_Tank_HasHigherHealthAndDefense()
        {
            var normal = config.GetMonsterByType(MonsterType.Normal);
            var tank = config.GetMonsterByType(MonsterType.Tank);
            Assert.Greater(tank.maxHealth, normal.maxHealth,
                "Tank should have more HP than Normal");
            Assert.Greater(tank.defense, normal.defense,
                "Tank should have more defense than Normal");
        }
        #endregion

        #region GetUnitsByRarity
        [Test]
        public void GetUnitsByRarity_Normal_ReturnsWarrior()
        {
            var normals = config.GetUnitsByRarity(Rarity.Normal);
            Assert.AreEqual(1, normals.Count, "Should have 1 Normal unit");
            Assert.AreEqual("Warrior", normals[0].unitName);
        }

        [Test]
        public void GetUnitsByRarity_Legendary_ReturnsTwoUnits()
        {
            var legendaries = config.GetUnitsByRarity(Rarity.Legendary);
            Assert.AreEqual(2, legendaries.Count, "Should have 2 Legendary units");
        }
        #endregion
    }
}
