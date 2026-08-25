using ROIO;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class CharCreationController : MonoBehaviour {

    private const int HUMAN_MAX_HAIRSTYLE = 23;
    private const int JOB_NOVICE = 0;
    private const int JOB_SWORDMAN = 1;
    private const int JOB_MAGE = 2;
    private const int JOB_ARCHER = 3;
    private const int JOB_ACOLYTE = 4;
    private const int JOB_MERCHANT = 5;
    private const int JOB_THIEF = 6;

    public Entity StyleEntity;
    public Entity HumanSelectionEntity;
    public Entity DoramSelecionEntity;
    public TMP_InputField CharacterName;

    public GridLayoutGroup GridLayout;
    public ToggleGroup HairToggleGroup;
    public ToggleGroup SexToggleGroup;

    public RawImage background;

    private bool IsDirty = false;
    private List<ToggleImage> HairToggleList;
    private NetworkClient NetworkClient;

    private int SelectedSex = 1;
    private int SelectedHair = 1;
    private int SelectedHairColor = 0;
    private int SelectedStartJob = JOB_SWORDMAN;
    private Text PathTitle;
    private Text PathDescription;
    private readonly List<Button> PathButtons = new List<Button>();

    private class PathDefinition {
        public int JobId;
        public string Name;
        public string Role;
        public string Description;
    }

    private static readonly PathDefinition[] Paths = {
        new PathDefinition { JobId = JOB_SWORDMAN, Name = "Combatente", Role = "Guerreiro", Description = "Resistência e combate corpo a corpo. Ideal para segurar a linha de frente." },
        new PathDefinition { JobId = JOB_ARCHER, Name = "Atirador", Role = "Arqueiro", Description = "Precisão e mobilidade. Ataca de longe e controla o ritmo da batalha." },
        new PathDefinition { JobId = JOB_MAGE, Name = "Arcano", Role = "Arcanista", Description = "Poder elemental e dano explosivo. Forte, técnico e perigoso." },
        new PathDefinition { JobId = JOB_ACOLYTE, Name = "Devoto", Role = "Devoto", Description = "Cura, proteção e poder sagrado para sustentar aliados." },
        new PathDefinition { JobId = JOB_THIEF, Name = "Sombra", Role = "Ladino", Description = "Velocidade, evasão e golpes críticos para quem gosta de risco." },
        new PathDefinition { JobId = JOB_MERCHANT, Name = "Artesão", Role = "Mercador", Description = "Economia, utilidade e evolução ligada a comércio e criação." }
    };

    void Start() {
        background.SetLoginBackground();
        if (background != null) background.color = new Color(0.86f, 0.92f, 1f, 1f);
        NetworkClient = FindObjectOfType<NetworkClient>();

        InitEntity(StyleEntity, sex: SelectedSex, job: SelectedStartJob);
        InitEntity(HumanSelectionEntity, sex: SelectedSex, job: SelectedStartJob);
        if (DoramSelecionEntity != null) {
            InitEntity(DoramSelecionEntity, sex: SelectedSex, job: JOB_NOVICE);
            DoramSelecionEntity.gameObject.SetActive(false);
        }
        if (HumanSelectionEntity != null) HumanSelectionEntity.gameObject.SetActive(false);

        HairToggleList = GridLayout.GetComponentsInChildren<ToggleImage>().ToList();

        if (CharacterName != null) {
            if (CharacterName.placeholder is TMP_Text placeholder) {
                placeholder.text = "Seu nome no Núcleo...";
            }

            if (string.IsNullOrWhiteSpace(CharacterName.text)) {
                CharacterName.text = BlackCoreNameService.SuggestPlayerName();
                CharacterName.caretPosition = CharacterName.text.Length;
            }
        }

        ApplyLegacyLabelCleanup();
        BuildPathSelectionUi();
        SelectPath(SelectedStartJob);
    }

    void Update() {
        if (!IsDirty) {
            StyleEntity.ChangeMotion(new MotionRequest { Motion = SpriteMotion.Idle });
            if (HairToggleList != null && HairToggleList.Count > 0) HairToggleList[0].Toggle.isOn = true;
            var activeSex = SexToggleGroup != null ? SexToggleGroup.ActiveToggles().FirstOrDefault() : null;
            if (activeSex != null) activeSex.isOn = true;
            SetHairstyles();
            IsDirty = true;
        }
    }

    // Kept for prefab compatibility. Legacy race buttons now map to the Black Core default path.
    public void SetRace(bool isHuman) {
        SelectPath(isHuman ? JOB_SWORDMAN : JOB_MAGE);
    }

    public void SetSex(int sex) {
        SelectedSex = sex;
        SetHairstyles();
        UpdateEntity(StyleEntity, sex);
    }

    public void SuggestCharacterName() {
        if (CharacterName == null) return;
        CharacterName.text = BlackCoreNameService.SuggestPlayerName();
        CharacterName.caretPosition = CharacterName.text.Length;
        CharacterName.ActivateInputField();
    }

    public void SelectPath(int jobId) {
        var path = Paths.FirstOrDefault(p => p.JobId == jobId) ?? Paths[0];
        SelectedStartJob = path.JobId;

        if (StyleEntity != null) {
            StyleEntity.Status.jobId = (short) SelectedStartJob;
            StyleEntity.UpdateSprites();
            StyleEntity.ChangeMotion(new MotionRequest { Motion = SpriteMotion.Idle });
        }

        if (PathTitle != null) PathTitle.text = $"{path.Name}  •  {path.Role}";
        if (PathDescription != null) PathDescription.text = path.Description;

        for (int i = 0; i < PathButtons.Count; i++) {
            var image = PathButtons[i].GetComponent<Image>();
            if (image == null) continue;
            bool selected = Paths[i].JobId == SelectedStartJob;
            image.color = selected
                ? new Color(0.18f, 0.38f, 0.52f, 0.96f)
                : new Color(0.06f, 0.09f, 0.16f, 0.90f);
        }
    }

    public void CreateCharacter() {
        var name = CharacterName.text.Trim();
        if (name.Length < 4) return;

        new CH.MAKE_CHAR2() {
            Name = name,
            CharNum = (byte) NetworkClient.State.CurrentCharactersInfo.Chars.Count,
            Sex = (byte) SelectedSex,
            Head = (ushort) SelectedHair,
            HeadPal = (ushort) SelectedHairColor,
            StartJob = SelectedStartJob
        }.Send();
    }

    public void CloseWindow() {
        SceneManager.UnloadSceneAsync(6);
    }

    private void InitEntity(Entity entity, int sex = 1, int job = 0) {
        if (entity == null) return;
        entity.Init(new CharacterData() { Sex = sex, Job = (short) job, Name = "Viajante", GID = 20001, Weapon = 1, Speed = 150, Head = 1 }, LayerMask.NameToLayer("Characters"), null, true);
        entity.SortingGroup.sortingOrder = 3;
        entity.SetReady(true, true);
    }

    private void UpdateEntity(Entity entity, int sex = 1, int hair = 1, int color = 1) {
        if (entity == null) return;
        entity.Status.sex = (byte) sex;
        entity.Status.hair = (short) hair;
        entity.Status.hair_color = (short) color;
        entity.UpdateSprites();
    }

    private void SetHairstyles() {
        if (HairToggleList == null) return;
        HairToggleList.ForEach(it => it.SetImage(null, -1));
        for (int i = 1; i <= HUMAN_MAX_HAIRSTYLE && i <= HairToggleList.Count; i++) {
            var hairstylePath = "make_character_ver2/img_hairstyle";
            var sexPath = SelectedSex == 1 ? "" : "_girl";
            hairstylePath += $"{sexPath}{i.ToString("D2")}";

            var index = i - 1;
            var toggle = HairToggleList[index];
            toggle.SetImage(hairstylePath + ".png", index);
            toggle.onValueChanged.RemoveListener(OnHairToggleChanged);
            toggle.onValueChanged.AddListener(OnHairToggleChanged);
        }
    }

    private void OnHairToggleChanged(int index) {
        SelectedHair = index + 1;
        UpdateEntity(StyleEntity, SelectedSex, SelectedHair, SelectedHairColor);
    }

    private void ApplyLegacyLabelCleanup() {
        foreach (var text in GetComponentsInChildren<TMP_Text>(true)) {
            if (text == null) continue;
            switch (text.text.Trim()) {
                case "Character Creation": text.text = "Crie seu Viajante"; break;
                case "Hair Style": text.text = "Cabelo"; break;
                case "Hair Color": text.text = "Cor do cabelo"; break;
                case "Human": text.text = "Caminhos"; break;
                case "Doram": text.text = "Ascensão"; break;
                case "Create": text.text = "Entrar no Núcleo"; break;
                case "Enter text...": text.text = "Seu nome no Núcleo..."; break;
            }
        }
    }

    private void BuildPathSelectionUi() {
        var rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas == null || rootCanvas.transform.Find("BlackCore_PathSelection") != null) return;

        var root = new GameObject("BlackCore_PathSelection", typeof(RectTransform), typeof(Image), typeof(Outline));
        root.transform.SetParent(rootCanvas.transform, false);
        var rect = root.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0.5f);
        rect.anchorMax = new Vector2(0f, 0.5f);
        rect.pivot = new Vector2(0f, 0.5f);
        rect.anchoredPosition = new Vector2(24f, 0f);
        rect.sizeDelta = new Vector2(280f, 500f);

        root.GetComponent<Image>().color = new Color(0.035f, 0.055f, 0.105f, 0.94f);
        var outline = root.GetComponent<Outline>();
        outline.effectColor = new Color(0.26f, 0.88f, 1f, 0.38f);
        outline.effectDistance = new Vector2(1.5f, -1.5f);

        CreateText(root.transform, "Heading", new Vector2(14f, -16f), new Vector2(252f, 34f), 19, FontStyle.Bold, TextAnchor.MiddleLeft, "ESCOLHA SEU CAMINHO", new Color(0.76f, 0.94f, 1f));
        CreateText(root.transform, "Hint", new Vector2(14f, -52f), new Vector2(252f, 36f), 11, FontStyle.Normal, TextAnchor.UpperLeft, "A escolha define sua classe inicial e o preview real do personagem.", new Color(0.72f, 0.78f, 0.90f));

        for (int i = 0; i < Paths.Length; i++) {
            int capturedJob = Paths[i].JobId;
            var button = CreatePathButton(root.transform, Paths[i], i);
            button.onClick.AddListener(() => SelectPath(capturedJob));
            PathButtons.Add(button);
        }

        PathTitle = CreateText(root.transform, "PathTitle", new Vector2(14f, -398f), new Vector2(252f, 28f), 15, FontStyle.Bold, TextAnchor.MiddleLeft, "", new Color(0.82f, 0.95f, 1f));
        PathDescription = CreateText(root.transform, "PathDescription", new Vector2(14f, -428f), new Vector2(252f, 58f), 11, FontStyle.Normal, TextAnchor.UpperLeft, "", new Color(0.74f, 0.81f, 0.92f));
    }

    private Button CreatePathButton(Transform parent, PathDefinition path, int index) {
        var go = new GameObject($"Path_{path.Name}", typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(14f, -96f - (index * 49f));
        rect.sizeDelta = new Vector2(252f, 42f);

        var image = go.GetComponent<Image>();
        image.color = new Color(0.06f, 0.09f, 0.16f, 0.90f);
        var button = go.GetComponent<Button>();
        button.targetGraphic = image;

        CreateText(go.transform, "Label", new Vector2(12f, -4f), new Vector2(228f, 20f), 14, FontStyle.Bold, TextAnchor.MiddleLeft, path.Name, Color.white);
        CreateText(go.transform, "Role", new Vector2(12f, -22f), new Vector2(228f, 16f), 10, FontStyle.Normal, TextAnchor.MiddleLeft, path.Role, new Color(0.55f, 0.88f, 1f));
        return button;
    }

    private Text CreateText(Transform parent, string name, Vector2 position, Vector2 size, int fontSize,
        FontStyle style, TextAnchor alignment, string content, Color color) {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        var text = go.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.alignment = alignment;
        text.color = color;
        text.text = content;
        return text;
    }
}
