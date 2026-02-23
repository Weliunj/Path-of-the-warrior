using UnityEngine;

public class PlayerAtr : MonoBehaviour
{
    // Singleton instance
    public static PlayerAtr Instance { get; private set; }

    public InventoryObject inventory;
    public InventoryObject equipment;
    public Atribute[] attributes;

    public float interactionDistance = 3f;
    public LayerMask interactableLayer;
    public RectTransform Crosshair;
    public float animationSpeed = 10f; // Tăng tốc độ này lên để thấy hiệu ứng mượt hơn

    [Header("Item Dropping")]
    public GameObject itemWorldPrefab; // Kéo ItemWorld prefab vào đây trong Inspector
    public float dropDistance = 1.5f; // Khoảng cách thả vật phẩm trước mặt người chơi

    private void Start()
    {
        for (int i = 0; i < attributes.Length; i++)
        {
            attributes[i].SetParent(this);
        }
        for (int i = 0; i < equipment.GetSlots.Length; i++)
        {
            equipment.GetSlots[i].OnBeforeUpdate += OnBeforeSlotUpdate;
            equipment.GetSlots[i].OnAfterUpdate += OnAfterSlotUpdate;
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void OnBeforeSlotUpdate(InventorySlot _slot)
    {
        if(_slot.ItemObject == null) return;
        switch (_slot.parent.inventory.type)
        {
            case InterfaceType.Inventory:
                break;
            case InterfaceType.Equipment:
                print(string.Concat("Removed ", _slot.ItemObject, " on ", _slot.parent.inventory.type, ", Allowed Items: ", string.Join(", ", _slot.AllowedItems)));

                for (int i = 0; i < _slot.item.buffs.Length; i++)
                {
                    for (int j = 0; j < attributes.Length; j++)
                    {
                        if(attributes[j].type == _slot.item.buffs[i].atribute)
                            attributes[j].value.RemoveModifier(_slot.item.buffs[i]);
                    }
                }
                break;
            case InterfaceType.Chest:
                break;
            default:
                break;
        }
    }
    public void OnAfterSlotUpdate(InventorySlot _slot)
    {
        if(_slot.ItemObject == null) return;
        switch (_slot.parent.inventory.type)
        {
            case InterfaceType.Inventory:
                break;
            case InterfaceType.Equipment:
                print(string.Concat("Placed ", _slot.ItemObject, " on ", _slot.parent.inventory.type, ", Allowed Items: ", string.Join(", ", _slot.AllowedItems)));
                
                for (int i = 0; i < _slot.item.buffs.Length; i++)
                {
                    for (int j = 0; j < attributes.Length; j++)
                    {
                        if(attributes[j].type == _slot.item.buffs[i].atribute)
                            attributes[j].value.AddModifier(_slot.item.buffs[i]);
                    }
                }
                break;
            case InterfaceType.Chest:
                break;
            default:
                break;
        }
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            inventory.Save();
            equipment.Save();
        }
        else if (Input.GetKeyDown(KeyCode.L))
        {
            inventory.Load();
            equipment.Load();
        }
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        // Vẽ tia Ray trong cửa sổ Scene để bạn dễ debug (chỉ thấy khi đang chạy game)
        Debug.DrawRay(ray.origin, ray.direction * interactionDistance, Color.green);

        // 2. Kiểm tra va chạm tia Ray
        if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayer))
        {
            var _item = hit.collider.GetComponent<ItemWorld>();

            if (_item != null)
            {
                // Gọi hiệu ứng phóng to
                ScaleCrosshair(1.1f);

                if (Input.GetKeyDown(KeyCode.E))
                {
                    bool added = inventory.AddItem(new Item(_item.item), 1);
                    if (added)
                    {
                        if (hit.collider != null)
                            hit.collider.gameObject.SetActive(false);
                    }
                    else
                    {
                        Debug.Log("Inventory is full!"); // TODO: show UI feedback
                    }
                }
            }
            else
            {
                // Có chạm nhưng không phải Item
                ScaleCrosshair(0.6f);
            }
        }
        else
        {
            // KHÔNG chạm vào bất cứ thứ gì thuộc interactableLayer
            ScaleCrosshair(0.6f);
        }

