using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NativeWebSocket;
using LottoDefense.Config;
using LottoDefense.Security;

namespace LottoDefense.Networking
{
    /// <summary>
    /// WebSocket connection manager with authentication and auto-reconnect.
    /// Handles real-time communication for multiplayer and live updates.
    /// </summary>
    public class WebSocketManager : MonoBehaviour
    {
        #region Singleton
        private static WebSocketManager _instance;
        public static WebSocketManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("WebSocketManager");
                    _instance = go.AddComponent<WebSocketManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }
        #endregion

        #region Private Fields
        private WebSocket websocket;
        private string currentUrl;
        private bool isConnected = false;
        private bool shouldReconnect = true;
        private float reconnectDelay = 1f;
        private float maxReconnectDelay = 30f;
        private int reconnectAttempts = 0;
        private Queue<string> messageQueue = new Queue<string>();
        private Coroutine heartbeatCoroutine;
        private float heartbeatInterval = 30f;
        private float connectionTimeout = 10f;
        #endregion

        #region Events
        public event Action OnConnected;
        public event Action OnDisconnected;
        public event Action<string> OnError;
        public event Action<string> OnMessage;
        public event Action<byte[]> OnBinaryMessage;
        #endregion

        #region Public Properties
        public bool IsConnected => isConnected && websocket != null && websocket.State == WebSocketState.Open;
        public WebSocketState State => websocket?.State ?? WebSocketState.Closed;
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

