using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace Roguelite.Editor
{
    public static class EnemyBuilder
    {
        public static void Create()
        {
            var tags = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layers = tags.FindProperty("layers");
            string[] names = { "Player", "Enemy", "World", "Hitbox", "Projectile" };
            for (int i = 0; i < names.Length; i++) layers.GetArrayElementAtIndex(8 + i).stringValue = names[i];
            tags.ApplyModifiedProperties();
            Directory.CreateDirectory("Assets/_Game/Data/Enemies");
            Directory.CreateDirectory("Assets/_Game/Prefabs/Enemies");
            var scene = EditorSceneManager.OpenScene("Assets/_Game/Scenes/CombatArena.unity", OpenSceneMode.Single);
            Object.DestroyImmediate(GameObject.Find("TrainingDummy"));
            var player = GameObject.Find("Player"); player.layer = 8;
            foreach (var root in scene.GetRootGameObjects()) if (root.name.StartsWith("Wall")) root.layer = 10;
            var enemies = new List<EnemyAI>();
            string[] archetypes = { "Chaser", "Ranged", "Charger" };
            var positions = new[] { new Vector2(-4, 0), new Vector2(4, 2), new Vector2(0, 4) };
            var colors = new[] { new Color(1, 0.35f, 0.3f), new Color(0.7f, 0.45f, 1), new Color(1, 0.65f, 0.2f) };
            for (int i = 0; i < archetypes.Length; i++)
            {
                string name = archetypes[i];
                var data = AssetDatabase.LoadAssetAtPath<EnemyData>($"Assets/_Game/Data/Enemies/{name}.asset");
                if (data == null)
                {
                    data = ScriptableObject.CreateInstance<EnemyData>(); data.id = name.ToLowerInvariant();
                    data.attackKind = (EnemyAttackKind)i;
                    data.health = i == 0 ? 30 : i == 1 ? 24 : 45;
                    data.attack = ScriptableObject.CreateInstance<AttackDefinition>();
                    data.attack.damage = i == 2 ? 14 : 8;
                    data.attack.range = i == 1 ? 7 : 1.15f;
                    data.attack.arc = i == 2 ? 360 : i == 1 ? 12 : 110;
                    data.attack.windup = i == 2 ? 0.7f : i == 1 ? 0.5f : 0.35f;
                    data.attack.activeTime = i == 2 ? 0.45f : 0.12f;
                    data.attack.recovery = i == 2 ? 1.2f : 0.8f;
                    AssetDatabase.CreateAsset(data.attack, $"Assets/_Game/Data/Enemies/{name}Attack.asset");
                    AssetDatabase.CreateAsset(data, $"Assets/_Game/Data/Enemies/{name}.asset");
                }
                var go = new GameObject(name, typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(Rigidbody2D));
                go.layer = 9; go.transform.position = positions[i];
                go.GetComponent<SpriteRenderer>().sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Game/Art/Square.png");
                go.GetComponent<SpriteRenderer>().color = colors[i];
                go.GetComponent<CircleCollider2D>().radius = 0.4f;
                var rb = go.GetComponent<Rigidbody2D>(); rb.gravityScale = 0; rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
                var health = go.AddComponent<EnemyHealth>(); health.data = data;
                go.AddComponent<Hurtbox>().health = health;
                go.AddComponent<HitFeedback>(); go.AddComponent<EnemyMotor2D>();
                go.AddComponent<EnemyAttack>().data = data;
                EnemyAI ai = i == 0 ? go.AddComponent<ChaserAI>() : i == 1 ? (EnemyAI)go.AddComponent<RangedAI>() : go.AddComponent<ChargerAI>();
                ai.data = data; enemies.Add(ai);
                PrefabUtility.SaveAsPrefabAsset(go, $"Assets/_Game/Prefabs/Enemies/{name}.prefab");
            }
            var arena = new GameObject("Enemy Arena", typeof(EnemyArena)).GetComponent<EnemyArena>();
            arena.player = player.GetComponent<PlayerHealth>(); arena.enemies = enemies.ToArray();
            const string path = "Assets/_Game/Scenes/EnemyArena.unity";
            EditorSceneManager.SaveScene(scene, path);
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            scenes.RemoveAll(s => s.path == path); scenes.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            AssetDatabase.SaveAssets();
        }
    }
}
