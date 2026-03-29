using UnityEngine;
using Fusion;
using UnityEngine.UI;
using System.Collections;

public class HealthHandler : NetworkBehaviour
{
    [Header("Chỉ số")]
    // [Networked]: Biến này sẽ được Photon tự động đồng bộ giá trị đến TẤT CẢ các máy trong phòng.
    // Khi máy chủ thay đổi máu, máy khách sẽ nhận được giá trị đó ngay lập tức.
    [Networked] public float NetworkHealth { get; set; } = 100f;
    
    public float maxHealth = 100f;

    [Header("UI Máu Màn Hình")]
    public Image bloodImage; 
    public float fadeSpeed = 1f; 

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            GameObject uiObj = GameObject.FindWithTag("BloodUI"); 
            if (uiObj != null)
            {
                bloodImage = uiObj.GetComponent<Image>();
                
                Color c = bloodImage.color;
                c.a = 0;
                bloodImage.color = c;
            }
        }
        if (Object.HasStateAuthority) NetworkHealth = maxHealth;
    }

    // RpcSources.All: Bất kỳ ai cũng có thể gọi hàm này (ví dụ: người bắn).
    // RpcTargets.StateAuthority: Lệnh trừ máu này CHỈ thực thi trên máy đang quản lý nhân vật bị trúng đạn.
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_TakeDamage(float damage)
    {
        // Vì đang chạy trên máy có StateAuthority, việc thay đổi biến [Networked] sẽ được đồng bộ ra toàn mạng.
        NetworkHealth -= damage;
        if (NetworkHealth < 0) NetworkHealth = 0;

        // Sau khi trừ máu, gọi tiếp một RPC để báo cho chính người bị bắn hiện hiệu ứng đỏ màn hình.
        // Object.InputAuthority: Mục tiêu là máy của người chơi đang điều khiển nhân vật này.
        RPC_ShowBloodEffect(Object.InputAuthority);
    }

    // RPC_ShowBloodEffect: Hàm hiển thị hiệu ứng vết máu.
    // RpcTargets.InputAuthority: Đảm bảo CHỈ màn hình của nạn nhân mới bị đỏ, các máy khác không thấy.
    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    private void RPC_ShowBloodEffect(PlayerRef player)
    {
        if (bloodImage != null)
        {
            StopAllCoroutines();
            StartCoroutine(FadeBloodScreen());
        }
    }

    private IEnumerator FadeBloodScreen()
    {
        Color tempColor = bloodImage.color;
        tempColor.a = 1f; 
        bloodImage.color = tempColor;

        while (bloodImage.color.a > 0)
        {
            
            tempColor.a -= Time.deltaTime * fadeSpeed;
            bloodImage.color = tempColor;
            yield return null; 
        }

        tempColor.a = 0;
        bloodImage.color = tempColor;
    }
}