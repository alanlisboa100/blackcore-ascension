using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Small compatibility layer for pointer input. It keeps gameplay code independent
/// from whether the primary pointer is a mouse or a touchscreen finger.
/// </summary>
public static class PointerInput {
    public struct PointerState {
        public Vector2 Position;
        public int PointerId;
        public bool PressedThisFrame;

        public PointerState(Vector2 position, int pointerId, bool pressedThisFrame) {
            Position = position;
            PointerId = pointerId;
            PressedThisFrame = pressedThisFrame;
        }
    }

    public static bool TryGetPrimaryPointer(out PointerState state) {
        if (Input.touchSupported && Input.touchCount > 1) {
            // Two or more fingers are reserved for camera gestures.
            state = default;
            return false;
        }

        if (Input.touchSupported && Input.touchCount == 1) {
            var touch = Input.GetTouch(0);
            state = new PointerState(
                touch.position,
                touch.fingerId,
                touch.phase == TouchPhase.Began
            );
            return true;
        }

        // Real mobile builds should not treat the synthetic mouse position as an
        // always-active hover pointer when no finger is touching the screen.
        if (Application.isMobilePlatform) {
            state = default;
            return false;
        }

        state = new PointerState(Input.mousePosition, -1, Input.GetMouseButtonDown(0));
        return true;
    }

    public static bool IsPointerOverUI(int pointerId) {
        if (EventSystem.current == null) {
            return false;
        }

        return pointerId >= 0
            ? EventSystem.current.IsPointerOverGameObject(pointerId)
            : EventSystem.current.IsPointerOverGameObject();
    }
}
