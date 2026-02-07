using UnityEngine;

public class Lab6 : MonoBehaviour
{
    // ==================== TERRAIN TYPE ====================
    public enum TerrainType
    {
        Plain,
        Lake,
        Mountain
    }

    [Header("=== TERRAIN TYPE ===")]
    public TerrainType terrainType;

    // ==================== TERRAIN ====================
    public Terrain terrain;
    public float terrainHeight = 20f;

    // ==================== PERLIN ====================
    [Header("Perlin Noise")]
    public float perlinScale = 5f;
    public int octaves = 4;
    public float persistence = 0.5f;
    public float lacunarity = 2f;
    public Vector2 noiseOffset;

    // ==================== LAKE ====================
    [Header("Lake Settings")]
    [Range(0.1f, 0.5f)] public float lakeRadius = 0.35f;
    public float lakeDepthMultiplier = 0.35f; // Độ sâu/cao của hồ/đồi nước
    public bool invertLakeShape = false; // Nếu true: tạo đồi nước, false: tạo hồ
    public float waterLevelHeight = 0.18f; // Độ cao của mặt nước (tỷ lệ so với terrainHeight)

    // ==================== PREFABS ====================
    [Header("Prefabs")]
    public GameObject[] trees;     // cây lớn (có collider)
    public GameObject[] grass;     // cỏ, cây nhỏ (đi xuyên)
    public GameObject[] rocks;
    public GameObject waterPrefab;

    // ==================== SPAWN COUNT ====================
    [Header("Spawn Count")]
    public int treeCount = 80;
    public int grassCount = 250;
    public int rockCount = 50;

    private Transform _spawnedObjectsParent; // Đối tượng cha chứa tất cả các vật thể được spawn

    // ==================== UNITY ====================
    void Start()
    {
        if (!terrain)
            terrain = GetComponent<Terrain>();

        if (!terrain)
        {
            Debug.LogError("❌ Terrain not found!");
            return;
        }

        SetupSpawnParent(); // Đảm bảo có đối tượng cha để chứa các vật thể spawn
        GenerateTerrain();
        SpawnObjects();
        SpawnWater();
    }

    // ==================== TERRAIN GENERATION ====================
    void GenerateTerrain()
    {
        TerrainData data = terrain.terrainData;
        int res = data.heightmapResolution;

        data.size = new Vector3(data.size.x, terrainHeight, data.size.z);

        float[,] heights = new float[res, res];

        for (int y = 0; y < res; y++)
        {
            for (int x = 0; x < res; x++)
            {
                float perlin = FractalPerlin(x, y, res);
                float height = 0f;

                switch (terrainType)
                {
                    case TerrainType.Plain:
                        height = perlin * 0.15f;
                        break;

                    case TerrainType.Mountain:
                        height = perlin * 0.85f;
                        break;

                    case TerrainType.Lake:
                        float radial = RadialMask(x, y, res);
                        height = perlin * radial * lakeDepthMultiplier;
                        break;
                }

                heights[y, x] = height;
            }
        }

        data.SetHeights(0, 0, heights);
    }

    // ==================== PERLIN ====================
    float FractalPerlin(int x, int y, int res)
    {
        float amplitude = 1f;
        float frequency = 1f;
        float noise = 0f;

        for (int i = 0; i < octaves; i++)
        {
            float nx = (x / (float)res) * perlinScale * frequency + noiseOffset.x;
            float ny = (y / (float)res) * perlinScale * frequency + noiseOffset.y;

            float p = Mathf.PerlinNoise(nx, ny) * 2f - 1f;
            noise += p * amplitude;

            amplitude *= persistence;
            frequency *= lacunarity;
        }

        return Mathf.Clamp01((noise + 1f) * 0.5f);
    }

    // ==================== RADIAL MASK ====================
    // Tạo mask hình tròn, giá trị 0 ở tâm, 1 ở rìa (nếu không invert)
    float RadialMask(int x, int y, int res)
    {
        Vector2 center = new Vector2(res / 2f, res / 2f);
        float dist = Vector2.Distance(new Vector2(x, y), center);
        float maxDist = res * lakeRadius;

        float value = Mathf.Clamp01(dist / maxDist); // 0 ở tâm, 1 ở rìa
        // Nếu invertLakeShape là true, đảo ngược giá trị để tạo đồi nước (1 ở tâm, 0 ở rìa)
        return invertLakeShape ? (1f - value) : value;
    }

    // ==================== SPAWN OBJECT ====================
    void SpawnObjects()
    {
        ClearSpawned();

        switch (terrainType)
        {
            case TerrainType.Mountain:
                Spawn(rocks, rockCount, true);
                Spawn(trees, treeCount, true);
                break;

            case TerrainType.Plain:
                Spawn(grass, grassCount, false);
                break;

            case TerrainType.Lake:
                Spawn(trees, treeCount, true); // Thêm dòng này để spawn cây
                Spawn(grass, grassCount / 2, false);
                break;
        }
    }

    void Spawn(GameObject[] prefabs, int count, bool solid)
    {
        if (prefabs == null || prefabs.Length == 0) return;

        for (int i = 0; i < count; i++)
        {
            Vector3 pos = RandomTerrainPosition();
            GameObject obj = Instantiate(
                prefabs[Random.Range(0, prefabs.Length)],
                pos,
                Quaternion.Euler(0, Random.Range(0, 360f), 0), // Random rotation
                _spawnedObjectsParent // Đặt làm con của _spawnedObjectsParent
            );

            if (!solid)
            {
                Collider col = obj.GetComponent<Collider>();
                if (col) Destroy(col);
            }
        }
    }

