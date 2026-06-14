// ============================================================================
//  BuildSizeOptimizer.cs
//  Unity 6.3 (6000.3) — WebGL / 2D build-size optimizer (Editor tool)
//
//  ⚠ PLACEMENT: put this file inside ANY folder named "Editor"
//     e.g. Assets/Editor/BuildSizeOptimizer.cs
//
//  OPEN: menu  ->  Tools / Build Size Optimizer
//
//  What it does (one click):
//    • Player Settings: IL2CPP "Optimize Size", Managed Stripping "High",
//      .NET Standard 2.1, strip engine + unused mesh components.
//    • WebGL: compression (Brotli/Gzip + optional decompression fallback),
//      data caching, exceptions off, debug symbols off, Disk-Size-LTO.
//    • Texture import: per-asset WebGL override, crunch compression, max size.
//    • Audio import: per-asset WebGL override, Vorbis, quality, mono, load type.
//    • Audit: lists heavy assets the tool CANNOT auto-fix (Spine .json, SDF
//      fonts, oversized textures) so you can handle them manually.
//
//  APIs verified against Unity 6.3 LTS (6000.3) docs.
// ============================================================================

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

public class BuildSizeOptimizer : EditorWindow
{
    // ---- Player / engine ---------------------------------------------------
    private bool _optIl2cpp        = true;
    private bool _optStripping     = true;
    private bool _optNetStandard   = true;
    private bool _optStripEngine   = true;
    private bool _optStripMesh     = true;
    private bool _optDiskSizeLto   = true;
    private bool _optWasm2023      = false; // smaller/faster but newer-browser only

    // ---- WebGL publishing --------------------------------------------------
    private enum CompressionChoice
    {
        BrotliWithFallback,   // smallest files, works on any host (bigger loader)
        BrotliServerConfig,   // smallest files, needs https + server headers
        GzipWithFallback,     // bigger than brotli, works anywhere
        GzipServerConfig      // needs server headers
    }
    private CompressionChoice _compression = CompressionChoice.BrotliWithFallback;
    private bool _dataCaching      = true;
    private bool _exceptionsOff    = true;   // None = smallest, but any exception kills the app
    private bool _debugSymbolsOff  = true;

    // ---- Textures ----------------------------------------------------------
    private bool _doTextures       = true;
    private int  _texMaxSize       = 1024;
    private bool _texCrunch        = true;
    private int  _texQuality       = 50;     // crunch quality 0..100

    // ---- Audio -------------------------------------------------------------
    private bool _doAudio          = true;
    [Range(0f, 1f)] private float _audioQuality = 0.45f;
    private bool _audioForceMono   = false;
    private AudioClipLoadType _audioLoadType = AudioClipLoadType.CompressedInMemory;

    private Vector2 _scroll;
    private const string Tag = "[BuildSizeOptimizer]";
    private static readonly NamedBuildTarget WebGL = NamedBuildTarget.WebGL;

    [MenuItem("Tools/Build Size Optimizer")]
    public static void Open()
    {
        var w = GetWindow<BuildSizeOptimizer>("Build Size Optimizer");
        w.minSize = new Vector2(420, 560);
    }

