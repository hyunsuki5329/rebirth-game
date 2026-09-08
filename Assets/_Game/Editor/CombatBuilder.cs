using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace Roguelite.Editor
{
    public static class CombatBuilder
    {
        public static void Create()
        {
            Directory.CreateDirectory("Assets/_Game/Data/Combat");
            var definition = AssetDatabase.LoadAssetAtPath<AttackDefinition>("Assets/_Game/Data/Combat/BasicAttack.asset");
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<AttackDefinition>();
                AssetDatabase.CreateAsset(definition, "Assets/_Game/Data/Combat/BasicAttack.asset");
            }
            var prefab = PrefabUtility.LoadPrefabContents("Assets/_Game/Prefabs/Player.prefab");
            var health = prefab.GetComponent<PlayerHealth>();
            if (health == null) health = prefab.AddComponent<PlayerHealth>();
            health.stats = AssetDatabase.LoadAssetAtPath<PlayerStats>("Assets/_Game/Data/Player/PlayerStats.asset");
            var hurt = prefab.GetComponent<Hurtbox>();
            if (hurt == null) hurt = prefab.AddComponent<Hurtbox>();
            hurt.health = health;
            var combat = prefab.GetComponent<PlayerCombat>();
            if (combat == null) combat = prefab.AddComponent<PlayerCombat>();
            combat.attack = definition;
            if (prefab.GetComponent<HitFeedback>() == null) prefab.AddComponent<HitFeedback>();
            PrefabUtility.SaveAsPrefabAsset(prefab, "Assets/_Game/Prefabs/Player.prefab");
            PrefabUtility.UnloadPrefabContents(prefab);
            var scene = EditorSceneManager.OpenScene("Assets/_Game/Scenes/PlayerArena.unity", OpenSceneMode.Single);
            Object.DestroyImmediate(GameObject.Find("Player"));
            var player = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Prefabs/Player.prefab"));
            player.name = "Player";
            Camera.main.GetComponent<CameraFollow>().target = player.transform;
            var dummy = new GameObject("TrainingDummy", typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(Rigidbody2D), typeof(HealthBody), typeof(Hurtbox), typeof(HitFeedback));
            dummy.transform.position = new Vector3(1.3f, 0, 0);
            dummy.GetComponent<SpriteRenderer>().sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Game/Art/Square.png");
            dummy.GetComponent<SpriteRenderer>().color = new Color(1, 0.35f, 0.3f);
            dummy.GetComponent<BoxCollider2D>().size = Vector2.one * 0.7f;
            dummy.GetComponent<Rigidbody2D>().gravityScale = 0;
            dummy.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
            dummy.GetComponent<Rigidbody2D>().collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            dummy.AddComponent<KnockbackBody>();
            dummy.GetComponent<Hurtbox>().health = dummy.GetComponent<HealthBody>();
            const string path = "Assets/_Game/Scenes/CombatArena.unity";
            EditorSceneManager.SaveScene(scene, path);
            var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            scenes.RemoveAll(s => s.path == path); scenes.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            AssetDatabase.SaveAssets();
        }
    }
}
