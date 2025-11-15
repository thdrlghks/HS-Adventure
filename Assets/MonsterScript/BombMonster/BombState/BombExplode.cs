using UnityEngine;

public class BombExplode : State<BombMan>
{
    private float Timer;
    public override void Enter(BombMan bombman)
    {
        Debug.Log("Æø¹ß!!");
        Timer = 0f;
        bombman.Animator.SetBool("walk",false);
        bombman.Animator.SetTrigger("attack01");
    }

    public override void Execute(BombMan bombman)
    {
        Timer += Time.deltaTime;
        if(Timer > 0.5f)
        {
            bombman.BombhpHandler.Attack(bombman.BombhpHandler.BombCollider, 30f);
        }

        if (Timer > 2f)
        {
            bombman.BombhpHandler.CheckExplode();
        }
    }

    public override void FixedExecute(BombMan bombman)
    {
        
    }

    public override void Exit(BombMan bombman)
    {
        
    }
}
