using Unity.VisualScripting;
using UnityEngine;

public class arrowAttack : MonoBehaviour
{
    private P_Player _player;

    private void Awake()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        _player = p.GetComponent<P_Player>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            _player.TakeDamage(10);
        }
    }
}