using System;
using System.Collections;
using UnityEngine;
using LottoDefense.Backend;
using LottoDefense.Gameplay;
using LottoDefense.Monsters;

namespace LottoDefense.Networking
{
    /// <summary>
    /// REST polling을 통한 협동 플레이 상태 동기화
    /// 3초마다 내 상태를 서버에 전송하고 상대방 상태를 수신
    /// </summary>
    public class CoopStateSync : MonoBehaviour
    {
        #region Singleton
        private static CoopStateSync _instance;
        
        public static CoopStateSync Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("CoopStateSync");
                    _instance = go.AddComponent<CoopStateSync>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }
        #endregion

        #region Properties
        private uint roomID;
        private float syncInterval = 3f; // 3초마다 동기화
        private Coroutine syncCoroutine;
        private bool isActive = false;
        private APIClient apiClient;

        public OpponentStateData OpponentState { get; private set; }
        public bool IsEnabled => isActive;
        #endregion

        #region Events
        public event Action<OpponentStateData> OnOpponentStateUpdated;
        public event Action OnOpponentDefeated;
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
        #endregion

        #region Public Methods
        /// <summary>
        /// 협동 플레이 동기화 시작
        /// </summary>
        public void StartSync(uint roomID, APIClient client)
        {
            this.roomID = roomID;
            this.apiClient = client;
            isActive = true;
            
            if (syncCoroutine != null)
            {
                StopCoroutine(syncCoroutine);
            }
            
            syncCoroutine = StartCoroutine(SyncLoop());
            Debug.Log($"[CoopStateSync] Started syncing for room {roomID}");
        }

        /// <summary>
        /// 협동 플레이 동기화 중지
        /// </summary>
        public void StopSync()
        {
            isActive = false;
            
            if (syncCoroutine != null)
            {
                StopCoroutine(syncCoroutine);
                syncCoroutine = null;
            }
            
            Debug.Log("[CoopStateSync] Stopped syncing");
        }

        /// <summary>
        /// 동기화 간격 변경 (중요 이벤트 시 1초로 단축)
        /// </summary>
        public void SetSyncInterval(float seconds)
        {
            syncInterval = Mathf.Max(1f, seconds);
        }
        #endregion

        #region Private Methods
        private IEnumerator SyncLoop()
        {
            while (isActive)
            {
                // 1. 내 상태 전송
                yield return UpdateMyState();
                
                // 2. 0.5초 대기
                yield return new WaitForSeconds(0.5f);
                
                // 3. 상대방 상태 수신
                yield return GetOpponentState();
                
                // 4. 다음 동기화까지 대기
                yield return new WaitForSeconds(syncInterval - 0.5f);
            }
        }

        private IEnumerator UpdateMyState()
        {
            if (GameplayManager.Instance == null || apiClient == null)
            {
                yield break;
            }

            var state = new UpdateGameStateRequest
            {
                round = GameplayManager.Instance.CurrentRound,
                hp = GameplayManager.Instance.CurrentLife,
                gold = GameplayManager.Instance.CurrentGold,
                kills = MonsterManager.Instance != null ? MonsterManager.Instance.TotalMonstersKilled : 0,
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            bool success = false;
            string error = null;

            yield return apiClient.Post(
                $"/coop/rooms/{roomID}/state",
                state,
                (string response) => {
                    success = true;
                    Debug.Log("[CoopStateSync] State updated");
                },
                (string err) => {
                    error = err;
                }
            );

            if (!success && error != null)
            {
                Debug.LogWarning($"[CoopStateSync] State update failed: {error}");
            }
        }

        private IEnumerator GetOpponentState()
        {
            if (apiClient == null)
            {
                yield break;
            }

            bool success = false;
            string error = null;

            yield return apiClient.Get(
                $"/coop/rooms/{roomID}/opponent-state",
                (OpponentStateResponse response) => {
                    success = true;
                    
                    // 이전 상태와 비교
                    bool wasAlive = OpponentState != null && OpponentState.is_alive;
                    
                    // 상태 업데이트
                    OpponentState = new OpponentStateData
                    {
                        opponent_id = response.opponent_id,
                        opponent_name = response.opponent_name,
                        round = response.round,
                        hp = response.hp,
                        gold = response.gold,
                        kills = response.kills,
                        last_update = response.last_update,
                        is_alive = response.is_alive
                    };

                    // 이벤트 발생
                    OnOpponentStateUpdated?.Invoke(OpponentState);

                    // 상대방이 처음으로 사망했을 때
                    if (wasAlive && !response.is_alive)
                    {
                        Debug.Log($"[CoopStateSync] Opponent {response.opponent_name} defeated!");
                        OnOpponentDefeated?.Invoke();
                    }
                },
                (string err) => {
                    error = err;
                }
            );

            if (!success && error != null)
            {
                // 상대방이 아직 상태를 보내지 않았을 수 있음
                if (!error.Contains("opponent not found"))
                {
                    Debug.LogWarning($"[CoopStateSync] Opponent state failed: {error}");
                }
            }
        }
        #endregion

        #region Unit Placement Sync
        /// <summary>
        /// Send unit placement information to server
        /// </summary>
        public void SendUnitPlacement(int playerNumber, Vector2Int position, string unitType)
        {
            // This would typically send to server via API
            // For now, just log the action
            Debug.Log($"[CoopStateSync] Player {playerNumber} placed {unitType} at {position}");

            // In a real implementation:
            // StartCoroutine(SendUnitPlacementToServer(playerNumber, position, unitType));
        }

        /// <summary>
        /// Send unit removal information to server
        /// </summary>
        public void SendUnitRemoval(int playerNumber, Vector2Int position)
        {
            // This would typically send to server via API
            // For now, just log the action
            Debug.Log($"[CoopStateSync] Player {playerNumber} removed unit at {position}");

            // In a real implementation:
            // StartCoroutine(SendUnitRemovalToServer(playerNumber, position));
        }
        #endregion
    }

    #region Data Models
    [Serializable]
    public class UpdateGameStateRequest
    {
        public int round;
        public int hp;
        public int gold;
        public int kills;
        public long timestamp;
    }

    [Serializable]
    public class OpponentStateResponse
    {
        public uint opponent_id;
        public string opponent_name;
        public int round;
        public int hp;
        public int gold;
        public int kills;
        public long last_update;
        public bool is_alive;
    }

    [Serializable]
    public class OpponentStateData
    {
        public uint opponent_id;
        public string opponent_name;
        public int round;
        public int hp;
        public int gold;
        public int kills;
        public long last_update;
        public bool is_alive;
    }
    #endregion
}
