using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using UnityEngine.Events;
using Unity.VisualScripting;
using UnityEditor.U2D;

public abstract class UserInterface : MonoBehaviour
{
    public PlayerInteraction playerI;

    [Header("Cấu hình UI")]
    public InventoryObject inventory;  // File dữ liệu rương đồ (ScriptableObject)
    
    [Header("Trạng thái")]

    // Dictionary giúp liên kết Dữ liệu (Slot) với Đối tượng hiển thị (GameObject)
    // Giúp cập nhật số lượng cực nhanh mà không cần tạo lại toàn bộ UI
    public Dictionary<GameObject, InventorySlot> itemDisplayed = new Dictionary<GameObject, InventorySlot>();

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
        UpdateSlot();
    }

    // Hàm khởi tạo giao diện rương đồ lần đầu
    public abstract void CreateSlot();
    public void UpdateSlot()
    {
        foreach(KeyValuePair<GameObject, InventorySlot> _slot in itemDisplayed)
        {
            if(_slot.Value.ID >= 0) // Nếu ô có vật phẩm
            {
                // 1. Hiển thị hình ảnh từ Database dựa trên ID
                _slot.Key.transform.GetChild(0).GetComponentInChildren<Image>().sprite = inventory.database.GetItem[_slot.Value.item.id].uiDisplay;
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
        playerI.mouseItem.hoverObj = obj;
        if(itemDisplayed.ContainsKey(obj))
            playerI.mouseItem.hoverItem = itemDisplayed[obj];
    }
    public void OnExit(GameObject obj)
    {
        playerI.mouseItem.hoverObj = null;
        playerI.mouseItem.hoverItem = null;
    }
    public void OnEnterInterface(GameObject obj)
    {
        playerI.mouseItem.ui = obj.GetComponent<UserInterface>();
    }
    public void OnExitInterface(GameObject obj)
    {
        playerI.mouseItem.ui = null;
    }
    public void OnDragStart(GameObject obj)
    {
        //Tạo ra một đối tượng trống mới ngay khi bạn bắt đầu kéo
        var mouseObject = new GameObject();     
        var rt = mouseObject.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(90, 90);     //sizeDelta: 
        
        //Đưa nó ra ngoài cùng cấp với bảng Inventory để nó không bị các ô đồ khác che mất
        mouseObject.transform.SetParent(transform.parent);
        if(itemDisplayed[obj].ID >= 0)
        {
            var img = mouseObject.AddComponent<Image>();
            img.sprite = inventory.database.GetItem[itemDisplayed[obj].ID].uiDisplay;
            img.raycastTarget = false;
        }
        playerI.mouseItem.obj = mouseObject;
        playerI.mouseItem.item = itemDisplayed[obj];
    }
    public void OnDragEnd(GameObject obj)
    {
        var itemOnMouse = playerI.mouseItem;
        var mouseHoverItem = itemOnMouse.hoverItem;
        var mouseHoverObj = itemOnMouse.hoverObj;
        var GetItemObject = inventory.database.GetItem;

        if(itemOnMouse.ui != null)
        {
            if (mouseHoverObj)
                if(mouseHoverItem.CanplaceInSlot(GetItemObject[itemDisplayed[obj].ID]) 
                    && (mouseHoverItem.item.id <= -1 || (mouseHoverItem.item.id >= 0 
                    && itemDisplayed[obj].CanplaceInSlot(GetItemObject[mouseHoverItem.item.id]))))
                inventory.MoveItem(itemDisplayed[obj], mouseHoverItem.parent.itemDisplayed[itemOnMouse.hoverObj]);
        }
        else
        {
            Debug.Log("Drop item");
        }
        Destroy(itemOnMouse.obj);
        itemOnMouse.item = null;
    }
    public void OnDrag(GameObject obj)
    {
        if(playerI.mouseItem.obj != null)
        {
            playerI.mouseItem.obj.GetComponent<RectTransform>().position = Input.mousePosition;
        }
    }
}
public class MouseItem      //Người đưa tin
{
    public UserInterface ui;
    public GameObject obj;      //Cái hình ảnh vật phẩm đang bay theo chuột.
    public InventorySlot item;      //Dữ liệu của ô đồ gốc (nơi bắt đầu kéo).
    public InventorySlot hoverItem;
    public GameObject hoverObj;     //Cái ô UI mà chuột đang nằm đè lên.
}