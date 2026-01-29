using UnityEngine;
using System.Collections; // Cần thiết để dùng IEnumerator
using UnityEngine.InputSystem;
using UnityEditor; // Dùng cho hệ thống Input mới
public class Spawm : MonoBehaviour
{
    public enum SpawmMode{ _default, _random}
    public SpawmMode currState;
    private Vector3 spawnPosition;
    private bool isSpawning = false; // Biến kiểm soát để tránh spam Coroutine

    [Header("Random mode")]
    public GameObject CenterR;
    public float range = 5f; // Nên để biến này ra ngoài để dễ chỉnh trong Inspector

    [Header("Other")]
    public GameObject cube;
    public float delaySpawn;
    public int quanlity;
    
    
    void Start()
    {
        StartCoroutine(SpawmObj());
    }

    // Update is called once per frame
    void Update()
    {
        // Kiểm tra phím S và chỉ cho phép chạy nếu chưa đang trong quá trình spawn
        if (Keyboard.current != null && Keyboard.current.sKey.wasPressedThisFrame)
        {
            if (!isSpawning)
            {
                StartCoroutine(SpawmObj());
            }
        }
    }

    public void SpawmHandle()
    {
        switch (currState)
        {
            case SpawmMode._default:
                spawnPosition = CenterR.transform.position;
                break;
            case SpawmMode._random:
                // Vật thể bay lơ lửng ngẫu nhiên quanh tâm
                spawnPosition = CenterR.transform.position + Random.insideUnitSphere * range;
                break;
        }
    }

    public IEnumerator SpawmObj()
    {
        isSpawning = true;
        int currQuanlity = 0;
        
        // Tối ưu: Cache WaitForSeconds
        WaitForSeconds wait = new WaitForSeconds(delaySpawn);

        while(currQuanlity != quanlity)
        {
            yield return wait;
            SpawmHandle();
            Instantiate(cube, spawnPosition, Quaternion.identity);
            currQuanlity++;
        }
        isSpawning = false; // Kết thúc quá trình spawn
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(CenterR.transform.position, range);
    }
}
