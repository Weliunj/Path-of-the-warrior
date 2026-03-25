using Fusion;
using UnityEngine;

public class MouseLook : NetworkBehaviour
{
    public float mouseSensitivity = 100f;
    private float xRotation = 0f;

    public override void Spawned() {
        if (Object.HasInputAuthority) {
            Cursor.lockState = CursorLockMode.Locked; // Khóa chuột vào tâm màn hình
        }
    }

    public override void Render() // Dùng Render để xoay chuột mượt mà hơn
    {
        if (Object.HasInputAuthority)
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            // Xoay nhân vật theo chiều ngang (trục Y)
            transform.Rotate(Vector3.up * mouseX);

            // Xoay hướng nhìn lên/xuống (trục X)
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f); // Giới hạn góc nhìn

            // Áp dụng xoay cho Camera (Bạn cần kéo Camera vào hoặc dùng Camera.main)
            Camera.main.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }
    }
}