        // Thêm chức năng hủy vật phẩm khi nhấn phím 'X' và di chuột qua một slot trong UI
        if (Input.GetKeyDown(KeyCode.X))
        {
            // Kiểm tra xem có slot nào đang được di chuột qua và có giao diện UI đang hoạt động không
            if (MouseData.slotHoveredOver != null && MouseData.interfaceMouseIsOver != null)
            {
                // Lấy InventorySlot tương ứng với GameObject của slot đang được di chuột qua
                if (MouseData.interfaceMouseIsOver.slotsOnInterface.TryGetValue(MouseData.slotHoveredOver, out InventorySlot hoveredSlot))
                {
                    if (hoveredSlot.item.Id >= 0) // Đảm bảo slot có vật phẩm
                    {
                        hoveredSlot.RemoveItem(); // Hủy vật phẩm khỏi kho đồ
                        Debug.Log($"Đã hủy vật phẩm: {hoveredSlot.ItemObject.name}");
                        Ui.Instance?.SetItemDescription(""); // Xóa mô tả ngay lập tức
                    }
                }
            }
        }
    }

    // Thả vật phẩm vào thế giới với số lượng cụ thể (tạo 1 ItemWorld chứa `amount`)
    public void DropItem(ItemObject itemToDrop, int amountToDrop)
    {
        if (itemToDrop == null || itemWorldPrefab == null)
        {
            Debug.LogWarning("Không thể thả vật phẩm: ItemObject là null hoặc itemWorldPrefab chưa được gán.");
            return;
        }
        if (amountToDrop <= 0) return;

        // Tính toán vị trí thả trước mặt người chơi
        Vector3 dropPosition = transform.position + transform.forward * dropDistance + new Vector3(UnityEngine.Random.Range(-0.2f, 0.2f), 0.5f, UnityEngine.Random.Range(-0.2f, 0.2f));

        GameObject droppedItemGO = Instantiate(itemWorldPrefab, dropPosition, Quaternion.identity);
        ItemWorld itemWorldComponent = droppedItemGO.GetComponent<ItemWorld>();

        if (itemWorldComponent != null)
        {
            itemWorldComponent.Initialize(itemToDrop, amountToDrop);
            droppedItemGO.SetActive(true);
            Debug.Log($"Đã thả {amountToDrop} x {itemToDrop.name} tại {dropPosition}");
        }
        else
        {
            Debug.LogError("Prefab vật phẩm thả không có component ItemWorld!");
            Destroy(droppedItemGO);
        }
    }

    private void ScaleCrosshair(float targetScale)
    {
        Vector3 target = new Vector3(targetScale, targetScale, targetScale);
        // Sử dụng tốc độ cao hơn (ví dụ: 10f) để thấy rõ sự thay đổi
        Crosshair.localScale = Vector3.Lerp(Crosshair.localScale, target, animationSpeed * Time.deltaTime);
    }

    public void AtributeModified(Atribute atribute)
    {
        Debug.Log(string.Concat(atribute.type, " was updated! Value is now ", atribute.value.ModifiedValue));
    }
    
    private void OnApplicationQuit()    // Được gọi tự động khi bạn nhấn Stop (tắt game) hoặc thoát ứng dụng
    {
        inventory.Clear();
        equipment.Clear();
    }

}

[System.Serializable]
public class Atribute
{
    [System.NonSerialized]
    public PlayerAtr parent;
    public Atributes type;
    public ModifiableInt value;
    public void SetParent(PlayerAtr _parent)
    {
        parent = _parent;
        value = new ModifiableInt(AtributeModified);
    }
    public void AtributeModified()
    {
        if (parent != null)
            parent.AtributeModified(this);
    }
}
