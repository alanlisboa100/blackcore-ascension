using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// App-lifetime cache for small, frequently reused Addressables such as audio clips,
/// cursor resources and common VFX textures. Large per-map/entity sprite atlases should
/// keep explicit scoped ownership instead of using this cache.
/// </summary>
public static class AddressableAssetCache<T> where T : Object {
    private static readonly Dictionary<string, AsyncOperationHandle<T>> Handles = new Dictionary<string, AsyncOperationHandle<T>>();


    public static T LoadSync(string key) {
        if (Handles.TryGetValue(key, out var existing) && existing.IsValid()) {
            return existing.WaitForCompletion();
        }

        var handle = Addressables.LoadAssetAsync<T>(key);
        Handles[key] = handle;

        try {
            return handle.WaitForCompletion();
        } catch {
            if (handle.IsValid()) {
                Addressables.Release(handle);
            }
            Handles.Remove(key);
            throw;
        }
    }

    public static async Task<T> LoadAsync(string key) {
        if (Handles.TryGetValue(key, out var existing) && existing.IsValid()) {
            return await existing.Task;
        }

        var handle = Addressables.LoadAssetAsync<T>(key);
        Handles[key] = handle;

        try {
            return await handle.Task;
        } catch {
            if (handle.IsValid()) {
                Addressables.Release(handle);
            }
            Handles.Remove(key);
            throw;
        }
    }

    public static bool TryGetLoaded(string key, out T asset) {
        asset = null;
        if (!Handles.TryGetValue(key, out var handle) || !handle.IsValid() || !handle.IsDone) {
            return false;
        }

        if (handle.Status != AsyncOperationStatus.Succeeded) {
            return false;
        }

        asset = handle.Result;
        return asset != null;
    }

    public static void Release(string key) {
        if (!Handles.TryGetValue(key, out var handle)) {
            return;
        }

        if (handle.IsValid()) {
            Addressables.Release(handle);
        }
        Handles.Remove(key);
    }

    public static void ReleaseAll() {
        foreach (var handle in Handles.Values) {
            if (handle.IsValid()) {
                Addressables.Release(handle);
            }
        }
        Handles.Clear();
    }
}
