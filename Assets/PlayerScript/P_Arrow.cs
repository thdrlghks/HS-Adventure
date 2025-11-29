using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class P_Arrow : MonoBehaviour
{
    [Header("발사체 설정")]
    private float currentSpeed;
    public int damage = 10;
    public float lifeTime = 5f;

    private Rigidbody rb;
    private bool hasHit = false; // 충돌했는지 확인하는 변수

    public void Init(float speed)
    {
        rb = GetComponent<Rigidbody>();
        this.currentSpeed = speed;
        // 초기 발사 힘
        rb.linearVelocity = transform.forward * currentSpeed;

        Destroy(gameObject, lifeTime);
    }

    // 🔥 [핵심 추가] 매 물리 프레임마다 실행
    void FixedUpdate()
    {
        // 1. 이미 어딘가에 박혔다면 회전 중단
        if (hasHit) return;

        // 2. 현재 이동 속도가 어느 정도 있을 때만 (정지 상태 떨림 방지)
        if (rb.linearVelocity.sqrMagnitude > 0.5f)
        {
            // 3. 이동 방향(velocity)을 바라보는 회전값(Quaternion)을 만들어서 적용
            transform.rotation = Quaternion.LookRotation(rb.linearVelocity);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // 플레이어 몸은 무시
        if (other.CompareTag("Player")) return;

        // 이미 맞았으면 로직 스킵 (중복 충돌 방지)
        if (hasHit) return;

        hasHit = true; // 맞았다고 표시 (이제 회전 멈춤)

        if (other.CompareTag("Enemy"))
        {
            Debug.Log("적중! 데미지: " + damage);
            // 적 데미지 처리...
            Destroy(gameObject);
        }
        else if (!other.isTrigger) // 벽이나 바닥
        {
            // 벽에 박히는 연출 (선택 사항)
            // 물리 연산을 끄고 벽에 고정시킴
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;

            // 화살이 바로 사라지면 어색하니까 2초 뒤 삭제
            Destroy(gameObject, 2f);

            // (주의) 부모를 벽으로 설정하면 벽이 움직일 때 따라감
            transform.SetParent(other.transform);
        }
    }
}