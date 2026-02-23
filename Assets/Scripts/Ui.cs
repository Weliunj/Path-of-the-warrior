using UnityEngine;
using TMPro; // Thêm namespace này để sử dụng TextMeshProUGUI

public class Ui : MonoBehaviour
{
    // Singleton instance để các script khác có thể dễ dàng truy cập
    public static Ui Instance { get; private set; }

    [Header("UI Elements")]
    public TextMeshProUGUI itemDescriptionText; // Tham chiếu đến Text UI để hiển thị mô tả

    void Awake()
    {
        // Đảm bảo chỉ có một instance của Ui trong scene
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    // Phương thức để cập nhật text mô tả vật phẩm
    public void SetItemDescription(string description)
    {
        if (itemDescriptionText != null)
        {
            itemDescriptionText.text = description;
        }
    }
}
