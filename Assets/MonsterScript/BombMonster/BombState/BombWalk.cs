using UnityEngine;
using UnityEngine.AI;

public class BombWalk : State<BombMan>
{
    public override void Enter(BombMan bombman)
    {
        Debug.Log("플레이어 발견!!");
        bombman.Animator.SetBool("walk", true);
    }

    public override void Execute(BombMan bombman)
    {
        Transform player = bombman.BombStateHandler.Player;
        NavMeshAgent agent = bombman.NavMeshAgent;

        if (player == null || agent == null) return;

        agent.SetDestination(player.position);
    }

    public override void FixedExecute(BombMan bombman)
    {
    }

    public override void Exit(BombMan bombman)
    {
        bombman.Animator.SetBool("walk", false);
        bombman.NavMeshAgent.ResetPath();
    }
}
