using System.IO;
using Roguelite.Core;
using UnityEditor;
using UnityEngine;
namespace Roguelite.Editor
{
    public static class MomentumBuilder
    {
        public static void Create()
        {
            Directory.CreateDirectory("Assets/_Game/Data/Momentum");
            Directory.CreateDirectory("Assets/_Game/Data/Combos");
            var config = AssetDatabase.LoadAssetAtPath<MomentumConfig>("Assets/_Game/Data/Momentum/Momentum.asset");
            if (config == null) { config = ScriptableObject.CreateInstance<MomentumConfig>(); AssetDatabase.CreateAsset(config, "Assets/_Game/Data/Momentum/Momentum.asset"); }
            var combos = new[] {
                Combo("dagger-chain", new[] { CombatAction.Attack, CombatAction.Attack, CombatAction.Dash, CombatAction.Attack }),
                Combo("dash-chain", new[] { CombatAction.Dash, CombatAction.Attack, CombatAction.Attack }) };
            const string path = "Assets/_Game/Prefabs/Player.prefab";
            var player = PrefabUtility.LoadPrefabContents(path);
            (player.GetComponent<ComboSystem>() ?? player.AddComponent<ComboSystem>()).definitions = combos;
            (player.GetComponent<MomentumSystem>() ?? player.AddComponent<MomentumSystem>()).config = config;
            PrefabUtility.SaveAsPrefabAsset(player, path); PrefabUtility.UnloadPrefabContents(player);
            AssetDatabase.SaveAssets();
        }
        private static ComboDefinition Combo(string id, CombatAction[] steps)
        {
            string path = $"Assets/_Game/Data/Combos/{id}.asset";
            var data = AssetDatabase.LoadAssetAtPath<ComboDefinition>(path);
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<ComboDefinition>(); data.id = id; data.steps = steps;
                AssetDatabase.CreateAsset(data, path);
            }
            return data;
        }
    }
}
