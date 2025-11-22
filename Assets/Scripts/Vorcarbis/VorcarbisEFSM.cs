using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IA2;
using System;
using JetBrains.Annotations;
using TMPro;
using UnityEngine.VFX;

public class VorcarbisEFSM : MonoBehaviour , ISlowable
{
    public enum VInputs
    {
        THINKING,
        REST,
        FOLLOW,
        LIGHTATTK,
        HITSTUN,
        SMOKESHIELD,
        FRONTATTACK,
        VOMITATTACK,
        DEATH
    }
    private EventFSM<VInputs> _myFsm;
    //[SerializeField] private EnemyLife _enemyLife;
    //[SerializeField] private Entity _enemyLife; //CHELO WAS HERE: NO PUEDO HACERLO CON ENTITY PORQUE ENTITY CARECE DE COMPONENTES QUE SI TIENE BOSSLIFE
    [SerializeField] private BossLife _enemyLife;
    private Rigidbody _myRb;
    private Renderer _myRen;
    private Animator _anim;
    PlayerLife player;

    //CHELO WAS HERE: AGREGO DEPENDENCIA DE SERVICE LOCATOR ISRESTING PARA APAGAR EL THINKING
    public bool isResting = false;

    #region PRECONDITIONS AND OBJECTIVES
    //PRECONDITIONS
    public bool _isPlayerOnSight,
         _isPlayerResting,
         _isPlayerOnAttkRange,
         _amIDead,
         _amIHurt,
         _amIStunned,
         _smokeShieldActive = false,
         _canSmokeShield,
         _haveToRest = false,
         _isPlayerFarAway,
        _canVomit,
        _isPlayerOnSightVomit;

    //OBJECTIVES
    bool _isPlayerDead;
    bool[] allPreconditions;
    string[] allPreconditionsNames;
    #endregion

    #region VARIABLES
    [Header("<color=green>VARIABLES</color>")]
    //public TextMeshProUGUI currentStateTMP;
    float _counter = 0, _smokeCounter= 0, _vomitCounter = 3; //LE PONGO EN TRES PARA QUE AL PRINCIPIO YA TE ATAQUE Y NO SE QUEDE SIN ESTADO
    [SerializeField] float _counterToRest = 5f;
    [SerializeField] int hitCounterToRest = 3;
    int _restCounter = 0;
    [SerializeField] float _counterSmokeShield = 30f, counterFollow = 4;
    int randomAttack;
    public bool _animationFinished = false, canThink = true, canGetHitstunned = true, _haveIDied = false, comingFromFollow = false;
    bool canTurnOnHitbox = false, canSlamGround = false;
    float _currentDmg;
    Transform target;
    [SerializeField] Transform mouthSpawner;
    [SerializeField] GameObject maceHitbox, smokeHitbox, bileProyectile;
    [SerializeField] GameObject karmicTrigger;
    [SerializeField] GameObject groundSlam;
    [SerializeField] Vector3 offsetYForSight;
    [SerializeField] float _rotationSpeed = 5f;
    public float dmgAttk1 = 22f;
    //[SerializeField] float _dmgFeast = 7f;
    //[SerializeField] int healPerBite = 8;
    [SerializeField] float _speed = 5;
    //[SerializeField] float timeToEat = 1;
    //[SerializeField] float timeToFeast = 1.25f;
    [SerializeField] float dstToAttk = 2f;
    //[SerializeField] float dstToReach = 1f;
    [SerializeField] float followRadius = 8f;
    //[SerializeField] float randomPointTolerance = 4f;
    [SerializeField] float viewDistance = 15f;
    [SerializeField] float angleOfView = 45f;
    [SerializeField] LayerMask obstacles = 1 << 13; //Activo la layer 13 que va a ser obstacles
    [Header("Pursuit Variables")]
    [SerializeField] float _timePrediction = 0.3f;
    [SerializeField] VisualEffect smokeShieldVfx, rageVfx;
    [SerializeField] Material mat;
    bool phaseChanged, isSlowed;
    Vector3 _previousTargetPosition;
    #endregion

    #region BILE FRAMES AND FLAGS
    [Header("BILE VARIABLES (attks to vomit after follow 3 HARDCODED)")]
    [SerializeField] int bileQuantity = 3;
    [SerializeField] float angleOfBile = 45f;
    Vector2 bileRange = new Vector2(0.2f, 0.6f);
    bool isVomiting = false;
    bool[] bileShot;
    float[] bileTimes;
    int bileAfterJumpCounter = 0;
    #endregion

