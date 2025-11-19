using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IA2;
using System;
using JetBrains.Annotations;
using TMPro;
using UnityEngine.VFX;

public class Beelzebub : MonoBehaviour, ISlowable
{
    public enum BInputs
    {
        THINKING,
        REST,
        HITSTUN,
        FOLLOW,
        TONGUEATTK,
        DEATHRAY,
        MORTARVOMIT,
        TAUNT,
        DEATH,
        CINEMATICDEATH
    }
    private EventFSM<BInputs> _myFsm;
    [SerializeField] private BossLife _enemyLife;
    private Rigidbody _myRb;
    private Renderer _myRen;
    private Animator _anim;
    [SerializeField] PlayerLife player;


    #region PRECONDITIONS AND OBJECTIVES
    [Header("<color=orange>PRECONDITIONS AND DEBUGS</color>")]
    //CHELO WAS HERE: AGREGO DEPENDENCIA DE SERVICE LOCATOR ISRESTING PARA APAGAR EL THINKING
    public bool isResting = false;
    public TextMeshProUGUI currentStateTMP;
    //PRECONDITIONS
    public bool _isPlayerOnSight; //if player not on sight look at last place and taunt
    public bool _canTaunt;
    public bool _isPlayerOnAttkRange;//change attackrange tongue is huge
    public bool _hasStamina;// depends of stamina (maybe i just use a float)
    public bool _amIDead;// if im dead
    public bool _amIStunned;// just in case
    public bool _amIHurt;// instead of phases i go faster and stronger
    public bool _canMortarVomit;//can mortar if stamina and cant reach player
    public bool _canDeathRay;//can deathRay if player running in a straight line
    public bool _haveToRest = false;// if (qty of skills) reached recharges stamina
    public bool _isPlayerHurt; // if player low on life, press him more close

    //OBJECTIVES
    bool _isPlayerDead;
    bool[] allPreconditions;
    #endregion

    #region VARIABLES
    [Header("<color=orange>CAN THINK</color>")]
    [SerializeField] bool canThink = true;
    [Header("<color=orange>VARIABLES</color>")]
    [SerializeField] float _counterToRest = 5f;
    [SerializeField] int hitCounterToRest = 3;
    [SerializeField] float _counterSmokeShield = 30f, counterFollow = 4;
    [SerializeField] Transform mouthSpawner;
    [SerializeField] GameObject maceHitbox, pushHitbox, smokeHitbox, mortarProyectile;
    [SerializeField] GameObject karmicTrigger;
    [SerializeField] GameObject groundSlam;
    [SerializeField] Vector3 offsetYForSight;
    [SerializeField] float _rotationSpeed = 5f;
    [SerializeField] float _speed = 5;
    [SerializeField] float dstToAttk = 2f;
    [SerializeField] float followRadius = 8f;
    [SerializeField] float viewDistance = 15f;
    [SerializeField] float angleOfView = 45f;
    [SerializeField] LayerMask obstacles = 1 << 13, playerLayer; //Activo la layer 13 que va a ser obstacles
    [Header("<color=orange>PURSUIT VARIABLES</color>")]
    [SerializeField] float _timePrediction = 0.3f;
    [SerializeField] VisualEffect smokeShieldVfx, rageVfx;
    [SerializeField] Material mat;
    public bool _animationFinished = false, canGetHitstunned = true, _haveIDied = false, comingFromFollow = false;
    float _counter = 0, _smokeCounter = 0, _vomitCounter = 3; //LE PONGO EN TRES PARA QUE AL PRINCIPIO YA TE ATAQUE Y NO SE QUEDE SIN ESTADO
    int _restCounter = 0;
    int randomAttack;
    bool canTurnOnHitbox = false, canSlamGround = false;
    float _currentDmg;
    [SerializeField] Transform target;
    public float dmgAttk1 = 22f;
    bool phaseChanged, isSlowed;
    Vector3 _previousTargetPosition;
    #endregion

    #region STAMINA
    float bossStamina = 100f;
    #endregion

