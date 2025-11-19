using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class SpecialEnemyLife : Entity //NO ENTIENDO LOS ERRORES
{
    [SerializeField] private int soulReward = 100;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    [SerializeField] private VisualEffect bloodVFX;
    private Vector3 offsetY = new Vector3(0, 1, 0);
    [SerializeField] private GameObject vfxSoulsReward;

    //Agus ADD-ON ANIMATOR - SAQUE SETACTIVE - REEMPLACE POR UNA CORRUTINA QUE HACE ANIMACION DE MUERTE
    [SerializeField] private Animator _anim;
    [SerializeField] private FSMEnemy _fsm;

    [SerializeField] BoxCollider _boxCollider;
    //[SerializeField] BoxCollider _karmicBoxCollider;
    [SerializeField] GameObject _karmicBoxCollider;



    protected override void Awake()
    {
        base.Awake();
        initialPosition = transform.position;
        initialRotation = transform.rotation;

        _anim = GetComponent<Animator>();
        _fsm = GetComponent<FSMEnemy>();

        // se registra el enemigo en la lista estatica para el reset
        //SI LO PIENSO PARA BOSSES TENGO QUE CAMBIAR UN POCO ESTA LOGICA PERO PARA ESTO SIRVE
        //SERIA QUE AL MORIR EL JUGADOR SE REINICIEN LOS ENEMIGOS, Y SE REINICIE EL JEFE SI NO LO MATARON ANTES

        _boxCollider.enabled = true;





        //_karmicBoxCollider.enabled = false;
        _karmicBoxCollider.SetActive(false);





    }

    private void Start()
    {        
        ResetTrigger.RegisterEnemy(this); // SI REINICIO EL OBJETO CON LOS ENABLES, VUELVE A ENTRAR ACA? NO CREO, SI NO SE REGISTRARIA 2 VECES
    }

    //protected void Start()
    //{
    //    base.Awake();
    //    initialPosition = transform.position;
    //    initialRotation = transform.rotation;

    //    //_anim = GetComponent<Animator>();
    //    //_fsm = GetComponent<FSMEnemy>();

    //    // se registra el enemigo en la lista estatica para el reset
    //    ResetTrigger.RegisterEnemy(this);
    //    //SI LO PIENSO PARA BOSSES TENGO QUE CAMBIAR UN POCO ESTA LOGICA PERO PARA ESTO SIRVE
    //    //SERIA QUE AL MORIR EL JUGADOR SE REINICIEN LOS ENEMIGOS, Y SE REINICIE EL JEFE SI NO LO MATARON ANTES

    //    _boxCollider.enabled = true;
    //}

    // se ejecuta cada vez que el objeto se activa. osea, reinicio la vida cada vez que el objeto se activa, por eso tambien le paso la posicion y rotacion inicial
    protected virtual void OnEnable()
    {
        InitializeOnEnable();
    }

    public override void TakeDamage(float damage) //overrideo para enemigos
    {
        if (Life <= 0) return;

        Life -= damage;
        bloodVFX.SendEvent("BloodSplatter");
        Debug.Log("vida restante: " + Life);



        //HACER QUE ENTRE EL ESTADO DESDE ACA PARA EVITAR EL ONTRIGGERENTER SIEMPRE
        _fsm.SwitchState(_fsm.onHitStateEnemy);






        if (Life <= 0)
        {
            //DropSouls();                                    
            _fsm._currentSpeed = 0;

            //implementacion nueva: quitarle rootmotion para que no se salga del suelo
            _anim.applyRootMotion = false;
            //CUANDO LE PEGO AL ENEMIGO MUERTO, EL TRIGGER DEL KARMIC ES PARTE DEL ENEMIGO, POR LO QUE ACTIVA LA ANIMACION
            //CAMBIARLE LA LAYER A INVULNERABLE PARA EVITAR QUE LE PEGUE AL TRIGGER EXTERIOR Y SIGA PEGANDOLE AL ENEMIGO O UNA NUEVA LOGICA PARA LA VIDA:
            //if (Life > 0)
            //{

            //    Life -= damage;
            //}


            _anim.SetTrigger("Death");
            _boxCollider.enabled = false;
            Debug.Log("enemigo especial muerto. SISTEMA KARMICO HABILITADO");



            ResetTrigger.RemoveEnemy(this);


            //_karmicBoxCollider.enabled = true;
            _karmicBoxCollider.SetActive(true);




        }
    }

    public void DropSouls()
    {
        // manda los puntos por el singleton
        Debug.Log("almas obtenidas " + soulReward);
        Instantiate(vfxSoulsReward, transform.position + offsetY, transform.rotation);
    }

    public void Death()
    {
        //gameObject.SetActive(false);
        Debug.Log("enemigo especial muerto. SISTEMA KARMICO HABILITADO");
    }

    private void InitializeOnEnable()
    {
        life = maxLife;
        transform.position = initialPosition;
        transform.rotation = initialRotation;
        //RESETEO LA FSM
        if (_fsm != null)
        {
            _fsm.ResetFSM();
        }
        //RESETEO LOS TRIGGERS Y ANIMACION
        if (_anim != null)
        {
            //_anim.ResetTrigger("Death");
            _anim.Play("Idle");
        }
        // REACTIVO COLISION
        if (_boxCollider != null) _boxCollider.enabled = true;









        //if (_karmicBoxCollider != null) _karmicBoxCollider.enabled = false;

        if (_karmicBoxCollider != null) _karmicBoxCollider.SetActive(false);

    }
}