using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class ValidateGroupSevenFire
{
    const string ScenePath = "Assets/Scenes/FlipbookShowcaseScene.unity";
    const string Output = "C:/Users/lutoX/.codex/visualizations/2026/09/30/01a0efeb-b191-75a3-a339-5fe9aac14e01/";
    static int captured;
    static double next;

    static ValidateGroupSevenFire() { EditorApplication.delayCall += Apply; }

    static void Apply()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorApplication.isPlaying = false;
            EditorApplication.delayCall += Apply;
            return;
        }
        var scene = EditorSceneManager.OpenScene(ScenePath);
        var flame = GameObject.Find("B - Campfire Main Flame");
        if (flame == null) { Debug.LogError("Flame object missing"); return; }
        flame.transform.localScale = new Vector3(1.05f, 2.1f, 1f);
        foreach (var name in new[] { "B - Campfire Smoke Flipbook", "B - Smoke Particles Shader Controlled", "B - Flame Particles Shader Controlled" })
        {
            var item = GameObject.Find(name);
            if (item != null) Object.DestroyImmediate(item);
        }
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        var material = AssetDatabase.LoadAssetAtPath<Material>("Assets/FlipbookEffect/FlameMaterial.mat");
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/FlipbookEffect/FlameFlipbook.png");
        Debug.Log("Group 7-5 applied: atlas " + texture.width + "x" + texture.height +
            ", " + material.GetFloat("_Columns") + "x" + material.GetFloat("_Rows") +
            ", " + material.GetFloat("_Speed") + " fps, filter " + texture.filterMode);
        captured = 0;
        next = EditorApplication.timeSinceStartup + 0.4;
        EditorApplication.update += Capture;
    }

    static void Capture()
    {
        if (EditorApplication.timeSinceStartup < next) return;
        var camera = Camera.main;
        if (camera == null) { EditorApplication.update -= Capture; return; }
        var target = new RenderTexture(1600, 900, 24);
        camera.targetTexture = target;
        camera.Render();
        RenderTexture.active = target;
        var image = new Texture2D(1600, 900, TextureFormat.RGB24, false);
        image.ReadPixels(new Rect(0, 0, 1600, 900), 0, 0);
        image.Apply();
        File.WriteAllBytes(Output + "pixel-fire-group-7-5-preview-" + captured + ".png", image.EncodeToPNG());
        camera.targetTexture = null;
        RenderTexture.active = null;
        Object.DestroyImmediate(image);
        Object.DestroyImmediate(target);
        captured++;
        if (captured == 2) EditorApplication.update -= Capture;
        else next = EditorApplication.timeSinceStartup + 0.5;
        Debug.Log("Group 7-5 capture " + captured);
    }
}

