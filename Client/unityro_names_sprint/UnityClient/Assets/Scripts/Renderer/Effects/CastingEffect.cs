using Assets.Scripts.Effects;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

class CastingEffect : MonoBehaviour {
    public static Dictionary<string, Material> CastMaterials = new Dictionary<string, Material>();
    public Material CastMaterial;
    public GameObject FollowTarget;
    public float Duration;

    private PrimitiveCylinderEffect prim;
    private PrimitiveCircleEffect castRing;

    public static async void StartCasting(float duration, string texture, GameObject followTarget) {
        var go = new GameObject("CastingEffect");
        var cast = go.AddComponent<CastingEffect>();

        try {
            if (CastMaterials.TryGetValue(texture, out var cachedMaterial) && cachedMaterial != null) {
                cast.CastMaterial = cachedMaterial;
            } else {
                var loadedTexture = await AddressableAssetCache<Texture2D>.LoadAsync(texture);
                cast.CastMaterial = new Material(Shader.Find("Mobile/Particles/Additive"));
                cast.CastMaterial.mainTexture = loadedTexture;
                CastMaterials[texture] = cast.CastMaterial;
            }

            if (followTarget == null) {
                GameObject.Destroy(go);
                return;
            }

            cast.FollowTarget = followTarget;
            cast.Duration = duration;
            cast.Init();
        } catch (System.Exception ex) {
            Debug.LogWarning($"Unable to load casting texture '{texture}': {ex.Message}");
            GameObject.Destroy(go);
        }
    }

    public void Init() {
        prim = PrimitiveCylinderEffect.LaunchEffect(gameObject, CastMaterial, 4, Duration);
        prim.Updater = prim.Update3DCasting;
        prim.Renderer = prim.Render3DCasting;

        prim.FollowEntity(FollowTarget);

        // A ground ring makes casting readable on small mobile screens and lets
        // Bloom pick up the spell silhouette without requiring new art assets.
        var ringObject = new GameObject("BlackCore_CastRing");
        ringObject.transform.SetParent(transform, false);
        ringObject.transform.localPosition = new Vector3(0f, 0.025f, 0f);
        castRing = PrimitiveCircleEffect.LaunchEffect(ringObject, CastMaterial, 1, Duration);
        castRing.Updater = castRing.Update3DCircle;
        castRing.Renderer = castRing.Render3DCircle;
        castRing.Radius = 4.65f;
        castRing.InnerSize = 3.72f;
        castRing.ArcAngle = 18f;
        castRing.Alpha = 0f;
        castRing.MaxAlpha = 135f;
        castRing.AlphaSpeed = 0.018f;
        ringObject.AddComponent<BlackCoreEffectSpinner>().DegreesPerSecond = 28f;

        transform.localScale = new Vector3(2f, 2f, 2f);

        prim.Parts[0] = new EffectPart() {
            Active = true,
            Step = 0,
            CoverAngle = 315,
            MaxHeight = 25,
            Angle = 0,
            Alpha = 180,
            Distance = 4.5f, //4.5f,
            RiseAngle = 70
        };

        prim.Parts[1] = new EffectPart() {
            Active = true,
            Step = 0,
            CoverAngle = 315,
            MaxHeight = 22,
            Angle = 90,
            Alpha = 180,
            Distance = 4.5f, //5f,
            RiseAngle = 57
        };

        prim.Parts[2] = new EffectPart() {
            Active = true,
            Step = 0,
            CoverAngle = 315,
            MaxHeight = 19,
            Angle = 45,
            Alpha = 180,
            Distance = 4.5f, //5.5f,
            RiseAngle = 45
        };

        prim.Parts[3] = new EffectPart() {
            Active = true,
            Step = 0,
            CoverAngle = 360,
            MaxHeight = 250,
            Angle = 0,
            Alpha = 70,
            Distance = 4f, //4f,
            RiseAngle = 89
        };

        for (var i = 0; i < 4; i++) {
            for (var j = 0; j < EffectPart.PartCount; j++) {
                prim.Parts[i].Heights[j] = 0;
                prim.Parts[i].Flags[j] = 0;
            }
        }
    }

}