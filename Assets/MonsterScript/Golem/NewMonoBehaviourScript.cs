using UnityEngine;

public class ArmHitBox : MonoBehaviour
{
    public float damage = 10f;
    public bool active = false;    
    private P_Player player;

    private void OnTriggerEnter(Collider other)
    {
        if (!active) return;     

        if (other.CompareTag("Player"))
        {
            P_Player player = other.GetComponent<P_Player>();
            if (player != null)
            {
                player.TakeDamage((int)damage);
                Debug.Log("Golem attack hit player!");
            }
        }
    }
}
