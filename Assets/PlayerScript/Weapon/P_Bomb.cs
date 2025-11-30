using UnityEngine;

public class P_Bomb : MonoBehaviour
{
    [Header("Explosion Settings")]
    public float explosionDelay = 3.0f; // 3초 뒤 폭발
    public float explosionRadius = 3f;
    public int damage = 30;
    public GameObject explosionEffect;
    public LayerMask targetLayer;

    // (선택) 튕길 때 소리
    // public AudioClip bounceSound;
    // private AudioSource audioSource;

    private bool hasExploded = false;

    void Start()
    {
        // audioSource = GetComponent<AudioSource>();

        // 🔥 [핵심] 생성되자마자 "3초 뒤에 터져라" 라고 예약
        Invoke("Explode", explosionDelay);
    }

    // 💥 충돌 시: 이제는 터지지 않고 그냥 튕깁니다.
    void OnCollisionEnter(Collision collision)
    {
        // 플레이어 몸에 닿았을 때 물리적 충돌은 일어나지만 로직은 무시
        if (collision.gameObject.CompareTag("Player")) return;

        // (선택) 벽이나 바닥에 부딪힐 때 '퉁' 소리 재생
        // if (audioSource != null && bounceSound != null)
        // {
        //     audioSource.PlayOneShot(bounceSound);
        // }
    }

    void Explode()
    {
        // 중복 폭발 방지
        if (hasExploded) return;
        hasExploded = true;

        // 1. 이펙트 생성
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, transform.rotation);
        }

        // 2. 광역 데미지 (Physics.OverlapSphere)
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius, targetLayer);

        foreach (Collider nearbyObject in colliders)
        {
            if (nearbyObject.CompareTag("Enemy"))
            {
                // 적에게 데미지 주기 (예시)
                Debug.Log(nearbyObject.name + " 폭발 휘말림!");
                // nearbyObject.GetComponent<Enemy>().TakeDamage(damage);
            }

            // 폭발 힘으로 밀어내기
            Rigidbody rb = nearbyObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(500f, transform.position, explosionRadius);
            }
        }

        // 3. 폭탄 제거
        Destroy(gameObject);
    }
}