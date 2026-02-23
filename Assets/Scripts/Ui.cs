using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Ui : MonoBehaviour
{
    public static Ui Instance { get; private set; }

    [Header("UI Elements")]
    public TextMeshProUGUI itemDescriptionText;
    
    [Header("Vitals UI")]
    public Slider hpSlider;
    public Slider staminaSlider;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI staminaText;

    [Header("Managers & Panels")]
    public PlayerManager manager;
    public RectTransform mainUiRect;      
    public RectTransform inventoryUiRect; 

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
        
        // Khởi tạo trạng thái ban đầu: Đóng túi đồ, khóa chuột
        isInventoryOpen = false;
        ToggleInventory(false); 
    }

    public void Update()
    {
        hpSlider.value = manager.currentHealth;
        staminaSlider.value = manager.currentStamina;
        hpText.text = $"Hp: {manager.currentHealth} / {manager.maxHealth}";
        staminaText.text = $"Stamina: {manager.currentStamina} / {manager.maxStamina}";

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            isInventoryOpen = !isInventoryOpen;
            ToggleInventory(isInventoryOpen);
        }
    }

    private void ToggleInventory(bool isOpen)
    {
        if (isOpen)
        {
            // --- XỬ LÝ VỊ TRÍ UI ---
            mainUiRect.anchoredPosition = new Vector2(0, 1200); 
            inventoryUiRect.anchoredPosition = new Vector2(0, 0);
            
            // --- MỞ KHÓA CHUỘT ---
            Cursor.visible = true;                          // Hiện con trỏ chuột
            Cursor.lockState = CursorLockMode.None;         // Cho phép chuột di chuyển tự do khỏi tâm màn hình
            
            // Nếu bạn muốn dừng thời gian khi mở túi đồ:
            // Time.timeScale = 0; 
        }
        else
        {
            // --- XỬ LÝ VỊ TRÍ UI ---
            mainUiRect.anchoredPosition = new Vector2(0, 0);
            inventoryUiRect.anchoredPosition = new Vector2(0, 1200); 
            
            // --- KHÓA CHUỘT ---
            Cursor.visible = false;                         // Ẩn con trỏ chuột
            Cursor.lockState = CursorLockMode.Locked;       // Khóa chuột vào giữa màn hình (để xoay Camera)
            
            // Trả lại thời gian bình thường:
            // Time.timeScale = 1;
        }
    }

    public void SetItemDescription(string description)
    {
        if (itemDescriptionText != null) itemDescriptionText.text = description;
    }
}