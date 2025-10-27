using UnityEngine;
using UnityEditor;
using UnityEngine.EventSystems;
using UnityEditor.SceneManagement;

public class FixEventSystem : EditorWindow
{
    [MenuItem("Lotto Defense/Fix EventSystem for New Input System")]
    public static void FixEventSystemInputModule()
    {
        EventSystem eventSystem = FindObjectOfType<EventSystem>();

        if (eventSystem == null)
        {
            Debug.LogWarning("No EventSystem found in scene");
            return;
        }

        // Remove old StandaloneInputModule
        StandaloneInputModule oldModule = eventSystem.GetComponent<StandaloneInputModule>();
        if (oldModule != null)
        {
            DestroyImmediate(oldModule);
            Debug.Log("Removed StandaloneInputModule");
        }

        // Add InputSystemUIInputModule if not exists
        var newInputModule = eventSystem.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        if (newInputModule == null)
        {
            eventSystem.gameObject.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            Debug.Log("Added InputSystemUIInputModule");
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("EventSystem updated to use new Input System!");
    }
}
