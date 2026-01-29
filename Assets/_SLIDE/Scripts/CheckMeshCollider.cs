using TMPro;
using UnityEngine;

public class CheckMeshCollider : MonoBehaviour
{
    public bool isStopping = false;
    public TextMeshPro info;

    void Start()
    {
        info.text = "";
    }
    void Update()
    {
        
    }
    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isStopping)
        {
            isStopping = true;
            Rigidbody rb = GetComponent<Rigidbody>();
            if(rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
            }
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            info.text = "Đã chạm chuối";
        }
    }
}
