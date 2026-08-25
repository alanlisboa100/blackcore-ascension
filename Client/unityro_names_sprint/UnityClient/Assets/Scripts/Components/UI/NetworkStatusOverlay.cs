using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Minimal runtime network-state UX. It intentionally avoids scene/prefab dependencies so
/// reconnect feedback is available before the final mobile UI art is designed.
/// </summary>
public class NetworkStatusOverlay : MonoBehaviour {
    private Text StatusText;
    private Button RetryButton;
    private NetworkClient Client;
    private CanvasGroup CanvasGroup;

    public static NetworkStatusOverlay EnsureCreated(RectTransform parent, NetworkClient networkClient) {
        var existing = parent.GetComponentInChildren<NetworkStatusOverlay>(true);
        if (existing != null) {
            existing.Client = networkClient;
            return existing;
        }

        var root = new GameObject("NetworkStatusOverlay", typeof(RectTransform), typeof(CanvasGroup), typeof(Image), typeof(NetworkStatusOverlay));
        root.transform.SetParent(parent, false);
        var rect = root.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -20f);
        rect.sizeDelta = new Vector2(360f, 74f);

        var image = root.GetComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0.72f);

        var overlay = root.GetComponent<NetworkStatusOverlay>();
        overlay.Client = networkClient;
        overlay.BuildUi();
        overlay.SetVisible(false);
        return overlay;
    }

    private void BuildUi() {
        CanvasGroup = GetComponent<CanvasGroup>();
        var textGo = new GameObject("Status", typeof(RectTransform), typeof(Text));
        textGo.transform.SetParent(transform, false);
        var textRect = textGo.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0f, 0f);
        textRect.anchorMax = new Vector2(0.72f, 1f);
        textRect.offsetMin = new Vector2(12f, 8f);
        textRect.offsetMax = new Vector2(-4f, -8f);
        StatusText = textGo.GetComponent<Text>();
        StatusText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        StatusText.fontSize = 18;
        StatusText.alignment = TextAnchor.MiddleLeft;
        StatusText.color = Color.white;
        StatusText.text = "Reconectando...";

        var retryGo = new GameObject("Retry", typeof(RectTransform), typeof(Image), typeof(Button));
        retryGo.transform.SetParent(transform, false);
        var retryRect = retryGo.GetComponent<RectTransform>();
        retryRect.anchorMin = new Vector2(0.74f, 0.18f);
        retryRect.anchorMax = new Vector2(0.97f, 0.82f);
        retryRect.offsetMin = Vector2.zero;
        retryRect.offsetMax = Vector2.zero;
        retryGo.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.18f);
        RetryButton = retryGo.GetComponent<Button>();
        RetryButton.onClick.AddListener(() => _ = Client?.TryReconnectCurrentServerAsync());

        var labelGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
        labelGo.transform.SetParent(retryGo.transform, false);
        var labelRect = labelGo.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
        var label = labelGo.GetComponent<Text>();
        label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        label.fontSize = 16;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = Color.white;
        label.text = "Tentar";
    }

    private void OnEnable() {
        NetworkClient.OnConnectionStateChanged += OnConnectionStateChanged;
        NetworkClient.OnReconnectAttempt += OnReconnectAttempt;
        NetworkClient.OnReconnected += OnReconnected;
    }

    private void OnDisable() {
        NetworkClient.OnConnectionStateChanged -= OnConnectionStateChanged;
        NetworkClient.OnReconnectAttempt -= OnReconnectAttempt;
        NetworkClient.OnReconnected -= OnReconnected;
    }

    private void OnConnectionStateChanged(NetworkConnectionState state) {
        switch (state) {
            case NetworkConnectionState.Reconnecting:
                SetVisible(true);
                if (StatusText != null) StatusText.text = "Reconectando ao servidor...";
                if (RetryButton != null) RetryButton.gameObject.SetActive(false);
                break;
            case NetworkConnectionState.Disconnected:
                SetVisible(true);
                if (StatusText != null) StatusText.text = "Conexão perdida";
                if (RetryButton != null) RetryButton.gameObject.SetActive(true);
                break;
            case NetworkConnectionState.Connected:
                SetVisible(false);
                break;
        }
    }

    private void OnReconnectAttempt(int attempt, int total) {
        SetVisible(true);
        if (StatusText != null) {
            StatusText.text = $"Reconectando... {attempt}/{total}";
        }
        if (RetryButton != null) RetryButton.gameObject.SetActive(false);
    }

    private void OnReconnected() {
        SetVisible(false);
    }

    private void SetVisible(bool visible) {
        if (CanvasGroup == null) {
            CanvasGroup = GetComponent<CanvasGroup>();
        }
        if (CanvasGroup == null) {
            return;
        }

        CanvasGroup.alpha = visible ? 1f : 0f;
        CanvasGroup.interactable = visible;
        CanvasGroup.blocksRaycasts = visible;
    }
}
