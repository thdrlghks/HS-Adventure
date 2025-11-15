using UnityEngine;

public class BombStateHandler : MonoBehaviour
{
    private BombMan _bomb;
    private Transform _player;
    private Collider _bombCollider;

    private float detectRadius = 5f;
    private float frontAngle = 60f, backAngle = 150f;          
    private float detectlimit = 3f;
    public LayerMask playerMask;

    private float detectTimer = 0f;
    private bool _readyExplode, _walking, _damaged;

    public Transform Player => _player;
    public void Initialize(BombMan bomb)
    {
        _bomb = bomb;
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
    }

    private void CheckArea()
    {
        if (!_bomb.WanderArea.IsInsideArea(_player.position))
        {
            if (_walking)
            {
                _bomb.ChangeActionState(BombManAction.idle);
                Debug.Log("플레이어가 활동범위 밖이므로 추적 중지.");
                _walking = false;
            }

            detectTimer = 0f;
            return;
        }

        Collider[] detect = Physics.OverlapSphere(transform.position, detectRadius, playerMask);

        if (detect.Length == 0)
        {
            if(_walking == true)
            {
                _bomb.ChangeActionState(BombManAction.idle);
                Debug.Log("앗 놓쳤다.");
            }
            _walking = false;
            detectTimer = 0f;
            return;
            
        }
        
        float distance = Vector3.Distance(transform.position, _player.position);
        if (distance <= 1.5f && _readyExplode == false && _walking == true || _damaged)
        {
            _readyExplode = true;
            _bomb.ChangeActionState(BombManAction.explode);
        } 
        
        Vector3 dir = (_player.position - transform.position).normalized;
        float sight = Vector3.Angle(transform.forward, dir);

        if (sight < frontAngle && !_readyExplode && _walking == false)
        {
            _bomb.ChangeActionState(BombManAction.walk);
            _walking = true;
        }

        if (sight >= frontAngle && sight <= backAngle && !_readyExplode)
        {
            detectTimer += Time.deltaTime;
            
            if(detectTimer >= detectlimit && _walking == false)
            {
                _bomb.ChangeActionState(BombManAction.walk);
                _walking = true;
            }
        }

        if(_bomb.BombhpHandler.Damaged)
        {
            _bomb.ChangeActionState(BombManAction.walk);
            _walking = true;
        }
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
