using NUnit.Framework;
using UnityEngine;
using LottoDefense.Units;

namespace LottoDefense.Tests
{
    public class UnitUpgradeTests
    {
        private UnitUpgradeManager manager;
        private GameObject managerObj;
        private UnitData defaultData;
        private UnitData phoenixData;

        [SetUp]
        public void Setup()
        {
            managerObj = new GameObject("TestUpgradeManager");
            manager = managerObj.AddComponent<UnitUpgradeManager>();

            defaultData = ScriptableObject.CreateInstance<UnitData>();
            defaultData.unitName = "TestUnit";
            defaultData.rarity = Rarity.Normal;
            defaultData.baseUpgradeCost = 5;
            defaultData.attackUpgradePercent = 10f;
            defaultData.attackSpeedUpgradePercent = 8f;
            defaultData.maxUpgradeLevel = 10;

            phoenixData = ScriptableObject.CreateInstance<UnitData>();
            phoenixData.unitName = "Phoenix";
            phoenixData.rarity = Rarity.Legendary;
            phoenixData.baseUpgradeCost = 60;
            phoenixData.attackUpgradePercent = 12f;
            phoenixData.attackSpeedUpgradePercent = 10f;
            phoenixData.maxUpgradeLevel = 10;
        }

        [TearDown]
        public void TearDown()
        {
            if (defaultData != null) Object.DestroyImmediate(defaultData);
            if (phoenixData != null) Object.DestroyImmediate(phoenixData);
            if (managerObj != null) Object.DestroyImmediate(managerObj);
        }

        #region GetAttackMultiplier
        [Test]
        public void GetAttackMultiplier_Level0_Returns1()
        {
            // 1 + 0 * 0.1 = 1.0
            Assert.AreEqual(1.0f, manager.GetAttackMultiplier(0, defaultData), 0.001f,
                "Level 0 attack multiplier should be 1.0");
        }

        [Test]
        public void GetAttackMultiplier_Level1_Returns1_1()
        {
            // 1 + 1 * 0.1 = 1.1
            Assert.AreEqual(1.1f, manager.GetAttackMultiplier(1, defaultData), 0.001f,
                "Level 1 attack multiplier should be 1.1 (10% increase)");
        }

        [Test]
        public void GetAttackMultiplier_Level5_Returns1_5()
        {
            // 1 + 5 * 0.1 = 1.5
            Assert.AreEqual(1.5f, manager.GetAttackMultiplier(5, defaultData), 0.001f,
                "Level 5 attack multiplier should be 1.5");
        }

        [Test]
        public void GetAttackMultiplier_Level10_Returns2()
        {
            // 1 + 10 * 0.1 = 2.0
            Assert.AreEqual(2.0f, manager.GetAttackMultiplier(10, defaultData), 0.001f,
                "Level 10 attack multiplier should be 2.0 (100% increase)");
        }

        [Test]
        public void GetAttackMultiplier_NullData_UsesDefaultPercent()
        {
            // Default: 0.1 per level → 1 + 3 * 0.1 = 1.3
            Assert.AreEqual(1.3f, manager.GetAttackMultiplier(3, null), 0.001f,
                "Null data should use default 10% per level");
        }

        [Test]
        public void GetAttackMultiplier_PhoenixData_UsesCustomPercent()
        {
            // Phoenix: 12% → 1 + 5 * 0.12 = 1.6
            Assert.AreEqual(1.6f, manager.GetAttackMultiplier(5, phoenixData), 0.001f,
                "Phoenix should use 12% per level");
        }

        [Test]
        public void GetAttackMultiplier_IncreasesMonotonically()
        {
            float previous = 0f;
            for (int level = 0; level <= 10; level++)
            {
                float multiplier = manager.GetAttackMultiplier(level, defaultData);
                Assert.Greater(multiplier, previous,
                    $"Attack multiplier should increase at level {level}");
                previous = multiplier;
            }
        }
        #endregion

