using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using UnityEngine.Events;
using Unity.VisualScripting;
using UnityEditor.U2D;

public class DisplayInventory : MonoBehaviour
{
    public MouseItem mouseItem = new MouseItem();

    [Header("Cấu hình UI")]
    public GameObject inventoryPrefab; // Mẫu thiết kế của 1 ô đồ (Prefab)
    public InventoryObject inventory;  // File dữ liệu rương đồ (ScriptableObject)
    
    [Header("Trạng thái")]
    public bool toggleUI = false;      // Biến kiểm tra rương đang đóng hay mở

    // Dictionary giúp liên kết Dữ liệu (Slot) với Đối tượng hiển thị (GameObject)
    // Giúp cập nhật số lượng cực nhanh mà không cần tạo lại toàn bộ UI
    Dictionary<GameObject, InventorySlot> itemDisplayed = new Dictionary<GameObject, InventorySlot>();

    void Start()
    {
        // Khi game bắt đầu, vẽ rương đồ dựa trên dữ liệu có sẵn
        CreateSlot(); 
    }

    void Update()
    {
        UpdateSlot();
        ToggleUI();      // Lắng nghe phím Tab để đóng/mở rương
    }

    // Hàm khởi tạo giao diện rương đồ lần đầu
    public void CreateSlot()
    {
        itemDisplayed = new Dictionary<GameObject, InventorySlot>();
        for(int i = 0; i < inventory.Container.Items.Length; i++)
        {
            // Tạo ra một bản sao của Prefab (ô đồ trên UI)
            var obj = Instantiate(inventoryPrefab, Vector3.zero, Quaternion.identity, transform);

            AddEvent(obj, EventTriggerType.PointerEnter, delegate { OnEnter(obj);});
            AddEvent(obj, EventTriggerType.PointerExit, delegate { OnExit(obj);});
            AddEvent(obj, EventTriggerType.BeginDrag, delegate { OnDragStart(obj);});
            AddEvent(obj, EventTriggerType.EndDrag, delegate { OnDragEnd(obj);});
            AddEvent(obj, EventTriggerType.Drag, delegate { OnDrag(obj);});

            // Lưu mối quan hệ giữa "Cái ô trên màn hình" và "Dữ liệu trong Script"
            itemDisplayed.Add(obj, inventory.Container.Items[i]);
        }
    }
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
                _slot.Key.transform.GetChild(0).GetComponentInChildren<Image>().color = new Color(1,1,1,1);
                _slot.Key.GetComponentInChildren<TextMeshProUGUI>().text = "";
            }
        }
    }

    private void  AddEvent(GameObject obj, EventTriggerType type, UnityAction<BaseEventData> action)
    {
        EventTrigger trigger = obj.GetComponent<EventTrigger>();
        var eventTrigger = new EventTrigger.Entry();
        eventTrigger.eventID = type;
        eventTrigger.callback.AddListener(action);
        trigger.triggers.Add(eventTrigger);
    }
    public void OnEnter(GameObject obj)
    {
        mouseItem.hoverObj = obj;
        if(itemDisplayed.ContainsKey(obj))
            mouseItem.hoverItem = itemDisplayed[obj];
    }
    public void OnExit(GameObject obj)
    {
        mouseItem.hoverObj = null;
        mouseItem.hoverItem = null;
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
        mouseItem.obj = mouseObject;
        mouseItem.item = itemDisplayed[obj];
    }
    public void OnDragEnd(GameObject obj)
    {
        if (mouseItem.hoverObj)
        {
            inventory.MoveItem(itemDisplayed[obj], itemDisplayed[mouseItem.hoverObj]);
        }
        else
        {
            
        }
        Destroy(mouseItem.obj);
        mouseItem.item = null;
    }
    public void OnDrag(GameObject obj)
    {
        if(mouseItem.obj != null)
        {
            mouseItem.obj.GetComponent<RectTransform>().position = Input.mousePosition;
        }
    }
    // Hàm xử lý logic đóng/mở giao diện và con trỏ chuột
    public void ToggleUI()
    {
        if(Input.GetKeyDown(KeyCode.Tab)) // Kiểm tra phím Tab
        {
            toggleUI = !toggleUI;
            // Ở đây bạn có thể thêm: panelUI.SetActive(toggleUI); để ẩn/hiện cái bảng
        }

        if (toggleUI)
        {
            // MỞ RƯƠNG: Hiện chuột và cho phép di chuyển chuột tự do trên màn hình
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true; 
        }
        else
        {
            // ĐÓNG RƯƠNG: Khóa chuột vào tâm màn hình và ẩn đi (dành cho game góc nhìn thứ nhất/thứ ba)
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}