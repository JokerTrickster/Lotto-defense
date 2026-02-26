using NUnit.Framework;
using UnityEngine;
using LottoDefense.Gameplay;

namespace LottoDefense.Tests
{
    public class GameplayStateTests
    {
        private GameplayManager manager;
        private GameObject managerObj;

        [SetUp]
        public void Setup()
        {
            managerObj = new GameObject("TestGameplayManager");
            manager = managerObj.AddComponent<GameplayManager>();
        }

        [TearDown]
        public void TearDown()
        {
            if (managerObj != null) Object.DestroyImmediate(managerObj);
        }

        #region Initial State
        [Test]
        public void Initialize_StartsInCountdown()
        {
            Assert.AreEqual(GameState.Countdown, manager.CurrentState,
                "Game should start in Countdown state");
        }

        [Test]
        public void Initialize_StartsAtRound1()
        {
            Assert.AreEqual(1, manager.CurrentRound,
                "Game should start at round 1");
        }

        [Test]
        public void Initialize_StartsWith10Life()
        {
            Assert.AreEqual(10, manager.CurrentLife,
                "Game should start with 10 life");
        }

        [Test]
        public void Initialize_StartsWithGold()
        {
            Assert.GreaterOrEqual(manager.CurrentGold, 0,
                "Game should start with non-negative gold");
        }

        [Test]
        public void Initialize_DefaultDifficulty_IsNormal()
        {
            Assert.AreEqual(GameDifficulty.Normal, manager.CurrentDifficulty,
                "Default difficulty should be Normal");
        }
        #endregion

        #region Valid State Transitions
        [Test]
        public void ChangeState_CountdownToPreparation_Succeeds()
        {
            GameState oldState = GameState.Countdown;
            GameState newState = GameState.Countdown;
            manager.OnStateChanged += (old, next) => { oldState = old; newState = next; };

            manager.ChangeState(GameState.Preparation);

            Assert.AreEqual(GameState.Preparation, manager.CurrentState);
            Assert.AreEqual(GameState.Countdown, oldState, "Old state should be Countdown");
            Assert.AreEqual(GameState.Preparation, newState, "New state should be Preparation");
        }

        [Test]
        public void ChangeState_PreparationToCombat_Succeeds()
        {
            manager.ChangeState(GameState.Preparation);
            manager.ChangeState(GameState.Combat);

            Assert.AreEqual(GameState.Combat, manager.CurrentState);
        }

        [Test]
        public void ChangeState_CombatToRoundResult_Succeeds()
        {
            manager.ChangeState(GameState.Preparation);
            manager.ChangeState(GameState.Combat);
            manager.ChangeState(GameState.RoundResult);

            Assert.AreEqual(GameState.RoundResult, manager.CurrentState);
        }

        [Test]
        public void ChangeState_RoundResultToPreparation_Succeeds()
        {
            manager.ChangeState(GameState.Preparation);
            manager.ChangeState(GameState.Combat);
            manager.ChangeState(GameState.RoundResult);
            manager.ChangeState(GameState.Preparation);

            Assert.AreEqual(GameState.Preparation, manager.CurrentState);
        }

        [Test]
        public void ChangeState_CombatToVictory_Succeeds()
        {
            manager.ChangeState(GameState.Preparation);
            manager.ChangeState(GameState.Combat);
            manager.ChangeState(GameState.Victory);

            Assert.AreEqual(GameState.Victory, manager.CurrentState);
        }

        [Test]
        public void ChangeState_AnyStateToDefeat_Succeeds()
        {
            Assert.AreEqual(GameState.Countdown, manager.CurrentState);
            manager.ChangeState(GameState.Defeat);
            Assert.AreEqual(GameState.Defeat, manager.CurrentState,
                "Should be able to transition from Countdown to Defeat");
        }

        [Test]
        public void ChangeState_PreparationToDefeat_Succeeds()
        {
            manager.ChangeState(GameState.Preparation);
            manager.ChangeState(GameState.Defeat);
            Assert.AreEqual(GameState.Defeat, manager.CurrentState);
        }

