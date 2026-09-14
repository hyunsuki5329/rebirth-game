using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
namespace Roguelite.Editor
{
    public static class BuildScript
    {
        [MenuItem("Roguelite/Build Windows Development")]
        public static void BuildWindowsDevelopment()
        {
            string[] scenes = { "Assets/_Game/Scenes/MainMenu.unity", "Assets/_Game/Scenes/Hub.unity", "Assets/_Game/Scenes/Dungeon.unity" };
            foreach (string scene in scenes) if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scene) == null) throw new InvalidOperationException("Required build scene is missing: " + scene);
            string output = Path.GetFullPath("Builds/Windows/ActionRoguelite.exe");
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes, locationPathName = output, target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development | BuildOptions.AllowDebugging
            });
            Debug.Log($"ROGUELITE_BUILD result={report.summary.result} errors={report.summary.totalErrors} warnings={report.summary.totalWarnings} bytes={report.summary.totalSize} path={output}");
            if (report.summary.result != BuildResult.Succeeded || report.summary.totalErrors != 0)
                throw new InvalidOperationException("Windows Development Build failed; inspect the build log.");
        }
    }
}
