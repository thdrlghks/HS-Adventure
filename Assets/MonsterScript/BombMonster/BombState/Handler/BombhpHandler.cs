using Unity.VisualScripting;
using UnityEngine;

public class BombhpHandler : MonoBehaviour
{
    private BombMan _bomb;
    private float _bombHP = 5f, _currentHp, _damageTimer, cooltime;
    private bool _damaged, _attacked, _exploed;
    private Collider[] childColliders;
    private Collider _bombCollider, _monsterCollider; 

    public Collider BombCollider => _bombCollider;
    public bool Damaged => _damaged;

    public void Initialize(BombMan bomb)
    {
        _bomb = bomb;
    }

    private void Awake()
    {
        childColliders = GetComponentsInChildren<Collider>();

        foreach (var col in childColliders)
        {
            if (col.name == "explodeCollider")
            {
                _bombCollider = col;
            }
            else if(col.name == "monsterCollider")
            {
                _monsterCollider = col;
            }
        }

        cooltime = 0f;
    }

    private void Update()
    {
        Attack(_monsterCollider, 3f);
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

    public void Attack(Collider collider,float damage)
    {
        if (cooltime > 0) return;
        
        if(collider.CompareTag("Player"))
        {
            // 플레이어 hp 로직 삽입
            cooltime = 2f;
        }
    }
}
