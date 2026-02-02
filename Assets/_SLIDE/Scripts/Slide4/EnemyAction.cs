using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI; // Để dùng NavMeshAgent

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Enemy", story: "[Agent] go to [target] [distance]", category: "Action", id: "a924c9f26ab84e72c74d2fddf6a35cc7")]
public partial class EnemyAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<GameObject> target;
    [SerializeReference] public BlackboardVariable<float> Distance;

    private NavMeshAgent _navAgent;

    protected override Status OnStart()
    {
        if (Agent.Value == null || target.Value == null) return Status.Failure;

        // Lấy NavMeshAgent từ Agent được gán trong Blackboard
        _navAgent = Agent.Value.GetComponent<NavMeshAgent>();
        
        if (_navAgent == null) return Status.Failure;

        _navAgent.isStopped = false;        //Cho phep di chuyen
        _navAgent.SetDestination(target.Value.transform.position);      //Diem start
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (_navAgent == null || target.Value == null) return Status.Failure;

        _navAgent.SetDestination(target.Value.transform.position);

        float dis = Vector3.Distance(Agent.Value.transform.position, target.Value.transform.position);
        if (dis <= Distance.Value)
        {
            _navAgent.isStopped = true;
            return Status.Success;
        }

        return Status.Running;
    }

    protected override void OnEnd()
    {
        if (_navAgent != null && _navAgent.isOnNavMesh)
            _navAgent.isStopped = true;
    }
}
