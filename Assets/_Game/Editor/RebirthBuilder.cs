using System.IO;
using UnityEditor;
using UnityEngine;
namespace Roguelite.Editor
{
    public static class RebirthBuilder
    {
        public static void Create()
        {
            Directory.CreateDirectory("Assets/_Game/Data/Rebirth");
            const string root = "Assets/_Game/Data/Rebirth/";
            var config = LoadOrCreate<RebirthConfig>(root + "RebirthConfig.asset");
            var vitality = LoadOrCreate<RebirthUpgradeData>(root + "Vitality.asset");
            vitality.id = "max-health"; vitality.stat = PermanentStat.MaximumHealth; vitality.value = 20;
            var focus = LoadOrCreate<RebirthUpgradeData>(root + "Focus.asset");
            focus.id = "combo-window"; focus.stat = PermanentStat.ComboWindow; focus.value = 0.25f;
            config.upgrades = new[] { vitality, focus };
            const string prefabPath = "Assets/_Game/Prefabs/GameBootstrap.prefab";
            var rootObject = PrefabUtility.LoadPrefabContents(prefabPath); rootObject.GetComponent<GameBootstrap>().rebirthConfig = config;
            PrefabUtility.SaveAsPrefabAsset(rootObject, prefabPath); PrefabUtility.UnloadPrefabContents(rootObject);
            EditorUtility.SetDirty(config); EditorUtility.SetDirty(vitality); EditorUtility.SetDirty(focus); AssetDatabase.SaveAssets();
        }
        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null) { asset = ScriptableObject.CreateInstance<T>(); AssetDatabase.CreateAsset(asset, path); }
            return asset;
        }
    }
}
