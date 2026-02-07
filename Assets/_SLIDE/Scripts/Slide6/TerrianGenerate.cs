using UnityEngine;

[ExecuteAlways]
public class TerrianGenerate : MonoBehaviour
{
    // ==================== TERRAIN TYPE ====================

    public enum TerrainKind
    {
        Plain,
        Hill,
        Lake
    }

    public TerrainKind terrainKind;

    // ==================== TERRAIN SETTINGS ====================

    public Terrain terrain;
    public bool autoGenerate = true;

    int resolution;

    // ==================== PERLIN SETTINGS ====================

    [Header("Perlin Noise")]
    public float perlinScale = 4f;
    public int octaves = 4;
    public float persistence = 0.5f;
    public float lacunarity = 2f;
    public float heightMultiplier = 0.3f;
    public Vector2 noiseOffset;

    // ==================== RADIAL (LAKE) ====================

    [Header("Lake Settings")]
    [Range(0f, 1f)] public float lakeRadius = 0.45f;
    public bool invertLake = true;

    // ==================== SPAWN PREFABS ====================

    [Header("Prefabs")]
    public GameObject[] trees;
    public GameObject[] rocks;
    public GameObject[] grass;

    [Header("Spawn Count")]
    public int treeCount = 100;
    public int rockCount = 50;
    public int grassCount = 300;

    // ==================== UNITY ====================

    void Start()
    {
        if (!autoGenerate) return;

        if (!terrain)
            terrain = GetComponent<Terrain>() ?? Terrain.activeTerrain;

        if (!terrain) return;

        resolution = terrain.terrainData.heightmapResolution;

        GenerateTerrain();
        SpawnObjects();
    }

    void OnValidate()
    {
        if (!autoGenerate || !terrain) return;

        resolution = terrain.terrainData.heightmapResolution;
        GenerateTerrain();
    }

    // ==================== GENERATE TERRAIN ====================

    void GenerateTerrain()
    {
        float[,] heights = new float[resolution, resolution];

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float perlin = FractalPerlin(x, y);

                switch (terrainKind)
                {
                    case TerrainKind.Plain:
                        heights[y, x] = perlin * 0.15f; // rất phẳng
                        break;

                    case TerrainKind.Hill:
                        heights[y, x] = perlin * heightMultiplier;
                        break;

                    case TerrainKind.Lake:
                        float radial = RadialMask(x, y);
                        heights[y, x] = perlin * radial * 0.2f;
                        break;
                }
            }
        }

        terrain.terrainData.SetHeights(0, 0, heights);
    }

    // ==================== PERLIN ====================

    float FractalPerlin(int x, int y)
    {
        float amplitude = 1f;
        float frequency = 1f;
        float noise = 0f;

        for (int i = 0; i < octaves; i++)
        {
            float nx = (x / (float)resolution) * perlinScale * frequency + noiseOffset.x;
            float ny = (y / (float)resolution) * perlinScale * frequency + noiseOffset.y;

            float p = Mathf.PerlinNoise(nx, ny) * 2f - 1f;
            noise += p * amplitude;

            amplitude *= persistence;
            frequency *= lacunarity;
        }

        return Mathf.Clamp01((noise + 1f) * 0.5f);
    }

    // ==================== RADIAL MASK ====================

    float RadialMask(int x, int y)
    {
        Vector2 center = new Vector2(resolution / 2f, resolution / 2f);
        float dist = Vector2.Distance(new Vector2(x, y), center);
        float maxDist = resolution * lakeRadius;

        float value = Mathf.Clamp01(1f - dist / maxDist);
        return invertLake ? value : 1f - value;
    }

    // ==================== SPAWN OBJECT ====================

    void SpawnObjects()
    {
        ClearChildren();

        if (terrainKind == TerrainKind.Hill)
        {
            Spawn(rocks, rockCount, true);
            Spawn(trees, treeCount, true);
        }

        if (terrainKind == TerrainKind.Plain)
        {
            Spawn(trees, treeCount / 2, true);
            Spawn(grass, grassCount, false);
        }

        if (terrainKind == TerrainKind.Lake)
        {
            Spawn(grass, grassCount / 2, false);
        }
    }

    void Spawn(GameObject[] prefabs, int count, bool solid)
    {
        if (prefabs == null || prefabs.Length == 0) return;

        for (int i = 0; i < count; i++)
        {
            Vector3 pos = RandomTerrainPos();
            GameObject obj = Instantiate(
                prefabs[Random.Range(0, prefabs.Length)],
                pos,
                Quaternion.Euler(0, Random.Range(0, 360), 0),
                transform
            );

            if (!solid && obj.GetComponent<Collider>())
                DestroyImmediate(obj.GetComponent<Collider>());
        }
    }

    Vector3 RandomTerrainPos()
    {
        float x = Random.Range(0f, terrain.terrainData.size.x);
        float z = Random.Range(0f, terrain.terrainData.size.z);
        float y = terrain.SampleHeight(new Vector3(x, 0, z));
        return new Vector3(x, y, z) + terrain.transform.position;
    }

    void ClearChildren()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);
    }
}
