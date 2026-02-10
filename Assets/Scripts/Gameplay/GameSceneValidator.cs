using UnityEngine;
using UnityEngine.SceneManagement;

namespace LottoDefense.Gameplay
{
    /// <summary>
    /// Validates GameScene setup on load and ensures all required components exist.
    /// Automatically runs when GameScene is loaded.
    /// </summary>
    public class GameSceneValidator
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void ValidateOnSceneLoad()
        {
            // Only run in GameScene
            if (SceneManager.GetActiveScene().name != "GameScene")
            {
                return;
            }

            Debug.Log("[GameSceneValidator] Validating GameScene setup...");

            ValidateCamera();
            ValidateBootstrapper();
            ValidateGameplayManager();

            Debug.Log("[GameSceneValidator] Validation complete");
        }

        private static void ValidateCamera()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogWarning("[GameSceneValidator] No main camera found! Creating one...");

                GameObject cameraObj = new GameObject("Main Camera");
                Camera camera = cameraObj.AddComponent<Camera>();
                camera.tag = "MainCamera";
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0.1f, 0.1f, 0.15f); // Dark blue
                camera.orthographic = true;
                camera.orthographicSize = 10f;
                camera.transform.position = new Vector3(0, 0, -10);

                // Add AudioListener
                cameraObj.AddComponent<AudioListener>();

                Debug.Log("[GameSceneValidator] Main camera created");
            }
        }

        private static void ValidateBootstrapper()
        {
            GameSceneBootstrapper bootstrapper = FindFirstObjectByType<GameSceneBootstrapper>();
            if (bootstrapper == null)
            {
                Debug.LogWarning("[GameSceneValidator] No GameSceneBootstrapper found! Creating one...");

                GameObject bootstrapperObj = new GameObject("GameSceneBootstrapper");
                bootstrapperObj.AddComponent<GameSceneBootstrapper>();

                Debug.Log("[GameSceneValidator] GameSceneBootstrapper created");
            }
        }

        private static void ValidateGameplayManager()
        {
            // Just access Instance to trigger auto-creation if needed
            if (GameplayManager.Instance != null)
            {
                Debug.Log("[GameSceneValidator] GameplayManager validated");
            }
        }
    }
}