        [Test]
        public void ChangeState_CombatToDefeat_Succeeds()
        {
            manager.ChangeState(GameState.Preparation);
            manager.ChangeState(GameState.Combat);
            manager.ChangeState(GameState.Defeat);
            Assert.AreEqual(GameState.Defeat, manager.CurrentState);
        }
        #endregion

        #region Invalid State Transitions
        [Test]
        public void ChangeState_VictoryToPreparation_Rejected()
        {
            manager.ChangeState(GameState.Preparation);
            manager.ChangeState(GameState.Combat);
            manager.ChangeState(GameState.Victory);

            manager.ChangeState(GameState.Preparation);

            Assert.AreEqual(GameState.Victory, manager.CurrentState,
                "Victory is a terminal state - cannot transition to Preparation");
        }

        [Test]
        public void ChangeState_DefeatToCombat_Rejected()
        {
            manager.ChangeState(GameState.Defeat);

            manager.ChangeState(GameState.Combat);

            Assert.AreEqual(GameState.Defeat, manager.CurrentState,
                "Defeat is a terminal state - cannot transition to Combat");
        }

        [Test]
        public void ChangeState_CountdownToCombat_Rejected()
        {
            manager.ChangeState(GameState.Combat);

            Assert.AreEqual(GameState.Countdown, manager.CurrentState,
                "Cannot skip from Countdown directly to Combat");
        }

        [Test]
        public void ChangeState_PreparationToVictory_Rejected()
        {
            manager.ChangeState(GameState.Preparation);
            manager.ChangeState(GameState.Victory);

            Assert.AreEqual(GameState.Preparation, manager.CurrentState,
                "Cannot go from Preparation directly to Victory");
        }

        [Test]
        public void ChangeState_SameState_NoOp()
        {
            manager.ChangeState(GameState.Preparation);
            bool eventFired = false;
            manager.OnStateChanged += (old, next) => { eventFired = true; };

            manager.ChangeState(GameState.Preparation);

            Assert.IsFalse(eventFired, "Changing to same state should not fire event");
        }
        #endregion

        #region Gold Management
        [Test]
        public void SetGold_PositiveValue_SetsCorrectly()
        {
            manager.SetGold(100);
            Assert.AreEqual(100, manager.CurrentGold);
        }

        [Test]
        public void SetGold_NegativeValue_ClampsToZero()
        {
            manager.SetGold(-50);
            Assert.AreEqual(0, manager.CurrentGold,
                "Gold should clamp to 0, not go negative");
        }

        [Test]
        public void ModifyGold_AddGold_Increases()
        {
            int startGold = manager.CurrentGold;
            manager.ModifyGold(50);
            Assert.AreEqual(startGold + 50, manager.CurrentGold);
        }

        [Test]
        public void ModifyGold_SpendGold_Decreases()
        {
            manager.SetGold(100);
            manager.ModifyGold(-30);
            Assert.AreEqual(70, manager.CurrentGold);
        }

        [Test]
        public void ModifyGold_SpendMoreThanHave_ClampsToZero()
        {
            manager.SetGold(10);
            manager.ModifyGold(-50);
            Assert.AreEqual(0, manager.CurrentGold,
                "Spending more gold than available should clamp to 0");
        }

        [Test]
        public void SetGold_FiresEvent()
        {
            string eventType = null;
            int eventValue = -1;
            manager.OnGameValueChanged += (type, val) => { eventType = type; eventValue = val; };

            manager.SetGold(42);

            Assert.AreEqual("Gold", eventType);
            Assert.AreEqual(42, eventValue);
        }
        #endregion

        #region Life Management
        [Test]
        public void SetLife_PositiveValue_SetsCorrectly()
        {
            manager.SetLife(5);
            Assert.AreEqual(5, manager.CurrentLife);
        }

        [Test]
        public void SetLife_NegativeValue_ClampsToZero()
        {
            manager.SetLife(-10);
            Assert.AreEqual(0, manager.CurrentLife,
                "Life should clamp to 0");
        }

