using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Roguelite.Editor
{
    public static class PlayerBuilder
    {
        public static void Create()
        {
            Directory.CreateDirectory("Assets/_Game/Data/Player");
            Directory.CreateDirectory("Assets/_Game/Prefabs");
            var stats = AssetDatabase.LoadAssetAtPath<PlayerStats>("Assets/_Game/Data/Player/PlayerStats.asset");
            if (stats == null)
            {
                stats = ScriptableObject.CreateInstance<PlayerStats>();
                AssetDatabase.CreateAsset(stats, "Assets/_Game/Data/Player/PlayerStats.asset");
            }
            var scene = EditorSceneManager.OpenScene("Assets/_Game/Scenes/PlayerTest.unity", OpenSceneMode.Single);
            var go = GameObject.Find("FoundationBody");
            go.SetActive(false);
            go.name = "Player";
            go.transform.localScale = new Vector3(0.65f, 0.65f, 1);
            var input = go.AddComponent<PlayerInputReader>();
            input.actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/InputSystem_Actions.inputactions");
            go.AddComponent<PlayerSprint>().stats = stats;
            go.AddComponent<PlayerMovement>().stats = stats;
            var aim = go.AddComponent<PlayerAim>();
            go.AddComponent<PlayerDash>().stats = stats;
            go.AddComponent<PlayerMotor2D>();
            var marker = new GameObject("Aim", typeof(SpriteRenderer));
            marker.transform.SetParent(go.transform);
            marker.transform.localScale = new Vector3(0.4f, 0.12f, 1);
            marker.GetComponent<SpriteRenderer>().sprite = go.GetComponent<SpriteRenderer>().sprite;
            marker.GetComponent<SpriteRenderer>().color = Color.white;
            aim.indicator = marker.transform;
            go.SetActive(true);
            PrefabUtility.SaveAsPrefabAsset(go, "Assets/_Game/Prefabs/Player.prefab");
            Camera.main.gameObject.AddComponent<CameraFollow>().target = go.transform;
            const string path = "Assets/_Game/Scenes/PlayerArena.unity";
            EditorSceneManager.SaveScene(scene, path);
            EditorBuildSettings.scenes = new[] {
                new EditorBuildSettingsScene("Assets/_Game/Scenes/PlayerTest.unity", true),
                new EditorBuildSettingsScene(path, true)
            };
            AssetDatabase.SaveAssets();
        }
    }
}
