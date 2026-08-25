using System;
using UnityEngine;

namespace UnityRO.GameCamera {
    /// <summary>
    /// Ingame player camera controller
    /// </summary>
    public class CharacterCamera : MonoBehaviour {
        [Header(":: Refs")]
        public Camera GameCamera;
        [Header(":: User Parameters")]
        public Vector2 MouseSensitivity = Vector2.one;
        public float ScrollPitchSensitivity = 1f;
        public float ScrollZoomSensitivity = 1f;
        [Header(":: Mobile Parameters")]
        [Tooltip("Degrees of yaw per horizontal touch pixel when two fingers are dragging.")]
        public float MobileYawDegreesPerPixel = 0.12f;
        [Tooltip("World zoom units per pinch pixel.")]
        public float MobileZoomPerPixel = 0.03f;
        [Header(":: Settings")]
        public CameraControlProfile YawControl;
        public CameraControlProfile ZoomControl;
        [Header(":: Black Core Combat Feedback")]
        [Range(0f, 12f)] public float ShakeDecay = 5.5f;
        [Range(0f, 0.5f)] public float MaxShakeOffset = 0.14f;
        [Range(0f, 2f)] public float MaxShakeRotation = 0.55f;

        public float LerpTime = 0.5f;
        public float Distance = 30f;
        public Vector2 ZoomConstraint;
        public Vector2 PitchConstraint;

        [SerializeField]
        private Transform m_Target;

        public Direction Direction;
        public Vector3 HorizontalDirection { get; private set; }
        public float Pitch { get; private set; } = 0.7853982f;

        private float m_Yaw = 7.869574f;
        private float m_Altitude = 0.7071068f;
        private float m_SphereSliceRadius = 0.7071068f;
        private float m_ShakeTrauma;
        // cache
        private readonly float s_PI2 = Mathf.PI * 2f;
        private Vector2 m_PitchConstraintRad = new Vector2(0.5235988f, 0.7853982f);

        //@TODO: Double right tap to reset cam
        //@TODO: Remove static / create entityviewer factories
        /// <summary>
        /// Don't use
        /// </summary>
        public static CharacterCamera ROCamera { get; private set; }

        public void SetTarget(Transform tr) {
            m_Target = tr;
        }

        private void Awake() {
            ROCamera = this; // 
            m_PitchConstraintRad = new Vector2(PitchConstraint.x, PitchConstraint.y) * Mathf.Deg2Rad;
            RecomputeCameraAngle();
        }

        private void Update() {
            float dt = Time.deltaTime;

            if (Application.isMobilePlatform && Input.touchSupported) {
                UpdateMobileInput();
            } else {
                UpdateDesktopInput(dt);
            }

            UpdateCameraInertia(dt);
        }

        private void UpdateDesktopInput(float dt) {
            bool shiftModifier = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            float mouseScroll = Input.mouseScrollDelta.y;

            if (Input.GetMouseButton(1)) {
                float hX = Input.GetAxis("Mouse X");
                float hY = Input.GetAxis("Mouse Y");

                if (shiftModifier) {
                    float vScroll = hY * MouseSensitivity.y + mouseScroll * ScrollPitchSensitivity;
                    if (vScroll != 0f) {
                        Pitch -= vScroll * dt;
                        RecomputeCameraAngle();
                    }
                } else {
                    YawControl.SetInertia(hX * MouseSensitivity.x);
                }
            } else if (Input.GetMouseButtonUp(1) && !shiftModifier) {
                YawControl.Release();
            }

            if (!shiftModifier && mouseScroll != 0f) {
                ZoomControl.SetInertia(mouseScroll * ScrollZoomSensitivity);
                ZoomControl.Release();
            }
        }

