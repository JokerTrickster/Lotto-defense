using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class BuildSettingsHelper : EditorWindow
{
    [MenuItem("Lotto Defense/Add Scenes to Build Settings")]
    public static void AddScenesToBuildSettings()
    {
        // Get current scenes in build settings
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

        // Define scenes to add
        string[] scenePaths = new string[]
        {
            "Assets/Scenes/LoginScene.unity",
            "Assets/Scenes/MainGame.unity",
            "Assets/Scenes/GameScene.unity"
        };

        foreach (string scenePath in scenePaths)
        {
            // Check if scene already exists
            bool sceneExists = false;
            foreach (var scene in scenes)
            {
                if (scene.path == scenePath)
                {
                    sceneExists = true;
                    break;
                }
            }

            // Add if not exists
            if (!sceneExists)
            {
                scenes.Add(new EditorBuildSettingsScene(scenePath, true));
                Debug.Log($"Added scene to build settings: {scenePath}");
            }
            else
            {
                Debug.Log($"Scene already in build settings: {scenePath}");
            }
        }

        // Update build settings
        EditorBuildSettings.scenes = scenes.ToArray();

        Debug.Log("Build settings updated!");
        EditorUtility.DisplayDialog("Success", "Scenes added to Build Settings!\n\n- LoginScene\n- MainGame\n- GameScene\n\nYou can now test the full flow!", "OK");
    }
}
