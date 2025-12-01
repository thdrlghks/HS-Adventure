using UnityEngine;
using System.Collections;

public class W_Bow : W_Base
{
    public float chargeStaminaCost = 15f;

    [Header("Bow Settings")]
    public GameObject arrowPrefab;
    public Transform firePoint;
    public float fireDelay = 0.1f;

    [Header("Charging Settings")]
    public float minForce = 15f;
    public float maxForce = 40f;
    public float maxChargeTime = 1.0f;

    [Header("Cooldown Settings")]
    public float attackCooldown = 1.0f;
    private float lastFireTime = -10f;

    private float currentChargeTime = 0f;
    private bool isCharging = false;

    public float aimRotationSpeed = 20f;

    private float originalShieldSpeed;

    public override bool IsShielding => animator.GetBool("Shield");

    public override bool CanMoveWhileAiming => false;
    public override void Attack() { }

    void Start()
    {
        if (move != null)
        {
            // 원래 쉴드 속도 저장
            originalShieldSpeed = move.shieldSpeed;
            // 조준 시 속도 0
            move.shieldSpeed = 0f;
        }
    }

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
            animator.SetBool("Shield", false);

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
            bool hasStamina = playerStats.UseStaminaContinuous(chargeStaminaCost);

            if (Input.GetMouseButton(0) && hasStamina)
            {
                currentChargeTime += Time.deltaTime;
            }
            else if (Input.GetMouseButtonUp(0) || !hasStamina)
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
            P_Arrow arrowScript = arrowObj.GetComponent<P_Arrow>();

            if (arrowScript != null)
            {
                arrowScript.Init(finalSpeed);
            }
        }

        // Debug.Log($"발사! 차징: {currentChargeTime:F2}s, 속도: {finalSpeed:F2}");
        currentChargeTime = 0f;
    }

    void RotatePlayerToCamera()
    { // 카메라를 따라 플레이어 회전
        if (Camera.main == null) return;

        Vector3 camForward = Camera.main.transform.forward;
        camForward.y = 0f;
        camForward.Normalize();

        if (camForward.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(camForward);
            owner.transform.rotation = Quaternion.Slerp(owner.transform.rotation, targetRotation, Time.deltaTime * aimRotationSpeed);
        }
    }
}