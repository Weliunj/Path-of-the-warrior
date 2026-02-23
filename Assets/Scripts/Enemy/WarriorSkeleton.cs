using System.Collections;
using UnityEngine;

public class WarriorSkeleton : EnemyBase
{
    [Header("Cấu hình Tấn công (Chậm & Mạnh)")]
    public float atk1Dmg = 35f;
    public float atk2Dmg = 50f;
    public float atkcd = 3.0f;
    public Collider weaponCollider;

    [Header("Cấu hình Đỡ đòn (Block)")]
    [Range(0, 100)]
    public float blockChance = 30f;    // Tỉ lệ %
    public float blockDuration = 1.0f; 
    public float damageReduction = 0.7f; // Giảm 70% sát thương
    public float blockCD = 5f;

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

    protected override void Update()
    {
        // Chạy logic cooldown của lớp con
        if (currentAtkCd > 0f) currentAtkCd -= Time.deltaTime;
        if (currentBlockCd > 0f) currentBlockCd -= Time.deltaTime;

        // Gọi Update của lớp cha (EnemyBase) để chạy Vision và State Machine
        base.Update();
    }

    // --- LOGIC BLOCK ĐÃ CÂN BẰNG ---
    public override void LookAtPlayer()
    {
        base.LookAtPlayer();
        if (isDead) return;

        // Chỉ kiểm tra Block khi hết CD và ĐANG KHÔNG THỰC HIỆN HÀNH ĐỘNG KHÁC
        if (currentBlockCd <= 0 && !isAtking && !isBlocking)
        {
            // QUAN TRỌNG: Ngay khi vào đây, ta bắt đầu hồi chiêu một chút 
            // để quái không kiểm tra Random liên tục mỗi frame
            currentBlockCd = 1.0f; 

            if (Random.Range(0f, 100f) < blockChance)
            {
                StartCoroutine(BlockRoutine());
                return; 
            }
        }

        // Tấn công
        if (currentAtkCd <= 0 && !isAtking && !isBlocking && currentState == State.Atk)
        {
            StartAtk();
        }
    }

    public override void IdleLogic()
    {
        base.IdleLogic();
        // Cho phép quái thỉnh thoảng đứng thủ khi đang đi tuần/idle
        if (currentBlockCd <= 0 && !isAtking && !isBlocking)
        {
            if (Random.Range(0f, 100f) < blockChance * 0.5f) // Giảm tỉ lệ khi idle để đỡ bị spam
            {
                StartCoroutine(BlockRoutine());
            }
        }
    }

    void StartAtk()
    {
        isAtking = true;
        hasDealtDamage = false;
        currentAtkCd = atkcd;

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

    // --- LOGIC NHẬN SÁT THƯƠNG (OVERRIDE) ---
    public override void TakeDamage(float incomingDamage)
    {
        float finalDamage = incomingDamage;

        // Nếu trúng đòn mà chưa kịp block chủ động, thử phản xạ block (Reactive Block)
        if (!isBlocking && currentBlockCd <= 0f)
        {
            if (Random.Range(0f, 100f) < blockChance)
            {
                finalDamage *= (1f - damageReduction);
                Debug.Log($"<color=blue>Warrior Phản xạ Block!</color> Giảm còn: {finalDamage}");
                StartCoroutine(HitBlockRoutine(blockDuration));
                currentBlockCd = blockCD;
            }
        }
        else if (isBlocking)
        {
            // Đang thủ sẵn (Proactive Block)
            finalDamage *= (1f - damageReduction);
            Debug.Log($"<color=cyan>Warrior Đỡ đòn thành công!</color> Giảm còn: {finalDamage}");
        }

        // Gọi hàm trừ máu và xử lý chết từ lớp cha
        base.TakeDamage(finalDamage);
    }

    // --- ANIMATION EVENTS ---
    public void EnableWeapon() { if (weaponCollider != null) weaponCollider.enabled = true; }
    public void DisableWeapon() { if (weaponCollider != null) weaponCollider.enabled = false; isAtking = false; }

    // --- COROUTINES ---
    IEnumerator BlockRoutine()
    {
        isBlocking = true;
        currentBlockCd = blockCD; 

        // Dừng quái lại khi đang thủ
        if(agent != null) agent.isStopped = true;

        animator.SetBool("Block", true);
        
        yield return new WaitForSeconds(blockDuration);

        animator.SetBool("Block", false);
        
        // GIẢI VÂY: Cho phép quái di chuyển tiếp
        if(agent != null && !isDead) 
        {
            agent.isStopped = false;
        }

        isBlocking = false;
    }

    IEnumerator HitBlockRoutine(float duration)
    {
        isBlocking = true;
        animator.SetBool("Block", true);
        
        // Làm chậm quái khi bị đánh trúng lúc đang đỡ
        float oldSpeed = agent.speed;
        agent.speed *= 0.2f;

        yield return new WaitForSeconds(duration);

        agent.speed = oldSpeed;
        animator.SetBool("Block", false);
        isBlocking = false;
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (!isAtking || hasDealtDamage) return;

        if (collision.CompareTag("Player"))
        {
            playerManager.ExecuteDamage(currentAtkDmg);
            hasDealtDamage = true;
            if (weaponCollider != null) weaponCollider.enabled = false;
        }
    }
}