    private void OnGUI()
    {
        _scroll = EditorGUILayout.BeginScrollView(_scroll);

        EditorGUILayout.HelpBox(
            "Unity 6.3 • WebGL • 2D — minimise build size.\n" +
            "Tick what you want, then Apply. Re-importing assets can take a while.",
            MessageType.Info);

        // ---------------------------------------------------------------
        Header("Player / Engine (code-side wins — biggest impact)");
        _optIl2cpp      = EditorGUILayout.ToggleLeft("IL2CPP code generation: Optimize Size", _optIl2cpp);
        _optStripping   = EditorGUILayout.ToggleLeft("Managed stripping level: High", _optStripping);
        _optNetStandard = EditorGUILayout.ToggleLeft("API compatibility: .NET Standard 2.1", _optNetStandard);
        _optStripEngine = EditorGUILayout.ToggleLeft("Strip engine code", _optStripEngine);
        _optStripMesh   = EditorGUILayout.ToggleLeft("Strip unused mesh components", _optStripMesh);
        _optDiskSizeLto = EditorGUILayout.ToggleLeft("Code optimization: Disk Size + LTO", _optDiskSizeLto);
        _optWasm2023    = EditorGUILayout.ToggleLeft("WebAssembly 2023 (newer browsers only)", _optWasm2023);

        // ---------------------------------------------------------------
        Header("WebGL Publishing");
        _compression = (CompressionChoice)EditorGUILayout.EnumPopup("Compression", _compression);
        if (_compression == CompressionChoice.BrotliServerConfig ||
            _compression == CompressionChoice.GzipServerConfig)
        {
            EditorGUILayout.HelpBox(
                "Without decompression fallback you MUST serve the build with the correct " +
                "Content-Encoding header (https for Brotli). On itch.io / GitHub Pages use a *WithFallback* option instead.",
                MessageType.Warning);
        }
        _dataCaching     = EditorGUILayout.ToggleLeft("Data caching (IndexedDB)", _dataCaching);
        _debugSymbolsOff = EditorGUILayout.ToggleLeft("Debug symbols: Off", _debugSymbolsOff);
        _exceptionsOff   = EditorGUILayout.ToggleLeft("Exceptions: None (smallest build)", _exceptionsOff);
        if (_exceptionsOff)
        {
            EditorGUILayout.HelpBox(
                "Exceptions = None gives the smallest, fastest build, but ANY thrown exception " +
                "stops the game with an error. Test thoroughly before shipping.",
                MessageType.Warning);
        }

        // ---------------------------------------------------------------
        Header("Textures (WebGL override)");
        _doTextures = EditorGUILayout.ToggleLeft("Optimise all textures", _doTextures);
        using (new EditorGUI.DisabledScope(!_doTextures))
        {
            _texMaxSize = EditorGUILayout.IntPopup("Max size", _texMaxSize,
                new[] { "256", "512", "1024", "2048" },
                new[] { 256, 512, 1024, 2048 });
            _texCrunch  = EditorGUILayout.ToggleLeft("Crunch compression (smallest download)", _texCrunch);
            _texQuality = EditorGUILayout.IntSlider("Crunch quality", _texQuality, 0, 100);
            EditorGUILayout.HelpBox(
                "Crunch shrinks download a lot but is lossy — check sharp UI / pixel art after applying.",
                MessageType.None);
        }

        // ---------------------------------------------------------------
        Header("Audio (WebGL override)");
        _doAudio = EditorGUILayout.ToggleLeft("Optimise all audio clips", _doAudio);
        using (new EditorGUI.DisabledScope(!_doAudio))
        {
            _audioLoadType  = (AudioClipLoadType)EditorGUILayout.EnumPopup("Load type", _audioLoadType);
            _audioQuality   = EditorGUILayout.Slider("Vorbis quality", _audioQuality, 0f, 1f);
            _audioForceMono = EditorGUILayout.ToggleLeft("Force to mono", _audioForceMono);
        }

        EditorGUILayout.Space(10);

        // ---------------------------------------------------------------
        using (new EditorGUILayout.HorizontalScope())
        {
            GUI.backgroundColor = new Color(0.6f, 0.9f, 0.6f);
            if (GUILayout.Button("Apply Optimizations", GUILayout.Height(38)))
                ApplyAll();
            GUI.backgroundColor = Color.white;

            if (GUILayout.Button("Audit Assets", GUILayout.Height(38), GUILayout.Width(120)))
                AuditAssets();
        }

        EditorGUILayout.EndScrollView();
    }

    private static void Header(string t)
    {
        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField(t, EditorStyles.boldLabel);
    }

