using NUnit.Framework;
using UnityEngine;

namespace LottoDefense.Tests
{
    /// <summary>
    /// Tests for crowd control formulas extracted from Monster.ApplySlow/ApplyFreeze.
    /// Tests the pure math without requiring active Monster instances.
    /// </summary>
    public class MonsterCCTests
    {
        private float slowMultiplier;
        private float slowTimer;
        private float freezeTimer;
        private bool isFrozen;

        [SetUp]
        public void Setup()
        {
            slowMultiplier = 1f;
            slowTimer = 0f;
            freezeTimer = 0f;
            isFrozen = false;
        }

        private void ApplySlow(float multiplier, float duration)
        {
            if (duration <= 0f) return;
            slowMultiplier = Mathf.Min(slowMultiplier, multiplier);
            slowTimer = Mathf.Max(slowTimer, duration);
        }

        private void ApplyFreeze(float duration)
        {
            if (duration <= 0f) return;
            isFrozen = true;
            freezeTimer = Mathf.Max(freezeTimer, duration);
        }

        #region Slow Tests
        [Test]
        public void Slow_FirstApplication_SetsValues()
        {
            ApplySlow(0.5f, 3f);

            Assert.AreEqual(0.5f, slowMultiplier, 0.001f,
                "First slow should set multiplier to 0.5");
            Assert.AreEqual(3f, slowTimer, 0.001f,
                "First slow should set timer to 3 seconds");
        }

        [Test]
        public void Slow_StrongerOverwrite_KeepsStronger()
        {
            ApplySlow(0.5f, 3f);
            ApplySlow(0.3f, 2f);

            Assert.AreEqual(0.3f, slowMultiplier, 0.001f,
                "Stronger slow (0.3 < 0.5) should overwrite multiplier");
        }

        [Test]
        public void Slow_WeakerDoesNotOverwrite_KeepsStronger()
        {
            ApplySlow(0.3f, 3f);
            ApplySlow(0.7f, 5f);

            Assert.AreEqual(0.3f, slowMultiplier, 0.001f,
                "Weaker slow (0.7 > 0.3) should NOT overwrite multiplier");
        }

        [Test]
        public void Slow_LongerDuration_Extends()
        {
            ApplySlow(0.5f, 3f);
            ApplySlow(0.5f, 5f);

            Assert.AreEqual(5f, slowTimer, 0.001f,
                "Longer duration (5 > 3) should extend timer");
        }

        [Test]
        public void Slow_ShorterDuration_DoesNotReduce()
        {
            ApplySlow(0.5f, 5f);
            ApplySlow(0.5f, 2f);

            Assert.AreEqual(5f, slowTimer, 0.001f,
                "Shorter duration (2 < 5) should NOT reduce timer");
        }

        [Test]
        public void Slow_ZeroDuration_NoEffect()
        {
            ApplySlow(0.5f, 0f);

            Assert.AreEqual(1f, slowMultiplier, 0.001f,
                "Zero duration slow should have no effect");
            Assert.AreEqual(0f, slowTimer, 0.001f);
        }

        [Test]
        public void Slow_NegativeDuration_NoEffect()
        {
            ApplySlow(0.5f, -1f);

            Assert.AreEqual(1f, slowMultiplier, 0.001f,
                "Negative duration slow should have no effect");
        }

        [Test]
        public void Slow_MultiplierAffectsSpeed()
        {
            float baseSpeed = 2.0f;
            ApplySlow(0.5f, 3f);
            float effectiveSpeed = baseSpeed * slowMultiplier;

            Assert.AreEqual(1.0f, effectiveSpeed, 0.001f,
                "50% slow on speed 2.0 should give effective speed 1.0");
        }

        [Test]
        public void Slow_MultipleApplications_KeepsBestOfBoth()
        {
            // First: 50% slow for 3s
            ApplySlow(0.5f, 3f);
            // Second: 30% slow for 5s (stronger multiplier, longer duration)
            ApplySlow(0.3f, 5f);

            Assert.AreEqual(0.3f, slowMultiplier, 0.001f, "Should keep stronger multiplier");
            Assert.AreEqual(5f, slowTimer, 0.001f, "Should keep longer timer");
        }

        [Test]
        public void Slow_MixedApplications_KeepsBestParts()
        {
            // First: strong slow, short duration
            ApplySlow(0.2f, 2f);
            // Second: weak slow, long duration
            ApplySlow(0.8f, 10f);

            Assert.AreEqual(0.2f, slowMultiplier, 0.001f,
                "Should keep the stronger multiplier (0.2 from first)");
            Assert.AreEqual(10f, slowTimer, 0.001f,
                "Should keep the longer timer (10 from second)");
        }
        #endregion

        #region Freeze Tests
        [Test]
        public void Freeze_FirstApplication_Freezes()
        {
            ApplyFreeze(2f);

            Assert.IsTrue(isFrozen, "Should be frozen after ApplyFreeze");
            Assert.AreEqual(2f, freezeTimer, 0.001f, "Freeze timer should be set");
        }

        [Test]
        public void Freeze_LongerDuration_Extends()
        {
            ApplyFreeze(2f);
            ApplyFreeze(5f);

            Assert.AreEqual(5f, freezeTimer, 0.001f,
                "Longer freeze (5 > 2) should extend timer");
        }

        [Test]
        public void Freeze_ShorterDuration_DoesNotReduce()
        {
            ApplyFreeze(5f);
            ApplyFreeze(2f);

            Assert.AreEqual(5f, freezeTimer, 0.001f,
                "Shorter freeze (2 < 5) should NOT reduce timer");
        }

        [Test]
        public void Freeze_ZeroDuration_NoEffect()
        {
            ApplyFreeze(0f);

            Assert.IsFalse(isFrozen, "Zero duration freeze should not freeze");
            Assert.AreEqual(0f, freezeTimer, 0.001f);
        }

        [Test]
        public void Freeze_NegativeDuration_NoEffect()
        {
            ApplyFreeze(-1f);

            Assert.IsFalse(isFrozen, "Negative duration freeze should not freeze");
        }
        #endregion

        #region Slow + Freeze Interaction
        [Test]
        public void SlowThenFreeze_BothActive()
        {
            ApplySlow(0.5f, 3f);
            ApplyFreeze(2f);

            Assert.IsTrue(isFrozen, "Should be frozen");
            Assert.AreEqual(0.5f, slowMultiplier, 0.001f,
                "Slow should still be tracked during freeze");
            Assert.AreEqual(3f, slowTimer, 0.001f,
                "Slow timer should persist during freeze");
        }

        [Test]
        public void FreezeThenSlow_BothActive()
        {
            ApplyFreeze(5f);
            ApplySlow(0.3f, 8f);

            Assert.IsTrue(isFrozen, "Should still be frozen");
            Assert.AreEqual(0.3f, slowMultiplier, 0.001f,
                "Slow should apply even while frozen");
            Assert.AreEqual(5f, freezeTimer, 0.001f,
                "Freeze timer should not be affected by slow");
        }

        [Test]
        public void Freeze_StopsMovement_RegardlessOfSlow()
        {
            float baseSpeed = 4.0f;
            ApplySlow(0.5f, 3f);
            ApplyFreeze(2f);

            float effectiveSpeed = isFrozen ? 0f : baseSpeed * slowMultiplier;
            Assert.AreEqual(0f, effectiveSpeed, 0.001f,
                "Frozen monster should have zero speed regardless of slow");
        }
        #endregion
    }
}
