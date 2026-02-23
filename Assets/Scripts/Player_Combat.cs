using UnityEngine;

public class Player_Combat : MonoBehaviour
{
    private Animator anim;
    [Header("References")]
    public PlayerManager settings; // assign in inspector
    public PlayerWeapon weaponScript; // assign weapon GameObject's PlayerWeapon component
    [Header("Auto Hit Window")]
    public float hitEnableStart = 0.2f; // normalizedTime start to enable weapon
    public float hitEnableEnd = 0.5f;   // normalizedTime end to disable weapon
    bool weaponWindowActive = false;

    [Header("Atk, Combo")]
    public float cooldownTime = 0.5f;     // Giảm xuống một chút cho mượt
    private float nextFireTime = 0f;
    public static int noOfClicks = 0;           // Bỏ static nếu không cần dùng ở script khác
    float lastClickedTime = 0f;
    float maxComboDelay = 1f;
    public static bool isAttacking;
    public int currentAttackStage = 0; // 1,2,3

    [Header("Shield Block")]
    public static bool isBlock;

    private void Start()
    {
        settings.currentHealth = settings.maxHealth; // Ensure health is initialized
        settings.currentStamina = settings.maxStamina; // Ensure stamina is initialized
        anim = GetComponentInChildren<Animator>();
        if (weaponScript != null) weaponScript.DisableWeapon();
    }

    void Update()
    {
        Atk();
        Block();
    }

    void Block()
    {
        if (Input.GetKey(KeyCode.F))
        {
            if (!isBlock)
            {
                anim.SetBool("Block", true);
                isBlock = true;
            }
        }
        else
        {
            anim.SetBool("Block", false);
            isBlock = false;
        }
    }

    void Atk()
    {
        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        isAttacking = stateInfo.IsName("hit1") || stateInfo.IsName("hit2") || stateInfo.IsName("hit3");

        // Auto-enable/disable weapon collider during hit animations (no Animation Events needed)
        if (weaponScript != null)
        {
            int animStage = 0;
            if (stateInfo.IsName("hit1")) animStage = 1;
            else if (stateInfo.IsName("hit2")) animStage = 2;
            else if (stateInfo.IsName("hit3")) animStage = 3;

            if (animStage > 0)
            {
                float t = stateInfo.normalizedTime % 1f; // handle >1 normalizedTime
                if (t >= hitEnableStart && t <= hitEnableEnd)
                {
                    if (!weaponWindowActive)
                    {
                        weaponWindowActive = true;
                        weaponScript.EnableWeapon(currentAttackStage, this);
                    }
                }
                else if (weaponWindowActive)
                {
                    weaponWindowActive = false;
                    weaponScript.DisableWeapon();
                }
            }
            else if (weaponWindowActive)
            {
                weaponWindowActive = false;
                weaponScript.DisableWeapon();
            }
        }

        // --- PHẦN 1: LOGIC CHUYỂN COMBO (Update liên tục để bắt đúng thời điểm) ---
        
        // Nếu đang ở hit1, gần xong (70%) và người chơi đã bấm ít nhất 2 lần -> Sang hit2
        if (stateInfo.IsName("hit1") && stateInfo.normalizedTime > 0.7f)
        {
            anim.SetBool("atk1", false);
            if (noOfClicks >= 2) anim.SetBool("atk2", true);
            if (noOfClicks >= 2) currentAttackStage = 2;
        }

        // Nếu đang ở hit2, gần xong và người chơi đã bấm 3 lần -> Sang hit3
        if (stateInfo.IsName("hit2") && stateInfo.normalizedTime > 0.7f)
        {
            anim.SetBool("atk2", false);
            if (noOfClicks >= 3) anim.SetBool("atk3", true);
            if (noOfClicks >= 3) currentAttackStage = 3;
        }

        // Nếu đang ở hit3, gần xong -> Reset hết
        if (stateInfo.IsName("hit3") && stateInfo.normalizedTime > 0.7f)
        {
            anim.SetBool("atk3", false);
            noOfClicks = 0;
            currentAttackStage = 0;
        }

        // --- PHẦN 2: RESET NẾU ĐỨNG YÊN QUÁ LÂU ---
        if (Time.time - lastClickedTime > maxComboDelay)
        {
            noOfClicks = 0;
            anim.SetBool("atk1", false);
            anim.SetBool("atk2", false);
            anim.SetBool("atk3", false);
        }

        // --- PHẦN 3: NHẬN INPUT ---
        if (Input.GetMouseButtonDown(0) && Time.time > nextFireTime && !isBlock)
        {
            OnClick();
        }
    }

    void OnClick()
    {
        lastClickedTime = Time.time;
        noOfClicks++;
        noOfClicks = Mathf.Clamp(noOfClicks, 0, 3);

        if (noOfClicks == 1)
        {
            anim.SetBool("atk1", true);
            currentAttackStage = 1;
        }
    }

    // Called by animation events on attack animations to enable/disable weapon collider
    public void EnableWeapon()
    {
        if (weaponScript != null)
            weaponScript.EnableWeapon(currentAttackStage, this);
    }

    public void DisableWeapon()
    {
        if (weaponScript != null)
            weaponScript.DisableWeapon();
    }

    // Return damage value for given attack stage using PlayerManager settings
    public float GetDamageForStage(int stage)
    {
        if (settings == null) return 10f;
        switch (stage)
        {
            case 1: return settings.Atk1;
            case 2: return settings.Atk2;
            case 3: return settings.Atk3;
            default: return settings.Atk1;
        }
    }

    // Return the current combo animation name as string (e.g. "hit1","hit2","hit3")
    public string GetCurrentComboName()
    {
        if (anim == null) return "";
        var stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("hit1")) return "hit1";
        if (stateInfo.IsName("hit2")) return "hit2";
        if (stateInfo.IsName("hit3")) return "hit3";
        return "";
    }
}