using UnityEngine;
using System;

public class ControllerPlayer
{
    ModelPlayer _model;

    //CHELO WAS HERE: var para bloquear inputs
    public bool isResting = false, canAttackAtAll = true, isAttacking = false;

    //AGUS ADDON DEMO HDRP
    public float horizontal, vertical;

    //DASH EVENT
    public event Action OnSpacebarDown = delegate { };

    //ATTACK EVENTS
    public event Action OnMouseDown = delegate { };
    public event Action OnMouseUp = delegate { };
    public event Action OnMouse = delegate { };

    //SKILL EVENTS
    public event Func<KeyCode, bool> OnKeyPressed;

    public ControllerPlayer(ModelPlayer model)
    {
        _model = model;
    }

    public void ArtificialUpdate()
    {
        if (OnKeyPressed.Invoke(KeyCode.Tab)) //PARA CAMBIAR LA SKILL NO PONGO UN PAUSE
        {
            //Debug.Log("ENTRO A CAMBIAR SKILL");
            ServiceLocator.Instance.GetDependency<SkillManager>().ChangeSkill();
        }
        //if (isResting) { return; }

        //CHELO WAS HERE: bloquear inputs
        if (isResting) { return; }

        if (canAttackAtAll)
        {
            if (Input.GetMouseButtonDown(0)) OnMouseDown?.Invoke();
            if (Input.GetMouseButtonUp(0)) OnMouseUp?.Invoke();
            if (Input.GetMouseButton(0)) OnMouse?.Invoke();
        }
        if (isAttacking) return;

        _model.CalculateMovement();

        _model.RechargeMana();
        //HASTA ENCONTRAR MEJOR MANERA DE MANEJAR RECHARGE DEJAMOS MANA INRECARGABLE

        if (Input.GetKeyDown(KeyCode.Space) || (Input.GetButtonDown("JoystickCircle"))) //Llamo al evento al que se suscribe el dash para usarlo con la MVC
        {
            OnSpacebarDown?.Invoke();
        }
        
        if (OnKeyPressed.Invoke(KeyCode.X) && OnKeyPressed != null)
        {
            ServiceLocator.Instance.GetDependency<SkillManager>().UseSkill();
        }


        var turnX = Input.GetAxisRaw("Mouse X");
        var turnY = Input.GetAxisRaw("Mouse Y");
    }

    //CHELO WAS HERE: var para bloquear inputs
    //public void SetResting(bool resting, ViewPlayer view = null)
    public void SetResting(bool resting)
    {
        isResting = resting;
    }

    //CAMBIE MOVIMIENTO EN FIXED PUEDE SER QUE SE ROMPA POR ESO
    public void ArtificialFixed()
    {
        if (isResting) { return; }

        if (isAttacking) return;

        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        //Debug.Log($"{horizontal} {vertical}");

        if (horizontal != 0 || vertical != 0)
        {
            _model.NewRun(horizontal, vertical);
            _model.StaminaRechargePS();
        }
        else if (horizontal == 0 || vertical == 0)
        {
            _model.Idle();
            _model.StaminaRechargePS();
        }
    }


    public float GetMouseX()
    {
        return Input.GetAxis("Mouse X");
    }

    public float GetMouseY()
    {
        return Input.GetAxis("Mouse Y");
    }
}
