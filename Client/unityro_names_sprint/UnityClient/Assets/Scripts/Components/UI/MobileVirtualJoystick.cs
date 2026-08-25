using UnityEngine;
using UnityEngine.EventSystems;

public class MobileVirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler {
    public Vector2 Value { get; private set; }

    private RectTransform BaseRect;
    private RectTransform KnobRect;
    private Canvas RootCanvas;

    public void Initialize(RectTransform knob) {
        BaseRect = transform as RectTransform;
        KnobRect = knob;
        RootCanvas = GetComponentInParent<Canvas>();
    }

    public void OnPointerDown(PointerEventData eventData) {
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData) {
        if (BaseRect == null || KnobRect == null) {
            return;
        }

        Camera eventCamera = null;
        if (RootCanvas != null && RootCanvas.renderMode != RenderMode.ScreenSpaceOverlay) {
            eventCamera = RootCanvas.worldCamera;
        }

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(BaseRect, eventData.position, eventCamera, out var localPoint)) {
            return;
        }

        float radius = Mathf.Min(BaseRect.rect.width, BaseRect.rect.height) * 0.5f;
        if (radius <= 0f) {
            return;
        }

        var normalized = Vector2.ClampMagnitude(localPoint / radius, 1f);
        Value = normalized;
        KnobRect.anchoredPosition = normalized * radius * 0.55f;
    }

    public void OnPointerUp(PointerEventData eventData) {
        Value = Vector2.zero;
        if (KnobRect != null) {
            KnobRect.anchoredPosition = Vector2.zero;
        }
    }

    private void OnDisable() {
        Value = Vector2.zero;
        if (KnobRect != null) {
            KnobRect.anchoredPosition = Vector2.zero;
        }
    }
}
