    using UnityEngine;
    using System.Collections;

    public class W_Sword : W_Base

    {

    private int comboIndex = 0;

    private bool isAttacking = false;

    private bool canCombo = false;

    private bool comboQueued = false;

    private float comboResetTime = 0.9f;



    private bool rollQueued = false;



    private readonly string[] comboTriggers = { "Attack1", "Attack2", "Attack3" };

    public float[] comboMinTimes = { 0.6f, 1.0f, 1.3f };

    public float[] comboDurations = { 1.2f, 2.6f, 2.7f };

    public int[] comboDamages = { 10, 15, 25 };

    private Coroutine routine;



    public override bool IsAttacking => isAttacking;



    [Header("Defense Setting")]

    [Range(0f, 1f)]

    public float shieldGuardRate = 0.7f;

    public override float GetDamageReductionRate()

    {

        // IsShielding은 W_Base에 정의되어 있고, W_Sword에서 로직이 구현되어 있으므로 바로 사용 가능

        if (IsShielding)

        {

            return shieldGuardRate; // 우클릭(실드) 중이면 0.5 반환

        }



        return 0f; // 아니면 0 반환

    }



    public override bool IsShielding

    {

        get

        {

            return animator.GetBool("Shield");

        }

    }



    public override void Init(P_Attack owner)

    {

        base.Init(owner);

    }


    // ------------------------------

    //   메인 공격 진입 함수

    // ------------------------------

    public override void Attack()

    {

        if (!controller.isGrounded) return;

        if (move.isRolling) return;

        if (IsShielding) return;



        if (isAttacking)

        {

            if (!canCombo)

            {

                comboQueued = true;

                return;

            }



            canCombo = false;

            comboQueued = false;



            comboIndex = Mathf.Min(comboIndex + 1, comboTriggers.Length - 1);



            if (routine != null)

                owner.StopCoroutine(routine);



            routine = owner.StartCoroutine(DoAttack());

            return;

        }



        comboIndex = 0;

        comboQueued = false;



        if (routine != null)

            owner.StopCoroutine(routine);



        routine = owner.StartCoroutine(DoAttack());

    }







    // ------------------------------

    //    공격 실행 코루틴

    // ------------------------------

    IEnumerator DoAttack()

    {

        isAttacking = true;

        canCombo = false;

        comboQueued = false;

        rollQueued = false;



        animator.SetTrigger(comboTriggers[comboIndex]);



        float minTime = comboMinTimes[comboIndex];

        float duration = comboDurations[comboIndex];

        float timer = 0f;



        // 1) 최소 시간 동안 콤보/롤 금지

        while (timer < minTime)

        {

            if (RollInput(out Vector3 dir))

                rollQueued = true;



            timer += Time.deltaTime;

            yield return null;

        }



        // 최소 시간 직후 롤 예약 처리

        if (rollQueued)

        {

            rollQueued = false;

            StopImmediately();

            move.StartRoll(move.GetCameraRelativeInputDir());

            yield break;

        }



        // 2) 콤보 입력 가능 시간

        canCombo = true;

        timer = 0f;



        while (timer < comboResetTime)

        {

            if (RollInput(out Vector3 dir))

            {

                StopImmediately();

                move.StartRoll(dir);

                yield break;

            }



            if (comboQueued)

            {

                comboQueued = false;

                canCombo = false;

                comboIndex = Mathf.Min(comboIndex + 1, comboTriggers.Length - 1);



                routine = owner.StartCoroutine(DoAttack());

                yield break;

            }



            timer += Time.deltaTime;

            yield return null;

        }



        // 3) 애니 끝날 때까지 대기

        float remain = duration - minTime - 1.5f;

        if (comboIndex == 0)

            remain -= 0.5f;



        if (remain > 0)

            yield return new WaitForSeconds(remain);



        isAttacking = false;

    }







    // ---------------------------------------

    // 롤 입력 처리 (공격 중에 사용됨)

    // ---------------------------------------

    bool RollInput(out Vector3 dir)

    {

        float h = Input.GetAxisRaw("Horizontal");

        float v = Input.GetAxisRaw("Vertical");



        Vector3 camForward = move.cameraTransform.forward;

        Vector3 camRight = move.cameraTransform.right;



        camForward.y = 0;

        camRight.y = 0;



        camForward.Normalize();

        camRight.Normalize();



        dir = (camForward * v + camRight * h).normalized;



        return dir.sqrMagnitude > 0.1f && Input.GetKeyDown(KeyCode.Space);

    }







    // ---------------------------------------

    // 공격 즉시 종료

    // ---------------------------------------

    public override void StopImmediately()

    {

        if (routine != null)

            owner.StopCoroutine(routine);



        isAttacking = false;

        canCombo = false;

        comboQueued = false;

        comboIndex = 0;

    }





    // ---------------------------------------

    // 이 무기만의 Update (실드 처리 등)

    // ---------------------------------------

    public override void OnUpdate()

    {

        HandleShield();

    }





    void HandleShield()

    {

        if (!controller.isGrounded || isAttacking)

            return;



        if (Input.GetMouseButton(1))

        {

            animator.SetBool("Shield", true);

            StopImmediately();



            if (RollInput(out Vector3 dir))

            {

                animator.SetBool("Shield", false);

                move.StartRoll(dir);

            }

            return;

        }



        // 우클릭 떼면 실드 해제

        if (animator.GetBool("Shield") && Input.GetMouseButtonUp(1))

            animator.SetBool("Shield", false);

    }

    }