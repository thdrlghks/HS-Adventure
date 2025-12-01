using UnityEngine;

public abstract class W_Base : MonoBehaviour
{
    protected P_Attack owner;
    protected P_Move move;
    protected Animator animator;
    protected CharacterController controller;
    protected P_Player playerStats;

    [SerializeField] protected AnimatorOverrideController weaponOverride;

    public virtual void Init(P_Attack owner)
    {
        this.owner = owner;
        move = owner.GetMove();
        animator = owner.GetAnimator();
        controller = owner.GetCharacterController();
        playerStats = owner.GetComponent<P_Player>();

        if (weaponOverride != null && animator != null)
        {
            // 클립 교체
            animator.runtimeAnimatorController = weaponOverride;
        }

        move = owner.GetMove();
        animator = owner.GetAnimator();
        controller = owner.GetCharacterController();
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
        if (weaponOverride != null && animator != null)
        {
            // 클립 교체
            animator.runtimeAnimatorController = weaponOverride;
        }
    }
    public virtual void OnUnequip() { }
}
    