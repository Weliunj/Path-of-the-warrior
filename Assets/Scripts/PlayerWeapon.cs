using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PlayerWeapon : MonoBehaviour
{
    public int maxHitsPerSwing = 4;
    private Collider col;
    private HashSet<Health> hitTargets = new HashSet<Health>();
    private Player_Combat owner;
    private int currentStage = 1;

    [Header("Audio Settings")]
    public AudioSource[] audioSources;
    private bool hasPlayedSound = false;

    void Awake()
    {
        col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    // Gộp 2 hàm EnableWeapon thành 1 bằng cách dùng tham số mặc định
    public void EnableWeapon(int stage, Player_Combat ownerRef = null)
    {
        currentStage = Mathf.Max(1, stage);
        if (ownerRef != null) owner = ownerRef;
        
        hitTargets.Clear();
        hasPlayedSound = false; // Reset để mỗi cú chém phát âm thanh 1 lần
        if (col != null) col.enabled = true;
    }

    public void DisableWeapon()
    {
        if (col != null) col.enabled = false;
        hitTargets.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {   
        // 1. Kiểm tra giới hạn mục tiêu
        if (hitTargets.Count >= maxHitsPerSwing) return;

        // 2. Kiểm tra đối tượng có máu không
        Health h = other.GetComponentInParent<Health>();
        if (h == null || hitTargets.Contains(h)) return;

        // 3. Logic Âm thanh (Phát ngẫu nhiên 1 lần duy nhất khi trúng mục tiêu đầu tiên)
        if (audioSources.Length > 0 && !hasPlayedSound)
        {
            int r = Random.Range(0, audioSources.Length);
            audioSources[r].Play();
            hasPlayedSound = true;
        }

        // 4. Tính toán sát thương từ Owner
        float dmg = 10f;
        if (owner != null)
        {
            dmg = owner.GetDamageForStage(currentStage);
        }

        // 5. Gây sát thương (Ưu tiên EnemyBase để xử lý giáp/điểm)
        EnemyBase enemy = other.GetComponentInParent<EnemyBase>();
        if (enemy != null)
        {
            enemy.TakeDamage(dmg);
        }
        else
        {
            h.ExecuteDamage(dmg);
        }

        hitTargets.Add(h);
    }

    void OnDisable() => DisableWeapon();

    void OnDestroy()
    {
        if (owner != null) owner.weaponScript = null;
    }
}