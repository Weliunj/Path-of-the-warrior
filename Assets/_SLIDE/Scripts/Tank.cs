using System.Collections;
using Palmmedia.ReportGenerator.Core.Reporting.Builders;
using Unity.VisualScripting;
using UnityEngine;

public class Tank : MonoBehaviour
{
    public GameObject target; 
    //Huong target
    [Header("Settings")]
    public float turnTime = 2f;
    public float speedMove = 5f;

    [Header("Atk")]
    public GameObject laser;
    public float AtkRanger = 5f;
    private bool done = true;

    void Start()
    {
       done = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T) && done)
        {
             StartCoroutine(TankHandle());
        }

    }

    public void AtkHandle()
    {
        
    }

    public IEnumerator TankHandle()
    {
        done = false;
        yield return StartCoroutine(TurnTank());
        yield return new WaitForSeconds(1f);

        yield return StartCoroutine(MoveTank());
        yield return new WaitForSeconds(1f);

        yield return StartCoroutine(AtkTank());
        yield return new WaitForSeconds(1f);
        done = true;
    }

    IEnumerator TurnTank()
    {
        float elapsed = 0f;

        //Tao 1 goc xoay ao
        Quaternion startRota = transform.rotation;

        while(elapsed < turnTime)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / turnTime;
            
            Vector3 direc = target.transform.position - transform.position;
            direc.y = 0;
            //Xoay goc xoay ao den target
            Quaternion targetRotation = Quaternion.LookRotation(direc);

            transform.rotation = Quaternion.Slerp(startRota, targetRotation, percent);
            yield return null;
        }
    }

    IEnumerator MoveTank()
    {
        while(Vector3.Distance(transform.position, target.transform.position) > AtkRanger)
        {
            transform.position = Vector3.MoveTowards(transform.position, target.transform.position, speedMove * Time.deltaTime);
            yield return null;
        }
    }

    IEnumerator AtkTank()
    {
        Debug.Log("Atk");
        yield return new WaitForSeconds(1f);
    }
    void OnDrawGizmos()
    {
        if(Vector3.Distance(transform.position, target.transform.position) < AtkRanger)
        {
            Gizmos.color = Color.red;
        }
        else
        {
            Gizmos.color = Color.green;
        }
        Gizmos.DrawWireSphere(transform.position, AtkRanger);
    }
}
