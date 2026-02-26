using System.Collections;
using UnityEngine;
using UnityEngine.XR;

public class PlayerHandleStats : MonoBehaviour
{
    public PlayerManager manager; 
    private StarterAssets.StarterAssetsInputs starterInput;
    private PlayerAtr atr;

    [Header("Stamina Settings")]
    float Basestamina = 0f;

    public float sprintDrainRate = 12f; 
    // Đã chỉnh sửa: 8f sẽ giúp chặn đòn trụ được lâu hơn chạy nhanh (12f)
    public float blockDrainRate = 8f;   
    public float staminaRegenRate = 10f; 
    
    // Thêm biến delay để hồi thể lực (tùy chọn để game hay hơn)
    private float regenDelayTimer;
    public float regenDelay = 1.0f;

    [Header("Defense Settings")]
    float Basearmor = 0;

    [Header("Movement Settings")]
    float Basemovespeed = 0;
    float Basesprintspeed = 0;

    [Header("Defense Settings")]
    float BaseStreg = 0;

    void Start()
    {   
        atr = GetComponent<PlayerAtr>();
        starterInput = GetComponent<StarterAssets.StarterAssetsInputs>();
        if (manager != null)
        {
            manager.currentHealth = manager.maxHealth;
            manager.currentStamina = manager.maxStamina;
            Basemovespeed = manager.MoveSpeed;
            Basesprintspeed = manager.SprintSpeed;
            Basestamina = manager.maxStamina;
            Basearmor = manager.armor;
            BaseStreg = manager.baseAtk;
        }
    }

    public void ExecuteDamage(float amount)
    {
        if (amount <= 0) return;

        // 1. LẤY GIÁ TRỊ GIÁP TỪ HỆ THỐNG ATTRIBUTE
        // Giả sử chỉ số thứ 2 (index 2) trong mảng attributes của bạn là Armor/Defense
        
        float value = atr.attributes[2].value.ModifiedValue;
        float totalArmor = manager.armor = value + Basearmor;
        // 2. TÍNH TOÁN GIẢM SÁT THƯƠNG THEO CÔNG THỨC GIÁP
        // Công thức: Dmg thực = Dmg gốc * (100 / (100 + Giáp))
        float reductionMultiplier = 100f / (100f + totalArmor);
        float damageAfterArmor = amount * reductionMultiplier;

        float RealDmg = damageAfterArmor;

        // 3. TÍNH TOÁN NẾU ĐANG BLOCK (GIẢM THÊM LẦN NỮA)
        if (Player_Combat.isBlock && manager != null && manager.currentStamina > 0.1f)
        {
            // Giảm tiếp dựa trên phần trăm giảm của khiên/vũ khí
            RealDmg *= (1f - Mathf.Clamp01(manager.blockReductionPercent));

            // Khi bị đánh trúng lúc đang block, reset timer hồi stamina
            regenDelayTimer = regenDelay;

            // Trừ stamina khi đỡ thành công
            float extraPercent = Random.Range(0.10f, 0.15f);
            float extraDrain = extraPercent * manager.maxStamina;
            manager.currentStamina = Mathf.Max(0f, manager.currentStamina - extraDrain);
            
            if (manager.currentStamina <= 0f) Player_Combat.isBlock = false;
        }

        // 4. ÁP DỤNG SÁT THƯƠNG CUỐI CÙNG VÀO MÁU
        if (manager != null)
        {
            manager.currentHealth -= RealDmg;
            manager.currentHealth = Mathf.Max(0f, manager.currentHealth);
            
            Debug.Log($"Gốc: {amount} | Sau Giáp ({totalArmor}): {damageAfterArmor:F1} | Sau Block: {RealDmg:F1}");
            
            if (manager.currentHealth <= 0f) Die();
        }
    }

    void Update()
    {
        HandArmor();
        HandleStamina();
        HandleSpeed();
        HandleStreg();
    }

    void HandArmor()
    {
        if (manager == null || atr == null) return;

        // Attribute[2] used as flat armor bonus in ExecuteDamage (value + Basearmor)
        float value = atr.attributes[2].value.ModifiedValue;
        manager.armor = Basearmor + value;
    }
    void HandleSpeed()
    {
        if (manager == null || atr == null) return;

        // Treat attribute[0] as a percent bonus (e.g., 20 means +20%)
        float modifier = 1f + (atr.attributes[0].value.ModifiedValue / 100f);
        manager.MoveSpeed = Basemovespeed * modifier;
        manager.SprintSpeed = Basesprintspeed * modifier;
    }

    void HandleStreg()
    {
        if (manager == null || atr == null) return;

        // Attribute[3] is attack percent bonus (e.g., 20 -> +20%)
        float modifier = 1f + (atr.attributes[3].value.ModifiedValue / 100f);
        manager.baseAtk = BaseStreg * modifier;
    }
    void HandleStamina()
    {
        if (manager == null) return;

        // Attribute[1] is stamina percent bonus (e.g., 20 -> +20%)
        float modifier = 1f + (atr.attributes[1].value.ModifiedValue / 100f);
        manager.maxStamina = Basestamina * modifier;

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
        StartCoroutine(DisableAfterDelay(1f));
    }

    IEnumerator DisableAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }
}