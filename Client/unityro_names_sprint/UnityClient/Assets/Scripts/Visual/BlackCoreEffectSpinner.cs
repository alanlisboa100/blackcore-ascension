using UnityEngine;

public class BlackCoreEffectSpinner : MonoBehaviour {
    public float DegreesPerSecond = 32f;

    private void Update() {
        transform.Rotate(0f, DegreesPerSecond * Time.deltaTime, 0f, Space.Self);
    }
}
