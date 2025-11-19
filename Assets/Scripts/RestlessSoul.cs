using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using IA2;
using System;
using JetBrains.Annotations;


public class RestlessSoul : MonoBehaviour , ISlowable
{
    public enum RSInputs { THINKING, IDLE, FOLLOW, ATTACK, HITSTUN, DIE }
    [SerializeField] private EventFSM<RSInputs> _myFsm;
    [SerializeField] private Rigidbody _myRb;
    [SerializeField] Vector3 _currentVelocity;
    [SerializeField] float steeringRadius = 1f;
    [SerializeField] Animator _anim;
    [SerializeField] EnemyLife _enemyLife;
    //GameObject attackPrefab;
    [SerializeField] PlayerMVC _player;
    [SerializeField] PlayerLife _playerLife;
    VisualEffect _playerBlood;



    //CHELO WAS HERE, CAMBIE PLAYERMVC POR UN TARGET PARA QUE AGARRE CUALQUIERA
    //[SerializeField] GameObject target;
    //CHELO WAS HERE: YA NO SE QUE ESTOY HACIENDO, CREE ESTADOS PUROS SIN VAR PARA QUE LOS PUEDA REUTILIZAR AL RESETEAR EL ENEMIGO
    private State<RSInputs> thinking, idle, follow, attack, hitstun, die;

    bool hasBeenInstantiated = false, canThink = true; //Para saber si ya se instancio el enemigo, para no volver a instanciarlo
    //Radius Attack and Follow
    [SerializeField] float followRadius, attackRadius;
    [SerializeField] float _speed = 2f, _currentSpeed, _rotationSpeed = 8f, _currentRotation;
    [SerializeField] float counter;
    [SerializeField] float timeToRest = 1f, timeAfterHit = .5f;
    [SerializeField] float dmg;

    [SerializeField] bool isOnFollowRange, isOnAttackRange, hasBeenHit, isDeath, animationFinished, isPlayerDeath; //Este va a ser nuestro objetivo

