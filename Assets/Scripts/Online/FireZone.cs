using UnityEngine;
using Fusion;
using System.Collections;
using System.Collections.Generic; // Để quản lý danh sách người chơi trong vùng

public class FireZone : NetworkBehaviour
{
    [Header("Cấu hình sát thương")]
    public float damagePerTick = 5f;    // Sát thương mỗi lần giật
    public float tickInterval = 1.0f;  // Khoảng thời gian giữa các lần giật (giây)

    // Danh sách để theo dõi những ai đang đứng trong lửa
    private List<HealthHandler> _playersInZone = new List<HealthHandler>();

    public override void Spawned()
    {
        // Chỉ chạy logic gây sát thương trên máy có quyền kiểm soát vùng lửa này
        // (Trong Shared Mode thường là người tạo ra vùng lửa hoặc Master Client)
        if (Object.HasStateAuthority)
        {
            StartCoroutine(DealDamageOverTime());
        }
    }

    // Khi Player bước vào vùng lửa
    private void OnTriggerEnter(Collider other)
    {
        if (!Object.HasStateAuthority) return;

        HealthHandler hp = other.GetComponent<HealthHandler>();
        if (hp != null && !_playersInZone.Contains(hp))
        {
            _playersInZone.Add(hp);
            Debug.Log("Player đã vào vùng lửa!");
        }
    }

    // Khi Player thoát khỏi vùng lửa
    private void OnTriggerExit(Collider other)
    {
        if (!Object.HasStateAuthority) return;

        HealthHandler hp = other.GetComponent<HealthHandler>();
        if (hp != null && _playersInZone.Contains(hp))
        {
            _playersInZone.Remove(hp);
            Debug.Log("Player đã thoát khỏi vùng lửa.");
        }
    }

    private IEnumerator DealDamageOverTime()
    {
        while (true)
        {
            // Đợi theo khoảng thời gian tickInterval
            yield return new WaitForSeconds(tickInterval);

            // Duyệt qua danh sách và gây sát thương
            for (int i = _playersInZone.Count - 1; i >= 0; i--)
            {
                if (_playersInZone[i] != null)
                {
                    // Gọi RPC nhận sát thương từ script HealthHandler đã làm trước đó
                    _playersInZone[i].RPC_TakeDamage(damagePerTick);
                }
                else
                {
                    // Xóa nếu player bị biến mất (ví dụ: thoát game)
                    _playersInZone.RemoveAt(i);
                }
            }
        }
    }
}