using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PlayerWeapon : MonoBehaviour
{
    public int maxHitsPerSwing = 4;
    Collider col;
    HashSet<Health> hitTargets = new HashSet<Health>();
    Player_Combat owner;
    int currentStage = 1;

    void Awake()
    {
        col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    public void EnableWeapon(int stage, Player_Combat ownerRef = null)
    {
        currentStage = stage <= 0 ? 1 : stage;
        owner = ownerRef == null ? owner : ownerRef;
        hitTargets.Clear();
        if (col != null) col.enabled = true;
    }

    // convenience called from Player_Combat
    public void EnableWeapon(int stage)
    {
        currentStage = stage <= 0 ? 1 : stage;
        hitTargets.Clear();
        if (col != null) col.enabled = true;
    }

    public void DisableWeapon()
    {
        if (col != null) col.enabled = false;
        hitTargets.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hitTargets.Count >= maxHitsPerSwing) return;
        // Require this weapon to be a Sword (by tag or name) to apply sword logic
        if (!(gameObject.CompareTag("Sword") || gameObject.name.ToLower().Contains("sword"))) return;

        // Require this weapon to be a Sword (by tag or name) to apply sword logic
        if (!(gameObject.CompareTag("Sword") || gameObject.name.ToLower().Contains("sword"))) return;

        Health h = other.GetComponentInParent<Health>();
        if (h == null) return;
        if (hitTargets.Contains(h)) return;

        // Determine damage and combo name
        float dmg = 10f;
        string comboName = "";
        if (owner != null)
        {
            dmg = owner.GetDamageForStage(currentStage);
            comboName = owner.GetCurrentComboName();
        }

        // If the hit object has an EnemyBase (or subclass like WarriorSkeleton), call its TakeDamage
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
        Debug.Log($"Sword hit {h.gameObject.name} | combo={comboName} | stage={currentStage} | dmg={dmg}");
    }
}
