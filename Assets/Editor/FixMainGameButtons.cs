using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEditor.SceneManagement;
using LottoDefense.UI;

public class FixMainGameButtons : EditorWindow
{
    [MenuItem("Lotto Defense/Fix MainGame Button Events")]
    public static void FixButtonEvents()
    {
        // Find or create SceneNavigator
        SceneNavigator navigator = FindFirstObjectByType<SceneNavigator>();

        if (navigator == null)
        {
            GameObject navObj = new GameObject("SceneNavigator");
            navigator = navObj.AddComponent<SceneNavigator>();
            Debug.Log("Created SceneNavigator");
        }

        // Find buttons
        Button[] allButtons = FindObjectsByType<Button>(FindObjectsSortMode.None);

        bool foundStartButton = false;
        bool foundLogoutButton = false;

        foreach (Button button in allButtons)
        {
            if (button.gameObject.name == "StartGameButton")
            {
                // Clear existing listeners
                button.onClick.RemoveAllListeners();

                // Add new listener
                button.onClick.AddListener(() => navigator.LoadGameScene());

                Debug.Log("Connected StartGameButton to LoadGameScene()");
                foundStartButton = true;
            }
            else if (button.gameObject.name == "LogoutButton")
            {
                // Clear existing listeners
                button.onClick.RemoveAllListeners();

                // Add new listener
                button.onClick.AddListener(() => navigator.LoadLoginScene());

                Debug.Log("Connected LogoutButton to LoadLoginScene()");
                foundLogoutButton = true;
            }
        }

        if (!foundStartButton)
        {
            Debug.LogWarning("StartGameButton not found!");
        }

        if (!foundLogoutButton)
        {
            Debug.LogWarning("LogoutButton not found!");
        }

        // Mark scene dirty
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        string message = "Button events fixed!\n\n";
        if (foundStartButton) message += "✓ StartGameButton → LoadGameScene()\n";
        if (foundLogoutButton) message += "✓ LogoutButton → LoadLoginScene()\n";

        EditorUtility.DisplayDialog("Success", message, "OK");
    }
}
