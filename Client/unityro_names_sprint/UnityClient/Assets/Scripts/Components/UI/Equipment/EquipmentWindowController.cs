using UnityEngine;

public class EquipmentWindowController : DraggableUIWindow {

    [SerializeField] private NormalEquipmentWindow NormalWindow;

    // Start is called before the first frame update
    void Start() {
        BlackCoreWindowDecorator.TranslateCommonLabels(gameObject);
        BlackCoreWindowDecorator.Apply(gameObject, "Arsenal", "Equipamentos ativos do personagem");
        BlackCoreItemWindowPolish.Apply(gameObject);
    }

    // Update is called once per frame
    void Update() {

    }

    public void UpdateEquipment() {
        NormalWindow.UpdateEquipment();
    }

    public void EquipAmmo(ItemInfo item) {
        NormalWindow.EquipAmmo(item);
    }

    internal void UnequipAmmo() {
        NormalWindow.UnequipAmmo();
    }
}
