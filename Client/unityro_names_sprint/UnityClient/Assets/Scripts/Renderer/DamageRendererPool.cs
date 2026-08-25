using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Reuses transient damage-number GameObjects to avoid Instantiate/Destroy churn
/// during dense combat. Scene unloads may destroy pooled objects, so Borrow skips
/// stale Unity references automatically.
/// </summary>
public static class DamageRendererPool {
    private const int MaxPoolSize = 128;
    private static readonly Stack<DamageRenderer> Pool = new Stack<DamageRenderer>(32);

    public static DamageRenderer Borrow(GameObject prefab) {
        while (Pool.Count > 0) {
            var renderer = Pool.Pop();
            if (renderer == null) {
                continue;
            }

            renderer.gameObject.SetActive(true);
            renderer.ResetForPool();
            return renderer;
        }

        var instance = Object.Instantiate(prefab).GetComponent<DamageRenderer>();
        instance.ResetForPool();
        return instance;
    }

    public static void Return(DamageRenderer renderer) {
        if (renderer == null) {
            return;
        }

        renderer.ResetForPool();

        if (Pool.Count >= MaxPoolSize) {
            Object.Destroy(renderer.gameObject);
            return;
        }

        renderer.gameObject.SetActive(false);
        Pool.Push(renderer);
    }
}
