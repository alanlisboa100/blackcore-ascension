using System;
using Assets.Scripts.Renderer.Map;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Creates a lightweight URP cinematic layer at runtime so visual tuning can be
/// iterated without editing every legacy scene. The values are intentionally
/// subtle and mobile-friendly; artists can later replace them with authored
/// Volume Profiles per biome.
/// </summary>
public class BlackCoreVisualDirector : MonoBehaviour {
    private enum Mood {
        Neutral,
        City,
        Forest,
        Desert,
        Frost,
        Dark
    }

    public static BlackCoreVisualDirector Instance { get; private set; }

    private Volume GlobalVolume;
    private VolumeProfile Profile;
    private Bloom Bloom;
    private ColorAdjustments ColorAdjustments;
    private Vignette Vignette;
    private Tonemapping Tonemapping;
    private FilmGrain FilmGrain;
    private Camera LastCamera;
    private string CurrentMapName;
    private GameObject BrandOverlay;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(this);
            return;
        }

        Instance = this;
        BuildVolume();
        BuildBrandOverlay();
        ApplyMood(Mood.Neutral);
        UpdateBrandOverlay(SceneManager.GetActiveScene());
    }

    private void OnEnable() {
        SceneManager.activeSceneChanged += OnSceneChanged;
    }

    private void OnDisable() {
        SceneManager.activeSceneChanged -= OnSceneChanged;
    }

    private void OnDestroy() {
        if (Profile != null) Destroy(Profile);
        if (Instance == this) Instance = null;
    }

    private void LateUpdate() {
        EnsureCameraPostProcessing();
    }

    public void ApplyMapMood(string mapName) {
        CurrentMapName = mapName ?? string.Empty;
        ApplyMood(DetectMood(CurrentMapName));
    }

    private void OnSceneChanged(Scene oldScene, Scene newScene) {
        LastCamera = null;
        UpdateBrandOverlay(newScene);
        if (newScene.name.IndexOf("Map", StringComparison.OrdinalIgnoreCase) < 0) {
            RenderSettings.fog = false;
            ApplyMood(Mood.Neutral);
        } else if (!string.IsNullOrWhiteSpace(CurrentMapName)) {
            ApplyMood(DetectMood(CurrentMapName));
        }
    }


    private void BuildBrandOverlay() {
        var canvasObject = new GameObject("BlackCore_BrandOverlay", typeof(Canvas), typeof(CanvasScaler));
        canvasObject.transform.SetParent(transform, false);
        var canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 30000;

        var panelObject = new GameObject("Brand", typeof(RectTransform), typeof(Image));
        panelObject.transform.SetParent(canvasObject.transform, false);
        var rect = panelObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 0f);
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(1f, 0f);
        rect.anchoredPosition = new Vector2(-18f, 18f);
        rect.sizeDelta = new Vector2(280f, 54f);
        panelObject.GetComponent<Image>().color = new Color(BlackCoreUiTheme.Panel.r, BlackCoreUiTheme.Panel.g, BlackCoreUiTheme.Panel.b, 0.72f);

        var textObject = new GameObject("Title", typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(panelObject.transform, false);
        var textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(12f, 4f);
        textRect.offsetMax = new Vector2(-12f, -4f);
        var text = textObject.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = 16;
        text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.MiddleRight;
        text.color = BlackCoreUiTheme.TextPrimary;
        text.text = $"{BlackCoreBrand.GameName}\n<size=10>{BlackCoreBrand.StudioName}</size>";

        var outline = panelObject.AddComponent<Outline>();
        outline.effectColor = new Color(BlackCoreUiTheme.AccentAlt.r, BlackCoreUiTheme.AccentAlt.g, BlackCoreUiTheme.AccentAlt.b, 0.38f);
        outline.effectDistance = new Vector2(1f, -1f);
        BrandOverlay = canvasObject;
    }

    private void UpdateBrandOverlay(Scene scene) {
        if (BrandOverlay == null) return;
        bool isMap = scene.name.IndexOf("Map", StringComparison.OrdinalIgnoreCase) >= 0;
        bool isLoading = scene.name.IndexOf("Loading", StringComparison.OrdinalIgnoreCase) >= 0;
        BrandOverlay.SetActive(!isMap && !isLoading);
    }

    private void BuildVolume() {
        var volumeObject = new GameObject("BlackCore_CinematicVolume");
        volumeObject.transform.SetParent(transform, false);

        GlobalVolume = volumeObject.AddComponent<Volume>();
        GlobalVolume.isGlobal = true;
        GlobalVolume.priority = 100f;

        Profile = ScriptableObject.CreateInstance<VolumeProfile>();
        Profile.name = "Black Core Runtime Cinematic";
        GlobalVolume.sharedProfile = Profile;

        Bloom = Profile.Add<Bloom>(true);
        ColorAdjustments = Profile.Add<ColorAdjustments>(true);
        Vignette = Profile.Add<Vignette>(true);
        Tonemapping = Profile.Add<Tonemapping>(true);
        FilmGrain = Profile.Add<FilmGrain>(true);

        Tonemapping.mode.Override(TonemappingMode.ACES);

        Bloom.threshold.Override(1.05f);
        Bloom.scatter.Override(0.62f);
        Bloom.intensity.Override(Application.isMobilePlatform ? 0.18f : 0.28f);

        Vignette.smoothness.Override(0.58f);
        Vignette.rounded.Override(true);

        // Film grain helps de-band gradients on desktop, but is deliberately
        // disabled on mobile where every fullscreen pass matters.
        FilmGrain.active = !Application.isMobilePlatform;
        FilmGrain.type.Override(FilmGrainLookup.Thin1);
        FilmGrain.intensity.Override(0.045f);
        FilmGrain.response.Override(0.72f);
    }

    private void EnsureCameraPostProcessing() {
        var camera = Camera.main;
        if (camera == null || camera == LastCamera) return;

        LastCamera = camera;
        var data = camera.GetUniversalAdditionalCameraData();
        data.renderPostProcessing = true;
        data.antialiasing = Application.isMobilePlatform
            ? AntialiasingMode.FastApproximateAntialiasing
            : AntialiasingMode.SubpixelMorphologicalAntiAliasing;
    }

    private static Mood DetectMood(string mapName) {
        var name = (mapName ?? string.Empty).ToLowerInvariant();

        if (ContainsAny(name, "dun", "cave", "maze", "crypt", "tower", "glast", "nifl", "abbey")) return Mood.Dark;
        if (ContainsAny(name, "snow", "xmas", "lutie", "ice", "frost")) return Mood.Frost;
        if (ContainsAny(name, "moc", "desert", "sograt", "sand", "veins")) return Mood.Desert;
        if (ContainsAny(name, "pay", "forest", "wood", "fild", "field", "gef_f")) return Mood.Forest;
        if (ContainsAny(name, "prontera", "alberta", "geffen", "payon", "morocc", "izlude", "aldebaran", "city", "town")) return Mood.City;
        return Mood.Neutral;
    }

    private static bool ContainsAny(string source, params string[] values) {
        foreach (var value in values) {
            if (source.Contains(value)) return true;
        }
        return false;
    }

    private void ApplyMood(Mood mood) {
        if (ColorAdjustments == null) return;

        Color filter;
        float contrast;
        float saturation;
        float exposure;
        float vignette;
        bool fog;
        Color fogColor;
        float fogDensity;

        switch (mood) {
            case Mood.City:
                filter = new Color(1.04f, 1.01f, 0.94f, 1f);
                contrast = 8f;
                saturation = 5f;
                exposure = 0.04f;
                vignette = 0.13f;
                fog = true;
                fogColor = new Color(0.37f, 0.43f, 0.52f);
                fogDensity = 0.0015f;
                break;
            case Mood.Forest:
                filter = new Color(0.95f, 1.05f, 0.97f, 1f);
                contrast = 9f;
                saturation = 9f;
                exposure = 0f;
                vignette = 0.16f;
                fog = true;
                fogColor = new Color(0.20f, 0.31f, 0.28f);
                fogDensity = 0.0024f;
                break;
            case Mood.Desert:
                filter = new Color(1.08f, 1.01f, 0.88f, 1f);
                contrast = 10f;
                saturation = 7f;
                exposure = 0.06f;
                vignette = 0.14f;
                fog = true;
                fogColor = new Color(0.58f, 0.44f, 0.29f);
                fogDensity = 0.0018f;
                break;
            case Mood.Frost:
                filter = new Color(0.90f, 0.98f, 1.10f, 1f);
                contrast = 11f;
                saturation = -3f;
                exposure = 0.05f;
                vignette = 0.17f;
                fog = true;
                fogColor = new Color(0.55f, 0.67f, 0.78f);
                fogDensity = 0.0022f;
                break;
            case Mood.Dark:
                filter = new Color(0.88f, 0.92f, 1.08f, 1f);
                contrast = 14f;
                saturation = -5f;
                exposure = -0.08f;
                vignette = 0.23f;
                fog = true;
                fogColor = new Color(0.035f, 0.045f, 0.075f);
                fogDensity = 0.0045f;
                break;
            default:
                filter = new Color(0.98f, 1.0f, 1.04f, 1f);
                contrast = 7f;
                saturation = 4f;
                exposure = 0.02f;
                vignette = 0.12f;
                fog = false;
                fogColor = Color.gray;
                fogDensity = 0.001f;
                break;
        }

        ColorAdjustments.colorFilter.Override(filter);
        ColorAdjustments.contrast.Override(contrast);
        ColorAdjustments.saturation.Override(saturation);
        ColorAdjustments.postExposure.Override(exposure);
        Vignette.intensity.Override(vignette);
        Vignette.color.Override(new Color(0.025f, 0.02f, 0.055f));

        RenderSettings.fog = fog;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogDensity = Application.isMobilePlatform ? fogDensity * 0.78f : fogDensity;
        RenderSettings.reflectionIntensity = 0.82f;
        ApplyEnvironmentAccents(mood);
    }

    private void ApplyEnvironmentAccents(Mood mood) {
        Color waterTint;
        switch (mood) {
            case Mood.Desert:
                waterTint = new Color(0.58f, 0.86f, 0.92f, 0.86f);
                break;
            case Mood.Frost:
                waterTint = new Color(0.62f, 0.82f, 1.0f, 0.88f);
                break;
            case Mood.Dark:
                waterTint = new Color(0.34f, 0.50f, 0.72f, 0.82f);
                break;
            case Mood.Forest:
                waterTint = new Color(0.42f, 0.76f, 0.72f, 0.86f);
                break;
            default:
                waterTint = new Color(0.55f, 0.78f, 0.92f, 0.86f);
                break;
        }

        foreach (var water in FindObjectsOfType<WaterRenderer>()) {
            var renderer = water.GetComponent<MeshRenderer>();
            if (renderer == null) continue;
            var material = renderer.material;
            if (material != null && material.HasProperty("_Color")) {
                material.SetColor("_Color", waterTint);
            }
        }
    }
}
