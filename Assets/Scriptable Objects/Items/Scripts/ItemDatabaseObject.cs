using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item Database", menuName = "Inventory System/Items/Database")]

// ScriptableObject này đóng vai trò là "Sổ cái" chứa toàn bộ vật phẩm tồn tại trong game của bạn.
// ISerializationCallbackReceiver: Interface giúp bạn thực hiện các hành động đặc biệt 
// trước và sau khi Unity lưu/nạp dữ liệu (Serialization).
public class ItemDatabaseObject : ScriptableObject, ISerializationCallbackReceiver
{
    // Mảng chứa tất cả các ItemObject (vật phẩm) bạn đã tạo ra trong Project.
    // Unity có thể hiển thị và lưu trữ mảng (Array) này một cách bình thường.
    public ItemObject[] Items;

    // Dictionary để tra cứu ID của một vật phẩm cực nhanh.
    // Key: ItemObject (Vật phẩm) | Value: int (Số ID tương ứng).
    // Lưu ý: Unity không thể lưu trực tiếp Dictionary vào file .asset, nên ta cần interface ở trên.
    // public Dictionary<int, ItemObject> GetItem = new Dictionary<int, ItemObject>();
    [ContextMenu("Update ID's")]
    public void UpdateID()
    {
        // Khởi tạo lại Dictionary và đổ dữ liệu từ mảng 'item' vào để sử dụng trong lúc chơi.
        // GetItem = new Dictionary<int, ItemObject>();
        for (int i = 0; i < Items.Length; i++)
        {
            // Gán ID cho vật phẩm dựa trên vị trí (chỉ số i) của nó trong mảng.
            if (Items[i].data.Id != i)
            Items[i].data.Id = i;
                // GetItem.Add(i, Items[i]);
        }
    }
    // Hàm này chạy NGAY SAU KHI Unity nạp xong dữ liệu từ ổ cứng vào RAM.
    public void OnAfterDeserialize()
    {
        UpdateID();
    }

    // Hàm này chạy NGAY TRƯỚC KHI Unity tiến hành lưu dữ liệu từ RAM xuống ổ cứng.
    public void OnBeforeSerialize()
    {
        // GetItem = new Dictionary<int, ItemObject>();
    }

    /* ====================================================================================================
    TỔNG HỢP KIẾN THỨC VỀ SERIALIZATION TRONG UNITY
    ====================================================================================================

    1. SERIALIZE (Tuần tự hóa): 
        - Là quá trình "đóng gói" các đối tượng (Object) từ bộ nhớ RAM thành một định dạng dữ liệu 
        (như file .asset, JSON, hoặc Binary) để Unity có thể lưu trữ trên ổ cứng.

    2. DESERIALIZE (Giải tuần tự hóa): 
        - Là quá trình "mở hộp" ngược lại, đọc dữ liệu từ ổ cứng và nạp lại vào RAM dưới dạng các 
        đối tượng mà code có thể hiểu và thao tác được.

    3. [SYSTEM.SERIALIZABLE]: 
        - Là một "nhãn dán" (Attribute) đặt trên một Class hoặc Struct tự tạo.
        - Công dụng: Cho phép Unity Serialize (đóng gói) lớp đó. Nếu thiếu nhãn này, lớp đó sẽ 
        không bao giờ hiển thị được trên cửa sổ INSPECTOR và dữ liệu sẽ không được lưu lại.

    4. ISERIALIZATIONCALLBACKRECEIVER (Giao diện can thiệp):
        - Là một Interface cung cấp 2 hàm "móc" (Hook) để bạn can thiệp vào quá trình Đóng gói/Mở hộp:
        
        a. OnBeforeSerialize(): 
        - Chạy NGAY TRƯỚC khi Unity lưu dữ liệu xuống máy.
        - Thường dùng để chuẩn bị dữ liệu phức tạp thành dạng đơn giản hơn mà Unity hiểu được.
        
        b. OnAfterDeserialize(): 
        - Chạy NGAY SAU khi Unity nạp dữ liệu từ file vào RAM xong.
        - Cực kỳ hữu ích để tái tạo lại các kiểu dữ liệu mà Unity không tự lưu được (như Dictionary) 
            dựa trên các danh sách (List/Array) đã được lưu.

    ----------------------------------------------------------------------------------------------------
    MỐI QUAN HỆ:
    - [System.Serializable] giúp Unity "nhìn thấy" dữ liệu trên UI/Inspector.
    - Serialize/Deserialize là cơ chế cốt lõi để Unity xử lý dữ liệu đó giữa Code và Ổ cứng.
    - ISerializationCallbackReceiver là "trợ thủ" để xử lý các kiểu dữ liệu nâng cao mà cơ chế 
    mặc định của Unity không làm được.
    ====================================================================================================
    */
}