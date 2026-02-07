using UnityEngine;

public enum ExerciseMode
{
    None,
    Exercise1_PerlinNoise,
    Exercise2_LinearGradient,
    Exercise1_And_Trees,
    Exercise2_And_Trees
}

public class Lab6_TerrainGenerate : MonoBehaviour
{
    [Header("=== AUTO RUN SETTINGS ===")]
    [Tooltip("Chọn bài tập nào chạy khi nhấn Play")]
    public ExerciseMode autoRunOnStart = ExerciseMode.None;
    
    [Tooltip("Tự động cập nhật khi thay đổi Inspector (có thể gây lag nếu bật)")]
    public bool autoUpdateOnInspectorChange = false;

    [Header("=== GENERAL SETTINGS ===")]
    public Terrain terrain;
    public float depth = 20f; // Độ cao tối đa của địa hình

    [Header("=== EXERCISE 1: Perlin Noise ===")]
    public float scale = 20f; // Độ mịn của mẫu nhiễu
    public float offsetX = 100f;
    public float offsetY = 100f;

    [Header("=== EXERCISE 2: Linear Gradient ===")]
    public float gradientStrength = 1.0f; // Độ dốc
    public float noiseScale = 20f;
    public float noiseStrength = 0.15f;

    [Header("=== EXERCISE 3: Random Tree ===")]
    public GameObject treePrefab;
    public int treeCount = 100;
    public float treeMinHeight = 0.1f; // Chỉ trồng cây ở độ cao nhất định
    public float treeMaxHeight = 0.8f;

    [Header("=== TERRAIN TEXTURES (Optional) ===")]
    [Tooltip("Texture cho vùng thấp (cỏ, bùn)")]
    public Texture2D lowTexture;
    [Tooltip("Texture cho vùng trung bình (đất, đá)")]
    public Texture2D midTexture;
    [Tooltip("Texture cho vùng cao (đá núi, tuyết)")]
    public Texture2D highTexture;
    [Tooltip("Độ cao chuyển từ low -> mid (0-1)")]
    [Range(0f, 1f)] public float midHeightThreshold = 0.3f;
    [Tooltip("Độ cao chuyển từ mid -> high (0-1)")]
    [Range(0f, 1f)] public float highHeightThreshold = 0.6f;

    private ExerciseMode lastAppliedMode = ExerciseMode.None;

    private void Start()
    {
        if (terrain == null)
        {
            terrain = GetComponent<Terrain>();
        }

        // Tự động chạy bài tập khi Start
        if (autoRunOnStart != ExerciseMode.None)
        {
            RunExercise(autoRunOnStart);
        }
    }

    // Tự động cập nhật khi thay đổi Inspector (chỉ trong Editor)
    private void OnValidate()
    {
        if (!autoUpdateOnInspectorChange) return;
        if (terrain == null) return;
        if (!Application.isPlaying) return; // Chỉ chạy trong Play Mode

        // Chạy lại bài tập đang được chọn
        if (autoRunOnStart != ExerciseMode.None)
        {
            RunExercise(autoRunOnStart);
        }
    }

    void RunExercise(ExerciseMode mode)
    {
        switch (mode)
        {
            case ExerciseMode.Exercise1_PerlinNoise:
                ApplyExercise1();
                break;
            case ExerciseMode.Exercise2_LinearGradient:
                ApplyExercise2();
                break;
            case ExerciseMode.Exercise1_And_Trees:
ApplyExercise1();
                ApplyExercise3();
                break;
            case ExerciseMode.Exercise2_And_Trees:
                ApplyExercise2();
                ApplyExercise3();
                break;
        }
        lastAppliedMode = mode;
    }

    // --- Bài tập 1: Thiết kế rừng cây với núi tuyết (Perlin Noise) ---
    [ContextMenu("Run Exercise 1 (Perlin Noise)")]
    public void ApplyExercise1()
    {
        if (terrain == null) return;

        TerrainData terrainData = terrain.terrainData;
        int width = terrainData.heightmapResolution;
        int height = terrainData.heightmapResolution;
        
        // Cập nhật kích thước Terrain theo depth
        terrainData.size = new Vector3(terrainData.size.x, depth, terrainData.size.z);

        float[,] heights = GenerateHeights(width, height);
        terrainData.SetHeights(0, 0, heights);
        
        // Áp dụng texture nếu có
        ApplyTerrainTextures();
        
        Debug.Log("Exercise 1 Complete: Perlin Noise Terrain Generated.");
    }

