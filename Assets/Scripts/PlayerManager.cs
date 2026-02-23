using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayerSettings", menuName = "StarterAssets/PlayerSettings")]
public class PlayerManager : ScriptableObject
{
    [Header("MoveSettings")]
    [Tooltip("Tốc độ di chuyển đi bộ bình thường (m/s)")]
    public float MoveSpeed = 4.0f;

    [Tooltip("Tốc độ khi chạy nhanh (m/s)")]
    public float SprintSpeed = 6.0f;

    [Tooltip("Tốc độ xoay camera của nhân vật")]
    public float RotationSpeed = 1.0f;

    [Tooltip("Tỷ lệ tăng tốc và giảm tốc (số càng cao nhân vật dừng/đi càng nhanh)")]
    public float SpeedChangeRate = 10.0f;


    [Header("JumpAndGravity")]
    [Tooltip("Độ cao tối đa khi nhân vật nhảy")]
    public float JumpHeight = 1.2f;

    [Tooltip("Giá trị trọng lực riêng của nhân vật (Mặc định của Unity là -9.81)")]
    public float Gravity = -15.0f;

    [Tooltip("Thời gian chờ tối thiểu giữa 2 lần nhảy")]
    public float JumpTimeout = 0.1f;

    [Tooltip("Thời gian chờ trước khi chuyển sang trạng thái rơi (Hữu ích khi đi xuống cầu thang)")]
    public float FallTimeout = 0.15f;
    
    [Header("Attack")]
    public float baseAtk = 10f;
    [Tooltip("Multiplier applied to baseAtk for the 2nd combo hit (default 1.2 = +20%)")]
    public float atk2Multiplier = 1.2f;
    [Tooltip("Multiplier applied to atk2 for the 3rd combo hit (default 1.2 = +20%)")]
    public float atk3Multiplier = 1.2f;

    public float Atk1 { get { return baseAtk; } }
    public float Atk2 { get { return baseAtk * atk2Multiplier; } }
    public float Atk3 { get { return Atk2 * atk3Multiplier; } }

    [Header("Vitals")]
    [Tooltip("Mức HP tối đa của người chơi")]
    public float maxHealth = 200f;
    public float currentHealth;
    [Tooltip("Mức Stamina tối đa của người chơi")]
    public float maxStamina = 100f;
    public float currentStamina;

    [Header("Defensive Attributes")]
    [Tooltip("Giảm thêm khi đang block (0..1)")]
    [Range(0f,1f)]
    public float blockReductionPercent = 0.5f;
}