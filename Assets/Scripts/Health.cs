using UnityEngine;
using UnityEngine.UI; // Dùng nếu có thanh máu

public class Health : MonoBehaviour
{
    [Header("Thông số")]
    public float maxHealth = 100f;
    public float currentHealth;
    
    [Header("Giao diện (Tùy chọn)")]
    public Slider healthBar; 

    private void Start()
    {
        currentHealth = maxHealth;
        UpdateUI();
    }

    // Hàm nhận sát thương cơ bản
    public void ExecuteDamage(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        
        UpdateUI();
        Debug.Log($"{gameObject.name} còn {currentHealth} HP");

        if (currentHealth <= 0) Die();
    }

    void UpdateUI()
    {
        if (healthBar != null) healthBar.value = currentHealth / maxHealth;
    }

    void Die()
    {
        // Kích hoạt anim chết hoặc xóa object
        Debug.Log($"{gameObject.name} đã bị tiêu diệt!");
        Destroy(gameObject, 0.5f); 
    }
}