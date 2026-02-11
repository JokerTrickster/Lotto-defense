using UnityEngine;
using System.Collections;

namespace LottoDefense.Gameplay
{
    /// <summary>
    /// Singleton manager handling mobile-specific optimizations.
    /// Enforces portrait orientation, validates touch targets, and monitors performance.
    /// </summary>
    public class MobileOptimizationManager : MonoBehaviour
    {
        #region Singleton
        private static MobileOptimizationManager _instance;

        /// <summary>
        /// Global access point for the MobileOptimizationManager singleton.
        /// </summary>
        public static MobileOptimizationManager Instance
        {
            get
            {
                if (GameplayManager.IsCleaningUp) return null;

                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<MobileOptimizationManager>();

                    if (_instance == null)
                    {
                        GameObject go = new GameObject("MobileOptimizationManager");
                        _instance = go.AddComponent<MobileOptimizationManager>();
                    }
                }
                return _instance;
            }
        }
        #endregion

        #region Inspector Fields
        [Header("Mobile Settings")]
        [Tooltip("Enforce portrait orientation")]
        [SerializeField] private bool enforcePortraitOrientation = true;

        [Tooltip("Target frame rate (60 recommended)")]
        [SerializeField] private int targetFrameRate = 60;

        [Tooltip("Enable VSync")]
        [SerializeField] private bool enableVSync = true;

        [Header("Touch Target Validation")]
        [Tooltip("Minimum touch target size in points (Apple HIG: 44pt)")]
        [SerializeField] private float minTouchTargetSize = 44f;

        [Tooltip("Show warnings for undersized touch targets")]
        [SerializeField] private bool validateTouchTargets = true;

        [Header("Performance Monitoring")]
        [Tooltip("Enable performance monitoring")]
        [SerializeField] private bool enablePerformanceMonitoring = true;

        [Tooltip("FPS warning threshold")]
        [SerializeField] private float fpsWarningThreshold = 55f;

        [Tooltip("Performance check interval (seconds)")]
        [SerializeField] private float performanceCheckInterval = 1f;
        #endregion

        #region Performance Stats
        /// <summary>
        /// Current FPS.
        /// </summary>
        public float CurrentFPS { get; private set; }

        /// <summary>
        /// Average FPS over last second.
        /// </summary>
        public float AverageFPS { get; private set; }

        /// <summary>
        /// Minimum FPS recorded.
        /// </summary>
        public float MinFPS { get; private set; } = float.MaxValue;

        /// <summary>
        /// Maximum FPS recorded.
        /// </summary>
        public float MaxFPS { get; private set; } = 0f;

        /// <summary>
        /// Current memory usage in MB.
        /// </summary>
        public float CurrentMemoryMB => (UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong() / 1024f / 1024f);
        #endregion

        #region Private Fields
        private float[] fpsBuffer = new float[60];
        private int fpsBufferIndex = 0;
        private Coroutine performanceMonitorCoroutine;
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

