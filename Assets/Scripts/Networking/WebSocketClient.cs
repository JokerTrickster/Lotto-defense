using System;
using System.Collections;
using UnityEngine;
using NativeWebSocket;

namespace LottoDefense.Networking
{
    /// <summary>
    /// Low-level WebSocket wrapper using NativeWebSocket.
    /// Handles connection lifecycle, message dispatch, and automatic reconnection.
    /// </summary>
    public class WebSocketClient : MonoBehaviour
    {
        #region Events
        public event Action OnConnected;
        public event Action<string> OnDisconnected;
        public event Action<NetworkMessage> OnMessageReceived;
        public event Action<string> OnError;
        #endregion

        #region Private Fields
        private WebSocket websocket;
        private string serverUrl;
        private bool isConnecting;
        private bool intentionalDisconnect;
        private Coroutine heartbeatCoroutine;

        private const float HEARTBEAT_INTERVAL = 15f;
        private const float RECONNECT_DELAY = 3f;
        private const int MAX_RECONNECT_ATTEMPTS = 5;
        private int reconnectAttempts;
        #endregion

        #region Properties
        public bool IsConnected => websocket != null && websocket.State == WebSocketState.Open;
        #endregion

        #region Connection
        public async void Connect(string url)
        {
            if (isConnecting || IsConnected)
            {
                Debug.LogWarning("[WebSocketClient] Already connected or connecting");
                return;
            }

            serverUrl = url;
            isConnecting = true;
            intentionalDisconnect = false;
            reconnectAttempts = 0;

            Debug.Log($"[WebSocketClient] Connecting to {url}");

            try
            {
                websocket = new WebSocket(url);

                websocket.OnOpen += HandleOpen;
                websocket.OnMessage += HandleMessage;
                websocket.OnError += HandleError;
                websocket.OnClose += HandleClose;

                await websocket.Connect();
            }
            catch (Exception e)
            {
                isConnecting = false;
                Debug.LogError($"[WebSocketClient] Connection failed: {e.Message}");
                OnError?.Invoke(e.Message);
                TryReconnect();
            }
        }

        public async void Disconnect()
        {
            intentionalDisconnect = true;
            StopHeartbeat();

            if (websocket != null && websocket.State == WebSocketState.Open)
            {
                Debug.Log("[WebSocketClient] Disconnecting...");
                await websocket.Close();
            }

            websocket = null;
        }
        #endregion

        #region Message Sending
        public async void SendMessage(NetworkMessage message)
        {
            if (!IsConnected)
            {
                Debug.LogWarning("[WebSocketClient] Cannot send message - not connected");
                return;
            }

            try
            {
                string json = JsonUtility.ToJson(message);
                await websocket.SendText(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[WebSocketClient] Send failed: {e.Message}");
                OnError?.Invoke(e.Message);
            }
        }

        public void SendMessage<T>(string type, T payload)
        {
            SendMessage(NetworkMessage.Create(type, payload));
        }
        #endregion

        #region Unity Lifecycle
        private void Update()
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            websocket?.DispatchMessageQueue();
#endif
        }

        private void OnDestroy()
        {
            Disconnect();
        }
        #endregion

        #region WebSocket Handlers
        private void HandleOpen()
        {
            isConnecting = false;
            reconnectAttempts = 0;
            Debug.Log("[WebSocketClient] Connected");
            OnConnected?.Invoke();
            StartHeartbeat();
        }

        private void HandleMessage(byte[] data)
        {
            string json = System.Text.Encoding.UTF8.GetString(data);

            try
            {
                NetworkMessage message = JsonUtility.FromJson<NetworkMessage>(json);
                if (message != null && !string.IsNullOrEmpty(message.type))
                {
                    OnMessageReceived?.Invoke(message);
                }
                else
                {
                    Debug.LogWarning($"[WebSocketClient] Invalid message received: {json}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[WebSocketClient] Message parse error: {e.Message}, json: {json}");
            }
        }

        private void HandleError(string error)
        {
            isConnecting = false;
            Debug.LogError($"[WebSocketClient] Error: {error}");
            OnError?.Invoke(error);
        }

        private void HandleClose(WebSocketCloseCode closeCode)
        {
            isConnecting = false;
            Debug.Log($"[WebSocketClient] Closed: {closeCode}");
            StopHeartbeat();
            OnDisconnected?.Invoke(closeCode.ToString());

            if (!intentionalDisconnect)
            {
                TryReconnect();
            }
        }
        #endregion

        #region Heartbeat
        private void StartHeartbeat()
        {
            StopHeartbeat();
            heartbeatCoroutine = StartCoroutine(HeartbeatRoutine());
        }

        private void StopHeartbeat()
        {
            if (heartbeatCoroutine != null)
            {
                StopCoroutine(heartbeatCoroutine);
                heartbeatCoroutine = null;
            }
        }

        private IEnumerator HeartbeatRoutine()
        {
            while (IsConnected)
            {
                yield return new WaitForSecondsRealtime(HEARTBEAT_INTERVAL);

                if (IsConnected)
                {
                    SendMessage(MessageType.Heartbeat, new HeartbeatPayload
                    {
                        timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                    });
                }
            }
        }
        #endregion

        #region Reconnection
        private void TryReconnect()
        {
            if (intentionalDisconnect || string.IsNullOrEmpty(serverUrl))
                return;

            if (reconnectAttempts >= MAX_RECONNECT_ATTEMPTS)
            {
                Debug.LogWarning("[WebSocketClient] Max reconnect attempts reached");
                OnError?.Invoke("Max reconnect attempts reached");
                return;
            }

            reconnectAttempts++;
            Debug.Log($"[WebSocketClient] Reconnecting ({reconnectAttempts}/{MAX_RECONNECT_ATTEMPTS})...");
            StartCoroutine(ReconnectRoutine());
        }

        private IEnumerator ReconnectRoutine()
        {
            yield return new WaitForSecondsRealtime(RECONNECT_DELAY);

            if (!intentionalDisconnect && !IsConnected)
            {
                Connect(serverUrl);
            }
        }
        #endregion
    }
}
