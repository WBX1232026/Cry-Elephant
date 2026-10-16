using UnityEditor;
using UnityEditor.SceneManagement;

// 临时工具：命令行 -executeMethod PlayTest.Run 一键打开首场景并进入 Play 模式
public static class PlayTest
{
    public static void Run()
    {
        // 打开 Build Settings 中的第一个场景（0_Home）
        var firstScene = EditorBuildSettings.scenes.Length > 0
            ? EditorBuildSettings.scenes[0].path
            : "Assets/Scenes/0_Home.unity";
        EditorSceneManager.OpenScene(firstScene);
        EditorApplication.isPlaying = true;
    }
}
