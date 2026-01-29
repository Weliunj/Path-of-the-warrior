using UnityEngine;

public class EnviromentManager : MonoBehaviour
{
    [Header("Cycle Settings")]
    [Range(0, 1)] public float timeValue; 
    public float dayCycleSpeed = 0.1f;    
    public bool isLooping = true;         

    [Header("Skybox & Sun")]
    public Material skyboxMaterial; 
    public Light sunLight; 

    private void Start()
    {
        if (skyboxMaterial != null)
        {
            // Tạo bản sao để không ảnh hưởng file gốc trên ổ cứng
            skyboxMaterial = new Material(skyboxMaterial);
            RenderSettings.skybox = skyboxMaterial;
        }
    }

    private void Update()
    {
        if (isLooping)
        {
            AutoLoopTime();
        }
        UpdateEnvironment();
    }

    private void AutoLoopTime()
    {
        timeValue += Time.deltaTime * dayCycleSpeed;
        if (timeValue >= 1f) timeValue = 0f; 
    }

    private void UpdateEnvironment()
    {
        // 1. Tính toán Alpha dựa trên hàm Sin (Sáng giữa ngày, tối ban đêm)
        float ambientAlpha = Mathf.Max(0, Mathf.Sin(timeValue * Mathf.PI));

        // 2. XOAY MẶT TRỜI VÀ BẦU TRỜI 360 ĐỘ
        float rotationDegrees = timeValue * 360f;
        float skyRotation = (timeValue * 360f) / 2f;

        if (sunLight != null)
        {
            // Xoay mặt trời quanh trục X
            sunLight.transform.rotation = Quaternion.Euler(rotationDegrees, -90f, 0f);
            
            // Cường độ mặt trời: chỉ sáng khi ở trên mặt đất (0 -> 0.5)
            if (timeValue > 0f && timeValue < 0.5f) 
                sunLight.intensity = Mathf.Lerp(0f, 1f, ambientAlpha);
            else
                sunLight.intensity = 0f;
        }

        if (skyboxMaterial != null)
        {
            // Xoay Material Skybox đồng bộ với mặt trời
            // Hầu hết Shader Skybox của Unity sử dụng thuộc tính "_Rotation"
            if (skyboxMaterial.HasProperty("_Rotation"))
            {
                skyboxMaterial.SetFloat("_Rotation", skyRotation);
            }
        }

        // 3. Ambient Intensity (Lighting Tab)
        RenderSettings.ambientIntensity = Mathf.Lerp(0.05f, 1.5f, ambientAlpha);

        // 4. Chỉnh độ sáng (V - Value) cho Skybox
        if (skyboxMaterial != null)
        {
            float vValue = Mathf.Lerp(0.03f, 0.55f, ambientAlpha);
            Color finalSkyColor = Color.HSVToRGB(0, 0, vValue);

            if (skyboxMaterial.HasProperty("_Tint"))
                skyboxMaterial.SetColor("_Tint", finalSkyColor);
            else if (skyboxMaterial.HasProperty("_SkyColor"))
                skyboxMaterial.SetColor("_SkyColor", finalSkyColor);
        }
    }
}