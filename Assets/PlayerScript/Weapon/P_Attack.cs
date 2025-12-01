using UnityEngine;
using System.Collections;
public enum WeaponType
{
    Sword,
    Bow,
    Bomb,
}

public class P_Attack : MonoBehaviour
{
    private Animator animator;
    private CharacterController characterController;
    private P_Move move;

    public W_Base[] weapons;
    public W_Base currentWeapon;

    private bool isSwapping = false;

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        characterController = GetComponent<CharacterController>();
        move = GetComponent<P_Move>();
    }

    void Start()
    {
        foreach (var w in weapons)
        {
            w.Init(this);
            w.OnUnequip();
            w.gameObject.SetActive(false);
        }

        EquipImmediate(WeaponType.Sword);
    }

    void Update()
    {

        // 1. 교체 중이거나 구르는 중이면 -> 아무것도 못함 (완전 차단)
        if (isSwapping || (move != null && move.isRolling))
            return;

        // 2. 공격 중이 '아닐 때만' 무기 교체 허용
        if (!IsAttacking)
        {
            HandleWeaponSwitch();
        }

        if (currentWeapon != null)
        {
            currentWeapon.OnUpdate();

            if (Input.GetMouseButtonDown(0))
            {
                currentWeapon.Attack();
            }
        }
    }

    // ======================================
    // 숫자키로 무기 교체
    // ======================================
    void HandleWeaponSwitch()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) StartCoroutine(SwapWeaponRoutine(WeaponType.Sword));
        else if (Input.GetKeyDown(KeyCode.Alpha2)) StartCoroutine(SwapWeaponRoutine(WeaponType.Bow));
        else if (Input.GetKeyDown(KeyCode.Alpha3)) StartCoroutine(SwapWeaponRoutine(WeaponType.Bomb));
    }

    IEnumerator SwapWeaponRoutine(WeaponType newType)
    {
        // 이미 같은 무기면 패스
        if (currentWeapon == weapons[(int)newType]) yield break;

        isSwapping = true;

        // 1. 기존 무기 넣기
        if (currentWeapon != null)
        {
            // 무기 넣는 애니메이션
            if (currentWeapon == weapons[1])
            {
                animator.SetTrigger("Equip");
            }
            else
            {
                animator.SetTrigger("Unequip");
            }
            // 넣는 모션 시간만큼 대기
            yield return new WaitForSeconds(0.3f);

            currentWeapon.StopImmediately();

            // 방패
            currentWeapon.OnUnequip(); 

            currentWeapon.gameObject.SetActive(false);
        }

        // 2. 새 무기 교체
        currentWeapon = weapons[(int)newType];

        if (currentWeapon != null)
        {
            currentWeapon.gameObject.SetActive(true);

            // 새 무기 장착 함수
            currentWeapon.OnEquip();

            // 무기 꺼내는 애니메이션
            if (currentWeapon == weapons[1])
            {
                animator.SetTrigger("Unequip");
            }
            else
            {
                animator.SetTrigger("Equip");
            }

            // 꺼내는 딜레이
            yield return new WaitForSeconds(0.2f);
        }
        isSwapping = false;

        Debug.Log($"무기 교체 완료: {newType}");
    }
    void EquipImmediate(WeaponType type)
    {
        if (currentWeapon != null) currentWeapon.gameObject.SetActive(false);
        currentWeapon = weapons[(int)type];
        currentWeapon.gameObject.SetActive(true);
        currentWeapon.OnEquip();
    }

    public Animator GetAnimator() => animator;
    public CharacterController GetCharacterController() => characterController;
    public P_Move GetMove() => move;
    public bool IsAttacking => currentWeapon != null && currentWeapon.IsAttacking;
    public bool IsShielding => currentWeapon != null && currentWeapon.IsShielding;
    public void StopAttackImmediately()
    {
        if (currentWeapon != null)
            currentWeapon.StopImmediately();
    }
}
