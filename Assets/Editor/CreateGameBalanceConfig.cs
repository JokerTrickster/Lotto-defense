using UnityEngine;
using UnityEditor;
using LottoDefense.Gameplay;
using System.IO;

namespace LottoDefense.Editor
{
    /// <summary>
    /// Editor utility to create GameBalanceConfig asset in Resources folder.
    /// Unity Menu: Tools → Lotto Defense → Create Game Balance Config
    /// </summary>
    public static class CreateGameBalanceConfig
    {
        [MenuItem("Tools/Lotto Defense/Create Game Balance Config")]
        public static void Create()
        {
            // Check if already exists
            string resourcePath = "Assets/Resources/GameBalanceConfig.asset";
            
            if (File.Exists(resourcePath))
            {
                bool overwrite = EditorUtility.DisplayDialog(
                    "GameBalanceConfig Exists",
                    "GameBalanceConfig.asset already exists in Resources folder.\n\nOverwrite with new default config?",
                    "Yes, Overwrite",
                    "Cancel"
                );
                
                if (!overwrite)
                {
                    Debug.Log("[CreateGameBalanceConfig] Cancelled - existing config preserved");
                    return;
                }
            }
            
            // Create Resources folder if not exists
            if (!Directory.Exists("Assets/Resources"))
            {
                Directory.CreateDirectory("Assets/Resources");
                AssetDatabase.Refresh();
            }
            
            // Create new GameBalanceConfig instance
            GameBalanceConfig config = ScriptableObject.CreateInstance<GameBalanceConfig>();
            
            // Save as asset
            AssetDatabase.CreateAsset(config, resourcePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            // Select in Project window
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = config;
            
            Debug.Log($"✅ [CreateGameBalanceConfig] Created at: {resourcePath}");
            EditorUtility.DisplayDialog(
                "Success",
                $"GameBalanceConfig.asset created successfully!\n\nLocation: {resourcePath}\n\nYou can now edit it in the Inspector.",
                "OK"
            );
        }
        
        [MenuItem("Tools/Lotto Defense/Move DifficultyConfig to Resources")]
        public static void MoveDifficultyConfig()
        {
            string oldPath = "Assets/Data/DifficultyConfig.asset";
            string newPath = "Assets/Resources/DifficultyConfig.asset";
            
            if (!File.Exists(oldPath))
            {
                EditorUtility.DisplayDialog("Error", "DifficultyConfig.asset not found at:\n" + oldPath, "OK");
                return;
            }
            
            // Create Resources folder if not exists
            if (!Directory.Exists("Assets/Resources"))
            {
                Directory.CreateDirectory("Assets/Resources");
                AssetDatabase.Refresh();
            }
            
            // Move file
            string error = AssetDatabase.MoveAsset(oldPath, newPath);
            
            if (string.IsNullOrEmpty(error))
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                
                Debug.Log($"✅ [CreateGameBalanceConfig] Moved DifficultyConfig to: {newPath}");
                EditorUtility.DisplayDialog("Success", "DifficultyConfig.asset moved to Resources folder!", "OK");
            }
            else
            {
                Debug.LogError($"Failed to move DifficultyConfig: {error}");
                EditorUtility.DisplayDialog("Error", "Failed to move DifficultyConfig:\n" + error, "OK");
            }
        }
    }
}
