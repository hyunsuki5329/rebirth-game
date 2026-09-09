using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace Roguelite.Editor
{
    public static class WeaponBuilder
    {
        public static void Create()
        {
            Directory.CreateDirectory("Assets/_Game/Data/Weapons"); Directory.CreateDirectory("Assets/_Game/Data/Items");
            string[] names = { "Dagger", "Sword", "Greatsword" };
            var weapons = new WeaponData[3];
            for (int i = 0; i < 3; i++)
            {
                string path = $"Assets/_Game/Data/Weapons/{names[i]}.asset";
                var data = AssetDatabase.LoadAssetAtPath<WeaponData>(path);
                if (data == null)
                {
                    data = ScriptableObject.CreateInstance<WeaponData>(); data.id = names[i].ToLowerInvariant();
                    data.canEvolve = i == 0; data.comboWindowBonus = i == 2 ? 0.5f : 0;
                    data.waveSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Game/Art/Square.png");
                    var dagger = AssetDatabase.LoadAssetAtPath<ComboDefinition>("Assets/_Game/Data/Combos/dagger-chain.asset");
                    var dash = AssetDatabase.LoadAssetAtPath<ComboDefinition>("Assets/_Game/Data/Combos/dash-chain.asset");
                    data.combos = i == 0 ? new[] { dagger, dash } : new[] { dash };
                    data.color = i == 0 ? Color.cyan : i == 1 ? Color.green : new Color(0.4f, 0.6f, 1);
                    data.attack = ScriptableObject.CreateInstance<AttackDefinition>();
                    data.attack.damage = new[] { 12f, 20, 32 }[i]; data.attack.range = new[] { 1.8f, 2.4f, 3 }[i];
                    data.attack.arc = new[] { 100f, 130, 170 }[i]; data.attack.windup = new[] { 0.06f, 0.14f, 0.3f }[i];
                    data.attack.activeTime = new[] { 0.12f, 0.15f, 0.2f }[i]; data.attack.recovery = new[] { 0.25f, 0.35f, 0.5f }[i];
                    data.attack.knockback = new[] { 4f, 5, 8 }[i]; data.attack.movementMultiplier = new[] { 0.7f, 0.5f, 0.25f }[i];
                    AssetDatabase.CreateAsset(data.attack, $"Assets/_Game/Data/Weapons/{names[i]}Attack.asset"); AssetDatabase.CreateAsset(data, path);
                }
                weapons[i] = data;
            }
            var items = new ItemData[3]; string[] itemNames = { "Edge", "Boots", "Tempo" };
            for (int i = 0; i < 3; i++)
            {
                string path = $"Assets/_Game/Data/Items/{itemNames[i]}.asset";
                var item = AssetDatabase.LoadAssetAtPath<ItemData>(path);
                if (item == null) { item = ScriptableObject.CreateInstance<ItemData>(); item.id = itemNames[i].ToLowerInvariant(); item.stat = (ItemStat)i;
                    item.bonusPerStack = i == 0 ? 0.15f : 0.1f; AssetDatabase.CreateAsset(item, path); }
                items[i] = item;
            }
            const string playerPath = "Assets/_Game/Prefabs/Player.prefab";
            System.Array.Resize(ref weapons, 4); weapons[3] = BowBuilder.CreateData();
            var player = PrefabUtility.LoadPrefabContents(playerPath);
            (player.GetComponent<WeaponController>() ?? player.AddComponent<WeaponController>()).weapons = weapons;
            if (player.GetComponent<RunInventory>() == null) player.AddComponent<RunInventory>();
            if (player.GetComponent<HeldWeaponVisual>() == null) player.AddComponent<HeldWeaponVisual>();
            PrefabUtility.SaveAsPrefabAsset(player, playerPath); PrefabUtility.UnloadPrefabContents(player);
            var scene = EditorSceneManager.OpenScene("Assets/_Game/Scenes/DungeonArena.unity", OpenSceneMode.Single);
            Object.FindFirstObjectByType<DungeonManager>().rewardItems = items;
            EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
        }
    }
}
