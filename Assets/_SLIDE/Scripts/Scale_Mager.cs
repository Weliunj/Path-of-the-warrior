using UnityEngine;
using System.Collections;

public class Scale_Mager : MonoBehaviour
{
    [Header("Settings")]
    public float timeScale = 2f;
    public float minScale = 1f;
    public float maxScale = 3f;

    private void Start()
    {
        // Khởi tạo kích thước ban đầu
        transform.localScale = Vector3.one * minScale;
        // Bắt đầu chu kỳ lặp đi lặp lại
        StartCoroutine(ContinuousScaling());
    }

    IEnumerator ContinuousScaling()
    {
        while (true) // Vòng lặp vô tận để Scale qua lại
        {
            // 1. Phóng to lên Max
            yield return StartCoroutine(ScaleTo(Vector3.one * maxScale));

            // 2. Thu nhỏ về Min
            yield return StartCoroutine(ScaleTo(Vector3.one * minScale));
        }
    }

    IEnumerator ScaleTo(Vector3 targetScale)
    {
        Vector3 startScale = transform.localScale;
        float elapsed = 0f;

        while (elapsed < timeScale)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(startScale, targetScale, elapsed / timeScale);
            yield return null;
        }

        transform.localScale = targetScale;
    }
}