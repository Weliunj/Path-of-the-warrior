using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class Bai3 : MonoBehaviour
{
    public Volume volume;
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.V))
        {
            volume.enabled = !volume.enabled;
        }
    }
}
