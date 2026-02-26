using System.Collections;
using UnityEngine;

public class MageSkeleton : EnemyBase
{
    [Header("Cài đặt Pháp sư")]
    public AudioSource audioSource;
    public string projectileTag = "Fireball"; // Tag của cầu lửa trong Pool
    public Transform firePoint;               // Vị trí xuất hiện phép thuật
    public float atkDmg = 25f;                // Sát thương phép
    public float atkcd = 4f;                  // Thời gian hồi chiêu
    public float castDelay = 0.5f;            // Thời gian chờ trong anim để tung chiêu

    private float currentCdTimer;
    private bool isAtking = false;

    protected override void Start()
    {
        audioSource.loop = false;
        audioSource.playOnAwake = false;
        base.Start();
        currentCdTimer = atkcd; 
    }

    public override void LookAtPlayer()
    {
        base.LookAtPlayer();

        // Đếm ngược hồi chiêu
        if (currentCdTimer > 0)
        {
            currentCdTimer -= Time.deltaTime;
        }

        // Kiểm tra điều kiện tấn công
        if (currentCdTimer <= 0 && !isAtking && currentState == State.Atk)
        {
            StartCoroutine(AttackRoutine());
        }
    }

    IEnumerator AttackRoutine()
    {
        isAtking = true;

        // 1. Chỉ dùng 1 Trigger duy nhất cho toàn bộ quá trình tung phép
        animator.SetTrigger("Atk1"); 

        // 2. Chờ một khoảng thời gian ngắn để khớp với động tác vung gậy trong Anim
        yield return new WaitForSeconds(castDelay);

        // 3. Triệu hồi kỹ năng từ Pool
        if (ProjectilePool.Instance != null && firePoint != null)
        {
            ProjectileBase magic = ProjectilePool.Instance.Get(projectileTag, firePoint.position, firePoint.rotation);
            
            if (magic != null)
            {
                magic.damage = atkDmg;
            }
        }
        if(audioSource.isPlaying == false)
        {
            audioSource.Play();
        }
        // 4. Reset hồi chiêu và kết thúc trạng thái
        currentCdTimer = atkcd;
        
        // Chờ nốt phần còn lại của Animation (nếu cần) trước khi cho phép bắn phát tiếp theo
        // Ví dụ: Nếu anim dài 1.5s, castDelay mất 0.5s thì chờ thêm 1.0s
        yield return new WaitForSeconds(0.5f); 
        
        isAtking = false;
    }
}