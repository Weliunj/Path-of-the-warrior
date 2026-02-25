using UnityEngine;

[CreateAssetMenu(fileName = "SettingManager", menuName = "StarterAssets/SettingManager")]
public class SettingManager : ScriptableObject
{
    [Header("Movement")]
    public float moveSpeed = 4.0f;
    public float sprintSpeed = 6.0f;
    public float rotationSpeed = 1.0f;

    [Header("Physics")]
    public float jumpHeight = 1.2f;

    [Header("Combat & Multipliers")]
    public float baseAtk = 10f;
    public float atk2Multiplier = 1.2f;
    public float atk3Multiplier = 1.2f;

    [Header("Vitals & Defense")]
    public float maxHealth = 200f;
    public float maxStamina = 100f;
    public float armor = 0f;
    public float blockReductionPercent = 0.5f;

    // Lưu vào máy tính
    public void SaveSettings()
    {
        PlayerPrefs.SetFloat("Set_Move", moveSpeed);
        PlayerPrefs.SetFloat("Set_Sprint", sprintSpeed);
        PlayerPrefs.SetFloat("Set_Rot", rotationSpeed);
        PlayerPrefs.SetFloat("Set_JumpH", jumpHeight);
        PlayerPrefs.SetFloat("Set_AtkBase", baseAtk);
        PlayerPrefs.SetFloat("Set_Atk2Mult", atk2Multiplier);
        PlayerPrefs.SetFloat("Set_Atk3Mult", atk3Multiplier);
        PlayerPrefs.SetFloat("Set_MaxHP", maxHealth);
        PlayerPrefs.SetFloat("Set_MaxStam", maxStamina);
        PlayerPrefs.SetFloat("Set_Armor", armor);
        PlayerPrefs.SetFloat("Set_BlockRed", blockReductionPercent);
        PlayerPrefs.Save();
        Debug.Log("Đã lưu cấu hình Admin!");
    }

    // Tải từ máy tính
    public void LoadSettings()
    {
        moveSpeed = PlayerPrefs.GetFloat("Set_Move", 4.0f);
        sprintSpeed = PlayerPrefs.GetFloat("Set_Sprint", 6.0f);
        rotationSpeed = PlayerPrefs.GetFloat("Set_Rot", 1.0f);
        jumpHeight = PlayerPrefs.GetFloat("Set_JumpH", 1.2f);
        baseAtk = PlayerPrefs.GetFloat("Set_AtkBase", 10f);
        atk2Multiplier = PlayerPrefs.GetFloat("Set_Atk2Mult", 1.2f);
        atk3Multiplier = PlayerPrefs.GetFloat("Set_Atk3Mult", 1.2f);
        maxHealth = PlayerPrefs.GetFloat("Set_MaxHP", 200f);
        maxStamina = PlayerPrefs.GetFloat("Set_MaxStam", 100f);
        armor = PlayerPrefs.GetFloat("Set_Armor", 0f);
        blockReductionPercent = PlayerPrefs.GetFloat("Set_BlockRed", 0.5f);
    }
}