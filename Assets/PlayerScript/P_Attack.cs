using UnityEngine;
using System.Collections;

public class P_Attack : MonoBehaviour
{
    [Header("공격 설정")]
    public float attackTurnSpeed = 10f;   // 회전 속도
    public float attackDuration = 2f;     // 공격 애니메이션 길이
    public string attackTrigger = "Attack1";

    private Animator animator;
    private bool isAttacking = false;
    public bool IsAttacking => isAttacking;

    private Quaternion targetRot;  // 목표 회전

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        // 공격 입력
        if (Input.GetMouseButtonDown(0) && !isAttacking)
        {
            StartCoroutine(PerformAttack());
        }

        // 공격 중에는 회전만 유지
        if (isAttacking)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * attackTurnSpeed);
        }
    }

    IEnumerator PerformAttack()
    {
        isAttacking = true;

        // 공격 시작 시, 마우스 바라보는 방향 계산
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 200f))
        {
            Vector3 targetDir = hit.point - transform.position;
            targetDir.y = 0f;

            if (targetDir.sqrMagnitude > 0.01f)
                targetRot = Quaternion.LookRotation(targetDir);
            else
                targetRot = transform.rotation;
        }

        // 애니메이션 실행
        animator.SetTrigger(attackTrigger);

        // 공격 중 2초 동안 회전 보정 유지
        yield return new WaitForSeconds(attackDuration);

        isAttacking = false;
    }
}
