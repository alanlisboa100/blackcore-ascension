using ROIO;
using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class Minimap : MonoBehaviour {

    [SerializeField] private RawImage PlayerIndicator;
    
    private Texture2D MapThumbTexture;
    private Texture2D PlayerIndicatorTexture;

    private RawImage MapThumb;
    private string CurrentMap;
    private int CurrentZoom = 1;
    private TextMeshProUGUI RegionLabel;

    // Start is called before the first frame update
    async void Start() {
        MapThumb = GetComponent<RawImage>();
        CreateRegionLabel();

        PlayerIndicatorTexture = await Addressables.LoadAssetAsync<Texture2D>($"{DBManager.INTERFACE_PATH}map/map_arrow.png").Task;
        Session.OnMapChanged += OnMapChanged;
    }

    private void OnDestroy() {
        Session.OnMapChanged -= OnMapChanged;
    }

    private async void OnMapChanged(string mapName) {
        CurrentMap = Path.GetFileNameWithoutExtension(mapName);
        if (RegionLabel != null) {
            RegionLabel.text = BlackCoreLoreService.ResolveMapName(CurrentMap);
        }
        MapThumbTexture = await Addressables.LoadAssetAsync<Texture2D>($"{DBManager.INTERFACE_PATH}map/{CurrentMap}.png").Task;

        if (MapThumbTexture == null) {
            return;
        }

        MapThumb.texture = MapThumbTexture;
        var size = CalculateNewSize(MapThumbTexture.width, MapThumbTexture.height, 128, 128);
        (transform as RectTransform).sizeDelta = size;
    }

    private void Update() {
        if (CurrentMap != null && MapThumbTexture == null) {
            OnMapChanged(CurrentMap);
        }
    }


    private void CreateRegionLabel() {
        var labelObject = new GameObject("BlackCoreRegionLabel", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        labelObject.transform.SetParent(transform, false);

        var rect = labelObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(0f, 6f);
        rect.sizeDelta = new Vector2(0f, 26f);

        RegionLabel = labelObject.GetComponent<TextMeshProUGUI>();
        RegionLabel.alignment = TextAlignmentOptions.Center;
        RegionLabel.fontSize = 15f;
        RegionLabel.fontStyle = FontStyles.Bold;
        RegionLabel.color = new Color(0.72f, 0.96f, 1f, 0.98f);
        RegionLabel.outlineColor = new Color(0.02f, 0.03f, 0.08f, 0.95f);
        RegionLabel.outlineWidth = 0.18f;
        RegionLabel.raycastTarget = false;
    }

    private Vector2 CalculateNewSize(int srcWidth, int srcHeight, int maxWidth, int maxHeight) {
        var ratio = Mathf.Min((float) maxWidth / (float) srcWidth, (float) maxHeight / (float) srcHeight);
        return new Vector2(srcWidth * ratio, srcHeight * ratio);
    }

}
