using UnityEngine;
using UnityEngine.UI;
using UnityRO.GameCamera;

/// <summary>
/// Central combat feedback layer: subtle camera impulse and screen flash for
/// player-relevant hits. It intentionally avoids Time.timeScale hit-stop because
/// networked movement and packet playback should never depend on client timescale.
/// </summary>
public class BlackCoreCombatFeedback : MonoBehaviour {
    public static BlackCoreCombatFeedback Instance { get; private set; }

    private Image FlashImage;
    private float FlashAlpha;
    private Color FlashColor = Color.white;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(this);
            return;
        }

        Instance = this;
        BuildFlashOverlay();
    }

    private void OnDestroy() {
        if (Instance == this) Instance = null;
    }

    private void Update() {
        if (FlashImage == null || FlashAlpha <= 0f) return;

        FlashAlpha = Mathf.MoveTowards(FlashAlpha, 0f, Time.unscaledDeltaTime * 1.9f);
        FlashImage.color = new Color(FlashColor.r, FlashColor.g, FlashColor.b, FlashAlpha);
    }

    public static void ReportHit(Entity source, Entity target, float damage, bool critical) {
        if (Instance == null || damage <= 0f) return;

        var local = Session.CurrentSession?.Entity as Entity;
        bool localIsSource = local != null && source != null && source.GID == local.GID;
        bool localIsTarget = local != null && target != null && target.GID == local.GID;
        if (!localIsSource && !localIsTarget) return;

        float trauma = critical ? 0.72f : (localIsTarget ? 0.38f : 0.22f);
        CharacterCamera.ROCamera?.AddTrauma(trauma);

        Instance.FlashColor = critical
            ? BlackCoreUiTheme.Critical
            : (localIsTarget ? BlackCoreUiTheme.Danger : BlackCoreUiTheme.Accent);
        Instance.FlashAlpha = Mathf.Max(Instance.FlashAlpha, critical ? 0.11f : 0.045f);
    }

    private void BuildFlashOverlay() {
        var canvasObject = new GameObject("BlackCore_CombatFlash", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);

        var canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32000;

        var imageObject = new GameObject("Flash", typeof(RectTransform), typeof(Image));
        imageObject.transform.SetParent(canvasObject.transform, false);
        var rect = imageObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        FlashImage = imageObject.GetComponent<Image>();
        FlashImage.raycastTarget = false;
        FlashImage.color = Color.clear;
    }
}
