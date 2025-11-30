using UnityEngine;
using System.Collections;

public class W_Bomb : W_Base
{
    public float throwStaminaCost = 30f;

    public GameObject bombPrefab;
    public Transform throwPoint;
    public float throwDelay = 0.79f;

    public float throwForce = 5f;
    public float throwUpwardForce = 8f;

    private bool isThrowing = false;

    public override bool IsAttacking => isThrowing;
    public override bool IsShielding => false;

    public override void Attack()
    {
        if (!isThrowing && controller.isGrounded)
        {
            if (playerStats.UseStamina(throwStaminaCost))
            {
                // 1. 🔥 [핵심] 클릭한 순간의 카메라 방향을 미리 저장(캡처)합니다.
                Vector3 lockedDirection = Camera.main.transform.forward;

                // 2. 저장한 방향을 코루틴에게 넘겨줍니다.
                StartCoroutine(ThrowRoutine(lockedDirection));
            }
            
        }
    }

    // 매개변수로 클릭 당시의 방향(targetDir)을 받습니다.
    IEnumerator ThrowRoutine(Vector3 targetDir)
    {
        isThrowing = true;
        animator.SetTrigger("Attack1");

        // 3. 플레이어 회전도 '현재 카메라'가 아니라 '클릭했던 방향'으로 고정합니다.
        RotatePlayerToDirection(targetDir);

        yield return new WaitForSeconds(throwDelay);

        if (bombPrefab != null && throwPoint != null)
        {
            GameObject bomb = Instantiate(bombPrefab, throwPoint.position, throwPoint.rotation);
            Rigidbody rb = bomb.GetComponent<Rigidbody>();
            Collider bombCol = bomb.GetComponent<Collider>();

            // 플레이어와 충돌 무시 (자폭 방지)
            if (bombCol != null && controller != null)
            {
                Physics.IgnoreCollision(controller, bombCol, true);
            }

            if (rb != null)
            {
                // 4. 🔥 날아가는 방향 계산도 'targetDir'을 기준으로 합니다.
                // (선입력된 방향 + 위쪽 살짝)
                Vector3 throwDir = (targetDir + Vector3.up * 0.2f).normalized;

                // 힘 계산
                Vector3 force = (targetDir * throwForce) + (Vector3.up * throwUpwardForce);

                rb.AddForce(force, ForceMode.Impulse);
            }
        }

        yield return new WaitForSeconds(0.8f);
        isThrowing = false;
    }

    // 카메라 대신 특정 방향(Vector3)을 보게 하는 함수로 변경
    void RotatePlayerToDirection(Vector3 direction)
    {
        // Y축 회전만 반영하기 위해 높이 값 제거
        direction.y = 0f;

        // 방향이 너무 작으면(0에 가까우면) 회전 안 함
        if (direction.sqrMagnitude > 0.001f)
        {
            owner.transform.rotation = Quaternion.LookRotation(direction);
        }
    }
}