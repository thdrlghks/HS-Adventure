using UnityEngine;

public class BombWalk : State<BombMan>
{
    public override void Enter(BombMan bombman)
    {
        Debug.Log("플레이어 발견!!");
    }

    public override void Execute(BombMan bombman)
    {

    }

    public override void FixedExecute(BombMan bombman)
    {

    }

    public override void Exit(BombMan bombman)
    {

    }
}
