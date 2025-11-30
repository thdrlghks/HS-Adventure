using UnityEngine;

public class archerAttackState : State<Archer>
{
    private float timer;
    private bool hasShot;

    public override void Enter(Archer ar)
    {
        timer = 0f;
        hasShot = false;

        ar.Animator.SetFloat("shoot", 0f);
        ar.Animator.Play("shoot", 0, 0f);

        ar.NavMeshAgent.isStopped = true;
        ar.NavMeshAgent.ResetPath();
    }

    public override void Execute(Archer ar)
    {
        timer += Time.deltaTime;

        Transform player = ar.archerStatehandler.Player;
        if (player == null) return;

        Vector3 dir = (player.position - ar.transform.position).normalized;
        dir.y = 0;
        Quaternion rot = Quaternion.LookRotation(dir);
        ar.transform.rotation = Quaternion.Slerp(ar.transform.rotation, rot, 10f * Time.deltaTime);

        if (!hasShot && timer >= 0.45f)
        {
            ar.monsterShoot.shoot(20f, player.gameObject);
            hasShot = true;
        }

        if (timer >= 2.0f)
        {
            ar.ChangeActionState(archerAction.Detect);
        }
    }

    public override void FixedExecute(Archer ar)
    {
        
    }
    public override void Exit(Archer ar)
    {
        ar.NavMeshAgent.isStopped = false;
        ar.Animator.SetFloat("shoot", 5f);
        ar.archerStatehandler.exitAttackMode();
    }
}
