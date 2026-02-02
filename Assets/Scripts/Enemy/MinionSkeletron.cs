using Unity.VisualScripting;
using UnityEngine;

public class MinionSkeletron : EnemyBase
{
    [Header("Atk")]
    public float atkcd = 5f;
    
    protected override void Awake()
    {
        base.Awake(); 
    }

    protected override void Start()
    {
        base.Start(); 
    }

    protected override void Update()
    {
        base.Update();
    }
}
