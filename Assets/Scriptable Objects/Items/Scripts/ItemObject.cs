using UnityEngine;

public enum ItemType
{
    Food, Helmet, Chest, Legguards, Boots, Gloves, Belt, Weapon, Shield, Default
}
public enum Atributes
{
    Agility, Intellect, stamina, strngth 
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
public class ItemBuff
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
    public void GenerateValue()
    {
        value = UnityEngine.Random.Range(min, max);
    }
}