using Unity.VisualScripting;
using UnityEngine;

public class BombhpHandler : MonoBehaviour
{
    [SerializeField] private ArmHitBox _Explosion;
   
    private BombMan _bomb;
    private float _bombHP = 5f, _currentHp, _damageTimer, cooltime;
    private bool _damaged, _attacked, _exploed;
    private Collider[] childColliders;
    private Collider _bombCollider, _monsterCollider;
    private P_Player _player;
    public Collider BombCollider => _bombCollider;
    public bool Damaged => _damaged;

    public void Initialize(BombMan bomb)
    {
        _bomb = bomb;
    }

    private void Awake()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        _player = p.GetComponent<P_Player>();

        cooltime = 0f;
    }

    private void Update()
    {
        DestroyBombMan();

        if(cooltime > 0f)
        {
            cooltime -= Time.deltaTime;
        }
    }

    public void isDamaged(float damage)
    {
        _bombHP -= damage;
        _damaged = true;
        _bomb.Animator.SetBool("walk", false);
        _bomb.Animator.SetTrigger("damage");

        _damageTimer += Time.deltaTime;

        if (_damageTimer >= 2f)
        {
            _bomb.Animator.SetBool("walk", true);
        }
    } 

    private void DestroyBombMan()
    {
        if(_bombHP <= 0 || _exploed)
        {
            Destroy(gameObject);
        }
    }

    public void CheckExplode()
    {
        _exploed = true;
    }

    public void Attack(Collider collider,int damage)
    {
        _Explosion.active = true;
        if (cooltime > 0) return;
        
        if(collider.CompareTag("Player"))
        {
            _player.TakeDamage(damage);
            cooltime = 2f;
        }
    }
}
