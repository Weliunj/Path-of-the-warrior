using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class DynamicInterface : UserInterface
{
    public GameObject inventoryPrefab;
    public override void CreateSlot()
    {
        slotsOnInterface = new Dictionary<GameObject, InventorySlot>();
        for(int i = 0; i < inventory.GetSlots.Length; i++)
        {
            // Tạo ra một bản sao của Prefab (ô đồ trên UI)
            var obj = Instantiate(inventoryPrefab, Vector3.zero, Quaternion.identity, transform);

            AddEvent(obj, EventTriggerType.PointerEnter, delegate { OnEnter(obj);});
            AddEvent(obj, EventTriggerType.PointerExit, delegate { OnExit(obj);});
            AddEvent(obj, EventTriggerType.BeginDrag, delegate { OnDragStart(obj);});
            AddEvent(obj, EventTriggerType.EndDrag, delegate { OnDragEnd(obj);});
            AddEvent(obj, EventTriggerType.Drag, delegate { OnDrag(obj);});

            inventory.GetSlots[i].slotDisplay = obj;
            // Lưu mối quan hệ giữa "Cái ô trên màn hình" và "Dữ liệu trong Script"
            slotsOnInterface.Add(obj, inventory.GetSlots[i]);
        }
    }
}
