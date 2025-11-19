using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class P_Move : MonoBehaviour
{
    [Header("이동 속도 설정")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float crouchSpeed = 1.5f;
    public float rotationSmooth = 10f;
    private bool canMove = true;

    [Header("점프 설정")]
    public float jumpHeight = 2.2f;
    public float gravity = -9.81f;
    private float verticalVelocity = 0f;
    [Header("점프 물리 보정")]
    public float jumpUpMultiplier = 0.8f;      // 상승 중 중력 보정
    public float fallMultiplier = 2.5f;        // 하강 가속도 (더 무거워짐)
    public float lowJumpMultiplier = 3.0f;     // 점프키 떼면 더 빨리 떨어짐
    private bool wasGroundedLastFrame = true;
    
    [Header("웅크리기 설정")]
    public float crouchHeight = 1.0f;
    private float originalHeight;
    private bool isCrouching = false;

    [Header("카메라 참조")]
    public Transform cameraTransform;

    [Header("애니메이션 설정")]
    public Animator animator;

    private CharacterController controller;
    private P_Attack attack;

    // 회피
    public bool isRolling = false;
    public float rollSpeed = 8f;
    public float rollDuration = 0.5f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        attack = GetComponent<P_Attack>();
        originalHeight = controller.height;

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

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

        // 방어 중 이동 금지
        if (attack != null && attack.IsShielding)
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

        HandleCrouch();
        HandleMovement();
        HandleJump();
    }

    // ----------------------------------------
    // 회피 시작
    // ----------------------------------------
    public void StartRoll(Vector3 dir)
    {
        if (isRolling) return;
        StartCoroutine(RollRoutine(dir));
    }

    IEnumerator RollRoutine(Vector3 dir)
    {
        attack.StopAttackImmediately();
        isRolling = true;
        animator.SetTrigger("Roll");

        if (dir.sqrMagnitude < 0.1f)
            dir = transform.forward;

        dir.Normalize();
        transform.rotation = Quaternion.LookRotation(dir);

        float timer = 0f;
        while (timer < rollDuration)
        {
            // 🔥 중력 적용
            if (controller.isGrounded)
            {
                if (verticalVelocity < 0f)
                    verticalVelocity = -2f;
            }
            verticalVelocity += gravity * Time.deltaTime;

            // 🔥 이동 + 중력 통합
            Vector3 velocity = dir * rollSpeed;
            velocity.y = verticalVelocity;

            controller.Move(velocity * Time.deltaTime);

            timer += Time.deltaTime;
            yield return null;
        }

        isRolling = false;
        StartCoroutine(RollRecoveryDelay());
    }
    IEnumerator RollRecoveryDelay()
    {
        canMove = false;
        yield return new WaitForSeconds(0.1f);  // 원하는 후딜 시간
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
        bool isGroundedNow = controller.isGrounded;

        if (controller.isGrounded)
        {
            if (verticalVelocity < 0f)
                verticalVelocity = -2f;

            if (Input.GetKeyDown(KeyCode.Space) && !isCrouching)
            {
                // 현실적인 점프 높이
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                animator.SetTrigger("Jump");
            }
        }

        // ✨ 상승 중 (Key 유지 시)
        if (verticalVelocity > 0 && Input.GetKey(KeyCode.Space))
        {
            verticalVelocity += gravity * jumpUpMultiplier * Time.deltaTime;
        }
        // ✨ Space 떼면 더 빨리 떨어짐 (짧은 점프 가능)
        else if (verticalVelocity > 0 && !Input.GetKey(KeyCode.Space))
        {
            verticalVelocity += gravity * lowJumpMultiplier * Time.deltaTime;
        }
        // ✨ 하강 중 (중력 강화)
        else if (verticalVelocity < 0)
        {
            verticalVelocity += gravity * fallMultiplier * Time.deltaTime;
        }

        // 마지막: 중력 적용
        verticalVelocity += gravity * Time.deltaTime;

        if (!wasGroundedLastFrame && isGroundedNow)
        {
            // 🔥 착지 강도에 따른 조건 분기
            if (verticalVelocity < -5f)
            {
                animator.SetTrigger("Landing");  // 높은 낙하 → 착지 모션
                StartCoroutine(LandingDelay(0.2f));
            }
            else
            {
                StartCoroutine(LandingDelay(0.05f));
            }
        }

        wasGroundedLastFrame = isGroundedNow;
    }

    IEnumerator LandingDelay(float landingDelayTime)
    {
        canMove = false;
        yield return new WaitForSeconds(landingDelayTime);  // 착지 느낌용 텀
        canMove = true;
    }

    // ----------------------------------------
    // 웅크리기 처리
    // ----------------------------------------
    void HandleCrouch()
    {
        // 왼쪽 컨트롤 키를 누르고 있으면 crouch, 떼면 stand
        if (Input.GetKey(KeyCode.LeftControl))
        {
            // 웅크리기 ON
            if (!isCrouching)
            {
                isCrouching = true;
                controller.height = crouchHeight;
                animator.SetBool("Crouch", true);
            }
        }
        else
        {
            // 웅크리기 OFF
            if (isCrouching)
            {
                isCrouching = false;
                controller.height = originalHeight;
                animator.SetBool("Crouch", false);
            }
        }
    }


    // ----------------------------------------
    // 일반 이동 처리
    // ----------------------------------------
    void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 inputDir = (camForward * v + camRight * h).normalized;
        bool isMoving = inputDir.magnitude > 0;

        float currentSpeed = walkSpeed;

        // Shift + 이동 = 달리기
        if (Input.GetKey(KeyCode.LeftShift) && isMoving && !isCrouching)
            currentSpeed = runSpeed;
        else if (isCrouching)
            currentSpeed = crouchSpeed;

        Vector3 velocity = inputDir * currentSpeed;
        velocity.y = verticalVelocity;
        controller.Move(velocity * Time.deltaTime);

        // 회전
        if (isMoving)
        {
            Quaternion targetRot = Quaternion.LookRotation(inputDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSmooth);
        }

        // Speed 애니메이션
        float animSpeed = 0;

        if (isCrouching)
            animSpeed = isMoving ? 0.5f : 0f;
        else if (currentSpeed == runSpeed)
            animSpeed = 2f;
        else if (isMoving)
            animSpeed = 1f;

        animator.SetFloat("Speed", animSpeed);
    }
}
