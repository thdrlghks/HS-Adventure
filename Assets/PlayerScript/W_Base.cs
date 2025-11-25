using UnityEngine;

public abstract class W_Base : MonoBehaviour
{
    protected P_Attack owner;
    protected P_Move move;
    protected Animator animator;
    protected CharacterController controller;

    public virtual void Init(P_Attack owner)
    {
        this.owner = owner;
        move = owner.GetMove();
        animator = owner.GetAnimator();
        controller = owner.GetCharacterController();
    }

    public abstract void Attack();
    public virtual void StopImmediately() { }
    public virtual void OnUpdate() { }
    public virtual bool IsAttacking => false;
    public virtual bool IsShielding => false;
}
    