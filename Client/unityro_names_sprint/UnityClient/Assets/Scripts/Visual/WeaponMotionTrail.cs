using System.Collections;
using UnityEngine;

/// <summary>
/// Lightweight procedural trail for weapon sprite anchors. No external texture is
/// required, so the effect works immediately and can later be replaced by authored
/// VFX Graph/particle assets without changing combat code.
/// </summary>
public class WeaponMotionTrail : MonoBehaviour {
    private TrailRenderer Trail;
    private Coroutine StopRoutine;
    private Material RuntimeMaterial;

    private void Awake() {
        Trail = GetComponent<TrailRenderer>();
        if (Trail == null) Trail = gameObject.AddComponent<TrailRenderer>();

        Trail.time = 0.13f;
        Trail.minVertexDistance = 0.025f;
        Trail.widthMultiplier = 0.16f;
        Trail.autodestruct = false;
        Trail.emitting = false;
        Trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        Trail.receiveShadows = false;
        Trail.alignment = LineAlignment.View;
        Trail.textureMode = LineTextureMode.Stretch;

        var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (shader == null) shader = Shader.Find("Sprites/Default");
        if (shader != null) {
            RuntimeMaterial = new Material(shader) { name = "BlackCore_WeaponTrail_Runtime" };
            if (RuntimeMaterial.HasProperty("_BaseColor")) {
                RuntimeMaterial.SetColor("_BaseColor", Color.white);
            } else if (RuntimeMaterial.HasProperty("_Color")) {
                RuntimeMaterial.SetColor("_Color", Color.white);
            }
            Trail.material = RuntimeMaterial;
        }

        var gradient = new Gradient();
        gradient.SetKeys(
            new[] {
                new GradientColorKey(BlackCoreUiTheme.Accent, 0f),
                new GradientColorKey(BlackCoreUiTheme.AccentAlt, 1f)
            },
            new[] {
                new GradientAlphaKey(0.78f, 0f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        Trail.colorGradient = gradient;

        Trail.widthCurve = new AnimationCurve(
            new Keyframe(0f, 1f),
            new Keyframe(0.55f, 0.55f),
            new Keyframe(1f, 0f)
        );
    }

    private void OnDestroy() {
        if (RuntimeMaterial != null) Destroy(RuntimeMaterial);
    }

    public void Trigger(float duration = 0.17f) {
        if (!isActiveAndEnabled || Trail == null) return;

        if (StopRoutine != null) StopCoroutine(StopRoutine);
        Trail.Clear();
        Trail.emitting = true;
        StopRoutine = StartCoroutine(StopAfter(duration));
    }

    private IEnumerator StopAfter(float duration) {
        yield return new WaitForSeconds(duration);
        if (Trail != null) Trail.emitting = false;
        StopRoutine = null;
    }
}
