using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyFSM : MonoBehaviour
{
    public enum EnemyState { Moving, Ability, Finished }
    
    [Header("Settings")]
    public GameObject end;
    public EnemyState currentState = EnemyState.Moving;

    [Header("Ability Settings")]
    public float boostDuration = 2f;
    public float jumpForce = 5f;
    
    private NavMeshAgent _agent;
    private float _initialDistance;
    private float _originalSpeed;
    private bool _abilityUsed = false;

    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _originalSpeed = _agent.speed;

        if (end != null)
        {
            _agent.SetDestination(end.transform.position);
            // Tính toán tổng quãng đường ban đầu
            _initialDistance = Vector3.Distance(transform.position, end.transform.position);
        }
    }

    void Update()
    {
        if (end == null || _agent == null) return;

        switch (currentState)
        {
            case EnemyState.Moving:
                HandleMovingState();
                break;
            case EnemyState.Ability:
                // Trạng thái này đợi Coroutine xử lý xong sẽ tự chuyển về Moving
                break;
            case EnemyState.Finished:
                _agent.isStopped = true;
                break;
        }
    }

    private void HandleMovingState()
    {
        float currentDistance = Vector3.Distance(transform.position, end.transform.position);

        // Kiểm tra nếu đã đi được 1/3 quãng đường (quãng đường còn lại <= 2/3 ban đầu)
        if (!_abilityUsed && currentDistance <= (_initialDistance * 2f / 3f))
        {
            StartCoroutine(PerformRandomAbility());
        }

        // Kiểm tra nếu đã đến đích
        if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance)
        {
            currentState = EnemyState.Finished;
        }
    }

    private IEnumerator PerformRandomAbility()
    {
        _abilityUsed = true;
        currentState = EnemyState.Ability;

        // Random giữa 0 (Nhảy) và 1 (Tăng tốc)
        int choice = Random.Range(0, 2);

        if (choice == 0)
        {
            Debug.Log("Quái thực hiện: NHẢY!");
            PerformJump();
            // Đợi một chút để cú nhảy hoàn tất trước khi tiếp tục di chuyển
            yield return new WaitForSeconds(0.5f);
        }
        else
        {
            Debug.Log("Quái thực hiện: TĂNG TỐC X2!");
            _agent.speed = _originalSpeed * 2f;
            yield return new WaitForSeconds(boostDuration);
            _agent.speed = _originalSpeed;
        }

        currentState = EnemyState.Moving;
    }

    private void PerformJump()
    {
        // Nhảy trong NavMesh thường dùng vận tốc dọc hoặc biến OffMeshLink
        // Ở đây sử dụng cách đơn giản nhất là đẩy Transform lên
        // Lưu ý: Object cần có Rigidbody (IsKinematic) hoặc xử lý qua code di chuyển dọc
        StartCoroutine(JumpVisual());
    }

    private IEnumerator JumpVisual()
    {
        float timer = 0;
        float duration = 0.5f;
        Vector3 startPos = transform.position;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            // Di chuyển lên theo hình vòng cung đơn giản (Parabola)
            float height = Mathf.Sin((timer / duration) * Mathf.PI) * jumpForce;
            _agent.baseOffset = height; // Thay đổi baseOffset để Agent "bay" lên khỏi mặt đất
            yield return null;
        }
        _agent.baseOffset = 1;
    }
}