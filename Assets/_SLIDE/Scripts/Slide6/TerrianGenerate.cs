using Unity.Mathematics;
using UnityEngine;

public class TerrianGenerate : MonoBehaviour
{  
    public enum Type{   LinearGradient, RadialGradient, PerlinNoise}
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Tooltip("Tick để tự động tạo khi Start")]
    public bool autoGenerate = true;
    private int width;  // Đã thay đổi kiểu dữ liệu thành int
    private int height; // Đã thay đổi kiểu dữ liệu thành int

    [Tooltip("Terrain target (optional). Nếu để trống sẽ lấy Terrain trên cùng object hoặc Terrain.activeTerrain")]
    public Terrain terrain;

    [Tooltip("Kiểu gradient")]
    public Type type = Type.LinearGradient;

    [Header("Radial Settings")]
    [Tooltip("Center in normalized terrain coordinates (0..1)")]
    public Vector2 radialCenter = new Vector2(0.5f, 0.5f);
    [Tooltip("Radius as normalized fraction (0..1), relative to max distance to corner")]
    [Range(0f, 1f)]
    public float radius = 0.5f;
    [Tooltip("Invert radial (center low, edge high)")]
    public bool invertRadial = false;

    [Header("Perlin Settings")]
    public float perlinScale = 50f; // Đã tăng giá trị để các chi tiết nhiễu nhỏ hơn và dày đặc hơn
    public float perlinOffsetX = 0f;
    public float perlinOffsetY = 0f;

    /// </summary>
    void Start()
    { 
        // Nếu autoGenerate là false, thoát khỏi phương thức Start ngay lập tức.
        if (!autoGenerate) return;
        
        // Nếu autoGenerate là true, tiếp tục thực hiện logic tạo địa hình.
        // (Không cần kiểm tra 'if (autoGenerate)' lần nữa vì đã kiểm tra ở trên)
        if (terrain == null) // Nếu terrain chưa được gán từ Inspector
        {   
            terrain = GetComponent<Terrain>(); // Thử lấy từ cùng GameObject
        }
        
        if (terrain == null) // Nếu vẫn chưa tìm thấy trên cùng GameObject
        {
            terrain = Terrain.activeTerrain; // Thử lấy Terrain đang hoạt động trong scene
        }

        if (terrain != null) // Nếu đã tìm thấy terrain
        {
            width = terrain.terrainData.heightmapResolution; // Khởi tạo width
            height = terrain.terrainData.heightmapResolution; // Khởi tạo height
            ApplyGradient(terrain);
        }
        else
            Debug.LogWarning("No Terrain found to generate.");
    }
    
    // Update không được sử dụng trong ví dụ này, có thể xóa nếu không cần.
    // protected virtual void Update() { }

    /// <summary>
    /// Tạo địa hình với độ dốc tuyến tính từ trái sang phải (hoặc từ dưới lên trên).
    /// </summary>
    /// <param name="terrainTarget">Đối tượng Terrain cần tạo.</param>
    [ContextMenu("Set Linear Gradient")]
    public void SetLinearGradientTerrain(Terrain terrainTarget)
    {
        // Kiểm tra nếu đối tượng Terrain là null, in lỗi và thoát.
        if (terrainTarget == null)
        {
            Debug.LogError("Terrain is null");
            return;
        }

        int res = width; // Sử dụng biến thành viên width
        // Mảng 2 chiều để lưu trữ giá trị chiều cao cho từng điểm trên địa hình.
        float[,] heights = new float[res, res];

        for (int x = 0; x < res; x++)
        {
            for (int y = 0; y < res; y++)
            {
                // Tính toán chiều cao: Dốc tuyến tính theo trục X.
                // Điểm x=0 có chiều cao 0, điểm x=res-1 có chiều cao 1.
                // LƯU Ý: mảng heights được index là [y, x] theo API Unity
                heights[y, x] = (float)x / (res - 1);
            }
        }

        terrainTarget.terrainData.SetHeights(0, 0, heights);
    }

    /// <summary>
    /// Phương thức chính để áp dụng kiểu tạo địa hình đã chọn.
    /// </summary>
    /// <param name="terrainTarget">Đối tượng Terrain cần tạo.</param>
    [ContextMenu("Set Gradient")]
    public void ApplyGradient(Terrain terrainTarget)
    {
        // Kiểm tra nếu đối tượng Terrain là null, in lỗi và thoát.
        if (terrainTarget == null) { Debug.LogError("Terrain is null"); return; }
        // Dựa vào giá trị của biến 'type' để gọi phương thức tạo địa hình tương ứng.
        switch (type)
        {
            case Type.LinearGradient:
                SetLinearGradientTerrain(terrainTarget);
                break;
            case Type.RadialGradient:
                SetRadialGradientTerrain(terrainTarget);
                break;
            case Type.PerlinNoise:
                SetPerlinNoiseTerrain(terrainTarget);
                break;
        }
    }

    /// <summary>
    /// Tạo địa hình với độ dốc tròn (Radial Gradient).
    /// </summary>
    /// <param name="terrainTarget">Đối tượng Terrain cần tạo.</param>
    [ContextMenu("Set Radial Gradient")]
    public void SetRadialGradientTerrain(Terrain terrainTarget)
    {
        // Kiểm tra nếu đối tượng Terrain là null, in lỗi và thoát.
        if (terrainTarget == null)
        {
            Debug.LogError("Terrain is null");
            return;
        }

        int res = width; // Sử dụng biến thành viên width
        float[,] heights = new float[res, res];

        // Chuyển đổi tâm radial từ tọa độ chuẩn hóa (0-1) sang tọa độ pixel của heightmap.
        float cx = Mathf.Clamp01(radialCenter.x);
        float cy = Mathf.Clamp01(radialCenter.y);
        float centerX = cx * (res - 1);
        float centerY = cy * (res - 1);

        Vector2 center = new Vector2(centerX, centerY);
        // Tính khoảng cách lớn nhất từ tâm đến bất kỳ góc nào của địa hình.
        // Điều này giúp chuẩn hóa bán kính một cách hợp lý.
        float maxDist = 0f;
        maxDist = Mathf.Max(maxDist, Vector2.Distance(center, new Vector2(0,0)));
        maxDist = Mathf.Max(maxDist, Vector2.Distance(center, new Vector2(0,res-1)));
        maxDist = Mathf.Max(maxDist, Vector2.Distance(center, new Vector2(res-1,0)));
        maxDist = Mathf.Max(maxDist, Vector2.Distance(center, new Vector2(res-1,res-1)));

        // Tính bán kính hiệu quả dựa trên bán kính chuẩn hóa và khoảng cách lớn nhất.
        float effectiveRadius = Mathf.Max(0.0001f, Mathf.Clamp01(radius) * maxDist);

        for (int y = 0; y < res; y++)
        {
            for (int x = 0; x < res; x++)
            {
                // Tính khoảng cách từ điểm hiện tại (x,y) đến tâm.
                float d = Vector2.Distance(new Vector2(x, y), center);
                // Tính giá trị chiều cao: càng gần tâm thì giá trị càng cao (hoặc thấp nếu invert).
                float val = Mathf.Clamp01(1f - d / effectiveRadius);
                // Nếu invertRadial là true, đảo ngược giá trị chiều cao (tâm thấp, cạnh cao).
                if (invertRadial) val = 1f - val;
                heights[y, x] = val;
            }
        }

        terrainTarget.terrainData.SetHeights(0, 0, heights);
    }

    /// <summary>
    /// Tạo địa hình bằng thuật toán Perlin Noise.
    /// </summary>
    /// <param name="terrainTarget">Đối tượng Terrain cần tạo.</param>
    [ContextMenu("Set Perlin Noise")]
    public void SetPerlinNoiseTerrain(Terrain terrainTarget)
    {
        // Kiểm tra nếu đối tượng Terrain là null, in lỗi và thoát.
        if (terrainTarget == null)
        {
            Debug.LogError("Terrain is null");
            return;
        }

        int res = width; // Sử dụng biến thành viên width
        float[,] heights = new float[res, res];

        for (int x = 0; x < res; x++)
        {
            for (int y = 0; y < res; y++)
            {
                // Tính toán tọa độ để lấy mẫu từ hàm Perlin Noise.
                // (float)x / res: Chuẩn hóa tọa độ x từ 0 đến 1.
                // * perlinScale: Điều chỉnh "độ lớn" của các đặc điểm nhiễu. Giá trị lớn hơn tạo chi tiết nhỏ hơn.
                // + perlinOffsetX/Y: Dịch chuyển vị trí lấy mẫu nhiễu, tạo ra các mẫu địa hình khác nhau.
                float xCoord = (float)x / res * perlinScale + perlinOffsetX;
                float yCoord = (float)y / res * perlinScale + perlinOffsetY;
                // Mathf.PerlinNoise trả về giá trị từ 0 đến 1.
                heights[y, x] = Mathf.PerlinNoise(xCoord, yCoord);
            }
        }

        terrainTarget.terrainData.SetHeights(0, 0, heights);
    }
}
