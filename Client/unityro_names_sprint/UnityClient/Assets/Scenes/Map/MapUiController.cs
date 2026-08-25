using UnityEngine;
using UnityEngine.SceneManagement;

public class MapUiController : MonoBehaviour {

    public static MapUiController Instance;

    [SerializeField] private Tooltip Tooltip;
    [SerializeField] private ItemDetailsWindow ItemDetailsPrefab;
    [SerializeField] private NpcBoxController NpcBox;
    [SerializeField] private NpcBoxMenuController NpcMenu;
    [SerializeField] private NpcShopController ShopController;
    [SerializeField] private PopupController PopupController;
    [SerializeField] public EquipmentWindowController EquipmentWindow;
    [SerializeField] public InventoryWindowController InventoryWindow;
    [SerializeField] public StatsWindowController StatsWindow;
    [SerializeField] public SkillWindowController SkillWindow;
    [SerializeField] public ChatBoxController ChatBox;
    [SerializeField] public NpcShopTypeSelectorController ShopDealType;
    [SerializeField] public EscapeWindow EscapeWindow;
    [SerializeField] public MenuController Menu;
    [SerializeField] public PacketLogWindow PacketLogWindow;

    private NetworkClient NetworkClient;

    void Awake() {
        if (Instance == null) {
            Instance = this;
        }

        NetworkClient = FindObjectOfType<NetworkClient>();

        NetworkClient.HookPacket(ZC.SAY_DIALOG.HEADER, NpcBox.OnNpcMessage);
        NetworkClient.HookPacket(ZC.CLOSE_DIALOG.HEADER, NpcBox.AddCloseButton);
        NetworkClient.HookPacket(ZC.WAIT_DIALOG.HEADER, NpcBox.AddNextButton);
        NetworkClient.HookPacket(ZC.CLOSE_SCRIPT.HEADER, NpcBox.CloseAndReset);
        NetworkClient.HookPacket(ZC.MENU_LIST.HEADER, NpcMenu.SetMenu);
        NetworkClient.HookPacket(ZC.SELECT_DEALTYPE.HEADER, ShopDealType.DisplayDealTypeSelector);
        NetworkClient.HookPacket(ZC.PC_PURCHASE_ITEMLIST.HEADER, ShopController.DisplayShop);
        NetworkClient.HookPacket(ZC.PC_SELL_ITEMLIST.HEADER, ShopController.DisplayShop);
        NetworkClient.HookPacket(ZC.PC_PURCHASE_RESULT.HEADER, ShopController.OnPurchaseResult);
        NetworkClient.HookPacket(ZC.PC_SELL_RESULT.HEADER, ShopController.OnSellResult);
        NetworkClient.HookPacket(ZC.RESTART_ACK.HEADER, OnRestartAnswer);

        NpcMenu.OnNpcMenuSelected = OnNpcMenuSelected;

        PacketLogWindow.Hide();

        var rootRect = transform as RectTransform;
        if (rootRect != null) {
            NetworkStatusOverlay.EnsureCreated(rootRect, NetworkClient);
            MobileHudController.EnsureCreated(rootRect, this);
        }
    }

    public void DisplayItemDetails(ItemInfo itemInfo, Vector2 position) {
        var details = Instantiate(ItemDetailsPrefab);
        details.SetItem(itemInfo);
        details.transform.position = position;
        details.transform.SetParent(gameObject.transform);
    }

    private void Update() {
        bool alt = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
        if (alt && Input.GetKeyDown(KeyCode.Q)) {
            BlackCoreWindowMotion.Toggle(EquipmentWindow);
        }
        if (alt && Input.GetKeyDown(KeyCode.E)) {
            BlackCoreWindowMotion.Toggle(InventoryWindow);
        }
        if (alt && Input.GetKeyDown(KeyCode.A)) {
            BlackCoreWindowMotion.Toggle(StatsWindow);
        }
        if (alt && Input.GetKeyDown(KeyCode.S)) {
            BlackCoreWindowMotion.Toggle(SkillWindow);
        }
        if (Input.GetKeyDown(KeyCode.Escape)) {
            BlackCoreWindowMotion.Toggle(EscapeWindow);
        }
    }

