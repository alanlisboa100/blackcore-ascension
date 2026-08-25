#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class MobileHudDebugMenu {
    private const string PrefKey = "unityro.force_mobile_hud";
    private const string MenuPath = "UnityRO/Mobile HUD/Force In Editor";

    [MenuItem(MenuPath)]
    public static void ToggleMobileHud() {
        bool enabled = PlayerPrefs.GetInt(PrefKey, 0) == 1;
        PlayerPrefs.SetInt(PrefKey, enabled ? 0 : 1);
        PlayerPrefs.Save();
        Menu.SetChecked(MenuPath, !enabled);
        Debug.Log($"Mobile HUD editor preview {(!enabled ? "enabled" : "disabled")}. Reload MapScene to apply.");
    }

    [MenuItem(MenuPath, true)]
    public static bool ValidateToggleMobileHud() {
        Menu.SetChecked(MenuPath, PlayerPrefs.GetInt(PrefKey, 0) == 1);
        return true;
    }
}
#endif
