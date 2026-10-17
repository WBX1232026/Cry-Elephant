using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// One-shot WebGL build for the itch.io submission. Builds the CURRENT final
/// scenes as-is (does NOT rebuild pages - all decorations and tweaks are kept).
/// Run with the 2022.3.62f3c1 (China) editor:
///   Unity.exe -batchmode -quit -projectPath ... -executeMethod ItchBuilder.Build
/// </summary>
public static class ItchBuilder
{
    public static void Build()
    {
        PlayerSettings.companyName = "SCU PROG2006";
        PlayerSettings.productName = "Cry Elephant";
        PlayerSettings.defaultScreenWidth = 1080;
        PlayerSettings.defaultScreenHeight = 1920;
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
        PlayerSettings.WebGL.decompressionFallback = true;
        PlayerSettings.WebGL.template = "APPLICATION:Default";

        string outDir = "Builds/WebGL";
        if (Directory.Exists(outDir)) Directory.Delete(outDir, true);

        var report = BuildPipeline.BuildPlayer(
            EditorBuildSettings.scenes, outDir, BuildTarget.WebGL, BuildOptions.None);

        if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.LogError("[ItchBuilder] BUILD FAILED: " + report.summary.result
                + " errors=" + report.summary.totalErrors);
            EditorApplication.Exit(1);
        }
        else
        {
            Debug.Log("[ItchBuilder] BUILD OK -> " + outDir
                + " size=" + report.summary.totalSize + " bytes");
        }
    }
}