    float[,] GenerateHeights(int width, int height)
    {
        float[,] heights = new float[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Tính toán tọa độ để lấy mẫu Perlin Noise
                float xCoord = (float)x / width * scale + offsetX;
                float yCoord = (float)y / height * scale + offsetY;

                // Lấy giá trị Perlin Noise (trả về 0.0 đến 1.0)
                heights[x, y] = Mathf.PerlinNoise(xCoord, yCoord);
            }
        }
        return heights;
    }

    // --- Bài tập 2: Thiết kế rừng cây có độ dốc (Linear Gradient + Perlin Noise) ---
    [ContextMenu("Run Exercise 2 (Linear Gradient + Noise)")]
    public void ApplyExercise2()
    {
        if (terrain == null) return;

        TerrainData terrainData = terrain.terrainData;
        int res = terrainData.heightmapResolution;
        float[,] heights = new float[res, res];

        // Cập nhật kích thước Terrain
        terrainData.size = new Vector3(terrainData.size.x, depth, terrainData.size.z);

        for (int x = 0; x < res; x++)
        {
            for (int y = 0; y < res; y++)
            {
                // 1. Tạo độ dốc Linear Gradient (theo trục X)
                // Giá trị tăng dần từ 0 đến gradientStrength khi x tăng
                float gradient = (float)x / (res - 1) * gradientStrength;

                // 2. Thêm Perlin Noise để tạo độ lồi lõm tự nhiên
                float xCoord = (float)x / res * noiseScale + offsetX;
                float yCoord = (float)y / res * noiseScale + offsetY;
                float noise = Mathf.PerlinNoise(xCoord, yCoord) * noiseStrength;

                // 3. Tổng hợp lại và kẹp giá trị trong khoảng [0, 1]
                heights[x, y] = Mathf.Clamp01(gradient + noise);
}
        }

        terrainData.SetHeights(0, 0, heights);
        
        // Áp dụng texture nếu có
        ApplyTerrainTextures();
        
        Debug.Log("Exercise 2 Complete: Linear Gradient + Perlin Noise Applied.");
    }

    // --- Bài tập 3: Thêm cây cối ngẫu nhiên (Random Tree) ---
    [ContextMenu("Run Exercise 3 (Place Random Trees)")]
    public void ApplyExercise3()
    {
        if (terrain == null || treePrefab == null)
        {
            Debug.LogWarning("Terrain or Tree Prefab is missing!");
            return;
        }

        TerrainData terrainData = terrain.terrainData;
        Vector3 terrainSize = terrainData.size;
        int heightmapRes = terrainData.heightmapResolution;
        
        // ✅ XÓA CÂY CŨ để tránh tràn RAM
        CleanupTrees(); 

        // Parent object để chứa cây cho gọn Scene
        Transform treeParent = this.transform.Find("GeneratedTrees");
        if (treeParent == null)
        {
            treeParent = new GameObject("GeneratedTrees").transform;
            treeParent.SetParent(this.transform);
        }

        int treesPlaced = 0;
        for (int i = 0; i < treeCount; i++)
        {
            // 1. Chọn vị trí ngẫu nhiên trên bản đồ (Normalized 0-1)
            float normX = Random.Range(0f, 1f);
            float normZ = Random.Range(0f, 1f);

            // 2. Lấy độ cao CHÍNH XÁC từ heightmap (không dùng SampleHeight vì có thể sai)
            // Chuyển đổi từ normalized sang heightmap index
            int heightmapX = Mathf.FloorToInt(normX * (heightmapRes - 1));
            int heightmapZ = Mathf.FloorToInt(normZ * (heightmapRes - 1));
            
            // Lấy độ cao normalized từ heightmap (giá trị 0-1)
            float normalizedHeight = terrainData.GetHeight(heightmapX, heightmapZ) / terrainSize.y;

            // 3. Kiểm tra điều kiện độ cao (ví dụ: không trồng dưới nước hoặc trên đỉnh núi tuyết quá cao)
            if (normalizedHeight >= treeMinHeight && normalizedHeight <= treeMaxHeight)
            {
                // Chuyển sang world position
                float worldX = normX * terrainSize.x + terrain.transform.position.x;
                float worldZ = normZ * terrainSize.z + terrain.transform.position.z;
                float worldY = normalizedHeight * terrainSize.y + terrain.transform.position.y;
                
                Vector3 treePos = new Vector3(worldX, worldY, worldZ);
                
                // Instantiate cây
                GameObject tree = Instantiate(treePrefab, treePos, Quaternion.identity);
                tree.transform.SetParent(treeParent);
                
                // Random xoay cây & scale nhẹ cho tự nhiên
                tree.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                float randomScale = Random.Range(0.8f, 1.2f);
                tree.transform.localScale = Vector3.one * randomScale;
treesPlaced++;
            }
        }
        Debug.Log($"Exercise 3 Complete: Placed {treesPlaced}/{treeCount} trees (within height range {treeMinHeight}-{treeMaxHeight}).");
    }

    [ContextMenu("Reset Terrain")]
    public void ResetTerrain()
    {
        if (terrain == null) return;
        TerrainData terrainData = terrain.terrainData;
        int res = terrainData.heightmapResolution;
        float[,] heights = new float[res, res]; // Mặc định là 0 (phẳng)
        terrainData.SetHeights(0, 0, heights);
        
        // Xóa cây đã tạo
        CleanupTrees();
        
        Debug.Log("Terrain Reset.");
    }

    // ========== TERRAIN TEXTURES ==========
    [ContextMenu("Apply Terrain Textures")]
    public void ApplyTerrainTextures()
    {
        if (terrain == null) return;
        if (lowTexture == null && midTexture == null && highTexture == null)
        {
            Debug.LogWarning("No textures assigned. Skipping texture application.");
            return;
        }

        TerrainData terrainData = terrain.terrainData;

        // 1. Setup Terrain Layers (Unity's Terrain Layer system)
        TerrainLayer[] layers = new TerrainLayer[3];
        
        // Low Layer (cỏ, bùn)
        if (lowTexture != null)
        {
            layers[0] = new TerrainLayer();
            layers[0].diffuseTexture = lowTexture;
            layers[0].tileSize = new Vector2(15, 15); // Kích thước texture lặp lại
        }
        
        // Mid Layer (đất, đá)
        if (midTexture != null)
        {
            layers[1] = new TerrainLayer();
            layers[1].diffuseTexture = midTexture;
            layers[1].tileSize = new Vector2(15, 15);
        }
        
        // High Layer (đá núi, tuyết)
        if (highTexture != null)
        {
            layers[2] = new TerrainLayer();
            layers[2].diffuseTexture = highTexture;
            layers[2].tileSize = new Vector2(15, 15);
        }

        terrainData.terrainLayers = layers;

        // 2. Setup Alphamap (texture blending dựa trên độ cao)
        int alphamapWidth = terrainData.alphamapWidth;
        int alphamapHeight = terrainData.alphamapHeight;
        float[,,] alphamap = new float[alphamapWidth, alphamapHeight, 3];

        for (int x = 0; x < alphamapWidth; x++)
        {
            for (int y = 0; y < alphamapHeight; y++)
            {
                // Lấy độ cao tại vị trí này (normalized 0-1)
                float normX = (float)x / (alphamapWidth - 1);
                float normY = (float)y / (alphamapHeight - 1);
                float height = terrainData.GetInterpolatedHeight(normX, normY) / terrainData.size.y;

                // Phân bố texture dựa trên độ cao
                float[] weights = new float[3];

                if (height < midHeightThreshold)
                {
                    // Vùng thấp -> Low texture
                    weights[0] = 1f;
                }
else if (height < highHeightThreshold)
                {
                    // Vùng trung bình -> chuyển từ Low sang Mid
                    float blend = (height - midHeightThreshold) / (highHeightThreshold - midHeightThreshold);
                    weights[0] = 1f - blend;
                    weights[1] = blend;
                }
                else
                {
                    // Vùng cao -> Mid và High
                    float blend = Mathf.Clamp01((height - highHeightThreshold) / (1f - highHeightThreshold));
                    weights[1] = 1f - blend;
                    weights[2] = blend;
                }

                // Gán vào alphamap
                alphamap[x, y, 0] = weights[0];
                alphamap[x, y, 1] = weights[1];
                alphamap[x, y, 2] = weights[2];
            }
        }

        terrainData.SetAlphamaps(0, 0, alphamap);
        Debug.Log("Terrain Textures Applied!");
    }

    // ========== HELPER FUNCTIONS ==========
    void CleanupTrees()
    {
        Transform treeParent = this.transform.Find("GeneratedTrees");
        if (treeParent != null)
        {
            // Dùng DestroyImmediate trong Editor mode, Destroy trong Play mode
            if (Application.isPlaying)
            {
                Destroy(treeParent.gameObject);
            }
            else
            {
                for (int i = treeParent.childCount - 1; i >= 0; i--)
                {
                    DestroyImmediate(treeParent.GetChild(i).gameObject);
                }
            }
        }
    }
}