using System.Collections;
using UnityEngine;

public class EnemyBox : MonoBehaviour
{
    private PlayerCap player;
    public float cdAtk = 1f;
    public bool isAtk = false;

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            player = collision.gameObject.GetComponent<PlayerCap>();
            if(player != null && !isAtk)
            {
                StartCoroutine(AtkCd());
            }
        }
    }

    IEnumerator AtkCd()
    {
        isAtk = true;
        player.updateHp(35);
        yield return new WaitForSeconds(cdAtk);
        isAtk = false;
    }
}