    #region BILE FRAMES AND FLAGS
    [Header("<color=orange>BILE VARIABLES (attks to vomit after follow 3 HARDCODED)</color>")]
    [SerializeField] int bileQuantity = 3;
    [SerializeField] float angleOfBile = 45f;
    Vector2 bileRange = new Vector2(0.2f, 0.6f);
    bool isVomiting = false;
    bool[] bileShot;
    float[] bileTimes;
    int bileAfterJumpCounter = 0;
    #endregion

    #region DEATHRAY
    [Header("<color=orange>DEATHRAY</color>")]
    [SerializeField] float speedDeathRay = .5f, dpsRay = .25f, rotationSpeed = 1f;
    [SerializeField] VisualEffect _vfxDeathRay;
    [SerializeField] Transform acidLaser;
    #endregion

    #region HITSTUN HANDLER
    [Header("<color=orange>HITSTUN VARIABLES</color>")]
    [SerializeField] int hitsToStun = 3;
    int _hitCounter = 0;
    #endregion


    //CHELO WAS HERE: AGREGO DEPENDENCIA DE SERVICE LOCATOR ISRESTING PARA APAGAR EL THINKING
    private void OnDisable()
    {
        ServiceLocator.Instance.RemoveDependency<Beelzebub>();
    }
    private void OnDestroy()
    {
        ServiceLocator.Instance.RemoveDependency<Beelzebub>();
    }

