using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class P_Arrow : MonoBehaviour
{
    private Rigidbody rb;


    [Header("발사체 설정")]
    private float currentSpeed;
    public int damage = 10;
    public float lifeTime = 5f;

    private bool hasHit = false;

    public void Init(float speed)
    { // 화살 생성 후 파괴
        rb = GetComponent<Rigidbody>();
        this.currentSpeed = speed;

        // 화살 속도
        rb.linearVelocity = transform.forward * currentSpeed;

        Destroy(gameObject, lifeTime);
    }

    void FixedUpdate()
    {
        if (hasHit) return;

        if (rb.linearVelocity.sqrMagnitude > 0.5f)
        {
            transform.rotation = Quaternion.LookRotation(rb.linearVelocity);
        } // 화살 정면을 날아가는 방향으로.
    }

    void OnTriggerEnter(Collider other)
    { // 맞았을 시
        if (other.CompareTag("Player")) return;
        if (hasHit) return;

        hasHit = true;

        // 박힌 연출을 의도했으나 잘 안 됨..
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;
        Destroy(gameObject, 2f);

        transform.SetParent(other.transform);
    }
}