using NUnit.Framework;
using UnityEngine;

namespace LottoDefense.Tests
{
    /// <summary>
    /// Tests for combat damage formulas extracted from Monster.TakeDamage and Unit attack patterns.
    /// These test the pure math without requiring MonoBehaviour instances.
    /// </summary>
    public class CombatFormulaTests
    {
        #region Damage Formula: max(1, attack - defense)

        private static int CalculateDamage(int rawDamage, int defense)
        {
            return Mathf.Max(1, rawDamage - defense);
        }

        [Test]
        public void Damage_AttackGreaterThanDefense_ReturnsDifference()
        {
            // 20 attack vs 5 defense = 15 damage
            Assert.AreEqual(15, CalculateDamage(20, 5),
                "20 attack - 5 defense should deal 15 damage");
        }

        [Test]
        public void Damage_AttackEqualsDefense_ReturnsMinimum1()
        {
            Assert.AreEqual(1, CalculateDamage(10, 10),
                "Equal attack and defense should deal minimum 1 damage");
        }

        [Test]
        public void Damage_AttackLessThanDefense_ReturnsMinimum1()
        {
            Assert.AreEqual(1, CalculateDamage(5, 20),
                "Attack lower than defense should still deal 1 damage");
        }

        [Test]
        public void Damage_ZeroAttack_ReturnsMinimum1()
        {
            Assert.AreEqual(1, CalculateDamage(0, 10),
                "Zero attack should deal minimum 1 damage");
        }

        [Test]
        public void Damage_ZeroDefense_ReturnsFullDamage()
        {
            Assert.AreEqual(50, CalculateDamage(50, 0),
                "Zero defense should take full damage");
        }

        [Test]
        public void Damage_HighAttack_ReturnsLargeDamage()
        {
            Assert.AreEqual(95, CalculateDamage(100, 5),
                "100 attack vs 5 defense should deal 95 damage");
        }

        [Test]
        public void Damage_AlwaysPositive()
        {
            for (int atk = 0; atk <= 50; atk += 5)
            {
                for (int def = 0; def <= 50; def += 5)
                {
                    int dmg = CalculateDamage(atk, def);
                    Assert.GreaterOrEqual(dmg, 1,
                        $"Damage must be >= 1 (attack={atk}, defense={def}, got {dmg})");
                }
            }
        }
        #endregion

        #region Splash/AOE Falloff: lerp(1, falloff%, dist/radius)

        private static float CalculateSplashMultiplier(float distance, float radius, float falloffPercent)
        {
            float falloff = falloffPercent / 100f;
            return Mathf.Lerp(1f, falloff, distance / radius);
        }

        [Test]
        public void Splash_AtCenter_FullDamage()
        {
            float mult = CalculateSplashMultiplier(0f, 1.5f, 50f);
            Assert.AreEqual(1.0f, mult, 0.01f,
                "At distance 0 (center), splash multiplier should be 1.0");
        }

        [Test]
        public void Splash_AtEdge_ReturnsFalloff()
        {
            float mult = CalculateSplashMultiplier(1.5f, 1.5f, 50f);
            Assert.AreEqual(0.5f, mult, 0.01f,
                "At splash edge (dist == radius), multiplier should equal falloff% (0.5)");
        }

        [Test]
        public void Splash_AtHalfRadius_ReturnsMidpoint()
        {
            float mult = CalculateSplashMultiplier(0.75f, 1.5f, 50f);
            // lerp(1, 0.5, 0.5) = 0.75
            Assert.AreEqual(0.75f, mult, 0.01f,
                "At half radius, multiplier should be 0.75 (midpoint of 1.0 and 0.5)");
        }

        [Test]
        public void Splash_NoFalloff_AlwaysFull()
        {
            float mult = CalculateSplashMultiplier(1.0f, 1.5f, 100f);
            // lerp(1, 1.0, 0.667) = 1.0
            Assert.AreEqual(1.0f, mult, 0.01f,
                "100% falloff means no damage reduction");
        }

