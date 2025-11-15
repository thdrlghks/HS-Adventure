using UnityEngine;
using UnityEngine.AI;

public class WanderState : State<Golem>
{
    private Vector3 _destination;
    private float _idleTimer;

    public override void Enter(Golem gol)
    {
        Debug.Log("∞Ò∑Ω πË»∏ Ω√¿€");
        SetNewRandomDestination(gol);

        gol.Agent.speed = 3.5f;
        gol.Animator.SetBool("Detect",true);
        gol.Animator.SetFloat("move", 0);
    }

    public override void Execute(Golem gol)
    {
        if (gol.NavMeshAgent == null) return;

        float distance = Vector3.Distance(gol.transform.position, _destination);

        if (distance < 1.0f)
        {
            gol.Animator.SetBool("Detect", false);
            gol.Animator.SetFloat("move", 1);
            _idleTimer += Time.deltaTime;

            if (_idleTimer >= 3f)
            {
                _idleTimer = 0f;
                SetNewRandomDestination(gol);
            }

            return;
        }
    }

    public override void FixedExecute(Golem gol)
    {

    }

    public override void Exit(Golem gol)
    {
        gol.NavMeshAgent.ResetPath();
        gol.Animator.SetBool("Detect", false);
        gol.Animator.SetFloat("move", 1);
    }

    private void SetNewRandomDestination(Golem gol)
    {
        gol.Animator.SetBool("Detect", true);
        gol.Animator.SetFloat("move", 0);
        if (gol.WanderArea == null)
        {
            Debug.LogWarning("Wander Area æ¯¿Ω!");
            return;
        }

        _destination = gol.WanderArea.GetRandomPoint();
        gol.NavMeshAgent.SetDestination(_destination);
        Debug.DrawLine(gol.transform.position, _destination, Color.green, 1f);
    }
}
