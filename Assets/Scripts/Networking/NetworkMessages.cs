using System;
using UnityEngine;

namespace LottoDefense.Networking
{
    /// <summary>
    /// All message type identifiers for client-server communication.
    /// </summary>
    public static class MessageType
    {
        // Client -> Server
        public const string RoomCreate = "room_create";
        public const string RoomJoin = "room_join";
        public const string RoomAutoMatch = "room_auto_match";
        public const string PlayerReady = "player_ready";
        public const string GameStateUpdate = "game_state_update";
        public const string PlayerDead = "player_dead";
        public const string Heartbeat = "heartbeat";

        // Server -> Client
        public const string RoomCreated = "room_created";
        public const string PlayerJoined = "player_joined";
        public const string MatchStart = "match_start";
        public const string WaveSync = "wave_sync";
        public const string OpponentState = "opponent_state";
        public const string OpponentDead = "opponent_dead";
        public const string MatchResult = "match_result";
        public const string Error = "error";
    }

    #region Base Message
    /// <summary>
    /// Base envelope for all messages. The "type" field is used to route messages.
    /// </summary>
    [Serializable]
    public class NetworkMessage
    {
        public string type;
        public string payload;

        public static NetworkMessage Create<T>(string messageType, T data)
        {
            return new NetworkMessage
            {
                type = messageType,
                payload = JsonUtility.ToJson(data)
            };
        }

        public T GetPayload<T>()
        {
            return JsonUtility.FromJson<T>(payload);
        }
    }
    #endregion

    #region Client -> Server Payloads
    [Serializable]
    public class RoomCreatePayload
    {
        public string playerName;
    }

    [Serializable]
    public class RoomJoinPayload
    {
        public string roomCode;
        public string playerName;
    }

    [Serializable]
    public class RoomAutoMatchPayload
    {
        public string playerName;
    }

    [Serializable]
    public class PlayerReadyPayload
    {
        public string roomCode;
    }

    [Serializable]
    public class GameStateUpdatePayload
    {
        public int life;
        public int round;
        public int gold;
        public int monstersKilled;
        public int unitCount;
    }

    [Serializable]
    public class PlayerDeadPayload
    {
        public int finalRound;
        public int contribution;
    }

    [Serializable]
    public class HeartbeatPayload
    {
        public long timestamp;
    }
    #endregion

    #region Server -> Client Payloads
    [Serializable]
    public class RoomCreatedPayload
    {
        public string roomCode;
        public string roomId;
    }

    [Serializable]
    public class PlayerJoinedPayload
    {
        public string playerName;
        public int playerCount;
    }

    [Serializable]
    public class MatchStartPayload
    {
        public int totalPlayers;
        public int startRound;
    }

    [Serializable]
    public class WaveSyncPayload
    {
        public int round;
    }

    [Serializable]
    public class OpponentStatePayload
    {
        public string playerName;
        public int life;
        public int round;
        public int gold;
        public int monstersKilled;
        public int unitCount;
        public bool isAlive;
    }

    [Serializable]
    public class OpponentDeadPayload
    {
        public string playerName;
        public int finalRound;
    }

    [Serializable]
    public class MatchResultPayload
    {
        public bool isWinner;
        public int myRound;
        public int opponentRound;
        public int myContribution;
        public int opponentContribution;
    }

    [Serializable]
    public class ErrorPayload
    {
        public string code;
        public string message;
    }
    #endregion
}
