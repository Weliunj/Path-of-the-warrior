using Fusion;
using UnityEngine;
using System.Collections.Generic;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
{
    // Kéo Prefab Player (có NetworkObject) vào đây từ Inspector
    public NetworkObject playerPrefab;
    // Hàm này tự động gọi khi có người chơi (Client) tham gia vào Session
    public void PlayerJoined(PlayerRef player)
    {
        Debug.Log($"Người chơi {player} đã tham gia. Đang tạo nhân vật...");
        
        // Lệnh quan trọng: Sinh nhân vật trên mạng
        // playerPrefab: Prefab đã tạo ở Bài 1 [cite: 14, 15]
        // inputAuthority: Gán quyền điều khiển cho người chơi vừa tham gia
        Runner.Spawn(playerPrefab, Vector3.up * 2, Quaternion.identity, player);
    }
}