    // =======================================================================
    //  APPLY
    // =======================================================================
    private void ApplyAll()
    {
        try
        {
            EditorUtility.DisplayProgressBar(Tag, "Applying player settings…", 0.1f);
            ApplyPlayerSettings();

            if (_doTextures)
            {
                EditorUtility.DisplayProgressBar(Tag, "Optimising textures…", 0.4f);
                int n = OptimiseTextures();
                Debug.Log($"{Tag} Textures updated: {n}");
            }

            if (_doAudio)
            {
                EditorUtility.DisplayProgressBar(Tag, "Optimising audio…", 0.7f);
                int n = OptimiseAudio();
                Debug.Log($"{Tag} Audio clips updated: {n}");
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"{Tag} ✅ Done. Build a Release WebGL build to see the new size.");
        }
        catch (Exception e)
        {
            Debug.LogError($"{Tag} Failed: {e}");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    private void ApplyPlayerSettings()
    {
        if (_optIl2cpp)
            PlayerSettings.SetIl2CppCodeGeneration(WebGL, Il2CppCodeGeneration.OptimizeSize);

        if (_optStripping)
            PlayerSettings.SetManagedStrippingLevel(WebGL, ManagedStrippingLevel.High);

        if (_optNetStandard)
            PlayerSettings.SetApiCompatibilityLevel(WebGL, ApiCompatibilityLevel.NET_Standard);

        if (_optStripEngine)
            PlayerSettings.stripEngineCode = true;

        if (_optStripMesh)
            PlayerSettings.stripUnusedMeshComponents = true;

        // ---- WebGL block ----
        PlayerSettings.WebGL.dataCaching     = _dataCaching;
        PlayerSettings.WebGL.wasm2023        = _optWasm2023;

        if (_debugSymbolsOff)
            PlayerSettings.WebGL.debugSymbolMode = WebGLDebugSymbolMode.Off;

        PlayerSettings.WebGL.exceptionSupport = _exceptionsOff
            ? WebGLExceptionSupport.None
            : WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly;

        switch (_compression)
        {
            case CompressionChoice.BrotliWithFallback:
                PlayerSettings.WebGL.compressionFormat   = WebGLCompressionFormat.Brotli;
                PlayerSettings.WebGL.decompressionFallback = true;
                break;
            case CompressionChoice.BrotliServerConfig:
                PlayerSettings.WebGL.compressionFormat   = WebGLCompressionFormat.Brotli;
                PlayerSettings.WebGL.decompressionFallback = false;
                break;
            case CompressionChoice.GzipWithFallback:
                PlayerSettings.WebGL.compressionFormat   = WebGLCompressionFormat.Gzip;
                PlayerSettings.WebGL.decompressionFallback = true;
                break;
            case CompressionChoice.GzipServerConfig:
                PlayerSettings.WebGL.compressionFormat   = WebGLCompressionFormat.Gzip;
                PlayerSettings.WebGL.decompressionFallback = false;
                break;
        }

        // Code optimization "Disk Size (LTO)" — documented 6.3 string API,
        // avoids referencing the WebGL editor extension assembly.
        if (_optDiskSizeLto)
        {
            try
            {
                EditorUserBuildSettings.SetPlatformSettings(
                    WebGL.TargetName, "CodeOptimization", "disksizelto");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"{Tag} Could not set Disk-Size-LTO via SetPlatformSettings " +
                                 $"(set it manually in Build Settings > Web > Web Settings). {e.Message}");
            }
        }

        // Splash screen removal requires Unity Pro/Plus — wrap to avoid throwing.
        try { PlayerSettings.SplashScreen.show = false; }
        catch { Debug.LogWarning($"{Tag} Splash screen toggle skipped (needs Pro/Plus license)."); }

        Debug.Log($"{Tag} Player settings applied.");
    }

    // =======================================================================
    //  TEXTURES
    // =======================================================================
    private int OptimiseTextures()
    {
        int count = 0;
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets" });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.StartsWith("Packages/")) continue;
            if (AssetImporter.GetAtPath(path) is not TextureImporter ti) continue;

            var ps = ti.GetPlatformTextureSettings("WebGL");
            ps.overridden     = true;
            ps.maxTextureSize = _texMaxSize;

            if (_texCrunch)
            {
                bool hasAlpha = ti.DoesSourceTextureHaveAlpha();
                ps.format             = hasAlpha
                    ? TextureImporterFormat.DXT5Crunched
                    : TextureImporterFormat.DXT1Crunched;
                ps.crunchedCompression = true;
                ps.compressionQuality  = _texQuality;
            }
            else
            {
                ps.format              = TextureImporterFormat.Automatic;
                ps.textureCompression  = TextureImporterCompression.Compressed;
            }

            ti.SetPlatformTextureSettings(ps);
            ti.SaveAndReimport();
            count++;
        }
        return count;
    }

