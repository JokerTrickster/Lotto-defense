using UnityEngine;
using System.Collections.Generic;
using LottoDefense.Monsters;

namespace LottoDefense.Gameplay
{
    /// <summary>
    /// 특정 라운드에 등장할 몬스터 설정.
    /// </summary>
    [System.Serializable]
    public class RoundMonsterConfig
    {
        [Tooltip("라운드 번호 (1부터 시작)")]
        public int roundNumber = 1;

        [Tooltip("이 라운드에 등장할 몬스터 (1종류만)")]
        public MonsterData monsterData;

        [Header("스폰 설정")]
        [Tooltip("이 라운드에서 스폰할 총 몬스터 수")]
        [Min(1)]
        public int totalMonsters = 30;

        [Tooltip("스폰 간격 (초)")]
        [Min(0.1f)]
        public float spawnInterval = 0.5f;

        [Tooltip("스폰 지속 시간 (초)")]
        [Min(1f)]
        public float spawnDuration = 15f;
    }

    /// <summary>
    /// 게임의 모든 라운드 설정을 관리하는 ScriptableObject.
    /// Unity 에디터에서 쉽게 편집 가능합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "RoundConfig", menuName = "Lotto Defense/Round Config", order = 2)]
    public class RoundConfig : ScriptableObject
    {
        #region Inspector Fields
        [Header("게임 설정")]
        [Tooltip("총 라운드 수")]
        [Min(1)]
        [SerializeField] private int totalRounds = 30;

        [Header("라운드별 몬스터 설정")]
        [Tooltip("각 라운드의 몬스터 설정 리스트")]
        [SerializeField] private List<RoundMonsterConfig> roundConfigs = new List<RoundMonsterConfig>();

        [Header("기본 설정 (라운드 설정이 없을 때)")]
        [Tooltip("설정되지 않은 라운드에 사용할 기본 몬스터")]
        [SerializeField] private MonsterData defaultMonster;

        [Tooltip("기본 스폰 수")]
        [SerializeField] private int defaultTotalMonsters = 30;

        [Tooltip("기본 스폰 간격")]
        [SerializeField] private float defaultSpawnInterval = 0.5f;

        [Tooltip("기본 스폰 지속 시간")]
        [SerializeField] private float defaultSpawnDuration = 15f;
        #endregion

        #region Properties
        /// <summary>
        /// 총 라운드 수.
        /// </summary>
        public int TotalRounds => totalRounds;
        #endregion

        #region Public Methods
        /// <summary>
        /// 특정 라운드의 몬스터 설정 가져오기.
        /// </summary>
        /// <param name="roundNumber">라운드 번호 (1부터 시작)</param>
        /// <returns>라운드 몬스터 설정 (없으면 기본값 반환)</returns>
        public RoundMonsterConfig GetRoundConfig(int roundNumber)
        {
            // 설정된 라운드 찾기
            RoundMonsterConfig config = roundConfigs.Find(r => r.roundNumber == roundNumber);

            // 설정이 있으면 반환
            if (config != null && config.monsterData != null)
            {
                return config;
            }

            // 설정이 없으면 기본값 생성
            Debug.LogWarning($"[RoundConfig] Round {roundNumber} configuration not found, using default settings");
            return new RoundMonsterConfig
            {
                roundNumber = roundNumber,
                monsterData = defaultMonster,
                totalMonsters = defaultTotalMonsters,
                spawnInterval = defaultSpawnInterval,
                spawnDuration = defaultSpawnDuration
            };
        }

        /// <summary>
        /// 특정 라운드의 몬스터 데이터 가져오기 (간편 메서드).
        /// </summary>
        /// <param name="roundNumber">라운드 번호</param>
        /// <returns>MonsterData</returns>
        public MonsterData GetMonsterForRound(int roundNumber)
        {
            RoundMonsterConfig config = GetRoundConfig(roundNumber);
            return config.monsterData;
        }

        /// <summary>
        /// 특정 라운드의 스폰 수 가져오기.
        /// </summary>
        public int GetTotalMonstersForRound(int roundNumber)
        {
            RoundMonsterConfig config = GetRoundConfig(roundNumber);
            return config.totalMonsters;
        }

