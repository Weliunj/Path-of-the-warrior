using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using UnityEngine.Events;

public abstract class UserInterface : MonoBehaviour
{
    [Header("Cấu hình UI")]
    public InventoryObject inventory;  // File dữ liệu rương đồ (ScriptableObject)
    
    [Header("Trạng thái")]

    // Dictionary giúp liên kết Dữ liệu (Slot) với Đối tượng hiển thị (GameObject)
    // Giúp cập nhật số lượng cực nhanh mà không cần tạo lại toàn bộ UI
    public Dictionary<GameObject, InventorySlot> slotsOnInterface = new Dictionary<GameObject, InventorySlot>();

    void Start()
    {
        for (int i = 0; i < inventory.GetSlots.Length; i++)
        {
            inventory.GetSlots[i].parent  = this;
            inventory.GetSlots[i].OnAfterUpdate += OnSlotUpdate;
        }
        // Khi game bắt đầu, vẽ rương đồ dựa trên dữ liệu có sẵn
        CreateSlot(); 
        AddEvent(gameObject, EventTriggerType.PointerEnter, delegate { OnEnterInterface(gameObject);});
        AddEvent(gameObject, EventTriggerType.PointerExit, delegate { OnExitInterface(gameObject);});
    }

    private void OnSlotUpdate(InventorySlot _slot)
    {
        if(_slot.item.Id >= 0) // Nếu ô có vật phẩm
            {
                // 1. Hiển thị hình ảnh từ Database dựa trên ID
                _slot.slotDisplay.transform.GetChild(0).GetComponentInChildren<Image>().sprite = _slot.ItemObject.uiDisplay;
                // 2. Hiện màu sắc (Alpha = 1)
                _slot.slotDisplay.transform.GetChild(0).GetComponentInChildren<Image>().color = new Color(1,1,1,1);
                // 3. Hiện số lượng (nếu = 1 thì ẩn chữ số đi cho đẹp)
                _slot.slotDisplay.GetComponentInChildren<TextMeshProUGUI>().text = _slot.amount == 1 ? "" : _slot.amount.ToString("n0");
            }
            else // Nếu ô trống (ID = -1)
            {
                // Ẩn hình ảnh và làm mờ màu sắc (Alpha = 0)
                _slot.slotDisplay.transform.GetChild(0).GetComponentInChildren<Image>().color = new Color(1,1,1,0);
                _slot.slotDisplay.GetComponentInChildren<TextMeshProUGUI>().text = "";
            }
    }

    // void Update()
    // {
    //     slotsOnInterface.UpdateSlotDisplay();
    // }

    // Hàm khởi tạo giao diện rương đồ lần đầu
    public abstract void CreateSlot();

