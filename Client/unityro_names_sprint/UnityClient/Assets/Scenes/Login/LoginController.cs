using ROIO;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginController : MonoBehaviour {

    public InputField usernameField;
    public InputField passwordField;
    public RawImage background;

    private NetworkClient NetworkClient;
    private RemoteConfiguration RemoteConfiguration;

    void Start() {
        background.SetLoginBackground();
        ApplyBlackCorePresentation();

        NetworkClient = FindObjectOfType<NetworkClient>();
        RemoteConfiguration = FindObjectOfType<GameManager>().RemoteConfiguration;

        NetworkClient.HookPacket(AC.ACCEPT_LOGIN3.HEADER, OnLoginResponse);
    }

    void Update() {
        TabBehaviour();
    }

    private void TabBehaviour() {
        EventSystem currentEvent = EventSystem.current;

        if (currentEvent.currentSelectedGameObject == null || !Input.GetKeyDown(KeyCode.Tab))
            return;

        Selectable current = currentEvent.currentSelectedGameObject.GetComponent<Selectable>();
        if (current == null)
            return;

        bool up = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        Selectable next = up ? current.FindSelectableOnUp() : current.FindSelectableOnDown();
        next = current == next || next == null ? Selectable.allSelectablesArray[0] : next;
        currentEvent.SetSelectedGameObject(next.gameObject);
    }

    public void OnLoginClicked() {
        var username = usernameField.text;
        var password = passwordField.text;

        if (username.Length == 0 || password.Length == 0) {
            return;
        }

        TryConnectAndLogin(username, password);
    }

    public void OnExitClicked() {
        Application.Quit();
    }

    private async void TryConnectAndLogin(string username, string password) {
        await NetworkClient.ChangeServer(RemoteConfiguration.loginServer, int.Parse(RemoteConfiguration.loginPort), NetworkServerRole.Login);
        new CA.LOGIN(username, password, 10, 10).Send();
    }

    private void OnLoginResponse(ushort cmd, int size, InPacket packet) {
        if (packet is AC.ACCEPT_LOGIN3) {
            var pkt = packet as AC.ACCEPT_LOGIN3;

            NetworkClient.State.LoginInfo = pkt;
            SceneManager.LoadSceneAsync("CharServerSelectionScene");
        }
    }

    private void ApplyBlackCorePresentation() {
        if (background != null) {
            background.color = new Color(0.86f, 0.92f, 1f, 1f);
        }

        ApplyInputPresentation(usernameField, "Usuário ou e-mail");
        ApplyInputPresentation(passwordField, "Senha");

        foreach (var text in FindObjectsOfType<Text>(true)) {
            if (text == null || string.IsNullOrWhiteSpace(text.text)) continue;

            switch (text.text.Trim()) {
                case "User":
                case "Username":
                    text.text = "Usuário";
                    break;
                case "Password":
                    text.text = "Senha";
                    break;
                case "Login":
                    text.text = "Entrar";
                    break;
                case "Exit":
                    text.text = "Sair";
                    break;
            }

            if (text.text.Equals("Entrar", StringComparison.OrdinalIgnoreCase)) {
                text.fontStyle = FontStyle.Bold;
                text.color = BlackCoreUiTheme.TextPrimary;
            }
        }

        BuildBrandHeader();
    }

    private void ApplyInputPresentation(InputField field, string placeholderText) {
        if (field == null) return;

        var image = field.GetComponent<Image>();
        if (image != null) {
            image.color = new Color(0.08f, 0.11f, 0.19f, 0.88f);
        }

        if (field.textComponent != null) {
            field.textComponent.color = BlackCoreUiTheme.TextPrimary;
        }

        if (field.placeholder is Text placeholder) {
            placeholder.text = placeholderText;
            placeholder.color = new Color(0.70f, 0.78f, 0.93f, 0.72f);
        }
    }

    private void BuildBrandHeader() {
        var canvas = FindObjectOfType<Canvas>();
        if (canvas == null || canvas.transform.Find("BlackCore_LoginHeader") != null) return;

        var panel = new GameObject("BlackCore_LoginHeader", typeof(RectTransform), typeof(Image), typeof(Outline));
        panel.transform.SetParent(canvas.transform, false);

        var rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -18f);
        rect.sizeDelta = new Vector2(420f, 92f);

        var image = panel.GetComponent<Image>();
        image.color = new Color(BlackCoreUiTheme.Panel.r, BlackCoreUiTheme.Panel.g, BlackCoreUiTheme.Panel.b, 0.84f);

        var outline = panel.GetComponent<Outline>();
        outline.effectColor = new Color(BlackCoreUiTheme.AccentAlt.r, BlackCoreUiTheme.AccentAlt.g, BlackCoreUiTheme.AccentAlt.b, 0.45f);
        outline.effectDistance = new Vector2(1.5f, -1.5f);

        CreateHeaderText(panel.transform, "Title", new Vector2(0f, -22f), 28, FontStyle.Bold,
            BlackCoreUiTheme.TextPrimary, BlackCoreBrand.GameName);
        CreateHeaderText(panel.transform, "Subtitle", new Vector2(0f, -58f), 13, FontStyle.Normal,
            BlackCoreUiTheme.AccentAlt, "Entre no Núcleo • MMORPG mobile com identidade própria");

        var footer = new GameObject("BlackCore_LoginFooter", typeof(RectTransform), typeof(Text));
        footer.transform.SetParent(canvas.transform, false);
        var footerRect = footer.GetComponent<RectTransform>();
        footerRect.anchorMin = new Vector2(0.5f, 0f);
        footerRect.anchorMax = new Vector2(0.5f, 0f);
        footerRect.pivot = new Vector2(0.5f, 0f);
        footerRect.anchoredPosition = new Vector2(0f, 14f);
        footerRect.sizeDelta = new Vector2(560f, 28f);
        var footerText = footer.GetComponent<Text>();
        footerText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        footerText.fontSize = 12;
        footerText.alignment = TextAnchor.MiddleCenter;
        footerText.color = new Color(0.80f, 0.86f, 0.98f, 0.88f);
        footerText.text = "Visual Black Core ativo • Conecte-se e continue sua ascensão";
    }

    private void CreateHeaderText(Transform parent, string name, Vector2 anchoredPosition, int fontSize,
        FontStyle style, Color color, string content) {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(400f, 28f);

        var text = go.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = color;
        text.text = content;
    }

    private void OnDestroy() {
        NetworkClient?.UnhookPacket(AC.ACCEPT_LOGIN3.HEADER, OnLoginResponse);
    }
}
