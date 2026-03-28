using Fusion;
using TMPro;
using UnityEngine;

public class PlayerInfo : NetworkBehaviour
{
    // Biến này sẽ được tự động đồng bộ từ máy này sang máy khác qua mạng
    [Networked, OnChangedRender(nameof(OnNameChanged))]
    public NetworkString<_16> PlayerName { get; set; }

    [SerializeField] private TextMeshPro nameText; // Kéo cái Text trên đầu nhân vật vào đây

    public override void Spawned()
    {
        Debug.Log($"Máy chủ sở hữu: {HasInputAuthority}, Tên lấy được: {RoomManager.LocalPlayerName}");
        // Chỉ máy sở hữu nhân vật này (Local) mới có quyền đặt tên
        if (HasInputAuthority)
        {
            // Lấy tên từ biến static đã lưu ở Menu
            PlayerName = RoomManager.LocalPlayerName;
        }
        else
        {
            // Đối với các máy khác (Proxy), hiển thị tên đã nhận được từ mạng
            UpdateNameDisplay();
        }
    }

    // Hàm này tự động chạy khi biến PlayerName thay đổi trên mạng
    void OnNameChanged()
    {
        UpdateNameDisplay();
    }

    private void UpdateNameDisplay()
    {
        if (nameText != null)
        {
            nameText.text = PlayerName.ToString();
        }
    }
}