    protected void  AddEvent(GameObject obj, EventTriggerType type, UnityAction<BaseEventData> action)
    {
        EventTrigger trigger = obj.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = obj.AddComponent<EventTrigger>();
        }
        var eventTrigger = new EventTrigger.Entry();
        eventTrigger.eventID = type;
        eventTrigger.callback.AddListener(action);
        trigger.triggers.Add(eventTrigger);
    }

    private void SetDescriptionText(string text) // Phương thức này giờ sẽ gọi Ui.Instance
    {
        Ui.Instance?.SetItemDescription(text); // Sử dụng instance Singleton của Ui để cập nhật text
    }
    public void OnEnter(GameObject obj)
    {
        // Hiển thị mô tả khi chuột di vào slot
        if (slotsOnInterface.ContainsKey(obj) && slotsOnInterface[obj].item.Id >= 0)
        {
            SetDescriptionText(slotsOnInterface[obj].ItemObject.description);
        }
        MouseData.slotHoveredOver = obj;
    }
    public void OnExit(GameObject obj)
    {
        // Xóa mô tả khi chuột rời khỏi slot
        SetDescriptionText("");
        MouseData.slotHoveredOver = null;
    }
    public void OnClick(GameObject obj)
    {
        // Hiển thị mô tả khi click vào slot
        // Nếu đã có mô tả từ OnEnter/OnDragStart, việc click sẽ giữ nguyên mô tả đó.
        // Nếu không, nó sẽ hiển thị mô tả mới.
        if (slotsOnInterface.ContainsKey(obj) && slotsOnInterface[obj].item.Id >= 0)
        {
            SetDescriptionText(slotsOnInterface[obj].ItemObject.description);
        }
        else
        {
            SetDescriptionText(""); // Nếu click vào slot trống, xóa mô tả
        }
    }

    // New: handle pointer click event so we can detect right-clicks
    public void OnPointerClick(GameObject obj, BaseEventData data)
    {
        // Show description as before
        OnClick(obj);

        // Only proceed if slot has an item
        if (!slotsOnInterface.ContainsKey(obj) || slotsOnInterface[obj].item.Id < 0) return;

        var ped = data as PointerEventData;
        if (ped == null) return;

        // Right mouse button = use/consume
        if (ped.button == PointerEventData.InputButton.Right)
        {
            InventorySlot slot = slotsOnInterface[obj];
            ItemObject itemObj = slot.ItemObject;
            if (itemObj == null) return;

            // Only consume Food type here
            if (itemObj.type == ItemType.Food)
            {
                // Apply buffs from the Item (simple behaviour)
                for (int i = 0; i < slot.item.buffs.Length; i++)
                {
                    var buff = slot.item.buffs[i];
                    // Health buff
                    if (buff.atribute == Atributes.Health)
                    {
                        var pm = FindObjectOfType<PlayerHealth>();
                        if (pm != null && pm.manager != null)
                        {
                            pm.manager.currentHealth = Mathf.Min(pm.manager.maxHealth, pm.manager.currentHealth + buff.value);
                        }
                    }
                    // Stamina buff
                    else if (buff.atribute == Atributes.Stamina)
                    {
                        var pm = FindObjectOfType<PlayerHealth>();
                        if (pm != null && pm.manager != null)
                        {
                            pm.manager.currentStamina = Mathf.Min(pm.manager.maxStamina, pm.manager.currentStamina + buff.value);
                        }
                    }
                    // (Other buff types can be added simply here)
                }

                // Decrease amount by 1 (or remove item)
                if (slot.ItemObject.stackable)
                {
                    slot.AddAmount(-1);
                    if (slot.amount <= 0) slot.RemoveItem();
                }
                else
                {
                    slot.RemoveItem();
                }
            }
        }
    }
    public void OnEnterInterface(GameObject obj)
    {
        MouseData.interfaceMouseIsOver = obj.GetComponent<UserInterface>();
    }
    public void OnExitInterface(GameObject obj)
    {
        // Xóa mô tả khi chuột rời khỏi toàn bộ giao diện
        SetDescriptionText("");
        MouseData.interfaceMouseIsOver = null;
    }
    public void OnDragStart(GameObject obj)
    {
        MouseData.tempItemBeingDragged = CreateTempItem(obj);
        // Hiển thị mô tả khi bắt đầu kéo
        if (slotsOnInterface.ContainsKey(obj) && slotsOnInterface[obj].item.Id >= 0)
        {
            SetDescriptionText(slotsOnInterface[obj].ItemObject.description);
        }

    }
    public GameObject CreateTempItem(GameObject obj)
    {
        GameObject tempItem = null;
        if(slotsOnInterface[obj].item.Id >= 0)
        {
            //Tạo ra một đối tượng trống mới ngay khi bạn bắt đầu kéo
            tempItem = new GameObject();     
            var rt = tempItem.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(40, 40);     

            //Đưa nó ra ngoài cùng cấp với bảng Inventory để nó không bị các ô đồ khác che mất
            tempItem.transform.SetParent(transform.parent);
            var img = tempItem.AddComponent<Image>();
            img.sprite = slotsOnInterface[obj].ItemObject.uiDisplay;
            img.raycastTarget = false;
        }
        return tempItem;
    }

    public void OnDragEnd(GameObject obj)
    {
        Destroy(MouseData.tempItemBeingDragged);
        // Xóa mô tả khi kết thúc kéo
        SetDescriptionText("");

        if(MouseData.interfaceMouseIsOver == null)
        {
            // Vật phẩm được kéo ra ngoài giao diện UI, thực hiện thả vào thế giới
            if (slotsOnInterface.ContainsKey(obj) && slotsOnInterface[obj].item.Id >= 0)
            {
                ItemObject itemToDrop = slotsOnInterface[obj].ItemObject;
                InventorySlot slotToDrop = slotsOnInterface[obj];
                if (PlayerAtr.Instance != null)
                {
                    Debug.Log($"Dropping item: {slotToDrop.ItemObject?.name} | amount={slotToDrop.amount} | stackable={slotToDrop.ItemObject?.stackable}");
                    // Nếu vật phẩm có thể stack thì chỉ thả 1 cái, giảm 1 số lượng
                    if (slotToDrop.ItemObject != null && slotToDrop.ItemObject.stackable)
                    {
                        PlayerAtr.Instance.DropItem(slotToDrop.ItemObject, 1);
                        slotToDrop.AddAmount(-1);
                        if (slotToDrop.amount <= 0)
                            slotToDrop.RemoveItem();
                    }
                    else
                    {
                        // Không stack => thả toàn bộ (hoặc item đơn lẻ)
                        PlayerAtr.Instance.DropItem(slotToDrop.ItemObject, slotToDrop.amount);
                        slotToDrop.RemoveItem();
                    }
                }
            }
            return;
        }
        if (MouseData.slotHoveredOver)
        {
            InventorySlot mouseHoverSlotData = MouseData.interfaceMouseIsOver.slotsOnInterface[MouseData.slotHoveredOver];
            inventory.SwapItems(slotsOnInterface[obj], mouseHoverSlotData);
        }
    }

    public void OnDrag(GameObject obj)
    {
        if(MouseData.tempItemBeingDragged != null)
        {
            MouseData.tempItemBeingDragged.GetComponent<RectTransform>().position = Input.mousePosition;
        }
    }
}
public static class MouseData      //Người đưa tin
{
    public static UserInterface interfaceMouseIsOver;
    public static GameObject tempItemBeingDragged;      //Cái hình ảnh vật phẩm đang bay theo chuột.
    public static GameObject slotHoveredOver;
}