    // =======================================================================
    //  AUDIO
    // =======================================================================
    private int OptimiseAudio()
    {
        int count = 0;
        string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets" });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.StartsWith("Packages/")) continue;
            if (AssetImporter.GetAtPath(path) is not AudioImporter ai) continue;

            var s = ai.GetOverrideSampleSettings("WebGL");
            s.loadType          = _audioLoadType;
            s.compressionFormat = AudioCompressionFormat.Vorbis;
            s.quality           = _audioQuality;
            s.sampleRateSetting = AudioSampleRateSetting.OptimizeSampleRate;

            ai.SetOverrideSampleSettings("WebGL", s);
            ai.forceToMono = _audioForceMono;
            ai.SaveAndReimport();
            count++;
        }
        return count;
    }

    // =======================================================================
    //  AUDIT — flags heavy assets the tool can't auto-fix
    // =======================================================================
    private void AuditAssets()
    {
        var rows = new List<(long size, string path, string note)>();

        foreach (string file in Directory.GetFiles("Assets", "*.*", SearchOption.AllDirectories))
        {
            string ext = Path.GetExtension(file).ToLowerInvariant();
            if (ext == ".meta" || ext == ".cs") continue;

            long size = new FileInfo(file).Length;
            if (size < 50 * 1024) continue; // ignore < 50 KB

            string note = ext switch
            {
                ".json" when file.IndexOf("skel", StringComparison.OrdinalIgnoreCase) >= 0
                          || file.IndexOf("cat",  StringComparison.OrdinalIgnoreCase) >= 0
                    => "Spine skeleton? Re-export from Spine as BINARY (.skel) — usually 50-70% smaller.",
                ".json" => "Large JSON — consider binary format or trimming.",
                ".mp3" or ".wav" or ".ogg"
                    => "Audio — covered by the Audio optimiser (Vorbis + lower quality + mono).",
                ".asset" when file.IndexOf("SDF", StringComparison.OrdinalIgnoreCase) >= 0
                    => "TMP SDF font atlas — regenerate with ONLY the glyphs you use (static atlas).",
                ".png" or ".jpg" or ".jpeg" or ".tga" or ".psd"
                    => "Texture — covered by the Texture optimiser (max size + crunch).",
                _ => "Large asset — review if it ships in the build."
            };

            rows.Add((size, file.Replace('\\', '/'), note));
        }

        rows.Sort((a, b) => b.size.CompareTo(a.size));

        Debug.Log($"{Tag} ===== AUDIT: heaviest source assets (>50 KB) =====");
        foreach (var r in rows.Take(30))
            Debug.Log($"{Tag} {r.size / 1024f,8:0.0} KB  {r.path}\n        ↳ {r.note}");

        Debug.Log($"{Tag} ===== Manual actions the tool can't do =====\n" +
                  "• Spine skeletons: export as .skel (binary), not .json — biggest win in your report (~2 MB).\n" +
                  "• TMP SDF fonts: rebuild atlas with only used characters.\n" +
                  "• Remove unused packages from Packages/manifest.json (Input System / Burst / Collections etc. if unused).\n" +
                  "• Always build in RELEASE (not Development) so stripping + compression actually run.");
    }
}
#endif
