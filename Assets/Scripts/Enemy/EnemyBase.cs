using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyBase : MonoBehaviour
{
    // ==================== HỆ THỐNG CHẾ ĐỘ & TRẠNG THÁI ====================
    public enum Mode { Idle_Chase_Atk, Wander_Chase_Atk, Patrol_Chase_Atk }
    [Header("Main Settings")]
    public Mode selectedMode; 

    [HideInInspector]
    public enum State { Idle, Wander, Patrol, Chase, Atk }
    public State currentState; 

    // ==================== THÀNH PHẦN CƠ BẢN ====================
    protected NavMeshAgent agent;   
    protected GameObject player;    
    protected Animator animator;    

    [Header("Movement Stats")]
    public float Movespeed = 3f;      
    public float Sprintspeed = 6f;    
    public float attackDistance = 2f; 

    [Header("Idle Position")]
    public GameObject IdlePos;       
    private float IdleRadius = 1.5f; 

    [Header("Wander Settings")]
    public GameObject WanderPos;     
    public float wanderRadius = 10f; 
    public float wanderWaitTime = 3f;
    private float wanderTimer;       
    private bool isWaiting;          

    [Header("Patrol Settings")]
    public List<Transform> patrolWaypoints; 
    public float waypointWaitTime = 2f;      
    private int currentWaypointIndex;       
    private float patrolTimer;               

    // ==================== CÀI ĐẶT TẦM NHÌN (VISION CONE) ====================
    [Header("Vision Cone Settings")]
    public Material visionMaterial;   
    public float visionRange = 40f;   
    public float visionAngle = 90f;   
    public float visionHeight = 2f;   // Độ dày/chiều cao của khối nón
    [Tooltip("Độ cao của nón so với mặt đất (Ví dụ: 1.5 là ngang mắt quái)")]
    public float visionHeightOffset = 0.7f; 
    public int resolution = 70;       
    public float updateRate = 0.1f;  
    public LayerMask playerLayer;     
    public LayerMask obstructLayer;   

    private Mesh visionMesh;          
    private MeshFilter meshFilter;    
    private GameObject visionObj;     // Lưu tham chiếu để cập nhật vị trí
    private float visionTimer;        
    private bool canSeePlayer;        

    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        // Tạo đối tượng con để vẽ nón
        visionObj = new GameObject("VisionCone");
        visionObj.transform.SetParent(transform); 
        // Đặt nón tại độ cao offset đã thiết lập
        visionObj.transform.localPosition = new Vector3(0, visionHeightOffset, 0);
        visionObj.transform.localRotation = Quaternion.identity;

        meshFilter = visionObj.AddComponent<MeshFilter>();
        MeshRenderer mr = visionObj.AddComponent<MeshRenderer>();
        visionMesh = new Mesh { name = "VisionConeMesh" };
        meshFilter.mesh = visionMesh;

        if (visionMaterial != null) mr.material = visionMaterial;
    }

    protected virtual void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        wanderTimer = wanderWaitTime; 
        currentState = State.Idle;
    }

    protected virtual void Update()
    {
        if (agent == null || player == null) return;

        // Luôn cập nhật vị trí nón theo Offset (phòng trường hợp bạn chỉnh trong lúc Game đang chạy)
        visionObj.transform.localPosition = new Vector3(0, visionHeightOffset, 0);

        visionTimer += Time.deltaTime;
        if (visionTimer >= updateRate)
        {
            canSeePlayer = DrawVisionCone3D(); 
            visionTimer = 0;
        }

        HandleState();      
        ExecuteLogic();     
        UpdateAnimation();  
    }

    void HandleState()
    {
        float horizontalDist = Vector2.Distance(
            new Vector2(transform.position.x, transform.position.z), 
            new Vector2(player.transform.position.x, player.transform.position.z)
        );

        if (canSeePlayer)
        {
            currentState = (horizontalDist <= attackDistance) ? State.Atk : State.Chase;
        }
        else 
        {
            if (currentState == State.Chase || currentState == State.Atk)
            {
                currentState = selectedMode switch
                {
                    Mode.Idle_Chase_Atk => State.Idle,
                    Mode.Wander_Chase_Atk => State.Wander,
                    _ => State.Patrol 
                };
            }
        }
    }

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

    // --- CÁC HÀM DI CHUYỂN (GIỮ NGUYÊN) ---
    void IdleLogic()
    {
        if (IdlePos == null) return;
        agent.speed = Movespeed;
        if (Vector3.Distance(transform.position, IdlePos.transform.position) <= IdleRadius)
            agent.isStopped = true;
        else { agent.isStopped = false; agent.SetDestination(IdlePos.transform.position); }
    }

    void WanderLogic()
    {
        if (WanderPos == null) return;
        agent.speed = Movespeed;
        if (isWaiting)
        {
            wanderTimer += Time.deltaTime;
            if (wanderTimer >= wanderWaitTime) { wanderTimer = 0; isWaiting = false; agent.isStopped = false; agent.SetDestination(RandomNavMeshLocation(wanderRadius)); }
        }
        else if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance) { isWaiting = true; agent.isStopped = true; }
    }

    void PatrolLogic()
    {
        if (patrolWaypoints == null || patrolWaypoints.Count == 0) return;
        agent.speed = Movespeed;
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f)
        {
            agent.isStopped = true;
            patrolTimer += Time.deltaTime;
            if (patrolTimer >= waypointWaitTime) { patrolTimer = 0f; currentWaypointIndex = (currentWaypointIndex + 1) % patrolWaypoints.Count; agent.isStopped = false; agent.SetDestination(patrolWaypoints[currentWaypointIndex].position); }
        }
        else if (!agent.hasPath) { agent.isStopped = false; agent.SetDestination(patrolWaypoints[currentWaypointIndex].position); }
    }

    void ChaseLogic()
    {
        agent.isStopped = false;
        agent.speed = Sprintspeed;
        agent.SetDestination(player.transform.position); 
    }

    public virtual void LookAtPlayer()
    {
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        Vector3 dir = (player.transform.position - transform.position);
        dir.y = 0; 
        if (dir != Vector3.zero) transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 6f);
    }

    // ==================== HỆ THỐNG VẼ NÓN TẦM NHÌN 3D (CẬP NHẬT OFFSET) ====================
    
    bool DrawVisionCone3D()
    {
        bool spotted = false;
        int numVertices = resolution * 2 + 2; 
        Vector3[] vertices = new Vector3[numVertices];
        int numTriangles = (resolution - 1) * 2 * 3 * 2;
        int[] triangles = new int[numTriangles];

        vertices[0] = Vector3.zero;
        vertices[1] = new Vector3(0, visionHeight, 0);

        float currentAngle = -visionAngle / 2;
        float angleStep = visionAngle / (resolution - 1);

        for (int i = 0; i < resolution; i++)
        {
            float rad = currentAngle * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Sin(rad), 0, Mathf.Cos(rad));
            Vector3 worldDir = transform.TransformDirection(dir);

            float distance = visionRange;
            
            // BẮN TIA TỪ ĐỘ CAO OFFSET
            Vector3 rayStart = transform.position + Vector3.up * visionHeightOffset;

            if (Physics.Raycast(rayStart, worldDir, out RaycastHit hit, visionRange, obstructLayer | playerLayer))
            {
                distance = hit.distance;
                if (((1 << hit.collider.gameObject.layer) & playerLayer) != 0) spotted = true;
            }

            vertices[i + 2] = dir * distance;
            vertices[i + resolution + 2] = new Vector3(dir.x, 0, dir.z) * distance + Vector3.up * visionHeight;
            currentAngle += angleStep;
        }

        int triIndex = 0;
        for (int i = 0; i < resolution - 1; i++)
        {
            triangles[triIndex++] = i + 2; triangles[triIndex++] = i + 3; triangles[triIndex++] = i + resolution + 2;
            triangles[triIndex++] = i + 3; triangles[triIndex++] = i + resolution + 3; triangles[triIndex++] = i + resolution + 2;
            triangles[triIndex++] = 0; triangles[triIndex++] = i + 3; triangles[triIndex++] = i + 2;
            triangles[triIndex++] = 1; triangles[triIndex++] = i + resolution + 2; triangles[triIndex++] = i + resolution + 3;
        }

        visionMesh.Clear();
        visionMesh.vertices = vertices;
        visionMesh.triangles = triangles;
        visionMesh.RecalculateNormals();
        return spotted;
    }

    Vector3 RandomNavMeshLocation(float radius)
    {
        Vector3 origin = WanderPos ? WanderPos.transform.position : transform.position;
        Vector3 randomDir = Random.insideUnitSphere * radius + origin;
        NavMeshHit hit;
        return NavMesh.SamplePosition(randomDir, out hit, radius, NavMesh.AllAreas) ? hit.position : origin;
    }

    void UpdateAnimation()
    {
        if (animator == null) return;
        float target = (currentState == State.Chase) ? 1f : (agent.isStopped ? 0f : 0.5f);
        animator.SetFloat("MotionSpeed", Mathf.Lerp(animator.GetFloat("MotionSpeed"), target, Time.deltaTime * 10f));
    }

    private void OnDrawGizmos()
    {
        // Vẽ tia xem trước điểm bắn Raycast trong Scene
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * visionHeightOffset, 0.2f);
        
        if (WanderPos != null) { Gizmos.color = Color.green; Gizmos.DrawWireSphere(WanderPos.transform.position, wanderRadius); }
        Gizmos.color = Color.red; Gizmos.DrawWireSphere(transform.position, attackDistance);
    }
}