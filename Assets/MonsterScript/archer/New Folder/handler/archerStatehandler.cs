using UnityEngine;

public class archerStatehandler : MonoBehaviour
{
    private Archer ar;
    private Transform _player;

    private float detectRadius = 25f, timer = 0f;
    private float frontAngle = 60f, backAngle = 150f;
    private float detectlimit = 1f;
    public LayerMask playerMask;

    private float detectTimer = 0f;
    private bool _attacking, _detected, _damaged;

    public Transform Player => _player;
    public bool Attacking => _attacking;
    public void Initialize(Archer ar)
    {
        this.ar = ar;
    }
    private void Awake()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p == null)
        {
            Debug.Log("플레이어를 찾을 수 없습니다.");
            return;
        }

        _player = p.transform;
    }

    private void Update()
    {
        CheckArea();
        float current = ar.Animator.GetFloat("shoot");
        if (current > 0f)
        {
            current -= Time.deltaTime; 
            if (current < 0f) current = 0f;

            ar.Animator.SetFloat("shoot", current);
        }
    }

    private void CheckArea()
    {
        if (!ar.WanderArea.IsInsideArea(_player.position))
        {
            if (_detected)
            {
                ar.ChangeActionState(archerAction.Wander);
                Debug.Log("플레이어가 활동 범위 밖이므로 추적 중지.");
                _detected = false;
                _attacking = false;
            }

            detectTimer = 0f;
            return;
        }

        Collider[] detect = Physics.OverlapSphere(transform.position, detectRadius, playerMask);


        if (detect.Length == 0)
        {
            if (_detected == true)
            {
                ar.ChangeActionState(archerAction.Wander);
                _detected = false;
                _attacking = false;
                Debug.Log("앗 놓쳤다.");
            }
            detectTimer = 0f;
            return;
        }

        float distance = Vector3.Distance(transform.position, _player.position);
        if (distance <= 20f && _detected && !_attacking)
        {
            float shootCooldown = ar.Animator.GetFloat("shoot");

            if (shootCooldown <= 0f)
            {
                ar.ChangeActionState(archerAction.Attack);
                _attacking = true;
            }
        }

        Vector3 dir = (_player.position - transform.position).normalized;
        float sight = Vector3.Angle(transform.forward, dir);

        if (sight < frontAngle && !_attacking && _detected == false)
        {
            ar.ChangeActionState(archerAction.Detect);
            _detected = true;
        }
        else if (sight >= frontAngle && sight <= backAngle && !_attacking)
        {
            detectTimer += Time.deltaTime;

            if (detectTimer >= detectlimit && _detected == false)
            {
                ar.ChangeActionState(archerAction.Detect);
                _detected = true;
            }
        }
        else
        {
            if (!_detected && !_attacking && _damaged)
            {
                ar.ChangeActionState(archerAction.Detect);
                _detected = true;
            }
        }
    }

    public void Checkdamaged()
    {
        _damaged = true;
    }

    public void exitAttackMode()
    {
        _attacking = false;
    }

    public void timerReset()
    {
        timer = 0f;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 1f, 1f, 0.1f);
        Gizmos.DrawWireSphere(transform.position, detectRadius);

        Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
        DrawSector(transform.position, transform.forward, detectRadius, frontAngle);

        Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
        DrawSector(transform.position, -transform.forward, detectRadius, 180f - backAngle);
    }

    private void DrawSector(Vector3 center, Vector3 direction, float radius, float angle)
    {
        int segments = 30;

        float halfAngle = angle;
        Quaternion leftRot = Quaternion.AngleAxis(-halfAngle, Vector3.up);
        Quaternion rightRot = Quaternion.AngleAxis(halfAngle, Vector3.up);

        Vector3 leftDir = leftRot * direction;
        Vector3 rightDir = rightRot * direction;

        Vector3 prevPoint = center + leftDir * radius;

        for (int i = 1; i <= segments; i++)
        {
            float t = (float)i / segments;
            Quaternion rot = Quaternion.Slerp(leftRot, rightRot, t);
            Vector3 nextDir = rot * direction;
            Vector3 nextPoint = center + nextDir * radius;

            Gizmos.DrawLine(prevPoint, nextPoint);
            prevPoint = nextPoint;
        }

        Gizmos.DrawLine(center, center + leftDir * radius);
        Gizmos.DrawLine(center, center + rightDir * radius);
    }
}
