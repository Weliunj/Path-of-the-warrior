using System.Collections;
using UnityEngine;

public class InterpolationDemo : MonoBehaviour
{
    // 1. Khai báo cái "khuôn" danh sách (thường viết hoa chữ cái đầu cho đúng chuẩn)
    public enum InterpolationType { Lerp, SmoothDamp, Slerp, MoveTowards } 

    [Header("Settings")]
    public InterpolationType selectedMethod;
    public float speed = 5f;
    public float smoothTime = 0.3f;
    private Vector3 currentVelocity = Vector3.zero; // Bắt buộc phải có cho SmoothDamp

    [Header("Slerp Settings")]
    public float arcHeight = 5f; // Độ cao/to của vòng cung
    private float slerpTime = 0f;
    
    [Header("Points")]
    public GameObject pointA;
    public GameObject pointB;
    private Vector3 targetPos;
    public float threshold = 0.1f;
    private bool iswaiting = false;
    
    void Start()
    {
        transform.position = pointA.transform.position;
        // Bắt đầu bằng việc đi tới điểm B
        if (pointB != null) targetPos = pointB.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (pointA == null || pointB == null) return;

        if (!iswaiting) 
        {
            MoveHandle();

            float distance = Vector3.Distance(transform.position, targetPos);
            if (distance < threshold)
            {
                StartCoroutine(WaitAndSwitchTarget());
            }
        }
    }
    public IEnumerator WaitAndSwitchTarget()
    {
        iswaiting = true;       //Da den target
        yield return new WaitForSeconds(1f);

        targetPos = (targetPos == pointB.transform.position) 
            ? pointA.transform.position : pointB.transform.position;
        iswaiting = false;
    }
    void MoveHandle()
    {
        switch(selectedMethod)
        {
            case InterpolationType.Lerp:
                transform.position = Vector3.Lerp(transform.position, targetPos, speed * Time.deltaTime);
                break;
            case InterpolationType.SmoothDamp:
                transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref currentVelocity, smoothTime);
                break;
            case InterpolationType.Slerp:
                // Tính toán tâm vòng cung (Pivot giả)
                Vector3 center = (pointA.transform.position + pointB.transform.position) * 0.5f;
                center -= new Vector3(0, arcHeight, 0); // Đẩy tâm xuống dưới

                // Vector từ tâm đến 2 điểm
                Vector3 startRel = pointA.transform.position - center;
                Vector3 endRel = pointB.transform.position - center;

                // Tính toán tỷ lệ thời gian di chuyển
                slerpTime += Time.deltaTime * (speed / 10f); 
                if (slerpTime > 1f) slerpTime = 0f;

                // Nội suy theo vòng cung mới
                Vector3 interpolatedRel = Vector3.Slerp(startRel, endRel, slerpTime);
                transform.position = interpolatedRel + center;
                break;
            case InterpolationType.MoveTowards:
                transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
                break;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(pointA.transform.position, pointB.transform.position);
    }
}
