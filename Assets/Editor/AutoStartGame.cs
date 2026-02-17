using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

namespace LottoDefense.Editor
{
    /// <summary>
    /// Unity 에디터 메뉴: 게임 자동 시작
    /// 메뉴: Tools > Auto Start Game
    /// </summary>
    public class AutoStartGame : EditorWindow
    {
        [MenuItem("Tools/Auto Start Game")]
        public static void StartGameDirectly()
        {
            // 플레이 모드가 아니면 시작
            if (!EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = true;
                
                // 플레이 모드 시작 후 GameScene 로드
                EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            }
            else
            {
                // 이미 플레이 모드면 바로 GameScene 로드
                LoadGameScene();
            }
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                // 플레이 모드 진입 완료, GameScene 로드
                LoadGameScene();
                
                // 리스너 제거
                EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            }
        }

        private static void LoadGameScene()
        {
            Debug.Log("[AutoStartGame] Loading GameScene...");
            SceneManager.LoadScene("GameScene");
        }
    }
}
