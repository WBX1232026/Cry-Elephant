using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 绘本《Cry Elephant》六页重构构建器。
/// 菜单：Tools/绘本/构建六页场景
///
/// 资源约定（缺失时自动回退到工程内已有素材，仍缺失则在 Console 打印清单）：
///   Assets/Art/Backgrounds/bg_home|play|choose|cry|calm|credits.png
///   Assets/Art/Characters/elephant_happy.png / elephant_cry.png / elephant_calm.png / monkey.png / rabbit.png / tree.png
///   Assets/Art/Props/balloon_red.png / balloon_blue.png / balloon_yellow.png / balloon_pink.png / tear.png / bubble.png / sun.png
///   Assets/Art/UI/button_orange.png / finger.png
///   Assets/Art/Audio/sfx_click.mp3 / sfx_balloon_pop.mp3 / sfx_elephant.mp3 / sfx_monkey.mp3
///                          sfx_monkey_happy.mp3 / sfx_monkey_sad.mp3 / sfx_breath.mp3 / bgm_happy.mp3
///                          narration_page0.mp3 ... narration_page5.mp3
/// </summary>
public static class HuiBenBuilder
{
    private const string ArtRoot = "Assets/Art";
    private const string ScenesDir = "Assets/Scenes";
    private const float RefW = 1080f;
    private const float RefH = 1920f;

    private static readonly Color Orange = new Color32(0xF2, 0xA6, 0x5A, 0xFF);
    private static readonly Color Cream = new Color32(0xFF, 0xF6, 0xE5, 0xFF);
    private static readonly Color DarkBrown = new Color32(0x4A, 0x2E, 0x16, 0xFF);
    private static readonly Color DarkOverlay = new Color(0.16f, 0.10f, 0.07f, 0.55f);

    private static readonly string[] PageNames = { "0_Home", "1_Play", "2_Choose", "3_Cry", "4_Calm", "5_Credits" };

    private static readonly List<string> Missing = new List<string>();

    // 构建期上下文
    private static TMP_FontAsset _font;
    private static Sprite _buttonSprite;
    private static Sprite _fingerSprite;
    private static Sprite _panelSprite;
    private static Sprite _badgeSprite;
    private static Sprite _ringSprite;
    private static AudioClip _clickSfx;
    private static AudioClip _popSfx;

    // ============ 单页重建（只动指定的一页，其余页保持原样） ============
    [MenuItem("Tools/绘本/只重建 Page0 Home")]
    public static void RebuildHome() { RebuildPage("0_Home", BuildHome); }

    [MenuItem("Tools/绘本/只重建 Page1 Play")]
    public static void RebuildPlay() { RebuildPage("1_Play", BuildPlay); }

    [MenuItem("Tools/绘本/只重建 Page2 Choose")]
    public static void RebuildChoose() { RebuildPage("2_Choose", BuildChoose); }

    [MenuItem("Tools/绘本/只重建 Page3 Cry")]
    public static void RebuildCry() { RebuildPage("3_Cry", BuildCry); }

    [MenuItem("Tools/绘本/只重建 Page4 Calm")]
    public static void RebuildCalm() { RebuildPage("4_Calm", BuildCalm); }

    [MenuItem("Tools/绘本/只重建 Page5 Credits")]
    public static void RebuildCredits() { RebuildPage("5_Credits", BuildCredits); }

    private static void RebuildPage(string sceneName, System.Action build)
    {
        if (!PrepAssets()) return;

        EditorSceneManager.SaveOpenScenes();
        build();
        AssetDatabase.Refresh();
        UpdateBuildSettings();
        ReportMissing("Page " + sceneName);
        Debug.Log("[绘本] 仅重建 " + sceneName + "，其余页未改动。");
    }

    // ============ 全量重建（慎用：会覆盖全部六页的手工调整） ============
    [MenuItem("Tools/绘本/构建六页场景（覆盖全部）")]
    public static void BuildAll()
    {
        if (!PrepAssets()) return;

        EditorSceneManager.SaveOpenScenes();

        BuildHome();
        BuildPlay();
        BuildChoose();
        BuildCry();
        BuildCalm();
        BuildCredits();

        AssetDatabase.Refresh();
        UpdateBuildSettings();
        ReportMissing("六页");
        Debug.Log("[绘本] 六页场景构建完成 → " + ScenesDir);
    }

    /// <summary>构建前的公共准备：字体/九宫格/图标/音效。返回 false 表示不可构建。</summary>
    private static bool PrepAssets()
    {
        Missing.Clear();

        if (EditorApplication.isPlaying)
        {
            Debug.LogError("[绘本] 请先退出 Play 模式再构建。");
            return false;
        }

        EnsureFolders();
        _font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/AaShenYeShiTang-2 SDF.asset");
        if (_font == null) Debug.LogWarning("[绘本] 未找到字体 AaShenYeShiTang-2 SDF，将使用 TMP 默认字体。");

        _buttonSprite = MakeOrLoadButtonSprite();
        _fingerSprite = MakeOrLoadFingerSprite();
        _panelSprite = MakeOrLoadPanelSprite();
        _badgeSprite = MakeOrLoadBadgeSprite();
        _ringSprite = MakeOrLoadRingSprite();
        AssetDatabase.SaveAssets();

        _clickSfx = LoadAudio(ArtRoot + "/Audio/sfx_click.mp3", null, "sfx_click");
        _popSfx = LoadAudio(ArtRoot + "/Audio/sfx_balloon_pop.mp3", "Assets/气球爆破点击声_Preview.mp3", "sfx_balloon_pop");
        return true;
    }