        [Test]
        public void ModifyLife_TakeDamage_Decreases()
        {
            manager.SetLife(10);
            manager.ModifyLife(-3);
            Assert.AreEqual(7, manager.CurrentLife);
        }

        [Test]
        public void SetLife_ZeroLife_TriggersDefeat()
        {
            manager.ChangeState(GameState.Preparation);
            manager.ChangeState(GameState.Combat);

            manager.SetLife(0);

            Assert.AreEqual(GameState.Defeat, manager.CurrentState,
                "Setting life to 0 should auto-transition to Defeat");
        }

        [Test]
        public void ModifyLife_ToZero_TriggersDefeat()
        {
            manager.ChangeState(GameState.Preparation);
            manager.ChangeState(GameState.Combat);
            manager.SetLife(3);

            manager.ModifyLife(-5);

            Assert.AreEqual(0, manager.CurrentLife);
            Assert.AreEqual(GameState.Defeat, manager.CurrentState,
                "Life reaching 0 via ModifyLife should trigger Defeat");
        }

        [Test]
        public void SetLife_FiresEvent()
        {
            string eventType = null;
            int eventValue = -1;
            manager.OnGameValueChanged += (type, val) => { eventType = type; eventValue = val; };

            manager.SetLife(7);

            Assert.AreEqual("Life", eventType);
            Assert.AreEqual(7, eventValue);
        }
        #endregion

        #region Round Management
        [Test]
        public void SetRound_ValidValue_SetsCorrectly()
        {
            manager.SetRound(5);
            Assert.AreEqual(5, manager.CurrentRound);
        }

        [Test]
        public void SetRound_ZeroOrNegative_Rejected()
        {
            manager.SetRound(0);
            Assert.AreEqual(1, manager.CurrentRound,
                "Round 0 should be rejected, keeping current round");

            manager.SetRound(-5);
            Assert.AreEqual(1, manager.CurrentRound,
                "Negative round should be rejected");
        }

        [Test]
        public void NextRound_IncrementsBy1()
        {
            Assert.AreEqual(1, manager.CurrentRound);
            manager.NextRound();
            Assert.AreEqual(2, manager.CurrentRound);
            manager.NextRound();
            Assert.AreEqual(3, manager.CurrentRound);
        }

        [Test]
        public void SetRound_FiresEvent()
        {
            string eventType = null;
            int eventValue = -1;
            manager.OnGameValueChanged += (type, val) => { eventType = type; eventValue = val; };

            manager.SetRound(3);

            Assert.AreEqual("Round", eventType);
            Assert.AreEqual(3, eventValue);
        }
        #endregion

        #region Difficulty
        [Test]
        public void SetDifficulty_UpdatesProperty()
        {
            manager.SetDifficulty(GameDifficulty.Hard);
            Assert.AreEqual(GameDifficulty.Hard, manager.CurrentDifficulty);

            manager.SetDifficulty(GameDifficulty.VeryHard);
            Assert.AreEqual(GameDifficulty.VeryHard, manager.CurrentDifficulty);
        }
        #endregion

        #region Event Firing
        [Test]
        public void ChangeState_FiresOnStateChanged()
        {
            GameState capturedOld = GameState.Countdown;
            GameState capturedNew = GameState.Countdown;
            int callCount = 0;

            manager.OnStateChanged += (old, next) =>
            {
                capturedOld = old;
                capturedNew = next;
                callCount++;
            };

            manager.ChangeState(GameState.Preparation);

            Assert.AreEqual(1, callCount, "Event should fire exactly once");
            Assert.AreEqual(GameState.Countdown, capturedOld);
            Assert.AreEqual(GameState.Preparation, capturedNew);
        }

        [Test]
        public void ChangeState_InvalidTransition_DoesNotFireEvent()
        {
            int callCount = 0;
            manager.OnStateChanged += (old, next) => { callCount++; };

            manager.ChangeState(GameState.Combat);

            Assert.AreEqual(0, callCount,
                "Invalid transition should not fire OnStateChanged");
        }
        #endregion
    }
}
