using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NpcBoxController : DraggableUIWindow {

    enum ButtonAction {
        NEXT, CLOSE
    }

    [SerializeField] private Text NpcText;
    [SerializeField] private Button nextCloseButton;

    private uint NAID;
    private ButtonAction CurrentAction;

    // Start is called before the first frame update
    void Start() {
        BlackCoreWindowDecorator.TranslateCommonLabels(gameObject);
        BlackCoreWindowDecorator.Apply(gameObject, "Transmissão do Núcleo", "Diálogo, missão e escolhas");
        if (NpcText != null) NpcText.supportRichText = true;
    }

    // Update is called once per frame
    void Update() {

    }

    public void SetText(string message, uint nAID) {
        this.NAID = nAID;
        NpcText.text += $"{BlackCoreQuestPresentation.Format(message)}\n";
    }

    public void CloseAndReset(ushort cmd, int size, InPacket packet) {
        if (!(packet is ZC.CLOSE_SCRIPT)) {
            return;
        }

        var pkt = packet as ZC.CLOSE_SCRIPT;
        NAID = pkt.NAID;
        CurrentAction = ButtonAction.CLOSE;
        NpcText.text = "";
        nextCloseButton.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }

    public void AddNextButton(ushort cmd, int size, InPacket packet) {
        if (packet is ZC.WAIT_DIALOG) {
            var pkt = packet as ZC.WAIT_DIALOG;

            NAID = pkt.NAID;
            CurrentAction = ButtonAction.NEXT;
            nextCloseButton.gameObject.SetActive(true);
            nextCloseButton.GetComponentInChildren<TextMeshProUGUI>().text = "Continuar";
        }
    }

    public void AddCloseButton(ushort cmd, int size, InPacket packet) {
        if (packet is ZC.CLOSE_DIALOG) {
            var pkt = packet as ZC.CLOSE_DIALOG;

            NAID = pkt.NAID;
            CurrentAction = ButtonAction.CLOSE;
            nextCloseButton.gameObject.SetActive(true);
            nextCloseButton.GetComponentInChildren<TextMeshProUGUI>().text = "Fechar";
        }
    }

    public void OnNpcMessage(ushort cmd, int size, InPacket packet) {
        if (packet is ZC.SAY_DIALOG) {
            var pkt = packet as ZC.SAY_DIALOG;

            gameObject.SetActive(true);
            SetText(pkt.Message, pkt.NAID);
        }
    }


    public void OnButtonClick() {
        switch (CurrentAction) {
            case ButtonAction.NEXT:
                new CZ.REQ_NEXT_SCRIPT() {
                    NAID = this.NAID
                }.Send();

                nextCloseButton.gameObject.SetActive(false);
                NpcText.text = "";

                break;
            case ButtonAction.CLOSE:
                new CZ.CLOSE_DIALOG() {
                    NAID = this.NAID
                }.Send();

                nextCloseButton.gameObject.SetActive(false);
                NpcText.text = "";

                gameObject.SetActive(false);
                break;
        }
    }
}