        /// <summary>
        /// 특정 라운드의 스폰 간격 가져오기.
        /// </summary>
        public float GetSpawnIntervalForRound(int roundNumber)
        {
            RoundMonsterConfig config = GetRoundConfig(roundNumber);
            return config.spawnInterval;
        }

        /// <summary>
        /// 특정 라운드의 스폰 지속 시간 가져오기.
        /// </summary>
        public float GetSpawnDurationForRound(int roundNumber)
        {
            RoundMonsterConfig config = GetRoundConfig(roundNumber);
            return config.spawnDuration;
        }

        /// <summary>
        /// 모든 라운드 설정 미리보기 (디버깅용).
        /// </summary>
        public void LogRoundProgression()
        {
            Debug.Log($"[RoundConfig] Total Rounds: {totalRounds}");
            Debug.Log("[RoundConfig] Round Progression:");

            for (int i = 1; i <= totalRounds; i++)
            {
                RoundMonsterConfig config = GetRoundConfig(i);
                string monsterName = config.monsterData != null ? config.monsterData.monsterName : "None";
                Debug.Log($"Round {i}: {monsterName} (x{config.totalMonsters}, Interval: {config.spawnInterval}s, Duration: {config.spawnDuration}s)");
            }
        }

        /// <summary>
        /// 라운드 설정이 유효한지 검증.
        /// </summary>
        public bool ValidateRoundConfig(int roundNumber)
        {
            if (roundNumber < 1 || roundNumber > totalRounds)
            {
                Debug.LogError($"[RoundConfig] Invalid round number: {roundNumber} (valid range: 1-{totalRounds})");
                return false;
            }

            RoundMonsterConfig config = GetRoundConfig(roundNumber);
            if (config.monsterData == null)
            {
                Debug.LogError($"[RoundConfig] Round {roundNumber} has no monster data!");
                return false;
            }

            return true;
        }
        #endregion

        #region Validation
        /// <summary>
        /// Unity 에디터에서 값 검증.
        /// </summary>
        private void OnValidate()
        {
            totalRounds = Mathf.Max(1, totalRounds);
            defaultTotalMonsters = Mathf.Max(1, defaultTotalMonsters);
            defaultSpawnInterval = Mathf.Max(0.1f, defaultSpawnInterval);
            defaultSpawnDuration = Mathf.Max(1f, defaultSpawnDuration);

            // 중복 라운드 번호 체크
            HashSet<int> roundNumbers = new HashSet<int>();
            foreach (var config in roundConfigs)
            {
                if (roundNumbers.Contains(config.roundNumber))
                {
                    Debug.LogWarning($"[RoundConfig] Duplicate round number detected: {config.roundNumber}");
                }
                roundNumbers.Add(config.roundNumber);
            }
        }
        #endregion

        #region Editor Helpers
#if UNITY_EDITOR
        /// <summary>
        /// 에디터에서 자동으로 라운드 설정 생성 (헬퍼 메서드).
        /// </summary>
        [ContextMenu("Auto-Generate Round Configs")]
        private void AutoGenerateRoundConfigs()
        {
            roundConfigs.Clear();

            for (int i = 1; i <= totalRounds; i++)
            {
                roundConfigs.Add(new RoundMonsterConfig
                {
                    roundNumber = i,
                    monsterData = defaultMonster,
                    totalMonsters = defaultTotalMonsters,
                    spawnInterval = defaultSpawnInterval,
                    spawnDuration = defaultSpawnDuration
                });
            }

            Debug.Log($"[RoundConfig] Auto-generated {totalRounds} round configs");
            UnityEditor.EditorUtility.SetDirty(this);
        }

        /// <summary>
        /// 라운드 번호 순으로 정렬.
        /// </summary>
        [ContextMenu("Sort Rounds by Number")]
        private void SortRoundConfigs()
        {
            roundConfigs.Sort((a, b) => a.roundNumber.CompareTo(b.roundNumber));
            Debug.Log("[RoundConfig] Sorted round configs by round number");
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
        #endregion
    }
}
