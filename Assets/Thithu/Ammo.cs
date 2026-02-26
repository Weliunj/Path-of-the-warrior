using System.Collections;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.Rendering;

public class Ammo : MonoBehaviour
{
    Rigidbody rb;
    PlayerCap player;
    protected Volume volume;
    public GameObject render;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.linearVelocity = transform.forward * 10f;
        volume = FindFirstObjectByType<Volume>();
        volume.weight = 0;
    }

    void Update()
    {
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            render.SetActive(false);
            player = other.gameObject.GetComponent<PlayerCap>();
            player.updateHp(50);
            StartCoroutine(BlurEff());
        }
    }

    IEnumerator BlurEff()
    {
        volume.weight = 1;
        yield return new WaitForSeconds(1f);
        volume.weight = 0;
        Destroy(gameObject);
    }
}
