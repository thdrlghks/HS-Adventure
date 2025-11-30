using UnityEngine;

public class GolemAttackHandler : MonoBehaviour
{
    private Golem gol;

    private Collider[] childColliders;
    private Collider _bodyCollider, _leftArmCollider, _rightArmCollider, _legCollider;

    private float _golemHP = 100f, _damageTimer;
    private bool _damaged;
    public void Initialize(Golem golem)
    {
        gol = golem;
    }

    private void Awake()
    {
        childColliders = GetComponentsInChildren<Collider>();

        foreach (var col in childColliders)
        {
            if (col.name == "B-chest")
            {
                _bodyCollider = col;
            }
            else if (col.name == "B-forearm.L")
            {
                _leftArmCollider = col;
            }
            else if (col.name == "B-forearm.R")
            {
                _rightArmCollider = col;
            }
            else if (col.name == "B-shin.R")
            {
                _legCollider = col;
            }
        }
    }

    private void Update()
    {
        if (_damaged)
        {
            _damageTimer += Time.deltaTime;
        }


        if (_damageTimer >= 2f)
        {
            _damaged = false;
            _damageTimer = 0;
        }
    }

    public void isDamaged(float damage)
    {
        if(_damaged) return;

        if (_bodyCollider.CompareTag("sword") && _leftArmCollider.CompareTag("sword") && _rightArmCollider.CompareTag("sword"))
        {
            _golemHP = _golemHP - damage;
            _damaged = true;
        }
    }
    

    public void Attack(float mode, float damage)
    {
        if (mode == 0)
        {
            if (_leftArmCollider.CompareTag("Player") && _rightArmCollider.CompareTag("Player"))
            {
                //플레이어 hp감소 로직 삽입 부분
            }
        }
        else if (mode == 1)
        {
            if(_legCollider.CompareTag("Player") && _rightArmCollider.CompareTag("Player") && _leftArmCollider.CompareTag("Player"))
            {
                //플레이어 hp감소 로직 삽입 부분
            }
        }
        else
        {
            if (_golemHP < 100)
            {
                _golemHP += 10f;
                if (_golemHP > 100)
                {
                    _golemHP = 100;
                }
            }
        }
    }
}
