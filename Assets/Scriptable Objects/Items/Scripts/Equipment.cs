using UnityEngine;

[CreateAssetMenu(fileName = "New Equipment Object", menuName = "Inventory System/Items/Equipment")]
public class Equipment : ItemObject
{
    void Awake()
    {
        type = ItemType.Chest;
    }
}
