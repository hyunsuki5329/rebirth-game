using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace Roguelite.Editor
{
    public static class ShortcutBuilder
    {
        public static void Create()
        {
            Directory.CreateDirectory("Assets/_Game/Data/Dungeon");
            const string rulePath = "Assets/_Game/Data/Dungeon/ShortcutRules.asset";
            var rules = AssetDatabase.LoadAssetAtPath<ShortcutRules>(rulePath);
            if (rules == null) { rules = ScriptableObject.CreateInstance<ShortcutRules>(); AssetDatabase.CreateAsset(rules, rulePath); }
            const string path = "Assets/_Game/Prefabs/Rooms/Shortcut.prefab";
            var room = PrefabUtility.LoadPrefabContents(path);
            var controller = room.GetComponent<RoomController>();
            controller.exit.transform.localPosition = new Vector3(7, 2, 0);
            var choice = room.GetComponent<ShortcutController>() ?? room.AddComponent<ShortcutController>();
            if (choice.dangerousExit == null)
            {
                var go = new GameObject("Dangerous exit", typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(RoomExit));
                go.transform.SetParent(room.transform, false); go.transform.localPosition = new Vector3(7, -2, 0);
                go.transform.localScale = new Vector3(1.2f, 1.2f, 1); go.transform.localRotation = Quaternion.Euler(0, 0, 45);
                go.layer = controller.exit.gameObject.layer;
                go.GetComponent<SpriteRenderer>().sprite = controller.exit.GetComponent<SpriteRenderer>().sprite;
                go.GetComponent<BoxCollider2D>().isTrigger = true;
                choice.dangerousExit = go.GetComponent<RoomExit>();
            }
            choice.dangerousExit.openColor = new Color(1, 0.5f, 0.08f); choice.dangerousExit.SetOpen(true);
            var dangerCollider = choice.dangerousExit.GetComponent<BoxCollider2D>();
            dangerCollider.size = Vector2.one; dangerCollider.offset = Vector2.zero;
            Label(room.transform, "Safe route label", new Vector2(1.5f, 2.6f), $"SAFE ROUTE\n2 fights | base Gold 20\nRare x{rules.rareStacks}: {rules.safeRareChance:P0}", Color.green);
            Label(room.transform, "Danger route label", new Vector2(1.5f, -1.3f), $"DANGEROUS SHORTCUT\n1 elite fight | base Gold {10 * rules.eliteGoldMultiplier:0}\nGround blasts | Rare x{rules.rareStacks}: {rules.eliteRareChance:P0}", new Color(1, 0.65f, 0.2f));
            PrefabUtility.SaveAsPrefabAsset(room, path); PrefabUtility.UnloadPrefabContents(room);
            var scene = EditorSceneManager.OpenScene("Assets/_Game/Scenes/DungeonArena.unity");
            Object.FindFirstObjectByType<DungeonManager>().shortcutRules = rules;
            EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
        }
        private static void Label(Transform parent, string name, Vector2 position, string text, Color color)
        {
            var existing = parent.Find(name);
            var go = existing != null ? existing.gameObject : new GameObject(name, typeof(TextMesh));
            go.transform.SetParent(parent, false); go.transform.localPosition = position;
            var mesh = go.GetComponent<TextMesh>(); mesh.text = text; mesh.fontSize = 48; mesh.characterSize = 0.09f;
            mesh.anchor = TextAnchor.UpperLeft; mesh.color = color; go.GetComponent<MeshRenderer>().sortingOrder = 8;
        }
    }
}
