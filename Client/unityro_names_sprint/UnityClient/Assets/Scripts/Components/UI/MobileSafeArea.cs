using UnityEngine;

/// <summary>
/// Keeps runtime-created mobile controls inside notches, rounded corners and gesture areas.
/// </summary>
public class MobileSafeArea : MonoBehaviour {
    private RectTransform RectTransform;
    private Rect LastSafeArea;
    private Vector2Int LastScreenSize;

    private void Awake() {
        RectTransform = transform as RectTransform;
    }

    private void Start() {
        ApplySafeArea();
    }

    private void Update() {
        if (LastSafeArea != Screen.safeArea || LastScreenSize.x != Screen.width || LastScreenSize.y != Screen.height) {
            ApplySafeArea();
        }
    }

    private void ApplySafeArea() {
        if (RectTransform == null || Screen.width <= 0 || Screen.height <= 0) {
            return;
        }

        var safeArea = Screen.safeArea;
        var anchorMin = safeArea.position;
        var anchorMax = safeArea.position + safeArea.size;

        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        RectTransform.anchorMin = anchorMin;
        RectTransform.anchorMax = anchorMax;
        RectTransform.offsetMin = Vector2.zero;
        RectTransform.offsetMax = Vector2.zero;

        LastSafeArea = safeArea;
        LastScreenSize = new Vector2Int(Screen.width, Screen.height);
    }
}
