using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("=== CẤU HÌNH DI CHUYỂN ===")]
    public float speed = 50f;          // Tốc độ bay
    public float lifeTime = 3f;        // Tự hủy sau 3s nếu không trúng gì

    [Header("=== HIỆU ỨNG VFX ===")]
    public GameObject explosionPrefab; // Prefab hiệu ứng nổ (VFX)

    void Start()
    {
        explosionPrefab.SetActive(false);
        // Tự động xóa viên đạn sau một khoảng thời gian để tránh rác RAM
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // Đạn bay theo hướng Z (local forward) của chính nó
        // Vì firePoint.rotation đã được gán khi Instantiate, đạn sẽ bay thẳng tới trước nòng
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Kiểm tra nếu chạm vào kẻ địch hoặc địa hình
        if (other.CompareTag("Enemy") || other.CompareTag("Terrain"))
        {   
            explosionPrefab.SetActive(true);
            Explode();
        }
    }

    void Explode()
    {
        // 1. Sinh hiệu ứng nổ tại vị trí hiện tại
        if (explosionPrefab != null)
        {
            GameObject vfx = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            
            // Tự xóa hiệu ứng nổ sau khi chạy xong (ví dụ 2 giây)
            Destroy(vfx, 2f);
        }

        // 2. Xóa viên đạn ngay lập tức
        Destroy(gameObject);
    }
}