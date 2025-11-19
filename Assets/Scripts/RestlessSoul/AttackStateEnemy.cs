using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackStateEnemy : BaseState
{
    public override void OnEnter(FSMEnemy fsm)
    {
        fsm.DebugCurrentState();
        fsm.anim.SetBool("Attack", true);

        fsm.DisableMovement();
    }

    public override void Execute(FSMEnemy fsm)
    {
        fsm.LookAtPlayer();

        //MEJOR PEGARLE AL PLAYER SI HACE LA ANIMACION DE ATAQUE OSEA QUE ESTA CERCA Y PUEDO HACERLO EN "X" FRAME

        //if (fsm.AttackPlayer())
        //{
        //    fsm.ResetCounterAttack();
        //    fsm.player.TakeDamage(fsm.dmg);
        //}
        if (fsm.DistanceToPlayer() > fsm.attackRadius)
        {
            fsm.SwitchState(fsm.followStateEnemy); //Aproximo a 45 frames sobre 60fps aprox
        }
    }


    public override void OnExit(FSMEnemy fsm)
    {
        fsm.anim.SetBool("Attack", false);
    }
}

