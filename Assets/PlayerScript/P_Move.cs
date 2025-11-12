using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class P_Move : MonoBehaviour
{
    [Header("이동 속도 설정")]
    public float moveSpeed = 6f;
    public float rotationSmooth = 10f;

    [Header("카메라 참조")]
    public Transform cameraTransform;

    [Header("애니메이션 설정")]
    public Animator animator;

    private CharacterController controller;
    private float gravity = -9.81f;
    private float verticalVelocity = 0f;
    private P_Attack attack; // 공격 스크립트 참조

    void Start()
    {
        controller = GetComponent<CharacterController>();
        attack = GetComponent<P_Attack>();

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

        // ⚠️ 공격 중에는 이동 완전 금지
        if (attack != null && attack.IsAttacking)
        {
            animator.SetFloat("Speed", 0f);
            return;
        }

        HandleMovement();
    }

    void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // 카메라 기준 방향
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 inputDir = (camForward * v + camRight * h).normalized;
        bool isMoving = inputDir.magnitude > 0;

        // 중력 처리
        if (controller.isGrounded)
            verticalVelocity = -1f;
        else
            verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = inputDir * moveSpeed;
        velocity.y = verticalVelocity;

        controller.Move(velocity * Time.deltaTime);

        if (isMoving)
        {
            Quaternion targetRot = Quaternion.LookRotation(inputDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSmooth);
        }

        animator.SetFloat("Speed", isMoving ? 1f : 0f);
    }
}
