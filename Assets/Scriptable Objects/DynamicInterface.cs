using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class DynamicInterface : UserInterface
{
    public GameObject inventoryPrefab; // Mẫu thiết kế của 1 ô đồ (Prefab)
    public override void CreateSlot()
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
}
