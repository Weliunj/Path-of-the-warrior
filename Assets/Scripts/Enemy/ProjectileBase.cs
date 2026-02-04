using UnityEngine;

// Đảm bảo đối tượng luôn có Rigidbody để xử lý va chạm tốt hơn transform.Translate
[RequireComponent(typeof(Rigidbody))]
public class ProjectileBase : MonoBehaviour
{
    [Header("Cấu hình chung")]
    public float speed = 20f;
    public float damage = 10f;
    public float lifeTime = 3f;
    
    [Header("Cài đặt vật lý")]
    public bool useGravity = false;
    [Tooltip("Dùng cho mũi tên để đầu luôn hướng về hướng bay")]
    public bool rotateToVelocity = true;

    protected Rigidbody rb;
    private float timer;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        // Chế độ này giúp đạn bay nhanh không bị xuyên vật thể (Tunneling)
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    protected virtual void OnEnable()
    {
        timer = lifeTime;
        rb.useGravity = useGravity;
        
        // Bắn đạn đi theo hướng forward của vật thể
        rb.linearVelocity = transform.forward * speed; 
    }

    protected virtual void Update()
    {
        // Tự động tắt sau một khoảng thời gian
        timer -= Time.deltaTime;
        if (timer <= 0) Deactivate();

        // Nếu là mũi tên, xoay đầu theo đường cong của trọng lực
        if (useGravity && rotateToVelocity && rb.linearVelocity != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(rb.linearVelocity);
        }
    }

    // Xử lý va chạm
    protected virtual void OnTriggerEnter(Collider other)
    {
        // Kiểm tra xem có chạm vào tường (Obstruct) hoặc mục tiêu không
        // Bạn có thể tùy chỉnh Tag hoặc Layer ở đây
        if (other.CompareTag("Player") || other.CompareTag("Enemy"))
        {
            // Gửi sát thương cho đối tượng bị trúng
            // other.GetComponent<IDamageable>()?.TakeDamage(damage);
            Debug.Log($"Đã trúng {other.name} gây {damage} sát thương");
        }

        // Đạn biến mất sau khi va chạm
        Deactivate();
    }

    protected void Deactivate()
    {
        // Đưa đạn về trạng thái nghỉ trước khi trả vào Pool
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        gameObject.SetActive(false);
    }
}