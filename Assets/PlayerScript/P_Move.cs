using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class P_Move : MonoBehaviour
{
    [Header("이동 속도 설정")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float shieldSpeed = 1.5f;
    public float rotationSmooth = 10f;
    private bool canMove = true;

    [Header("점프 설정")]
    public float jumpHeight = 2.2f;
    public float gravity = -9.81f;
    private float verticalVelocity = 0f;
    private bool isJumping = false;
    [Header("점프 물리 보정")]
    public float jumpUpMultiplier = 0.8f;      // 상승 중 중력 보정
    public float fallMultiplier = 2.5f;        // 하강 가속도
    public float lowJumpMultiplier = 3.0f;     // 점프키 떼면 더 빨리 떨어짐
    private bool wasGroundedLastFrame = true;
    private float jumpTime = 0f;

    [Header("파쿠르(등반) 설정")]
    public LayerMask climbLayer;        // 등반 가능한 레이어
    public float climbCheckDistance = 1.0f; // 벽 감지 거리
    public float climbHeightOffset = 1.2f;  // 머리 위에서 아래로 쏠 레이의 높이
    public float climbSpeed = 3f;           // 올라가는 속도
    private bool isClimbing = false;

    public float climbCooldown = 1.0f; // 등반 후 1초간 재등반 금지
    private float lastClimbTime = -10f; // 마지막 등반 시간

    [Header("카메라 참조")]
    public Transform cameraTransform;

    [Header("애니메이션 설정")]
    public Animator animator;

    private CharacterController controller;
    private P_Attack attack;

    [Header("회피 설정")]
    public bool isRolling = false;
    public float rollSpeed = 8f;
    public float rollDuration = 0.5f;

    [Header("무적 설정")]
    public float invincibilityDuration = 0.4f; // 무적 지속 시간
    public bool IsInvincible { get; private set; } = false;

    private P_Player playerStats; // 참조 변수 추가
    public float rollStaminaCost = 20f; // 구르기 스테미너 소모량
    void Start()
    {
        controller = GetComponent<CharacterController>();
        attack = GetComponent<P_Attack>();
        playerStats = GetComponent<P_Player>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (GameStarter.isGameRunning == false) return;
        /*
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        } // 임시
        */

        if (!canMove)
        {
            animator.SetFloat("Speed", 0f);
            return;
        }

        // 회피 중 이동 금지
        if (isRolling)
        {
            animator.SetFloat("Speed", 0f);
            return;
        }

        // 공격 중 이동 금지
        if (attack != null && attack.IsAttacking)
        {
            animator.SetFloat("Speed", 0f);
            return;
        }

        if (isClimbing) return; // 등반 중에는 다른 입력 무시

        // 점프 중에만 등반 체크
        if (!controller.isGrounded && !isRolling)
        {
            if (Time.time > lastClimbTime + climbCooldown)
            {
                CheckForLedge();
                if (isClimbing) return;
            }
        }

        HandleMovement();
        HandleJump();
    }

    // ----------------------------------------
    // 파쿠르 감지 로직
    // ----------------------------------------
    void CheckForLedge()
    {
        Vector3 rayOrigin = transform.position + Vector3.up * 1.0f; // 가슴 높이
        RaycastHit hitWall;

        if (Physics.Raycast(rayOrigin, transform.forward, out hitWall, climbCheckDistance, climbLayer))
        {
            Vector3 downRayOrigin = transform.position + Vector3.up * climbHeightOffset + transform.forward * (climbCheckDistance + 0.2f);
            RaycastHit hitLedge;

            // 땅이 있다면.
            if (Physics.Raycast(downRayOrigin, Vector3.down, out hitLedge, climbHeightOffset, climbLayer))
            {
                // 착지할 곳의 높이가 내 발밑보다는 높아야 함
                if (hitLedge.point.y > transform.position.y + 0.5f)
                {
                    StartCoroutine(ClimbLedge(hitWall.point, hitLedge.point));
                }
            }
        }
    }

    IEnumerator ClimbLedge(Vector3 wallPoint, Vector3 ledgePoint)
    {
        isClimbing = true;
        isJumping = false; // 점프 상태 초기화
        canMove = false;

        lastClimbTime = Time.time;
        animator.SetTrigger("Climb");

        // 물리 연산 끄기
        controller.enabled = false;

        // 올라가기 관련 POS
        Vector3 startPos = transform.position;
        Vector3 targetPos = new Vector3(transform.position.x, ledgePoint.y, transform.position.z);

        float timer = 0f;
        while (timer < 0.5f) // 올라가기
        {
            transform.position = Vector3.Lerp(startPos, targetPos, timer / 0.5f);
            timer += Time.deltaTime;
            yield return null;
        }

        // 앞으로 이동 관련 POS
        startPos = transform.position;
        targetPos = ledgePoint; // 최종 착지 지점

        timer = 0f;
        while (timer < 0.2f)
        {
            transform.position = Vector3.Lerp(startPos, targetPos, timer / 0.2f);
            timer += Time.deltaTime;
            yield return null;
        }

        controller.enabled = true; // 컨트롤러 다시 켜기
        lastClimbTime = Time.time;
        isClimbing = false;
        canMove = true;
        verticalVelocity = 0f; // 낙하 가속도 초기화
    }

    // ----------------------------------------
    // 회피 시작
    // ----------------------------------------
    public void StartRoll(Vector3 dir)
    {
        if (isRolling) return;
        if (playerStats.UseStamina(rollStaminaCost))
        {
            StartCoroutine(RollRoutine(dir));
        }
    }

    IEnumerator RollRoutine(Vector3 dir)
    {
        attack.StopAttackImmediately();
        isRolling = true;

        IsInvincible = true;
        animator.SetTrigger("Roll");

        if (dir.sqrMagnitude < 0.1f)
            dir = transform.forward;

        dir.Normalize();
        transform.rotation = Quaternion.LookRotation(dir);

        float timer = 0f;
        while (timer < rollDuration)
        {
            if (timer > invincibilityDuration && IsInvincible)
            {
                IsInvincible = false;
            }

            if (controller.isGrounded)
            {
                if (verticalVelocity < 0f)
                    verticalVelocity = -2f;
            }
            verticalVelocity += gravity * Time.deltaTime;

            Vector3 velocity = dir * rollSpeed;
            velocity.y = verticalVelocity;

            controller.Move(velocity * Time.deltaTime);

            timer += Time.deltaTime;
            yield return null;
        }

        IsInvincible = false;
        isRolling = false;
        StartCoroutine(RollRecoveryDelay());
    }
    IEnumerator RollRecoveryDelay()
    {
        canMove = false;
        yield return new WaitForSeconds(0.1f);  // 후딜레이
        canMove = true;
    }

    public Vector3 GetCameraRelativeInputDir()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 dir = (camForward * v + camRight * h);

        // 입력 없으면 정면 구르기
        if (dir.sqrMagnitude < 0.1f)
            dir = transform.forward;

        return dir.normalized;
    }

    // ----------------------------------------
    // 점프 처리
    // ----------------------------------------
    void HandleJump()
    {
        if (attack.IsShielding && attack.currentWeapon != null && !attack.currentWeapon.CanMoveWhileAiming)
        { // 활은 점프 금지
            return;
        }

        bool isGroundedNow = controller.isGrounded;

        if (controller.isGrounded)
        {
            if (verticalVelocity < 0f)
                verticalVelocity = -2f;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                isJumping = true;
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                animator.SetTrigger("Jump");
            }
        }

        if (isJumping)
        {
            jumpTime += Time.deltaTime;
        }

        if (verticalVelocity > 0 && Input.GetKey(KeyCode.Space))
        {
            verticalVelocity += gravity * jumpUpMultiplier * Time.deltaTime;
        }
        else if (verticalVelocity > 0 && !Input.GetKey(KeyCode.Space))
        {
            verticalVelocity += gravity * lowJumpMultiplier * Time.deltaTime;
        }
        else if (verticalVelocity < 0)
        {
            verticalVelocity += gravity * fallMultiplier * Time.deltaTime;
        }

        // 마지막: 중력 적용
        verticalVelocity += gravity * Time.deltaTime;

        if (!wasGroundedLastFrame && isGroundedNow)
        {
            isJumping = false;
            if (jumpTime > 0.7f)
            {
                animator.SetTrigger("Landing");  // 더 높은 곳에서의 낙하 → 착지 모션
                StartCoroutine(LandingDelay(0.6f));
            }
            else if (jumpTime > 0.6f)
            {
                animator.SetTrigger("Landing");  // 높은 낙하 → 착지 모션 ( 스페이스바 최대로 눌렀을 시 높이 )
                StartCoroutine(LandingDelay(0.2f));
            }
            else
            {
                StartCoroutine(LandingDelay(0.02f));
            }
        }

        wasGroundedLastFrame = isGroundedNow;
    }

    IEnumerator LandingDelay(float landingDelayTime)
    {
        jumpTime = 0f;
        canMove = false;
        yield return new WaitForSeconds(landingDelayTime);  // 착지 느낌
        canMove = true;
    }


    // ----------------------------------------
    // 일반 이동 처리
    // ----------------------------------------
    void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        if (attack.IsShielding && attack.currentWeapon != null && !attack.currentWeapon.CanMoveWhileAiming)
        {
            h = 0f;
            v = 0f;
        }

        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 inputDir = (camForward * v + camRight * h).normalized;
        bool isMoving = inputDir.magnitude > 0;

        float currentSpeed = walkSpeed;

        if (attack.IsShielding)
        {
            if (attack.currentWeapon != null && attack.currentWeapon.CanMoveWhileAiming)
            {
                currentSpeed = shieldSpeed;
            }
            else
            {
                currentSpeed = 0f;
            }
        }

        // 달리기 처리
        else if (Input.GetKey(KeyCode.LeftShift) && isMoving)
        {
            currentSpeed = runSpeed;
        }

        Vector3 velocity = inputDir * currentSpeed;
        velocity.y = verticalVelocity;

        if(controller.enabled)
        {
            controller.Move(velocity * Time.deltaTime);
        }

        // 회전
        if (isMoving)
        {
            Quaternion targetRot = Quaternion.LookRotation(inputDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSmooth);
        }

        float animSpeed = 0;

        if (attack.IsShielding)
        {
            if (attack.currentWeapon != null && attack.currentWeapon.CanMoveWhileAiming)
                animSpeed = isMoving ? 0.5f : 0f;
            else
                animSpeed = 0f; 
        }
        else
        {
            if (currentSpeed == runSpeed) animSpeed = 2f;
            else if (isMoving) animSpeed = 1f;
        }

        animator.SetFloat("Speed", animSpeed);
    }
}
