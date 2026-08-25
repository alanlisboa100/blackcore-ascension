using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterCellController : MonoBehaviour, IPointerClickHandler {

    private CharacterData data;

    public Text characterName;

    public bool IsEmpty => data == null;

    public Action<CharacterData> OnCharacterSelected;

    public void BindData(CharacterData data) {
        this.data = data;

        characterName.text = data.Name;
        characterName.fontStyle = FontStyle.Bold;
        characterName.color = BlackCoreUiTheme.TextPrimary;
        AddCharacterMeta(data);

        GameObject player = new GameObject(data.Name);
        player.layer = LayerMask.NameToLayer("Characters");
        player.transform.SetParent(this.transform);
        player.transform.localScale = new Vector3(30f, 30f, 1f);
        player.transform.localPosition = new Vector3(0, -40f, 0f);

        Entity entity = player.AddComponent<Entity>();
        entity.Init(data, LayerMask.NameToLayer("Characters"), null, true);
        entity.SetReady(true, true);
    }

    private void AddCharacterMeta(CharacterData character) {
        if (transform.Find("BlackCore_CharacterMeta") != null) return;

        var metaObject = new GameObject("BlackCore_CharacterMeta", typeof(RectTransform), typeof(Text));
        metaObject.transform.SetParent(transform, false);
        var rect = metaObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(0f, 8f);
        rect.sizeDelta = new Vector2(180f, 34f);

        var text = metaObject.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = 11;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = BlackCoreUiTheme.AccentAlt;
        var rawJob = JobHelper.GetJobName(character.Job, character.Sex);
        var job = BlackCoreLoreService.ResolveJobName(rawJob, character.Job, character.Sex);
        text.text = $"{job} • Nv. {character.Level}\n{BlackCoreLoreService.ResolveMapName(character.MapName)}";
        text.raycastTarget = false;
    }

    public void OnPointerClick(PointerEventData eventData) {
        if (eventData.button == PointerEventData.InputButton.Left) {
            OnCharacterSelected?.Invoke(data);
        }
    }
}
