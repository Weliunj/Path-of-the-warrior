using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public GameObject boxPrefab;
    public int poolSize = 100; // Yêu cầu tối thiểu 100 đối tượng
    private List<GameObject> boxPool;

    void Start()
    {
        boxPool = new List<GameObject>();

        // Khởi tạo sẵn 100 đối tượng ở trạng thái ẩn
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(boxPrefab);
            obj.SetActive(false);
            boxPool.Add(obj);
        }

        // Gọi hàm lấy đối tượng từ Pool liên tục mỗi 0.1 giây
        InvokeRepeating("SpawnBox", 0f, 1.5f);
    }

    void SpawnBox()
    {
        foreach (GameObject box in boxPool)
        {
            // Tìm đối tượng đang ẩn (đang rảnh) để sử dụng
            if (!box.activeInHierarchy)
            {
                box.SetActive(true);
                return;
            }
        }
    }
}