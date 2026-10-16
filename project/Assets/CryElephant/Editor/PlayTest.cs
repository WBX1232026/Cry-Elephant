using UnityEditor;
using UnityEditor.SceneManagement;

// Utility: -executeMethod PlayTest.Run opens the first scene and enters Play mode in one step
public static class PlayTest
{
    public static void Run()
    {
        // Open the first scene in Build Settings (0_Home)
        var firstScene = EditorBuildSettings.scenes.Length > 0
            ? EditorBuildSettings.scenes[0].path
            : "Assets/Scenes/0_Home.unity";
        EditorSceneManager.OpenScene(firstScene);
        EditorApplication.isPlaying = true;
    }
}
