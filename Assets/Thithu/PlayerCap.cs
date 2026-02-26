using Unity.Mathematics;
using UnityEngine;

public class PlayerCap : MonoBehaviour
{
    private float MoveX;
    private float MoveZ;
    private Rigidbody rb;
    public int StartHp = 100;
    public int currHp;
    public bool isDead = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currHp = StartHp;
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        MoveX = Input.GetAxis("Horizontal");
        MoveZ = Input.GetAxis("Vertical");
        Vector3 direc = new Vector3(MoveX,0,MoveZ).normalized * 5f;

            rb.linearVelocity = direc;
    }

    public void updateHp(int val)
    {
        if(isDead)
        {
            Debug.Log("Dead");
            return;
        }

        currHp -= val;
        currHp = math.clamp( currHp ,0, StartHp);
        if(currHp <= 0)
        {
            isDead = true;
        }
        Debug.Log($"Player Hp: {currHp}/{StartHp}");
    }

}