    private void OnEnable()
    {
        //CHELO WAS HERE: AGREGO DEPENDENCIA DE SERVICE LOCATOR ISRESTING PARA APAGAR EL THINKING
        ServiceLocator.Instance.RegisterDependency<Beelzebub>(this);

        bileShot = new bool[bileQuantity];
        bileTimes = new float[bileQuantity];

        float step = (bileRange.y - bileRange.x) / (bileQuantity - 1);
        for (int i = 0; i < bileQuantity; i++)
        {
            bileTimes[i] = bileRange.x + step * i;
        }

        if (_enemyLife == null) _enemyLife = GetComponent<BossLife>();

        _myRb = GetComponent<Rigidbody>();
        _myRen = GetComponent<Renderer>();
        _anim = GetComponent<Animator>();
        _smokeCounter = _counterSmokeShield; //IGUALO SMOKECOUNTER PARA QUE HAGA DE UNA APENAS QUEDA POR DEBAJO DEL 50%

        smokeShieldVfx.SendEvent("Stop");

        canGetHitstunned = true;

        phaseChanged = false;

        //mat.SetFloat("_Rage", 0);

        #region STATES DECLARATION
        var thinking = new State<BInputs>("THINKING");
        var rest = new State<BInputs>("REST");
        var hitstun = new State<BInputs>("HITSTUN");
        var follow = new State<BInputs>("FOLLOW");
        var tongueAttk = new State<BInputs>("TONGUEATTACK");
        var deathRay = new State<BInputs>("DEATHRAY");
        var mortarVomit = new State<BInputs>("MORTARVOMIT");
        var taunt = new State<BInputs>("TAUNT");
        var death = new State<BInputs>("DEATH");
        var cinematicDeath = new State<BInputs>("CINEMATICDEATH");
        #endregion

        #region STATE CONGIFURER
        StateConfigurer.Create(thinking)
            .SetTransition(BInputs.REST, rest)
            .SetTransition(BInputs.HITSTUN, hitstun)
            .SetTransition(BInputs.FOLLOW, follow)
            .SetTransition(BInputs.TONGUEATTK, tongueAttk)
            .SetTransition(BInputs.DEATHRAY, deathRay)
            .SetTransition(BInputs.MORTARVOMIT, mortarVomit)
            .SetTransition(BInputs.TAUNT, taunt)
            .SetTransition(BInputs.DEATH, death)
            .SetTransition(BInputs.CINEMATICDEATH, cinematicDeath)
            .Done();

        StateConfigurer.Create(rest)
            .SetTransition(BInputs.THINKING, thinking)
            .SetTransition(BInputs.HITSTUN, hitstun)
            .SetTransition(BInputs.DEATH, death)
            .Done();
        StateConfigurer.Create(hitstun)
            .SetTransition(BInputs.THINKING, thinking)
            .SetTransition(BInputs.HITSTUN, hitstun)
            .SetTransition(BInputs.DEATH, death)
            .Done();
        StateConfigurer.Create(follow)
            .SetTransition(BInputs.THINKING, thinking)
            .SetTransition(BInputs.TONGUEATTK, tongueAttk)
            .SetTransition(BInputs.HITSTUN, hitstun)
            .SetTransition(BInputs.DEATH, death)
            .Done();
        StateConfigurer.Create(tongueAttk)
            .SetTransition(BInputs.THINKING, thinking)
            .SetTransition(BInputs.HITSTUN, hitstun)
            .SetTransition(BInputs.REST, rest)
            .SetTransition(BInputs.DEATH, death)
            .Done();
        StateConfigurer.Create(deathRay)
            .SetTransition(BInputs.THINKING, thinking)
            .SetTransition(BInputs.DEATH, death)
            .Done();
        StateConfigurer.Create(mortarVomit)
            .SetTransition(BInputs.THINKING, thinking)
            .SetTransition(BInputs.DEATH, death)
            .Done();
        StateConfigurer.Create(taunt)
            .SetTransition(BInputs.THINKING, thinking)
            .SetTransition(BInputs.HITSTUN, hitstun)
            .SetTransition(BInputs.DEATH, death)
            .Done();
        StateConfigurer.Create(death)
            .SetTransition(BInputs.CINEMATICDEATH, cinematicDeath)
            .Done();
        StateConfigurer.Create(cinematicDeath)
            .SetTransition(BInputs.THINKING, thinking)
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
                if(Input.GetKeyDown(KeyCode.F))
                    SendInputToFSM(BInputs.FOLLOW);

                if (Input.GetKeyDown(KeyCode.P) && !isVomiting)
                    SendInputToFSM(BInputs.MORTARVOMIT);

                if (Input.GetKeyDown(KeyCode.K))
                    SendInputToFSM(BInputs.DEATHRAY);
            }
        };
        thinking.OnExit += x =>
        {
            _anim.SetBool("isIdle", false);
            ResetCounter();
        };
        rest.OnEnter += x =>
        {
            _anim.SetBool("isIdle", true);
            _myRb.velocity = Vector3.zero;
        };
        rest.OnUpdate += () =>
        {
            if (GenericCounter(_counterToRest)) Think();
        };
        rest.OnExit += x =>
        {
            _anim.SetBool("isIdle", false);
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
                SendInputToFSM(BInputs.TONGUEATTK);
            }
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
        // TONGUE ATTACK (POR AHORA NORMAL PORQUE NO TENGO ANIMACIONES NI MODELO)
        tongueAttk.OnEnter += x =>
        {
            _anim.applyRootMotion = true;

            randomAttack = UnityEngine.Random.Range(1, 4); //SI VENGO DE FOLLOW RANDOM ATTACK ES 3 Y HAGO ATTACK DE SALTO
            canGetHitstunned = false;

            _anim.SetTrigger("LightAttack" + randomAttack);
            _myRb.velocity = Vector3.zero;
        };
        bool canTurnSpecialHitbox = false;
        tongueAttk.OnUpdate += () =>
        {
            var p = GetStateProgress($"LightAttack{randomAttack}");

            // Ventana solo si REALMENTE estás en LightAttack1
            if (randomAttack == 1)
            { canTurnOnHitbox = p.inState && p.t01 >= 0.33f && p.t01 <= 0.66f; }
            else if (randomAttack == 2)
            { canTurnOnHitbox = p.inState && p.t01 >= 0.42f && p.t01 <= 0.46f; }
            else // randomAttack == 3 (49 / 126 frames) IMPACT
            {
                canTurnSpecialHitbox = p.inState && p.t01 >= 0.38f && p.t01 <= 0.75f;
                canSlamGround = p.inState && p.t01 >= 0.38f && p.t01 <= 0.75f;
            }

            if (canSlamGround)
            {
                canSlamGround = false;
                groundSlam.SetActive(true);
            }

            maceHitbox.SetActive(canTurnOnHitbox);
            pushHitbox.SetActive(canTurnSpecialHitbox);

            if (p.finished)
                SendInputToFSM(BInputs.REST);
        };
        tongueAttk.OnExit += x =>
        {
            groundSlam.SetActive(false);
            _anim.ResetTrigger("LightAttack" + randomAttack);
            maceHitbox.SetActive(false); // por las dudas, cerrá la hitbox al salir
            pushHitbox.SetActive(false);
        };
        // HITSTUN
        //hitstun.OnEnter += x =>
        //{

        //};
        //hitstun.OnUpdate += () =>
        //{

        //};
        //hitstun.OnExit += x =>
        //{
            
        //};
        // DEATH RAY
        bool isLaserActive = false;
        deathRay.OnEnter += x =>
        {
            _anim.applyRootMotion = false;
            _anim.SetTrigger("VomitAttack");
            _myRb.velocity = Vector3.zero;
            isLaserActive = false;
        };

        float yAxis = -1f;
        float xAxis = 0f;
        float rayLength = 100f;
        float radius = 3f;
        deathRay.OnUpdate += () =>
        {
            if (GenericCounter(2f))
            {
                if (!isLaserActive)
                {
                    isLaserActive = true;
                    ResetCounter();
                    _vfxDeathRay.SendEvent("AcidLaser");
                }
            }

            LookAtParameterOnY(target, rotationSpeed);

            var u = UtilitiesAgus.GetAnimatorStateProgress("VomitAttack", _anim);

            if (u.finished) Think(); // ARREGLAR, POR ALGUNA RAZÓN NO PASA DEVUELTA A THINKING

            if (u.t01 < 120f / 334f || !u.inState) return;


            yAxis += Time.deltaTime * speedDeathRay;
            xAxis += Time.deltaTime * speedDeathRay;

            // Dirección local del rayo
            Vector3 localDir = new Vector3(0, yAxis, xAxis).normalized;

            // Convertimos al espacio global (usa la orientación del mouthSpawner)
            Vector3 worldDir = mouthSpawner.TransformDirection(localDir);

            Vector3 start = mouthSpawner.position;
            Vector3 end = start + worldDir * rayLength;

            Debug.DrawLine(start, end, Color.green);

            Collider[] hits = Physics.OverlapCapsule(start, end, radius, playerLayer);
            foreach (var hit1 in hits)
            {
                if (hit1.TryGetComponent<PlayerLife>(out PlayerLife player))
                    player.TakeDamageWithoutFlinching(dpsRay);
            }

            Vector3 origin = acidLaser.position;
            Vector3 direction = acidLaser.forward;

            acidLaser.rotation = Quaternion.LookRotation(worldDir);

            Ray ray = new Ray(origin, direction);
            if (Physics.Raycast(ray, out RaycastHit hit, rayLength))
            {
                float lenght = Vector3.Distance(hit.point, origin);
                _vfxDeathRay.SetFloat("LaserLenght", lenght);
                _vfxDeathRay.SetVector3("ImpactPoint", hit.point);
                _vfxDeathRay.SetVector3("InpactNormal", hit.normal);

            }
            else
            {
                _vfxDeathRay.SetFloat("LaserLenght", rayLength);
                _vfxDeathRay.SetVector3("ImpactPoint", direction * rayLength);
                _vfxDeathRay.SetVector3("InpactNormal", -direction);
            }
        };

        deathRay.OnExit += x =>
        {
            isLaserActive = false;
            yAxis = -1f;
            xAxis = 0f;
            ResetCounter();
        };

        // MORTAR VOMIT
        mortarVomit.OnEnter += x =>
        {
            _anim.applyRootMotion = false;
            _anim.SetTrigger("VomitAttack");
            _myRb.velocity = Vector3.zero;
            isVomiting = true;
        };
        mortarVomit.OnUpdate += () =>
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
                    }
                }
            }

            if (p.finished && p.inState)
            {
                for (int i = 0; i < bileShot.Length; i++) bileShot[i] = false;
                Think();
            }
        };
        mortarVomit.OnExit += x =>
        {
            isVomiting = false;
            _anim.ResetTrigger("VomitAttack");
            ResetVomitCounter();
        };
        // TAUNT
        taunt.OnEnter += x => //GENERIC VOMIT COUNTER CON 3 SEGUNDOS PARA VOLVER A VOMITAR
        {

        };
        taunt.OnUpdate += () =>
        {
            
        };
        taunt.OnExit += x =>
        {
            
        };
        // DEATH
        death.OnEnter += x =>
        {
            _enemyLife.IsDead = true;
            _anim.SetTrigger("Death");
            _anim.SetBool("isDead", true);
        };
        death.OnUpdate += () => 
        {

        };
        death.OnExit += x =>
        {
            _anim.SetBool("isDead", false);
        };
        // CINEMATIC DEATH
        cinematicDeath.OnEnter += x =>
        {

        };
        cinematicDeath.OnUpdate += () => 
        {

        };
        cinematicDeath.OnExit += x =>
        {
        };
        #endregion

        _myFsm = new EventFSM<BInputs>(thinking);
    }

    #region CONDITION CHECKER
    void ConditionChecker()
    {
        if (player == null) return;

        //_isPlayerOnSight = CheckFOV().isOnSight && CheckFOV().isPlayer;
        _isPlayerOnSight = CheckPlayerInRadius(followRadius) && !ObstacleCovering(player.transform); //EL PLAYER ESTA EN RADIO, NO LO CUBRE NADA, LO MIRO Y LO SIGO
        _isPlayerOnAttkRange = _isPlayerOnSight && CheckFOV().distance <= dstToAttk; //SOLO SI LO ESTOY MIRANDO ES TRUE QUE PUEDA MORDERLO
        _amIStunned = _enemyLife.Life <= 0f;
        _amIHurt = (_enemyLife.Life / _enemyLife.MaxLife) <= 0.5f; //SI BAJA DEL 50% DE VIDA, ESTOY HERIDO ENTRO EN FASE 2 DONDE PUEDO SMOKEAR
        
        float dist = Vector3.Distance(transform.position, player.transform.position);
      
        //OBJECTIVE
        _isPlayerDead = player.isDead;

        if (_amIStunned && !_haveIDied)    //SI ESTOY STUNNEADO, MUERO (POR AHORA)
        {
            _haveIDied = true;
            SendInputToFSM(BInputs.DEATH);
        }
    }
    #endregion

    #region VARIOUS METHODS
    void ShootBile()
    {
        var bilis = Instantiate(mortarProyectile, mouthSpawner.position, transform.rotation);
        bilis.GetComponent<BileMortar>().Init(mouthSpawner);
    }
    void GetHitstunned() => SendInputToFSM(BInputs.HITSTUN);
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
        if (target == null) return;

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
    public void Think()
    {
        Debug.Log("PIENSO");
        SendInputToFSM(BInputs.THINKING);
    }
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
    IEnumerator Start()
    {
        yield return new WaitForSeconds(1f);

        player = ServiceLocator.Instance.GetDependency<PlayerLife>();
        target = player.transform;
        //_cameraShake.SetFloat("_Shaking", 0);
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
        if (player == null) return false;

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
        if (_amIDead) return;

        _myFsm.FixedUpdate();
    }
    private void LateUpdate()
    {
        if (_amIDead) return;

        _myFsm.LateUpdate();
    }
    private void SendInputToFSM(BInputs inp)
    {
        _myFsm.SendInput(inp);
        currentStateTMP.text = _myFsm.Current.Name;
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

    #region ACTIVE FADE
    public void ActiveFade()
    {
        //LOGICA BARRA DE VIDA, TENGO QUE REFERENCIAR SU BOSSLIFE PORQUE SINO CHAU, YA ESTA REFERENCIADO IGUAL PERO TENGO QUE PREPARARLE LOS METODOS
        _enemyLife.ShowLifeBar();
    }
    #endregion

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
    [Header("<color=orange>CAMERA SHAKE VARIABLES</color>")]
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
