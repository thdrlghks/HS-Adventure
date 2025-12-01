using UnityEngine;
using System.Collections;

public class W_Bomb : W_Base
{
    public float throwStaminaCost = 30f; // 스테미나 소모량

    public GameObject bombPrefab;
    public Transform throwPoint;
    public float throwDelay = 0.79f; // 애니메이션과 타이밍 맞추기

    public float throwForce = 5f; // 앞으로 던지는 힘
    public float throwUpwardForce = 8f; // 위로 던지는 힘

    private bool isThrowing = false;

    public override bool IsAttacking => isThrowing;
    public override bool IsShielding => false; // 우클릭 기능 없음

    public override void Attack()
    {
        if (!isThrowing && controller.isGrounded)
        {
            if (playerStats.UseStamina(throwStaminaCost))
            {
                Vector3 lockedDirection = Camera.main.transform.forward;
                // 방향 지정
                StartCoroutine(ThrowRoutine(lockedDirection));
            }
            
        }
    }

    IEnumerator ThrowRoutine(Vector3 targetDir)
    {
        isThrowing = true;
        animator.SetTrigger("Attack1");

        // 플레이어 회전
        RotatePlayerToDirection(targetDir);
        // 애니메이션 타이밍에 맞춰서 기다림
        yield return new WaitForSeconds(throwDelay);

        if (bombPrefab != null && throwPoint != null)
        {
            GameObject bomb = Instantiate(bombPrefab, throwPoint.position, throwPoint.rotation);
            Rigidbody rb = bomb.GetComponent<Rigidbody>();
            Collider bombCol = bomb.GetComponent<Collider>();

            // 플레이어와 충돌 무시
            if (bombCol != null && controller != null)
            {
                Physics.IgnoreCollision(controller, bombCol, true);
            }

            if (rb != null)
            {
                Vector3 throwDir = (targetDir + Vector3.up * 0.2f).normalized; // 포물선
                Vector3 force = (targetDir * throwForce) + (Vector3.up * throwUpwardForce);

                rb.AddForce(force, ForceMode.Impulse);
            }
        }

        yield return new WaitForSeconds(0.8f);
        isThrowing = false;
    }

    void RotatePlayerToDirection(Vector3 direction)
    { // 던지는 방향으로 회전
        direction.y = 0f;
        if (direction.sqrMagnitude > 0.001f)
        {
            owner.transform.rotation = Quaternion.LookRotation(direction);
        }
    }
}