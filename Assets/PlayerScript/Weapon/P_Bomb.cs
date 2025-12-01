using UnityEngine;

public class P_Bomb : MonoBehaviour
{
    [Header("Explosion Settings")]
    public float explosionDelay = 3.0f; // 3초 뒤 폭발
    public float explosionRadius = 3f; // 폭발 범위
    public int damage = 30;

    public LayerMask targetLayer;
    private bool hasExploded = false;

    void Start()
    {
        Invoke("Explode", explosionDelay);
    }

    void OnCollisionEnter(Collision collision)
    { // 플레이어에 막히지 않도록
        if (collision.gameObject.CompareTag("Player")) return;
    }

    void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        // 폭발 이펙트 범위 확인
        GameObject rangeSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        rangeSphere.transform.position = transform.position;
        rangeSphere.transform.localScale = Vector3.one * explosionRadius * 2;
        Destroy(rangeSphere.GetComponent<Collider>());
        Renderer sphereRenderer = rangeSphere.GetComponent<Renderer>();
        Destroy(rangeSphere, 0.2f);

        // 광역 딜
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius, targetLayer);
        foreach (Collider nearbyObject in colliders)
        {
            if (nearbyObject.CompareTag("Enemy"))
            {
                Debug.Log(nearbyObject.name + " 폭발 휘말림! " + damage + "의 데미지");
            }

            Rigidbody rb = nearbyObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(500f, transform.position, explosionRadius);
            }
        }

        // 폭탄 제거
        Destroy(gameObject);
    }
}