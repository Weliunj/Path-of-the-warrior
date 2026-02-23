using System.Collections;
using UnityEngine;

public class WarriorSkeleton : EnemyBase
{
    [Header("Cấu hình Tấn công (Chậm & Mạnh)")]
    public float atk1Dmg = 35f; 
    public float atk2Dmg = 50f; // Đòn 2 cực mạnh
    public float atkcd = 3.0f;  // Thời gian hồi chiêu lâu
    public Collider weaponCollider;

    [Header("Cấu hình Đỡ đòn (Block Trigger)")]
    [Range(0, 100)]
    public float blockChance = 25f;    
    public float blockDuration = 1.0f; // Thời gian "cứng" khi đang diễn anim block
    public float damageReduction = 0.7f; // Giảm 70% sát thương (Warrior thủ tốt hơn)
    public float blockCD = 4f;

    private bool isAtking = false;
    private bool isBlocking = false;
    private float currentAtkCd;
    private float currentBlockCd;
    private bool hasDealtDamage = false;
    private float currentAtkDmg;

    protected override void Start()
    {
        base.Start();
        currentAtkCd = atkcd;
        if (weaponCollider != null) weaponCollider.enabled = false;
    }

    public override void LookAtPlayer()
    {
        base.LookAtPlayer();

        // Đếm hồi chiêu
        if (currentAtkCd > 0) currentAtkCd -= Time.deltaTime;
        if (currentBlockCd > 0) currentBlockCd -= Time.deltaTime;

        // Ưu tiên Block khi ở trạng thái Idle và ngẫu nhiên trúng tỉ lệ
        if (currentBlockCd <= 0 && !isAtking && !isBlocking && currentState == State.Idle)
        {
            if (Random.Range(0, 100) < blockChance)
            {
                StartCoroutine(BlockRoutine());
                return;
            }
        }

        // Tấn công ngẫu nhiên 2 đòn
        if (currentAtkCd <= 0 && !isAtking && !isBlocking && currentState == State.Atk)
        {
            StartAtk();
        }
    }

    void StartAtk()
    {
        isAtking = true;
        hasDealtDamage = false;
        currentAtkCd = atkcd;

        // Random giữa Atk1 và Atk2
        int randomAtk = Random.Range(0, 2);
        if (randomAtk == 0)
        {
            currentAtkDmg = atk1Dmg;
            animator.SetTrigger("Atk1");
        }
        else
        {
            currentAtkDmg = atk2Dmg;
            animator.SetTrigger("Atk2");
        }
    }

    // --- ANIMATION EVENTS ---
    public void EnableWeapon() 
    {
        if (weaponCollider != null) weaponCollider.enabled = true;
    }

    public void DisableWeapon() 
    {
        if (weaponCollider != null) weaponCollider.enabled = false;
        isAtking = false;
    }

    // --- LOGIC BLOCK (TRIGGER) ---
    IEnumerator BlockRoutine()
    {
        isBlocking = true;
        currentBlockCd = blockCD;

        animator.SetTrigger("Block"); // Sử dụng Trigger thay vì Bool
        
        // Warrior sẽ ở trạng thái "Blocking" trong suốt thời gian diễn anim
        yield return new WaitForSeconds(blockDuration);

        isBlocking = false;
    }

    public void TakeDamage(float incomingDamage)
    {
        float finalDamage = incomingDamage;

        if (isBlocking)
        {
            finalDamage *= (1 - damageReduction);
            Debug.Log($"Warrior đã Block! Giảm từ {incomingDamage} xuống {finalDamage}");
            // Bạn có thể kích hoạt hiệu ứng âm thanh "Keng" ở đây
        }

        // Trừ máu vào logic Health của bạn ở đây
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (!isAtking || hasDealtDamage) return;

        if (collision.CompareTag("Player"))
        {
            Debug.Log($"Warrior chém trúng! Sát thương: {currentAtkDmg}");
            hasDealtDamage = true;
            // collision.GetComponent<PlayerHealth>()?.TakeDamage(currentAtkDmg);
            if (weaponCollider != null) weaponCollider.enabled = false;
        }
    }
}