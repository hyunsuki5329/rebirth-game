using UnityEditor;
using UnityEngine;
namespace Roguelite.Editor
{
    public static class PixelArtBuilder
    {
        private static Material material;
        public static void Create()
        {
            const string materialPath = "Assets/_Game/Art/SpriteHitFlash.mat";
            material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null) { material = new Material(Shader.Find("Roguelite/SpriteHitFlash")); AssetDatabase.CreateAsset(material, materialPath); }
            BowBuilder.Create();
            var hero = Import("Characters/Explorer", 2.5f, new Vector2(0.5f, 0.5f));
            var slime = Import("Enemies/Slime", 1.8f, new Vector2(0.5f, 0.5f));
            var goblin = Import("Enemies/Goblin", 1.6f, new Vector2(0.5f, 0.5f));
            var boar = Import("Enemies/Boar", 1.6f, new Vector2(0.5f, 0.5f));
            string[] names = { "Dagger", "Sword", "Greatsword" };
            float[] lengths = { 1.3f, 1.9f, 2.3f };
            for (int i = 0; i < names.Length; i++)
            {
                var weapon = AssetDatabase.LoadAssetAtPath<WeaponData>($"Assets/_Game/Data/Weapons/{names[i]}.asset");
                weapon.heldSprite = Import("Weapons/" + names[i], 1, new Vector2(0.28f, 0.5f));
                weapon.sizeHeldByWidth = true; weapon.heldHeight = lengths[i]; EditorUtility.SetDirty(weapon);
                weapon.waveSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Game/Art/Square.png");
            }
            var bow = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/_Game/Data/Weapons/Bow.asset");
            bow.heldHeight = 2; EditorUtility.SetDirty(bow);
            const string path = "Assets/_Game/Prefabs/Player.prefab";
            var player = PrefabUtility.LoadPrefabContents(path);
            player.GetComponent<SpriteRenderer>().sprite = hero; player.GetComponent<SpriteRenderer>().color = Color.white;
            player.GetComponent<SpriteRenderer>().sharedMaterial = material;
            player.GetComponent<WeaponController>().tintBodyWithWeapon = false;
            if (player.GetComponent<HeldWeaponVisual>() == null) player.AddComponent<HeldWeaponVisual>();
            player.GetComponent<HeldWeaponVisual>().handSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Game/Art/Square.png");
            var aim = player.transform.Find("AimIndicator"); if (aim != null) aim.gameObject.SetActive(false);
            PrefabUtility.SaveAsPrefabAsset(player, path); PrefabUtility.UnloadPrefabContents(player);
            ApplyEnemy("Chaser", slime); ApplyEnemy("Ranged", goblin); ApplyEnemy("Charger", boar);
            var ranged = AssetDatabase.LoadAssetAtPath<EnemyData>("Assets/_Game/Data/Enemies/Ranged.asset");
            ranged.projectileSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Game/Art/Weapons/Arrow.png");
            EditorUtility.SetDirty(ranged); AssetDatabase.SaveAssets();
        }
        private static void ApplyEnemy(string name, Sprite sprite)
        {
            string path = $"Assets/_Game/Prefabs/Enemies/{name}.prefab";
            var enemy = PrefabUtility.LoadPrefabContents(path);
            enemy.GetComponent<SpriteRenderer>().sprite = sprite; enemy.GetComponent<SpriteRenderer>().color = Color.white;
            enemy.GetComponent<SpriteRenderer>().sharedMaterial = material;
            PrefabUtility.SaveAsPrefabAsset(enemy, path); PrefabUtility.UnloadPrefabContents(enemy);
        }
        private static Sprite Import(string relative, float height, Vector2 pivot)
        {
            string path = "Assets/_Game/Art/" + relative + ".png";
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureShape = TextureImporterShape.Texture2D; importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single; importer.spritePixelsPerUnit = 100;
            importer.filterMode = FilterMode.Point; importer.mipmapEnabled = false; importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed; importer.maxTextureSize = 512;
            var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect; settings.spriteGenerateFallbackPhysicsShape = false;
            settings.spriteAlignment = (int)SpriteAlignment.Custom; settings.spritePivot = pivot;
            importer.SetTextureSettings(settings); importer.SaveAndReimport();
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null) throw new System.InvalidOperationException("Sprite missing after import: " + path);
            importer.spritePixelsPerUnit *= sprite.bounds.size.y / height;
            importer.SaveAndReimport();
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
    }
}