    private void Awake()
    //private void Start()
    {
        _currentSpeed = _speed;
        _currentRotation = _rotationSpeed;
        _enemyLife.OnHit += OnHitTrue; //LE AGREGO EL ONHITTRUE Y CUANDO TERMINO DE HACER LA ANIMACION SE VUELVE FALSE ASI NO VUELVE A ONHIT

        _myRb = GetComponent<Rigidbody>();
        _anim = GetComponent<Animator>();
        _enemyLife = GetComponent<EnemyLife>();
        
        try
        {
            if (_playerBlood == null)
                _playerBlood = _player.GetComponentInChildren<VisualEffect>();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("No se pudo obtener VisualEffect: " + e.Message);
        }


        #region States Declaration
        //var thinking = new State<RSInputs>("THINKING");
        //var idle = new State<RSInputs>("IDLE");
        //var follow = new State<RSInputs>("FOLLOW");
        //var attack = new State<RSInputs>("ATTACK");
        //var hitstun = new State<RSInputs>("HITSTUN");
        //var die = new State<RSInputs>("DIE");

        thinking = new State<RSInputs>("THINKING");
        idle = new State<RSInputs>("IDLE");
        follow = new State<RSInputs>("FOLLOW");
        attack = new State<RSInputs>("ATTACK");
        hitstun = new State<RSInputs>("HITSTUN");
        die = new State<RSInputs>("DIE");

        #endregion
        #region StateConfigurer States currently 9 States
        //THINKING VA A PODER PASAR A TODOS
        //Y LUEGO TODOS VAN A PODER PASAR A THINKING TAMBIEN ASI YO PUEDO DARLE UN CD AL PASAR DE ESTADOS
        //ATTACK Y FOLLLOW PASAN A HITSTUN
        //SOLO HITSTUN PUEDE PASAR A DEATH
        StateConfigurer.Create(thinking)
            .SetTransition(RSInputs.IDLE, idle)
            .SetTransition(RSInputs.FOLLOW, follow)
            .SetTransition(RSInputs.ATTACK, attack)
            .SetTransition(RSInputs.HITSTUN, hitstun)
            .Done();

        StateConfigurer.Create(idle)
            .SetTransition(RSInputs.FOLLOW, follow)
            .Done();

        StateConfigurer.Create(follow)
            .SetTransition(RSInputs.IDLE, idle)
            .SetTransition(RSInputs.ATTACK, attack)
            .SetTransition(RSInputs.HITSTUN, hitstun)
            .Done();

        StateConfigurer.Create(attack)
            .SetTransition(RSInputs.THINKING, thinking)
            .SetTransition(RSInputs.HITSTUN, hitstun)
            .Done();

        StateConfigurer.Create(hitstun)
            .SetTransition(RSInputs.HITSTUN, hitstun)
            .SetTransition(RSInputs.THINKING, thinking)
            .SetTransition(RSInputs.DIE, die)
            .Done();

        StateConfigurer.Create(die)
            .Done();
        #endregion

        #region StatesLogicAndTransitions
        //IDLE
        idle.OnEnter += x =>
        {
            _anim.SetBool("Idle", true);
            //Debug.Log("Entr� a IDLE");
        };
        idle.OnUpdate += () =>
        {
            if (isOnFollowRange) SendInputToFSM(RSInputs.FOLLOW);
            if (isSlowed) SendInputToFSM(RSInputs.THINKING);
        };
        idle.OnExit += x => _anim.SetBool("Idle", false);
        //FOLLOW
        follow.OnEnter += x =>
        {
            _anim.SetBool("Follow", true);
           // Debug.Log("Entr� a FOLLOW");
        };
        follow.OnUpdate += () =>
        {
            //LookAtParameterOnY(_player.transform); //ROTAR HACIA EL PARAMETRO EN Y
            LookAtParameterOnY(_player.transform); //ROTAR HACIA EL PARAMETRO EN Y
            if (!isOnFollowRange) SendInputToFSM(RSInputs.IDLE);
            if (isOnAttackRange) SendInputToFSM(RSInputs.ATTACK);
            //Debug.Log("la FSM Funciona");
            if (isSlowed) SendInputToFSM(RSInputs.THINKING);

        };
        follow.OnFixedUpdate += () =>
        {
            _myRb.position += GetSteerToParameter(_player.transform, _speed);
        };
        follow.OnExit += x => _anim.SetBool("Follow", false);
        //ATTACK
        attack.OnEnter += x =>
        {
            _anim.SetTrigger("Attack");
            animationFinished = false;
            DisableMovement();
        };
        attack.OnUpdate += () =>
        {
            if (animationFinished) SendInputToFSM(RSInputs.THINKING);
            if (isSlowed) SendInputToFSM(RSInputs.THINKING);
        };
        attack.OnExit += x => EnableMovement();
        //THINKING
        thinking.OnUpdate += () =>
        {
            if (GenericCounter(timeToRest))
            {
                if (isOnAttackRange) SendInputToFSM(RSInputs.ATTACK);
                if (isOnFollowRange) SendInputToFSM(RSInputs.FOLLOW);
                else SendInputToFSM(RSInputs.IDLE);
            }
        };
        thinking.OnExit += x => ResetCounter();
        //HITSTUN
        hitstun.OnEnter += x =>
        {
            animationFinished = false;
            _anim.SetTrigger("OnHit");
        };
        hitstun.OnUpdate += () =>
        {
            if (animationFinished) SendInputToFSM(RSInputs.THINKING);
            if (_enemyLife.Life <= 0) SendInputToFSM(RSInputs.DIE);
            if (isSlowed) SendInputToFSM(RSInputs.THINKING);
        };
        hitstun.OnExit += x => hasBeenHit = false;
        //DIE
        die.OnEnter += x =>
        {
            _anim.SetTrigger("Death");
            DisableMovement();
        };
        #endregion

        _myFsm = new EventFSM<RSInputs>(thinking);

        hasBeenInstantiated = true;
    }

    void Start()
    {
        _player = ServiceLocator.Instance.GetDependency<PlayerMVC>();
        _playerLife = ServiceLocator.Instance.GetDependency<PlayerLife>();

        if (ServiceLocator.Instance.TryGetDependency<KarmicToggle>(out KarmicToggle karmic))
        {
            //Debug.Log("Me suscribi a karmic OnMenu");
            karmic.OnMenuEnabled += StayIdle;
            karmic.OnMenuDisabled += ExitFromIdle;
        }
    }

