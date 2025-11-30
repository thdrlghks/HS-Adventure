using UnityEngine;

public class archerWanderState : State<Archer>
{
    private Vector3 _destination;
    private float _idleTimer;
    public override void Enter(Archer ar)
    {
        SetNewRandomDestination(ar);
        ar.Agent.speed = 3.5f;
        ar.Animator.SetBool("detect", false);
        ar.Animator.Play("walk", 0, 0f);
    }

    public override void Execute(Archer ar)
    {
        if (ar.NavMeshAgent == null) return;

        float distance = Vector3.Distance(ar.transform.position, _destination);

        if (distance < 1.0f)
        {
            ar.Animator.SetBool("detect", false);
            _idleTimer += Time.deltaTime;

            if (_idleTimer >= 3f)
            {
                _idleTimer = 0f;
                SetNewRandomDestination(ar);
            }

            return;
        }
    }

    public override void FixedExecute(Archer ar)
    {

    }

    public override void Exit(Archer ar)
    {
        ar.NavMeshAgent.ResetPath();
    }

    private void SetNewRandomDestination(Archer ar)
    {
        ar.Animator.SetBool("detect", true);
        if (ar.WanderArea == null)
        {
            Debug.LogWarning("Wander Area ¾øÀ½!");
            return;
        }

        _destination = ar.WanderArea.GetRandomPoint();
        ar.NavMeshAgent.SetDestination(_destination);
        Debug.DrawLine(ar.transform.position, _destination, Color.green, 1f);
    }
}
