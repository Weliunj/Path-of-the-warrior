using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class Bai2 : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public CinemachineCamera PlayerCam;
    public CinemachineCamera BoxCam;
    public GameObject Player;
    public GameObject Trigger;
    private bool isTriggered = false;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float distance = Vector3.Distance(Player.transform.position, Trigger.transform.position);
        Debug.Log("Distance: " + distance);
        if(distance < 2f && !isTriggered)
        {
            StartCoroutine(OnTrigger());
        }
    }

    IEnumerator OnTrigger()
    {
        isTriggered = true;
        PlayerCam.Priority = 1;
        BoxCam.Priority = 15;
        yield return new WaitForSeconds(2f);
        PlayerCam.Priority = 15;
        BoxCam.Priority = 1;
    }
}
