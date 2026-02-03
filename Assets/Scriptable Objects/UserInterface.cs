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
        for (int i = 0; i < inventory.Container.Items.Length; i++)
        {
            inventory.Container.Items[i].parent  = this;
        }
        // Khi game bắt đầu, vẽ rương đồ dựa trên dữ liệu có sẵn
        CreateSlot(); 
        AddEvent(gameObject, EventTriggerType.PointerEnter, delegate { OnEnterInterface(gameObject);});
        AddEvent(gameObject, EventTriggerType.PointerExit, delegate { OnExitInterface(gameObject);});
    }

    void Update()
    {
        slotsOnInterface.UpdateSlotDisplay();
    }

    // Hàm khởi tạo giao diện rương đồ lần đầu
    public abstract void CreateSlot();

    protected void  AddEvent(GameObject obj, EventTriggerType type, UnityAction<BaseEventData> action)
    {
        EventTrigger trigger = obj.GetComponent<EventTrigger>() ?? obj.AddComponent<EventTrigger>();
        var eventTrigger = new EventTrigger.Entry();
        eventTrigger.eventID = type;
        eventTrigger.callback.AddListener(action);
        trigger.triggers.Add(eventTrigger);
    }
    public void OnEnter(GameObject obj)
    {
        MouseData.slotHoveredOver = obj;
    }
    public void OnExit(GameObject obj)
    {
        MouseData.slotHoveredOver = null;
    }
    public void OnEnterInterface(GameObject obj)
    {
        MouseData.interfaceMouseIsOver = obj.GetComponent<UserInterface>();
    }
    public void OnExitInterface(GameObject obj)
    {
        MouseData.interfaceMouseIsOver = null;
    }
    public void OnDragStart(GameObject obj)
    {
        MouseData.tempItemBeingDragged = CreateTempItem(obj);
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
        if(MouseData.interfaceMouseIsOver == null)
        {
            slotsOnInterface[obj].RemoveItem();
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