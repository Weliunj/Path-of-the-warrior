using UnityEngine;

public class ItemWorld : MonoBehaviour
{
    public ItemObject item;
    public int amount; // Thêm trường này để lưu số lượng vật phẩm trong stack

    // Phương thức để khởi tạo ItemWorld với dữ liệu ItemObject
    public void Initialize(ItemObject itemData, int itemAmount)
    {
        item = itemData;
        amount = itemAmount; // Gán số lượng
        // Cập nhật SpriteRenderer của GameObject nếu có
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null && itemData != null)
        {
            sr.sprite = itemData.uiDisplay; // Giả sử uiDisplay là sprite bạn muốn hiển thị trong thế giới
        }
        gameObject.name = itemData != null ? itemData.name + "_WorldItem" : "Empty_WorldItem";
    }
}