    void OnDestroy()
    {
        if (ServiceLocator.Instance.TryGetDependency<KarmicToggle>(out KarmicToggle karmic))
        {
            karmic.OnMenuEnabled -= StayIdle;
            karmic.OnMenuDisabled -= ExitFromIdle;
        }
    }

    void StayIdle()
    {
        canThink = false;
    }
    void ExitFromIdle()
    {
        canThink = true;
    }

    //LO MISMO QUE EN EL AWAKE PERO SI YA SE INSTANCIO LO VUELVO A HACER PARA QUE NO PIERDA LOS ESTADOS
    void OnEnable()
    {
        if (!hasBeenInstantiated) return;

        _currentSpeed = _speed;
        _currentRotation = _rotationSpeed;
        _enemyLife.OnHit += OnHitTrue; //LE AGREGO EL ONHITTRUE Y CUANDO TERMINO DE HACER LA ANIMACION SE VUELVE FALSE ASI NO VUELVE A ONHIT

        _myRb = GetComponent<Rigidbody>();
        _anim = GetComponent<Animator>();
        _enemyLife = GetComponent<EnemyLife>();

        #region States Declaration
        //var thinking = new State<RSInputs>("THINKING");
        //var idle = new State<RSInputs>("IDLE");
        //var follow = new State<RSInputs>("FOLLOW");
        //var attack = new State<RSInputs>("ATTACK");
        //var hitstun = new State<RSInputs>("HITSTUN");
        //var die = new State<RSInputs>("DIE");

        thinking = new State<RSInputs>("THINKING");
        idle = new State<RSInputs>("IDLE");
        follow = new State<RSInputs>("FOLLOW");
        attack = new State<RSInputs>("ATTACK");
        hitstun = new State<RSInputs>("HITSTUN");
        die = new State<RSInputs>("DIE");

        #endregion
        #region StateConfigurer States currently 9 States
        //THINKING VA A PODER PASAR A TODOS
        //Y LUEGO TODOS VAN A PODER PASAR A THINKING TAMBIEN ASI YO PUEDO DARLE UN CD AL PASAR DE ESTADOS
        //ATTACK Y FOLLLOW PASAN A HITSTUN
        //SOLO HITSTUN PUEDE PASAR A DEATH
        StateConfigurer.Create(thinking)
            .SetTransition(RSInputs.IDLE, idle)
            .SetTransition(RSInputs.FOLLOW, follow)
            .SetTransition(RSInputs.ATTACK, attack)
            .SetTransition(RSInputs.HITSTUN, hitstun)
            .Done();

        StateConfigurer.Create(idle)
            .SetTransition(RSInputs.FOLLOW, follow)
            .Done();

        StateConfigurer.Create(follow)
            .SetTransition(RSInputs.IDLE, idle)
            .SetTransition(RSInputs.ATTACK, attack)
            .SetTransition(RSInputs.HITSTUN, hitstun)
            .Done();

        StateConfigurer.Create(attack)
            .SetTransition(RSInputs.THINKING, thinking)
            .SetTransition(RSInputs.HITSTUN, hitstun)
            .Done();

        StateConfigurer.Create(hitstun)
            .SetTransition(RSInputs.HITSTUN, hitstun)
            .SetTransition(RSInputs.THINKING, thinking)
            .SetTransition(RSInputs.DIE, die)
            .Done();

        StateConfigurer.Create(die)
            .Done();
        #endregion

        #region StatesLogicAndTransitions
        //IDLE
        idle.OnEnter += x =>
        {
            _anim.SetBool("Idle", true);
            //Debug.Log("Entr� a IDLE");
        };
        idle.OnUpdate += () =>
        {
            if (isOnFollowRange) SendInputToFSM(RSInputs.FOLLOW);
            if (isSlowed) SendInputToFSM(RSInputs.THINKING);
        };
        idle.OnExit += x => _anim.SetBool("Idle", false);
        //FOLLOW
        follow.OnEnter += x =>
        {
            _anim.SetBool("Follow", true);
            //Debug.Log("Entr� a FOLLOW");
        };
        follow.OnUpdate += () =>
        {
            //LookAtParameterOnY(_player.transform); //ROTAR HACIA EL PARAMETRO EN Y
            LookAtParameterOnY(_player.transform); //ROTAR HACIA EL PARAMETRO EN Y
            if (!isOnFollowRange) SendInputToFSM(RSInputs.IDLE);
            if (isOnAttackRange) SendInputToFSM(RSInputs.ATTACK);
            //Debug.Log("la FSM Funciona");
            if (isSlowed) SendInputToFSM(RSInputs.THINKING);

        };
        follow.OnFixedUpdate += () =>
        {
            _myRb.position += GetSteerToParameter(_player.transform, _speed);
        };
        follow.OnExit += x => _anim.SetBool("Follow", false);
        //ATTACK
        attack.OnEnter += x =>
        {
            _anim.SetTrigger("Attack");
            animationFinished = false;
            DisableMovement();
        };
        attack.OnUpdate += () =>
        {
            if (animationFinished) SendInputToFSM(RSInputs.THINKING);
            if (isSlowed) SendInputToFSM(RSInputs.THINKING);
        };
        attack.OnExit += x => EnableMovement();
        //THINKING
        thinking.OnUpdate += () =>
        {
            if (GenericCounter(timeToRest))
            {
                if (isOnAttackRange) SendInputToFSM(RSInputs.ATTACK);
                if (isOnFollowRange) SendInputToFSM(RSInputs.FOLLOW);
                else SendInputToFSM(RSInputs.IDLE);
            }
        };
        thinking.OnExit += x => ResetCounter();
        //HITSTUN
        hitstun.OnEnter += x =>
        {
            animationFinished = false;
            _anim.SetTrigger("OnHit");
        };
        hitstun.OnUpdate += () =>
        {
            if (animationFinished) SendInputToFSM(RSInputs.THINKING);
            if (_enemyLife.Life <= 0) SendInputToFSM(RSInputs.DIE);
            if (isSlowed) SendInputToFSM(RSInputs.THINKING);
        };
        hitstun.OnExit += x => hasBeenHit = false;
        //DIE
        die.OnEnter += x =>
        {
            //_anim.SetTrigger("Death");
            DisableMovement();
        };
        #endregion

        _myFsm = new EventFSM<RSInputs>(thinking);
    }

