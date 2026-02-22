using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class OpenMainGameScene
{
    [MenuItem("Lotto Defense/Open MainGame Scene")]
    private static void OpenScene()
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorSceneManager.OpenScene("Assets/Scenes/MainGame.unity");
            Debug.Log("[OpenMainGameScene] MainGame.unity opened");
        }
    }
}