        [Test]
        public void Splash_ZeroFalloff_EdgDealsNoDamage()
        {
            float mult = CalculateSplashMultiplier(1.5f, 1.5f, 0f);
            // lerp(1, 0, 1.0) = 0
            Assert.AreEqual(0f, mult, 0.01f,
                "0% falloff at edge means zero damage");
        }

        [Test]
        public void Splash_DamageInRange()
        {
            int primaryDamage = 25;
            float mult = CalculateSplashMultiplier(0.75f, 1.5f, 50f);
            int splashDamage = Mathf.RoundToInt(primaryDamage * mult);
            Assert.AreEqual(19, splashDamage,
                "25 * 0.75 = 18.75, rounded to 19");
        }
        #endregion

        #region Chain Damage: damage * pow(0.8, bounceIndex)

        private static int CalculateChainDamage(int baseDamage, int bounceIndex)
        {
            return Mathf.RoundToInt(baseDamage * Mathf.Pow(0.8f, bounceIndex));
        }

        [Test]
        public void Chain_FirstTarget_FullDamage()
        {
            Assert.AreEqual(100, CalculateChainDamage(100, 0),
                "First target (bounce 0) takes full damage");
        }

        [Test]
        public void Chain_SecondTarget_80Percent()
        {
            // 100 * 0.8 = 80
            Assert.AreEqual(80, CalculateChainDamage(100, 1),
                "Second target takes 80% damage");
        }

        [Test]
        public void Chain_ThirdTarget_64Percent()
        {
            // 100 * 0.64 = 64
            Assert.AreEqual(64, CalculateChainDamage(100, 2),
                "Third target takes 64% damage");
        }

        [Test]
        public void Chain_FourthTarget_51Percent()
        {
            // 100 * 0.512 = 51 (rounded)
            Assert.AreEqual(51, CalculateChainDamage(100, 3),
                "Fourth target takes ~51% damage");
        }

        [Test]
        public void Chain_DamageDecreases_PerBounce()
        {
            int baseDmg = 50;
            int previous = baseDmg;
            for (int i = 1; i <= 5; i++)
            {
                int dmg = CalculateChainDamage(baseDmg, i);
                Assert.Less(dmg, previous,
                    $"Chain damage should decrease at bounce {i} (was {previous}, got {dmg})");
                previous = dmg;
            }
        }

        [Test]
        public void Chain_NeverReachesZero_InReasonableBounces()
        {
            int baseDmg = 50;
            for (int i = 0; i < 10; i++)
            {
                int dmg = CalculateChainDamage(baseDmg, i);
                Assert.GreaterOrEqual(dmg, 1,
                    $"Chain damage should be >= 1 at bounce {i} with base {baseDmg}");
            }
        }
        #endregion

        #region Combined Combat Scenario

        [Test]
        public void CombatScenario_WarriorVsNormalMonster()
        {
            int warriorAttack = 10;
            int normalDefense = 5;
            int damage = CalculateDamage(warriorAttack, normalDefense);
            Assert.AreEqual(5, damage, "Warrior (10) vs Normal (5 def) = 5 damage");
        }

        [Test]
        public void CombatScenario_WeakUnitVsTank()
        {
            int weakAttack = 8;
            int tankDefense = 10;
            int damage = CalculateDamage(weakAttack, tankDefense);
            Assert.AreEqual(1, damage,
                "Weak unit (8) vs Tank (10 def) = minimum 1 damage");
        }

        [Test]
        public void CombatScenario_PhoenixChainAttack()
        {
            int phoenixAttack = 50;
            int monsterDefense = 5;
            int primaryDamage = CalculateDamage(phoenixAttack, monsterDefense);
            Assert.AreEqual(45, primaryDamage, "Phoenix primary damage");

            int chain1 = CalculateChainDamage(primaryDamage, 1);
            Assert.AreEqual(36, chain1, "Chain 1 = 45 * 0.8 = 36");

            int chain2 = CalculateChainDamage(primaryDamage, 2);
            Assert.AreEqual(29, chain2, "Chain 2 = 45 * 0.64 = 28.8 → 29");

            int chain3 = CalculateChainDamage(primaryDamage, 3);
            Assert.AreEqual(23, chain3, "Chain 3 = 45 * 0.512 = 23.04 → 23");
        }
        #endregion
    }
}
