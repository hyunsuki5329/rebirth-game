using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace Roguelite.Editor
{
    [InitializeOnLoad]
    public static class PlaytestLauncher
    {
        private const string ScenePath = "Assets/_Game/Scenes/DungeonArena.unity";
        private const string RequestPath = "Temp/RoguelitePlaytest.request";
        static PlaytestLauncher()
        {
            EditorApplication.update += CheckRequest;
            EditorApplication.playModeStateChanged += state =>
            {
                if (state == PlayModeStateChange.EnteredPlayMode && !Application.isBatchMode)
                    File.WriteAllText("Temp/RoguelitePlaytest.ready", UnityEngine.SceneManagement.SceneManager.GetActiveScene().path);
            };
        }
        private static void CheckRequest()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || !File.Exists(RequestPath)) return;
            File.Delete(RequestPath);
            Play();
        }
        [MenuItem("Roguelite/Play Vertical Slice")]
        public static void Play()
        {
            if (EditorApplication.isPlaying) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene("Assets/_Game/Scenes/MainMenu.unity", OpenSceneMode.Single);
            EditorApplication.isPlaying = true;
            Debug.Log("ROGUELITE_PLAYTEST_READY: MainMenu. Enter Hub, choose a weapon, then Enter Dungeon.");
        }
        [MenuItem("Roguelite/Open Dungeon Test Scene")]
        public static void Open()
        {
            if (EditorApplication.isPlaying) return;
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }
    }
}