        private void UpdateMobileInput() {
            // Reserve one-finger taps for movement/targeting. Two fingers manipulate
            // the camera so camera gestures do not fight gameplay taps.
            if (Input.touchCount < 2) {
                return;
            }

            var touch0 = Input.GetTouch(0);
            var touch1 = Input.GetTouch(1);

            Vector2 averageDelta = (touch0.deltaPosition + touch1.deltaPosition) * 0.5f;
            if (averageDelta.x != 0f) {
                m_Yaw -= averageDelta.x * MobileYawDegreesPerPixel * Mathf.Deg2Rad;
                m_Yaw = Mathf.Repeat(m_Yaw, s_PI2);
                RecomputeHorizontalDirection();
            }

            Vector2 previous0 = touch0.position - touch0.deltaPosition;
            Vector2 previous1 = touch1.position - touch1.deltaPosition;
            float previousDistance = Vector2.Distance(previous0, previous1);
            float currentDistance = Vector2.Distance(touch0.position, touch1.position);
            float pinchDelta = currentDistance - previousDistance;

            if (pinchDelta != 0f) {
                Distance = Mathf.Clamp(
                    Distance - pinchDelta * MobileZoomPerPixel,
                    ZoomConstraint.x,
                    ZoomConstraint.y
                );
            }
        }

        private void UpdateCameraInertia(float dt) {
            if (YawControl.Update(dt)) {
                m_Yaw -= YawControl.Velocity * dt;
                m_Yaw = Mathf.Repeat(m_Yaw, s_PI2);
                RecomputeHorizontalDirection();
            }

            if (ZoomControl.Update(dt)) {
                float zoomVel = ZoomControl.Velocity * dt;
                Distance = Mathf.Clamp(Distance + zoomVel, ZoomConstraint.x, ZoomConstraint.y);
            }
        }

        private void LateUpdate() {
            UpdateCameraPosition();
            UpdateCameraLookAt();
            ApplyCombatShake();
        }

        /// <summary>
        /// Adds a short client-only camera impulse. Network simulation is untouched.
        /// </summary>
        public void AddTrauma(float amount) {
            m_ShakeTrauma = Mathf.Clamp01(Mathf.Max(m_ShakeTrauma, amount));
        }

        private void ApplyCombatShake() {
            if (GameCamera == null || m_ShakeTrauma <= 0f) {
                return;
            }

            float squaredTrauma = m_ShakeTrauma * m_ShakeTrauma;
            float seed = Time.unscaledTime * 29f;
            float xNoise = Mathf.PerlinNoise(seed, 0.173f) * 2f - 1f;
            float yNoise = Mathf.PerlinNoise(0.617f, seed) * 2f - 1f;
            float rotNoise = Mathf.PerlinNoise(seed, seed * 0.37f) * 2f - 1f;

            GameCamera.transform.position += (GameCamera.transform.right * xNoise + GameCamera.transform.up * yNoise)
                * MaxShakeOffset * squaredTrauma;
            GameCamera.transform.Rotate(0f, 0f, rotNoise * MaxShakeRotation * squaredTrauma, Space.Self);

            m_ShakeTrauma = Mathf.MoveTowards(m_ShakeTrauma, 0f, Time.unscaledDeltaTime * ShakeDecay);
        }

        private void UpdateCameraPosition() {
            Vector3 pos = Vector3.zero;
            Vector3 hDir = HorizontalDirection;

            if (m_Target != null) {
                pos = m_Target.transform.position;
            }

            hDir.y = -m_Altitude;
            pos -= hDir * Distance;
            GameCamera.transform.localPosition = pos;
        }

        private void UpdateCameraLookAt() {
            if (m_Target != null) {
                GameCamera.transform.LookAt(m_Target);
            }

            float angle = (float) ((m_Yaw + Math.PI / 8f) / (2f * Math.PI));

            if (angle < 0f)
                angle += 1f;

            float orientedAngle = angle - 1f / 4f;
            int direction = (int) (orientedAngle * 8) % 8;
            Direction = (Direction) direction;
        }

        private void RecomputeHorizontalDirection() {
            HorizontalDirection = new Vector3(Mathf.Cos(m_Yaw) * m_SphereSliceRadius, 0f, Mathf.Sin(m_Yaw) * m_SphereSliceRadius);
        }

        private void RecomputeCameraAngle() {
            Pitch = Mathf.Clamp(Pitch, m_PitchConstraintRad.x, m_PitchConstraintRad.y);
            m_Altitude = Mathf.Sin(Pitch);
            m_SphereSliceRadius = Mathf.Cos(Pitch);
            RecomputeHorizontalDirection();
        }
    }
}
