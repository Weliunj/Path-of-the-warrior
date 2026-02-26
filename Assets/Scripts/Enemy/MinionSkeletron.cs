using System.Collections;
using UnityEngine;

public class MinionSkeletron : EnemyBase
{
    [Header("Cài đặt tấn công")]
    public AudioSource audioSource;
    public string projectileTag = "Arrow"; // Khớp với Tag trong ProjectilePool
    public Transform firePoint;            // Điểm xuất phát của đạn (ví dụ: đầu cung)
    public float atkDmg = 15f;             // Sát thương riêng của con quái này
    public float atkcd = 5f;               // Thời gian hồi chiêu
    
    private float currentCdTimer;          // Bộ đếm thời gian hồi chiêu thực tế
    private bool isAtking = false;

    protected override void Start()
    {
        audioSource.loop = false;
        audioSource.playOnAwake = false;
        base.Start();
        currentCdTimer = atkcd; // Khởi tạo hồi chiêu
    }

    public override void LookAtPlayer()
    {
        // Luôn xoay về phía Player (logic từ lớp cha)
        base.LookAtPlayer();

        // Xử lý đếm ngược hồi chiêu
        if (currentCdTimer > 0)
        {
            currentCdTimer -= Time.deltaTime;
        }

        // Kiểm tra điều kiện bắn: Hết hồi chiêu + Chưa trong trạng thái bắn + Đang ở trạng thái Atk
        if (currentCdTimer <= 0 && !isAtking && currentState == State.Atk)
        {
            StartCoroutine(AtkRoutine());
        }
    }

    IEnumerator AtkRoutine()
    {
        isAtking = true;

        // 1. Giai đoạn nạp đạn/vận công
        animator.SetTrigger("AtkS"); 
        yield return new WaitForSeconds(1.1f); // Chờ đến thời điểm đạn thực sự bay ra

        // 2. Giai đoạn bắn đạn (Gọi từ Pool)
        if (ProjectilePool.Instance != null && firePoint != null)
        {
            // Lấy đạn từ kho theo Tag ("Arrow" hoặc "Ammo")
            ProjectileBase projectile = ProjectilePool.Instance.Get(projectileTag, firePoint.position, firePoint.rotation);
            
            if (projectile != null)
            {
                // Gán sát thương của con quái này cho viên đạn vừa lấy ra
                projectile.damage = atkDmg;
            }
        }

        // 3. Giai đoạn kết thúc/Hồi chiêu
        animator.SetTrigger("AtkR"); 
        if(audioSource.isPlaying == false)
        {
            audioSource.Play();
        }
        currentCdTimer = atkcd; // Reset bộ đếm hồi chiêu
        isAtking = false;
    }
}