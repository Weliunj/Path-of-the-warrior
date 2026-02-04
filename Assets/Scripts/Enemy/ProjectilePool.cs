using System.Collections.Generic;
using UnityEngine;

public class ProjectilePool : MonoBehaviour
{
    public static ProjectilePool Instance;

    [System.Serializable]
    public class Pool {
        public string tag;           // Tên định danh (VD: "Arrow", "Ammo")
        public ProjectileBase prefab; // Prefab có gắn script ProjectileBase
        public int size;             // Số lượng khởi tạo sẵn
    }

    public List<Pool> pools; // Danh sách các loại đạn muốn tạo Pool
    
    // Lưu trữ các kho đạn bằng Dictionary để truy xuất cực nhanh theo tên
    private Dictionary<string, Queue<ProjectileBase>> poolDictionary;

    void Awake() {
        Instance = this;
        poolDictionary = new Dictionary<string, Queue<ProjectileBase>>();

        foreach (Pool pool in pools) {
            Queue<ProjectileBase> projectileQueue = new Queue<ProjectileBase>();

            for (int i = 0; i < pool.size; i++) {
                ProjectileBase obj = Instantiate(pool.prefab, transform.parent);
                obj.gameObject.SetActive(false);
                projectileQueue.Enqueue(obj);
            }

            poolDictionary.Add(pool.tag, projectileQueue);
        }
    }

    // Hàm lấy đạn ra: Trả về kiểu ProjectileBase để dùng luôn
    public ProjectileBase Get(string tag, Vector3 position, Quaternion rotation) {
        if (!poolDictionary.ContainsKey(tag)) {
            Debug.LogWarning($"Pool với tag {tag} không tồn tại!");
            return null;
        }

        // Lấy đạn từ đầu hàng đợi
        ProjectileBase projectileToSpawn = poolDictionary[tag].Dequeue();

        // Đưa đạn về vị trí và góc xoay mới
        projectileToSpawn.transform.position = position;
        projectileToSpawn.transform.rotation = rotation;
        
        // Kích hoạt (OnEnable trong ProjectileBase sẽ tự chạy để đẩy lực bắn)
        projectileToSpawn.gameObject.SetActive(true);

        // Cho lại vào cuối hàng đợi để xoay vòng sử dụng
        poolDictionary[tag].Enqueue(projectileToSpawn);

        return projectileToSpawn;
    }
}