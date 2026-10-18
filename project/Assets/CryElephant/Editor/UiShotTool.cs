using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Temporary tool: prepare editor UI for the Canvas/hierarchy proof screenshots.
/// Marker-file handshake: logs SHOTSTEP:<name>, waits for Shots/next_step.flag.
/// Delete this file afterwards.
/// </summary>
public static class UiShotTool
{
    private static int step;

    public static void Run()
    {
        EditorSceneManager.OpenScene("Assets/CryElephant/Scenes/1_Play.unity");
        // 重置为默认布局：确保 Inspector / Hierarchy 面板可见（否则截图只有 Game 视图）
        EditorApplication.ExecuteMenuItem("Window/Layouts/Default");
        step = 0;
        EditorApplication.update += Tick;
    }

    private static void Tick()
    {
        if (step == 0)
        {
            var canvas = Object.FindObjectOfType<Canvas>();
            Selection.activeObject = canvas != null ? canvas.gameObject : null;
            Debug.Log("SHOTSTEP:canvas");
            step = 1;
        }
        else if (step == 1)
        {
            if (File.Exists("Shots/next_step.flag"))
            {
                File.Delete("Shots/next_step.flag");
                var gos = new System.Collections.Generic.List<GameObject>();
                foreach (var n in new[] { "Elephant_Play", "Monkey_Play", "Rabbit_Play", "Balloon_1", "Balloon_2" })
                {
                    var g = GameObject.Find(n);
                    if (g != null) gos.Add(g);
                }
                if (gos.Count > 0) Selection.objects = gos.ToArray();
                Debug.Log("SHOTSTEP:hierarchy");
                step = 2;
            }
        }
        else if (step == 2)
        {
            if (File.Exists("Shots/next_step.flag"))
            {
                File.Delete("Shots/next_step.flag");
                var narr = GameObject.Find("Narration_Play");
                Selection.activeObject = narr != null ? narr : null;
                Debug.Log("SHOTSTEP:audio");
                step = 3;
            }
        }
        else if (step == 3)
        {
            if (File.Exists("Shots/next_step.flag"))
            {
                File.Delete("Shots/next_step.flag");
                Debug.Log("SHOTDONE");
                EditorApplication.update -= Tick;
                // 不退出编辑器：外部脚本还要截图
            }
        }
    }
}
