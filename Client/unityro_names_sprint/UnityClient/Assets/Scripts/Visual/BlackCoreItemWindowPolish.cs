using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Conservative visual polish for item, equipment and skill grids. It only
/// touches structural UI images by name so item/skill icons are left intact.
/// </summary>
public static class BlackCoreItemWindowPolish {
    public static void Apply(GameObject root) {
        if (root == null) return;

        foreach (var image in root.GetComponentsInChildren<Image>(true)) {
            if (image == null) continue;
            string name = image.gameObject.name.ToLowerInvariant();

            if (ContainsAny(name, "cell", "slot")) {
                image.color = new Color(0.075f, 0.10f, 0.17f, 0.94f);
                AddOutline(image.gameObject, new Color(0.28f, 0.50f, 0.70f, 0.34f));
            } else if (ContainsAny(name, "background", "panel", "window", "grid")) {
                image.color = Color.Lerp(image.color, new Color(0.06f, 0.08f, 0.14f, image.color.a), 0.38f);
            } else if (ContainsAny(name, "tab")) {
                AddOutline(image.gameObject, new Color(0.40f, 0.91f, 1f, 0.26f));
            }
        }

        foreach (var raw in root.GetComponentsInChildren<RawImage>(true)) {
            if (raw == null) continue;
            string name = raw.gameObject.name.ToLowerInvariant();
            if (ContainsAny(name, "container", "slot", "background")) {
                raw.color = Color.Lerp(raw.color, new Color(0.12f, 0.16f, 0.25f, raw.color.a), 0.30f);
            }
        }
    }

    private static void AddOutline(GameObject go, Color color) {
        if (go.GetComponent<Outline>() != null) return;
        var outline = go.AddComponent<Outline>();
        outline.effectColor = color;
        outline.effectDistance = new Vector2(1f, -1f);
    }

    private static bool ContainsAny(string source, params string[] values) {
        foreach (var value in values) {
            if (source.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0) return true;
        }
        return false;
    }
}
