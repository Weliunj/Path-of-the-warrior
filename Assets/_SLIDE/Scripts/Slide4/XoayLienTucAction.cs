using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using Unity.Mathematics;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "XoayLienTuc", story: "Xoay [Object] lien tuc [Angle] do", category: "Action", id: "651f5a8805a71abacde0459082b8e935")]
public partial class XoayLienTucAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Object;
    [SerializeReference] public BlackboardVariable<float> Angle;

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (Object.Value == null) return Status.Failure;

        // Nhân thêm Time.deltaTime để xoay đều theo thời gian
        float rotationStep = Angle.Value * Time.deltaTime;
        Object.Value.transform.rotation *= quaternion.AxisAngle(Vector3.up, Angle.Value);

        return Status.Running;
    }

    protected override void OnEnd()
    {
    }
}

