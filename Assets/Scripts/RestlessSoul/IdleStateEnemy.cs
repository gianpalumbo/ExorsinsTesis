using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleStateEnemy : BaseState
{
    public override void OnEnter(FSMEnemy fsm)
    {
        fsm.DebugCurrentState();
        fsm.anim.SetBool("Idle",true);
    }

    public override void Execute(FSMEnemy fsm)
    {
        if (fsm.DistanceToPlayer() < fsm.alertedRadius) //SI PASO EL RADIO DE ALERTED CAMBIO A ALERTED
            fsm.SwitchState(fsm.alertedStateEnemy);
    }


    public override void OnExit(FSMEnemy fsm)
    {
        fsm.anim.SetBool("Idle", false);
    }
}
