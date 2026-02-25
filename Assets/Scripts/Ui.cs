using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Ui : MonoBehaviour
{
    public static Ui Instance { get; private set; }

    [Header("Player Vitals UI")]
    public PlayerManager manager;
    public Slider hpSlider;
    public Slider staminaSlider;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI staminaText;

    [Header("Player Stats Text")]
    public TextMeshProUGUI MovespeedText;
    public TextMeshProUGUI SprintSpeedText;
    public TextMeshProUGUI ArmorText;

    [Header("Inventory Settings")]
    public RectTransform mainUiRect;      
    public RectTransform inventoryUiRect; 
    public TextMeshProUGUI itemDescriptionText;

    private bool isInventoryOpen = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); }
        else { Instance = this; }
    }

    void Start()
    {
        hpSlider.maxValue = manager.maxHealth;
        staminaSlider.maxValue = manager.maxStamina;
        
        // Luôn đảm bảo lúc vào game thì túi đồ đóng
        isInventoryOpen = false;
        ToggleInventory(false);
    }

    void Update()
    {
        UpdatePlayerStatsUI();

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            isInventoryOpen = !isInventoryOpen;
            ToggleInventory(isInventoryOpen);
        }
    }

    void UpdatePlayerStatsUI()
    {
        hpSlider.value = manager.currentHealth;
        staminaSlider.value = manager.currentStamina;
        hpText.text = $"Hp: {Mathf.RoundToInt(manager.currentHealth)} / {manager.maxHealth}";
        staminaText.text = $"Stamina: {Mathf.RoundToInt(manager.currentStamina)} / {manager.maxStamina}";
        MovespeedText.text = $"Move: {manager.MoveSpeed:F1}";
        SprintSpeedText.text = $"Sprint: {manager.SprintSpeed:F1}";
        ArmorText.text = $"Armor: {manager.armor:F0}";
    }

    private void ToggleInventory(bool isOpen)
    {
        if (isOpen)
        {
            mainUiRect.anchoredPosition = new Vector2(0, 1200); 
            inventoryUiRect.anchoredPosition = new Vector2(0, 0);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            mainUiRect.anchoredPosition = new Vector2(0, 0);
            inventoryUiRect.anchoredPosition = new Vector2(0, 1200); 
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public void SetItemDescription(string description)
    {
        if (itemDescriptionText != null) itemDescriptionText.text = description;
    }
}