using System.Collections;
using UnityEngine;

/// <summary>
/// Reusable open/close motion for legacy UI windows. It adds no prefab dependency:
/// existing panels get a CanvasGroup the first time they are toggled.
/// </summary>
public class BlackCoreWindowMotion : MonoBehaviour {
    private CanvasGroup Group;
    private RectTransform Rect;
    private Coroutine Animation;
    private Vector3 BaseScale = Vector3.one;

    private void Awake() {
        Group = gameObject.GetOrAddComponent<CanvasGroup>();
        Rect = transform as RectTransform;
        BaseScale = Rect != null ? Rect.localScale : transform.localScale;
    }

    public static void Toggle(MonoBehaviour window) {
        if (window == null) return;
        Toggle(window.gameObject);
    }

    public static void Toggle(GameObject window) {
        if (window == null) return;

        var motion = window.GetComponent<BlackCoreWindowMotion>();
        if (motion == null) motion = window.AddComponent<BlackCoreWindowMotion>();

        if (window.activeInHierarchy) {
            motion.PlayClose();
        } else {
            window.SetActive(true);
            motion.PlayOpen();
        }
    }

    public void PlayOpen() {
        gameObject.SetActive(true);
        EnsureRefs();
        if (Animation != null) StopCoroutine(Animation);
        Animation = StartCoroutine(Animate(true));
    }

    public void PlayClose() {
        EnsureRefs();
        if (Animation != null) StopCoroutine(Animation);
        Animation = StartCoroutine(Animate(false));
    }

    private void EnsureRefs() {
        if (Group == null) Group = gameObject.GetOrAddComponent<CanvasGroup>();
        if (Rect == null) Rect = transform as RectTransform;
        if (BaseScale == Vector3.zero) BaseScale = Vector3.one;
    }

    private IEnumerator Animate(bool opening) {
        const float duration = 0.14f;
        float elapsed = 0f;
        float fromAlpha = opening ? 0f : Group.alpha;
        float toAlpha = opening ? 1f : 0f;
        Vector3 closedScale = BaseScale * 0.965f;
        Vector3 fromScale = opening ? closedScale : (Rect != null ? Rect.localScale : transform.localScale);
        Vector3 toScale = opening ? BaseScale : closedScale;

        Group.alpha = fromAlpha;
        Group.interactable = opening;
        Group.blocksRaycasts = opening;

        while (elapsed < duration) {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            // Smoothstep without allocating an AnimationCurve.
            t = t * t * (3f - 2f * t);
            Group.alpha = Mathf.Lerp(fromAlpha, toAlpha, t);
            if (Rect != null) Rect.localScale = Vector3.LerpUnclamped(fromScale, toScale, t);
            else transform.localScale = Vector3.LerpUnclamped(fromScale, toScale, t);
            yield return null;
        }

        Group.alpha = toAlpha;
        if (Rect != null) Rect.localScale = toScale;
        else transform.localScale = toScale;

        Animation = null;
        if (!opening) gameObject.SetActive(false);
    }
}
