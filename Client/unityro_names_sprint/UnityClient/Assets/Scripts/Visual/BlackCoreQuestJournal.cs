using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Real client quest journal backed by rAthena quest packets. It does not invent
/// progress: counts and active quests come from the map server.
/// </summary>
public class BlackCoreQuestJournal : MonoBehaviour {
    private class QuestState {
        public int Id;
        public byte State;
        public readonly List<QuestObjective> Objectives = new List<QuestObjective>();
    }

    private class QuestObjective {
        public int MobId;
        public string Name;
        public int Current;
        public int Total;
    }

    private static readonly Dictionary<int, string> Titles = new Dictionary<int, string> {
        { 90001, "Primeiro Pulso" },
        { 90002, "Eco do Abismo" }
    };

    private static readonly Dictionary<int, string> Descriptions = new Dictionary<int, string> {
        { 90001, "Estabilize os Campos de Nova Aurora e descubra por que as criaturas estão reagindo ao Núcleo." },
        { 90002, "Enfrente a ruptura nos Campos de Nova Aurora e derrote o Senhor do Abismo." }
    };

    private readonly Dictionary<int, QuestState> Quests = new Dictionary<int, QuestState>();
    private NetworkClient NetworkClient;
    private GameObject CanvasObject;
    private GameObject TrackerPanel;
    private GameObject JournalPanel;
    private Text TrackerText;
    private Text JournalText;
    private Text HeaderCounter;

    private void Start() {
        NetworkClient = FindObjectOfType<NetworkClient>();
        if (NetworkClient == null) return;

        NetworkClient.HookPacket(ZC.ALL_QUEST_LIST3.HEADER, OnQuestPacket);
        NetworkClient.HookPacket(ZC.ADD_QUEST_EX.HEADER, OnQuestPacket);
        NetworkClient.HookPacket(ZC.HUNTING_QUEST_INFO.HEADER, OnQuestPacket);
        NetworkClient.HookPacket(ZC.DEL_QUEST.HEADER, OnQuestPacket);

        SceneManager.activeSceneChanged += OnSceneChanged;
        BuildUI();
        OnSceneChanged(default(Scene), SceneManager.GetActiveScene());
    }

    private void OnDestroy() {
        SceneManager.activeSceneChanged -= OnSceneChanged;
        if (NetworkClient == null) return;
        NetworkClient.UnhookPacket(ZC.ALL_QUEST_LIST3.HEADER, OnQuestPacket);
        NetworkClient.UnhookPacket(ZC.ADD_QUEST_EX.HEADER, OnQuestPacket);
        NetworkClient.UnhookPacket(ZC.HUNTING_QUEST_INFO.HEADER, OnQuestPacket);
        NetworkClient.UnhookPacket(ZC.DEL_QUEST.HEADER, OnQuestPacket);
    }

    private void OnQuestPacket(ushort cmd, int size, InPacket packet) {
        if (packet is ZC.ALL_QUEST_LIST3 list) {
            Quests.Clear();
            foreach (var entry in list.Quests) {
                var state = new QuestState { Id = entry.QuestId, State = entry.State };
                foreach (var objective in entry.Objectives) {
                    state.Objectives.Add(new QuestObjective {
                        MobId = objective.MobId,
                        Name = ResolveObjectiveName(objective.Name),
                        Current = objective.Killed,
                        Total = objective.Total
                    });
                }
                Quests[state.Id] = state;
            }
        } else if (packet is ZC.ADD_QUEST_EX added) {
            var state = new QuestState { Id = added.QuestId, State = added.State };
            foreach (var objective in added.Objectives) {
                state.Objectives.Add(new QuestObjective {
                    MobId = objective.MobId,
                    Name = ResolveObjectiveName(objective.Name),
                    Current = objective.Killed,
                    Total = objective.Total
                });
            }
            Quests[state.Id] = state;
        } else if (packet is ZC.HUNTING_QUEST_INFO progress) {
            foreach (var update in progress.Updates) {
                if (!Quests.TryGetValue(update.QuestId, out var state)) continue;
                var objective = state.Objectives.FirstOrDefault(o => o.MobId == update.MobId);
                if (objective == null) continue;
                objective.Current = update.Current;
                objective.Total = update.Total;
            }
        } else if (packet is ZC.DEL_QUEST removed) {
            Quests.Remove(removed.QuestId);
        }

        RefreshUI();
    }

