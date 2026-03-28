using Fusion;
using Unity.Cinemachine;
using UnityEngine;

public class MouseLook : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private string cameraHolderName = "CameraHolder";
    private Transform _cameraHolderTransform;
    private CinemachineCamera _vcam;
    private CinemachineThirdPersonFollow _tpFollow;

    [Header("Look Settings")]
    public float mouseSensitivity = 100f;
    public float upperLookLimit = 80f;
    public float lowerLookLimit = -70f;

    [Header("Zoom Settings")]
    public float zoomSensitivity = 2f;
    public float minDistance = 2f;
    public float maxDistance = 10f;

    private float _verticalRotation = 0f;
    private float _currentDistance = 5f; // Khoảng cách camera mặc định

    public override void Spawned()
    {
        // Chỉ chạy trên máy của người chơi sở hữu nhân vật này
        if (!HasInputAuthority) return;

        _cameraHolderTransform = transform.Find(cameraHolderName);
        _vcam = FindFirstObjectByType<CinemachineCamera>();

        if (_vcam != null && _cameraHolderTransform != null)
        {
            _vcam.Target.TrackingTarget = _cameraHolderTransform;
            
            // Lấy component Third Person Follow để chỉnh Distance (Zoom)
            _tpFollow = _vcam.GetComponent<CinemachineThirdPersonFollow>();
            if (_tpFollow != null) _currentDistance = _tpFollow.CameraDistance;

            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasInputAuthority) return;

        // --- 1. XOAY CHUỘT ---
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Runner.DeltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Runner.DeltaTime;

        // Xoay thân người chơi theo trục Y (ngang)
        transform.Rotate(Vector3.up * mouseX);

        // Xoay CameraHolder theo trục X (dọc)
        _verticalRotation -= mouseY;
        _verticalRotation = Mathf.Clamp(_verticalRotation, lowerLookLimit, upperLookLimit);

        if (_cameraHolderTransform != null)
        {
            _cameraHolderTransform.localRotation = Quaternion.Euler(_verticalRotation, 0, 0);
        }

        // --- 2. ZOOM (CAMERA DISTANCE) ---
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0 && _tpFollow != null)
        {
            _currentDistance -= scroll * zoomSensitivity;
            _currentDistance = Mathf.Clamp(_currentDistance, minDistance, maxDistance);
            
            // Cập nhật khoảng cách trực tiếp vào Cinemachine
            _tpFollow.CameraDistance = _currentDistance;
        }
    }
}