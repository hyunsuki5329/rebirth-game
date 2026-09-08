using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Roguelite.Editor
{
    public static class FoundationBuilder
    {
        public static void Create()
        {
            Directory.CreateDirectory("Assets/_Game/Scenes");
            Directory.CreateDirectory("Assets/_Game/Art");
            var texture = new Texture2D(16, 16);
            var pixels = new Color[256];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
            texture.SetPixels(pixels);
            texture.Apply();
            const string spritePath = "Assets/_Game/Art/Square.png";
            if (!File.Exists(spritePath)) File.WriteAllBytes(spritePath, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(spritePath);
            var importer = (TextureImporter)AssetImporter.GetAtPath(spritePath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 16;
            importer.filterMode = FilterMode.Point;
            importer.SaveAndReimport();
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var camera = new GameObject("Main Camera", typeof(Camera)).GetComponent<Camera>();
            camera.tag = "MainCamera";
            camera.orthographic = true;
            camera.orthographicSize = 6;
            camera.transform.position = new Vector3(0, 0, -10);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.025f, 0.04f, 0.07f);
            var body = Box("FoundationBody", Vector2.zero, Vector2.one, new Color(0.2f, 0.9f, 0.8f), sprite);
            var rb = body.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            Box("WallRight", new Vector2(7.5f, 0), new Vector2(1, 12), Color.gray, sprite);
            Box("WallLeft", new Vector2(-7.5f, 0), new Vector2(1, 12), Color.gray, sprite);
            Box("WallTop", new Vector2(0, 5.5f), new Vector2(16, 1), Color.gray, sprite);
            Box("WallBottom", new Vector2(0, -5.5f), new Vector2(16, 1), Color.gray, sprite);
            const string path = "Assets/_Game/Scenes/PlayerTest.unity";
            EditorSceneManager.SaveScene(scene, path);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(path, true) };
            EditorSettings.defaultBehaviorMode = EditorBehaviorMode.Mode2D;
            AssetDatabase.SaveAssets();
            Debug.Log("FOUNDATION_CREATED");
        }

        private static GameObject Box(string name, Vector2 position, Vector2 size, Color color, Sprite sprite)
        {
            var go = new GameObject(name, typeof(SpriteRenderer), typeof(BoxCollider2D));
            go.transform.position = position;
            go.transform.localScale = new Vector3(size.x, size.y, 1);
            var renderer = go.GetComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            go.GetComponent<BoxCollider2D>().size = Vector2.one;
            return go;
        }
    }
}
