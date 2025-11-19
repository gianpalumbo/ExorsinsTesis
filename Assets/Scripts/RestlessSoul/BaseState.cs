using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseState
{
    public abstract void OnEnter(FSMEnemy fsm);

    public abstract void Execute(FSMEnemy fsm);

    public abstract void OnExit(FSMEnemy fsm);
}
