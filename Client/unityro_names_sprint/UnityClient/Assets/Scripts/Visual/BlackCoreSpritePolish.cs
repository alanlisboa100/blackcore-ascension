using System;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Lightweight presentation polish for sprite entities. This gives monsters and
/// weapons a more curated look without touching combat logic or server data.
/// </summary>
public static class BlackCoreSpritePolish {
    public static void Apply(Entity entity, MeshRenderer renderer, ViewerType viewerType) {
        if (entity == null || renderer == null) return;

        var block = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(block);
        bool changed = false;

        if (entity.Type == EntityType.MOB || entity.Type == EntityType.DISGUISED) {
            var monsterName = BlackCoreNameService.ResolveMonsterName(entity.Status.name);
            if (IsBoss(monsterName)) {
                block.SetColor("_Color", new Color(1.06f, 0.98f, 1.06f, 1f));
                renderer.sortingOrder += 2;
                changed = true;
            } else if (viewerType == ViewerType.BODY) {
                block.SetColor("_Color", new Color(1.03f, 1.03f, 1.03f, 1f));
                changed = true;
            }
        } else if (entity.Type == EntityType.PC && viewerType == ViewerType.WEAPON) {
            block.SetColor("_Color", new Color(0.94f, 1.02f, 1.08f, 1f));
            changed = true;
        }

        if (changed) {
            renderer.SetPropertyBlock(block);
        }

        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
    }

    private static bool IsBoss(string name) {
        if (string.IsNullOrWhiteSpace(name)) return false;
        var lower = name.ToLowerInvariant();
        return ContainsAny(lower, "senhor", "lorde", "rainha", "faraó", "guardião", "capitão", "cavaleiro", "abismo");
    }

    private static bool ContainsAny(string source, params string[] values) {
        foreach (var value in values) {
            if (source.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0) return true;
        }
        return false;
    }
}
