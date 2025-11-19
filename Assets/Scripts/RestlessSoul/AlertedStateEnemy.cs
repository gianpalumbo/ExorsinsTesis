using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlertedStateEnemy : BaseState
{
    public override void OnEnter(FSMEnemy fsm)
    {
        fsm.DebugCurrentState();
        fsm.anim.SetTrigger("Alerted");
    }

    public override void Execute(FSMEnemy fsm)
    {
        if (fsm.DistanceToPlayer() < fsm.followRadius) //SI PASA EL RADIO DE ATAQUE CAMBIO A FOLLOW Y LO PERSIGO
            fsm.SwitchState(fsm.followStateEnemy);
        else if (fsm.DistanceToPlayer() > fsm.alertedRadius)
            fsm.SwitchState(fsm.idleStateEnemy);
    }


    public override void OnExit(FSMEnemy fsm)
    {
        
    }
}