    public void DamagePlayerAnimMethod()
    {
        //if (_player != null && isOnAttackRange) _player.Model.TakeDamage(dmg);
        if (_player != null && isOnAttackRange) //_player.Model.TakeDamage(dmg);
        {
            _playerLife.TakeDamage(dmg);
            if (_playerBlood != null)
            {
                _playerBlood.SetVector3("EnemyPos", new Vector3(transform.position.x, transform.position.y + .5f, transform.position.z));
                _playerBlood.SendEvent("BloodSplatter");
            }
        }
    }
    void AnimationFinishedFalse() { animationFinished = false; }
    void AnimationFinishedTrue() { animationFinished = true; }
    public void DisableMovement()
    {
        _currentSpeed = 0;
        _currentRotation = 0;
        //gameObject.layer = 10;
    }
    void EnableMovement()
    {
        _currentSpeed = _speed;
        _currentRotation = _rotationSpeed;
        //gameObject.layer = 10;
    }
    bool GenericCounter(float time)
    {
        //variable que va a ir de cero a time, mientras que no es igual a time no se vuelve true
        if (counter != time)
        {
            //Debug.Log($"tiempo de contador: {counter}");
            counter += Time.deltaTime;
            counter = Mathf.Clamp(counter, 0, time);
            return false;
        }
        else
        {
            //Debug.Log("termine");
            return true;
        }
    }
    void ResetCounter() { counter = 0; }
    //void HitboxOn() { attackPrefab.SetActive(true); }
    //void HitboxOff() { attackPrefab.SetActive(false); }
    float DistanceToParameter(Transform myTransform, Transform parameter)
    {
        return Vector3.Distance(myTransform.position, parameter.position);
    }
    Vector3 GetSteerToParameter(Transform parameter, float speed)
    {
        //DIR TO PLAYER
        Vector3 dirToTarget = new Vector3(parameter.transform.position.x, 0, parameter.transform.position.z) - new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 desiredVelocity = dirToTarget.normalized * speed * Time.fixedDeltaTime;// velocidad deseada = direccion normalizada * aceleracion
        Vector3 steering = desiredVelocity - _currentVelocity;// correccion de velocidad = velocidad deseada - velocidad actual
        _currentVelocity += steering;// a la velocidad actual se le suma la correccion 
        return _currentVelocity *= Mathf.Clamp01(dirToTarget.magnitude / steeringRadius);
    }
    //Vector3 GetDir() => _player.transform.position - transform.position;
    Vector3 GetDir() => _player.transform.position - transform.position;