    private string ResolveObjectiveName(string raw) {
        if (string.IsNullOrWhiteSpace(raw)) return "Alvo desconhecido";
        return BlackCoreNameService.ResolveMonsterName(raw);
    }

    private void OnSceneChanged(Scene oldScene, Scene newScene) {
        if (CanvasObject == null) return;
        bool mapScene = newScene.name.IndexOf("Map", StringComparison.OrdinalIgnoreCase) >= 0;
        CanvasObject.SetActive(mapScene);
        if (mapScene) RefreshUI();
    }

    private void BuildUI() {
        CanvasObject = new GameObject("BlackCore_QuestUI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        CanvasObject.transform.SetParent(transform, false);
        var canvas = CanvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 15000;
        var scaler = CanvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.55f;

        bool mobileLayout = MobileHudController.ShouldShow;
        Vector2 trackerPosition = mobileLayout ? new Vector2(-28f, -185f) : new Vector2(-28f, -115f);
        Vector2 buttonPosition = mobileLayout ? new Vector2(-28f, -132f) : new Vector2(-28f, -68f);

        TrackerPanel = CreatePanel("QuestTracker", CanvasObject.transform, new Vector2(1f, 1f), trackerPosition, new Vector2(330f, 205f));
        var trackerTitle = CreateText(TrackerPanel.transform, "Title", "MISSÕES ATIVAS", 18, FontStyle.Bold, BlackCoreUiTheme.AccentAlt);
        SetRect(trackerTitle.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -12f), new Vector2(-24f, 30f));
        TrackerText = CreateText(TrackerPanel.transform, "Tracker", "Nenhuma missão ativa", 15, FontStyle.Normal, BlackCoreUiTheme.TextPrimary);
        TrackerText.alignment = TextAnchor.UpperLeft;
        SetRect(TrackerText.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(14f, 12f), new Vector2(-14f, -46f));

        var openButton = CreateButton(CanvasObject.transform, "JournalButton", "MISSÕES", ToggleJournal);
        var buttonRect = openButton.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(1f, 1f);
        buttonRect.anchorMax = new Vector2(1f, 1f);
        buttonRect.pivot = new Vector2(1f, 1f);
        buttonRect.anchoredPosition = buttonPosition;
        buttonRect.sizeDelta = new Vector2(150f, 38f);

        JournalPanel = CreatePanel("QuestJournal", CanvasObject.transform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(700f, 590f));
        var title = CreateText(JournalPanel.transform, "Title", "DIÁRIO DO NÚCLEO", 26, FontStyle.Bold, BlackCoreUiTheme.TextPrimary);
        SetRect(title.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -18f), new Vector2(-30f, 40f));

