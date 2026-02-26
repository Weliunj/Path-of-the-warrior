using UnityEngine;

public class BoxBehavior : MonoBehaviour
{
    [SerializeField] private float speedMove = 3f;
    private Vector3 startPos;

    void Awake()
    {
        // Lưu vị trí ban đầu để đưa về khi tái sử dụng
        startPos = transform.position; 
    }

    void OnEnable()
    {
        // Đưa đối tượng về vị trí mặc định ngay khi được bật lại
        transform.position = startPos;
        // Tự động tắt sau 3 giây để trả về Pool
        Invoke("DisableObject", 3f);
    }

    void Update()
    {
        // Di chuyển liên tục theo hướng tùy ý (ví dụ: lên trên)
        transform.Translate(Vector3.up * speedMove * Time.deltaTime);
    }

    void DisableObject()
    {
        gameObject.SetActive(false);
    }
}