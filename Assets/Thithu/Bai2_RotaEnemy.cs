using UnityEngine;

public class Bai2_RotaEnemy : MonoBehaviour
{
    public enum State { Rotation, Atk }
    public State mode;

    [Header("Settings")]
    public float speedRota = 100f;
    public float AtkRanger = 10f;
    public float Atkcd = 2f;
    public Transform FirePos;
    private float currentCd;

    [Header("References")]
    public PlayerCap player;
    public GameObject AmmoPref;

    void Start()
    {
        player = FindFirstObjectByType<PlayerCap>();
        currentCd = Atkcd;
    }

    void Update()
    {
        float dis = Vector3.Distance(player.transform.position,transform.position);
        if(dis <= AtkRanger)
        {
            mode = State.Atk;
        }
        else
        {
            mode = State.Rotation;
        }

        switch(mode)
        {
            case State.Rotation:
            Rotation();
            break;
            case State.Atk:
            Atk();
            break;

        }
    }
    
    void Rotation()
    {
        transform.Rotate(Vector3.up * speedRota * Time.deltaTime);
    }
    void Atk()
    {
        // 1. Tính toán hướng về phía Player
        Vector3 dir = player.transform.position - transform.position;
        dir.y = 0; // Giữ hướng trên mặt phẳng ngang
        
        // 2. Xoay thân Quái mượt mà
        Quaternion target = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Lerp(transform.rotation, target, 5f * Time.deltaTime);

        // 3. QUAN TRỌNG: Ép FirePos nhìn thẳng vào Player
        // Điều này giúp đạn sinh ra từ FirePos luôn bay đúng hướng
        FirePos.LookAt(player.transform.position);

        // 4. Logic bắn đạn với Cooldown
        if (currentCd > 0)
        {
            currentCd -= Time.deltaTime;
        }
        else
        {
            // Sử dụng FirePos.position và FirePos.rotation (đã LookAt ở trên)
            Instantiate(AmmoPref, FirePos.position, FirePos.rotation);
            currentCd = Atkcd;
        }
    }
}
