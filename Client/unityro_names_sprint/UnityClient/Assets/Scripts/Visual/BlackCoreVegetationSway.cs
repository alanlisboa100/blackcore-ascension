using UnityEngine;

/// <summary>
/// Tiny visible-only sway for grass/leaves. This is deliberately subtle so legacy
/// low-poly assets feel alive without bending whole buildings or costing mobile CPU.
/// </summary>
public class BlackCoreVegetationSway : MonoBehaviour {
    private Quaternion BaseRotation;
    private Renderer CachedRenderer;
    private float Phase;
    private float Amplitude = 0.6f;

    public void Configure(float amplitude) {
        Amplitude = amplitude;
    }

    private void Awake() {
        BaseRotation = transform.localRotation;
        CachedRenderer = GetComponent<Renderer>();
        Phase = Mathf.Abs(GetInstanceID() * 0.0137f) % 10f;
    }

    private void Update() {
        if (CachedRenderer != null && !CachedRenderer.isVisible) return;

        float angle = Mathf.Sin(Time.time * 0.85f + Phase) * Amplitude;
        transform.localRotation = BaseRotation * Quaternion.AngleAxis(angle, Vector3.forward);
    }
}
