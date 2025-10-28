using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using LottoDefense.Gameplay;

namespace LottoDefense.Tests
{
    /// <summary>
    /// Unit tests for RoundManager round progression and phase management.
    /// </summary>
    public class RoundManagerTests
    {
        private GameObject testGameObject;
        private RoundManager roundManager;
        private GameObject gameplayManagerObj;
        private GameplayManager gameplayManager;

        [SetUp]
        public void Setup()
        {
            // Create GameplayManager first (required dependency)
            gameplayManagerObj = new GameObject("GameplayManager");
            gameplayManager = gameplayManagerObj.AddComponent<GameplayManager>();
            gameplayManager.Initialize();

            // Create RoundManager test object
            testGameObject = new GameObject("RoundManager");
            roundManager = testGameObject.AddComponent<RoundManager>();
        }

        [TearDown]
        public void Teardown()
        {
            if (testGameObject != null)
            {
                Object.DestroyImmediate(testGameObject);
            }

            if (gameplayManagerObj != null)
            {
                Object.DestroyImmediate(gameplayManagerObj);
            }
        }

        [Test]
        public void RoundManager_StartsInPreparationPhase()
        {
            // Verify initial phase is Preparation after Countdown
            gameplayManager.ChangeState(GameState.Preparation);

            Assert.AreEqual(GamePhase.Preparation, roundManager.CurrentPhase,
                "RoundManager should start in Preparation phase after Countdown");
        }

        [Test]
        public void RoundManager_TransitionsToCombatAfterPreparation()
        {
            bool phaseChanged = false;
            GamePhase newPhase = GamePhase.Preparation;

            roundManager.OnPhaseChanged += (oldPhase, phase) =>
            {
                phaseChanged = true;
                newPhase = phase;
            };

            gameplayManager.ChangeState(GameState.Preparation);
            gameplayManager.ChangeState(GameState.Combat);

            Assert.IsTrue(phaseChanged, "Phase changed event should fire");
            Assert.AreEqual(GamePhase.Combat, newPhase, "Should transition to Combat phase");
        }

        [Test]
        public void RoundManager_CurrentRoundMatchesGameplayManager()
        {
            gameplayManager.SetRound(5);

            Assert.AreEqual(5, roundManager.CurrentRound,
                "RoundManager CurrentRound should match GameplayManager round");
        }

        [Test]
        public void RoundManager_MaxRoundsDefaultsTo30()
        {
            Assert.AreEqual(30, roundManager.MaxRounds,
                "MaxRounds should default to 30 when no DifficultyConfig assigned");
        }

        [Test]
        public void RoundManager_GetPhaseDisplayName_ReturnsCorrectStrings()
        {
            gameplayManager.ChangeState(GameState.Preparation);
            Assert.AreEqual("PREPARATION", roundManager.GetPhaseDisplayName(),
                "Preparation phase should return 'PREPARATION'");

            gameplayManager.ChangeState(GameState.Combat);
            Assert.AreEqual("COMBAT", roundManager.GetPhaseDisplayName(),
                "Combat phase should return 'COMBAT'");
        }

        [Test]
        public void RoundManager_GetFormattedTime_ReturnsMMSSFormat()
        {
            // This test requires reflection or exposing RemainingTime setter
            // For now, verify the method exists and returns expected format
            string formatted = roundManager.GetFormattedTime();
            Assert.IsTrue(formatted.Contains(":"), "Formatted time should contain ':'");
            Assert.AreEqual(5, formatted.Length, "Formatted time should be MM:SS format (5 chars)");
        }

        [Test]
        public void RoundManager_IsPreparationPhase_ReturnsCorrectValue()
        {
            gameplayManager.ChangeState(GameState.Preparation);
            Assert.IsTrue(roundManager.IsPreparationPhase(),
                "IsPreparationPhase should return true during Preparation");

            gameplayManager.ChangeState(GameState.Combat);
            Assert.IsFalse(roundManager.IsPreparationPhase(),
                "IsPreparationPhase should return false during Combat");
        }

        [Test]
        public void RoundManager_IsCombatPhase_ReturnsCorrectValue()
        {
            gameplayManager.ChangeState(GameState.Combat);
            Assert.IsTrue(roundManager.IsCombatPhase(),
                "IsCombatPhase should return true during Combat");

            gameplayManager.ChangeState(GameState.Preparation);
            Assert.IsFalse(roundManager.IsCombatPhase(),
                "IsCombatPhase should return false during Preparation");
        }

        [Test]
        public void RoundManager_GetStats_ReturnsValidString()
        {
            string stats = roundManager.GetStats();

            Assert.IsNotNull(stats, "GetStats should not return null");
            Assert.IsTrue(stats.Contains("Round"), "Stats should contain 'Round'");
            Assert.IsTrue(stats.Contains("Phase"), "Stats should contain 'Phase'");
            Assert.IsTrue(stats.Contains("Time"), "Stats should contain 'Time'");
        }
    }
}