    void LookAtParameterOnY(Transform param)
    {
        Vector3 dir = GetDir();
        dir.y = 0;
        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
        }
    }
    void LookAtParameter(Transform parameter)
    {
        Vector3 dir = (parameter.position - transform.position).normalized;
        dir.y = 0;
        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
        }
    }
    public void SendInputToFSM(RSInputs inp)
    {
        //Debug.Log(inp);
        _myFsm.SendInput(inp);
    }
    float nextCheckTime = 0f;
    float checkInterval = 0.5f;
    private void Update()
    {
        if (!canThink) 
        {
            SendInputToFSM(RSInputs.IDLE);
            //Debug.Log("NO PUEDO PENSAAAAAR");
            return;
        }

        if (Time.time >= nextCheckTime)
        {
            nextCheckTime = Time.time + checkInterval;
            ConditionChecker();
        }
        _myFsm.Update();
        //Debug.Log($"FSM Update � estado actual: {_myFsm.Current?.ToString()} | followRange: {isOnFollowRange} | attackRange: {isOnAttackRange}");
    }
    void OnHitTrue() 
    { 
        hasBeenHit = true;
        SendInputToFSM(RSInputs.HITSTUN);
    }
    
    private void ConditionChecker()
    {
        //isOnFollowRange = DistanceToParameter(transform, _player.transform) < followRadius;
        isOnFollowRange = DistanceToParameter(transform, _player.transform) < followRadius;
        //isOnAttackRange = DistanceToParameter(transform, _player.transform) < attackRadius;
        isOnAttackRange = DistanceToParameter(transform, _player.transform) < attackRadius;

        isDeath = _enemyLife.Life <= 0;
        //isPlayerDeath = _playerLife.Life <= 0;
        isPlayerDeath = _playerLife.Life <= 0;
    }
    private void FixedUpdate()
    {
        _myFsm.FixedUpdate();
    }
    void LateUpdate()
    {
        _myFsm.LateUpdate();
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, followRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }

    //CHELO WAS HERE
    public void ResetFSM()
    {


        // Restaurar movimiento y rotaci�n
        _currentSpeed = _speed;
        _currentRotation = _rotationSpeed;

        // Opcional: resetear animaciones
        _anim.ResetTrigger("Death");
        _anim.ResetTrigger("OnHit");
        _anim.ResetTrigger("Idle");
        //_anim.Play("Idle");
        _anim.enabled = false;
        _anim.enabled = true;
        _anim.Play("Idle", 0, 0f);

        // Variables l�gicas
        isDeath = false;
        hasBeenHit = false;
        animationFinished = false;
        counter = 0f;


        // Resetear el FSM al estado inicial
        // Puede que tu FSM tenga un m�todo como Restart o re-inicializar el objeto
        //_myFsm = new EventFSM<RSInputs>(THINKING);
        _myFsm = new EventFSM<RSInputs>(thinking);


        //SendInputToFSM(RSInputs.THINKING);

        ConditionChecker();
        if (isOnAttackRange) SendInputToFSM(RSInputs.ATTACK);
        else if (isOnFollowRange) SendInputToFSM(RSInputs.FOLLOW);
        else SendInputToFSM(RSInputs.IDLE);


        //gameObject.SetActive(false);
        //gameObject.SetActive(true);

        //Debug.Log("RestlessSoul reiniciado");

        //Debug.Log($"[RestlessSoul] FSM actual tras reset: {_myFsm?.Current}");

        //if (_myFsm == null)  Debug.LogError("�_myFsm sigue siendo null tras ResetFSM!");

        //EnableMovement();
    }

    #region SLOWABLE
    float originalSpeed;
    bool isSlowed;
    public void SlowEntity()
    {
        isSlowed = true;
        originalSpeed = _speed;
        _speed = 0;
        _anim.speed = 0;

        Invoke("UnSlowEntity", 5f);
    }

    public void UnSlowEntity()
    {
        isSlowed = false;
        _speed = originalSpeed;
        _anim.speed = 1;
    }
    #endregion
}