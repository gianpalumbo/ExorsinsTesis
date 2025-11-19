using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FatAlly : MonoBehaviour
{
//    public enum EliteEnemyInputs { IDLE, TANKFOLLOW, REST, SLOWATTACK, FRENZY, FRENZYFOLLOW, FRENZYATTACK, FRENZYHEAL, DIE, HITSTUN }
//    private EventFSM<EliteEnemyInputs> _myFsm;
//    private Rigidbody _myRb;
//    [Header("References")]
//    [SerializeField] Animator _anim;
//    AnimatorStateInfo animInfo;
//    [SerializeField] float normTime;
//    [SerializeField] float counter;
//    public float DmgSlow { get { return dmgSlow; } }
//    public float DmgFrenzy { get { return dmgFrenzy; } }
//    public float SpeedFrenzy { get { return _speed * frenzySpeedMultiplier; } }
//    public GameObject slowAttackPrefab, frenzyAttackPrefab;
//    public Renderer _myRen;
//    Vector3 _currentVelocity = Vector3.zero;
//    [SerializeField] PlayerMVC _player;
//    private EnemyLife _enemyLife;
//    [SerializeField] private int _hitCounter;
//    public bool animationFinished, hasBeenHit, canEat;
//    float shortestDistance;
//    Transform closestWaypoint;

//    [Header("Elite Enemy Values")]
//    [SerializeField] float dmgSlow;
//    [SerializeField] float dmgFrenzy;
//    [SerializeField] float _followRadiusSlow;
//    [SerializeField] float _attackRadiusSlow;
//    [SerializeField] float _attackRadiusFrenzy;
//    [SerializeField] float _speed; //Frenzy speed va a ser Speed multiplicado por tanto
//    [SerializeField] float frenzySpeedMultiplier = 1.5f;
//    [SerializeField] float _rotationSpeed;
//    [SerializeField] float steeringRadius = 1;
//    [SerializeField] float timeToRest;
//    [SerializeField] float timeToRestartHitCounter;
//    [SerializeField] float healMultiplier;
//    [SerializeField] int hitsToStun;
//    [SerializeField] Material originalMat;
//    [SerializeField] Material frenzyMat;

//    [Header("Meat Waypoints")]
//    [SerializeField] Transform[] meatWP = new Transform[7];

//    private void Awake()
//    {
//        _myRb = GetComponent<Rigidbody>();
//        _anim = GetComponent<Animator>();
//        _enemyLife = GetComponent<EnemyLife>();

//        #region States Declaration
//        var idle = new State<EliteEnemyInputs>("IDLE");
//        var tankFollow = new State<EliteEnemyInputs>("TANKFOLLOW");
//        var slowAttack = new State<EliteEnemyInputs>("SLOWATTACK");
//        var rest = new State<EliteEnemyInputs>("REST");
//        var frenzyScream = new State<EliteEnemyInputs>("FRENZY");
//        var frenzyFollow = new State<EliteEnemyInputs>("FRENZYFOLLOW");
//        var frenzyAttack = new State<EliteEnemyInputs>("FRENZYATTACK");
//        var frenzyHeal = new State<EliteEnemyInputs>("FRENZYHEAL");
//        var hitStun = new State<EliteEnemyInputs>("HITSTUN");
//        var die = new State<EliteEnemyInputs>("DIE");
//        #endregion

//        #region StateConfigurer States currently 9 States
//        StateConfigurer.Create(idle) //Idle va a poder pasar a followTank a slowAttack a rest despues de atacar y cuando la vida este menor al 50% va a entrar a FrenzyScream
//            .SetTransition(EliteEnemyInputs.TANKFOLLOW, tankFollow)
//            .SetTransition(EliteEnemyInputs.SLOWATTACK, slowAttack)
//            .Done();

//        StateConfigurer.Create(tankFollow)
//            .SetTransition(EliteEnemyInputs.IDLE, idle)
//            .SetTransition(EliteEnemyInputs.SLOWATTACK, slowAttack)
//            .SetTransition(EliteEnemyInputs.FRENZY, frenzyScream)
//            .Done();

//        StateConfigurer.Create(slowAttack)
//            .SetTransition(EliteEnemyInputs.REST, rest)
//            .Done();

//        StateConfigurer.Create(rest)
//            .SetTransition(EliteEnemyInputs.TANKFOLLOW, tankFollow)
//            .SetTransition(EliteEnemyInputs.FRENZY, frenzyScream)
//            .Done();

//        StateConfigurer.Create(frenzyScream) //ESTE VA A SER COMO UN IDLE, QUE CADA 3 ATAQUES GRITE Y VUELVA A ATACARTE PARA GENERAR UNA PEQUEÑA VENTANA PARA ATACAR
//            .SetTransition(EliteEnemyInputs.FRENZYFOLLOW, frenzyFollow)
//            .SetTransition(EliteEnemyInputs.FRENZYATTACK, frenzyAttack)
//            .Done();

//        StateConfigurer.Create(frenzyFollow)
//            .SetTransition(EliteEnemyInputs.FRENZYATTACK, frenzyAttack)
//            .SetTransition(EliteEnemyInputs.FRENZYHEAL, frenzyHeal)
//            .SetTransition(EliteEnemyInputs.HITSTUN, hitStun)
//            .SetTransition(EliteEnemyInputs.DIE, die)
//            .Done();

//        StateConfigurer.Create(frenzyAttack)
//            .SetTransition(EliteEnemyInputs.FRENZYFOLLOW, frenzyFollow)
//            .SetTransition(EliteEnemyInputs.FRENZYHEAL, frenzyHeal)
//            .SetTransition(EliteEnemyInputs.HITSTUN, hitStun)
//            .SetTransition(EliteEnemyInputs.DIE, die)
//            .Done();

//        StateConfigurer.Create(frenzyHeal)
//            .SetTransition(EliteEnemyInputs.FRENZYFOLLOW, frenzyFollow)
//            .SetTransition(EliteEnemyInputs.FRENZYATTACK, frenzyAttack)
//            .SetTransition(EliteEnemyInputs.DIE, die)
//            .Done();

//        StateConfigurer.Create(hitStun)
//            .SetTransition(EliteEnemyInputs.FRENZYHEAL, frenzyHeal)
//            .SetTransition(EliteEnemyInputs.FRENZYFOLLOW, frenzyFollow)
//            .SetTransition(EliteEnemyInputs.FRENZYATTACK, frenzyAttack)
//            .SetTransition(EliteEnemyInputs.DIE, die)
//            .Done();

//        StateConfigurer.Create(die).Done();
//        #endregion

//        //AGREGAR ANIMATIONFINISHED A GETUP
//        #region IDLE
//        idle.OnUpdate += () =>
//        {
//            if (Input.GetKeyDown(KeyCode.C))
//                SendInputToFSM(EliteEnemyInputs.FRENZYHEAL);

//            if (Vector3.Distance(transform.position, _player.transform.position) <= _followRadiusSlow)
//                SendInputToFSM(EliteEnemyInputs.TANKFOLLOW);
//        };
//        idle.OnExit += x => { _anim.SetTrigger("GetUp"); };
//        #endregion

//        #region TANKFOLLOW
//        tankFollow.OnEnter += x =>
//        {
//            _anim.SetBool("isWalking", true);
//        };
//        tankFollow.OnUpdate += () =>
//        {
//            LookAtPlayerOnY(_player.transform);
//            if ((_enemyLife.Life / _enemyLife.MaxLife) < .5f) SendInputToFSM(EliteEnemyInputs.FRENZY); //SI TENGO MENOS DEL 50% ENTRO EN MODO FRENZY
//            if (GetDistanceToParameterFrom(transform, _player.transform) < _attackRadiusSlow) SendInputToFSM(EliteEnemyInputs.SLOWATTACK);
//        };
//        tankFollow.OnFixedUpdate += () =>
//        {       //TEMPORALMENTE VA A IR DIRECTO CON VELOCITY
//            if (!animationFinished) return;
//            _myRb.position += GetSteerToParameter(_player.transform, _speed);
//        };
//        tankFollow.OnExit += x => { _anim.SetBool("isWalking", false); };
//        #endregion

//        //AGREGAR ANIMATIONFINISHED
//        #region SLOW ATTACK
//        slowAttack.OnEnter += x =>
//        {
//            animationFinished = false;
//            this.gameObject.layer = LayerMask.NameToLayer("Invulnerable");
//            Debug.Log(gameObject.layer);
//            _anim.SetTrigger("Attack");
//        };
//        slowAttack.OnUpdate += () =>
//        {
//            if (animationFinished)
//                SendInputToFSM(EliteEnemyInputs.REST);
//        };
//        slowAttack.OnExit += x =>
//        {
//            ResetCounter();
//            this.gameObject.layer = LayerMask.NameToLayer("Enemies");

//        };
//        #endregion

//        #region REST
//        //rest.OnEnter += x => { _anim.SetTrigger("Rest"); };
//        rest.OnUpdate += () =>
//        {
//            if ((_enemyLife.Life / _enemyLife.MaxLife) < .5f) SendInputToFSM(EliteEnemyInputs.FRENZY); //SI TENGO MENOS DEL 50% ENTRO EN MODO FRENZY
//            if (GenericCounter(timeToRest)) //con el genericCounter usar resetCounter
//            {
//                SendInputToFSM(EliteEnemyInputs.TANKFOLLOW);
//            }
//        };
//        rest.OnExit += x =>
//        {
//            Debug.Log("ENTRE ACA");
//            animationFinished = true;
//            ResetCounter();
//        };
//        #endregion

//        //AGREGAR ANIMATIONFINISHED
//        #region FRENZY SCREAM
//        frenzyScream.OnEnter += x => {
//            _enemyLife.OnHit += OnHitFrenzyMode;
//            animationFinished = false;
//            _myRen.material = frenzyMat;
//            _anim.SetTrigger("FrenzyScream");
//        };
//        frenzyScream.OnUpdate += () => {
//            LookAtPlayerOnY(_player.transform);
//            if (animationFinished) SendInputToFSM(EliteEnemyInputs.FRENZYFOLLOW);
//        };
//        #endregion

//        #region FRENZY FOLLOW 
//        frenzyFollow.OnEnter += x =>
//        {
//            _anim.SetBool("isRunning", true);
//        };
//        frenzyFollow.OnUpdate += () => {
//            LookAtPlayerOnY(_player.transform);
//            //Paso a un OnHit que tiene que terminarse para pasar de estado y chequear a donde ir
//            if (_hitCounter >= hitsToStun)
//                SendInputToFSM(EliteEnemyInputs.HITSTUN);
//            if (hasBeenHit)
//            {
//                if (GenericCounter(timeToRestartHitCounter))
//                {
//                    hasBeenHit = false;
//                }
//            }
//            if (GetDistanceToParameterFrom(transform, _player.transform) < _attackRadiusFrenzy)
//                SendInputToFSM(EliteEnemyInputs.FRENZYATTACK);
//        };
//        frenzyFollow.OnFixedUpdate += () => { _myRb.position += GetSteerToParameter(_player.transform, _speed * frenzySpeedMultiplier); };
//        frenzyFollow.OnExit += x => _anim.SetBool("isRunning", false);
//        #endregion

//        //AGREGAR ANIMATIONFINISHED
//        #region FRENZY ATTACK/SWIPE
//        frenzyAttack.OnEnter += x => {
//            animationFinished = false;
//            _anim.SetTrigger("FrenzySwipe");
//        };
//        frenzyAttack.OnUpdate += () => {
//            //Paso a un OnHit que tiene que terminarse para pasar de estado y chequear a donde ir
//            if (_hitCounter >= hitsToStun)
//                SendInputToFSM(EliteEnemyInputs.HITSTUN);
//            if (hasBeenHit)
//            {
//                if (GenericCounter(timeToRestartHitCounter))
//                {
//                    hasBeenHit = false;
//                }
//            }
//            if (animationFinished)
//            {
//                if (GetDistanceToParameterFrom(transform, _player.transform) > _attackRadiusFrenzy)
//                    SendInputToFSM(EliteEnemyInputs.FRENZYFOLLOW);
//                else
//                    SendInputToFSM(EliteEnemyInputs.FRENZYATTACK);
//            }
//        };
//        #endregion

//        //AGREGAR ANIMATIONFINISHED
//        #region FRENZYHEAL
//        //EN EL ENTER HACE EL CALCULO AL CLOSESTWAYPOINT
//        frenzyHeal.OnEnter += x =>
//        {
//            animationFinished = false;
//            _anim.SetBool("isRunning", true);
//            shortestDistance = Mathf.Infinity;
//            for (int i = 0; i < meatWP.Length; i++)
//            {
//                var currentDistance = GetDistanceToParameterFrom(transform, meatWP[i]);
//                if (currentDistance < shortestDistance)
//                {
//                    shortestDistance = currentDistance;
//                    closestWaypoint = meatWP[i];
//                }
//            }
//        };
//        //EN EL UPDATE CUANDO CAN EAT ES TRUE MIRO A LA CARNE Y ME PONGO A COMER
//        //CUANDO LA ANIMACION TERMINA PASO A FOLLOW Y CANEAT ES FALSO PARA CUANDO VUELVO A ENTRAR
//        frenzyHeal.OnUpdate += () =>
//        {
//            Debug.Log(GetDistanceToParameterFrom(transform, closestWaypoint));
//            LookAtParameter(closestWaypoint);
//            if (GetDistanceToParameterFrom(transform, closestWaypoint) < 1f)
//            { //SI ESTOY A MENOS DE .5F PUEDO COMER Y CORTO EL RUNNING
//                canEat = true;
//            }

//            if (canEat)
//            {
//                _anim.SetBool("isRunning", false);
//                _anim.SetBool("CanEat", true);
//                hasBeenHit = false;
//                HealForSeconds(healMultiplier);
//            }

//            if (animationFinished)
//            {
//                _anim.SetBool("CanEat", false);

//                if (GetDistanceToParameterFrom(transform, _player.transform) < _attackRadiusFrenzy)
//                    SendInputToFSM(EliteEnemyInputs.FRENZYATTACK);
//                else
//                    SendInputToFSM(EliteEnemyInputs.FRENZYFOLLOW);
//            }
//        };
//        //ACA SOLAMENTE VOY HACIA EL WAYPOINT MAS CERCANO EN VELOCIDAD FRENZY
//        //SI LA DISTANCIA ES MENOR A .2F ENTONCES ME PARO A COMER
//        frenzyHeal.OnFixedUpdate += () =>
//        {
//            _myRb.position += GetSteerToParameter(closestWaypoint, _speed * frenzySpeedMultiplier);
//        };
//        frenzyHeal.OnExit += x => _anim.SetBool("CanEat", false);
//        #endregion

//        //AGREGAR ANIMATIONFINISHED
//        #region HITSTUN
//        hitStun.OnEnter += x =>
//        {
//            _hitCounter = 0;
//            animationFinished = false;
//            _anim.SetTrigger("HitStun");
//        };
//        hitStun.OnUpdate += () =>
//        {
//            if (animationFinished)
//            {
//                if ((_enemyLife.Life / _enemyLife.MaxLife) < .25f)
//                    SendInputToFSM(EliteEnemyInputs.FRENZYHEAL);
//                else if (GetDistanceToParameterFrom(transform, _player.transform) < _attackRadiusFrenzy)
//                    SendInputToFSM(EliteEnemyInputs.FRENZYATTACK);
//                else
//                    SendInputToFSM(EliteEnemyInputs.FRENZYFOLLOW);
//            }
//        };
//        #endregion 

//        #region DIE
//        die.OnEnter += x =>
//        {
//            _anim.SetTrigger("Die");
//            DeathMethod();
//        };
//        #endregion

//        _myFsm = new EventFSM<EliteEnemyInputs>(idle);
//    }

//    void HealForSeconds(float healMultiplier)
//    {
//        _enemyLife.Life += Time.deltaTime * healMultiplier;
//    }
//    void OnHitFrenzyMode()
//    {
//        _hitCounter++;
//        hasBeenHit = true;
//    }
//    public void DeathMethod()
//    {
//        _speed = 0;
//        _rotationSpeed = 0;
//        gameObject.layer = 10;
//    }
//    void AnimationNotFinished() { animationFinished = false; }
//    void AnimationFinished() { animationFinished = true; }
//    bool GenericCounter(float time)
//    {
//        //variable que va a ir de cero a time, mientras que no es igual a time no se vuelve true
//        if (counter != time)
//        {
//            //Debug.Log($"tiempo de contador: {counter}");
//            counter += Time.deltaTime;
//            counter = Mathf.Clamp(counter, 0, time);
//            return false;
//        }
//        else
//        {
//            Debug.Log("termine");
//            return true;
//        }
//    }
//    void ResetCounter() { counter = 0; }
//    void HitboxOn() { slowAttackPrefab.SetActive(true); }
//    void HitboxOff() { slowAttackPrefab.SetActive(false); }
//    void HitboxFrenzyOn() { frenzyAttackPrefab.SetActive(true); }
//    void HitboxFrenzyOff() { frenzyAttackPrefab.SetActive(false); }
//    float GetDistanceToParameterFrom(Transform myTransform, Transform parameter)
//    {
//        return Vector3.Distance(myTransform.position, parameter.position);
//    }
//    Vector3 GetSteerToParameter(Transform parameter, float speed)
//    {
//        //DIR TO PLAYER
//        Vector3 dirToTarget = new Vector3(parameter.transform.position.x, 0, parameter.transform.position.z) - new Vector3(transform.position.x, 0, transform.position.z);
//        Vector3 desiredVelocity = dirToTarget.normalized * speed * Time.fixedDeltaTime;// velocidad deseada = direccion normalizada * aceleracion
//        Vector3 steering = desiredVelocity - _currentVelocity;// correccion de velocidad = velocidad deseada - velocidad actual
//        _currentVelocity += steering;// a la velocidad actual se le suma la correccion 
//        return _currentVelocity *= Mathf.Clamp01(dirToTarget.magnitude / steeringRadius);
//    }
//    Vector3 GetDir() => _player.transform.position - transform.position;
//    void LookAtPlayerOnY(Transform player)
//    {
//        Vector3 dir = GetDir();
//        dir.y = 0;
//        if (dir.sqrMagnitude > 0.001f)
//        {
//            Quaternion targetRotation = Quaternion.LookRotation(dir);
//            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
//        }
//    }
//    void LookAtParameter(Transform parameter)
//    {
//        Vector3 dir = (parameter.position - transform.position).normalized;
//        dir.y = 0;
//        if (dir.sqrMagnitude > 0.001f)
//        {
//            Quaternion targetRotation = Quaternion.LookRotation(dir);
//            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
//        }
//    }
//    public void SendInputToFSM(EliteEnemyInputs inp)
//    {
//        Debug.Log(inp);
//        _myFsm.SendInput(inp);
//    }
//    private void Update()
//    {
//        _myFsm.Update();
//    }
//    private void FixedUpdate()
//    {
//        _myFsm.FixedUpdate();
//    }
//    void LateUpdate()
//    {
//        _myFsm.LateUpdate();
//    }
//    private void OnDrawGizmos()
//    {
//        Gizmos.color = Color.magenta;
//        Gizmos.DrawWireSphere(transform.position, _followRadiusSlow);
//        Gizmos.color = Color.red;
//        Gizmos.DrawWireSphere(transform.position, _attackRadiusSlow);
//    }
}
