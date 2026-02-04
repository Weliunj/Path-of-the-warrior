using UnityEngine;

// ExecuteAlways: cho phép script chạy cả khi KHÔNG Play (Editor Mode)
[ExecuteAlways]
public class TerrianGenerate : MonoBehaviour
{
    // Enum chọn kiểu tạo địa hình
    public enum Type
    {
        LinearGradient,
        RadialGradient,
        PerlinNoise
    }

    // ==================== MAIN SETTINGS ====================

    public bool autoGenerate = true;        // Nếu true → tự generate terrain khi Start / chỉnh Inspector
    public Terrain terrain;                 // Tham chiếu tới Terrain component trong Scene
    public Type type = Type.PerlinNoise;    // Kiểu địa hình đang dùng

    public int resolution;                 // Độ phân giải heightmap (ví dụ 513, 1025)

    // ==================== RADIAL SETTINGS ====================

    [Range(0f, 1f)]
    public float radius = 0.6f;             // Bán kính radial (chuẩn hóa 0–1)
    public Vector2 radialCenter = new Vector2(0.5f, 0.5f); // Tâm radial (tọa độ chuẩn hóa)
    public bool invertRadial = false;        // Đảo ngược cao–thấp (tâm thấp, rìa cao)

    // ==================== PERLIN BASIC ====================

    public float perlinScale = 4f;           // Tỉ lệ lấy mẫu Perlin (ảnh hưởng kích thước đồi)
    public float offsetX;                    // Dịch noise theo trục X (đổi seed)
    public float offsetY;                    // Dịch noise theo trục Y (đổi seed)

    // ==================== PERLIN ADVANCED ====================

    public int octaves = 4;                  // Số lớp noise (càng nhiều càng chi tiết)
    public float persistence = 0.5f;         // Giảm biên độ mỗi octave
    public float lacunarity = 2f;            // Tăng tần số mỗi octave
    public float heightMultiplier = 0.3f;    // Độ cao tổng thể (0–1, phụ thuộc TerrainData.size.y)

    // ==================== UNITY LIFECYCLE ====================

    void Start()
    {
        if (!autoGenerate) return; // Không tự generate nếu user tắt

        // Nếu chưa gán Terrain trong Inspector
        if (!terrain)
        {
            // Thử lấy Terrain trên cùng GameObject
            terrain = GetComponent<Terrain>();

            // Nếu vẫn null → lấy Terrain đang active trong Scene
            if (!terrain)
                terrain = Terrain.activeTerrain;
        }

        // Nếu vẫn không tìm thấy Terrain
        if (!terrain)
        {
            Debug.LogWarning("No Terrain found!");
            return;
        }

        // Lấy TerrainData → lấy độ phân giải heightmap
        // heightmapResolution = kích thước mảng heights[,] bắt buộc phải khớp
        resolution = terrain.terrainData.heightmapResolution;

        // Gọi hàm generate terrain
        Generate();
    }

    // OnValidate được gọi khi chỉnh giá trị trong Inspector (Editor)
    void OnValidate()
    {
        if (!autoGenerate || !terrain) return;

        // Cập nhật lại resolution từ TerrainData
        resolution = terrain.terrainData.heightmapResolution;

        // Generate lại địa hình ngay khi chỉnh Inspector
        Generate();
    }

    // ==================== GENERATE DISPATCH ====================

    [ContextMenu("Generate Terrain")]
    public void Generate()
    {
        // Gọi hàm tạo địa hình tương ứng với enum Type
        switch (type)
        {
            case Type.LinearGradient:
                GenerateLinear();
                break;

            case Type.RadialGradient:
                GenerateRadial();
                break;

            case Type.PerlinNoise:
                GeneratePerlin();
                break;
        }
    }

    // ==================== LINEAR GRADIENT ====================

    void GenerateLinear()
    {
        // Tạo mảng heightmap 2D (0–1)
        float[,] heights = new float[resolution, resolution];

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                // Độ cao tăng dần theo trục X
                // heights[y, x] là chuẩn index của Unity
                heights[y, x] = (float)x / (resolution - 1);
            }
        }

        // Ghi toàn bộ heightmap vào TerrainData
        // (0,0) là góc dưới trái của terrain
        terrain.terrainData.SetHeights(0, 0, heights);
    }

    // ==================== RADIAL GRADIENT ====================

    void GenerateRadial()
    {
        float[,] heights = new float[resolution, resolution];

        // Chuyển tâm từ tọa độ chuẩn hóa (0–1) sang pixel heightmap
        Vector2 center = new Vector2(
            radialCenter.x * (resolution - 1),
            radialCenter.y * (resolution - 1)
        );

        // Tính khoảng cách lớn nhất từ tâm đến các góc terrain
        float maxDist = Vector2.Distance(center, Vector2.zero);
        maxDist = Mathf.Max(maxDist, Vector2.Distance(center, new Vector2(0, resolution)));
        maxDist = Mathf.Max(maxDist, Vector2.Distance(center, new Vector2(resolution, 0)));
        maxDist = Mathf.Max(maxDist, Vector2.Distance(center, new Vector2(resolution, resolution)));

        // Bán kính thật tính theo heightmap
        float realRadius = Mathf.Max(0.001f, radius * maxDist);

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                // Khoảng cách từ điểm hiện tại tới tâm
                float dist = Vector2.Distance(new Vector2(x, y), center);

                // Chuẩn hóa giá trị cao–thấp
                float value = Mathf.Clamp01(1f - dist / realRadius);

                // Gán vào heightmap (có thể đảo ngược)
                heights[y, x] = invertRadial ? 1f - value : value;
            }
        }

        // Ghi heightmap vào TerrainData
        terrain.terrainData.SetHeights(0, 0, heights);
    }

    // ==================== PERLIN NOISE (FRACTAL) ====================

    void GeneratePerlin()
    {
        float[,] heights = new float[resolution, resolution];

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float amplitude = 1f; // Biên độ ban đầu
                float frequency = 1f; // Tần số ban đầu
                float noise = 0f;     // Tổng giá trị noise

                // Fractal Perlin Noise (nhiều octave)
                for (int i = 0; i < octaves; i++)
                {
                    // Chuẩn hóa tọa độ heightmap về 0–1
                    float sampleX = (x / (float)resolution) * perlinScale * frequency + offsetX;
                    float sampleY = (y / (float)resolution) * perlinScale * frequency + offsetY;

                    // PerlinNoise trả về 0–1 → map về -1 → 1
                    float perlin = Mathf.PerlinNoise(sampleX, sampleY) * 2f - 1f;

                    // Cộng dồn theo biên độ
                    noise += perlin * amplitude;

                    // Giảm biên độ, tăng tần số cho octave tiếp theo
                    amplitude *= persistence;
                    frequency *= lacunarity;
                }

                // Chuẩn hóa noise về 0–1
                noise = (noise + 1f) * 0.5f;

                // Gán độ cao (0–1), height thật = value * TerrainData.size.y
                heights[y, x] = Mathf.Clamp01(noise * heightMultiplier);
            }
        }

        // Ghi dữ liệu độ cao vào TerrainData
        terrain.terrainData.SetHeights(0, 0, heights);
    }
}
