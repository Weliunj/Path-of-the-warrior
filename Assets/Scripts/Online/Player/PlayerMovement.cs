using Fusion;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    private CharacterController characterController;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpHeight = 2f;
    public float gravityValue = -9.81f;

    private Vector3 playerVelocity;
    private bool isGrounded;

    public override void Spawned()
    {
        characterController = GetComponent<CharacterController>();
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasInputAuthority) return;

        isGrounded = characterController.isGrounded;
        if (isGrounded && playerVelocity.y < 0) playerVelocity.y = 0f;

        // Di chuyển dựa trên hướng Forward của nhân vật (đã được xoay bởi PlayerCamera)
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 move = (transform.forward * v + transform.right * h).normalized;
        
        characterController.Move(move * moveSpeed * Runner.DeltaTime);

        if (Input.GetButton("Jump") && isGrounded)
        {
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -2f * gravityValue);
        }

        playerVelocity.y += gravityValue * Runner.DeltaTime;
        characterController.Move(playerVelocity * Runner.DeltaTime);
    }
}