    void OnNpcMenuSelected(uint NAID, byte index) {
        if (index == 255) {
            NpcBox.gameObject.SetActive(false);
        }

        new CZ.CHOOSE_MENU() {
            NAID = NAID,
            Index = index
        }.Send();
    }

    public void DisplayPopup(Texture2D itemRes, string label) {
        PopupController.DisplayPopup(itemRes, label);
    }

    public void UpdateEquipment() {
        EquipmentWindow.UpdateEquipment();
        InventoryWindow.UpdateEquipment();
    }

    public void DisplayTooltip(string text, Vector3 position) {
        Tooltip.SetText(text, position);
    }

    public void HideTooltip() {
        Tooltip.SetText(null, Vector3.zero);
    }

    public void OnMenuClick(int itemType) {
        var menuItemType = (MenuController.MenuItemType) itemType;
        switch (menuItemType) {
            case MenuController.MenuItemType.STATUS:
                BlackCoreWindowMotion.Toggle(StatsWindow);
                break;
            case MenuController.MenuItemType.EQUIPMENT:
                BlackCoreWindowMotion.Toggle(EquipmentWindow);
                break;
            case MenuController.MenuItemType.SKILL:
                BlackCoreWindowMotion.Toggle(SkillWindow);
                break;
            case MenuController.MenuItemType.OPTIONS:
                BlackCoreWindowMotion.Toggle(EscapeWindow);
                break;
            case MenuController.MenuItemType.INVENTORY:
                BlackCoreWindowMotion.Toggle(InventoryWindow);
                break;
            default:
                break;
        }
    }

    public void OnRestartAnswer(ushort cmd, int size, InPacket packet) {
        if (packet is ZC.RESTART_ACK pkt) {
            if (pkt.type == 0) {
                ChatBox.DisplayMessage(502, ChatMessageType.ERROR);
            }
            else {
                // @todo ?
                // clear StatusIcons
                // clear ChatBox
                // clear ShortCut
                // clear PartyFriends
                // clear renderers
                OnRestart();
            }
        }
    }

    public void OnRestart() {
        // @todo this keeps the entire UI on the screen
        // SceneManager.LoadSceneAsync("CharSelectionScene");
    }
    private void OnDestroy() {
        if (NetworkClient == null) return;

        if (NpcBox != null) {
            NetworkClient.UnhookPacket(ZC.SAY_DIALOG.HEADER, NpcBox.OnNpcMessage);
            NetworkClient.UnhookPacket(ZC.CLOSE_DIALOG.HEADER, NpcBox.AddCloseButton);
            NetworkClient.UnhookPacket(ZC.WAIT_DIALOG.HEADER, NpcBox.AddNextButton);
            NetworkClient.UnhookPacket(ZC.CLOSE_SCRIPT.HEADER, NpcBox.CloseAndReset);
        }
        if (NpcMenu != null) {
            NetworkClient.UnhookPacket(ZC.MENU_LIST.HEADER, NpcMenu.SetMenu);
        }
        if (ShopDealType != null) {
            NetworkClient.UnhookPacket(ZC.SELECT_DEALTYPE.HEADER, ShopDealType.DisplayDealTypeSelector);
        }
        if (ShopController != null) {
            NetworkClient.UnhookPacket(ZC.PC_PURCHASE_ITEMLIST.HEADER, ShopController.DisplayShop);
            NetworkClient.UnhookPacket(ZC.PC_SELL_ITEMLIST.HEADER, ShopController.DisplayShop);
            NetworkClient.UnhookPacket(ZC.PC_PURCHASE_RESULT.HEADER, ShopController.OnPurchaseResult);
            NetworkClient.UnhookPacket(ZC.PC_SELL_RESULT.HEADER, ShopController.OnSellResult);
        }
        NetworkClient.UnhookPacket(ZC.RESTART_ACK.HEADER, OnRestartAnswer);

        if (Instance == this) {
            Instance = null;
        }
    }

}