    #region FRONTKICK
    //frame 41/111
    [SerializeField] GameObject pushHitBox;
    bool isKicking = false, isHitboxActive = false, canFrontKick = false;
    #endregion

    #region HITSTUN HANDLER
    [Header("HITSTUN VARIABLES")]
    [SerializeField] int hitsToStun = 3;
    int _hitCounter = 0;
    #endregion


    //CHELO WAS HERE: AGREGO DEPENDENCIA DE SERVICE LOCATOR ISRESTING PARA APAGAR EL THINKING
    private void OnDisable()
    {
        ServiceLocator.Instance.RemoveDependency<VorcarbisEFSM>();
    }
    private void OnDestroy()
    {
        ServiceLocator.Instance.RemoveDependency<VorcarbisEFSM>();
    }

    private void OnEnable()
    {
        //CHELO WAS HERE: AGREGO DEPENDENCIA DE SERVICE LOCATOR ISRESTING PARA APAGAR EL THINKING
        ServiceLocator.Instance.RegisterDependency<VorcarbisEFSM>(this);

        //CHELO WAS HERE: AGREGO DEPENDENCIA DE SERVICE LOCATOR ISRESTING PARA APAGAR EL THINKING
        isResting = true;

        bileShot = new bool[bileQuantity];
        bileTimes = new float[bileQuantity];

        float step = (bileRange.y - bileRange.x) / (bileQuantity - 1);
        for (int i = 0; i < bileQuantity; i++)
        {
            bileTimes[i] = bileRange.x + step * i;
        }


        // _enemyLife = GetComponent<EnemyLife>();
        if (_enemyLife == null) _enemyLife = GetComponent<BossLife>();

        _myRb = GetComponent<Rigidbody>();
        _myRen = GetComponent<Renderer>();
        _anim = GetComponent<Animator>();
        //karmicTrigger3 = GetComponentInChildren<KarmicToggle>(true).gameObject;
        _smokeCounter = _counterSmokeShield; //IGUALO SMOKECOUNTER PARA QUE HAGA DE UNA APENAS QUEDA POR DEBAJO DEL 50%

        smokeShieldVfx.SendEvent("Stop");

        canGetHitstunned = true;

        phaseChanged = false;

        mat.SetFloat("_Rage", 0);

        #region STATES DECLARATION
        var thinking = new State<VInputs>("THINKING");
        var rest = new State<VInputs>("REST");
        var follow = new State<VInputs>("FOLLOW");
        var lightAttk = new State<VInputs>("LIGHTATTK");
        var hitstun = new State<VInputs>("HITSTUN");
        var smokeShield = new State<VInputs>("SMOKESHIELD");
        var frontAttack = new State<VInputs>("FRONTATTACK");
        var vomitAttk = new State<VInputs>("VOMITATTACK");
        var death = new State<VInputs>("DEATH");
        #endregion

        #region STATE CONGIFURER
        StateConfigurer.Create(thinking)
            .SetTransition(VInputs.REST, rest)
            .SetTransition(VInputs.FOLLOW, follow)
            .SetTransition(VInputs.LIGHTATTK, lightAttk)
            .SetTransition(VInputs.HITSTUN, hitstun)
            .SetTransition(VInputs.SMOKESHIELD, smokeShield)
            .SetTransition(VInputs.VOMITATTACK, vomitAttk)
            .SetTransition(VInputs.FRONTATTACK, frontAttack)
            .SetTransition(VInputs.DEATH, death)
            .Done();

        StateConfigurer.Create(rest)
            .SetTransition(VInputs.THINKING, thinking)
            .SetTransition(VInputs.HITSTUN, hitstun)
            .SetTransition(VInputs.DEATH, death)
            .Done();
        StateConfigurer.Create(follow)
            .SetTransition(VInputs.THINKING, thinking)
            .SetTransition(VInputs.HITSTUN, hitstun)
            .SetTransition(VInputs.LIGHTATTK, lightAttk)
            .SetTransition(VInputs.DEATH, death)
            .Done();
        StateConfigurer.Create(lightAttk)
            .SetTransition(VInputs.THINKING, thinking)
            .SetTransition(VInputs.HITSTUN, hitstun)
            .SetTransition(VInputs.VOMITATTACK, vomitAttk)
            .SetTransition(VInputs.DEATH, death)
            .Done();
        StateConfigurer.Create(hitstun)
            .SetTransition(VInputs.THINKING, thinking)
            .SetTransition(VInputs.HITSTUN, hitstun)
            .SetTransition(VInputs.DEATH, death)
            .Done();
        StateConfigurer.Create(smokeShield)
            .SetTransition(VInputs.THINKING, thinking)
            .SetTransition(VInputs.DEATH, death)
            .Done();
        StateConfigurer.Create(frontAttack)
            .SetTransition(VInputs.THINKING, thinking)
            .SetTransition(VInputs.DEATH, death)
            .Done();
        StateConfigurer.Create(vomitAttk)
            .SetTransition(VInputs.THINKING, thinking)
            .SetTransition(VInputs.HITSTUN, hitstun)
            .SetTransition(VInputs.DEATH, death)
            .Done();
        StateConfigurer.Create(death)
            .Done();
        #endregion

        #region STATE LOGIC

        // THINKING
        thinking.OnEnter += x =>
        {
            _anim.SetBool("isIdle", true);
        };
        thinking.OnUpdate += () =>
        {
            //CHELO WAS HERE: AGREGO DEPENDENCIA DE SERVICE LOCATOR ISRESTING PARA APAGAR EL THINKING
            if (isResting == true) return;

            //DEBUG PARA VOMIT ATTK
            if (!canThink) 
            {
                if (Input.GetKeyDown(KeyCode.P) && !isVomiting)
                    SendInputToFSM(VInputs.VOMITATTACK);

                if (Input.GetKeyDown(KeyCode.K) && !isKicking)
                    SendInputToFSM(VInputs.FRONTATTACK);
            }

            if (!canThink || _isPlayerDead) return; //para desactivar update

            if (_haveToRest)
                SendInputToFSM(VInputs.REST); //SI TENGO QUE DESCANSAR, DESCANSO

            if (canFrontKick)
                SendInputToFSM(VInputs.FRONTATTACK); //LUEGO DE REST TENGO UN 33% DE POSIBILIDADES QUE HAGA FRONT KICK Y ME EMPUJE

            if (_canSmokeShield)  //SI ESTOY HERIDO, PUEDO SMOKEAR Y PASO EL CONTADOR, USO SMOKE SHIELD
                SendInputToFSM(VInputs.SMOKESHIELD); //SI ESTOY MAL HERIDO, NO HICE SMOKE Y PASO EL CONTADOR, USO SMOKE SHIELD

            if (_isPlayerOnAttkRange && _isPlayerOnSight && !_haveToRest)   //SI ESTOY CERCA DEL PLAYER, LO ATACO
                SendInputToFSM(VInputs.LIGHTATTK);

            if (_isPlayerOnSight)   //SI VEO AL PLAYER, LO SIGO
                SendInputToFSM(VInputs.FOLLOW);

            if (_isPlayerFarAway && _canVomit) //&& _amIHurt POR AHORA SE LO SACAMOS PARA QUE LO HAGA MAS SEGUIDO
                SendInputToFSM(VInputs.VOMITATTACK);
        };
        thinking.OnExit += x =>
        {
            _anim.SetBool("isIdle", false);
        };
        rest.OnEnter += x =>
        {
            Debug.LogWarning("entre a rest");
            _anim.applyRootMotion = false;
            _anim.SetBool("isIdle", true);
            _myRb.velocity = Vector3.zero;
            _counter = 0;
        };
        rest.OnUpdate += () =>
        {
            if (GenericCounter(_counterToRest))
            {
                Think();
            }
        };
        rest.OnExit += x =>
        {
            _anim.SetBool("isIdle", false);
            ResetCounter();
            _restCounter = 0;
            _haveToRest = false; // YA DESCANSÉ
            int num = UnityEngine.Random.Range(1, 5);
            canFrontKick = num >= 3;
        };
        // FOLLOW
        follow.OnEnter += x =>
        {
            _anim.applyRootMotion = false;
            _anim.SetBool("isWalking", true);
        };
        follow.OnUpdate += () =>
        {
            //LO MIRO Y ME MUEVO HACIA
            LookAtParameterOnY(player.transform, _rotationSpeed * 1.5f);

            //CONTADOR PARA PASAR DIRECTO A LIGHTATTACK (CONFIGURAR TRANSICION EXCEPCION)
            if (GenericCounter(counterFollow)) //CONTADOR POR SI ME ESTA SIGUIENDO MUCHO TIEMPO PASA DIRECTO A LIGHTATTACK Y LE PASO EL ATTACK 3
            {
                comingFromFollow = true;
                SendInputToFSM(VInputs.LIGHTATTK);
            }

            if (_isPlayerOnAttkRange) Think();
            else if (!_isPlayerOnSight) Think();

            if(isSlowed) Think();
        };
        follow.OnFixedUpdate += () =>
        {
            //FollowParamWithRB(player.transform);
            PursuitWithRBV3(player.transform);
        };
        follow.OnExit += x =>
        {
            ResetCounter();
            _anim.SetBool("isWalking", false);
        };
        // LIGHT ATTACK
        lightAttk.OnEnter += x =>
        {
            _anim.applyRootMotion = true;

            //PONER BOOL O ALGUN INDICADOR QUE VENGO DE FOLLOW PARA HACER DIRECTAMENTE ATTACK 3

            if (comingFromFollow)
            {
                bileAfterJumpCounter++; //SUMO AL CONTADOR DE QUE ATACA DE SALTO POR SEGUIRME MUCHO TIEMPO
                randomAttack = 3;
            }
            else randomAttack = UnityEngine.Random.Range(1, 3); //SI VENGO DE FOLLOW RANDOM ATTACK ES 3 Y HAGO ATTACK DE SALTO
            //_enemyLife.isInvulnerable = true;

            _restCounter++; // cada vez que ataco, sumo 1 al contador de ataques para descansar
            Debug.Log($"PEGUE {_restCounter} VECES");
            if (_restCounter >= hitCounterToRest) // si llegué a 3 ataques, tengo que descansar
            {
                _haveToRest = true;
                Debug.Log($"TENGO QUE DESCANSAR: {_haveToRest}");
                _restCounter = 0; // reseteo el contador
            }

            _anim.SetTrigger("LightAttack" + randomAttack);
            _myRb.velocity = Vector3.zero;
        };
        lightAttk.OnUpdate += () =>
        {
            var p = GetStateProgress($"LightAttack{randomAttack}");

            // Ventana solo si REALMENTE estás en LightAttack1
            if (randomAttack == 1)
            { canTurnOnHitbox = p.inState && p.t01 >= 0.33f && p.t01 <= 0.66f; }
            else if (randomAttack == 2)
            { canTurnOnHitbox = p.inState && p.t01 >= 0.42f && p.t01 <= 0.46f; }
            else // randomAttack == 3
            {
                canTurnOnHitbox = p.inState && p.t01 >= 0.48f && p.t01 <= 0.75f;
                canSlamGround = p.inState && p.t01 >= 0.48f && p.t01 <= 0.75f;
            }

            if(canSlamGround)
            {
                canSlamGround = false;
                groundSlam.SetActive(true);
            }

            maceHitbox.SetActive(canTurnOnHitbox);

            if (p.finished && bileAfterJumpCounter < 3)
                Think();
            else if (p.finished && bileAfterJumpCounter >= 3)
            {
                bileAfterJumpCounter = 0; //REINICIAMOS CONTADOR PARA QUE HAGA EL VOMITO
                SendInputToFSM(VInputs.VOMITATTACK);
            }

            if (isSlowed) Think();
        };
        lightAttk.OnExit += x =>
        {
            groundSlam.SetActive(false);
            comingFromFollow = false; //DESACTIVO SINO HACE SIEMPRE ATAQUE 3
            canGetHitstunned = true;
            _anim.ResetTrigger("LightAttack" + randomAttack);
            maceHitbox.SetActive(false); // por las dudas, cerrá la hitbox al salir
        };
        // HITSTUN
        hitstun.OnEnter += x =>
        {
            _hitCounter = 0; //REINICIO COUNTER DE HITS
            _anim.applyRootMotion = true;
            _anim.SetTrigger("Hitstun");
            _myRb.velocity = Vector3.zero;
        };
        hitstun.OnUpdate += () =>
        {
            var p = GetStateProgress("Hitstun");

            if (p.finished)
                Think();

            if (isSlowed) Think();
        };
        hitstun.OnExit += x =>
        {
            //_anim.ResetTrigger("Hitstun");
        };
        //SMOKESHIELD
        smokeShield.OnEnter += x =>
        {
            _smokeShieldActive = true;
            _anim.SetTrigger("SmokeShield"); //ACTIVO ANIMACION DE SMOKE SHIELD
            _myRb.velocity = Vector3.zero;
            _enemyLife.isInvulnerable = true;
            smokeHitbox.GetComponent<SmokeHitbox>().HasDisipated = false;
            smokeShieldVfx.SendEvent("OnPlay");
            if (!phaseChanged)
            {
                phaseChanged = true;
                rageVfx.SendEvent("Play");
                mat.SetFloat("_Rage", 1);
            }
            //ACTIVO EL VISUAL EFFECT
            //ME HAGO INVULNERABLE
        };
        smokeShield.OnUpdate += () =>
        {
            var p = GetStateProgress("SmokeShield"); //EL ESTADO DEBE LLAMARSE ASI

            //en 0.53 prender collider que hace daño de veneno
            if (p.inState && p.t01 > .53f)
                smokeHitbox.SetActive(true);

            if (smokeHitbox.GetComponent<SmokeHitbox>().HasDisipated)  //CUANDO TERMINE LA ANIMACION, VUELVO A THINKING
            {
                //DENTRO DE ESTA HITBOX VA A ESTAR EL SCRIPT DE HACERLE TANTO DAÑO AL PLAYER CADA TANTO Y DE APAGARSE A SI MISMA\
                _canSmokeShield = false;
                Think();
            }
        };
        smokeShield.OnExit += x =>
        {
            _anim.ResetTrigger("SmokeShield");
            _smokeShieldActive = false;
            _enemyLife.isInvulnerable = false;
            //DESACTIVO EL VISUAL EFFECT
            //VUELVO A SER VULNERABLE
            //RESTTEO EL CONTADOR DEL SMOKE SHIELD, PORQUE IGUAL YO SIGO amIHurt y DESACTIVE EL SMOKE CUANDO TERMIN
            ResetSmokeShieldCounter(); //EMPIEZA DE 0 EL CONTADOR PARA PODER USARLO OTRA VEZ
            smokeShieldVfx.SendEvent("Stop");
        };
        //BELLY ATTK
        frontAttack.OnEnter += x =>
        {
            canFrontKick = false;
            _anim.applyRootMotion = true;
            _anim.SetTrigger("FrontKick");
            _myRb.velocity = Vector3.zero;
            isKicking = true;
        };
        frontAttack.OnUpdate += () =>
        {
            LookAtParameterOnY(player.transform, _rotationSpeed * 2);

            var p = GetStateProgress("FrontKick");

            if (p.inState && p.t01 >= (41f / 111f)) //en el frame 41/111 quiero que prenda la hitbox y la apague al toque, la hitbox aparte de empujar a Faith se va a apagar sola
            {
                pushHitBox.SetActive(true);
            }
            if (p.inState && p.t01 >= (50f / 111f)) //en el frame 41/111 quiero que prenda la hitbox y la apague al toque, la hitbox aparte de empujar a Faith se va a apagar sola
            {
                pushHitBox.SetActive(false);
            }

            if (p.finished && p.inState)
                Think();

            if (isSlowed) Think();
        };
        frontAttack.OnExit += x =>
        {
            isKicking = false;
            _anim.ResetTrigger("FrontKick");
        };
        //VOMIT ATTK
        vomitAttk.OnEnter += x => //GENERIC VOMIT COUNTER CON 3 SEGUNDOS PARA VOLVER A VOMITAR
        {
            _anim.SetTrigger("VomitAttack");
            _myRb.velocity = Vector3.zero;
            isVomiting = true;
        };
        vomitAttk.OnUpdate += () =>
        {
            var p = GetStateProgress("VomitAttack");

            LookAtParameterOnY(player.transform, _rotationSpeed / 4);

            if (p.inState)
            {
                for (int i = 0; i < bileTimes.Length; i++)
                {
                    if (!bileShot[i] && p.t01 >= bileTimes[i])
                    {
                        ShootBile();
                        bileShot[i] = true;
                        break; // <- SOLO UN DISPARO POR FRAME
                    }
                }
            }

            if (p.finished && p.inState)
            {
                for (int i = 0; i < bileShot.Length; i++) bileShot[i] = false;
                Think();
            }

            if (isSlowed) Think();
        };
        vomitAttk.OnExit += x =>
        {
            isVomiting = false;
            _anim.ResetTrigger("VomitAttack");
            ResetVomitCounter();
        };
        // DEATH
        death.OnEnter += x =>
        {
            _anim.SetTrigger("Death");
            _amIDead = true;
            _anim.SetBool("isDead", _amIDead);



            _enemyLife.HideLifeBar();

            rageVfx.SendEvent("Stop");
            mat.SetFloat("_Rage", 0);

        };
        death.OnUpdate += () => { };
        death.OnExit += x =>
        {
            //_anim.SetBool("isDead", false);
        };


        #endregion

        _myFsm = new EventFSM<VInputs>(thinking);
    }

