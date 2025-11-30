using UnityEngine;
using System.Collections;

public class W_Bomb : W_Base
{
    public GameObject bombPrefab;
    public Transform throwPoint;
    public float throwDelay = 0.3f;

    // 던지는 힘 조절
    public float throwForce = 15f;
    public float throwUpwardForce = 2f; // 위쪽으로 살짝 더 띄워주는 힘

    private bool isThrowing = false;

    // 🔥 [핵심] 이 프로퍼티를 오버라이드하면 P_Move가 알아서 이동을 멈춰줍니다.
    public override bool IsAttacking => isThrowing;

    // 폭탄은 조준(Shield) 기능이 없으므로 false
    public override bool IsShielding => false;

    public override void Attack()
    {
        // 땅에 있고, 이미 던지는 중이 아닐 때만
        if (!isThrowing && controller.isGrounded)
        {
            StartCoroutine(ThrowRoutine());
        }
    }

    IEnumerator ThrowRoutine()
    {
        isThrowing = true; // 이동 정지 시작

        // 투척 애니메이션
        animator.SetTrigger("Attack1");

        // 1. 플레이어를 카메라 방향으로 회전 (던질 때 정면 보기)
        RotatePlayerToCamera();

        yield return new WaitForSeconds(throwDelay);

        // 폭탄 생성
        if (bombPrefab != null && throwPoint != null)
        {
            GameObject bomb = Instantiate(bombPrefab, throwPoint.position, throwPoint.rotation);
            Rigidbody rb = bomb.GetComponent<Rigidbody>();

            if (rb != null)
            {
                // 2. 던지는 방향 계산 (카메라 정면 + 약간 위쪽)
                Vector3 camForward = Camera.main.transform.forward;
                Vector3 throwDir = (camForward + Vector3.up * 0.2f).normalized; // 0.2f 정도로 미세 조정

                // ForceMode.VelocityChange가 무게 상관없이 똑같은 속도로 날아가서 더 직관적임
                // (Impulse를 써도 되지만 VelocityChange가 슈팅게임 감각에 더 맞음)
                Vector3 force = (camForward * throwForce) + (Vector3.up * throwUpwardForce);
                rb.AddForce(force, ForceMode.Impulse);
            }
        }

        yield return new WaitForSeconds(0.8f); // 후딜레이 (애니메이션 끝날 때까지 대기)
        isThrowing = false; // 이동 정지 해제
    }

    void RotatePlayerToCamera()
    {
        if (Camera.main == null) return;
        Vector3 camForward = Camera.main.transform.forward;
        camForward.y = 0f;
        if (camForward.sqrMagnitude > 0.001f)
        {
            owner.transform.rotation = Quaternion.LookRotation(camForward);
        }
    }
}