using UnityEngine;
using System;
using System.Collections;
using LottoDefense.Gameplay;
using LottoDefense.Grid;
using LottoDefense.UI;
using LottoDefense.Profile;
using LottoDefense.Networking;

namespace LottoDefense.Multiplayer
{
    /// <summary>
    /// Manages cooperative gameplay mechanics for 2-player mode.
    /// Handles game initialization, player synchronization, and victory conditions.
    /// </summary>
    public class CoopGameManager : MonoBehaviour
    {
        #region Singleton
        private static CoopGameManager _instance;
        public static CoopGameManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<CoopGameManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("CoopGameManager");
                        _instance = go.AddComponent<CoopGameManager>();
                    }
                }
                return _instance;
            }
        }
        #endregion

        #region Events
        /// <summary>
        /// Fired when cooperative game starts
        /// </summary>
        public event Action<int, int> OnCoopGameStarted; // player1Id, player2Id

        /// <summary>
        /// Fired when cooperative game ends
        /// </summary>
        public event Action<bool> OnCoopGameEnded; // victory

        /// <summary>
        /// Fired when a player's area is breached
        /// </summary>
        public event Action<int> OnPlayerAreaBreached; // playerNumber
        #endregion

        #region Private Fields
        private bool _isCoopActive = false;
        private int _localPlayerNumber = 1;
        private int _remotePlayerNumber = 2;
        private CoopStateSync _stateSync;
        private EnhancedCountdownUI _enhancedCountdown;
        #endregion

        #region Public Properties
        /// <summary>
        /// Whether cooperative game is currently active
        /// </summary>
        public bool IsCoopActive => _isCoopActive;

        /// <summary>
        /// Local player's assigned number (1 or 2)
        /// </summary>
        public int LocalPlayerNumber => _localPlayerNumber;

        /// <summary>
        /// Remote player's assigned number (1 or 2)
        /// </summary>
        public int RemotePlayerNumber => _remotePlayerNumber;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            // Find or create state sync component
            _stateSync = FindFirstObjectByType<CoopStateSync>();
            if (_stateSync == null)
            {
                GameObject syncObj = new GameObject("CoopStateSync");
                _stateSync = syncObj.AddComponent<CoopStateSync>();
            }
        }
        #endregion

        #region Game Initialization
        /// <summary>
        /// Initialize a cooperative game session
        /// </summary>
        public void InitializeCoopGame(bool isHost)
        {
            if (_isCoopActive)
            {
                Debug.LogWarning("[CoopGameManager] Coop game already active");
                return;
            }

            Debug.Log($"[CoopGameManager] Initializing coop game - IsHost: {isHost}");

            // Assign player numbers based on host status
            _localPlayerNumber = isHost ? 1 : 2;
            _remotePlayerNumber = isHost ? 2 : 1;

            // Enable coop mode in map manager
            CoopMapManager.Instance.EnableCoopMode();

            // Setup enhanced countdown
            SetupEnhancedCountdown();

            _isCoopActive = true;

            // Notify listeners
            OnCoopGameStarted?.Invoke(_localPlayerNumber, _remotePlayerNumber);
        }

        /// <summary>
        /// Setup the enhanced countdown UI for coop mode
        /// </summary>
        private void SetupEnhancedCountdown()
        {
            // Find or create enhanced countdown
            _enhancedCountdown = FindFirstObjectByType<EnhancedCountdownUI>();
            if (_enhancedCountdown == null)
            {
                GameObject countdownObj = new GameObject("EnhancedCountdownUI");
                _enhancedCountdown = countdownObj.AddComponent<EnhancedCountdownUI>();
            }
        }

        /// <summary>
        /// Start the cooperative game with countdown
        /// </summary>
        public void StartCoopGame()
        {
            if (!_isCoopActive)
            {
                Debug.LogError("[CoopGameManager] Cannot start - coop not initialized");
                return;
            }

            StartCoroutine(CoopGameStartSequence());
        }

        /// <summary>
        /// Cooperative game start sequence
        /// </summary>
        private IEnumerator CoopGameStartSequence()
        {
            Debug.Log("[CoopGameManager] Starting coop game sequence");

            // Show enhanced countdown with player profiles
            if (_enhancedCountdown != null)
            {
                bool countdownComplete = false;
                _enhancedCountdown.StartEnhancedCountdown(true, () => countdownComplete = true);

                // Wait for countdown to complete
                while (!countdownComplete)
                {
                    yield return null;
                }
            }
            else
            {
                // Fallback to standard countdown
                yield return new WaitForSeconds(3f);
            }

            // Start the actual gameplay
            if (GameplayManager.Instance != null)
            {
                GameplayManager.Instance.StartGame();
            }

            Debug.Log("[CoopGameManager] Coop game started successfully");
        }
        #endregion

        #region Player Management
        /// <summary>
        /// Check if a grid position belongs to the local player
        /// </summary>
        public bool IsLocalPlayerPosition(Vector2Int pos)
        {
            if (!_isCoopActive) return true; // All positions in single player

            return CoopMapManager.Instance.GetPlayerForPosition(pos) == _localPlayerNumber;
        }

        /// <summary>
        /// Check if the local player can place at a position
        /// </summary>
        public bool CanLocalPlayerPlace(Vector2Int pos)
        {
            if (!_isCoopActive) return true; // No restrictions in single player

            return CoopMapManager.Instance.CanPlayerPlaceAt(_localPlayerNumber, pos);
        }

        /// <summary>
        /// Get the color associated with a player
        /// </summary>
        public Color GetPlayerColor(int playerNumber)
        {
            if (playerNumber == 1)
                return new Color(0.2f, 0.5f, 1f); // Blue for Player 1
            else if (playerNumber == 2)
                return new Color(1f, 0.5f, 0.2f); // Orange for Player 2
            else
                return Color.white;
        }
        #endregion

        #region State Synchronization
        /// <summary>
        /// Sync local unit placement with remote player
        /// </summary>
        public void SyncUnitPlacement(Vector2Int position, string unitType)
        {
            if (!_isCoopActive || _stateSync == null) return;

            // Send placement data through state sync
            _stateSync.SendUnitPlacement(_localPlayerNumber, position, unitType);
        }

        /// <summary>
        /// Sync local unit removal with remote player
        /// </summary>
        public void SyncUnitRemoval(Vector2Int position)
        {
            if (!_isCoopActive || _stateSync == null) return;

            // Send removal data through state sync
            _stateSync.SendUnitRemoval(_localPlayerNumber, position);
        }

        /// <summary>
        /// Handle remote player's unit placement
        /// </summary>
        public void OnRemoteUnitPlaced(int playerNumber, Vector2Int position, string unitType)
        {
            if (playerNumber == _localPlayerNumber) return; // Ignore own placements

            Debug.Log($"[CoopGameManager] Remote player {playerNumber} placed {unitType} at {position}");

            // Place the unit visually
            // Implementation depends on your unit spawning system
        }

        /// <summary>
        /// Handle remote player's unit removal
        /// </summary>
        public void OnRemoteUnitRemoved(int playerNumber, Vector2Int position)
        {
            if (playerNumber == _localPlayerNumber) return; // Ignore own removals

            Debug.Log($"[CoopGameManager] Remote player {playerNumber} removed unit at {position}");

            // Remove the unit visually
            if (GridManager.Instance != null)
            {
                GridManager.Instance.RemoveUnit(position);
            }
        }
        #endregion

        #region Victory Conditions
        /// <summary>
        /// Check cooperative victory conditions
        /// </summary>
        public void CheckCoopVictory()
        {
            if (!_isCoopActive) return;

            // In coop mode, both players must survive the waves
            // Check if either player's base has been breached
            bool player1Alive = CheckPlayerAlive(1);
            bool player2Alive = CheckPlayerAlive(2);

            if (!player1Alive || !player2Alive)
            {
                // Game over - cooperative loss
                EndCoopGame(false);
            }
            else if (GameplayManager.Instance != null && GameplayManager.Instance.AllWavesComplete)
            {
                // All waves complete and both players alive - victory!
                EndCoopGame(true);
            }
        }

        /// <summary>
        /// Check if a player is still alive
        /// </summary>
        private bool CheckPlayerAlive(int playerNumber)
        {
            // Check player's health/lives
            // This would depend on your health system implementation
            return true; // Placeholder
        }

        /// <summary>
        /// End the cooperative game
        /// </summary>
        public void EndCoopGame(bool victory)
        {
            if (!_isCoopActive) return;

            Debug.Log($"[CoopGameManager] Coop game ended - Victory: {victory}");

            _isCoopActive = false;

            // Disable coop map mode
            CoopMapManager.Instance.DisableCoopMode();

            // Notify listeners
            OnCoopGameEnded?.Invoke(victory);

            // Show results UI
            ShowCoopResults(victory);
        }

        /// <summary>
        /// Show cooperative game results
        /// </summary>
        private void ShowCoopResults(bool victory)
        {
            // Display coop-specific victory/defeat screen
            // This would show both players' stats, scores, etc.
            string resultMessage = victory ?
                "Cooperative Victory!\nGreat teamwork!" :
                "Cooperative Defeat\nBetter luck next time!";

            Debug.Log($"[CoopGameManager] {resultMessage}");
        }
        #endregion

        #region Cleanup
        /// <summary>
        /// Clean up cooperative game resources
        /// </summary>
        public void CleanupCoopGame()
        {
            if (_isCoopActive)
            {
                EndCoopGame(false);
            }

            // Reset player numbers
            _localPlayerNumber = 1;
            _remotePlayerNumber = 2;

            // Clear references
            _enhancedCountdown = null;
            _stateSync = null;
        }

        private void OnDestroy()
        {
            CleanupCoopGame();
        }
        #endregion
    }
}