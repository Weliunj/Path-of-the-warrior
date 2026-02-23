using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public PlayerManager manager; 
    private StarterAssets.StarterAssetsInputs starterInput;

    [Header("Stamina Settings")]
    public float sprintDrainRate = 12f; 
    // Đã chỉnh sửa: 8f sẽ giúp chặn đòn trụ được lâu hơn chạy nhanh (12f)
    public float blockDrainRate = 8f;   
    public float staminaRegenRate = 10f; 
    
    // Thêm biến delay để hồi thể lực (tùy chọn để game hay hơn)
    private float regenDelayTimer;
    public float regenDelay = 1.0f;

    void Start()
    {
        starterInput = GetComponent<StarterAssets.StarterAssetsInputs>();
        if (manager != null)
        {
            manager.currentHealth = manager.maxHealth;
            manager.currentStamina = manager.maxStamina;
        }
    }

    public void ExecuteDamage(float amount)
    {
        if (amount <= 0) return;

        float RealDmg = amount;

        if (Player_Combat.isBlock && manager != null)
        {
            RealDmg *= (1f - Mathf.Clamp01(manager.blockReductionPercent));

            // Khi bị đánh trúng lúc đang block, reset timer hồi stamina
            regenDelayTimer = regenDelay;

            float extraPercent = Random.Range(0.10f, 0.20f);
            float extraDrain = extraPercent * manager.maxStamina;
            manager.currentStamina = Mathf.Max(0f, manager.currentStamina - extraDrain);
            
            if (manager.currentStamina <= 0f) Player_Combat.isBlock = false;
        }

        if (manager != null)
        {
            manager.currentHealth -= RealDmg;
            manager.currentHealth = Mathf.Max(0f, manager.currentHealth);
            if (manager.currentHealth <= 0f) Die();
        }
    }

    void Update()
    {
        HandleStamina();
    }

    void HandleStamina()
    {
        if (manager == null) return;

        bool isSprinting = false;
        if (starterInput != null)
        {
            isSprinting = starterInput.sprint && starterInput.move != Vector2.zero;
        }

        // --- TIÊU TỐN STAMINA ---
        if (isSprinting && manager.currentStamina > 0f)
        {
            manager.currentStamina -= sprintDrainRate * Time.deltaTime;
            regenDelayTimer = regenDelay; // Reset timer khi đang dùng sức
        }

        // Block tiêu tốn ít hơn Sprint (8f vs 12f)
        if (Player_Combat.isBlock && manager.currentStamina > 0f)
        {
            manager.currentStamina -= blockDrainRate * Time.deltaTime;
            regenDelayTimer = regenDelay; // Reset timer khi đang dùng sức
        }

        // --- HỒI STAMINA ---
        if (!isSprinting && !Player_Combat.isBlock)
        {
            if (regenDelayTimer > 0)
            {
                regenDelayTimer -= Time.deltaTime;
            }
            else
            {
                manager.currentStamina += staminaRegenRate * Time.deltaTime;
            }
        }

        manager.currentStamina = Mathf.Clamp(manager.currentStamina, 0f, manager.maxStamina);

        if (manager.currentStamina <= 0f && Player_Combat.isBlock)
            Player_Combat.isBlock = false;
    }

    void Die()
    {
        var anim = GetComponentInChildren<Animator>();
        if (anim != null) anim.SetTrigger("Died");
        StartCoroutine(DisableAfterDelay(2.5f));
    }

    IEnumerator DisableAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }
}