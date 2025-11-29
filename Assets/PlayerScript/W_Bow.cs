using UnityEngine;
using System.Collections;

public class W_Bow : W_Base
{
    [Header("Bow Settings")]
    public GameObject arrowPrefab;
    public Transform firePoint;
    public float fireDelay = 0.1f;

    [Header("Charging Settings")]
    public float minForce = 15f;
    public float maxForce = 40f;
    public float maxChargeTime = 2.0f;

    [Header("Cooldown Settings")]
    public float attackCooldown = 1.0f;
    private float lastFireTime = -10f;

    private float currentChargeTime = 0f;
    private bool isCharging = false;

    public float aimRotationSpeed = 20f; // 회전 속도 약간 올림

    // 원래 쉴드 속도를 기억했다가 되돌리기 위한 변수 (선택사항)
    private float originalShieldSpeed;

    public override bool IsShielding => animator.GetBool("Shield");

    public override bool CanMoveWhileAiming => false;
    public override void Attack() { }

    // 🔥 [추가] 시작할 때 P_Move 설정을 활 전용으로 변경
    void Start()
    {
        // P_Move 컴포넌트가 있다면
        if (move != null)
        {
            // 원래 쉴드 속도 저장 (나중에 필요할 수 있으니)
            originalShieldSpeed = move.shieldSpeed;

            // 🔥 [핵심 1] 조준(Shield) 중 이동 속도를 0으로 강제 설정
            // 이러면 P_Move 스크립트를 안 고쳐도, 활을 들면 조준 시 못 움직입니다.
            move.shieldSpeed = 0f;
        }
    }

    // (옵션) 활을 집어넣을 때(파괴되거나 비활성될 때) 속도 복구
    void OnDisable()
    {
        if (move != null)
        {
            move.shieldSpeed = originalShieldSpeed;
        }
    }

    public override void OnUpdate()
    {
        HandleAiming();
        HandleShooting();
    }

    // 🔥 [핵심 2] 회전 로직을 Update가 아닌 LateUpdate로 이동
    // 이유: P_Move가 Update에서 WASD 방향으로 회전을 시도하는데,
    // 여기서도 Update로 돌리면 서로 싸워서 캐릭터가 덜덜 떨림.
    // LateUpdate에서 마지막으로 덮어씌워야 카메라 방향을 완벽하게 봅니다.
    void LateUpdate()
    {
        if (IsShielding)
        {
            RotatePlayerToCamera();
        }
    }

    void HandleAiming()
    {
        if (Input.GetMouseButton(1) && controller.isGrounded)
        {
            animator.SetBool("Shield", true);
            RotatePlayerToCamera();
        }
        else
        {
            // 우클릭을 뗐거나 || 공중에 떴다면 조준 해제
            animator.SetBool("Shield", false);

            // 조준이 풀리면 차징도 취소
            isCharging = false;
            currentChargeTime = 0f;
        }
    }

    void HandleShooting()
    {
        if (!IsShielding) return;

        if (Input.GetMouseButton(0) && !isCharging)
        {
            if (Time.time < lastFireTime + attackCooldown) return;

            isCharging = true;
            currentChargeTime = 0f;
        }

        if (isCharging)
        {
            if (Input.GetMouseButton(0))
            {
                currentChargeTime += Time.deltaTime;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                StartCoroutine(FireProcess());
                isCharging = false;
            }
        }
    }

    IEnumerator FireProcess()
    {
        lastFireTime = Time.time;
        animator.SetTrigger("Attack1");

        yield return new WaitForSeconds(fireDelay);

        float chargeRate = Mathf.Clamp01(currentChargeTime / maxChargeTime);
        float finalSpeed = Mathf.Lerp(minForce, maxForce, chargeRate);

        if (arrowPrefab != null && firePoint != null)
        {
            GameObject arrowObj = Instantiate(arrowPrefab, firePoint.position, firePoint.rotation);
            P_Arrow arrowScript = arrowObj.GetComponent<P_Arrow>(); // P_Arrow인지 P_Projectile인지 이름 확인!

            if (arrowScript != null)
            {
                arrowScript.Init(finalSpeed);
            }
        }

        // Debug.Log($"발사! 차징: {currentChargeTime:F2}s, 속도: {finalSpeed:F2}");
        currentChargeTime = 0f;
    }

    void RotatePlayerToCamera()
    {
        if (Camera.main == null) return;

        Vector3 camForward = Camera.main.transform.forward;
        camForward.y = 0f;
        camForward.Normalize();

        if (camForward.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(camForward);
            // LateUpdate에서는 Time.deltaTime을 써도 되고, 더 즉각적인 반응을 원하면 보간 없이 바로 넣어도 됨
            // 여기서는 부드럽게 유지
            owner.transform.rotation = Quaternion.Slerp(owner.transform.rotation, targetRotation, Time.deltaTime * aimRotationSpeed);
        }
    }
}