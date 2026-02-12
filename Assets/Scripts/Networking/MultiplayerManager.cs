using System;
using UnityEngine;
using LottoDefense.Gameplay;

namespace LottoDefense.Networking
{
    /// <summary>
    /// Multiplayer session state.
    /// </summary>
    public enum MultiplayerState
    {
        Disconnected,
        Connecting,
        InLobby,
        InRoom,
        Playing,
        Result
    }

    /// <summary>
    /// Singleton manager for multiplayer sessions.
    /// Coordinates WebSocket communication, room management, and opponent state tracking.
    /// </summary>
    public class MultiplayerManager : MonoBehaviour
    {
        #region Singleton
        private static MultiplayerManager _instance;

        public static MultiplayerManager Instance
        {
            get
            {
                if (GameplayManager.IsCleaningUp) return null;

                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<MultiplayerManager>();
                }
                return _instance;
            }
        }
        #endregion

        #region Events
        public event Action OnConnected;
        public event Action<string> OnDisconnected;
        public event Action<string> OnRoomCreated;
        public event Action<string> OnPlayerJoined;
        public event Action OnMatchStart;
        public event Action<int> OnWaveSync;
        public event Action<OpponentStatePayload> OnOpponentStateUpdated;
        public event Action<string, int> OnOpponentDead;
        public event Action<MatchResultPayload> OnMatchResult;
        public event Action<string> OnError;
        #endregion

        #region Properties
        public MultiplayerState CurrentState { get; private set; } = MultiplayerState.Disconnected;
        public bool IsMultiplayer => CurrentState == MultiplayerState.Playing;
        public bool IsInSession => CurrentState != MultiplayerState.Disconnected;
        public string RoomCode { get; private set; }
        public string PlayerName { get; private set; }
        public OpponentStatePayload OpponentState { get; private set; }
        #endregion

        #region Private Fields
        private WebSocketClient wsClient;
        private float stateReportInterval = 3f;
        private float stateReportTimer;
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