        private void Update()
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            if (websocket != null)
            {
                websocket.DispatchMessageQueue();
            }
#endif
        }

        private async void OnDestroy()
        {
            shouldReconnect = false;
            StopHeartbeat();

            if (websocket != null && websocket.State == WebSocketState.Open)
            {
                await websocket.Close();
            }
        }

        private async void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                // App paused - disconnect
                shouldReconnect = false;
                if (websocket != null && websocket.State == WebSocketState.Open)
                {
                    await websocket.Close();
                }
            }
            else
            {
                // App resumed - reconnect
                shouldReconnect = true;
                if (websocket == null || websocket.State != WebSocketState.Open)
                {
                    Connect(currentUrl);
                }
            }
        }
        #endregion

        #region Connection Management
        /// <summary>
        /// Connect to WebSocket server with authentication
        /// </summary>
        public async void Connect(string endpoint = "/game")
        {
            // Get WebSocket URL from environment config
            if (EnvironmentManager.Instance?.Config == null)
            {
                Debug.LogError("[WebSocketManager] Environment configuration not loaded");
                OnError?.Invoke("Configuration not available");
                return;
            }

            currentUrl = EnvironmentManager.Instance.GetWebSocketEndpoint(endpoint);

            // Get authentication token
            string accessToken = SecureTokenStorage.Instance.GetAccessToken();

            if (string.IsNullOrEmpty(accessToken))
            {
                Debug.LogError("[WebSocketManager] No authentication token available");
                OnError?.Invoke("Authentication required");
                return;
            }

            // Add token to URL as query parameter
            currentUrl += $"?token={accessToken}";

            try
            {
                Debug.Log($"[WebSocketManager] Connecting to {endpoint}...");

                // Close existing connection if any
                if (websocket != null)
                {
                    await websocket.Close();
                }

                // Create new WebSocket connection
                websocket = new WebSocket(currentUrl);

                // Setup event handlers
                websocket.OnOpen += HandleOpen;
                websocket.OnError += HandleError;
                websocket.OnClose += HandleClose;
                websocket.OnMessage += HandleMessage;

                // Connect with timeout
                StartCoroutine(ConnectWithTimeout());

                await websocket.Connect();
            }
            catch (Exception e)
            {
                Debug.LogError($"[WebSocketManager] Connection failed: {e.Message}");
                OnError?.Invoke(e.Message);
                HandleReconnect();
            }
        }

        /// <summary>
        /// Disconnect from WebSocket server
        /// </summary>
        public async void Disconnect()
        {
            shouldReconnect = false;
            StopHeartbeat();

            if (websocket != null && websocket.State == WebSocketState.Open)
            {
                Debug.Log("[WebSocketManager] Disconnecting...");
                await websocket.Close();
            }

            isConnected = false;
            OnDisconnected?.Invoke();
        }

        /// <summary>
        /// Connection with timeout handling
        /// </summary>
        private IEnumerator ConnectWithTimeout()
        {
            float elapsed = 0f;

            while (elapsed < connectionTimeout)
            {
                if (websocket != null && websocket.State == WebSocketState.Open)
                {
                    yield break; // Successfully connected
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            // Timeout reached
            Debug.LogError("[WebSocketManager] Connection timeout");
            OnError?.Invoke("Connection timeout");
            HandleReconnect();
        }
        #endregion

        #region Event Handlers
        private void HandleOpen()
        {
            Debug.Log("[WebSocketManager] Connected successfully");
            isConnected = true;
            reconnectAttempts = 0;
            reconnectDelay = 1f;

            // Start heartbeat
            StartHeartbeat();

            // Send queued messages
            ProcessMessageQueue();

            OnConnected?.Invoke();
        }

        private void HandleError(string error)
        {
            Debug.LogError($"[WebSocketManager] Error: {error}");
            OnError?.Invoke(error);
        }

        private void HandleClose(WebSocketCloseCode code)
        {
            Debug.Log($"[WebSocketManager] Disconnected with code: {code}");
            isConnected = false;
            StopHeartbeat();

            OnDisconnected?.Invoke();

            // Check if token expired
            if (code == WebSocketCloseCode.UnsupportedData || !SecureTokenStorage.Instance.IsTokenValid())
            {
                Debug.Log("[WebSocketManager] Token expired, need re-authentication");
                OnError?.Invoke("Authentication expired");
                return;
            }

            // Auto-reconnect if enabled
            if (shouldReconnect)
            {
                HandleReconnect();
            }
        }

        private void HandleMessage(byte[] bytes)
        {
            try
            {
                string message = System.Text.Encoding.UTF8.GetString(bytes);

                if (EnvironmentManager.Instance?.Config?.VerboseLogging ?? false)
                {
                    Debug.Log($"[WebSocketManager] Received: {message}");
                }

                // Handle different message types
                var msgData = JsonUtility.FromJson<WebSocketMessage>(message);

                switch (msgData.type)
                {
                    case "ping":
                        SendPong();
                        break;

                    case "error":
                        Debug.LogError($"[WebSocketManager] Server error: {msgData.payload}");
                        OnError?.Invoke(msgData.payload);
                        break;

                    default:
                        OnMessage?.Invoke(message);
                        OnBinaryMessage?.Invoke(bytes);
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[WebSocketManager] Failed to handle message: {e.Message}");
            }
        }
        #endregion

        #region Message Sending
        /// <summary>
        /// Send text message
        /// </summary>
        public async void SendMessage(string message)
        {
            if (!IsConnected)
            {
                Debug.LogWarning("[WebSocketManager] Not connected, queueing message");
                messageQueue.Enqueue(message);
                return;
            }

            try
            {
                if (EnvironmentManager.Instance?.Config?.VerboseLogging ?? false)
                {
                    Debug.Log($"[WebSocketManager] Sending: {message}");
                }

                await websocket.SendText(message);
            }
            catch (Exception e)
            {
                Debug.LogError($"[WebSocketManager] Failed to send message: {e.Message}");
                OnError?.Invoke(e.Message);
            }
        }

        /// <summary>
        /// Send typed object as JSON
        /// </summary>
        public void SendJson<T>(T data) where T : class
        {
            string json = JsonUtility.ToJson(data);
            SendMessage(json);
        }

        /// <summary>
        /// Send binary data
        /// </summary>
        public async void SendBinary(byte[] data)
        {
            if (!IsConnected)
            {
                Debug.LogWarning("[WebSocketManager] Not connected");
                return;
            }

            try
            {
                await websocket.Send(data);
            }
            catch (Exception e)
            {
                Debug.LogError($"[WebSocketManager] Failed to send binary: {e.Message}");
                OnError?.Invoke(e.Message);
            }
        }

        /// <summary>
        /// Process queued messages
        /// </summary>
        private void ProcessMessageQueue()
        {
            while (messageQueue.Count > 0 && IsConnected)
            {
                string message = messageQueue.Dequeue();
                SendMessage(message);
            }
        }
        #endregion

        #region Heartbeat
        /// <summary>
        /// Start heartbeat to keep connection alive
        /// </summary>
        private void StartHeartbeat()
        {
            StopHeartbeat();
            heartbeatCoroutine = StartCoroutine(HeartbeatLoop());
        }

        /// <summary>
        /// Stop heartbeat
        /// </summary>
        private void StopHeartbeat()
        {
            if (heartbeatCoroutine != null)
            {
                StopCoroutine(heartbeatCoroutine);
                heartbeatCoroutine = null;
            }
        }

        /// <summary>
        /// Heartbeat loop
        /// </summary>
        private IEnumerator HeartbeatLoop()
        {
            while (IsConnected)
            {
                yield return new WaitForSeconds(heartbeatInterval);

                if (IsConnected)
                {
                    SendPing();
                }
            }
        }

        /// <summary>
        /// Send ping message
        /// </summary>
        private void SendPing()
        {
            var ping = new WebSocketMessage
            {
                type = "ping",
                payload = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()
            };

            SendJson(ping);
        }

        /// <summary>
        /// Send pong response
        /// </summary>
        private void SendPong()
        {
            var pong = new WebSocketMessage
            {
                type = "pong",
                payload = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()
            };

            SendJson(pong);
        }
        #endregion

        #region Reconnection
        /// <summary>
        /// Handle reconnection with exponential backoff
        /// </summary>
        private void HandleReconnect()
        {
            if (!shouldReconnect)
                return;

            reconnectAttempts++;
            float delay = Mathf.Min(reconnectDelay * Mathf.Pow(2, reconnectAttempts - 1), maxReconnectDelay);

            Debug.Log($"[WebSocketManager] Reconnecting in {delay} seconds (attempt {reconnectAttempts})");

            StartCoroutine(ReconnectAfterDelay(delay));
        }

        /// <summary>
        /// Reconnect after delay
        /// </summary>
        private IEnumerator ReconnectAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);

            if (shouldReconnect && !IsConnected)
            {
                // Check if token needs refresh before reconnecting
                if (SecureTokenStorage.Instance.NeedsTokenRefresh())
                {
                    Debug.Log("[WebSocketManager] Token needs refresh before reconnect");
                    OnError?.Invoke("Token refresh required");
                    yield break;
                }

                Connect();
            }
        }

        /// <summary>
        /// Reset connection
        /// </summary>
        public void ResetConnection()
        {
            shouldReconnect = true;
            reconnectAttempts = 0;
            reconnectDelay = 1f;

            if (!IsConnected)
            {
                Connect();
            }
        }
        #endregion

        #region Room Management
        /// <summary>
        /// Join a game room
        /// </summary>
        public void JoinRoom(string roomId)
        {
            var joinMsg = new RoomMessage
            {
                type = "join_room",
                roomId = roomId,
                playerId = SystemInfo.deviceUniqueIdentifier
            };

            SendJson(joinMsg);
        }

        /// <summary>
        /// Leave current room
        /// </summary>
        public void LeaveRoom(string roomId)
        {
            var leaveMsg = new RoomMessage
            {
                type = "leave_room",
                roomId = roomId,
                playerId = SystemInfo.deviceUniqueIdentifier
            };

            SendJson(leaveMsg);
        }

        /// <summary>
        /// Send game state update
        /// </summary>
        public void SendGameState(object gameState)
        {
            var stateMsg = new GameStateMessage
            {
                type = "game_state",
                state = JsonUtility.ToJson(gameState),
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            SendJson(stateMsg);
        }
        #endregion

        #region Message Classes
        [Serializable]
        private class WebSocketMessage
        {
            public string type;
            public string payload;
        }

        [Serializable]
        private class RoomMessage
        {
            public string type;
            public string roomId;
            public string playerId;
        }

        [Serializable]
        private class GameStateMessage
        {
            public string type;
            public string state;
            public long timestamp;
        }
        #endregion
    }
}