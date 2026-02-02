using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "New Inventory", menuName = "Inventory System/Inventory")]
public class InventoryObject : ScriptableObject
{
    public string savePath;           // Tên file lưu (ví dụ: /inventory.save)
    public ItemDatabaseObject database; // Cơ sở dữ liệu để tra cứu ID vật phẩm
    public Inventory Container;

// Hàm OnEnable: Chạy khi đối tượng rương đồ này được kích hoạt/nạp lên

    // Thêm vật phẩm vào rương
    // Trả về true nếu thêm thành công, false nếu rương đầy
    public bool AddItem(Item _item, int _amount)
    {
        if(_item.buffs.Length > 0)
        {
            var slot = SetEmptySlot(_item, _amount);
            return slot != null;
        }

        for(int i = 0; i < Container.Items.Length; i++)
        {
            if (Container.Items[i].ID == _item.id) 
            {
                Container.Items[i].AddAmount(_amount);
                return true;
            }
        }
        var s = SetEmptySlot(_item, _amount);
        return s != null;
    }
    public InventorySlot SetEmptySlot(Item _item, int _amount)
    {
        for (int i = 0; i < Container.Items.Length; i++)
        {
            // ID < 0 nghĩa là ô trống (tránh nhầm ID==0)
            if(Container.Items[i].ID < 0)
            {
                 Container.Items[i].UpdateSlot(_item.id, _item, _amount);
                 return Container.Items[i];
            }
        }
        //Set up funcionality for full inventory
        return null;
    }

    public void MoveItem(InventorySlot item1, InventorySlot item2)
    {
        InventorySlot temp = new InventorySlot(item2.ID, item2.item, item2.amount);
        item2.UpdateSlot(item1.ID, item1.item, item1.amount);
        item1.UpdateSlot(temp.ID, temp.item, temp.amount);
         
    }

    /* --- PHẦN SAVE/LOAD/CLEAR DỮ LIỆU --- */
    [ContextMenu("Save")] // Chuột phải vào Script trong Inspector để hiện nút Save
    public void Save()
    {
        /*
        // 1. Chuyển đối tượng hiện tại thành chuỗi JSON
        string saveData = JsonUtility.ToJson(this, true);       //true: định dạng dễ đọc
        // 2. Công cụ mã hóa nhị phân để bảo mật file
        BinaryFormatter bf = new BinaryFormatter();
        // 3. Tạo file tại đường dẫn lưu trữ cố định của hệ điều hành
        FileStream file = File.Create(string.Concat(Application.persistentDataPath, savePath));
        // 4. Mã hóa chuỗi JSON và ghi vào file
        bf.Serialize(file, saveData);
        file.Close(); // Luôn đóng file sau khi ghi xong
        Debug.Log("Đã lưu tại: " + Application.persistentDataPath);
        */

        // Tạo bộ mã hóa nhị phân
        IFormatter formatter = new BinaryFormatter();
        // Tạo luồng dữ liệu (Stream) trỏ đến file cần tạo
        Stream stream = new FileStream(string.Concat(Application.persistentDataPath, savePath), FileMode.Create, FileAccess.Write);
        // Mã hóa toàn bộ đối tượng Container và đẩy vào file
        formatter.Serialize(stream, Container);
        // Đóng luồng để hoàn tất ghi file
        stream.Close();
    }
    [ContextMenu("Load")]   
    public void Load()
    {
        string path = string.Concat(Application.persistentDataPath, savePath);
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
            for (int i = 0; i < Container.Items.Length; i++)
            {
                Container.Items[i].UpdateSlot(newContainer.Items[i].ID, newContainer.Items[i].item, newContainer.Items[i].amount);
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
    public InventorySlot[] Items = new InventorySlot[50];
    public void Clear()
    {
        for (int i = 0; i < Items.Length; i++)
        {
            Items[i].UpdateSlot(-1, new Item(), 0);
        }
    }
}
[System.Serializable]
public class InventorySlot
{
    public ItemType[] AllowedItems = new ItemType[0];
    [System.NonSerialized]
    public UserInterface parent;
    public int ID = -1;          // ID để lưu trữ (vì Unity không lưu được file Asset trực tiếp)
    public Item item; // File Asset vật phẩm (dùng để hiển thị trong game)
    public int amount;      // Số lượng

    public InventorySlot()
    {
        ID = -1;
        item = null;
        amount = 0;
    }
    public InventorySlot(int _id, Item _item, int _amount)
    {
        ID = _id;
        item = _item;
        amount = _amount;
    }
    public void UpdateSlot(int _id, Item _item, int _amount)
    {
        ID = _id;
        item = _item;
        amount = _amount;
    }

    public void AddAmount(int value)
    {
        amount += value;
    }

    public bool CanplaceInSlot(ItemObject _item)
    {
        if(AllowedItems.Length <= 0)
        {
            return true;
        }
        for (int i = 0; i < AllowedItems.Length; i++)
        {
            if(_item.type == AllowedItems[i])
            {
                return true;
            }
        }
        return false;
    }
}