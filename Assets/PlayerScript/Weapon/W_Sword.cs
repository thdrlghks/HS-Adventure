using UnityEngine;
using System.Collections;

public class W_Sword : W_Base
{
    public float shieldStaminaCost = 5f;

    private int comboIndex = 0;
    private bool isAttacking = false;
    private bool comboQueued = false;
    private bool rollQueued = false;

    private readonly string[] comboTriggers = { "Attack1", "Attack2", "Attack3" };

    public bool _isAttacking => isAttacking;
    public GameObject myShieldObject;


    [Header("Combo Timing Settings")]
    // 1. 공격 발생 후 이 시간까지는 구르기 불가 (예약만 됨)
    public float[] comboMinTimes = { 0.6f, 1.0f, 1.3f };

    // 2. 애니메이션 전체 길이
    public float[] comboDurations = { 1.2f, 2.6f, 2.7f };

    // 3. 🔥 [추가] 애니메이션 끝자락의 '조작 불가' 후딜레이 시간
    // 값이 클수록 공격 후 멍때리는 시간이 길어져 난이도가 올라감
    public float[] comboRecoveryTimes = { 0.2f, 0.4f, 1.0f };

    public int[] comboDamages = { 10, 15, 25 };

    private Coroutine routine;

    [Header("Attack Hit Settings")]
    public float attackRange = 1.0f;
    public float attackRadius = 1.0f;
    public float[] hitDelays = { 0.2f, 0.4f, 0.4f };
    public LayerMask enemyLayer;

    public override bool IsAttacking => isAttacking;

    [Header("Defense Setting")]
    [Range(0f, 1f)] public float shieldGuardRate = 0.7f;
    public override float GetDamageReductionRate() { return IsShielding ? shieldGuardRate : 0f; }
    public override bool IsShielding => animator.GetBool("Shield");
    public override void Init(P_Attack owner) { base.Init(owner); }

    private Vector3 debugHitPos;
    private float debugHitRadius;
    private bool showDebugHit = false;

    public override void OnEquip()
    {
        base.OnEquip(); // 애니메이터 교체
        if (myShieldObject != null) myShieldObject.SetActive(true); // 방패 켜기
    }

    public override void OnUnequip()
    {
        base.OnUnequip();
        if (myShieldObject != null) myShieldObject.SetActive(false); // 방패 끄기
    }

    // --------------------------------------------------------
    // [1] 공격 입력 처리
    // --------------------------------------------------------
    public override void Attack()
    {
        if (!controller.isGrounded) return;
        if (move.isRolling) return;
        if (IsShielding) return;

        if (isAttacking)
        {
            if (comboIndex >= comboTriggers.Length - 1) return;
            comboQueued = true; // 언제든 선입력은 받음 (단, 실행은 코루틴이 결정)
        }
        else
        {
            comboIndex = 0;
            comboQueued = false;
            StartAttackRoutine();
        }
    }

    void ExecuteNextCombo()
    {
        comboQueued = false;
        comboIndex = Mathf.Min(comboIndex + 1, comboTriggers.Length - 1);
        StartAttackRoutine();
    }

    void StartAttackRoutine()
    {
        if (routine != null) owner.StopCoroutine(routine);
        routine = owner.StartCoroutine(DoAttack());
    }

