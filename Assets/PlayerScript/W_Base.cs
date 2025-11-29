using UnityEngine;

public abstract class W_Base : MonoBehaviour
{
    protected P_Attack owner;
    protected P_Move move;
    protected Animator animator;
    protected CharacterController controller;

    [SerializeField] protected AnimatorOverrideController weaponOverride;

    public virtual void Init(P_Attack owner)
    {
        this.owner = owner;
        move = owner.GetMove();
        animator = owner.GetAnimator();
        controller = owner.GetCharacterController();

        if (weaponOverride != null && animator != null)
        {
            // 현재 애니메이터의 상태(걷는 중 등)를 유지하며 클립만 교체
            animator.runtimeAnimatorController = weaponOverride;
        }
    }
    public virtual bool CanMoveWhileAiming => true;
    public abstract void Attack();
    public virtual void StopImmediately() { }
    public virtual void OnUpdate() { }
    public virtual bool IsAttacking => false;
    public virtual bool IsShielding => false;
    public virtual float GetDamageReductionRate()
    {
        return 0f;
    }
    public virtual void OnEquip()
    {
        // 1. 애니메이션 컨트롤러 교체
        if (weaponOverride != null && animator != null)
        {
            animator.runtimeAnimatorController = weaponOverride;
        }
    }
}
    