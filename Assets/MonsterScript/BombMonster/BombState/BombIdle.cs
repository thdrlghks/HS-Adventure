using UnityEngine;

public class BombIdle : State<BombMan>
{
    private Vector3 _destination;
    private float _idleTimer;
    

    public override void Enter(BombMan bombman)
    {
        Debug.Log("Wander 시작");

        SetNewRandomDestination(bombman);
    }

    public override void Execute(BombMan bombman)
    {
        if (bombman.NavMeshAgent == null) return;

        float distance = Vector3.Distance(bombman.transform.position, _destination);

        if (distance < 1.0f)
        {
            bombman.Animator.SetBool("walk",false);
            _idleTimer += Time.deltaTime;

            if (_idleTimer >= 3f)
            {
                _idleTimer = 0f;
                SetNewRandomDestination(bombman);
            }

            return;
        }
    }

    public override void FixedExecute(BombMan bombman)
    {

    }

    public override void Exit(BombMan bombman)
    {
        bombman.NavMeshAgent.ResetPath();
    }

    private void SetNewRandomDestination(BombMan bombman)
    {
        bombman.Animator.SetBool("walk", true);
        if (bombman.WanderArea == null)
        {
            Debug.LogWarning("Wander Area 없음!");
            return;
        }

        _destination = bombman.WanderArea.GetRandomPoint();
        bombman.NavMeshAgent.SetDestination(_destination);
        Debug.DrawLine(bombman.transform.position, _destination, Color.green, 1f);
    }
}
