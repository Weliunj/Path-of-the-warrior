using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public enum InterfaceType
{
    Inventory,
    Equipment,
    Chest
}

[CreateAssetMenu(fileName = "New Inventory", menuName = "Inventory System/Inventory")]
public class InventoryObject : ScriptableObject
{
    public string savePath;           // Tên file lưu (ví dụ: /inventory.save)
    public ItemDatabaseObject database; // Cơ sở dữ liệu để tra cứu ID vật phẩm
    public InterfaceType type;
    public Inventory Container;
    public InventorySlot[] GetSlots { get { return Container.Slots; }}

// Hàm OnEnable: Chạy khi đối tượng rương đồ này được kích hoạt/nạp lên

    // Thêm vật phẩm vào rương
    // Trả về true nếu thêm thành công, false nếu rương đầy hoặc item không hợp lệ
    public bool AddItem(Item _item, int _amount)
    {
        if (_item == null)
        {
            Debug.LogWarning("[InventoryObject] AddItem called with null Item.");
            return false;
        }

        // Kiểm tra ID hợp lệ trước khi truy cập database
        if (_item.Id < 0 || database == null || database.ItemObjects == null)
        {
            Debug.LogWarning($"[InventoryObject] Invalid or missing Item Id {_item.Id}. Cannot add to inventory.");
            return false;
        }

        if(EmptySlotCount <= 0)
            return false;

        InventorySlot slot = FindItemOnInventory(_item);

        if(!database.ItemObjects[_item.Id].stackable || slot == null)
        {
            SetEmptySlot(_item, _amount);
            return true;
        }
        slot.AddAmount(_amount);
        return true;
    }

    public int EmptySlotCount
    {
        get
        {
            int counter = 0;
            for (int i = 0; i < GetSlots.Length; i++)
            {
                if(GetSlots[i].item.Id <= -1)
                    counter++;
            }
            return counter;
        }
    } 

    public InventorySlot FindItemOnInventory(Item _item)
    {
        for (int i = 0; i < GetSlots.Length; i++)
        {
            if(GetSlots[i].item.Id == _item.Id)
            {
                return GetSlots[i];
            }
        }
        return null;
    }

    public InventorySlot SetEmptySlot(Item _item, int _amount)
    {
        for (int i = 0; i < GetSlots.Length; i++)
        {
            // ID < 0 nghĩa là ô trống (tránh nhầm ID==0)
            if(GetSlots[i].item.Id < 0)
            {
                 GetSlots[i].UpdateSlot(_item, _amount);
                 return GetSlots[i];
            }
        }
        //Set up funcionality for full inventory
        return null;
    }

    public void SwapItems(InventorySlot item1, InventorySlot item2)
    {
        if(item2.CanplaceInSlot(item1.ItemObject) && item1.CanplaceInSlot(item2.ItemObject))
        {
            InventorySlot temp = new InventorySlot(item2.item, item2.amount);
            item2.UpdateSlot(item1.item, item1.amount);
            item1.UpdateSlot(temp.item, temp.amount);
         
        }
    }

    /* --- PHẦN SAVE/LOAD/CLEAR DỮ LIỆU --- */
    [ContextMenu("Save")] // Chuột phải vào Script trong Inspector để hiện nút Save
    public void Save()
    {
        // Kiểm tra nếu savePath trống thì gán tên mặc định để tránh lỗi Folder Access
        if (string.IsNullOrEmpty(savePath))
        {
            savePath = "default_inventory.save";
        }

        string fullPath = Path.Combine(Application.persistentDataPath, savePath);
        string directoryPath = Path.GetDirectoryName(fullPath);

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        // Kiểm tra xem fullPath có thực sự trỏ tới một file không (không phải thư mục)
        if (Directory.Exists(fullPath))
        {
            Debug.LogError("SavePath đang trỏ tới một thư mục hiện có! Hãy đổi tên file trong Inspector.");
            return;
        }

        IFormatter formatter = new BinaryFormatter();
        using (Stream stream = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
        {
            formatter.Serialize(stream, Container);
        }
        
        Debug.Log("Đã lưu tại: " + fullPath);
    }
    [ContextMenu("Load")]   
    public void Load()
    {
        // Use Path.Combine for consistency and correctness
        string path = Path.Combine(Application.persistentDataPath, savePath);
        // Kiểm tra xem file có tồn tại hay không
        if(File.Exists(path)){
            /*
            BinaryFormatter bf = new BinaryFormatter();
            // Mở file ra để đọc
            FileStream file = File.Open(path, FileMode.Open);
            // Giải mã nhị phân thành chuỗi và ghi đè dữ liệu vào rương đồ này
            JsonUtility.FromJsonOverwrite(bf.Deserialize(file).ToString(), this);
            file.Close();
            */
            IFormatter formatter = new BinaryFormatter();
            // Mở luồng để đọc file hiện có
            Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            // Giải mã dữ liệu từ file và ép kiểu ngược lại thành lớp Inventory
            Inventory newContainer = (Inventory)formatter.Deserialize(stream);
            for (int i = 0; i < GetSlots.Length; i++)
            {
                GetSlots[i].UpdateSlot( newContainer.Slots[i].item, newContainer.Slots[i].amount);
            }
            stream.Close();
        }
    }
    [ContextMenu("Clear")]
    public void Clear()
    {
        Container.Clear();
    }
}

[System.Serializable]
public class Inventory
{
    public InventorySlot[] Slots = new InventorySlot[50];
    public void Clear()
    {
        for (int i = 0; i < Slots.Length; i++)
        {
            Slots[i].RemoveItem();
        }
    }
}

public delegate void SlotUpdated(InventorySlot _slot);

[System.Serializable]
public class InventorySlot
{
    public ItemType[] AllowedItems = new ItemType[0];
    [System.NonSerialized]
    public UserInterface parent;
    [System.NonSerialized]
    public GameObject slotDisplay;

    [System.NonSerialized]
    public SlotUpdated OnAfterUpdate;
    [System.NonSerialized]
    public SlotUpdated OnBeforeUpdate;
    public Item item; // File Asset vật phẩm (dùng để hiển thị trong game)
    public int amount;      // Số lượng

    public ItemObject ItemObject
    {
        get
        {
            if(item.Id >= 0)
            {
                return parent.inventory.database.ItemObjects[item.Id];
            }
            return null;
        }
    }

    public InventorySlot()
    {
        UpdateSlot(new Item(), 0);
    }
    public InventorySlot(Item _item, int _amount)
    {
        UpdateSlot(_item, _amount);
    }
    public void UpdateSlot(Item _item, int _amount)
    {
        if(OnBeforeUpdate != null)
        {
            OnBeforeUpdate.Invoke(this);
        }
        item = _item;
        amount = _amount;
        if(OnAfterUpdate != null)
        {
            OnAfterUpdate.Invoke(this);
        }
    }
    public void RemoveItem()
    {
        UpdateSlot(new Item(), 0);  
    }
    public void AddAmount(int value)
    {
        UpdateSlot(item, amount += value);
    }

    public bool CanplaceInSlot(ItemObject _itemObject)
    {
        if(AllowedItems.Length <= 0 || _itemObject == null || _itemObject.data.Id < 0)
        {
            return true;
        }
        for (int i = 0; i < AllowedItems.Length; i++)
        {
            if(_itemObject.type == AllowedItems[i])
            {
                return true;
            }
        }
        return false;
    }
}