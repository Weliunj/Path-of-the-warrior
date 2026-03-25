using Fusion;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour 
{
    private CharacterController _controller;
    
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    public float gravity = -9.81f;

    private Vector3 _velocity; 

    private void Awake() {
        _controller = GetComponent<CharacterController>();
    }

    public override void FixedUpdateNetwork() 
    {
        if (Object.HasInputAuthority)
        {
            // 1. Di chuyển cơ bản [cite: 54, 67]
            float moveX = Input.GetAxis("Horizontal");
            float moveZ = Input.GetAxis("Vertical");
            Vector3 move = transform.right * moveX + transform.forward * moveZ;
            _controller.Move(move * moveSpeed * Runner.DeltaTime);

            // 2. Kiểm tra chạm đất (Grounded) [cite: 91]
            if (_controller.isGrounded && _velocity.y < 0) {
                _velocity.y = -2f; 
            }

            // 3. Bắt sự kiện nút Nhảy (Space) [cite: 92, 93]
            if (Input.GetButtonDown("Jump") && _controller.isGrounded) {
                _velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            }

            // Áp dụng trọng lực
            _velocity.y += gravity * Runner.DeltaTime;
            _controller.Move(_velocity * Runner.DeltaTime);
        }
    }
}