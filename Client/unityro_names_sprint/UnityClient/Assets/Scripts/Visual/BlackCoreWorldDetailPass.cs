using System;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Second-pass environment polish applied after an entire map is instantiated.
/// This lets us touch ground tiles, houses, trees and prop clusters that are
/// harder to classify during individual model creation.
/// </summary>
public static class BlackCoreWorldDetailPass {
    public static void Apply(GameObject mapRoot, string mapName) {
        if (mapRoot == null) return;

        mapRoot.name = BlackCoreLoreService.ResolveMapName(mapName);

        foreach (var renderer in mapRoot.GetComponentsInChildren<MeshRenderer>(true)) {
            string signature = BuildSignature(renderer);
            BlackCoreEnvironmentPolish.Apply(renderer, signature);
            ApplySecondaryShading(renderer, signature);
        }
    }

    private static void ApplySecondaryShading(MeshRenderer renderer, string signature) {
        if (renderer == null || string.IsNullOrWhiteSpace(signature)) return;

        renderer.receiveShadows = true;
        renderer.shadowCastingMode = ShadowCastingMode.On;

        var block = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(block);

        if (ContainsAny(signature, "tree", "leaf", "grass", "flower", "plant", "bush", "vine")) {
            block.SetColor("_Color", new Color(0.92f, 1.06f, 0.94f, 1f));
            renderer.SetPropertyBlock(block);
            return;
        }

        if (ContainsAny(signature, "roof", "house", "inn", "home", "wall", "window", "door", "wood", "tower")) {
            block.SetColor("_Color", new Color(1.04f, 1.00f, 0.93f, 1f));
            renderer.SetPropertyBlock(block);
            return;
        }

        if (ContainsAny(signature, "road", "ground", "tile", "soil", "sand", "brick", "stone", "rock")) {
            block.SetColor("_Color", new Color(0.98f, 0.99f, 1.02f, 1f));
            renderer.SetPropertyBlock(block);
            return;
        }

        if (ContainsAny(signature, "crystal", "portal", "lamp", "torch", "fire", "lava")) {
            block.SetColor("_Color", new Color(1.06f, 1.02f, 1.08f, 1f));
            renderer.SetPropertyBlock(block);
        }
    }

    private static string BuildSignature(MeshRenderer renderer) {
        string signature = renderer.gameObject.name ?? string.Empty;

        if (renderer.sharedMaterial != null) {
            signature += " " + renderer.sharedMaterial.name;
            if (renderer.sharedMaterial.mainTexture != null) {
                signature += " " + renderer.sharedMaterial.mainTexture.name;
            }
        }

        return signature.ToLowerInvariant();
    }

    private static bool ContainsAny(string source, params string[] values) {
        foreach (var value in values) {
            if (source.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0) {
                return true;
            }
        }

        return false;
    }
}
