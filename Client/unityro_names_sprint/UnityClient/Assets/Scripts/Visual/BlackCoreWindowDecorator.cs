using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Runtime skin for legacy UI windows. It deliberately layers on top of the
/// existing functional prefabs instead of replacing their controls or event wiring.
/// </summary>
public static class BlackCoreWindowDecorator {
    public static void Apply(GameObject root, string title, string subtitle = null) {
        if (root == null) return;
        if (root.transform.Find("BlackCore_WindowHeader") != null) return;

        foreach (var text in root.GetComponentsInChildren<Text>(true)) {
            if (text == null) continue;
            text.color = IsMuted(text.text) ? new Color(0.70f, 0.78f, 0.90f, 1f) : BlackCoreUiTheme.TextPrimary;
            if (text.GetComponent<Outline>() == null) {
                var outline = text.gameObject.AddComponent<Outline>();
                outline.effectColor = new Color(0.02f, 0.03f, 0.08f, 0.65f);
                outline.effectDistance = new Vector2(1f, -1f);
            }
        }

        foreach (var tmp in root.GetComponentsInChildren<TMP_Text>(true)) {
            if (tmp == null) continue;
            tmp.color = BlackCoreUiTheme.TextPrimary;
        }

        foreach (var button in root.GetComponentsInChildren<Button>(true)) {
            BlackCoreUiTheme.StyleButton(button, IsPrimaryAction(button));
        }

        var header = new GameObject("BlackCore_WindowHeader", typeof(RectTransform), typeof(Image), typeof(Outline));
        header.transform.SetParent(root.transform, false);
        header.transform.SetAsLastSibling();
        var rect = header.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, 0f);
        rect.sizeDelta = new Vector2(0f, string.IsNullOrWhiteSpace(subtitle) ? 32f : 48f);

        var image = header.GetComponent<Image>();
        image.color = new Color(BlackCoreUiTheme.Panel.r, BlackCoreUiTheme.Panel.g, BlackCoreUiTheme.Panel.b, 0.90f);
        image.raycastTarget = false;
        var outlineHeader = header.GetComponent<Outline>();
        outlineHeader.effectColor = new Color(BlackCoreUiTheme.AccentAlt.r, BlackCoreUiTheme.AccentAlt.g, BlackCoreUiTheme.AccentAlt.b, 0.42f);
        outlineHeader.effectDistance = new Vector2(0f, -1f);

        AddLabel(header.transform, "Title", title, 15, FontStyle.Bold, BlackCoreUiTheme.TextPrimary,
            string.IsNullOrWhiteSpace(subtitle) ? Vector2.zero : new Vector2(0f, 7f));
        if (!string.IsNullOrWhiteSpace(subtitle)) {
            AddLabel(header.transform, "Subtitle", subtitle, 10, FontStyle.Normal, BlackCoreUiTheme.AccentAlt, new Vector2(0f, -11f));
        }
    }

    public static void TranslateCommonLabels(GameObject root) {
        if (root == null) return;
        foreach (var text in root.GetComponentsInChildren<Text>(true)) {
            if (text == null || string.IsNullOrWhiteSpace(text.text)) continue;
            text.text = Translate(text.text);
        }
        foreach (var tmp in root.GetComponentsInChildren<TMP_Text>(true)) {
            if (tmp == null || string.IsNullOrWhiteSpace(tmp.text)) continue;
            tmp.text = Translate(tmp.text);
        }
    }

    private static string Translate(string value) {
        switch (value.Trim()) {
            case "Equipment": return "Equipamentos";
            case "Inventory": return "Mochila";
            case "New Character": return "Novo Viajante";
            case "Enter": return "Entrar no Mundo";
            case "Level": return "Nível";
            case "Job": return "Caminho";
            case "Map": return "Região";
            case "Exp":
            case "EXP": return "Experiência";
            case "Close": return "Fechar";
            case "Next": return "Continuar";
            default: return BlackCoreLoreService.ApplyDialogueIdentity(value);
        }
    }


    private static bool IsPrimaryAction(Button button) {
        if (button == null) return false;
        var labels = button.GetComponentsInChildren<Text>(true);
        foreach (var label in labels) {
            if (label == null || string.IsNullOrWhiteSpace(label.text)) continue;
            var lower = label.text.ToLowerInvariant();
            if (lower.Contains("entrar") || lower.Contains("continuar") || lower.Contains("equip") || lower.Contains("confirm")) return true;
        }
        return false;
    }

    private static bool IsMuted(string value) {
        if (string.IsNullOrWhiteSpace(value)) return false;
        var lower = value.ToLowerInvariant();
        return lower.Contains("exp") || lower.Contains("level") || lower.Contains("hp") || lower.Contains("sp");
    }

    private static void AddLabel(Transform parent, string name, string content, int size, FontStyle style, Color color, Vector2 pos) {
        var label = new GameObject(name, typeof(RectTransform), typeof(Text));
        label.transform.SetParent(parent, false);
        var rect = label.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(10f, 2f);
        rect.offsetMax = new Vector2(-10f, -2f);
        rect.anchoredPosition = pos;
        var text = label.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = size;
        text.fontStyle = style;
        text.alignment = TextAnchor.MiddleLeft;
        text.color = color;
        text.raycastTarget = false;
        text.text = content;
    }
}
