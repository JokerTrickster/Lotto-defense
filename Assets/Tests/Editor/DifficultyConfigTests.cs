using NUnit.Framework;
using UnityEngine;
using LottoDefense.Gameplay;

namespace LottoDefense.Tests
{
    /// <summary>
    /// Unit tests for DifficultyConfig scaling calculations.
    /// </summary>
    public class DifficultyConfigTests
    {
        private DifficultyConfig config;

        [SetUp]
        public void Setup()
        {
            config = ScriptableObject.CreateInstance<DifficultyConfig>();

            // Create test curves using reflection to set private fields
            var hpCurveField = typeof(DifficultyConfig).GetField("hpCurve",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var defenseCurveField = typeof(DifficultyConfig).GetField("defenseCurve",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var baseHpField = typeof(DifficultyConfig).GetField("baseHpMultiplier",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var baseDefField = typeof(DifficultyConfig).GetField("baseDefenseMultiplier",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var maxRoundsField = typeof(DifficultyConfig).GetField("maxRounds",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Set linear curves for testing
            AnimationCurve hpCurve = AnimationCurve.Linear(0f, 1f, 1f, 5f);
            AnimationCurve defenseCurve = AnimationCurve.Linear(0f, 1f, 1f, 3f);

            hpCurveField?.SetValue(config, hpCurve);
            defenseCurveField?.SetValue(config, defenseCurve);
            baseHpField?.SetValue(config, 1.0f);
            baseDefField?.SetValue(config, 1.0f);
            maxRoundsField?.SetValue(config, 30);
        }

        [TearDown]
        public void Teardown()
        {
            if (config != null)
            {
                Object.DestroyImmediate(config);
            }
        }

        [Test]
        public void DifficultyConfig_Round1_ReturnsBaseMultipliers()
        {
            var multipliers = config.GetMultipliersForRound(1);

            Assert.AreEqual(1.0f, multipliers.hpMultiplier, 0.01f,
                "Round 1 should have base HP multiplier of 1.0");
            Assert.AreEqual(1.0f, multipliers.defenseMultiplier, 0.01f,
                "Round 1 should have base Defense multiplier of 1.0");
        }

        [Test]
        public void DifficultyConfig_Round30_ReturnsMaxMultipliers()
        {
            var multipliers = config.GetMultipliersForRound(30);

            Assert.AreEqual(5.0f, multipliers.hpMultiplier, 0.1f,
                "Round 30 should have max HP multiplier of ~5.0");
            Assert.AreEqual(3.0f, multipliers.defenseMultiplier, 0.1f,
                "Round 30 should have max Defense multiplier of ~3.0");
        }

        [Test]
        public void DifficultyConfig_Round15_ReturnsMiddleMultipliers()
        {
            var multipliers = config.GetMultipliersForRound(15);

            // At round 15 (halfway), linear curve should be at midpoint
            float expectedHp = 1.0f + (5.0f - 1.0f) * 0.5f; // ~3.0
            float expectedDef = 1.0f + (3.0f - 1.0f) * 0.5f; // ~2.0

            Assert.AreEqual(expectedHp, multipliers.hpMultiplier, 0.2f,
                "Round 15 should have HP multiplier around 3.0");
            Assert.AreEqual(expectedDef, multipliers.defenseMultiplier, 0.2f,
                "Round 15 should have Defense multiplier around 2.0");
        }

        [Test]
        public void DifficultyConfig_InvalidRound_ClampsToValidRange()
        {
            var multipliersNegative = config.GetMultipliersForRound(-5);
            var multipliersZero = config.GetMultipliersForRound(0);
            var multipliersTooHigh = config.GetMultipliersForRound(999);

            // Negative/zero rounds should clamp to round 1
            Assert.AreEqual(1.0f, multipliersNegative.hpMultiplier, 0.01f,
                "Negative round should clamp to round 1");
            Assert.AreEqual(1.0f, multipliersZero.hpMultiplier, 0.01f,
                "Round 0 should clamp to round 1");

            // Too high rounds should clamp to max round (30)
            Assert.AreEqual(5.0f, multipliersTooHigh.hpMultiplier, 0.1f,
                "Round 999 should clamp to max round (30)");
        }

        [Test]
        public void DifficultyConfig_MaxRoundsProperty_Returns30()
        {
            Assert.AreEqual(30, config.MaxRounds,
                "MaxRounds should return 30");
        }

        [Test]
        public void DifficultyConfig_MultipliersIncrease_MonotonicallyAcrossRounds()
        {
            float previousHp = 0f;
            float previousDef = 0f;

            for (int round = 1; round <= 30; round++)
            {
                var multipliers = config.GetMultipliersForRound(round);

                if (round > 1)
                {
                    Assert.GreaterOrEqual(multipliers.hpMultiplier, previousHp,
                        $"HP multiplier should increase or stay same from round {round - 1} to {round}");
                    Assert.GreaterOrEqual(multipliers.defenseMultiplier, previousDef,
                        $"Defense multiplier should increase or stay same from round {round - 1} to {round}");
                }

                previousHp = multipliers.hpMultiplier;
                previousDef = multipliers.defenseMultiplier;
            }
        }

        [Test]
        public void DifficultyConfig_LogDifficultyProgression_DoesNotThrow()
        {
            // Should not throw exception
            Assert.DoesNotThrow(() => config.LogDifficultyProgression(),
                "LogDifficultyProgression should not throw exception");
        }
    }
}
