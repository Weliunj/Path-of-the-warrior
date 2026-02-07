using Unity.Mathematics;
using UnityEngine;
using UnityEngine.VFX;

// Đảm bảo đối tượng luôn có Rigidbody để xử lý va chạm tốt hơn transform.Translate
[RequireComponent(typeof(Rigidbody))]
public class ProjectileBase : MonoBehaviour
{
    [Header("Cấu hình chung")] // General Configuration
    public VisualEffect explode;
    public float speed = 20f;
    public float damage = 10f;
    public float lifeTime = 3f;
    
    [Header("Cài đặt vật lý")]
    public bool useGravity = false;
    [Tooltip("Dùng cho mũi tên để đầu luôn hướng về hướng bay")] // Used for arrows so the head always points in the direction of flight
    public bool rotateToVelocity = true;

    protected Rigidbody rb;
    private float timer;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        // Chế độ này giúp đạn bay nhanh không bị xuyên vật thể (Tunneling) // This mode helps fast projectiles not to tunnel through objects
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    protected virtual void OnEnable()
    {
        explode = GetComponent<VisualEffect>();
        timer = lifeTime;
        rb.useGravity = useGravity;
        
        // Bắn đạn đi theo hướng forward của vật thể // Shoot the projectile in the object's forward direction
        rb.linearVelocity = transform.forward * speed; 
    }

    protected virtual void Update()
    {
        // Tự động tắt sau một khoảng thời gian // Automatically deactivate after a certain time
        timer -= Time.deltaTime;
        if (timer <= 0) Deactivate();

        // Nếu là mũi tên, xoay đầu theo đường cong của trọng lực // If it's an arrow, rotate its head along the curve of gravity
        if (useGravity && rotateToVelocity && rb.linearVelocity != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(rb.linearVelocity);
        }
    }

    // Xử lý va chạm
    // Collision handling
    protected virtual void OnTriggerEnter(Collider other)
{
    if (other.CompareTag("Player"))
    {  
        // 1. Tạo hiệu ứng nổ tại vị trí va chạm
        // Nên lấy từ một Pool riêng cho VFX để tối ưu
        if (explode != null)
        {
        }

        Debug.Log($"Đã trúng {other.name} gây {damage} sát thương");
    }

    // 2. Mũi tên biến mất và quay về Pool
    Deactivate();
}

    protected void Deactivate()
    {
        // Đưa đạn về trạng thái nghỉ trước khi trả vào Pool // Reset projectile to inactive state before returning to pool
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        gameObject.SetActive(false);
    }
}