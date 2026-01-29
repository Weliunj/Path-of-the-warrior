using Unity.VisualScripting;
using UnityEngine;

public class Cube : MonoBehaviour
{
    private Rigidbody rb;
    public float lifetime;
    private Material baseMaterial;

    void Start()
    {
        Renderer rend = GetComponent<Renderer>();
        if(rend != null)
        {
            baseMaterial = rend.material;
        }
        baseMaterial.color = Random.ColorHSV(0f, 1f, 1f, 1f, 1f, 1f);


        rb = GetComponent<Rigidbody>();
        float x = Random.Range(-15f, 15f);
        float y = Random.Range(50f, 100f);
        float z = Random.Range(-15f, 15f);
        rb.AddForce(new Vector3(x, y, z), ForceMode.Impulse);

        float s = Random.Range(1f, 5f);
        transform.localScale = Vector3.one * s;
    }

    // Update is called once per frame
    void Update()
    {
        Destroy(gameObject, lifetime);
    }
}