public static class ExtensionMethods
{
    public static void UpdateSlotDisplay( this Dictionary<GameObject, InventorySlot> _slotsOnInterface)
    {
        foreach(KeyValuePair<GameObject, InventorySlot> _slot in _slotsOnInterface)
        {
            if(_slot.Value.item.Id >= 0) // Nếu ô có vật phẩm
            {
                // 1. Hiển thị hình ảnh từ Database dựa trên ID
                _slot.Key.transform.GetChild(0).GetComponentInChildren<Image>().sprite = _slot.Value.ItemObject.uiDisplay;
                // 2. Hiện màu sắc (Alpha = 1)
                _slot.Key.transform.GetChild(0).GetComponentInChildren<Image>().color = new Color(1,1,1,1);
                // 3. Hiện số lượng (nếu = 1 thì ẩn chữ số đi cho đẹp)
                _slot.Key.GetComponentInChildren<TextMeshProUGUI>().text = _slot.Value.amount == 1 ? "" : _slot.Value.amount.ToString("n0");
            }
            else // Nếu ô trống (ID = -1)
            {
                // Ẩn hình ảnh và làm mờ màu sắc (Alpha = 0)
                _slot.Key.transform.GetChild(0).GetComponentInChildren<Image>().color = new Color(1,1,1,0);
                _slot.Key.GetComponentInChildren<TextMeshProUGUI>().text = "";
            }
        }
    }
}