using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnHitStateEnemy : BaseState
{
    public override void OnEnter(FSMEnemy fsm)
    {
        fsm.anim.SetTrigger("OnHit");
        fsm.DisableMovement();

        //if (fsm.isSlowed) fsm._currentSpeed = fsm.slowSpeed;
        //else fsm._currentSpeed = fsm.normalSpeed;
    }

    public override void Execute(FSMEnemy fsm)
    {
        if (fsm.DistanceToPlayer() < fsm.attackRadius)
            fsm.SwitchState(fsm.attackStateEnemy);
        else if (fsm.DistanceToPlayer() < fsm.followRadius)
            fsm.SwitchState(fsm.followStateEnemy);
        else if (fsm.DistanceToPlayer() < fsm.alertedRadius)
            fsm.SwitchState(fsm.alertedStateEnemy);

        //primero chequea si esta en ataque si lo esta pasa
        //sino pasa a follow
        //y sino pasa a alerted
        //a Idle doy por sentado que no llega
    }

    public override void OnExit(FSMEnemy fsm)
    {
        Debug.Log("Salgo de ONHIT");
    }
}
