using UnityEngine;

// Định nghĩa các loại vật phẩm trong game để dễ phân loại
public enum ItemType
{
    Food,       // Thức ăn (hồi máu, năng lượng)
    Helmet,
    Chest,
    Legguards,
    Boots,
    Gloves,
    Belt,
    Weapon,
    Shield,
    Default     // Vật phẩm cơ bản (nguyên liệu, rác)
}

// Danh sách các chỉ số nhân vật có thể được cộng thêm
public enum Atributes
{
    Agility,   // Thân pháp
    Intellect, // Trí tuệ
    stamina,   // Thể lực
    strngth    // Sức mạnh
}

// Lớp cha trừu tượng cho mọi vật phẩm, dùng ScriptableObject để lưu dữ liệu dưới dạng file
public abstract class ItemObject : ScriptableObject
{
    public int Id;
    public Sprite uiDisplay;
    public ItemType type;
    [TextArea(10, 15)]
    public string description;
    public ItemBuff[] buffs; // Danh sách các Buff thiết kế sẵn cho loại vật phẩm này

    // Hàm tạo ra một thực thể vật phẩm cụ thể từ file dữ liệu
    public Item CreateItem()
    {
        Item newItem = new Item(this);
        return newItem;
    }
}

[System.Serializable]
public class Item
{
    public string Name;
    public int id;
    public ItemBuff[] buffs; // Lưu lại các chỉ số riêng biệt của thực thể này

    public Item()
    {
        Name = "";
        id = -1;
    }
    public Item(ItemObject item)
    {
        Name = item.name;
        id = item.Id;
        // Khởi tạo mảng buff mới dựa trên số lượng buff của vật phẩm gốc
        buffs = new ItemBuff[item.buffs.Length];
        
        for(int i = 0; i < buffs.Length; i++)
        {
            // Sao chép min, max từ dữ liệu gốc sang thực thể mới
            buffs[i] = new ItemBuff(item.buffs[i].min, item.buffs[i].max)
            {
                atribute = item.buffs[i].atribute
            };
            // Lưu ý: Sau bước này, bạn nên gọi buffs[i].GenerateValue() 
            // để xác định chỉ số 'value' cho món đồ vừa tạo.
        }
    }
}

[System.Serializable]
public class ItemBuff
{
    public Atributes atribute; // Loại chỉ số (ví dụ: Agility)
    public int value;          // Giá trị thực tế sau khi đã Random (dùng để cộng cho người chơi)
    public int min;            // Giá trị nhỏ nhất có thể đạt được
    public int max;            // Giá trị lớn nhất có thể đạt được

    // Hàm khởi tạo để gán khung giá trị
    public ItemBuff(int _min, int _max)
    {
        min = _min;
        max = _max; 
        GenerateValue();
    }

    // Hàm cực kỳ quan trọng: Tính toán con số ngẫu nhiên thực tế
    public void GenerateValue()
    {
        // Chọn một số ngẫu nhiên trong khoảng [min, max]
        value = UnityEngine.Random.Range(min, max);
    }
}