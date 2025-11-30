using System.Threading;
using UnityEngine;

public class AttackState : State<Golem>
{
    private float _attackmode, _timer;
    public override void Enter(Golem gol)
    {
        _attackmode = Random.Range(0, 3);
        gol.Animator.SetTrigger("AttackMode");

        switch (_attackmode)
        {
            case 0: gol.Animator.SetTrigger("Attack"); break;
            case 1: gol.Animator.SetTrigger("Attack2"); break;
            case 2: gol.Animator.SetTrigger("Attack3"); break;
        }
        gol.Animator.Play("AttackMode", 0, 0f);
    }

    public override void Execute(Golem gol)
    {
        Transform target = gol.GolemStateHandler.Player;
        Vector3 direction = (target.position - gol.transform.position).normalized;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion lookRot = Quaternion.LookRotation(direction);
            gol.transform.rotation = Quaternion.Slerp(
                gol.transform.rotation,
                lookRot,
                10f * Time.deltaTime
            );
        }

        gol.golemAttackHandler.Attack(_attackmode, 10f);

    }

    public override void FixedExecute(Golem gol)
    {
        if(!gol.GolemStateHandler.Attacking)
        {
            gol.ChangeActionState(GolemAction.Wander);
        }
    }

    public override void Exit(Golem gol)
    {
        gol.golemAttackHandler.Reset();
    }
}
