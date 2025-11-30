using UnityEngine;

public class monsterShoot : MonoBehaviour
{
    [SerializeField] private GameObject _arrow;

    private Archer ar;
    private bool _isShooting;

    public bool isShooting => _isShooting;

    public void Initialize(Archer ar)   
    {
        this.ar = ar;
    }

    public void shoot(float power, GameObject player)
    {
        if (_isShooting) return;
        _isShooting = true;

        Vector3 target = player.transform.position;
        Vector3 shootDir = (target - transform.position).normalized;

        Quaternion rot = Quaternion.LookRotation(shootDir) * Quaternion.Euler(89f, 0f, 0f); 

        GameObject arrow = Instantiate(_arrow, transform.position, rot);
        Rigidbody rb = arrow.GetComponent<Rigidbody>();
        if (rb == null) rb = arrow.AddComponent<Rigidbody>();
        rb.useGravity = true;

        Vector3 dir = target - transform.position;
        float dist = dir.magnitude;
        dir.y += dist * 0.3f;

        rb.AddForce(dir.normalized * (power + dist * 0.2f), ForceMode.VelocityChange);

        Destroy(arrow, 3f);
        Invoke(nameof(resetShot), 0.5f);
    }


    private void resetShot()
    {
        _isShooting = false;
    }
}
