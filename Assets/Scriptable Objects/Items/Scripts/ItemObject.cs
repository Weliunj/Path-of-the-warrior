using UnityEngine;

public enum ItemType
{
    Food, Helmet, Chest, Legguards, Boots, Gloves, Belt, Weapon, Shield, Default
}
public enum Atributes
{
    MoveSpeed,    // Thay cho Agility (Tập trung vào tốc độ di chuyển)
    Stamina,      // Giữ nguyên hoặc viết hoa (Sức bền/Thể lực)
    Defense,      // Thay cho Intellect (Khả năng phòng thủ/Giảm sát thương)
    Strength,      // Viết lại cho đúng chính tả (Sức mạnh vật lý)
    Health,       // Thay cho Vitality (Máu/HP)
}

[CreateAssetMenu(fileName = "New Item", menuName ="Inventory System/Items/item")]
public class ItemObject : ScriptableObject
{
    public Sprite uiDisplay;
    public bool stackable;
    public ItemType type;
    [TextArea(10, 15)]
    public string description;
    public Item data = new Item();

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
    public int Id = -1;
    public ItemBuff[] buffs;

    public Item()
    {
        Name = "";
        Id = -1;
    }
    public Item(ItemObject item)
    {
        Name = item.name;
        Id = item.data.Id;
        buffs = new ItemBuff[item.data.buffs.Length];
        
        for(int i = 0; i < buffs.Length; i++)
        {
            // Sao chép min, max từ dữ liệu gốc sang thực thể mới
            buffs[i] = new ItemBuff(item.data.buffs[i].min, item.data.buffs[i].max)
            {
                atribute = item.data.buffs[i].atribute
            };
        }
    }
}

[System.Serializable]
public class ItemBuff : IModifiers
{
    public Atributes atribute;
    public int value;
    public int min;
    public int max; 

    public ItemBuff(int _min, int _max)
    {
        min = _min;
        max = _max; 
        GenerateValue();
    }

    public void AddValue(ref int baseValue)
    {
        baseValue += value;
    }

    public void GenerateValue()
    {
        value = UnityEngine.Random.Range(min, max);
    }
}