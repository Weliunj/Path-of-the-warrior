using UnityEngine;

public class Door : MonoBehaviour
{
    public GameObject door;
    public void OpenGate()
    {
        door.transform.Rotate(0, 90, 0); 
    }
}
