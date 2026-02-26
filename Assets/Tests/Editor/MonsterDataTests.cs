using NUnit.Framework;
using UnityEngine;
using LottoDefense.Monsters;

namespace LottoDefense.Tests
{
    public class MonsterDataTests
    {
        private MonsterData normalMonster;
        private MonsterData fastMonster;
        private MonsterData tankMonster;

        [SetUp]
        public void Setup()
        {
            normalMonster = ScriptableObject.CreateInstance<MonsterData>();
            normalMonster.monsterName = "기본 몬스터";
            normalMonster.type = MonsterType.Normal;
            normalMonster.maxHealth = 100;
            normalMonster.defense = 5;
            normalMonster.healthScaling = 1.1f;
            normalMonster.defenseScaling = 1.05f;

            fastMonster = ScriptableObject.CreateInstance<MonsterData>();
            fastMonster.monsterName = "빠른 몬스터";
            fastMonster.type = MonsterType.Fast;
            fastMonster.maxHealth = 70;
            fastMonster.defense = 3;
            fastMonster.healthScaling = 1.08f;
            fastMonster.defenseScaling = 1.03f;

            tankMonster = ScriptableObject.CreateInstance<MonsterData>();
            tankMonster.monsterName = "탱크 몬스터";
            tankMonster.type = MonsterType.Tank;
            tankMonster.maxHealth = 200;
            tankMonster.defense = 10;
            tankMonster.healthScaling = 1.12f;
            tankMonster.defenseScaling = 1.06f;
        }

        [TearDown]
        public void TearDown()
        {
            if (normalMonster != null) Object.DestroyImmediate(normalMonster);
            if (fastMonster != null) Object.DestroyImmediate(fastMonster);
            if (tankMonster != null) Object.DestroyImmediate(tankMonster);
        }

        [Test]
        public void GetScaledHealth_Round1_ReturnsBaseHealth()
        {
            Assert.AreEqual(100, normalMonster.GetScaledHealth(1),
                "Round 1 should return base maxHealth for Normal");
            Assert.AreEqual(70, fastMonster.GetScaledHealth(1),
                "Round 1 should return base maxHealth for Fast");
            Assert.AreEqual(200, tankMonster.GetScaledHealth(1),
                "Round 1 should return base maxHealth for Tank");
        }

        [Test]
        public void GetScaledDefense_Round1_ReturnsBaseDefense()
        {
            Assert.AreEqual(5, normalMonster.GetScaledDefense(1),
                "Round 1 should return base defense for Normal");
            Assert.AreEqual(3, fastMonster.GetScaledDefense(1),
                "Round 1 should return base defense for Fast");
            Assert.AreEqual(10, tankMonster.GetScaledDefense(1),
                "Round 1 should return base defense for Tank");
        }

        [Test]
        public void GetScaledHealth_Round5_AppliesScalingFormula()
        {
            // Normal: 100 * 1.1^4 = 100 * 1.4641 = 146 (rounded)
            int expected = Mathf.RoundToInt(100 * Mathf.Pow(1.1f, 4));
            Assert.AreEqual(expected, normalMonster.GetScaledHealth(5),
                $"Round 5 Normal HP should be {expected} (100 * 1.1^4)");

            // Fast: 70 * 1.08^4 = 70 * 1.36049 = 95 (rounded)
            int expectedFast = Mathf.RoundToInt(70 * Mathf.Pow(1.08f, 4));
            Assert.AreEqual(expectedFast, fastMonster.GetScaledHealth(5),
                $"Round 5 Fast HP should be {expectedFast}");

            // Tank: 200 * 1.12^4 = 200 * 1.57352 = 315 (rounded)
            int expectedTank = Mathf.RoundToInt(200 * Mathf.Pow(1.12f, 4));
            Assert.AreEqual(expectedTank, tankMonster.GetScaledHealth(5),
                $"Round 5 Tank HP should be {expectedTank}");
        }

        [Test]
        public void GetScaledDefense_Round5_AppliesScalingFormula()
        {
            // Normal: 5 * 1.05^4 = 5 * 1.21551 = 6 (rounded)
            int expected = Mathf.RoundToInt(5 * Mathf.Pow(1.05f, 4));
            Assert.AreEqual(expected, normalMonster.GetScaledDefense(5),
                $"Round 5 Normal defense should be {expected}");
        }

        [Test]
        public void GetScaledHealth_Round10_ScalesSignificantly()
        {
            // Normal: 100 * 1.1^9 = 100 * 2.3579 = 236
            int expected = Mathf.RoundToInt(100 * Mathf.Pow(1.1f, 9));
            Assert.AreEqual(expected, normalMonster.GetScaledHealth(10),
                $"Round 10 Normal HP should be {expected}");

            // Tank: 200 * 1.12^9 = 200 * 2.7731 = 555
            int expectedTank = Mathf.RoundToInt(200 * Mathf.Pow(1.12f, 9));
            Assert.AreEqual(expectedTank, tankMonster.GetScaledHealth(10),
                $"Round 10 Tank HP should be {expectedTank}");
        }

        [Test]
        public void GetScaledHealth_RoundZeroOrNegative_ReturnsBaseHealth()
        {
            Assert.AreEqual(100, normalMonster.GetScaledHealth(0),
                "Round 0 should return base health (clamped to round <= 1)");
            Assert.AreEqual(100, normalMonster.GetScaledHealth(-5),
                "Negative round should return base health");
        }

        [Test]
        public void GetScaledDefense_RoundZeroOrNegative_ReturnsBaseDefense()
        {
            Assert.AreEqual(5, normalMonster.GetScaledDefense(0),
                "Round 0 should return base defense");
            Assert.AreEqual(5, normalMonster.GetScaledDefense(-3),
                "Negative round should return base defense");
        }

        [Test]
        public void GetScaledHealth_IncreaseMonotonically()
        {
            int previousHp = 0;
            for (int round = 1; round <= 30; round++)
            {
                int hp = normalMonster.GetScaledHealth(round);
                Assert.GreaterOrEqual(hp, previousHp,
                    $"HP should increase from round {round - 1} to {round} (was {previousHp}, got {hp})");
                previousHp = hp;
            }
        }

        [Test]
        public void GetScaledDefense_IncreaseMonotonically()
        {
            int previousDef = 0;
            for (int round = 1; round <= 30; round++)
            {
                int def = tankMonster.GetScaledDefense(round);
                Assert.GreaterOrEqual(def, previousDef,
                    $"Defense should increase from round {round - 1} to {round} (was {previousDef}, got {def})");
                previousDef = def;
            }
        }

        [Test]
        public void GetScaledHealth_NoScaling_ReturnsBaseEveryRound()
        {
            var noScaleMonster = ScriptableObject.CreateInstance<MonsterData>();
            noScaleMonster.maxHealth = 50;
            noScaleMonster.healthScaling = 1.0f;

            Assert.AreEqual(50, noScaleMonster.GetScaledHealth(1));
            Assert.AreEqual(50, noScaleMonster.GetScaledHealth(5));
            Assert.AreEqual(50, noScaleMonster.GetScaledHealth(10));

            Object.DestroyImmediate(noScaleMonster);
        }
    }
}