    private static void ReportMissing(string what)
    {
        if (Missing.Count > 0)
            Debug.LogWarning("[绘本] " + what + "：以下资源缺失（已回退或留空）：\n - " + string.Join("\n - ", Missing.ToArray()));
        else
            Debug.Log("[绘本] " + what + "：全部资源齐备，构建完成。");
    }

    // ================= 资源加载 =================
    private static Sprite LoadSprite(string path, string fallback, string label)
    {
        Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (s != null) return s;

        if (!string.IsNullOrEmpty(fallback))
        {
            s = AssetDatabase.LoadAssetAtPath<Sprite>(fallback);
            if (s != null)
            {
                Missing.Add(label + " → " + path + "（已回退 " + fallback + "）");
                return s;
            }
        }

        Missing.Add(label + " → " + path);
        return null;
    }

    private static AudioClip LoadAudio(string path, string fallback, string label)
    {
        AudioClip c = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        if (c != null) return c;

        if (!string.IsNullOrEmpty(fallback))
        {
            c = AssetDatabase.LoadAssetAtPath<AudioClip>(fallback);
            if (c != null) return c;
        }

        Missing.Add("音频 " + label + " → " + path);
        return null;
    }

    private static AudioClip LoadAudioSoft(string path)
    {
        return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
    }

    private static void EnsureFolders()
    {
        EnsureFolder("Assets", "Art");
        EnsureFolder(ArtRoot, "Backgrounds");
        EnsureFolder(ArtRoot, "Characters");
        EnsureFolder(ArtRoot, "Props");
        EnsureFolder(ArtRoot, "UI");
        EnsureFolder(ArtRoot, "Audio");
        EnsureFolder("Assets", "Scenes");
    }

    private static void EnsureFolder(string parent, string child)
    {
        if (!AssetDatabase.IsValidFolder(parent + "/" + child))
            AssetDatabase.CreateFolder(parent, child);
    }

