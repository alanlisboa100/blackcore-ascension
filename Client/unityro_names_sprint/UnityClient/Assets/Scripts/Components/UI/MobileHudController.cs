using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// First functional mobile HUD layer. Art can be replaced later without changing gameplay APIs.
/// It is created at runtime only on mobile (or when explicitly forced through PlayerPrefs).
/// </summary>
public class MobileHudController : MonoBehaviour {
    private const string FORCE_MOBILE_HUD_PREF = "unityro.force_mobile_hud";

    private MobileVirtualJoystick Joystick;
    private EntityControl EntityControl;
    private MapUiController MapUiController;
    private readonly List<Button> SkillButtons = new List<Button>();
    private readonly List<Text> SkillLabels = new List<Text>();
    private float NextSkillRefresh;

    public static bool ShouldShow {
        get {
#if UNITY_EDITOR
            if (PlayerPrefs.GetInt(FORCE_MOBILE_HUD_PREF, 0) == 1) return true;
#endif
            return Application.isMobilePlatform;
        }
    }

    public static MobileHudController EnsureCreated(RectTransform parent, MapUiController mapUiController) {
        if (!ShouldShow) {
            return null;
        }

        var existing = parent.GetComponentInChildren<MobileHudController>(true);
        if (existing != null) {
            existing.MapUiController = mapUiController;
            return existing;
        }

        var root = new GameObject("MobileHUD", typeof(RectTransform), typeof(MobileSafeArea), typeof(MobileHudController));
        root.transform.SetParent(parent, false);
        var rect = root.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        var controller = root.GetComponent<MobileHudController>();
        controller.MapUiController = mapUiController;
        controller.BuildUi();
        return controller;
    }

    private void BuildUi() {
        CreateBrandBadge();
        Joystick = CreateJoystick();

        CreateActionButton("ATK", new Vector2(1f, 0f), new Vector2(-92f, 92f), new Vector2(112f, 112f), OnAttackPressed);

        var skillPositions = new[] {
            new Vector2(-205f, 74f),
            new Vector2(-160f, 155f),
            new Vector2(-78f, 200f),
            new Vector2(-255f, 158f)
        };
        for (int i = 0; i < 4; i++) {
            int slot = i;
            var button = CreateActionButton($"S{i + 1}", new Vector2(1f, 0f), skillPositions[i], new Vector2(78f, 78f), () => UseSkill(slot));
            SkillButtons.Add(button);
            SkillLabels.Add(button.GetComponentInChildren<Text>());
        }

        CreateActionButton("BAG", new Vector2(1f, 1f), new Vector2(-65f, -68f), new Vector2(72f, 54f), () => BlackCoreWindowMotion.Toggle(MapUiController?.InventoryWindow));
        CreateActionButton("SKL", new Vector2(1f, 1f), new Vector2(-145f, -68f), new Vector2(72f, 54f), () => BlackCoreWindowMotion.Toggle(MapUiController?.SkillWindow));
    }


    private void CreateBrandBadge() {
        var badge = new GameObject("BlackCoreBrand", typeof(RectTransform), typeof(Image));
        badge.transform.SetParent(transform, false);
        var rect = badge.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -18f);
        rect.sizeDelta = new Vector2(300f, 42f);
        badge.GetComponent<Image>().color = new Color(BlackCoreUiTheme.Panel.r, BlackCoreUiTheme.Panel.g, BlackCoreUiTheme.Panel.b, 0.66f);

        var outline = badge.AddComponent<Outline>();
        outline.effectColor = new Color(BlackCoreUiTheme.Accent.r, BlackCoreUiTheme.Accent.g, BlackCoreUiTheme.Accent.b, 0.38f);
        outline.effectDistance = new Vector2(1f, -1f);

        var textGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
        textGo.transform.SetParent(badge.transform, false);
        var textRect = textGo.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(8f, 0f);
        textRect.offsetMax = new Vector2(-8f, 0f);

        var text = textGo.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = 15;
        text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = BlackCoreUiTheme.TextPrimary;
        text.text = BlackCoreBrand.GameName.ToUpperInvariant();

