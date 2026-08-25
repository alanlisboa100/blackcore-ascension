using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// Keeps Addressables references alive for the lifetime of a GameObject and
/// releases them when that object is destroyed.
/// </summary>
public class AddressableHandleOwner : MonoBehaviour {
    private readonly List<Action> ReleaseActions = new List<Action>(4);

    public void Add<T>(AsyncOperationHandle<T> handle) {
        ReleaseActions.Add(() => {
            if (handle.IsValid()) {
                Addressables.Release(handle);
            }
        });
    }

    private void OnDestroy() {
        foreach (var release in ReleaseActions) {
            release();
        }
        ReleaseActions.Clear();
    }
}
