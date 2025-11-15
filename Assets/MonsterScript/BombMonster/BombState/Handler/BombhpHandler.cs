using Unity.VisualScripting;
using UnityEngine;

public class BombhpHandler : MonoBehaviour
{
    private BombMan _bomb;
    private float _bombHP = 5f, _currentHp;

    public void Initialize(BombMan bomb)
    {
        _bomb = bomb;
    }


    private void Update()
    {
        if (_currentHp != _bombHP)
        {
            _bomb.Animator.SetBool("walk", false);
            _bomb.Animator.SetTrigger("damage");
        }
    }


    private void isDamaged(int damage)
    {
        _bomb.Animator.SetTrigger("damage");
        _bomb.Animator.SetBool("walk", false);

        _bombHP -= damage;
    }
}
