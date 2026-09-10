using UnityEditor;
using UnityEngine;
namespace Roguelite.Editor
{
    public static class BossBuilder
    {
        public static void Create()
        {
            const string root = "Assets/_Game/Data/Enemies/";
            var attack = LoadOrCreate<AttackDefinition>(root + "BossCharge.asset");
            attack.damage = 18; attack.range = 1.15f; attack.arc = 360;
            attack.windup = 0.8f; attack.activeTime = 0.65f; attack.recovery = 1.3f; attack.knockback = 3;
            var data = LoadOrCreate<EnemyData>(root + "Boss.asset");
            data.id = "rift-warden"; data.health = 160; data.attackKind = EnemyAttackKind.Charge; data.attack = attack; data.chargeSpeed = 9;
            var patterns = LoadOrCreate<BossPatternData>(root + "BossPatterns.asset");
            var go = PrefabUtility.LoadPrefabContents("Assets/_Game/Prefabs/Enemies/Charger.prefab");
            Object.DestroyImmediate(go.GetComponent<EnemyAI>());
            go.name = "Rift Warden"; go.transform.localScale = Vector3.one * 1.65f;
            go.GetComponent<EnemyHealth>().data = data; go.GetComponent<EnemyAttack>().data = data;
            go.GetComponent<SpriteRenderer>().color = new Color(0.75f, 0.55f, 1);
            var boss = go.AddComponent<BossController>(); boss.data = data; boss.patterns = patterns;
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, "Assets/_Game/Prefabs/Enemies/Boss.prefab");
            PrefabUtility.UnloadPrefabContents(go);
            var room = AssetDatabase.LoadAssetAtPath<RoomDefinition>("Assets/_Game/Data/Rooms/BossEntrance.asset");
            room.enemies = new[] { new EnemySpawn { prefab = prefab.GetComponent<BossController>(), position = Vector2.zero } };
            room.gold = 35;
            EditorUtility.SetDirty(attack); EditorUtility.SetDirty(data); EditorUtility.SetDirty(patterns); EditorUtility.SetDirty(room);
            AssetDatabase.SaveAssets();
        }
        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null) { asset = ScriptableObject.CreateInstance<T>(); AssetDatabase.CreateAsset(asset, path); }
            return asset;
        }
    }
}