        #region GetAttackSpeedMultiplier
        [Test]
        public void GetAttackSpeedMultiplier_Level0_Returns1()
        {
            Assert.AreEqual(1.0f, manager.GetAttackSpeedMultiplier(0, defaultData), 0.001f);
        }

        [Test]
        public void GetAttackSpeedMultiplier_Level1_Returns1_08()
        {
            // 1 + 1 * 0.08 = 1.08
            Assert.AreEqual(1.08f, manager.GetAttackSpeedMultiplier(1, defaultData), 0.001f,
                "Level 1 speed multiplier should be 1.08 (8% increase)");
        }

        [Test]
        public void GetAttackSpeedMultiplier_Level10_Returns1_8()
        {
            // 1 + 10 * 0.08 = 1.8
            Assert.AreEqual(1.8f, manager.GetAttackSpeedMultiplier(10, defaultData), 0.001f,
                "Level 10 speed multiplier should be 1.8 (80% increase)");
        }

        [Test]
        public void GetAttackSpeedMultiplier_NullData_UsesDefault()
        {
            // Default: 0.08 per level → 1 + 5 * 0.08 = 1.4
            Assert.AreEqual(1.4f, manager.GetAttackSpeedMultiplier(5, null), 0.001f,
                "Null data should use default 8% per level");
        }

        [Test]
        public void GetAttackSpeedMultiplier_PhoenixData_UsesCustomPercent()
        {
            // Phoenix: 10% → 1 + 5 * 0.1 = 1.5
            Assert.AreEqual(1.5f, manager.GetAttackSpeedMultiplier(5, phoenixData), 0.001f,
                "Phoenix should use 10% per level for speed");
        }
        #endregion

        #region Rarity Level Storage
        [Test]
        public void GetRarityAttackLevel_Default_ReturnsZero()
        {
            Assert.AreEqual(0, manager.GetRarityAttackLevel(Rarity.Normal),
                "Unset rarity attack level should be 0");
            Assert.AreEqual(0, manager.GetRarityAttackLevel(Rarity.Legendary),
                "Unset rarity attack level should be 0");
        }

        [Test]
        public void GetRaritySpeedLevel_Default_ReturnsZero()
        {
            Assert.AreEqual(0, manager.GetRaritySpeedLevel(Rarity.Normal),
                "Unset rarity speed level should be 0");
        }
        #endregion

        #region UnitData helpers
        [Test]
        public void UnitData_GetDisplayName_FormatsCorrectly()
        {
            Assert.AreEqual("[Normal] TestUnit", defaultData.GetDisplayName());
            Assert.AreEqual("[Legendary] Phoenix", phoenixData.GetDisplayName());
        }

        [Test]
        public void UnitData_GetDPS_CalculatesCorrectly()
        {
            defaultData.attack = 10;
            defaultData.attackSpeed = 1.0f;
            Assert.AreEqual(10f, defaultData.GetDPS(), 0.01f, "DPS = attack * attackSpeed");

            phoenixData.attack = 50;
            phoenixData.attackSpeed = 0.6f;
            Assert.AreEqual(30f, phoenixData.GetDPS(), 0.01f, "Phoenix DPS = 50 * 0.6 = 30");
        }

        [Test]
        public void UnitData_GetRarityColor_AllRaritiesHaveDistinctColors()
        {
            Color normal = UnitData.GetRarityColor(Rarity.Normal);
            Color rare = UnitData.GetRarityColor(Rarity.Rare);
            Color epic = UnitData.GetRarityColor(Rarity.Epic);
            Color legendary = UnitData.GetRarityColor(Rarity.Legendary);

            Assert.AreNotEqual(normal, rare, "Normal and Rare should have different colors");
            Assert.AreNotEqual(rare, epic, "Rare and Epic should have different colors");
            Assert.AreNotEqual(epic, legendary, "Epic and Legendary should have different colors");
        }
        #endregion
    }
}
