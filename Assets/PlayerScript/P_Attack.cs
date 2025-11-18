using UnityEngine;
using System.Collections;

public class P_Attack : MonoBehaviour
{
    [Header("공격 설정")]
    public float attackTurnSpeed = 10f;
    public float comboResetTime = 0.9f;

    private Animator animator;
    private CharacterController characterController;
    private P_Move move;

    private bool isAttacking = false;
    public bool IsAttacking => isAttacking;

    private int currentCombo = 0;
    private bool canCombo = false;
    private bool comboQueued = false;

    private readonly string[] comboTriggers = { "Attack1", "Attack2", "Attack3" };
    public float[] comboMinTimes = { 0.6f, 1.0f, 1.3f };

    [Header("공격 전체 애니메이션 길이(초) - 자동 설정됨")]
    public float[] comboDurations = new float[3];
    private readonly string[] comboClipNames = { "Attack1", "Attack2", "Attack3" };

    private Quaternion targetRot;
    private Coroutine attackRoutine = null;

    private bool isShielding = false;
    public bool IsShielding => isShielding;

    private bool rollQueued = false;    

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        characterController = GetComponent<CharacterController>();
        move = GetComponent<P_Move>();

        LoadAnimationClipLengths();
    }

    void Update()
    {
        if (move != null && move.isRolling)
            return;

        HandleShield();  

        if (isShielding)
        {
            RotateToCameraDirection();
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (!characterController.isGrounded) return;
            HandleAttackInput();
        }

        /*if (isAttacking && currentCombo == 0)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * attackTurnSpeed);
        }*/
    }

    // ============================================
    // 🔥 빌드에서도 작동하는: 애니메이션 길이 로드
    // ============================================
    void LoadAnimationClipLengths()
    {
        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;

        for (int i = 0; i < comboClipNames.Length; i++)
        {
            string name = comboClipNames[i];

            foreach (var clip in clips)
            {
                if (clip.name == name)
                {
                    comboDurations[i] = clip.length;
                    Debug.Log($"[{name}] 애니메이션 길이 = {clip.length}초");
                    break;
                }
            }
        }
    }

    // --------------------------------------------
    // 회피 입력 (방향키 + Space)
    // --------------------------------------------
    bool RollInput(out Vector3 dir)
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 camForward = move.cameraTransform.forward;
        Vector3 camRight = move.cameraTransform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        dir = (camForward * v + camRight * h).normalized;

        return dir.sqrMagnitude > 0.1f && Input.GetKeyDown(KeyCode.Space);
    }

    // --------------------------------------------
    // 방어 입력 처리
    // --------------------------------------------
    void HandleShield()
    {
        // 점프 중 방패 금지
        if (!characterController.isGrounded || isAttacking)
        {
            if (isShielding)
            {
                isShielding = false;
                animator.SetBool("Shield", false);
            }
            return;
        }

        // 우클릭 유지 중
        if (Input.GetMouseButton(1))
        {
            if (!isShielding)
            {
                isShielding = true;
                animator.SetBool("Shield", true);
                StopAttackImmediately();
            }

            // ⭐ 롤 입력 체크 (Shield 중에도 가능하게!)
            if (RollInput(out Vector3 dir))
            {
                isShielding = false;
                animator.SetBool("Shield", false);
                move.StartRoll(dir);
                return;
            }

            // Shield 유지 중에는 카메라 방향으로 회전
            RotateToCameraDirection();
            return;
        }

        // 우클릭 떼면 Shield 해제
        if (isShielding && Input.GetMouseButtonUp(1))
        {
            isShielding = false;
            animator.SetBool("Shield", false);
        }
    }

    void RotateToCameraDirection()
    {
        if (move == null || move.cameraTransform == null) return;

        Vector3 camDir = move.cameraTransform.forward;
        camDir.y = 0f;

        if (camDir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(camDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
        }
    }

    // --------------------------------------------
    // 공격 입력 처리
    // --------------------------------------------
    void HandleAttackInput()
    {
        if (currentCombo == comboTriggers.Length - 1 && isAttacking)
            return;

        if (isAttacking && !canCombo)
        {
            comboQueued = true;
            return;
        }

        if (isAttacking && canCombo)
        {
            canCombo = false;
            comboQueued = false;
            currentCombo = Mathf.Min(currentCombo + 1, comboTriggers.Length - 1);

            if (attackRoutine != null)
                StopCoroutine(attackRoutine);

            attackRoutine = StartCoroutine(PerformAttack());
            return;
        }

        if (!isAttacking)
        {
            currentCombo = 0;
            comboQueued = false;

            if (attackRoutine != null)
                StopCoroutine(attackRoutine);

            attackRoutine = StartCoroutine(PerformAttack());
        }
    }

    // --------------------------------------------
    // 첫타 회전 계산
    // --------------------------------------------
    void CalculateFirstHitTurn()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 200f))
        {
            Vector3 dir = hit.point - transform.position;
            dir.y = 0;

            if (dir.sqrMagnitude > 0.1f)
                targetRot = Quaternion.LookRotation(dir);
        }
    }

    // --------------------------------------------
    // 공격 실행
    // --------------------------------------------
    IEnumerator PerformAttack()
    {
        isAttacking = true;
        comboQueued = false;
        canCombo = false;
        rollQueued = false;         // 🔥 새 공격마다 초기화

        if (currentCombo == 0)
            CalculateFirstHitTurn();

        animator.SetTrigger(comboTriggers[currentCombo]);

        float minTime = comboMinTimes[currentCombo];
        float duration = comboDurations[currentCombo];
        float timer = 0f;

        // ---------------------------------------------
        // 1) 최소 재생 시간동안 콤보/롤 모두 금지
        // ---------------------------------------------
        while (timer < minTime)
        {
            // 🔥 최소 시간 이전에는 롤 입력을 "예약"만 함
            if (RollInput(out Vector3 dir))
            {
                rollQueued = true;  // 롤 예약
            }

            timer += Time.deltaTime;
            yield return null;
        }

        // 🔥 최소 시간 끝난 직후 자동 롤 처리
        if (rollQueued)
        {
            rollQueued = false;
            StopAttackImmediately();
            move.StartRoll(move.GetCameraRelativeInputDir()); // 🔥 방향 계산 함수 필요
            yield break;
        }

        // ---------------------------------------------
        // 2) 콤보 입력 단계
        // ---------------------------------------------
        canCombo = true;
        timer = 0f;

        while (timer < comboResetTime)
        {
            // 🔥 최소 시간 이후에는 바로 구르기 가능
            if (RollInput(out Vector3 dir))
            {
                StopAttackImmediately();
                move.StartRoll(dir);
                yield break;
            }

            // 콤보 예약 처리
            if (comboQueued)
            {
                comboQueued = false;
                canCombo = false;
                currentCombo = Mathf.Min(currentCombo + 1, comboTriggers.Length - 1);

                attackRoutine = StartCoroutine(PerformAttack());
                yield break;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        if (RollInput(out Vector3 direc))
        {
            StopAttackImmediately();
            move.StartRoll(direc);
            yield break;
        }
        // ---------------------------------------------
        // 3) 콤보가 종료된 후 전체 애니메이션 재생 대기
        // ---------------------------------------------
        canCombo = false;
        comboQueued = false;
        float remain = duration - minTime - 1.5f;
        if (currentCombo == 0)
            remain -= 0.5f;
        if (remain > 0)
            yield return new WaitForSeconds(remain);

        isAttacking = false;
    }


    // --------------------------------------------
    // 공격 즉시 종료
    // --------------------------------------------
    public void StopAttackImmediately()
    {
        if (attackRoutine != null)
            StopCoroutine(attackRoutine);

        isAttacking = false;
        canCombo = false;
        comboQueued = false;
        currentCombo = 0;
    }
}