    // --------------------------------------------------------
    // [2] 공격 실행 코루틴 (3단계 구간 적용)
    // --------------------------------------------------------
    IEnumerator DoAttack()
    {
        isAttacking = true;
        rollQueued = false; // 새 공격 시작 시 구르기 예약 초기화

        animator.SetTrigger(comboTriggers[comboIndex]);
        float currentDelay = (comboIndex < hitDelays.Length) ? hitDelays[comboIndex] : 0.1f;
        Invoke("CheckHit", currentDelay);

        float minTime = comboMinTimes[comboIndex];
        float duration = comboDurations[comboIndex];
        float recoveryTime = comboRecoveryTimes[comboIndex];

        // 캔슬 가능 마지노선 시간 (전체 길이 - 후딜레이)
        float cancelEndTime = duration - recoveryTime;

        float timer = 0f;
        bool isLastHit = (comboIndex >= comboTriggers.Length - 1);

        // =================================================================
        // [구간 1] 선딜레이/공격 중 (0 ~ minTime)
        // 특징: 구르기 실행 불가, 예약(Queue)만 가능
        // =================================================================
        while (timer < minTime)
        {
            // 구르기 키를 누르면? -> 예약만 해둠 (실행 X)
            if (RollInput(out Vector3 dir))
            {
                rollQueued = true;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        // 구간 1이 끝나자마자, "예약된 구르기"가 있는지 확인
        if (rollQueued)
        {
            CancelAttackAndRoll(move.GetCameraRelativeInputDir());
            yield break;
        }

        // =================================================================
        // [구간 2] 캔슬 가능 구간 (minTime ~ cancelEndTime)
        // 특징: 자유롭게 구르기 캔슬 및 콤보 연계 가능 (유저가 반응해야 할 타이밍)
        // =================================================================
        while (timer < cancelEndTime)
        {
            // 1. 구르기 즉시 발동
            if (RollInput(out Vector3 dir))
            {
                CancelAttackAndRoll(dir);
                yield break;
            }

            // 2. 콤보 연계
            if (!isLastHit && comboQueued)
            {
                ExecuteNextCombo();
                yield break;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        // =================================================================
        // [구간 3] 후딜레이/경직 구간 (cancelEndTime ~ duration)
        // 특징: 키 입력 완전 무시 (패널티 시간)
        // =================================================================

        // 시각적 힌트를 위해 필요하다면 여기서 검 색깔을 바꾸거나 효과를 줄 수 있음

        while (timer < duration)
        {
            // 🔥 여기서는 RollInput이나 comboQueued를 체크하지 않음!
            // 즉, 아무리 키를 눌러도 캐릭터는 멍하니 서 있어야 함.

            // (선택사항) 여기서 입력하면 "틱" 소리 같은 걸 내서 '안 눌린다'는 피드백을 줄 수도 있음.

            timer += Time.deltaTime;
            yield return null;
        }

        // 종료
        yield return null;
        isAttacking = false;
        comboIndex = 0;
        comboQueued = false;
    }

    // --------------------------------------------------------
    // [3] 기타 함수들
    // --------------------------------------------------------
    void CancelAttackAndRoll(Vector3 dir)
    {
        StopImmediately();
        move.StartRoll(dir);
    }

    public override void StopImmediately()
    {
        if (routine != null) owner.StopCoroutine(routine);
        CancelInvoke("CheckHit");

        isAttacking = false;
        comboQueued = false;
        rollQueued = false;
        comboIndex = 0;
    }

    // ... (나머지 CheckHit, Gizmos, RollInput 등은 기존 유지) ...
    void CheckHit()
    {
        // 1. 공격 위치 계산
        Vector3 attackPos = transform.position + transform.forward * attackRange;

        // 디버깅용 정보 저장 (Gizmo에서 그리기 위함)
        debugHitPos = attackPos;
        debugHitRadius = attackRadius;
        showDebugHit = true;
        // 0.5초 뒤에 디버그 그림 끄기
        Invoke("DisableDebugView", 0.5f);

        // 2. 범위 내 적 검출
        Collider[] enemies = Physics.OverlapSphere(attackPos, attackRadius, enemyLayer);

        if (enemies.Length == 0)
        {
            Debug.Log("판정 범위 내에 적(Collider)이 없음!");
        }

        foreach (Collider enemy in enemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                Debug.Log($"🎯 적중! : {enemy.name}");
                // 데미지 처리...
            }
            else
            {
                Debug.Log($"⚠️ 범위엔 들어왔는데 태그가 Enemy가 아님: {enemy.name} (현재 태그: {enemy.tag})");
            }
        }
    }

    void OnDrawGizmos()
    {
        // 평소 설정값 미리보기 (흰색)
        Gizmos.color = Color.white;
        Vector3 previewPos = transform.position + transform.forward * attackRange;
        Gizmos.DrawWireSphere(previewPos, attackRadius);

        // 🔥 공격 순간 실제로 때린 위치 (빨간색 - 채워진 원)
        if (showDebugHit)
        {
            Gizmos.color = new Color(1, 0, 0, 0.5f); // 반투명 빨강
            Gizmos.DrawSphere(debugHitPos, debugHitRadius);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 attackPos = transform.position + transform.forward * attackRange;
        Gizmos.DrawWireSphere(attackPos, attackRadius);
    }

    bool RollInput(out Vector3 dir)
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 camForward = move.cameraTransform.forward;
        Vector3 camRight = move.cameraTransform.right;
        camForward.y = 0; camRight.y = 0;
        camForward.Normalize(); camRight.Normalize();
        dir = (camForward * v + camRight * h).normalized;
        return dir.sqrMagnitude > 0.1f && Input.GetKeyDown(KeyCode.Space);
    }

    public override void OnUpdate() { HandleShield(); }
    void HandleShield()
    {
        if (!controller.isGrounded || isAttacking) return;
        if (Input.GetMouseButton(1))
        {
            bool hasStamina = playerStats.UseStaminaContinuous(shieldStaminaCost);

            if (hasStamina && !IsShielding)
            {
                animator.SetBool("Shield", true);
                StopImmediately();
            }
            // 스테미너가 다 떨어지면 -> 강제 해제
            else if (!hasStamina && IsShielding)
            {
                animator.SetBool("Shield", false);
            }

            if (RollInput(out Vector3 dir))
            {
                animator.SetBool("Shield", false);
                move.StartRoll(dir);
            }
            return;
        }
        if (animator.GetBool("Shield") && Input.GetMouseButtonUp(1))
            animator.SetBool("Shield", false);
    }
}