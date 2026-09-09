using System.IO;
using UnityEditor;
using UnityEngine;
namespace Roguelite.Editor
{
    public static class BowBuilder
    {
        public static void Create()
        {
            var bow = CreateData();
            const string path = "Assets/_Game/Prefabs/Player.prefab";
            var player = PrefabUtility.LoadPrefabContents(path);
            var weapons = player.GetComponent<WeaponController>();
            var list = new System.Collections.Generic.List<WeaponData>(weapons.weapons);
            list.RemoveAll(w => w.id == "bow"); list.Add(bow); weapons.weapons = list.ToArray();
            if (player.GetComponent<HeldWeaponVisual>() == null) player.AddComponent<HeldWeaponVisual>();
            PrefabUtility.SaveAsPrefabAsset(player, path); PrefabUtility.UnloadPrefabContents(player); AssetDatabase.SaveAssets();
        }
        public static WeaponData CreateData()
        {
            const string art = "Assets/_Game/Art/Weapons/";
            Directory.CreateDirectory(art);
            const string arrowPath = art + "Arrow.png";
            if (!File.Exists(arrowPath))
            {
                var texture = new Texture2D(24, 7, TextureFormat.RGBA32, false);
                texture.SetPixels(new Color[24 * 7]);
                for (int x = 2; x < 21; x++) texture.SetPixel(x, 3, new Color(0.65f, 0.4f, 0.2f));
                for (int x = 0; x < 5; x++) { texture.SetPixel(x, 1 + x / 2, Color.cyan); texture.SetPixel(x, 5 - x / 2, Color.cyan); }
                for (int x = 19; x < 24; x++)
                    for (int y = 0; y < 7; y++) if (Mathf.Abs(y - 3) <= (23 - x) / 2) texture.SetPixel(x, y, new Color(0.85f, 0.9f, 0.95f));
                texture.Apply(); File.WriteAllBytes(arrowPath, texture.EncodeToPNG()); Object.DestroyImmediate(texture);
            }
            AssetDatabase.Refresh();
            ImportSprite(art + "Bow.png", 1024, 512); ImportSprite(arrowPath, 32, 32);
            const string path = "Assets/_Game/Data/Weapons/Bow.asset";
            var bow = AssetDatabase.LoadAssetAtPath<WeaponData>(path);
            if (bow == null)
            {
                bow = ScriptableObject.CreateInstance<WeaponData>(); bow.id = "bow"; bow.color = new Color(0.3f, 0.8f, 0.7f);
                bow.combos = new[] { AssetDatabase.LoadAssetAtPath<ComboDefinition>("Assets/_Game/Data/Combos/dash-chain.asset") };
                bow.attack = ScriptableObject.CreateInstance<AttackDefinition>();
                bow.attack.kind = PlayerAttackKind.Projectile; bow.attack.damage = 16; bow.attack.range = 8;
                bow.attack.arc = 8; bow.attack.windup = 0.12f; bow.attack.activeTime = 0.02f; bow.attack.recovery = 0.36f;
                bow.attack.knockback = 2; bow.attack.movementMultiplier = 0.8f;
                bow.attack.projectileSpeed = 14; bow.attack.projectileLifetime = 0.6f;
                AssetDatabase.CreateAsset(bow.attack, "Assets/_Game/Data/Weapons/BowAttack.asset"); AssetDatabase.CreateAsset(bow, path);
            }
            bow.heldSprite = AssetDatabase.LoadAssetAtPath<Sprite>(art + "Bow.png");
            bow.heldFlipX = true; bow.heldGrip = new Vector2(0.435f, 0.5f);
            bow.attack.projectileSprite = AssetDatabase.LoadAssetAtPath<Sprite>(arrowPath);
            if (bow.heldSprite == null || bow.attack.projectileSprite == null)
                throw new System.InvalidOperationException("Bow sprite import did not produce a Sprite asset.");
            EditorUtility.SetDirty(bow); EditorUtility.SetDirty(bow.attack); return bow;
        }
        private static void ImportSprite(string path, float ppu, int size)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
            importer.textureShape = TextureImporterShape.Texture2D;
            importer.spritePixelsPerUnit = ppu; importer.filterMode = FilterMode.Point; importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true; importer.textureCompression = TextureImporterCompression.Uncompressed;
            var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect; settings.spriteGenerateFallbackPhysicsShape = false;
            importer.SetTextureSettings(settings);
            importer.maxTextureSize = size; importer.SaveAndReimport();
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(path)) Debug.Log("SPRITE_IMPORT " + path + " " + asset.GetType().Name);
        }
    }
}
