using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IA2;
using System;
using JetBrains.Annotations;
using TMPro;
using UnityEngine.VFX;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BossLife))]
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
    public bool _animationFinished = false, canGetHitstunned = true, _haveIDied = false, comingFromFollow = false;
    float _counter = 0, counterFollow = 3f;
    int _restCounter = 0;
    int randomAttack;
    bool canTurnOnHitbox = false, canSlamGround = false;
    float _currentDmg;
    bool phaseChanged, isSlowed;
    Vector3 _previousTargetPosition;
    [SerializeField] PlayerLife player;


    #region PRECONDITIONS AND OBJECTIVES
    [Header("<color=orange>PRECONDITIONS AND DEBUGS</color>")]
    //CHELO WAS HERE: AGREGO DEPENDENCIA DE SERVICE LOCATOR ISRESTING PARA APAGAR EL THINKING
    public bool isResting = false;
    public TextMeshProUGUI currentStateTMP;
    /*PRECONDITIONS:
    isPlayerInSight
    isPlayerInAttackRange
     */
    bool isPlayerInSight, isPlayerInAttackRange, _amIDead;

    //OBJECTIVES
    bool _isPlayerDead;
    #endregion

    #region VARIABLES
    [Header("<color=orange>CAN THINK</color>")]
    [SerializeField] bool canThink = true;
    [Header("<color=orange>VARIABLES</color>")]
    [SerializeField] float _counterToRest = 5f;
    [SerializeField] Transform mouthSpawner, target, tonguePos;
    //[SerializeField] GameObject pushHitbox, smokeHitbox, mortarProyectile;
    [SerializeField] GameObject groundSlam, tongueHitbox1, tongueHitbox2, tongueHitbox3, mortarProyectile;
    Vector3 
        posSlam1 = new Vector3(-187.95f, 21.2f, 4.52f), 
        posSlam2 = new Vector3(-207.27f, 21.2f, 1.48f), 
        posSlam3 = new Vector3(-212.45f, 21.2f, 0.75f);
    [SerializeField] VisualEffect smokeShieldVfx, rageVfx;
    [SerializeField] Material mat;
    [SerializeField] Vector3 offsetYForSight;
    [SerializeField] float _rotationSpeed = 5f, _speed = 5, dstToAttk = 2f, followRadius = 8f, viewDistance = 15f, angleOfView = 45f,
                             dmgAttk1 = 22f, dmgAttk2 = 34f;
    [SerializeField] LayerMask obstacles = 1 << 13, playerLayer; //Activo la layer 13 que va a ser obstacles
    [Header("<color=orange>PURSUIT VARIABLES</color>")]
    [SerializeField] float _timePrediction = 0.3f;
    #endregion

    #region STAMINA
    float bossStamina = 100f, currentStamina, staminaMutliplier = 2f;
    #endregion

    #region BILE FRAMES AND FLAGS
    [Header("<color=orange>BILE VARIABLES (attks to vomit after follow 3 HARDCODED)</color>")]
    [SerializeField] int bileQuantity = 3;
    [SerializeField] float angleOfBile = 45f, timeToShootBile = 1f, costOfMortarVomit = 25f;
    Vector2 bileRange = new Vector2(0.2f, 0.6f);
    bool isVomiting = false;
    bool[] bileShot;
    float[] bileTimes;
    int bileAfterJumpCounter = 0;
    #endregion

    #region DEATHRAY
    [Header("<color=orange>DEATHRAY</color>")]
    [SerializeField] float speedDeathRay = .5f, dpsRay = .25f, rotationSpeed = 1f, costOfDeathRay = 50f;
    [SerializeField] VisualEffect _vfxDeathRay;
    [SerializeField] Transform acidLaser;
    #endregion

    //#region HITSTUN HANDLER
    //[Header("<color=orange>HITSTUN VARIABLES</color>")]
    //[SerializeField] int hitsToStun = 3;
    //int _hitCounter = 0;
    //#endregion


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

        smokeShieldVfx.SendEvent("Stop");

        canGetHitstunned = true;

        phaseChanged = false;

        currentStamina = bossStamina;
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
            .SetTransition(BInputs.DEATHRAY, deathRay)
            .SetTransition(BInputs.MORTARVOMIT, mortarVomit)
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
            if (isResting) return;

            if (_isPlayerDead) return;

            #region DEBUG MODE (DESACTIVAR CANTHINK)
            if (!canThink)
            {
                if(Input.GetKeyDown(KeyCode.F))
                    SendInputToFSM(BInputs.FOLLOW);

                if (Input.GetKeyDown(KeyCode.P) && !isVomiting)
                    SendInputToFSM(BInputs.MORTARVOMIT);

                if (Input.GetKeyDown(KeyCode.K))
                    SendInputToFSM(BInputs.DEATHRAY);
            }
            #endregion
            else
            {
                if (isPlayerInSight) SendInputToFSM(BInputs.FOLLOW);

                if (isPlayerInAttackRange) SendInputToFSM(BInputs.TONGUEATTK);
            }
        };
        thinking.OnExit += x =>
        {
            _anim.SetBool("isIdle", false);
            ResetCounter();
        };
        int randomSkill = 0;
        rest.OnEnter += x =>
        {
            _anim.SetBool("isIdle", true);
            _myRb.velocity = Vector3.zero;
            randomSkill = UnityEngine.Random.Range(1, 4);
        };
        rest.OnUpdate += () =>
        {
            if (GenericCounter(_counterToRest))
            {
                if (randomSkill == 1) SendInputToFSM(BInputs.DEATHRAY);
                else if (randomSkill == 2) SendInputToFSM(BInputs.MORTARVOMIT);
                else Think();
            }
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
        bool flag1 = false;
        bool flag2 = false;
        bool flag3 = false;
        tongueAttk.OnEnter += x =>
        {
            flag1 = false;
            flag2 = false;
            flag3 = false;

            randomAttack = UnityEngine.Random.Range(1, 3);
            canGetHitstunned = false;

            if (randomAttack == 1) _currentDmg = dmgAttk1;
            else _currentDmg = dmgAttk2;

            Debug.Log(randomAttack);

            _anim.SetTrigger("LightAttack" + randomAttack);
            _myRb.velocity = Vector3.zero;
        };
        tongueAttk.OnUpdate += () =>
        {
            var p = GetStateProgress($"LightAttack{randomAttack}");

            // Ventana solo si REALMENTE estás en LightAttack1
            if (randomAttack == 1)
            {
                bool inWindow1 = p.t01 >= 0.16f && p.t01 <= 0.25f;
                bool inWindow2 = p.t01 >= 0.66f && p.t01 <= 0.75f;

                canSlamGround = inWindow1 || inWindow2;

                bool canTurnOnHitbox1 = p.inState && inWindow1;
                bool canTurnOnHitbox2 = p.inState && inWindow2;

                if (inWindow1 && !flag1)
                {
                    flag1 = true;
                    groundSlam.transform.position = new Vector3(tonguePos.position.x, groundSlam.transform.position.y, tonguePos.position.z);
                }
                else if (inWindow2 && !flag2) 
                {
                    flag2 = true;
                    groundSlam.transform.position = new Vector3(tonguePos.position.x, groundSlam.transform.position.y, tonguePos.position.z);
                }
                tongueHitbox1.SetActive(canTurnOnHitbox1);
                tongueHitbox2.SetActive(canTurnOnHitbox2);
            }
            else
            {
                bool inWindow3 = p.t01 >= 0.56f && p.t01 <= 0.6f;

                if (inWindow3 && !flag3)
                {
                    flag3 = true;
                    groundSlam.transform.position = new Vector3(tonguePos.position.x, groundSlam.transform.position.y, tonguePos.position.z);
                }
                canSlamGround = inWindow3;

                bool canTurnOnHitbox3 = p.inState && inWindow3;

                tongueHitbox3.SetActive(canTurnOnHitbox3);
            }

            if (canSlamGround)
            {
                canSlamGround = false;
                groundSlam.SetActive(true);
            }

            if (p.finished)
                SendInputToFSM(BInputs.REST);
        };
        tongueAttk.OnExit += x =>
        {
            groundSlam.SetActive(false);
            _anim.ResetTrigger("LightAttack" + randomAttack);

            //pushHitbox.SetActive(false);
        };

        #region HITSTUN NO USADO POR AHORA
        // HITSTUN
        //hitstun.OnEnter += x =>
        //{
        //
        //};
        //hitstun.OnUpdate += () =>
        //{
        //
        //};
        //hitstun.OnExit += x =>
        //{
        //
        //};
        #endregion

        #region DEATH RAY
        // DEATH RAY
        bool isLaserActive = false;
        float laserFollowT = 0f;

        float yAxis = -1f;
        float xAxis = 0f;
        float rayLength = 100f;
        float radius = 1.5f;

        deathRay.OnEnter += x =>
        {
            currentStamina -= costOfDeathRay;

            _anim.applyRootMotion = false;
            _anim.SetTrigger("DeathRay");
            _myRb.velocity = Vector3.zero;

            isLaserActive = false;
            laserFollowT = 0f;

            yAxis = -1f;
            xAxis = 0f;
            ResetCounter();
        };

        deathRay.OnUpdate += () =>
        {
            LookAtParameterOnY(player.transform, _rotationSpeed / 2);

            // PROGRESO DE LA ANIMACIÓN
            var p = UtilitiesAgus.GetAnimatorStateProgress("DeathRay", _anim);

            // SI LA ANIMACIÓN TERMINÓ, VOLVEMOS A PENSAR
            if (p.finished)
            {
                Think();
                return;
            }

            // *** ACTIVACIÓN EN 0.63 NORMALIZED TIME ***
            if (p.t01 >= 0.65f && p.inState && !isLaserActive)
            {
                isLaserActive = true;
                _vfxDeathRay.SendEvent("AcidLaser");
            }

            if (!isLaserActive)
                return; // hasta 0.63f NO HACEMOS NADA

            //--------------------------------------------------
            // *** SEGUIMIENTO DEL PLAYER CON LERP ***
            //--------------------------------------------------

            // La rotación debe ir suavizando hacia el target
            laserFollowT += Time.deltaTime * .25f; // <- ajustá este valor para suavidad
            Vector3 desiredDir = (player.transform.position - mouthSpawner.position).normalized;
            Vector3 followDir = Vector3.Lerp(mouthSpawner.forward, desiredDir, laserFollowT).normalized;

            // Hacemos que el rayo “suba”
            yAxis += Time.deltaTime * speedDeathRay;
            xAxis += Time.deltaTime * speedDeathRay;

            // dirección local de subida
            Vector3 localDir = new Vector3(0, yAxis, xAxis).normalized;

            // combinación de:
            // - dirección de seguimiento
            // - dirección de subida
            Vector3 worldDir = (mouthSpawner.TransformDirection(localDir) + followDir).normalized;

            //--------------------------------------------------
            // CÁLCULO DE rayo
            //--------------------------------------------------

            Vector3 start = mouthSpawner.position;
            Vector3 end = start + worldDir * rayLength;

            Debug.DrawLine(start, end, Color.green);

            // DAÑO EN CAPSULE
            Collider[] hits = Physics.OverlapCapsule(start, end, radius, playerLayer);
            foreach (var hit1 in hits)
            {
                if (hit1.TryGetComponent<PlayerLife>(out PlayerLife playerLife))
                    playerLife.TakeDamageWithoutFlinching(dpsRay);
            }

            //--------------------------------------------------
            // VFX
            //--------------------------------------------------

            acidLaser.rotation = Quaternion.LookRotation(worldDir);

            Ray ray = new Ray(acidLaser.position, acidLaser.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, rayLength))
            {
                float length = Vector3.Distance(hit.point, acidLaser.position);
                _vfxDeathRay.SetFloat("LaserLenght", length);
                _vfxDeathRay.SetVector3("ImpactPoint", hit.point);
                _vfxDeathRay.SetVector3("InpactNormal", hit.normal);
            }
            else
            {
                _vfxDeathRay.SetFloat("LaserLenght", rayLength);
                _vfxDeathRay.SetVector3("ImpactPoint", acidLaser.forward * rayLength);
                _vfxDeathRay.SetVector3("InpactNormal", -acidLaser.forward);
            }
        };

        deathRay.OnExit += x =>
        {
            isLaserActive = false;
            laserFollowT = 0f;

            yAxis = -1f;
            xAxis = 0f;
            ResetCounter();
        };
        #endregion

        // MORTAR VOMIT
        bool startedVomitPhase = false;
        mortarVomit.OnEnter += x =>
        {
            currentStamina -= costOfMortarVomit;

            _anim.applyRootMotion = false;
            _anim.SetTrigger("VomitAttackEnter");
            _myRb.velocity = Vector3.zero;
            isVomiting = true;
        };
        mortarVomit.OnUpdate += () =>
        {
            var p = GetStateProgress("VomitAttackEnter");

            LookAtParameterOnY(player.transform, _rotationSpeed / 4);

            // Espera inicial solo UNA vez
            if (!startedVomitPhase)
            {
                if (GenericCounter(timeToShootBile))
                {
                    startedVomitPhase = true;
                    ResetCounter();
                }
                else return;
            }

            if (p.inState)
            {
                for (int i = 0; i < bileTimes.Length; i++)
                {
                    if (!bileShot[i] && p.t01 >= bileTimes[i])
                    {
                        ShootBile();
                        bileShot[i] = true;
                        break; // << ESTA VEZ SÍ VA ACÁ Y FUNCIONA
                    }
                }
            }

            // chequeo manual, super barato
            for (int i = 0; i < bileShot.Length; i++)
            {
                if (!bileShot[i])
                    return;
            }

            // Si llegó acá es porque TODOS se tiraron
            for (int i = 0; i < bileShot.Length; i++)
                bileShot[i] = false;

            startedVomitPhase = false;
            Think();
        };
        mortarVomit.OnExit += x =>
        {
            startedVomitPhase = false;
            isVomiting = false;
            _anim.ResetTrigger("VomitAttackEnter");
            _anim.SetTrigger("VomitAttackExit");
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
            gameObject.layer = 0;
            _enemyLife.HideLifeBar();

            StartCoroutine(ServiceLocator.Instance.GetDependency<BlackPanelFade>().Fade(1f, 4f, true, false));
        };
        death.OnUpdate += () => 
        {

        };
        death.OnExit += x =>
        {
            gameObject.layer = 9;
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

        isPlayerInSight = CheckPlayerInRadius(viewDistance);
        isPlayerInAttackRange = CheckFOV().distance <= dstToAttk;
        _amIDead = _enemyLife.IsDead;
        _isPlayerDead = player.isDead;
    }
    #endregion

    #region VARIOUS METHODS
    public float GetCurrentDmg() => _currentDmg;
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
        SendInputToFSM(BInputs.THINKING);
    }
    bool GenericCounter(float time)
    {
        _counter += Time.deltaTime;
        return _counter >= time;
    }
    void ResetCounter() { _counter = 0; }
    #endregion

    #region MONOBEHAVIOURS, CONDITION CHECKER AND DEBUGS
    IEnumerator Start()
    {
        yield return new WaitForSeconds(1f);

        player = ServiceLocator.Instance.GetDependency<PlayerLife>();
        target = player.transform;
        //_cameraShake.SetFloat("_Shaking", 0);
    }

    #region CheckFOV
    (bool isOnSight, float distance) CheckFOV()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, viewDistance);
        if (hits == null || hits.Length == 0)
            return (false, Mathf.Infinity);

        foreach (var h in hits)
        {
            // ¿Es el player?
            if (h.TryGetComponent<PlayerLife>(out _))
            {
                Vector3 dir = h.transform.position - transform.position;
                float angle = Vector3.Angle(transform.forward, dir.normalized);

                // ¿Está dentro del FOV?
                if (angle > angleOfView * 0.5f)
                    continue;

                // ¿Hay obstáculo en el medio?
                if (ObstacleCovering(h.transform))
                    continue;

                // Ok, está en la vista
                return (true, dir.magnitude);
            }
        }

        return (false, Mathf.Infinity);
    }
    #endregion
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
    bool haveIDied = false;
    private void Update()
    {
        if (!haveIDied && _amIDead)
        {
            haveIDied = true;
            SendInputToFSM(BInputs.DEATH);
        }

        //if (currentStamina < bossStamina)
        //{
        //    currentStamina += Time.deltaTime * staminaMutliplier;
        //    //Debug.Log(currentStamina);
        //}

        ConditionChecker();

        _myFsm.Update();
    }
    private void FixedUpdate()
    {
        if (_amIDead || _isPlayerDead) return;

        _myFsm.FixedUpdate();
    }
    private void LateUpdate()
    {
        if (_amIDead || _isPlayerDead) return;

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
