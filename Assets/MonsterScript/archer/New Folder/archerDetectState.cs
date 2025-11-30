using UnityEngine;
using UnityEngine.AI;

public class archerDetectState : State<Archer>
{
    public override void Enter(Archer ar)
    {
        ar.Agent.speed = 5f;
        ar.Animator.SetBool("detect", true);
        ar.Animator.SetFloat("shoot", 5f);
        ar.Animator.Play("run", 0, 0f);
    }

    public override void Execute(Archer ar)
    {
        Transform player = ar.archerStatehandler.Player;
        NavMeshAgent agent = ar.NavMeshAgent;

        if (player == null || agent == null) return;

        float minRange = 10f;
        float maxRange = 25f;    
        float orbitSpeed = 3f;   
        float targetRange = 7.5f; 

        float distance = Vector3.Distance(ar.transform.position, player.position);

        if (distance < minRange)
        {
            Vector3 retreatDir = (ar.transform.position - player.position).normalized;
            agent.SetDestination(ar.transform.position + retreatDir * 3f);
            return;
        }
        else if (distance > maxRange)
        {
            agent.SetDestination(player.position);
            return;
        }

        Vector3 toArcher = (ar.transform.position - player.position).normalized;
        Vector3 orbitDir = Vector3.Cross(Vector3.up, toArcher).normalized;
        Vector3 orbitTarget = ar.transform.position + orbitDir * orbitSpeed;

        agent.SetDestination(orbitTarget);
        ar.transform.rotation = Quaternion.Slerp(
            ar.transform.rotation,
            Quaternion.LookRotation(player.position - ar.transform.position),
            Time.deltaTime * 5f
        );
    }

    public override void FixedExecute(Archer ar)
    {

    }

    public override void Exit(Archer ar)
    {

    }
}
