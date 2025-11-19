using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FollowStateEnemy : BaseState
{
    public override void OnEnter(FSMEnemy fsm)
    {
        fsm.DebugCurrentState();
        fsm.anim.SetBool("Follow", true);
        fsm.EnableMovement();
    }

    public override void Execute(FSMEnemy fsm)
    {
        if (fsm.IsDead()) return;

        if (!fsm.canFollow) return;

        fsm.LookAtPlayer();

        //fsm.CounterFollowTime();
        //bool canFollow = fsm.CounterFollowBool();

        Vector3 newPos = fsm.enemyRB.position + fsm.DirToTarget() * fsm._currentSpeed * Time.fixedDeltaTime;
        fsm.enemyRB.MovePosition(newPos);

        if (fsm.DistanceToPlayer() < fsm.attackRadius)
            fsm.SwitchState(fsm.attackStateEnemy);
        else if (fsm.DistanceToPlayer() > fsm.followRadius)
            fsm.SwitchState(fsm.alertedStateEnemy);
    }


    public override void OnExit(FSMEnemy fsm)
    {
        fsm.anim.SetBool("Follow", false);
    }
}
