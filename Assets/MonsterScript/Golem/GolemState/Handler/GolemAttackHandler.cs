using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class GolemAttackHandler : MonoBehaviour
{
    private Golem gol;
    private W_Sword sword1;

    private P_Player _player;

    [SerializeField] private ArmHitBox hit1;
    [SerializeField] private ArmHitBox hit2;
    [SerializeField] private ArmHitBox hit3;

    private float _golemHP = 100f,_currrentHp, _damageTimer, combotimer, combo;
    private bool _damaged, attacking;
    public void Initialize(Golem golem)
    {
        gol = golem;
    }

    private void Awake()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        _player = p.GetComponent<P_Player>();

        if (p == null)
        {
            Debug.Log("플레이어를 찾을 수 없습니다.");
            return;
        }

        GameObject sword = GameObject.FindGameObjectWithTag("sword");
        if(sword == null)
        {
            Debug.Log("오브젝트 탐색 실패");
            return;
        }

        _currrentHp = _golemHP;
        sword1 = sword.GetComponent<W_Sword>();
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

        if (_currrentHp != _golemHP)
        {
            Debug.Log(_currrentHp);
            if (_currrentHp <= 0f)
            {
                Destroy(gameObject);
            }
            _damaged = false;
        }

        if (attacking)
        {
            combotimer += Time.deltaTime;
        }
    }

    public void Attack(float mode, float damage)
    {
        if (mode == 0)
        {
            hit1.active = true;
            hit2.active = true;
        }
        else if (mode == 1)
        {
            hit1.active = true;
            hit2.active = true;
            hit3.active = true;
        }
        else if (mode == 2)
        {
            _currrentHp = Mathf.Min(_currrentHp + 10f, _golemHP);
            return;
        }

    }

    public void Reset()
    {
        hit1.active = false;
        hit2.active = false;
        hit3.active = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("sword") && sword1._isAttacking)
        {

            if (combotimer >= 2f)
            {
                attacking = false;
                combo = 0;
            }

            if (combo == 0)
            {
                attacking = true;
                _currrentHp -= 10f;
                combo++;
                combotimer = 0f;
            }
            else if (combo == 1)
            {
                _currrentHp -= 15f;
                combo++;
                combotimer = 0f;
            }
            else if (combo == 2)
            {
                _currrentHp -= 25f;
                combo = 0;
                attacking = false;
                combotimer = 0f;
            }
        }
        if (other.CompareTag("arrow"))
        {
            _currrentHp -= 10f;
        }

        if (other.CompareTag("bomb"))
        {
            _currrentHp -= 30;
        }
    }
}