            Initialize();
        }

        private void Start()
        {
            if (validateTouchTargets)
            {
                ValidateAllTouchTargets();
            }
        }

        private void Update()
        {
            // Update FPS tracking
            UpdateFPSTracking();
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize mobile optimization settings.
        /// </summary>
        private void Initialize()
        {
            // Set orientation
            if (enforcePortraitOrientation)
            {
                Screen.orientation = ScreenOrientation.Portrait;
                Screen.autorotateToPortrait = true;
                Screen.autorotateToPortraitUpsideDown = false;
                Screen.autorotateToLandscapeLeft = false;
                Screen.autorotateToLandscapeRight = false;

                Debug.Log("[MobileOptimization] Portrait orientation enforced");
            }

            // Set frame rate
            Application.targetFrameRate = targetFrameRate;
            QualitySettings.vSyncCount = enableVSync ? 1 : 0;

            Debug.Log($"[MobileOptimization] Target FPS: {targetFrameRate}, VSync: {enableVSync}");

            // Start performance monitoring
            if (enablePerformanceMonitoring)
            {
                performanceMonitorCoroutine = StartCoroutine(PerformanceMonitorRoutine());
            }

            // Initialize FPS buffer
            for (int i = 0; i < fpsBuffer.Length; i++)
            {
                fpsBuffer[i] = targetFrameRate;
            }

            Debug.Log("[MobileOptimization] Initialized");
        }
        #endregion

        #region FPS Tracking
        /// <summary>
        /// Update FPS tracking every frame.
        /// </summary>
        private void UpdateFPSTracking()
        {
            float currentFPS = 1f / Time.unscaledDeltaTime;
            CurrentFPS = currentFPS;

            // Update buffer
            fpsBuffer[fpsBufferIndex] = currentFPS;
            fpsBufferIndex = (fpsBufferIndex + 1) % fpsBuffer.Length;

            // Calculate average
            float sum = 0f;
            for (int i = 0; i < fpsBuffer.Length; i++)
            {
                sum += fpsBuffer[i];
            }
            AverageFPS = sum / fpsBuffer.Length;

            // Update min/max
            MinFPS = Mathf.Min(MinFPS, currentFPS);
            MaxFPS = Mathf.Max(MaxFPS, currentFPS);
        }
        #endregion

        #region Performance Monitoring
        /// <summary>
        /// Coroutine monitoring performance at intervals.
        /// </summary>
        private IEnumerator PerformanceMonitorRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(performanceCheckInterval);

                // Check FPS
                if (AverageFPS < fpsWarningThreshold)
                {
                    Debug.LogWarning($"[MobileOptimization] Performance warning: {AverageFPS:F1} FPS (threshold: {fpsWarningThreshold})");
                }

                // Check memory
                float memoryMB = CurrentMemoryMB;
                if (memoryMB > 500f)
                {
                    Debug.LogWarning($"[MobileOptimization] High memory usage: {memoryMB:F1} MB");
                }
            }
        }

        /// <summary>
        /// Get performance statistics as formatted string.
        /// </summary>
        /// <returns>Performance stats string</returns>
        public string GetPerformanceStats()
        {
            return $"FPS: {AverageFPS:F1} (Min: {MinFPS:F1}, Max: {MaxFPS:F1}) | Memory: {CurrentMemoryMB:F1} MB";
        }

        /// <summary>
        /// Reset performance statistics.
        /// </summary>
        public void ResetPerformanceStats()
        {
            MinFPS = float.MaxValue;
            MaxFPS = 0f;

            for (int i = 0; i < fpsBuffer.Length; i++)
            {
                fpsBuffer[i] = targetFrameRate;
            }

            Debug.Log("[MobileOptimization] Performance stats reset");
        }
        #endregion

        #region Touch Target Validation
        /// <summary>
        /// Validate all touch targets in the scene.
        /// </summary>
        public void ValidateAllTouchTargets()
        {
            int warningCount = 0;

            // Check all UI buttons
            UnityEngine.UI.Button[] buttons = FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.None);
            foreach (var button in buttons)
            {
                if (!ValidateTouchTarget(button.GetComponent<RectTransform>()))
                {
                    Debug.LogWarning($"[MobileOptimization] Touch target too small: {button.gameObject.name} - Size: {button.GetComponent<RectTransform>().sizeDelta}");
                    warningCount++;
                }
            }

            if (warningCount == 0)
            {
                Debug.Log($"[MobileOptimization] All {buttons.Length} touch targets validated successfully");
            }
            else
            {
                Debug.LogWarning($"[MobileOptimization] {warningCount}/{buttons.Length} touch targets below minimum size");
            }
        }

        /// <summary>
        /// Validate individual touch target size.
        /// </summary>
        /// <param name="rectTransform">RectTransform to validate</param>
        /// <returns>True if size is adequate</returns>
        private bool ValidateTouchTarget(RectTransform rectTransform)
        {
            if (rectTransform == null)
                return true;

            Vector2 size = rectTransform.sizeDelta;

            // Convert to points (assumes 1 Unity unit = 1 point at reference resolution)
            float minDimension = Mathf.Min(size.x, size.y);

            return minDimension >= minTouchTargetSize;
        }
        #endregion

        #region Public API
        /// <summary>
        /// Enable or disable performance monitoring at runtime.
        /// </summary>
        /// <param name="enabled">Whether to enable monitoring</param>
        public void SetPerformanceMonitoring(bool enabled)
        {
            enablePerformanceMonitoring = enabled;

            if (enabled && performanceMonitorCoroutine == null)
            {
                performanceMonitorCoroutine = StartCoroutine(PerformanceMonitorRoutine());
            }
            else if (!enabled && performanceMonitorCoroutine != null)
            {
                StopCoroutine(performanceMonitorCoroutine);
                performanceMonitorCoroutine = null;
            }
        }

        /// <summary>
        /// Set target frame rate at runtime.
        /// </summary>
        /// <param name="fps">Target FPS</param>
        public void SetTargetFrameRate(int fps)
        {
            targetFrameRate = fps;
            Application.targetFrameRate = fps;
            Debug.Log($"[MobileOptimization] Target FPS set to {fps}");
        }

        /// <summary>
        /// Check if device is mobile.
        /// </summary>
        /// <returns>True if running on mobile device</returns>
        public bool IsMobileDevice()
        {
            return Application.isMobilePlatform;
        }

        /// <summary>
        /// Get current aspect ratio.
        /// </summary>
        /// <returns>Aspect ratio (width/height)</returns>
        public float GetAspectRatio()
        {
            return (float)Screen.width / Screen.height;
        }

        /// <summary>
        /// Check if running in portrait orientation.
        /// </summary>
        /// <returns>True if portrait (height > width)</returns>
        public bool IsPortraitOrientation()
        {
            return Screen.height > Screen.width;
        }
        #endregion

        #region Debug UI
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void OnGUI()
        {
            if (!enablePerformanceMonitoring)
                return;

            // Display performance stats in top-right corner
            GUIStyle style = new GUIStyle();
            style.fontSize = 20;
            style.normal.textColor = AverageFPS < fpsWarningThreshold ? Color.red : Color.green;

            string stats = $"FPS: {AverageFPS:F1}\nMemory: {CurrentMemoryMB:F1} MB";
            GUI.Label(new Rect(Screen.width - 200, 10, 190, 60), stats, style);
        }
#endif
        #endregion
    }
}