        HeaderCounter = CreateText(JournalPanel.transform, "Counter", "0 missões", 13, FontStyle.Normal, BlackCoreUiTheme.AccentAlt);
        SetRect(HeaderCounter.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -55f), new Vector2(-30f, 24f));

        JournalText = CreateText(JournalPanel.transform, "Entries", "", 16, FontStyle.Normal, BlackCoreUiTheme.TextPrimary);
        JournalText.alignment = TextAnchor.UpperLeft;
        SetRect(JournalText.rectTransform, Vector2.zero, Vector2.one, new Vector2(28f, 32f), new Vector2(-28f, -96f));

        var close = CreateButton(JournalPanel.transform, "Close", "FECHAR", ToggleJournal);
        var closeRect = close.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(1f, 0f);
        closeRect.anchorMax = new Vector2(1f, 0f);
        closeRect.pivot = new Vector2(1f, 0f);
        closeRect.anchoredPosition = new Vector2(-24f, 18f);
        closeRect.sizeDelta = new Vector2(130f, 38f);

        JournalPanel.SetActive(false);
    }

    private void ToggleJournal() {
        if (JournalPanel != null) JournalPanel.SetActive(!JournalPanel.activeSelf);
    }

    private void RefreshUI() {
        if (TrackerText == null || JournalText == null) return;
        var active = Quests.Values.Where(q => q.State != 2).OrderBy(q => q.Id).ToList();
        HeaderCounter.text = active.Count == 1 ? "1 missão ativa" : $"{active.Count} missões ativas";

        if (active.Count == 0) {
            TrackerText.text = "Nenhuma missão ativa.\nExplore o mundo e fale com os moradores.";
            JournalText.text = "<color=#65E8FF>Nenhuma transmissão de missão ativa.</color>\n\nQuando você aceitar uma missão, o progresso real do servidor aparecerá aqui.";
            return;
        }

        TrackerText.text = string.Join("\n\n", active.Take(2).Select(BuildTrackerEntry));
        JournalText.text = string.Join("\n\n<color=#35405C>────────────────────────────────</color>\n\n", active.Select(BuildJournalEntry));
    }

    private string BuildTrackerEntry(QuestState quest) {
        string title = GetTitle(quest.Id);
        string objective = quest.Objectives.Count == 0
            ? "• Continue a investigação"
            : string.Join("\n", quest.Objectives.Take(2).Select(o => $"• {o.Name}  <color=#65E8FF>{o.Current}/{o.Total}</color>"));
        return $"<b>{title}</b>\n{objective}";
    }

    private string BuildJournalEntry(QuestState quest) {
        string title = GetTitle(quest.Id);
        string desc = Descriptions.TryGetValue(quest.Id, out var description)
            ? description
            : "Uma transmissão ativa do Núcleo aguarda sua intervenção.";
        string objectives = quest.Objectives.Count == 0
            ? "<color=#AAB8D6>• Continue seguindo as instruções da missão.</color>"
            : string.Join("\n", quest.Objectives.Select(o => {
                bool done = o.Total > 0 && o.Current >= o.Total;
                string color = done ? "#70E6A1" : "#F1F4FF";
                string mark = done ? "✓" : "•";
                return $"<color={color}>{mark} {o.Name}: {o.Current}/{o.Total}</color>";
            }));
        return $"<size=20><b><color=#65E8FF>{title}</color></b></size>\n{desc}\n\n<b>OBJETIVOS</b>\n{objectives}";
    }

    private string GetTitle(int id) => Titles.TryGetValue(id, out var title) ? title : $"Missão do Núcleo #{id}";

    private GameObject CreatePanel(string name, Transform parent, Vector2 anchor, Vector2 position, Vector2 size) {
        var panel = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Outline));
        panel.transform.SetParent(parent, false);
        var rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = anchor;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        panel.GetComponent<Image>().color = new Color(BlackCoreUiTheme.Panel.r, BlackCoreUiTheme.Panel.g, BlackCoreUiTheme.Panel.b, 0.94f);
        var outline = panel.GetComponent<Outline>();
        outline.effectColor = new Color(BlackCoreUiTheme.AccentAlt.r, BlackCoreUiTheme.AccentAlt.g, BlackCoreUiTheme.AccentAlt.b, 0.38f);
        outline.effectDistance = new Vector2(1f, -1f);
        return panel;
    }

    private Text CreateText(Transform parent, string name, string value, int size, FontStyle style, Color color) {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        var text = go.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = size;
        text.fontStyle = style;
        text.color = color;
        text.supportRichText = true;
        text.text = value;
        return text;
    }

    private Button CreateButton(Transform parent, string name, string label, Action action) {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
        go.transform.SetParent(parent, false);
        go.GetComponent<Image>().color = new Color(0.12f, 0.17f, 0.28f, 0.96f);
        var outline = go.GetComponent<Outline>();
        outline.effectColor = new Color(0.40f, 0.91f, 1f, 0.55f);
        outline.effectDistance = new Vector2(1f, -1f);
        var button = go.GetComponent<Button>();
        button.onClick.AddListener(() => action());
        var text = CreateText(go.transform, "Label", label, 14, FontStyle.Bold, BlackCoreUiTheme.TextPrimary);
        text.alignment = TextAnchor.MiddleCenter;
        SetRect(text.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        return button;
    }

    private void SetRect(RectTransform rect, Vector2 min, Vector2 max, Vector2 offsetMin, Vector2 offsetMax) {
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
    }
}
