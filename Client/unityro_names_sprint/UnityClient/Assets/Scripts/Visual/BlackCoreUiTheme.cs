using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Low-cost runtime theme used by the mobile HUD and new Black Core UI.
/// The palette intentionally avoids recoloring legacy Ragnarok textures; new
/// screens can opt-in one component at a time while the art pipeline is replaced.
/// </summary>
public static class BlackCoreUiTheme {
    public static readonly Color Panel = new Color32(10, 12, 20, 226);
    public static readonly Color PanelSoft = new Color32(18, 21, 34, 208);
    public static readonly Color Accent = new Color32(76, 225, 255, 255);
    public static readonly Color AccentAlt = new Color32(151, 86, 255, 255);
    public static readonly Color AccentPressed = new Color32(43, 149, 184, 255);
    public static readonly Color TextPrimary = new Color32(239, 247, 255, 255);
    public static readonly Color TextMuted = new Color32(160, 177, 199, 255);
    public static readonly Color Danger = new Color32(255, 82, 115, 255);
    public static readonly Color Critical = new Color32(255, 203, 76, 255);

    public static void StyleButton(Button button, bool primary = false) {
        if (button == null) return;

        var image = button.GetComponent<Image>();
        if (image != null) {
            image.color = primary
                ? new Color(AccentAlt.r, AccentAlt.g, AccentAlt.b, 0.72f)
                : PanelSoft;
        }

        var colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.08f, 1.08f, 1.08f, 1f);
        colors.pressedColor = new Color(0.72f, 0.86f, 1f, 1f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(0.5f, 0.5f, 0.58f, 0.42f);
        colors.fadeDuration = 0.08f;
        button.colors = colors;

        var label = button.GetComponentInChildren<Text>();
        if (label != null) {
            label.color = TextPrimary;
            var shadow = label.GetComponent<Shadow>();
            if (shadow == null) shadow = label.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.75f);
            shadow.effectDistance = new Vector2(1.5f, -1.5f);
        }

        var outline = button.GetComponent<Outline>();
        if (outline == null) outline = button.gameObject.AddComponent<Outline>();
        outline.effectColor = primary
            ? new Color(Accent.r, Accent.g, Accent.b, 0.72f)
            : new Color(AccentAlt.r, AccentAlt.g, AccentAlt.b, 0.42f);
        outline.effectDistance = new Vector2(1.5f, -1.5f);
    }
}
