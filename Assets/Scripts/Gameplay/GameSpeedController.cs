using UnityEngine;
using UnityEngine.UI;

namespace LottoDefense.Gameplay
{
    /// <summary>
    /// 게임 배속 조절 (×1, ×2)
    /// Time.timeScale을 조정하여 게임 속도 변경
    /// </summary>
    public class GameSpeedController : MonoBehaviour
    {
        #region Singleton
        private static GameSpeedController _instance;
        public static GameSpeedController Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<GameSpeedController>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("GameSpeedController");
                        _instance = go.AddComponent<GameSpeedController>();
                    }
                }
                return _instance;
            }
        }
        #endregion

        #region Fields
        private float currentSpeed = 1.0f;
        private readonly float[] availableSpeeds = { 1.0f, 2.0f };
        private int currentSpeedIndex = 0;
        #endregion

        #region Properties
        /// <summary>
        /// 현재 게임 배속 (1.0 = 정상, 2.0 = 2배속)
        /// </summary>
        public float CurrentSpeed => currentSpeed;

        /// <summary>
        /// 현재 배속 텍스트 (×1, ×2)
        /// </summary>
        public string CurrentSpeedText => $"×{currentSpeed:F0}";
        #endregion

        #region Events
        /// <summary>
        /// 배속 변경 시 발생하는 이벤트
        /// float: 새 배속 값
        /// </summary>
        public event System.Action<float> OnSpeedChanged;
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

            // 초기 배속 설정
            SetSpeed(1.0f);
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                // 게임 종료 시 배속 복구
                Time.timeScale = 1.0f;
                _instance = null;
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// 다음 배속으로 전환 (×1 → ×2 → ×1 ...)
        /// </summary>
        public void ToggleSpeed()
        {
            currentSpeedIndex = (currentSpeedIndex + 1) % availableSpeeds.Length;
            float newSpeed = availableSpeeds[currentSpeedIndex];
            SetSpeed(newSpeed);
        }

        /// <summary>
        /// 특정 배속으로 설정
        /// </summary>
        /// <param name="speed">배속 (1.0, 2.0)</param>
        public void SetSpeed(float speed)
        {
            // 유효한 배속인지 확인
            bool isValid = false;
            for (int i = 0; i < availableSpeeds.Length; i++)
            {
                if (Mathf.Approximately(availableSpeeds[i], speed))
                {
                    currentSpeedIndex = i;
                    isValid = true;
                    break;
                }
            }

            if (!isValid)
            {
                Debug.LogWarning($"[GameSpeedController] Invalid speed: {speed}, using 1.0");
                speed = 1.0f;
                currentSpeedIndex = 0;
            }

            currentSpeed = speed;
            Time.timeScale = speed;

            Debug.Log($"[GameSpeedController] Game speed changed: {CurrentSpeedText}");

            // 이벤트 발생
            OnSpeedChanged?.Invoke(currentSpeed);
        }

        /// <summary>
        /// 배속 초기화 (×1)
        /// </summary>
        public void ResetSpeed()
        {
            SetSpeed(1.0f);
            currentSpeedIndex = 0;
        }
        #endregion
    }
}
