using UnityEngine;

public class TurretDefense : MonoBehaviour
{
    [Header("=== CẤU HÌNH BẮN ===")]
    public string enemyTag = "Enemy";     // Tag của kẻ địch
    public float range = 15f;            // Tầm bắn
    public float fireRate = 1f;          // Tốc độ bắn (viên/giây)
    private float fireCountdown = 0f;

    [Header("=== TÀI NGUYÊN ===")]
    public GameObject bulletPrefab;      // Prefab viên đạn
    public Transform firePoint;          // Điểm xuất phát của đạn (đầu nòng)
    public Transform partToRotate;       // Phần quay của tháp pháo

    private Transform target;

    void Start()
    {
        // Quét mục tiêu mỗi 0.5 giây để tiết kiệm hiệu năng thay vì Update liên tục
        InvokeRepeating("UpdateTarget", 0f, 0.5f);
    }

    void UpdateTarget()
    {
        // Tìm tất cả đối tượng có tag "Enemy"
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        float shortestDistance = Mathf.Infinity;
        GameObject nearestEnemy = null;

        foreach (GameObject enemy in enemies)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            if (distanceToEnemy < shortestDistance)
            {
                shortestDistance = distanceToEnemy;
                nearestEnemy = enemy;
            }
        }

        // Nếu kẻ địch trong tầm bắn thì khóa mục tiêu
        if (nearestEnemy != null && shortestDistance <= range)
        {
            target = nearestEnemy.transform;
        }
        else
        {
            target = null;
        }
    }

    void Update()
    {
        if (target == null) return;

        // Xử lý quay đầu pháo về phía kẻ địch
        Vector3 dir = target.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(dir);
        Vector3 rotation = lookRotation.eulerAngles;
        partToRotate.rotation = Quaternion.Euler(0f, rotation.y, 0f);

        // Xử lý đếm ngược để bắn
        if (fireCountdown <= 0f)
        {
            Shoot();
            fireCountdown = 1f / fireRate;
        }

        fireCountdown -= Time.deltaTime;
    }

    void Shoot()
    {
        // Sinh đạn và bắn
        GameObject bulletGO = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        // Lưu ý: Bullet nên có script ProjectileComplete để xử lý va chạm và VFX
    }

    // --- DEBUG GIZMOS ---
    // Vẽ vòng tròn tầm bắn trong Scene để dễ căn chỉnh
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}