using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class MainMenu : MonoBehaviour
{
    [Header("Menu Panels")]
    public GameObject mainMenuPanel;
    public GameObject settingMenuPanel;

    [Header("UI Audio")]
    public AudioSource uiClickAudio;
    public AudioClip uiClickClip;
    public float uiClickVolume = 1f;

    private List<Button> hookedButtons = new List<Button>();

    [Header("Managers")]
    public PlayerManager playerManager;
    public SettingManager settingManager;

    [Header("Admin Sliders")]
    public Slider moveSlider;
    public Slider sprintSlider;
    public Slider rotSlider;
    public Slider jumpSlider;
    public Slider atkBaseSlider;
    public Slider armorSlider;
    public Slider blockRedSlider;
    // Bổ sung các Slider bị thiếu
    public Slider maxHpSlider;
    public Slider maxStaminaSlider;

    void Start()
    {
        Time.timeScale = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        ShowMainMenu();
        RegisterAllButtonClickSounds();
        
        // Load dữ liệu từ máy tính lên SettingManager trước
        settingManager.LoadSettings();
        // Cập nhật giá trị từ SettingManager lên các thanh kéo UI
        UpdateSlidersUI();
    }

    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        settingMenuPanel.SetActive(false);
    }

    public void OnPlayButton()
    {
        // Quan trọng: Áp dụng cấu hình hiện tại vào Player trước khi load cảnh
        ApplyToPlayer();
        SceneManager.LoadScene("World");
        Time.timeScale = 1; 
    }

    public void OnSettingButton()
    {
        mainMenuPanel.SetActive(false);
        settingMenuPanel.SetActive(true);
    }

    public void OnBackButton()
    {
        settingMenuPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void OnExitButton()
    {
        Debug.Log("Exiting Game...");
        Application.Quit();
    }

    // --- Hệ thống âm thanh ---
    void RegisterAllButtonClickSounds()
    {
        var buttons = FindObjectsOfType<Button>(true);
        foreach (var b in buttons)
        {
            b.onClick.AddListener(PlayClickSound);
            hookedButtons.Add(b);
        }
    }

    void PlayClickSound()
    {
        if (uiClickAudio == null) return;
        if (uiClickClip != null)
            uiClickAudio.PlayOneShot(uiClickClip, uiClickVolume);
        else
            uiClickAudio.Play();
    }

    void OnDestroy()
    {
        foreach (var b in hookedButtons)
        {
            if (b != null) b.onClick.RemoveListener(PlayClickSound);
        }
    }

    // Hàm Reset: Trả về Full phiên bản ban đầu
    public void OnResetToDefault()
    {
        settingManager.moveSpeed = 4.0f;
        settingManager.sprintSpeed = 9.0f;
        settingManager.rotationSpeed = 1.0f;
        settingManager.jumpHeight = 1.2f;
        settingManager.baseAtk = 10f;
        settingManager.atk2Multiplier = 1.2f;
        settingManager.atk3Multiplier = 1.6f;
        settingManager.maxHealth = 300f;
        settingManager.maxStamina = 200f;
        settingManager.armor = 0f;
        settingManager.blockReductionPercent = 0.5f;

        UpdateSlidersUI();
        Debug.Log("Đã khôi phục cài đặt gốc!");
    }

    // Cập nhật giá trị từ SettingManager lên Slider
    void UpdateSlidersUI()
    {
        if (moveSlider) moveSlider.value = settingManager.moveSpeed;
        if (sprintSlider) sprintSlider.value = settingManager.sprintSpeed;
        if (rotSlider) rotSlider.value = settingManager.rotationSpeed;
        if (jumpSlider) jumpSlider.value = settingManager.jumpHeight;
        if (atkBaseSlider) atkBaseSlider.value = settingManager.baseAtk;
        if (armorSlider) armorSlider.value = settingManager.armor;
        if (blockRedSlider) blockRedSlider.value = settingManager.blockReductionPercent;
        
        // Cập nhật các Slider bổ sung
        if (maxHpSlider) maxHpSlider.value = settingManager.maxHealth;
        if (maxStaminaSlider) maxStaminaSlider.value = settingManager.maxStamina;
    }

    public void OnSaveAndApply()
    {
        // Gán giá trị từ UI vào SettingManager
        settingManager.moveSpeed = moveSlider.value;
        settingManager.sprintSpeed = sprintSlider.value;
        settingManager.rotationSpeed = rotSlider.value;
        settingManager.jumpHeight = jumpSlider.value;
        settingManager.baseAtk = atkBaseSlider.value;
        settingManager.armor = armorSlider.value;
        settingManager.blockReductionPercent = blockRedSlider.value;
        
        // Gán các giá trị bổ sung
        if (maxHpSlider) settingManager.maxHealth = maxHpSlider.value;
        if (maxStaminaSlider) settingManager.maxStamina = maxStaminaSlider.value;

        settingManager.SaveSettings();
        ApplyToPlayer();
    }

    // Áp dụng vào Player thực tế (ScriptableObject)
    void ApplyToPlayer()
    {
        playerManager.MoveSpeed = settingManager.moveSpeed;
        playerManager.SprintSpeed = settingManager.sprintSpeed;
        playerManager.RotationSpeed = settingManager.rotationSpeed;
        playerManager.JumpHeight = settingManager.jumpHeight;
        playerManager.baseAtk = settingManager.baseAtk;
        playerManager.atk2Multiplier = settingManager.atk2Multiplier;
        playerManager.atk3Multiplier = settingManager.atk3Multiplier;
        playerManager.maxHealth = settingManager.maxHealth;
        playerManager.maxStamina = settingManager.maxStamina;
        playerManager.armor = settingManager.armor;
        playerManager.blockReductionPercent = settingManager.blockReductionPercent;
        
        // Đảm bảo máu hiện tại không vượt quá máu tối đa mới
        playerManager.currentHealth = playerManager.maxHealth;
        playerManager.currentStamina = playerManager.maxStamina;
    }
}