    Vector3 RandomTerrainPosition()
    {
        TerrainData data = terrain.terrainData;
        float x = Random.Range(0f, data.size.x);
        float z = Random.Range(0f, data.size.z);

        // Nếu loại địa hình là hồ, đảm bảo vật thể không spawn trong vùng hồ
        if (terrainType == TerrainType.Lake)
        {
            // Lặp lại cho đến khi tìm được vị trí hợp lệ
            while (true)
            {
                // Tính toán khoảng cách từ vị trí ngẫu nhiên đến tâm của terrain
                // Tâm của terrain cũng là tâm của hồ
                Vector2 terrainCenter = new Vector2(data.size.x / 2f, data.size.z / 2f);
                Vector2 currentPos = new Vector2(x, z);
                float distFromCenter = Vector2.Distance(currentPos, terrainCenter);

                // Bán kính thực tế của hồ trong không gian thế giới
                // lakeRadius là giá trị chuẩn hóa (0.1f-0.5f) so với độ phân giải heightmap,
                // nên cần nhân với kích thước thực tế của terrain (data.size.x)
                float actualLakeRadiusWorld = data.size.x * lakeRadius;

                // Nếu vị trí nằm trong bán kính hồ, tạo lại vị trí khác
                if (distFromCenter < actualLakeRadiusWorld)
                {
                    x = Random.Range(0f, data.size.x);
                    z = Random.Range(0f, data.size.z);
                    continue; // Thử lại
                }
                break; // Vị trí hợp lệ, thoát vòng lặp
            }
        }

        float y = terrain.SampleHeight(new Vector3(x, 0, z));
        return terrain.transform.position + new Vector3(x, y, z);
    }

    // ==================== WATER ====================
    void SpawnWater()
    {
        if (terrainType != TerrainType.Lake || waterPrefab == null) return;

        GameObject water = Instantiate(waterPrefab, transform);
        TerrainData data = terrain.terrainData;

        float lakeSizeX = data.size.x * lakeRadius * 2f;
        float lakeSizeZ = data.size.z * lakeRadius * 2f;

        water.transform.position =
            terrain.transform.position +
            new Vector3(
                data.size.x / 2f,
                data.size.y * waterLevelHeight, // Sử dụng waterLevelHeight mới
                data.size.z / 2f
            );

        // Plane Unity mặc định 10x10
        water.transform.localScale =
            new Vector3(lakeSizeX / 10f, 1f, lakeSizeZ / 10f);
    }

    // ==================== CLEAN ====================
    // Thiết lập hoặc tìm đối tượng cha để chứa các vật thể được spawn
    void SetupSpawnParent()
    {
        if (_spawnedObjectsParent == null)
        {
            // Cố gắng tìm đối tượng cha đã tồn tại
            Transform existingParent = transform.Find("SpawnedObjects");
            if (existingParent != null)
            {
                _spawnedObjectsParent = existingParent;
            }
            else
            {
                // Nếu không tìm thấy, tạo mới
                GameObject parentGO = new GameObject("SpawnedObjects");
                parentGO.transform.SetParent(transform); // Đặt làm con của đối tượng Lab6
                _spawnedObjectsParent = parentGO.transform;
            }
        }
    }

    void ClearSpawned()
    {
        if (_spawnedObjectsParent == null) return;

        // Xóa tất cả các vật thể con của _spawnedObjectsParent
        for (int i = _spawnedObjectsParent.childCount - 1; i >= 0; i--)
        {
            // Sử dụng DestroyImmediate trong Editor để thấy thay đổi ngay lập tức
            // Sử dụng Destroy trong Play Mode
            if (Application.isPlaying) Destroy(_spawnedObjectsParent.GetChild(i).gameObject);
            else DestroyImmediate(_spawnedObjectsParent.GetChild(i).gameObject);
        }
    }

    // ==================== RESET TERRAIN ====================
    [ContextMenu("Reset Terrain")]
    public void ResetTerrain()
    {
        if (!terrain)
        {
            terrain = GetComponent<Terrain>();
            if (!terrain)
            {
                Debug.LogError("❌ Terrain not found!");
                return;
            }
        }

        TerrainData data = terrain.terrainData;
        int res = data.heightmapResolution;
        float[,] heights = new float[res, res]; // Mặc định là 0 (phẳng)
        data.SetHeights(0, 0, heights);

        SetupSpawnParent(); // Đảm bảo đối tượng cha tồn tại trước khi xóa
        ClearSpawned(); // Xóa tất cả các vật thể đã spawn (bao gồm cả nước)

        Debug.Log("Terrain Reset to flat state.");
    }

    // void OnValidate()
    // {
    //     if (!terrain || !terrain.terrainData) return;

    //     SetupSpawnParent(); // Đảm bảo có đối tượng cha khi OnValidate chạy
    //     ClearSpawned(); // Xóa các vật thể cũ trước khi sinh lại
    //     GenerateTerrain();
    //     SpawnObjects(); // Các vật thể được spawn lại khi thay đổi trong Editor
    //     SpawnWater();
    // }
}
