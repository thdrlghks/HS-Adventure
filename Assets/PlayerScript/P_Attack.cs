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

    public GameObject shield;

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
            w.gameObject.SetActive(false);
        }

        Equip(WeaponType.Sword);
    }

    void Update()
    {
        HandleWeaponSwitch();
        currentWeapon.OnUpdate();

        if (Input.GetMouseButtonDown(0))
            currentWeapon.Attack();
    }

    // ======================================
    //   📌 숫자키로 무기 교체
    // ======================================
    void HandleWeaponSwitch()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Equip(WeaponType.Sword);
            Debug.Log("무기 교체: Sword");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Equip(WeaponType.Bow);
            Debug.Log("무기 교체: Bow");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Equip(WeaponType.Bomb);
            Debug.Log("무기 교체: Bomb");
        }
    }

    public void Equip(WeaponType type)
    {
        if (currentWeapon == weapons[(int)type])
            return;

        // 공격 중이면 즉시 중단
        if (currentWeapon != null)
        {
            currentWeapon.StopImmediately();
            currentWeapon.gameObject.SetActive(false);
        }

        currentWeapon = weapons[(int)type];
        currentWeapon.gameObject.SetActive(true);

        currentWeapon.OnEquip();

        if ((int)type == 0) shield.SetActive(true);
        else shield.SetActive(false);
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