    #region CONDITION CHECKER
    void ConditionChecker()
    {
        //_isPlayerOnSight = CheckFOV().isOnSight && CheckFOV().isPlayer;
        _isPlayerOnSight = CheckPlayerInRadius(followRadius) && !ObstacleCovering(player.transform); //EL PLAYER ESTA EN RADIO, NO LO CUBRE NADA, LO MIRO Y LO SIGO
        _isPlayerResting = ServiceLocator.Instance.GetDependency<PlayerMVC>().isResting;
        _isPlayerOnAttkRange = _isPlayerOnSight && CheckFOV().distance <= dstToAttk; //SOLO SI LO ESTOY MIRANDO ES TRUE QUE PUEDA MORDERLO
        _amIStunned = _enemyLife.Life <= 0f;
        _amIHurt = (_enemyLife.Life / _enemyLife.MaxLife) <= 0.5f; //SI BAJA DEL 50% DE VIDA, ESTOY HERIDO ENTRO EN FASE 2 DONDE PUEDO SMOKEAR
        if(!_smokeShieldActive && _amIHurt) //SI NO TIRE SMOKESHIELD Y ESTOY MENOS DEL 50
            if(SmokeShieldCounter(_counterSmokeShield)) //CORRO TIMER DE SMOKE
            {
                _canSmokeShield = true;
            }
        float dist = Vector3.Distance(transform.position, player.transform.position);
        _isPlayerFarAway = dist > followRadius && dist < 100f;
        _isPlayerOnSightVomit = CheckPlayerInRadius(followRadius*3) && !ObstacleCovering(player.transform); //EL PLAYER ESTA EN RADIO, NO LO CUBRE NADA, LO MIRO Y LO SIGO
        _canVomit = _isPlayerOnSightVomit && VomitCounter(3f);
        //OBJECTIVE
        _isPlayerDead = player.isDead;

        if (_amIStunned && !_haveIDied)    //SI ESTOY STUNNEADO, MUERO (POR AHORA)
        {
            _haveIDied = true;
            SendInputToFSM(VInputs.DEATH);
        }
    }
    #endregion

