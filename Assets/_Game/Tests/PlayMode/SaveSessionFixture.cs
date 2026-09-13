using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace Roguelite.Tests
{
    internal static class SaveSessionFixture
    {
        private static readonly List<string> directories = new List<string>();
        public static string NewDirectory()
        {
            string path = Path.Combine(Path.GetTempPath(), "RogueliteSessionTests", Guid.NewGuid().ToString("N"));
            directories.Add(path); return path;
        }
        public static IEnumerator Load(string directory = null)
        {
#if UNITY_EDITOR
            var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameBootstrap>("Assets/_Game/Prefabs/GameBootstrap.prefab");
            string saveDirectory = directory ?? NewDirectory();
            // sceneLoaded runs after the previous scene is unloaded and before GameSceneEntry.Start.
            // This keeps the test's UI lifetime identical to a normal MainMenu launch.
            void CreateSession(Scene scene, LoadSceneMode mode)
            {
                if (scene.name != "MainMenu") return;
                var holder = new GameObject("Inactive test bootstrap holder"); holder.SetActive(false);
                var session = UnityEngine.Object.Instantiate(prefab, holder.transform);
                session.gameObject.SetActive(false); session.profileDirectory = saveDirectory;
                session.transform.SetParent(null); UnityEngine.Object.Destroy(holder);
                session.gameObject.SetActive(true);
            }
            SceneManager.sceneLoaded += CreateSession;
            try { yield return SceneManager.LoadSceneAsync("MainMenu"); yield return null; yield return null; }
            finally { SceneManager.sceneLoaded -= CreateSession; }
#else
            throw new NotSupportedException("This Editor session fixture requires the source prefab.");
#endif
        }
        public static void Cleanup()
        {
            foreach (string directory in directories) if (Directory.Exists(directory)) Directory.Delete(directory, true);
            directories.Clear(); AudioListener.volume = 1;
        }
    }
}
