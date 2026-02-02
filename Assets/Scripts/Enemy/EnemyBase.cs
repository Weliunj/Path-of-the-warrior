using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

// Yêu cầu đối tượng phải có NavMeshAgent để điều khiển di chuyển
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyBase : MonoBehaviour
{
    // ==================== HỆ THỐNG CHẾ ĐỘ & TRẠNG THÁI ====================
    public enum Mode { Idle_Chase_Atk, Wander_Chase_Atk, Patrol_Chase_Atk }
    [Header("Main Settings")]
    public Mode selectedMode; // Chế độ AI mặc định chọn từ Inspector

    [HideInInspector]
    public enum State { Idle, Wander, Patrol, Chase, Atk }
    public State currentState; // Trạng thái hiện tại mà AI đang thực thi

    // ==================== THÀNH PHẦN CƠ BẢN ====================
    protected NavMeshAgent agent;   // "Bộ não" di chuyển, tìm đường né vật cản
    protected GameObject player;    // Tham chiếu tới đối tượng Người chơi
    protected Animator animator;    // Điều khiển các hoạt ảnh (Idle, Walk, Run)

    [Header("Movement Stats")]
    public float Movespeed = 3f;      // Tốc độ khi đi tuần/lang thang (Đi bộ)
    public float Sprintspeed = 6f;    // Tốc độ khi đuổi theo người chơi (Chạy)
    public float attackDistance = 2f; // Khoảng cách dừng lại để tấn công (Vùng đỏ)

    // ==================== CÀI ĐẶT CÁC VỊ TRÍ ====================
    [Header("Idle Position")]
    public GameObject IdlePos;       // Điểm đứng nghỉ cố định
    private float IdleRadius = 1.5f; // Sai số khoảng cách để xác nhận đã về tới đích nghỉ

    [Header("Wander Settings")]
    public GameObject WanderPos;     // Tâm của khu vực đi lang thang
    public float wanderRadius = 10f; // Bán kính vùng đi lang thang
    public float wanderWaitTime = 3f;// Thời gian quái đứng nghỉ trước khi tìm điểm đi tiếp
    private float wanderTimer;       // Bộ đếm thời gian nghỉ
    private bool isWaiting;          // Cờ kiểm tra: True là đang đứng nghỉ, False là đang đi

    [Header("Patrol Settings")]
    public List<Transform> patrolWaypoints; // Danh sách các điểm đi tuần ( Waypoints)
    public float waypointWaitTime = 2f;      // Thời gian nghỉ tại mỗi điểm tuần tra
    private int currentWaypointIndex;       // Chỉ số điểm đang đi trong danh sách (0, 1, 2...)
    private float patrolTimer;              // Bộ đếm thời gian nghỉ tuần tra

    // ==================== CÀI ĐẶT NÓN TẦM NHÌN 3D (VÙNG XANH) ====================
    [Header("Vision Cone (Green Zone)")]
    public Material visionMaterial;   // Vật liệu của nón tầm nhìn (nên dùng màu xanh trong suốt)
    public float visionRange = 40f;   // Tầm nhìn xa tối đa (Độ dài nón)
    public float visionAngle = 90f;   // Góc mở của nón tầm nhìn
    public float visionHeight = 10f;   // Chiều cao khối nón 3D
    public int resolution = 70;       // Số lượng tia quét (Càng cao nón càng mịn nhưng nặng máy)
    public float updateRate = 0.2f;  // Thời gian chờ cập nhật nón (Giảm lag cho CPU)
    public LayerMask playerLayer;     // Lớp vật lý của Người chơi (Layer Player)
    public LayerMask obstructLayer;   // Lớp vật lý của vật cản (Layer Wall/Obstacle)

    private Mesh visionMesh;          // Lưới Mesh để dựng hình nón
    private MeshFilter meshFilter;    // Thành phần chứa dữ liệu lưới Mesh
    private float visionTimer;        // Bộ đếm thời gian cập nhật tầm nhìn
    private bool canSeePlayer;        // Kết quả: True nếu thấy Player trong nón xanh

    // ==================== KHỞI TẠO (UNITY AWAKE & START) ====================
    protected virtual void Awake()
    {
        // Lấy thành phần NavMeshAgent và Animator gắn trên quái vật
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        // Tạo một GameObject con tên "VisionCone" để vẽ nón tầm nhìn tách biệt
        GameObject visionObj = new GameObject("VisionCone");
        visionObj.transform.SetParent(transform); // Gắn làm con của quái vật
        visionObj.transform.localPosition = Vector3.zero;
        visionObj.transform.localRotation = Quaternion.identity;

        // Thêm MeshFilter và MeshRenderer để hiển thị nón 3D
        meshFilter = visionObj.AddComponent<MeshFilter>();
        MeshRenderer mr = visionObj.AddComponent<MeshRenderer>();

        visionMesh = new Mesh { name = "VisionConeMesh" };
        meshFilter.mesh = visionMesh;

        // Gán vật liệu (màu sắc) cho nón nếu bạn đã kéo vào Inspector
        if (visionMaterial != null) mr.material = visionMaterial;
    }

    protected virtual void Start()
    {
        // Tìm đối tượng có Tag là "Player" để AI biết mục tiêu ở đâu
        player = GameObject.FindGameObjectWithTag("Player");
        wanderTimer = wanderWaitTime; // Khởi tạo thời gian nghỉ ban đầu cho Wander
        currentState = State.Atk;
    }

    // ==================== VÒNG LẶP CHÍNH (UPDATE) ====================
    protected virtual void Update()
    {
        // Nếu không có bộ não di chuyển hoặc không thấy người chơi trong Map thì thoát
        if (agent == null || player == null) return;

        // Cập nhật nón tầm nhìn 3D theo chu kỳ (Ví dụ: 20 lần/giây thay vì 60-120 lần/giây)
        visionTimer += Time.deltaTime;
        if (visionTimer >= updateRate)
        {
            canSeePlayer = DrawVisionCone3D(); // Vẽ nón và kiểm tra va chạm vật lý tia Raycast
            visionTimer = 0;
        }

        HandleState();      // Bước 1: Quyết định trạng thái (Đuổi, Đánh, hay Đi dạo)
        ExecuteLogic();     // Bước 2: Ra lệnh cho NavMeshAgent thực thi di chuyển
        UpdateAnimation();  // Bước 3: Đồng bộ hoạt ảnh theo vận tốc thực tế của quái
    }

    // ==================== QUẢN LÝ TRẠNG THÁI (STATE) ====================
    void HandleState()
    {
        // Tính khoảng cách đường chim bay giữa Quái và Người chơi
        float dist = Vector3.Distance(transform.position, player.transform.position);

        // NẾU PLAYER NẰM TRONG VÙNG XANH (Thấy qua nón tầm nhìn)
        if (canSeePlayer)
        {
            // Nếu khoảng cách <= attackDistance (Vào vùng đỏ) -> Chuyển sang Tấn công
            // Nếu > attackDistance nhưng vẫn trong nón xanh -> Chuyển sang Đuổi (Chase)
            currentState = (dist <= attackDistance) ? State.Atk : State.Chase;
        }
        else // NẾU KHÔNG THẤY PLAYER TRONG NÓN XANH
        {
            // Nếu AI đang Đuổi hoặc Đánh mà mất dấu, quay về chế độ tuần tra mặc định
            if (currentState == State.Chase || currentState == State.Atk)
            {
                currentState = selectedMode switch
                {
                    Mode.Idle_Chase_Atk => State.Idle,
                    Mode.Wander_Chase_Atk => State.Wander,
                    _ => State.Patrol // Các trường hợp khác (mặc định) là Patrol
                };
            }
        }
    }

    // Hàm gọi các logic thực thi tương ứng với trạng thái currentState
    void ExecuteLogic()
    {
        switch (currentState)
        {
            case State.Idle: IdleLogic(); break;
            case State.Wander: WanderLogic(); break;
            case State.Patrol: PatrolLogic(); break;
            case State.Chase: ChaseLogic(); break;
            case State.Atk: LookAtPlayer(); break;
        }
    }

    // ==================== CHI TIẾT LOGIC DI CHUYỂN ====================

    void IdleLogic()
    {
        if (IdlePos == null) return;
        agent.speed = Movespeed; // Tốc độ đi bộ

        // Nếu khoảng cách tới đích <= bán kính chấp nhận
        if (Vector3.Distance(transform.position, IdlePos.transform.position) <= IdleRadius)
            agent.isStopped = true; // Phanh Agent lại (Dừng lại)
        else
        {
            agent.isStopped = false; // Nhả phanh
            agent.SetDestination(IdlePos.transform.position); // Đi về điểm Idle
        }
    }

    void WanderLogic()
    {
        if (WanderPos == null) return;
        agent.speed = Movespeed;

        if (isWaiting) // Nếu đang đứng nghỉ
        {
            wanderTimer += Time.deltaTime; // Đếm thời gian
            if (wanderTimer >= wanderWaitTime) // Nếu nghỉ đủ lâu
            {
                wanderTimer = 0;
                isWaiting = false;
                agent.isStopped = false; // Nhả phanh
                // Tìm một vị trí ngẫu nhiên trên sàn và đi tới đó
                agent.SetDestination(RandomNavMeshLocation(wanderRadius));
            }
        }
        // Nếu AI đã đến gần đích của điểm lang thang hiện tại
        else if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            isWaiting = true; // Đánh dấu bắt đầu nghỉ
            agent.isStopped = true; // Phanh lại
        }
    }

    void PatrolLogic()
    {
        if (patrolWaypoints == null || patrolWaypoints.Count == 0) return;
        agent.speed = Movespeed;

        // Kiểm tra nếu đã đến gần điểm mốc tuần tra hiện tại
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f)
        {
            agent.isStopped = true; // Phanh lại nghỉ
            patrolTimer += Time.deltaTime; // Đếm thời gian nghỉ tại điểm mốc

            if (patrolTimer >= waypointWaitTime) // Hết thời gian chờ
            {
                patrolTimer = 0f;
                // Chuyển sang điểm tiếp theo (0 -> 1 -> 2 -> 0...)
                currentWaypointIndex = (currentWaypointIndex + 1) % patrolWaypoints.Count;
                agent.isStopped = false; // Nhả phanh đi tiếp
                agent.SetDestination(patrolWaypoints[currentWaypointIndex].position);
            }
        }
        else if (!agent.hasPath) // Nếu Agent bị mất đường đi, ra lệnh đi tới điểm mốc hiện tại
        {
            agent.isStopped = false;
            agent.SetDestination(patrolWaypoints[currentWaypointIndex].position);
        }
    }

    void ChaseLogic()
    {
        agent.isStopped = false; // LUÔN nhả phanh khi rượt đuổi
        agent.speed = Sprintspeed; // Chạy nhanh
        agent.SetDestination(player.transform.position); // Cập nhật đích đến là tọa độ Player
    }

    void LookAtPlayer()
    {
        agent.isStopped = true;    // PHANH lại ngay lập tức để thực hiện đòn đánh
        agent.velocity = Vector3.zero; // Xóa vận tốc thừa (không để quái bị trượt do quán tính)

        // Tính hướng từ Quái tới Người chơi
        Vector3 dir = (player.transform.position - transform.position);
        dir.y = 0; // Giữ hướng xoay thẳng, không để quái bị ngửa đầu lên trên
        if (dir != Vector3.zero)
        {
            // Xoay mặt mượt mà về phía người chơi bằng Slerp
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 6f);
        }
    }

    // ==================== HỆ THỐNG VẼ NÓN TẦM NHÌN 3D (RAYCAST) ====================
    
    bool DrawVisionCone3D()
    {
        bool spotted = false;
        // Chuẩn bị các đỉnh (vertices) và tam giác (triangles) để tạo hình khối nón 3D
        int numVertices = resolution * 2 + 2; 
        Vector3[] vertices = new Vector3[numVertices];
        int numTriangles = (resolution - 1) * 2 * 3 * 2;
        int[] triangles = new int[numTriangles];

        vertices[0] = Vector3.zero; // Tâm đáy nón
        vertices[1] = new Vector3(0, visionHeight, 0); // Tâm đỉnh nón

        float currentAngle = -visionAngle / 2;
        float angleStep = visionAngle / (resolution - 1);

        // Vòng lặp bắn tia Raycast để quét môi trường
        for (int i = 0; i < resolution; i++)
        {
            float rad = currentAngle * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Sin(rad), 0, Mathf.Cos(rad)); // Hướng tia quét cục bộ
            Vector3 worldDir = transform.TransformDirection(dir); // Chuyển sang hướng thế giới thực

            float distance = visionRange;
            // Bắn tia quét thực sự (Chặn bởi Tường hoặc Player)
            if (Physics.Raycast(transform.position + Vector3.up * 1f, worldDir, out RaycastHit hit, visionRange, obstructLayer | playerLayer))
            {
                distance = hit.distance; // Nón bị co ngắn lại nếu gặp tường hoặc người chơi
                // Nếu tia chạm trúng layer được gán cho Người chơi -> Nhìn thấy!
                if (((1 << hit.collider.gameObject.layer) & playerLayer) != 0) spotted = true;
            }

            // Gán tọa độ các đỉnh để tạo Mesh nón
            vertices[i + 2] = dir * distance;
            vertices[i + resolution + 2] = new Vector3(dir.x, 0, dir.z) * distance + Vector3.up * visionHeight;
            currentAngle += angleStep;
        }

        // Logic nối các điểm thành các mặt tam giác của hình nón
        int triIndex = 0;
        for (int i = 0; i < resolution - 1; i++)
        {
            // Mặt bên của nón
            triangles[triIndex++] = i + 2; triangles[triIndex++] = i + 3; triangles[triIndex++] = i + resolution + 2;
            triangles[triIndex++] = i + 3; triangles[triIndex++] = i + resolution + 3; triangles[triIndex++] = i + resolution + 2;
            // Mặt đáy & Mặt trên đỉnh
            triangles[triIndex++] = 0; triangles[triIndex++] = i + 3; triangles[triIndex++] = i + 2;
            triangles[triIndex++] = 1; triangles[triIndex++] = i + resolution + 2; triangles[triIndex++] = i + resolution + 3;
        }

        // Đẩy dữ liệu vào MeshFilter để Unity hiển thị cái nón xanh
        visionMesh.Clear();
        visionMesh.vertices = vertices;
        visionMesh.triangles = triangles;
        visionMesh.RecalculateNormals(); // Giúp nón nhận ánh sáng mượt mà hơn
        meshFilter.mesh = visionMesh;
        return spotted;
    }

    // ==================== CÔNG CỤ HỖ TRỢ DI CHUYỂN ====================
    Vector3 RandomNavMeshLocation(float radius)
    {
        // Lấy tâm khu vực: Ưu tiên WanderPos, nếu không có thì lấy vị trí quái hiện tại
        Vector3 origin = WanderPos ? WanderPos.transform.position : transform.position;
        // Lấy một điểm ngẫu nhiên bên trong hình cầu bán kính radius
        Vector3 randomDir = Random.insideUnitSphere * radius + origin;
        NavMeshHit hit;
        // Kiểm tra xem điểm ngẫu nhiên đó có nằm trên mặt sàn (NavMesh) hợp lệ hay không
        return NavMesh.SamplePosition(randomDir, out hit, radius, NavMesh.AllAreas) ? hit.position : origin;
    }

    // ==================== ĐỒNG BỘ HOẠT ẢNH (ANIMATION) ====================
    void UpdateAnimation()
    {
        if (animator == null) return;
        // MotionSpeed: 1.0 (Chạy), 0.5 (Đi bộ), 0 (Đứng im)
        // Nếu đang Chase thì chạy, nếu quái đang dừng (isStopped) thì Idle, còn lại là Walk
        float target = (currentState == State.Chase) ? 1f : (agent.isStopped ? 0f : 0.5f);
        // Sử dụng Mathf.Lerp để tham số MotionSpeed thay đổi từ từ, giúp chân quái chuyển động mượt
        animator.SetFloat("MotionSpeed", Mathf.Lerp(animator.GetFloat("MotionSpeed"), target, Time.deltaTime * 10f));
    }

    // ==================== VẼ GIZMOS (CỬA SỔ SCENE) ====================
    private void OnDrawGizmos()
    {
        // Vẽ vòng xanh hiển thị khu vực AI sẽ đi lang thang
        if (WanderPos != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(WanderPos.transform.position, wanderRadius);
        }
        // Vẽ vòng đỏ hiển thị tầm đánh cận chiến
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackDistance);
    }
}