using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Fading_Mager : MonoBehaviour
{
    private bool isFading = false;
    public Material block;
    public float TimeFade = 5f;
    [Range(0, 1)]public int target;
    private bool Faded = false;
    void Start()
    {
        target = 0;

        // Lấy Renderer của đối tượng để truy cập Material đang hiển thị
        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            // rend.material sẽ tự động tạo một bản sao (Instance) cho đối tượng này
            block = rend.material; 
        }
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (!isFading && block != null)
            {
                Faded = !Faded;
                target = Faded ? 0 : 1;
                StartCoroutine(FadeToZero());
            }
        }
    }

    private IEnumerator FadeToZero()
    {
        isFading = true;

        float startAlpha = block.color.a;
        float elapsed = 0f;     //thoi gian troi qua
        Color c = block.color;      //

        while (elapsed < TimeFade)
        {
            elapsed += Time.deltaTime;
            float a = Mathf.Lerp(startAlpha, target, elapsed / TimeFade);
            c.a = a;
            block.color = c;
            yield return null;
        }
        c.a = target;       // Chạy để khớp với mong muốn
        block.color = c;
        isFading = false;
    }
}