    #region VARIOUS METHODS
    void ShootBile()
    {
        var bilis = Instantiate(bileProyectile, mouthSpawner.position, transform.rotation);
        bilis.GetComponent<BileProjectile>().Init(mouthSpawner, player.transform, angleOfBile);
    }
    void GetHitstunned() => SendInputToFSM(VInputs.HITSTUN);
    // Devuelve: (estoyEnEseEstado, t[0..1], terminó)
    (bool inState, float t01, bool finished) GetStateProgress(string stateName)
    {
        var info = _anim.GetCurrentAnimatorStateInfo(0);

        //Debug.Log("Current hash: " + info.shortNameHash);

        bool inState = info.IsName(stateName);

        // Para la ventana usá t en [0..1]
        float t01 = Mathf.Clamp01(info.normalizedTime);

        // Para "terminó", NO uses % 1f: dejá el valor real (puede ser > 1 en no-loop)
        bool finishedAndInState = inState && info.normalizedTime >= .9f;

        //Debug.Log(info.IsName("LightAttack1") + " " + info.shortNameHash);
        //Debug.Log($"{inState} finished and is in state: {finishedAndInState} with normalized time {t01}");
        //Debug.Log($"{inState} {t01} {finished}");
        return (inState, t01, finishedAndInState);
    }
    public void GrabAttempt()
    {
        if (Vector3.Distance(player.transform.position, transform.position) <= dstToAttk)
        {
            SetRestingTrue(); // lo agarraste
        }
        else
        {
            _anim.SetBool("FeastBool", false);
            Think();
        }
    }
    void DeactivateThisGameObject() => gameObject.SetActive(false);
    void SetRestingTrue() => ServiceLocator.Instance.GetDependency<PlayerMVC>().SetResting(true);
    void SetRestingFalse() => ServiceLocator.Instance.GetDependency<PlayerMVC>().SetResting(false);
    Vector3 GetDir(Transform param) => param.transform.position - transform.position;
    Vector3 GetDirWithV3(Vector3 vector) => (vector - transform.position).normalized;
    float GetDistanceIgnoringY(Vector3 param)
    {
        return Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(param.x, 0, param.z));
    }
     /// <summary>
    /// UTILIZO SLERP
    /// </summary>
    /// <param name="param"></param>
    void LookAtParameterOnY(Transform param, float rotationSpeed)
    {
        Vector3 dir = GetDir(param.transform);
        dir.y = 0;
        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
    void LookAtParameterWithVector3(Vector3 vector)
    {
        Vector3 dir = GetDirWithV3(vector);
        dir.y = 0;
        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
        }
    }
    void FollowParamWithRB(Transform param)
    {
        Vector3 dir = GetDir(param.transform);
        dir.y = 0;
        dir.Normalize();
        _myRb.MovePosition(transform.position + dir * _speed * Time.fixedDeltaTime);
    }
    void FollowParamWithRBV3(Vector3 param)
    {
        Vector3 dir = GetDirWithV3(param);
        dir.y = 0;
        dir.Normalize();
        _myRb.MovePosition(transform.position + dir * _speed * Time.fixedDeltaTime);
    }
    void PursuitWithRBV3(Transform target)
    {
        // dirección de movimiento del target = posición actual - posición previa
        Vector3 targetDir = target.position - _previousTargetPosition;

        // posición futura estimada
        Vector3 futureTarget = target.position + targetDir * _timePrediction / Time.deltaTime;

        // actualizo posición previa
        _previousTargetPosition = target.position;

        // dirección hacia el target futuro
        Vector3 dir = (futureTarget - transform.position).normalized;
        dir.y = 0f;

        // movimiento con Rigidbody
        _myRb.MovePosition(transform.position + dir * _speed * Time.fixedDeltaTime);

        // debug para ver hacia dónde apunta
        Debug.DrawRay(transform.position, futureTarget - transform.position, Color.red);
    }
    public void Think() => SendInputToFSM(VInputs.THINKING);
    bool GenericCounter(float time)
    {
        _counter += Time.deltaTime;
        return _counter >= time;
    }
    void ResetCounter() { _counter = 0; }
    bool VomitCounter(float time)
    {
        _vomitCounter += Time.deltaTime;
        return _vomitCounter >= time;
    }
    void ResetVomitCounter() { _vomitCounter = 0; }
    bool SmokeShieldCounter(float time)
    {
        _smokeCounter += Time.deltaTime;
        return _smokeCounter >= time;
    }
    void ResetSmokeShieldCounter() => _smokeCounter = 0;
    Vector3 GetRandomPointWithinARadius(float radius)
    {
        float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
        float xAxis = Mathf.Cos(angle) * radius;
        float zAxis = Mathf.Sin(angle) * radius;

        return transform.position + new Vector3(xAxis, 0, zAxis);
    }
    #endregion

    #region MONOBEHAVIOURS, CONDITION CHECKER AND DEBUGS
    void Start()
    {
        player = ServiceLocator.Instance.GetDependency<PlayerLife>();
        _cameraShake.SetFloat("_Shaking", 0);
    }

    (bool isOnSight, bool isPlayer, float distance) CheckFOV()
    {
        bool sawPlayer = false;
        float bestPlayerDist = Mathf.Infinity;
        Transform bestPlayer = null;

        float bestFoodDist = Mathf.Infinity;
        Transform bestFood = null;

        Collider[] colliders = Physics.OverlapSphere(transform.position, viewDistance);
        if (colliders == null || colliders.Length == 0)
            return (false, false, Mathf.Infinity);

        foreach (var c in colliders)
        {
            Vector3 dir = c.transform.position - transform.position;
            float angle = Vector3.Angle(transform.forward, dir.normalized);
            if (angle > angleOfView * 0.5f) continue;

            if (ObstacleCovering(c.transform)) continue;

            float d = dir.magnitude;

            if (c.GetComponent<PlayerLife>() != null)
            {
                sawPlayer = true;
                if (d < bestPlayerDist)
                {
                    bestPlayerDist = d;
                    bestPlayer = c.transform;
                }
            }
            else if (c.GetComponent<MeatLife>() != null)
            {
                if (d < bestFoodDist)
                {
                    bestFoodDist = d;
                    bestFood = c.transform;
                }
            }
        }

        // Elegir target según prioridad (player > comida)
        Transform chosenTarget = null;
        float chosenDist = Mathf.Infinity;

        if (sawPlayer)
        {
            chosenTarget = bestPlayer;
            chosenDist = bestPlayerDist;
        }
        else if (bestFood != null)
        {
            chosenTarget = bestFood;
            chosenDist = bestFoodDist;
        }

        target = chosenTarget;
        bool any = (chosenTarget != null);

        return (any, sawPlayer, chosenDist);
    }
    bool CheckPlayerInRadius(float radius)
    {
        if (Vector3.Distance(transform.position, player.transform.position) <= radius)
            return true;
        else
            return false;
    }
    bool ObstacleCovering(Transform target)
    {
        Vector3 toTarget = target.position - transform.position;
        return Physics.Raycast(transform.position + offsetYForSight, toTarget.normalized, toTarget.magnitude, obstacles);
    }

    float nextCheckTime = 0f;
    float checkInterval = 0.5f;
    private void Update()
    {
        if (_amIDead) return;

        if (Time.time >= nextCheckTime)
        {
            nextCheckTime = Time.time + checkInterval;
            ConditionChecker();
        }

        _myFsm.Update();
    }
    private void FixedUpdate()
    {
        if (_amIDead || _isPlayerResting) return;

        _myFsm.FixedUpdate();
    }
    private void LateUpdate()
    {
        if (_amIDead || _isPlayerResting) return;

        _myFsm.LateUpdate();
    }
    private void SendInputToFSM(VInputs inp)
    {
        _myFsm.SendInput(inp);
        //currentStateTMP.text = _myFsm.Current.Name;
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, followRadius);
        //Quiero debuggear el angle con lineas
        Vector3 leftBoundary = Quaternion.Euler(0, -angleOfView / 2f, 0) * transform.forward * viewDistance;
        Vector3 rightBoundary = Quaternion.Euler(0, angleOfView / 2f, 0) * transform.forward * viewDistance;
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);

        if (player != null)
            Gizmos.DrawRay(transform.position + offsetYForSight, (player.transform.position - transform.position).normalized * viewDistance);
        //TRANSFORM + OFFSET EN LOS OJOS

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, dstToAttk);
        //Gizmos.DrawWireSphere(transform.position, dstToScratch);

        //if(randomPointPatrol != Vector3.zero)
        //    Gizmos.DrawSphere(randomPointPatrol, randomPointTolerance);
    }
    //void OnDestroy()
    //{
    //    //if (_enemyLife != null) _enemyLife.OnHit -= HandleOnHit;
    //}
    private void OnTriggerEnter(Collider other)
    {
        if (_enemyLife.isInvulnerable || !canGetHitstunned) return; //SI SOY INVULNERABLE O NO ME PUEDEN HITSTUNNEAR

        if (other.GetComponent<SwordCollider>()) //SI ME TOCA LA ESPADA PREGUNTO SI PUEDO IR A HITSTUN
        {
            _hitCounter++;
            Debug.LogWarning($"HITSTUN COUNTER: {_hitCounter}");

            if (_hitCounter >= hitsToStun) //SI EL HIT COUNTER ES MENOR A LOS HITS QUE PRECISO PARA HITSTUNNEARME VUELVO TAMBN
            {
                GetHitstunned();
            }
        }
    }
    #endregion
    #region NECESSARY DEPENDENCIES
    void AnimationFinishedFalse() { _animationFinished = false; }
    void AnimationFinishedTrue()
    {
        _animationFinished = true;
    }
    public void DamagePlayerAnimMethod()
    {

    }
    public void DisableMovement()
    {

    }
    #endregion




    public void ActiveFade()
    {
        //LOGICA BARRA DE VIDA, TENGO QUE REFERENCIAR SU BOSSLIFE PORQUE SINO CHAU, YA ESTA REFERENCIADO IGUAL PERO TENGO QUE PREPARARLE LOS METODOS
        _enemyLife.ShowLifeBar();
    }

    #region SLOWABLE
    float originalSpeed;
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

    #region CAMERA SHAKE
    [Header("CAMERA SHAKE VARIABLES")]
    [SerializeField] Material _cameraShake;
    [SerializeField] float maxCamearaShakeDistance;
    int _camShakesCount;

    public void CameraShake(float intesity) { StartCoroutine(CamShakeCoroutine(intesity)); }

    IEnumerator CamShakeCoroutine(float intesity)
    {
        float distance = Vector3.Distance(Camera.main.transform.position, transform.position);
        if (distance <= maxCamearaShakeDistance)
        {
            _camShakesCount++;
            float itensityAtten = 1 - distance / maxCamearaShakeDistance;
            float newIntesity = intesity * itensityAtten;
            _cameraShake.SetFloat("_Shaking", 1);
            _cameraShake.SetFloat("_ShakePower", newIntesity);
            //_cameraShake.SetFloat("_YOffset", intesity/2 );
            yield return new WaitForSeconds(newIntesity / 5);
            if (_camShakesCount <= 1) _cameraShake.SetFloat("_Shaking", 0);
            _camShakesCount--;
        }
    }
    #endregion
}
