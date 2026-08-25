using ROIO.Models.FileTypes;
using System;
using UnityEngine;

namespace Assets.Scripts.Renderer.Map {

    [Serializable]
    public class GameMap : MonoBehaviour {

        [SerializeField]
        private Vector2Int _size;
        public Vector2Int Size => _size;

        [SerializeField]
        private RSW.LightInfo LightInfo;

        [SerializeField]
        private Altitude Altitude;

        private Light WorldLight;
        private PathFinder PathFinder;

        private void Start() {
            InitWorldLight();
            InitPathFinder();
        }

        private void InitPathFinder() {
            PathFinder = gameObject.GetOrAddComponent<PathFinder>();
            PathFinder.LoadMap(Altitude);
        }

        private void InitWorldLight() {
            if (WorldLight != null)
                return;

            var worldLightGameObject = new GameObject("Light");
            worldLightGameObject.transform.SetParent(gameObject.transform);
            WorldLight = worldLightGameObject.GetOrAddComponent<Light>();
            SetupWorldLight();
        }

        private void SetupWorldLight() {
            if (LightInfo == null) {
                return;
            }

            WorldLight.type = LightType.Directional;
            WorldLight.shadows = LightShadows.Soft;
            WorldLight.shadowStrength = Application.isMobilePlatform ? 0.46f : 0.58f;
            WorldLight.shadowBias = 0.055f;
            WorldLight.shadowNormalBias = 0.35f;
            WorldLight.intensity = LightInfo.intensity;

            Vector3 lightRotation = new Vector3(LightInfo.longitude, LightInfo.latitude, 0);
            WorldLight.transform.rotation = Quaternion.identity;
            WorldLight.transform.Rotate(lightRotation);

            Color ambient = new Color(LightInfo.ambient[0], LightInfo.ambient[1], LightInfo.ambient[2]);
            Color diffuse = new Color(LightInfo.diffuse[0], LightInfo.diffuse[1], LightInfo.diffuse[2]);

            // Tri-light ambient preserves the original map palette while giving
            // buildings, foliage and characters more readable top/bottom separation.
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = Color.Lerp(ambient, diffuse, 0.18f) * LightInfo.intensity;
            RenderSettings.ambientEquatorColor = ambient * LightInfo.intensity;
            RenderSettings.ambientGroundColor = ambient * LightInfo.intensity * 0.58f;

            WorldLight.color = diffuse;
        }

        public void SetMapSize(int width, int height) {
            _size = new Vector2Int(width, height);
        }

        public void SetMapLightInfo(RSW.LightInfo lightInfo) {
            LightInfo = lightInfo;

            if (WorldLight == null) {
                InitWorldLight();
            }

            SetupWorldLight();
        }

        public void SetMapAltitude(Altitude altitude) {
            Altitude = altitude;
            PathFinder?.LoadMap(Altitude);
        }

        public PathFinder GetPathFinder() {
            if (PathFinder == null) {
                InitPathFinder();
            }

            return PathFinder;
        }
    }
}