            wsClient = gameObject.AddComponent<WebSocketClient>();
            wsClient.OnConnected += HandleConnected;
            wsClient.OnDisconnected += HandleDisconnected;
            wsClient.OnMessageReceived += HandleMessage;
            wsClient.OnError += HandleError;
        }

        private void Update()
        {
            if (CurrentState != MultiplayerState.Playing) return;

            stateReportTimer -= Time.deltaTime;
            if (stateReportTimer <= 0f)
            {
                stateReportTimer = stateReportInterval;
                SendGameStateUpdate();
            }
        }

        private void OnDestroy()
        {
            UnsubscribeFromGameplay();

            if (wsClient != null)
            {
                wsClient.OnConnected -= HandleConnected;
                wsClient.OnDisconnected -= HandleDisconnected;
                wsClient.OnMessageReceived -= HandleMessage;
                wsClient.OnError -= HandleError;
            }

            if (_instance == this)
            {
                _instance = null;
            }
        }
        #endregion

        #region Public API - Connection
        public void ConnectToServer(string url, string playerName)
        {
            if (CurrentState != MultiplayerState.Disconnected)
            {
                Debug.LogWarning("[MultiplayerManager] Already connected or connecting");
                return;
            }

            PlayerName = playerName;
            SetState(MultiplayerState.Connecting);
            wsClient.Connect(url);
        }

        public void DisconnectFromServer()
        {
            UnsubscribeFromGameplay();
            wsClient.Disconnect();
            RoomCode = null;
            OpponentState = null;
            SetState(MultiplayerState.Disconnected);
        }
        #endregion

        #region Public API - Room Management
        public void CreateRoom()
        {
            if (CurrentState != MultiplayerState.InLobby)
            {
                Debug.LogWarning("[MultiplayerManager] Must be in lobby to create room");
                return;
            }

            wsClient.SendMessage(MessageType.RoomCreate, new RoomCreatePayload
            {
                playerName = PlayerName
            });
        }

        public void JoinRoom(string roomCode)
        {
            if (CurrentState != MultiplayerState.InLobby)
            {
                Debug.LogWarning("[MultiplayerManager] Must be in lobby to join room");
                return;
            }

            wsClient.SendMessage(MessageType.RoomJoin, new RoomJoinPayload
            {
                roomCode = roomCode,
                playerName = PlayerName
            });
        }

        public void RequestAutoMatch()
        {
            if (CurrentState != MultiplayerState.InLobby)
            {
                Debug.LogWarning("[MultiplayerManager] Must be in lobby to auto-match");
                return;
            }

            wsClient.SendMessage(MessageType.RoomAutoMatch, new RoomAutoMatchPayload
            {
                playerName = PlayerName
            });
        }

        public void SendReady()
        {
            if (CurrentState != MultiplayerState.InRoom)
            {
                Debug.LogWarning("[MultiplayerManager] Must be in room to send ready");
                return;
            }

            wsClient.SendMessage(MessageType.PlayerReady, new PlayerReadyPayload
            {
                roomCode = RoomCode
            });
        }
        #endregion

        #region Public API - In-Game
        public void SendPlayerDead(int finalRound, int contribution)
        {
            if (CurrentState != MultiplayerState.Playing) return;

            wsClient.SendMessage(MessageType.PlayerDead, new PlayerDeadPayload
            {
                finalRound = finalRound,
                contribution = contribution
            });
        }
        #endregion

        #region Gameplay Subscription
        private void SubscribeToGameplay()
        {
            if (GameplayManager.Instance != null)
            {
                GameplayManager.Instance.OnStateChanged += HandleGameStateChanged;
            }
        }

        private void UnsubscribeFromGameplay()
        {
            if (GameplayManager.Instance != null)
            {
                GameplayManager.Instance.OnStateChanged -= HandleGameStateChanged;
            }
        }

        private void HandleGameStateChanged(GameState oldState, GameState newState)
        {
            if (CurrentState != MultiplayerState.Playing) return;

            if (newState == GameState.Defeat)
            {
                int round = GameplayManager.Instance != null ? GameplayManager.Instance.CurrentRound : 1;
                SendPlayerDead(round, 0);
            }
        }

        private void SendGameStateUpdate()
        {
            if (CurrentState != MultiplayerState.Playing) return;
            if (GameplayManager.Instance == null) return;

            wsClient.SendMessage(MessageType.GameStateUpdate, new GameStateUpdatePayload
            {
                life = GameplayManager.Instance.CurrentLife,
                round = GameplayManager.Instance.CurrentRound,
                gold = GameplayManager.Instance.CurrentGold,
                monstersKilled = 0,
                unitCount = 0
            });
        }
        #endregion

        #region WebSocket Handlers
        private void HandleConnected()
        {
            Debug.Log("[MultiplayerManager] Connected to server");
            SetState(MultiplayerState.InLobby);
            OnConnected?.Invoke();
        }

        private void HandleDisconnected(string reason)
        {
            Debug.Log($"[MultiplayerManager] Disconnected: {reason}");
            UnsubscribeFromGameplay();
            MultiplayerState previousState = CurrentState;
            SetState(MultiplayerState.Disconnected);
            OnDisconnected?.Invoke(reason);
        }

        private void HandleError(string error)
        {
            Debug.LogError($"[MultiplayerManager] Error: {error}");
            OnError?.Invoke(error);
        }

        private void HandleMessage(NetworkMessage message)
        {
            switch (message.type)
            {
                case MessageType.RoomCreated:
                    HandleRoomCreated(message);
                    break;
                case MessageType.PlayerJoined:
                    HandlePlayerJoined(message);
                    break;
                case MessageType.MatchStart:
                    HandleMatchStart(message);
                    break;
                case MessageType.WaveSync:
                    HandleWaveSync(message);
                    break;
                case MessageType.OpponentState:
                    HandleOpponentState(message);
                    break;
                case MessageType.OpponentDead:
                    HandleOpponentDead(message);
                    break;
                case MessageType.MatchResult:
                    HandleMatchResult(message);
                    break;
                case MessageType.Error:
                    HandleServerError(message);
                    break;
                default:
                    Debug.LogWarning($"[MultiplayerManager] Unknown message type: {message.type}");
                    break;
            }
        }
        #endregion

        #region Message Handlers
        private void HandleRoomCreated(NetworkMessage message)
        {
            var payload = message.GetPayload<RoomCreatedPayload>();
            RoomCode = payload.roomCode;
            SetState(MultiplayerState.InRoom);
            Debug.Log($"[MultiplayerManager] Room created: {RoomCode}");
            OnRoomCreated?.Invoke(RoomCode);
        }

        private void HandlePlayerJoined(NetworkMessage message)
        {
            var payload = message.GetPayload<PlayerJoinedPayload>();
            Debug.Log($"[MultiplayerManager] Player joined: {payload.playerName} ({payload.playerCount} players)");
            OnPlayerJoined?.Invoke(payload.playerName);
        }

        private void HandleMatchStart(NetworkMessage message)
        {
            SetState(MultiplayerState.Playing);
            stateReportTimer = stateReportInterval;
            SubscribeToGameplay();
            Debug.Log("[MultiplayerManager] Match started!");
            OnMatchStart?.Invoke();
        }

        private void HandleWaveSync(NetworkMessage message)
        {
            var payload = message.GetPayload<WaveSyncPayload>();
            Debug.Log($"[MultiplayerManager] Wave sync: round {payload.round}");
            OnWaveSync?.Invoke(payload.round);
        }

        private void HandleOpponentState(NetworkMessage message)
        {
            OpponentState = message.GetPayload<OpponentStatePayload>();
            OnOpponentStateUpdated?.Invoke(OpponentState);
        }

        private void HandleOpponentDead(NetworkMessage message)
        {
            var payload = message.GetPayload<OpponentDeadPayload>();
            Debug.Log($"[MultiplayerManager] Opponent dead: {payload.playerName} at round {payload.finalRound}");
            OnOpponentDead?.Invoke(payload.playerName, payload.finalRound);
        }

        private void HandleMatchResult(NetworkMessage message)
        {
            var payload = message.GetPayload<MatchResultPayload>();
            SetState(MultiplayerState.Result);
            Debug.Log($"[MultiplayerManager] Match result: {(payload.isWinner ? "WIN" : "LOSE")}");
            OnMatchResult?.Invoke(payload);
        }

        private void HandleServerError(NetworkMessage message)
        {
            var payload = message.GetPayload<ErrorPayload>();
            Debug.LogError($"[MultiplayerManager] Server error: [{payload.code}] {payload.message}");
            OnError?.Invoke(payload.message);
        }
        #endregion

        #region State Management
        private void SetState(MultiplayerState newState)
        {
            if (CurrentState == newState) return;
            Debug.Log($"[MultiplayerManager] State: {CurrentState} -> {newState}");
            CurrentState = newState;
        }
        #endregion
    }
}
