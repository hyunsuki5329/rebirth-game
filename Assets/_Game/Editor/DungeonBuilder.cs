using System.Collections.Generic;
using System.IO;
using Roguelite.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace Roguelite.Editor
{
    public static class DungeonBuilder
    {
        public static void Create()
        {
            Directory.CreateDirectory("Assets/_Game/Prefabs/Rooms");
            Directory.CreateDirectory("Assets/_Game/Data/Rooms");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var camera = new GameObject("Main Camera", typeof(Camera), typeof(CameraFollow));
            camera.tag = "MainCamera"; camera.transform.position = new Vector3(0, 0, -10);
            camera.GetComponent<Camera>().orthographic = true; camera.GetComponent<Camera>().orthographicSize = 6;
            camera.GetComponent<Camera>().backgroundColor = new Color(0.04f, 0.06f, 0.09f);
            var dungeon = new GameObject("Dungeon", typeof(DungeonManager)).GetComponent<DungeonManager>();
            dungeon.gameObject.AddComponent<DungeonTestHUD>();
            dungeon.playerPrefab = AssetDatabase.LoadAssetAtPath<PlayerHealth>("Assets/_Game/Prefabs/Player.prefab");
            dungeon.combatRooms = new[] { CreateRoom("CombatA", RoomKind.Combat, "Chaser", "Ranged"),
                CreateRoom("CombatB", RoomKind.Combat, "Chaser", "Charger") };
            dungeon.rewardRoom = CreateRoom("Reward", RoomKind.Reward);
            dungeon.shortcutRoom = CreateRoom("Shortcut", RoomKind.Shortcut);
            dungeon.eliteRoom = CreateRoom("Elite", RoomKind.Elite, "Charger", "Ranged");
            dungeon.bossRoom = CreateRoom("BossEntrance", RoomKind.Boss);
            const string path = "Assets/_Game/Scenes/DungeonArena.unity";
            EditorSceneManager.SaveScene(scene, path);
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            scenes.RemoveAll(s => s.path == path); scenes.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray(); AssetDatabase.SaveAssets();
        }
        private static RoomController CreateRoom(string id, RoomKind kind, params string[] enemies)
        {
            var data = AssetDatabase.LoadAssetAtPath<RoomDefinition>($"Assets/_Game/Data/Rooms/{id}.asset");
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<RoomDefinition>(); data.id = id; data.kind = kind;
                data.gold = kind == RoomKind.Combat || kind == RoomKind.Elite ? 10 : kind == RoomKind.Reward ? 5 : 0;
                data.enemies = new EnemySpawn[enemies.Length];
                for (int i = 0; i < enemies.Length; i++) data.enemies[i] = new EnemySpawn {
                    prefab = AssetDatabase.LoadAssetAtPath<EnemyAI>($"Assets/_Game/Prefabs/Enemies/{enemies[i]}.prefab"),
                    position = new Vector2(i * 3, i == 0 ? -1 : 2) };
                AssetDatabase.CreateAsset(data, $"Assets/_Game/Data/Rooms/{id}.asset");
            }
            var root = new GameObject(id, typeof(RoomController));
            var room = root.GetComponent<RoomController>(); room.definition = data;
            Block(root.transform, "North", new Vector2(0, 5), new Vector2(18, 1));
            Block(root.transform, "South", new Vector2(0, -5), new Vector2(18, 1));
            Block(root.transform, "West", new Vector2(-8.5f, 0), new Vector2(1, 10));
            Block(root.transform, "East", new Vector2(8.5f, 0), new Vector2(1, 10));
            var door = Block(root.transform, "Exit", new Vector2(7, 0), new Vector2(0.8f, 2));
            door.GetComponent<BoxCollider2D>().isTrigger = true;
            room.exit = door.AddComponent<RoomExit>(); room.exit.SetOpen(false);
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, $"Assets/_Game/Prefabs/Rooms/{id}.prefab");
            Object.DestroyImmediate(root); return prefab.GetComponent<RoomController>();
        }
        private static GameObject Block(Transform parent, string name, Vector2 position, Vector2 size)
        {
            var go = new GameObject(name, typeof(SpriteRenderer), typeof(BoxCollider2D));
            go.layer = 10; go.transform.SetParent(parent); go.transform.localPosition = position;
            go.transform.localScale = new Vector3(size.x, size.y, 1);
            go.GetComponent<BoxCollider2D>().size = Vector2.one;
            var sprite = go.GetComponent<SpriteRenderer>();
            sprite.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Game/Art/Square.png");
            sprite.color = new Color(0.2f, 0.3f, 0.4f); return go;
        }
    }
}
