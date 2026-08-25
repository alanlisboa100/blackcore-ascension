using System;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Conservative material/animation polish for legacy environment models. It uses
/// texture-name heuristics so existing maps immediately gain richer grass,
/// foliage, architecture and magical props without changing map files.
/// </summary>
public static class BlackCoreEnvironmentPolish {
    public static void Apply(MeshRenderer renderer, string texturePath) {
        if (renderer == null) return;

        string lower = (texturePath ?? string.Empty).ToLowerInvariant();
        if (lower.Length == 0 && renderer.sharedMaterial != null) {
            lower = (renderer.sharedMaterial.name ?? string.Empty).ToLowerInvariant();
        }

        renderer.receiveShadows = true;
        renderer.shadowCastingMode = ShadowCastingMode.On;

        Color tint = Color.white;
        bool tintChanged = false;

        if (ContainsAny(lower, "grass", "leaf", "plant", "bush", "flower", "tree", "vine", "forest")) {
            tint = new Color(0.93f, 1.06f, 0.94f, 1f);
            tintChanged = true;
            if (renderer.GetComponent<BlackCoreVegetationSway>() == null && renderer.GetComponent<NodeAnimation>() == null) {
                var sway = renderer.gameObject.AddComponent<BlackCoreVegetationSway>();
                sway.Configure(lower.Contains("grass") ? 1.20f : 0.55f);
            }
        } else if (ContainsAny(lower, "house", "roof", "wall", "wood", "door", "inn", "home", "window")) {
            tint = new Color(1.04f, 0.995f, 0.94f, 1f);
            tintChanged = true;
        } else if (ContainsAny(lower, "stone", "rock", "castle", "brick", "road", "ground", "tile")) {
            tint = new Color(0.97f, 0.99f, 1.035f, 1f);
            tintChanged = true;
        } else if (ContainsAny(lower, "crystal", "portal", "torch", "lamp", "fire", "lava", "rune")) {
            tint = new Color(1.055f, 1.01f, 1.08f, 1f);
            tintChanged = true;
        }

        if (!tintChanged) return;

        var block = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(block);
        block.SetColor("_Color", tint);
        block.SetColor("_EmissionColor", new Color(
            Mathf.Max(0f, tint.r - 0.90f),
            Mathf.Max(0f, tint.g - 0.90f),
            Mathf.Max(0f, tint.b - 0.90f),
            1f));
        renderer.SetPropertyBlock(block);
    }

    private static bool ContainsAny(string source, params string[] values) {
        foreach (var value in values) {
            if (source.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0) return true;
        }
        return false;
    }
}
