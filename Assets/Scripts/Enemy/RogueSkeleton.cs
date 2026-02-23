using System.Collections;
using UnityEngine;

public class RogueSkeleton : EnemyBase
{
    [Header("Cấu hình Sát thương")]
    public float atk1Dmg = 15f;
    public float atk2Dmg = 25f;
    public float atkcd = 2.1f;

    [Header("Cấu hình Vũ khí")]
    public Collider weaponCollider; 

    private bool isAtking = false;
    private float currentCdTimer;
    private bool hasDealtDamage = false;
    private float currentAtkDmg;

    protected override void Start()
    {
        base.Start();
        currentCdTimer = atkcd;
        if (weaponCollider != null) weaponCollider.enabled = false;
    }

    public override void LookAtPlayer()
    {
        base.LookAtPlayer();

        if (currentCdTimer > 0)
            currentCdTimer -= Time.deltaTime;

        // Chỉ cho phép bắt đầu đòn đánh mới nếu đang không trong quá trình Atk
        if (currentCdTimer <= 0 && !isAtking && currentState == State.Atk)
        {
            StartAtk();
        }
    }

    void StartAtk()
    {
        isAtking = true; // Đánh dấu đang tấn công
        hasDealtDamage = false; 
        currentCdTimer = atkcd; // Reset hồi chiêu ngay khi bắt đầu tung đòn

        int randomAtk = Random.Range(0, 6);
        if (randomAtk <= 3) // 70% cơ hội đánh thường, 30% cơ hội đánh mạnh
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

    // GỌI QUA ANIMATION EVENT (Bắt đầu vung)
    public void EnableWeapon()
    {
        if (weaponCollider != null) 
            weaponCollider.enabled = true;
    }

    // GỌI QUA ANIMATION EVENT (Kết thúc vung)
    public void DisableWeapon()
    {
        if (weaponCollider != null) 
            weaponCollider.enabled = false;
        
        isAtking = false; // QUAN TRỌNG: Giải phóng trạng thái để có thể đánh đòn tiếp theo
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