using UnityEngine;
using VolumetricLines;

public class Laser : MonoBehaviour
{
    private VolumetricLineBehavior volumetricLines;
    public float point;
    private float leftTime = 2f;
    void Start()
    {
        volumetricLines = GetComponent<VolumetricLineBehavior>();
        volumetricLines.StartPos = new Vector3(0, 0, 1);
    }

    void Update()
    {
        if (volumetricLines == null) return;
        
        // Phóng một tia Ray từ vị trí vật thể, theo hướng phía trước (Z)
        // Khoảng cách tối đa là biến 'a' của bạn
        RaycastHit hit;
        Vector3 endPoint;

        // transform.forward là hướng nhìn của xe tăng/nòng pháo
        if (Physics.Raycast(transform.position, transform.forward, out hit, point))
        {
            Debug.DrawLine(transform.position, hit.point, Color.red);
            // Nếu tia Ray chạm vào vật cản
            // hit.point là vị trí va chạm trong World Space
            // Chúng ta cần chuyển nó về Local Space để gán cho EndPos của Laser
            endPoint = transform.InverseTransformPoint(hit.point);
        }
        else
        {
            Debug.DrawLine(transform.position,transform.position + transform.forward * point, Color.green);
            endPoint = new Vector3(0, 0, point);
        }

        // Cập nhật điểm đầu và cuối cho Laser Asset
        volumetricLines.StartPos = Vector3.zero;
        volumetricLines.EndPos = endPoint;
    }
}
