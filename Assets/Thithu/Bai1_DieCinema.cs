using Unity.Cinemachine;
using UnityEngine;

public class Bai1_DieCinema : MonoBehaviour
{
    public PlayerCap player;
    public CinemachineCamera overViewCam;
    public CinemachineCamera PlayerCam;
    void Start()
    {
        player = FindFirstObjectByType<PlayerCap>();
        PlayerCam.Priority = 1;
        overViewCam.Priority = 5;

    }

    // Update is called once per frame
    void Update()
    {
        if(player.currHp <= 0)
        {
            overViewCam.Priority = 1;
            PlayerCam.Priority = 5;
        }
    }
}