        var shadow = textGo.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.8f);
        shadow.effectDistance = new Vector2(1.5f, -1.5f);
    }

    private MobileVirtualJoystick CreateJoystick() {
        var baseGo = new GameObject("Joystick", typeof(RectTransform), typeof(Image), typeof(MobileVirtualJoystick));
        baseGo.transform.SetParent(transform, false);
        var baseRect = baseGo.GetComponent<RectTransform>();
        baseRect.anchorMin = Vector2.zero;
        baseRect.anchorMax = Vector2.zero;
        baseRect.pivot = new Vector2(0.5f, 0.5f);
        baseRect.anchoredPosition = new Vector2(115f, 115f);
        baseRect.sizeDelta = new Vector2(170f, 170f);
        baseGo.GetComponent<Image>().color = new Color(BlackCoreUiTheme.AccentAlt.r, BlackCoreUiTheme.AccentAlt.g, BlackCoreUiTheme.AccentAlt.b, 0.16f);

        var knobGo = new GameObject("Knob", typeof(RectTransform), typeof(Image));
        knobGo.transform.SetParent(baseGo.transform, false);
        var knobRect = knobGo.GetComponent<RectTransform>();
        knobRect.anchorMin = new Vector2(0.5f, 0.5f);
        knobRect.anchorMax = new Vector2(0.5f, 0.5f);
        knobRect.pivot = new Vector2(0.5f, 0.5f);
        knobRect.anchoredPosition = Vector2.zero;
        knobRect.sizeDelta = new Vector2(76f, 76f);
        knobGo.GetComponent<Image>().color = new Color(BlackCoreUiTheme.Accent.r, BlackCoreUiTheme.Accent.g, BlackCoreUiTheme.Accent.b, 0.42f);

        var joystick = baseGo.GetComponent<MobileVirtualJoystick>();
        joystick.Initialize(knobRect);
        return joystick;
    }

    private Button CreateActionButton(string label, Vector2 anchor, Vector2 position, Vector2 size, UnityEngine.Events.UnityAction onClick) {
        var go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(transform, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = anchor;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        var image = go.GetComponent<Image>();
        image.color = BlackCoreUiTheme.PanelSoft;

        var button = go.GetComponent<Button>();
        button.onClick.AddListener(onClick);

        var textGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
        textGo.transform.SetParent(go.transform, false);
        var textRect = textGo.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        var text = textGo.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = 18;
        text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = BlackCoreUiTheme.TextPrimary;
        text.text = label;

        BlackCoreUiTheme.StyleButton(button, label == "ATK");
        return button;
    }

    private void Update() {
        if (EntityControl == null) {
            EntityControl = FindObjectOfType<EntityControl>();
        }

        EntityControl?.SetMobileMoveInput(Joystick != null ? Joystick.Value : Vector2.zero);

        if (Time.unscaledTime >= NextSkillRefresh) {
            NextSkillRefresh = Time.unscaledTime + 1f;
            RefreshSkillLabels();
        }
    }

    private void OnDisable() {
        EntityControl?.SetMobileMoveInput(Vector2.zero);
    }

    private void OnAttackPressed() {
        EntityControl?.RequestBasicAttackSelected();
    }

    private void UseSkill(int slot) {
        if (EntityControl == null || Session.CurrentSession?.Entity == null) {
            return;
        }

        var entity = Session.CurrentSession.Entity as Entity;
        var skills = GetUsableSkills(entity);
        if (slot < 0 || slot >= skills.Count) {
            return;
        }

        var skill = skills[slot];
        EntityControl.UseSkill(skill, skill.Level);
    }

    private void RefreshSkillLabels() {
        if (Session.CurrentSession?.Entity == null) {
            return;
        }

        var entity = Session.CurrentSession.Entity as Entity;
        var skills = GetUsableSkills(entity);
        for (int i = 0; i < SkillLabels.Count; i++) {
            if (i < skills.Count) {
                var info = skills[i];
                string skillName = SkillTable.Skills.TryGetValue(info.SkillID, out var skill) ? skill.SkillName : $"Skill {info.SkillID}";
                SkillLabels[i].text = Abbreviate(skillName, i + 1);
                SkillButtons[i].interactable = true;
            } else {
                SkillLabels[i].text = $"S{i + 1}";
                SkillButtons[i].interactable = false;
            }
        }
    }

    private static List<SkillInfo> GetUsableSkills(Entity entity) {
        var result = new List<SkillInfo>(4);
        if (entity?.SkillTree?.OwnedSkillsInfos == null) {
            return result;
        }

        foreach (var info in entity.SkillTree.OwnedSkillsInfos) {
            if (info != null && info.Level > 0) {
                result.Add(info);
                if (result.Count == 4) break;
            }
        }
        return result;
    }

    private static string Abbreviate(string name, int fallbackIndex) {
        if (string.IsNullOrWhiteSpace(name)) {
            return $"S{fallbackIndex}";
        }
        return name.Length <= 6 ? name : name.Substring(0, 6);
    }
}
