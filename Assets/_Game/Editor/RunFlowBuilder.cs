using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace Roguelite.Editor
{
    public static class RunFlowBuilder
    {
        public static void Create()
        {
            Directory.CreateDirectory("Assets/_Game/Data/Flow");
            const string configPath = "Assets/_Game/Data/Flow/RunFlowConfig.asset";
            var config = AssetDatabase.LoadAssetAtPath<RunFlowConfig>(configPath);
            if (config == null) { config = ScriptableObject.CreateInstance<RunFlowConfig>(); AssetDatabase.CreateAsset(config, configPath); }
            config.weapons = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Prefabs/Player.prefab").GetComponent<WeaponController>().weapons;
            EditorUtility.SetDirty(config);
            var root = new GameObject("Game Session", typeof(GameBootstrap), typeof(GameFlowView)); root.GetComponent<GameBootstrap>().config = config;
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, "Assets/_Game/Prefabs/GameBootstrap.prefab"); Object.DestroyImmediate(root);
            foreach (string name in new[] { "MainMenu", "Hub" })
            {
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                var camera = new GameObject("Main Camera", typeof(Camera)); camera.tag = "MainCamera";
                camera.transform.position = new Vector3(0, 0, -10); camera.GetComponent<Camera>().orthographic = true;
                camera.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor; camera.GetComponent<Camera>().backgroundColor = new Color(0.04f, 0.06f, 0.09f);
                if (name == "MainMenu") new GameObject("Entry", typeof(GameSceneEntry)).GetComponent<GameSceneEntry>().bootstrapPrefab = prefab.GetComponent<GameBootstrap>();
                EditorSceneManager.SaveScene(scene, $"Assets/_Game/Scenes/{name}.unity");
            }
            var dungeonScene = EditorSceneManager.OpenScene("Assets/_Game/Scenes/DungeonArena.unity");
            var dungeon = Object.FindFirstObjectByType<DungeonManager>(); dungeon.autoStart = false;
            Object.DestroyImmediate(dungeon.GetComponent<DungeonTestHUD>());
            EditorSceneManager.SaveScene(dungeonScene, "Assets/_Game/Scenes/Dungeon.unity");
            var scenes = new List<EditorBuildSettingsScene>();
            foreach (string name in new[] { "MainMenu", "Hub", "Dungeon" }) scenes.Add(new EditorBuildSettingsScene($"Assets/_Game/Scenes/{name}.unity", true));
            foreach (var scene in EditorBuildSettings.scenes) if (!scenes.Exists(s => s.path == scene.path)) scenes.Add(scene);
            EditorBuildSettings.scenes = scenes.ToArray(); AssetDatabase.SaveAssets();
        }
    }
}