    // ================= 统一样式 UI 图 =================
    private static Sprite MakeOrLoadButtonSprite()
    {
        string path = ArtRoot + "/UI/button_orange.png";
        Sprite existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (existing != null) return existing;

        const int size = 96;
        const int radius = 26;
        const int outline = 5;

        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] px = new Color[size * size];
        Color ink = new Color(0.15f, 0.09f, 0.04f, 1f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float d = RoundedRectDistance(x, y, size, radius);
                Color c = d <= -(outline + 1f) ? Orange
                        : d <= 0f ? ink
                        : new Color(0f, 0f, 0f, 0f);
                px[y * size + x] = c;
            }
        }

        tex.SetPixels(px);
        tex.Apply();
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

        TextureImporter imp = (TextureImporter)AssetImporter.GetAtPath(path);
        imp.textureType = TextureImporterType.Sprite;
        imp.spriteImportMode = SpriteImportMode.Single;
        imp.alphaIsTransparency = true;
        imp.mipmapEnabled = false;
        TextureImporterSettings st = new TextureImporterSettings();
        imp.ReadTextureSettings(st);
        st.spriteBorder = new Vector4(radius + outline + 2, radius + outline + 2, radius + outline + 2, radius + outline + 2);
        st.spriteMeshType = SpriteMeshType.FullRect;
        imp.SetTextureSettings(st);
        imp.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static Sprite MakeOrLoadFingerSprite()
    {
        string path = ArtRoot + "/UI/finger.png";
        Sprite existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (existing != null) return existing;

        const int w = 150;
        const int h = 200;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        Color[] px = new Color[w * h];
        Color fill = new Color(1f, 0.99f, 0.94f, 1f);
        Color ink = new Color(0.24f, 0.14f, 0.06f, 1f);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                bool inside = FingerMask(x, y, 0f);
                bool core = FingerMask(x, y, 7f);
                px[y * w + x] = !inside ? new Color(0f, 0f, 0f, 0f) : (core ? fill : ink);
            }
        }

        tex.SetPixels(px);
        tex.Apply();
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

        TextureImporter imp = (TextureImporter)AssetImporter.GetAtPath(path);
        imp.textureType = TextureImporterType.Sprite;
        imp.spriteImportMode = SpriteImportMode.Single;
        imp.alphaIsTransparency = true;
        imp.mipmapEnabled = false;
        imp.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    // 指向上方的手型：食指 + 手掌 + 拇指 + 指节
    private static bool FingerMask(float x, float y, float inset)
    {
        return Rounded(x, y, 72f, 122f, 26f - inset, 70f - inset, Mathf.Max(2f, 26f - inset))
            || Rounded(x, y, 78f, 50f, 56f - inset, 42f - inset, Mathf.Max(2f, 30f - inset))
            || Rounded(x, y, 30f, 66f, 24f - inset, 34f - inset, Mathf.Max(2f, 22f - inset))
            || Rounded(x, y, 116f, 56f, 22f - inset, 28f - inset, Mathf.Max(2f, 20f - inset));
    }

    private static Sprite MakeOrLoadPanelSprite()
    {
        string path = ArtRoot + "/UI/panel_cream.png";
        Sprite existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (existing != null) return existing;

        const int size = 96;
        const int radius = 30;
        const int outline = 4;

        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] px = new Color[size * size];
        Color panelFill = new Color(1f, 0.97f, 0.90f, 0.93f);
        Color panelInk = new Color(0.30f, 0.19f, 0.10f, 0.42f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float d = RoundedRectDistance(x, y, size, radius);
                Color c = d <= -(outline + 1f) ? panelFill
                        : d <= 0f ? panelInk
                        : new Color(0f, 0f, 0f, 0f);
                px[y * size + x] = c;
            }
        }

        tex.SetPixels(px);
        tex.Apply();
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

        TextureImporter imp = (TextureImporter)AssetImporter.GetAtPath(path);
        imp.textureType = TextureImporterType.Sprite;
        imp.spriteImportMode = SpriteImportMode.Single;
        imp.alphaIsTransparency = true;
        imp.mipmapEnabled = false;
        TextureImporterSettings st = new TextureImporterSettings();
        imp.ReadTextureSettings(st);
        st.spriteBorder = new Vector4(radius + outline + 2, radius + outline + 2, radius + outline + 2, radius + outline + 2);
        st.spriteMeshType = SpriteMeshType.FullRect;
        imp.SetTextureSettings(st);
        imp.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    // 金色太阳徽章：用于气泡旁标注"说话者"
    private static Sprite MakeOrLoadBadgeSprite()
    {
        string path = ArtRoot + "/UI/badge_sun.png";
        Sprite existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (existing != null) return existing;

        const int size = 128;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] px = new Color[size * size];
        Color gold = new Color32(0xF7, 0xC8, 0x4A, 0xFF);
        Color goldDark = new Color32(0xC9, 0x8A, 0x1E, 0xFF);

        float cx = size * 0.5f;
        float cy = size * 0.5f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x + 0.5f - cx;
                float dy = y + 0.5f - cy;
                float r = Mathf.Sqrt(dx * dx + dy * dy);
                float ang = Mathf.Atan2(dy, dx);
                // 外层花瓣/光芒：半径随角度波动
                float outer = 50f + 12f * Mathf.Cos(6f * ang);
                Color c = r <= outer ? (r <= 40f ? gold : goldDark) : new Color(0f, 0f, 0f, 0f);
                px[y * size + x] = c;
            }
        }

        tex.SetPixels(px);
        tex.Apply();
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        ConfigureSprite(path);
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    // 泪滴占位图：圆底 + 尖顶
    private static Sprite MakeOrLoadTearSprite()
    {
        string path = ArtRoot + "/Props/tear.png";
        Sprite existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (existing != null) return existing;

        const int w = 72;
        const int h = 108;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        Color[] px = new Color[w * h];
        Color fill = new Color(0.50f, 0.78f, 0.97f, 1f);
        Color ink = new Color(0.16f, 0.40f, 0.68f, 1f);

        float cx = w * 0.5f;
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float px0 = x + 0.5f;
                float py0 = y + 0.5f;
                // 底部圆
                float dc = Mathf.Sqrt((px0 - cx) * (px0 - cx) + (py0 - 36f) * (py0 - 36f));
                // 上部尖顶
                bool inTri = py0 >= 36f && py0 <= 100f && Mathf.Abs(px0 - cx) <= (100f - py0) * 0.30f;
                bool inside = dc <= 30f || inTri;

                bool coreC = dc <= 24f;
                bool coreTri = py0 >= 42f && py0 <= 94f && Mathf.Abs(px0 - cx) <= (94f - py0) * 0.25f;
                bool core = coreC || coreTri;

                px[y * w + x] = !inside ? new Color(0f, 0f, 0f, 0f) : (core ? fill : ink);
            }
        }

        tex.SetPixels(px);
        tex.Apply();
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        ConfigureSprite(path);
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    // 呼吸光环：一圈柔和的半透明金色圆环（Page 4 深呼吸引导）
    private static Sprite MakeOrLoadRingSprite()
    {
        string path = ArtRoot + "/UI/breath_ring.png";
        Sprite existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (existing != null) return existing;

        const int size = 256;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] px = new Color[size * size];
        Color ring = new Color32(0xFF, 0xD9, 0x7A, 0xC8);

        float cx = size * 0.5f;
        float cy = size * 0.5f;
        float rOuter = 118f;
        float rInner = 92f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x + 0.5f - cx;
                float dy = y + 0.5f - cy;
                float r = Mathf.Sqrt(dx * dx + dy * dy);
                float a = 0f;
                if (r <= rOuter && r >= rInner)
                {
                    // 内外边缘羽化，避免锯齿
                    float edge = Mathf.Min(rOuter - r, r - rInner);
                    a = Mathf.Clamp01(edge / 8f);
                }
                Color c = ring;
                c.a *= a;
                px[y * size + x] = c;
            }
        }

        tex.SetPixels(px);
        tex.Apply();
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        ConfigureSprite(path);
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static void ConfigureSprite(string path)
    {
        TextureImporter imp = (TextureImporter)AssetImporter.GetAtPath(path);
        imp.textureType = TextureImporterType.Sprite;
        imp.spriteImportMode = SpriteImportMode.Single;
        imp.alphaIsTransparency = true;
        imp.mipmapEnabled = false;
        imp.SaveAndReimport();
    }

    private static float RoundedRectDistance(float x, float y, int size, float radius)
    {
        float half = size * 0.5f;
        float dx = Mathf.Abs(x + 0.5f - half) - (half - radius);
        float dy = Mathf.Abs(y + 0.5f - half) - (half - radius);
        float ax = Mathf.Max(dx, 0f);
        float ay = Mathf.Max(dy, 0f);
        return Mathf.Sqrt(ax * ax + ay * ay) + Mathf.Min(Mathf.Max(dx, dy), 0f) - radius;
    }

    private static bool Rounded(float x, float y, float cx, float cy, float halfW, float halfH, float r)
    {
        float dx = Mathf.Abs(x - cx) - (halfW - r);
        float dy = Mathf.Abs(y - cy) - (halfH - r);
        float ax = Mathf.Max(dx, 0f);
        float ay = Mathf.Max(dy, 0f);
        return Mathf.Sqrt(ax * ax + ay * ay) + Mathf.Min(Mathf.Max(dx, dy), 0f) - r <= 0f;
    }

    // ================= 页面骨架 =================
    private static RectTransform BeginPage(string sceneName, string bgPath, string bgFallback)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject camGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        camGo.tag = "MainCamera";
        Camera cam = camGo.GetComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = RefH * 0.5f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Cream;
        camGo.transform.position = new Vector3(0f, 0f, -10f);

        new GameObject("EventSystem",
            typeof(UnityEngine.EventSystems.EventSystem),
            typeof(UnityEngine.EventSystems.StandaloneInputModule));

        GameObject app = new GameObject("_App");
        Sfx sfx = app.AddComponent<Sfx>();
        app.AddComponent<BgmManager>();
        SetField(sfx, "clickClip", _clickSfx);
        SetField(sfx, "balloonPopClip", _popSfx);

        GameObject canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(RefW, RefH);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        RectTransform root = canvasGo.GetComponent<RectTransform>();

        Sprite bg = LoadSprite(bgPath, bgFallback, sceneName + " 背景");
        if (bg != null)
        {
            RectTransform bgRt = MakeImage(root, "Background", bg, new Vector2(0.5f, 0.5f), Vector2.zero);
            Stretch(bgRt);
            AspectRatioFitter fitter = bgRt.gameObject.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fitter.aspectRatio = bg.rect.width / bg.rect.height;
            bgRt.SetSiblingIndex(0);
        }
        else
        {
            Debug.LogWarning("[绘本] " + sceneName + " 缺少背景图，页面将只有 UI。");
        }

        return root;
    }

    private static void EndPage(string sceneName)
    {
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), ScenesDir + "/" + sceneName + ".unity");
    }

    // ================= Page 0 Home =================
    private static void BuildHome()
    {
        RectTransform root = BeginPage("0_Home", ArtRoot + "/Backgrounds/bg_home.png", "Assets/微信图片_2026-06-24_135127_690.jpg");

        RectTransform title = MakeText(root, "Title_Text", "Cry Elephant\n哭不停的小象", 90, DarkBrown,
            new Vector2(0.5f, 0.80f), new Vector2(920f, 300f));
        FloatLoop titleFloat = title.gameObject.AddComponent<FloatLoop>();
        SetFloat(titleFloat, "distance", 12f);
        SetFloat(titleFloat, "duration", 1.4f);

        RectTransform start = MakeButton(root, "Button_Start", "Start Reading", new Vector2(0.5f, 0.26f), new Vector2(600f, 180f), 64);
        WireNavigation(start.gameObject, "1_Play");

        FingerHint(root, "Finger_Icon", new Vector2(0.5f, 0.15f));
        MakeNarration(root, "Narration_Home", ArtRoot + "/Audio/narration_page0.mp3");

        EndPage("0_Home");
    }

    // ================= Page 1 Play =================
    private static void BuildPlay()
    {
        RectTransform root = BeginPage("1_Play", ArtRoot + "/Backgrounds/bg_play.png", "Assets/157dbf7a0d03c33e5d12de47be83d893.jpg");
        MakeBanner(root, "和朋友一起玩");

        Sprite balloonBlue = LoadSprite(ArtRoot + "/Props/balloon_blue.png", "Assets/katonglansedeqiqiuyuansu_11767377.png", "balloon_blue");
        Sprite balloonRed = LoadSprite(ArtRoot + "/Props/balloon_red.png", "Assets/katonglansedeqiqiuyuansu_11767377.png", "balloon_red");
        Sprite balloonYellow = LoadSprite(ArtRoot + "/Props/balloon_yellow.png", "Assets/katonglansedeqiqiuyuansu_11767377.png", "balloon_yellow");
        Sprite monkeySp = LoadSprite(ArtRoot + "/Characters/monkey.png", "Assets/8a237eeee5eb722a0d274294d7d3eea8.png", "monkey");
        Sprite sunSp = LoadSprite(ArtRoot + "/Props/sun.png", "Assets/23e13b13ebb0c2f5997a773335616be5.png", "sun");
        Sprite elephant = LoadSprite(ArtRoot + "/Characters/elephant_happy.png", null, "elephant_happy");
        Sprite rabbit = LoadSprite(ArtRoot + "/Characters/rabbit.png", null, "rabbit");
        Sprite tree = LoadSprite(ArtRoot + "/Characters/tree.png", null, "tree");

        MakeImage(root, "Sun_Play", sunSp, new Vector2(0.16f, 0.87f), SpriteSize(sunSp, 220f));
        MakeImage(root, "Tree_Play", tree, new Vector2(0.87f, 0.33f), SpriteSize(tree, 380f));

        RectTransform el = MakeImage(root, "Elephant_Play", elephant, new Vector2(0.38f, 0.40f), SpriteSize(elephant, 470f));
        if (elephant != null)
        {
            TapReaction et = el.gameObject.AddComponent<TapReaction>();
            SetEnum(et, "kind", "EarWiggle");
            SetField(et, "sound", LoadAudioSoft(ArtRoot + "/Audio/sfx_elephant.mp3"));
        }

        MakeImage(root, "Rabbit_Play", rabbit, new Vector2(0.19f, 0.31f), SpriteSize(rabbit, 270f));

        RectTransform monkey = MakeImage(root, "Monkey_Play", monkeySp, new Vector2(0.75f, 0.60f), SpriteSize(monkeySp, 400f));
        TapReaction mt = monkey.gameObject.AddComponent<TapReaction>();
        SetEnum(mt, "kind", "Jump");
        SetField(mt, "sound", LoadAudioSoft(ArtRoot + "/Audio/sfx_monkey.mp3"));

        BalloonProp(root, "Balloon_1", balloonBlue, new Vector2(0.15f, 0.62f), 0f, "FloatUp");
        BalloonProp(root, "Balloon_2", balloonRed, new Vector2(0.87f, 0.71f), 0.3f, "FloatUp");
        BalloonProp(root, "Balloon_3", balloonYellow, new Vector2(0.62f, 0.85f), 0.6f, "FloatUp");

        MakeTextWithPanel(root, "Text_Hint", "点一点气球", 56, DarkBrown, new Vector2(0.5f, 0.135f), new Vector2(620f, 100f), new Vector2(46f, 26f));
        MakeNarration(root, "Narration_Play", ArtRoot + "/Audio/narration_page1.mp3");

        RectTransform next = MakeButton(root, "Button_Next", "下一页", new Vector2(0.78f, 0.075f), new Vector2(330f, 132f), 50);
        WireNavigation(next.gameObject, "2_Choose");

        FingerHint(root, "Finger_Balloon", new Vector2(0.07f, 0.55f));
        FingerHint(root, "Finger_Elephant", new Vector2(0.30f, 0.53f));
        FingerHint(root, "Finger_Monkey", new Vector2(0.66f, 0.66f));

        EndPage("1_Play");
    }

    private static void BalloonProp(RectTransform root, string name, Sprite sprite, Vector2 anchor, float delay, string kind)
    {
        RectTransform rt = MakeImage(root, name, sprite, anchor, SpriteSize(sprite, 300f));
        TapReaction tr = rt.gameObject.AddComponent<TapReaction>();
        SetEnum(tr, "kind", kind);
        SetField(tr, "sound", _popSfx);

        FloatLoop fl = rt.gameObject.AddComponent<FloatLoop>();
        SetFloat(fl, "distance", 36f);
        SetFloat(fl, "delay", delay);
    }

    // ================= Page 2 Choose =================
    private static void BuildChoose()
    {
        RectTransform root = BeginPage("2_Choose", ArtRoot + "/Backgrounds/bg_choose.png", "Assets/f61fc3d23dad28ab64dce7425cff78fc.jpg");

        Sprite monkeySp = LoadSprite(ArtRoot + "/Characters/monkey.png", "Assets/8a237eeee5eb722a0d274294d7d3eea8.png", "monkey");
        Sprite balloon = LoadSprite(ArtRoot + "/Props/balloon_blue.png", "Assets/katonglansedeqiqiuyuansu_11767377.png", "balloon_blue");
        Sprite elephant = LoadSprite(ArtRoot + "/Characters/elephant_happy.png", null, "elephant_happy");

        MakeImage(root, "Elephant_Meet", elephant, new Vector2(0.34f, 0.44f), SpriteSize(elephant, 450f));
        RectTransform monkey = MakeImage(root, "Monkey_Meet", monkeySp, new Vector2(0.74f, 0.50f), SpriteSize(monkeySp, 390f));
        // 猴子的"期待动作"：轻轻上下浮动，提示它想要气球
        FloatLoop monkeyExpect = monkey.gameObject.AddComponent<FloatLoop>();
        SetFloat(monkeyExpect, "distance", 18f);
        SetFloat(monkeyExpect, "duration", 1.0f);
        MakeImage(root, "Balloon_Meet", balloon, new Vector2(0.50f, 0.70f), SpriteSize(balloon, 300f));

        MakeTextWithPanel(root, "Text_Question", "你会怎么做？\n你想分享气球吗？", 60, DarkBrown, new Vector2(0.5f, 0.865f), new Vector2(820f, 240f), new Vector2(52f, 34f));
        MakeNarration(root, "Narration_Choose", ArtRoot + "/Audio/narration_page2.mp3");

        RectTransform share = MakeButton(root, "Button_Share", "分享气球", new Vector2(0.30f, 0.19f), new Vector2(430f, 155f), 52);
        RectTransform keep = MakeButton(root, "Button_Keep", "自己玩", new Vector2(0.70f, 0.19f), new Vector2(430f, 155f), 52);

        AudioClip happy = LoadAudioSoft(ArtRoot + "/Audio/sfx_monkey_happy.mp3");
        AudioClip sad = LoadAudioSoft(ArtRoot + "/Audio/sfx_monkey_sad.mp3");

        ChoiceController shareCtrl = share.gameObject.AddComponent<ChoiceController>();
        SetField(shareCtrl, "monkey", monkey);
        SetString(shareCtrl, "nextScene", "3_Cry");
        SetField(shareCtrl, "happySound", happy);
        SetField(shareCtrl, "sadSound", sad);
        UnityEventTools.AddPersistentListener(share.GetComponent<Button>().onClick, shareCtrl.ChooseShare);

        ChoiceController keepCtrl = keep.gameObject.AddComponent<ChoiceController>();
        SetField(keepCtrl, "monkey", monkey);
        SetString(keepCtrl, "nextScene", "3_Cry");
        SetField(keepCtrl, "happySound", happy);
        SetField(keepCtrl, "sadSound", sad);
        UnityEventTools.AddPersistentListener(keep.GetComponent<Button>().onClick, keepCtrl.ChooseKeep);

        FingerHint(root, "Finger_Share", new Vector2(0.14f, 0.17f));
        FingerHint(root, "Finger_Keep", new Vector2(0.54f, 0.17f));

        EndPage("2_Choose");
    }

    // ================= Page 3 Cry =================
    private static void BuildCry()
    {
        RectTransform root = BeginPage("3_Cry", ArtRoot + "/Backgrounds/bg_cry.png", "Assets/14fe8c585e519a46f6768d4fcec8b1a0.jpg");
        MakeBanner(root, "小象摔倒了，哭起来");

        RectTransform overlay = MakeImage(root, "Dark_Overlay", null, new Vector2(0.5f, 0.5f), Vector2.zero);
        Stretch(overlay);
        Image oi = overlay.GetComponent<Image>();
        oi.enabled = true;
        oi.color = DarkOverlay;
        oi.raycastTarget = false;
        overlay.SetSiblingIndex(1);

        Sprite elephant = LoadSprite(ArtRoot + "/Characters/elephant_cry.png", null, "elephant_cry");
        Sprite friend = LoadSprite(ArtRoot + "/Characters/rabbit.png", null, "rabbit");
        Sprite bubble = LoadSprite(ArtRoot + "/Props/bubble.png", "Assets/(568).png", "bubble");
        Sprite tear = MakeOrLoadTearSprite();   // 缺图时生成蓝色泪滴占位

        RectTransform el = MakeImage(root, "Elephant_Fall", elephant, new Vector2(0.33f, 0.42f), SpriteSize(elephant, 470f));
        TapReaction et = el.gameObject.AddComponent<TapReaction>();
        SetEnum(et, "kind", "Cry");
        SetField(et, "sound", AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/哭哭_Preview.mp3"));

        RectTransform fr = MakeImage(root, "Friend_Fall", friend, new Vector2(0.77f, 0.35f), SpriteSize(friend, 280f));
        TapReaction ft = fr.gameObject.AddComponent<TapReaction>();
        SetEnum(ft, "kind", "Comfort");
        SetField(ft, "sound", LoadAudioSoft(ArtRoot + "/Audio/sfx_monkey.mp3"));

        if (tear != null)
        {
            GameObject tearHost = new GameObject("Tear_Host", typeof(RectTransform));
            RectTransform tearRt = tearHost.GetComponent<RectTransform>();
            tearRt.SetParent(root, false);
            tearRt.anchorMin = tearRt.anchorMax = new Vector2(0.33f, 0.55f);
            tearRt.pivot = new Vector2(0.5f, 0.5f);
            tearRt.sizeDelta = new Vector2(56f, 84f);
            TearDrops td = tearHost.AddComponent<TearDrops>();
            SetField(td, "template", MakeImage(tearRt, "Tear_Template", tear, new Vector2(0.5f, 0.5f), new Vector2(56f, 84f)));
        }

        GameObject bubblesGo = new GameObject("Bubbles", typeof(RectTransform));
        RectTransform bubblesHost = bubblesGo.GetComponent<RectTransform>();
        bubblesHost.SetParent(root, false);
        Stretch(bubblesHost);

        string[] lines = { "别哭啦，我们一起玩吧！", "先深呼吸，慢慢就好了。", "我们都在这里陪着你。" };
        Vector2[] anchors = { new Vector2(0.64f, 0.78f), new Vector2(0.28f, 0.64f), new Vector2(0.66f, 0.50f) };

        List<RectTransform> bubbles = new List<RectTransform>();
        for (int i = 0; i < lines.Length; i++)
        {
            RectTransform b = MakeBubble(bubblesHost, "Bubble_" + (i + 1), bubble, lines[i], anchors[i], 600f, 250f);
            TapReaction bt = b.gameObject.AddComponent<TapReaction>();
            SetEnum(bt, "kind", "Pop");
            SetField(bt, "sound", _popSfx);
            FloatLoop fl = b.gameObject.AddComponent<FloatLoop>();
            SetFloat(fl, "distance", 16f);
            SetFloat(fl, "delay", i * 0.2f);
            // 金色太阳徽章：标注说话者
            RectTransform badge = MakeImage(b, "Speaker_Badge", _badgeSprite, new Vector2(0.08f, 0.84f), new Vector2(88f, 88f));
            badge.GetComponent<Image>().raycastTarget = false;
            bubbles.Add(b);
        }

        BubbleSequence seq = bubblesGo.AddComponent<BubbleSequence>();
        SetArray(seq, "bubbles", bubbles.ToArray());

        MakeNarration(root, "Narration_Cry", ArtRoot + "/Audio/narration_page3.mp3");

        RectTransform next = MakeButton(root, "Button_Next", "下一页", new Vector2(0.78f, 0.075f), new Vector2(330f, 132f), 50);
        WireNavigation(next.gameObject, "4_Calm");

        FingerHint(root, "Finger_Bubble", new Vector2(0.45f, 0.71f));

        EndPage("3_Cry");
    }

    // ================= Page 4 Calm =================
    private static void BuildCalm()
    {
        RectTransform root = BeginPage("4_Calm", ArtRoot + "/Backgrounds/bg_calm.png", "Assets/ad73ce70eab1593be78dd9fe2b021536.jpg");
        MakeBanner(root, "Let's Calm Down Together");

        Sprite elephant = LoadSprite(ArtRoot + "/Characters/elephant_calm.png", ArtRoot + "/Characters/elephant_happy.png", "elephant_calm");
        Sprite friend = LoadSprite(ArtRoot + "/Characters/rabbit.png", null, "rabbit");
        Sprite balloon = LoadSprite(ArtRoot + "/Props/balloon_blue.png", "Assets/katonglansedeqiqiuyuansu_11767377.png", "balloon_blue");
        Sprite bubble = LoadSprite(ArtRoot + "/Props/bubble.png", "Assets/(568).png", "bubble");

        RectTransform el = MakeImage(root, "Elephant_Calm", elephant, new Vector2(0.38f, 0.44f), SpriteSize(elephant, 1400f));

        // 呼吸光环：放在小象身后，随呼吸放大缩小（略大于小象，保证四周露出）
        RectTransform halo = MakeImage(root, "Breath_Halo", _ringSprite, new Vector2(0.40f, 0.44f), new Vector2(940f, 940f));
        halo.GetComponent<Image>().raycastTarget = false;
        halo.SetSiblingIndex(el.GetSiblingIndex());   // 移到小象之前 → 显示在身后

        DeepBreath db = el.gameObject.AddComponent<DeepBreath>();
        SetField(db, "target", el);
        SetField(db, "halo", halo);
        SetField(db, "breathSound", LoadAudioSoft(ArtRoot + "/Audio/sfx_breath.mp3"));
        SetField(db, "bgm", LoadAudioSoft(ArtRoot + "/Audio/bgm_happy.mp3"));

        // 呼吸引导文字
        RectTransform hint = MakeTextWithPanel(root, "Text_BreathHint", "Tap the elephant to breathe together", 40, DarkBrown,
            new Vector2(0.5f, 0.13f), new Vector2(760f, 84f), new Vector2(40f, 26f));
        SetField(db, "hintLabel", hint.GetComponent<TextMeshProUGUI>());
        SetString(db, "inhaleText", "Breathe in...");
        SetString(db, "exhaleText", "Breathe out...");

        RectTransform fr = MakeImage(root, "Friend_Calm", friend, new Vector2(0.77f, 0.37f), SpriteSize(friend, 280f));
        TapReaction ft = fr.gameObject.AddComponent<TapReaction>();
        SetEnum(ft, "kind", "Jump");
        SetField(ft, "sound", LoadAudioSoft(ArtRoot + "/Audio/sfx_monkey.mp3"));
        SetField(db, "companion", fr);   // 兔子跟着一起呼吸

        BalloonProp(root, "Balloon_Calm", balloon, new Vector2(0.72f, 0.71f), 0f, "FloatAway");

        RectTransform bubbleCalm = MakeBubble(root, "Bubble_Calm", bubble,
            "Thank you, everyone! I can handle my feelings now.", new Vector2(0.50f, 0.80f), 760f, 260f);
        ChoiceText ct = bubbleCalm.GetChild(0).gameObject.AddComponent<ChoiceText>();
        SetString(ct, "sharedText", "Thank you, everyone! I can handle my feelings now.");
        SetString(ct, "keepText", "Next time I'll tell you how I feel. Thanks for waiting.");

        MakeNarration(root, "Narration_Calm", ArtRoot + "/Audio/narration_page4.mp3");

        RectTransform next = MakeButton(root, "Button_Next", "Next", new Vector2(0.78f, 0.075f), new Vector2(330f, 132f), 52);
        WireNavigation(next.gameObject, "5_Credits");

        FingerHint(root, "Finger_Elephant", new Vector2(0.15f, 0.33f));

        EndPage("4_Calm");
    }

    // ================= Page 5 Credits =================
    private static void BuildCredits()
    {
        RectTransform root = BeginPage("5_Credits", ArtRoot + "/Backgrounds/bg_credits.png", "Assets/ad73ce70eab1593be78dd9fe2b021536.jpg");

        RectTransform creditsText = MakeTextWithPanel(root, "Text_Credits",
            "Cry Elephant\n哭不停的小象\n\n作者：XXX\n字体：AaShenYeShiTang-2\n音效来源：XXX\n\n参考文献\nNorman, D. (2013) The Design of Everyday Things\nNielsen, J. (1994) Usability Heuristics\n\n感谢阅读！",
            40, DarkBrown, new Vector2(0.5f, 0.52f), new Vector2(900f, 1020f), new Vector2(56f, 50f));
        creditsText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Top;
        creditsText.gameObject.AddComponent<TextFadeIn>();

        RectTransform home = MakeButton(root, "Button_Home", "Back to Home", new Vector2(0.5f, 0.17f), new Vector2(640f, 165f), 54);
        WireNavigation(home.gameObject, "0_Home");

        MakeTextWithPanel(root, "Text_HomeHint", "点这里回到首页", 44, DarkBrown,
            new Vector2(0.5f, 0.095f), new Vector2(560f, 62f), new Vector2(36f, 16f));

        FingerHint(root, "Finger_Home", new Vector2(0.5f, 0.045f));
        MakeNarration(root, "Narration_Credits", ArtRoot + "/Audio/narration_page5.mp3");

        EndPage("5_Credits");
    }

    // ================= 构件 =================
    private static RectTransform MakeImage(RectTransform parent, string name, Sprite sprite, Vector2 anchor, Vector2 size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = size;

        Image img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.preserveAspect = false;
        img.raycastTarget = sprite != null;
        img.enabled = sprite != null;
        return rt;
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
    }

    private static Vector2 SpriteSize(Sprite s, float reference)
    {
        if (s == null) return new Vector2(reference, reference);
        return new Vector2(reference * (s.rect.width / s.rect.height), reference);
    }

    private static RectTransform MakeText(RectTransform parent, string name, string content, float size, Color color, Vector2 anchor, Vector2 sizeDelta)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = sizeDelta;

        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        if (_font != null) tmp.font = _font;
        tmp.text = content;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        return rt;
    }

    // 在文字下方加一块奶油色圆角底板，保证在花哨背景上依然可读
    private static RectTransform MakeTextWithPanel(RectTransform parent, string name, string content, float size, Color color, Vector2 anchor, Vector2 sizeDelta, Vector2 pad)
    {
        RectTransform panel = MakeImage(parent, name + "_Panel", _panelSprite, anchor, sizeDelta + pad * 2f);
        Image pimg = panel.GetComponent<Image>();
        pimg.type = Image.Type.Sliced;
        pimg.raycastTarget = false;
        pimg.enabled = _panelSprite != null;

        return MakeText(panel, name, content, size, color, new Vector2(0.5f, 0.5f), sizeDelta);
    }

    // 每页统一顶部 Banner（响应老师 C2：UI 不统一）
    private static void MakeBanner(RectTransform parent, string pageTitle)
    {
        RectTransform t = MakeTextWithPanel(parent, "Banner", pageTitle, 46, DarkBrown,
            new Vector2(0.5f, 0.955f), new Vector2(620f, 86f), new Vector2(40f, 18f));
        t.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
    }

    private static RectTransform MakeButton(RectTransform parent, string name, string label, Vector2 anchor, Vector2 size, float fontSize)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = size;

        Image img = go.GetComponent<Image>();
        img.sprite = _buttonSprite;
        img.type = Image.Type.Sliced;
        img.color = Color.white;
        img.preserveAspect = false;

        go.AddComponent<PulseLoop>();
        go.AddComponent<PressFeedback>();

        GameObject textGo = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer));
        RectTransform textRt = textGo.GetComponent<RectTransform>();
        textRt.SetParent(rt, false);
        Stretch(textRt);
        textRt.offsetMin = new Vector2(28f, 0f);
        textRt.offsetMax = new Vector2(-28f, 0f);

        TextMeshProUGUI tmp = textGo.AddComponent<TextMeshProUGUI>();
        if (_font != null) tmp.font = _font;
        tmp.text = label;
        tmp.fontSize = fontSize;
        tmp.color = DarkBrown;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;

        return rt;
    }

    private static RectTransform MakeBubble(RectTransform parent, string name, Sprite bubbleSprite, string line, Vector2 anchor, float w, float h)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(w, h);

        Image img = go.GetComponent<Image>();
        img.sprite = bubbleSprite;
        img.type = bubbleSprite != null && bubbleSprite.border != Vector4.zero ? Image.Type.Sliced : Image.Type.Simple;
        img.raycastTarget = true;

        GameObject textGo = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer));
        RectTransform textRt = textGo.GetComponent<RectTransform>();
        textRt.SetParent(rt, false);
        Stretch(textRt);
        textRt.offsetMin = new Vector2(60f, 74f);
        textRt.offsetMax = new Vector2(-60f, -44f);

        TextMeshProUGUI tmp = textGo.AddComponent<TextMeshProUGUI>();
        if (_font != null) tmp.font = _font;
        tmp.text = line;
        tmp.fontSize = 40;
        tmp.color = DarkBrown;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;

        return rt;
    }

    private static void FingerHint(RectTransform parent, string name, Vector2 anchor)
    {
        RectTransform rt = MakeImage(parent, name, _fingerSprite, anchor, new Vector2(108f, 144f));
        rt.GetComponent<Image>().raycastTarget = false;
        rt.GetComponent<Image>().enabled = _fingerSprite != null;
        FloatLoop fl = rt.gameObject.AddComponent<FloatLoop>();
        SetFloat(fl, "distance", 20f);
        SetFloat(fl, "duration", 0.9f);
    }

    private static void MakeNarration(RectTransform parent, string name, string clipPath)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        NarrationOnce n = go.AddComponent<NarrationOnce>();
        SetField(n, "clip", AssetDatabase.LoadAssetAtPath<AudioClip>(clipPath));
    }

    private static void WireNavigation(GameObject buttonGo, string targetScene)
    {
        LoadSceneByName router = buttonGo.AddComponent<LoadSceneByName>();
        SetString(router, "sceneName", targetScene);
        SetField(router, "clickSound", _clickSfx);
        UnityEventTools.AddPersistentListener(buttonGo.GetComponent<Button>().onClick, router.LoadScene);
    }

    // ================= 序列化写值 =================
    private static void SetField(Object target, string field, Object value)
    {
        if (target == null) return;
        SerializedObject so = new SerializedObject(target);
        SerializedProperty p = so.FindProperty(field);
        if (p == null) { Debug.LogWarning("[绘本] 字段不存在: " + target.GetType().Name + "." + field); return; }
        p.objectReferenceValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetString(Object target, string field, string value)
    {
        if (target == null) return;
        SerializedObject so = new SerializedObject(target);
        SerializedProperty p = so.FindProperty(field);
        if (p == null) { Debug.LogWarning("[绘本] 字段不存在: " + target.GetType().Name + "." + field); return; }
        p.stringValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetFloat(Object target, string field, float value)
    {
        if (target == null) return;
        SerializedObject so = new SerializedObject(target);
        SerializedProperty p = so.FindProperty(field);
        if (p == null) { Debug.LogWarning("[绘本] 字段不存在: " + target.GetType().Name + "." + field); return; }
        p.floatValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetEnum(Object target, string field, string enumName)
    {
        if (target == null) return;
        SerializedObject so = new SerializedObject(target);
        SerializedProperty p = so.FindProperty(field);
        if (p == null) { Debug.LogWarning("[绘本] 字段不存在: " + target.GetType().Name + "." + field); return; }

        System.Type fieldType = target.GetType().GetField(field,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).FieldType;
        object value = System.Enum.Parse(fieldType, enumName);
        p.intValue = System.Convert.ToInt32(value);
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetArray(Object target, string field, Object[] values)
    {
        if (target == null) return;
        SerializedObject so = new SerializedObject(target);
        SerializedProperty p = so.FindProperty(field);
        if (p == null) { Debug.LogWarning("[绘本] 字段不存在: " + target.GetType().Name + "." + field); return; }
        p.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++)
            p.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void UpdateBuildSettings()
    {
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();
        foreach (string p in PageNames)
        {
            string path = ScenesDir + "/" + p + ".unity";
            if (File.Exists(path)) scenes.Add(new EditorBuildSettingsScene(path, true));
        }
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
