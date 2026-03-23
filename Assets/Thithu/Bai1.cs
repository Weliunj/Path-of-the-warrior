using System.Collections;
using UnityEngine;

public class Bai1 : MonoBehaviour
{
    public GameObject tank;
    public GameObject[] waypoints;
    public float speed = 3f;
    public float turnTime = 2f;
    void Start()
    {
        Vector3 startPos = new Vector3(waypoints[0].transform.position.x, tank.transform.position.y, waypoints[0].transform.position.z);
        tank.transform.position = startPos;
        StartCoroutine(Handle());
    }

    public IEnumerator Handle()
    {
        StartCoroutine(Move());
        yield return new WaitForSeconds(1f);
        StartCoroutine(Rota());
        yield return new WaitForSeconds(1f);
        Debug.Log("Fire");
    }

    public IEnumerator Move()
    {
        Vector3 EndPos = new Vector3(waypoints[1].transform.position.x, tank.transform.position.y, waypoints[1].transform.position.z);
        while(Vector3.Distance(transform.position, EndPos) > 0.2f)  
        {
            tank.transform.position = Vector3.MoveTowards(tank.transform.position, EndPos, speed * Time.deltaTime);
            yield return null;
        }
    }
    public IEnumerator Rota()
    {
        Quaternion startRota = tank.transform.rotation;
         float elapsed = 0f;
         while(elapsed < turnTime)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / turnTime;
            Quaternion targetRota = Quaternion.Euler(0, 90, 0);
            tank.transform.rotation = Quaternion.Slerp(startRota, targetRota, percent);
            yield return null;
        }
    }
}
