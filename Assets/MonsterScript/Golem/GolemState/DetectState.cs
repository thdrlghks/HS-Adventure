using UnityEngine;
using UnityEngine.AI;

public class DetectState : State<Golem>
{
    public override void Enter(Golem gol)
    {
        Debug.Log("플레이어 찾았따!!");
        gol.Agent.speed = 5f;
        gol.Animator.SetBool("Detect", true);
        gol.Animator.SetFloat("move", 0.5f);
        gol.Animator.Play("Detect", 0, 0f);
    }

    public override void Execute(Golem gol)
    {
        Transform player = gol.GolemStateHandler.Player;
        NavMeshAgent agent = gol.NavMeshAgent;

        if (player == null || agent == null) return;

        agent.SetDestination(player.position);
    }

    public override void FixedExecute(Golem gol)
    {
    }

    public override void Exit(Golem gol)
    {
        gol.Agent.speed = 3.5f;
        gol.Animator.SetBool("Detect", false);
        gol.Animator.SetFloat("move", 1);
    }
}

