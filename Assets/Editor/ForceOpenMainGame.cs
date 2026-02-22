using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public class ForceOpenMainGame
{
    static ForceOpenMainGame()
    {
        EditorApplication.delayCall += () =>
        {
            if (EditorSceneManager.GetActiveScene().name == "Untitled" || 
                EditorSceneManager.GetActiveScene().name == "")
            {
                Debug.Log("[ForceOpenMainGame] Opening MainGame.unity...");
                EditorSceneManager.OpenScene("Assets/Scenes/MainGame.unity");
            }
        };
    }
}
