using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IA2;
using System;
using JetBrains.Annotations;
using TMPro;

public class BoneScraper : MonoBehaviour , ISlowable
{
    public enum BSInputs
    {
        PATROL, //USADO
        EATFOOD, //USADO
        FOLLOW, //USADO
        FEAST, //USADO
        SCRATCH, //USADO
        HITSTUN, //USADO
        STUNNED, //USADO
        DEATH, //USADO
        FORGIVEN, //USADO
        CHAINED, //USADO
        THINKING //USADO
    }
    private EventFSM<BSInputs> _myFsm;
    EnemyLife _enemyLife;
    private Rigidbody _myRb;
    private Renderer _myRen;
    private Animator _anim;
    PlayerLife player;
    MeatLife meat;
    [SerializeField] TextMeshProUGUI[] texts;
    [SerializeField] TextMeshProUGUI debugState;

    #region PRECONDITIONS AND OBJECTIVES
    //PRECONDITIONS
    bool _isPlayerOnSight,
         _isFoodOnSight,
         _isPlayerResting,
         _isPlayerOnScratchRange,
         _isPlayerOnFeastRange,
         _isFoodOnFeastRange,
         _amIHungry,
         _amIDead,
         _amIStunned;

    //OBJECTIVES
    bool _isPlayerDead;
    bool[] allPreconditions;
    string[] allPreconditionsNames;
    #endregion

    #region VARIABLES
    [Header("<color=green>VARIABLES</color>")]
    [SerializeField] public int _hunger = 10;
    public int _hungerToBeHungry;
    float _counter = 0;
    public bool _animationFinished = false, debugPreConditions = false;
    float _currentDmg;
    Transform target;
    [SerializeField] GameObject karmicTrigger;
    [SerializeField] float _rotationSpeed = 5f;
    [SerializeField] float _dmgScratch = 11f;
    [SerializeField] float _dmgFeast = 7f;
    [SerializeField] int healPerBite = 8;
    [SerializeField] float _speed = 5;
    [SerializeField] float timeToEat = 1;
    [SerializeField] float timeToFeast = 1.25f;
    [SerializeField] float dstToFeast = 1;
    [SerializeField] float dstToScratch = 2;
    [SerializeField] float dstToReach = 1f;
    [SerializeField] float patrolRadius = 8f;
    [SerializeField] float randomPointTolerance = 4f;
    [SerializeField] float viewDistance = 15f;
    [SerializeField] float angleOfView = 45f;
    [SerializeField] LayerMask obstacles = 1 << 13; //Activo la layer 13 que va a ser obstacles
    #endregion

    #region VARIOUS VARIABLES
    Vector3 randomPointPatrol;
    #endregion

    private void OnEnable()
    {
        _enemyLife = GetComponent<EnemyLife>();
        _myRb = GetComponent<Rigidbody>();
        _myRen = GetComponent<Renderer>();
        _anim = GetComponent<Animator>();
        karmicTrigger = GetComponentInChildren<KarmicToggle>(true).gameObject;
        //karmicTrigger.SetActive(false);

        player = ServiceLocator.Instance.GetDependency<PlayerLife>();

        _myRb.excludeLayers = 0; //EXCLUYO NINGUNA LAYER

        if (_enemyLife != null) _enemyLife.OnHit += HandleOnHit;

        _hungerToBeHungry = UnityEngine.Random.Range(3, 8);

        #region STATES DECLARATION
        var thinking = new State<BSInputs>("THINKING");
        var patrol = new State<BSInputs>("PATROL");
        var eatFood = new State<BSInputs>("EATFOOD");
        var follow = new State<BSInputs>("FOLLOW");
        var feast = new State<BSInputs>("FEAST");
        var scratch = new State<BSInputs>("SCRATCH");
        var hitstun = new State<BSInputs>("HITSTUN");
        var stunned = new State<BSInputs>("STUNNED");
        var forgiven = new State<BSInputs>("FORGIVEN");
        var chained = new State<BSInputs>("CHAINED");
        var death = new State<BSInputs>("DEATH");
        #endregion

        #region STATE CONGIFURER
        StateConfigurer.Create(thinking)
            .SetTransition(BSInputs.THINKING, thinking)
            .SetTransition(BSInputs.PATROL, patrol)
            .SetTransition(BSInputs.EATFOOD, eatFood)
            .SetTransition(BSInputs.FOLLOW, follow)
            .SetTransition(BSInputs.FEAST, feast)
            .SetTransition(BSInputs.SCRATCH, scratch)
            .SetTransition(BSInputs.HITSTUN, hitstun)
            .SetTransition(BSInputs.STUNNED, stunned)
            .SetTransition(BSInputs.FORGIVEN, forgiven)
            .SetTransition(BSInputs.CHAINED, chained)
            .SetTransition(BSInputs.DEATH, death)
            .Done();

        StateConfigurer.Create(patrol)
            .SetTransition(BSInputs.THINKING, thinking)
            .SetTransition(BSInputs.HITSTUN, hitstun)
            .Done();

        StateConfigurer.Create(eatFood)
            .SetTransition(BSInputs.THINKING, thinking)
            .SetTransition(BSInputs.HITSTUN, hitstun)
            .Done();

        StateConfigurer.Create(follow)
            .SetTransition(BSInputs.THINKING, thinking)
            .SetTransition(BSInputs.HITSTUN, hitstun)
            .Done();

        StateConfigurer.Create(feast)
            .SetTransition(BSInputs.THINKING, thinking)
            .SetTransition(BSInputs.HITSTUN, hitstun)
            .Done();

        StateConfigurer.Create(scratch)
            .SetTransition(BSInputs.THINKING, thinking)
            .SetTransition(BSInputs.HITSTUN, hitstun)
            .Done();

        StateConfigurer.Create(hitstun)
            .SetTransition(BSInputs.THINKING, thinking)
            .SetTransition(BSInputs.HITSTUN, hitstun)
            .SetTransition(BSInputs.DEATH, death)
            .Done();

        StateConfigurer.Create(stunned)
            .SetTransition(BSInputs.THINKING, thinking)
            .SetTransition(BSInputs.HITSTUN, hitstun)
            .SetTransition(BSInputs.DEATH, death)
            .SetTransition(BSInputs.FORGIVEN, forgiven)
            .SetTransition(BSInputs.CHAINED, chained)
            .Done();

        StateConfigurer.Create(forgiven)
            .SetTransition(BSInputs.THINKING, thinking)
            .SetTransition(BSInputs.HITSTUN, hitstun)
            .Done();

        StateConfigurer.Create(chained)
            .SetTransition(BSInputs.THINKING, thinking)
            .SetTransition(BSInputs.HITSTUN, hitstun)
            .Done();

        StateConfigurer.Create(death)
            .SetTransition(BSInputs.THINKING, thinking)
            .SetTransition(BSInputs.HITSTUN, hitstun)
            .Done();
        #endregion

        #region STATE LOGIC

        #region THINKING
        thinking.OnEnter += x =>
        {
            _anim.SetBool("Idle", true);
        };
        thinking.OnUpdate += () =>
        {
            //Stunned o muerto (máxima prioridad)
            if (_amIStunned)
            {
                SendInputToFSM(BSInputs.STUNNED);
                return;
            }

            //Ataques al player (máxima prioridad)
            if (_isPlayerOnFeastRange && _amIHungry)
            {
                SendInputToFSM(BSInputs.FEAST);
                return;
            }
            if (_isPlayerOnScratchRange && !_amIHungry)
            {
                SendInputToFSM(BSInputs.SCRATCH);
                return;
            }

            //Player a la vista pero lejos
            if (_isPlayerOnSight)
            {
                SendInputToFSM(BSInputs.FOLLOW);
                return;
            }

            //Comida en rango cercano
            if (_isFoodOnFeastRange && !_isPlayerOnSight)
            {
                SendInputToFSM(BSInputs.EATFOOD);
                return;
            }

            //Comida a la vista pero lejos
            if (_isFoodOnSight && !_isPlayerOnSight)
            {
                SendInputToFSM(BSInputs.FOLLOW);
                return;
            }

            //No hay nada patrulla
            if (!_isPlayerOnSight && !_isFoodOnSight)
                SendInputToFSM(BSInputs.PATROL);
        };
        thinking.OnExit += x =>
        {
            _anim.SetBool("Idle", false);
        };
        #endregion

        #region PATROL
        patrol.OnEnter += x =>
        {
            _anim.SetBool("Follow", true);
            randomPointPatrol = GetRandomPointWithinARadius(patrolRadius);
            Vector3 dir = randomPointPatrol - transform.position;

            while (Physics.Raycast(transform.position, dir.normalized, dir.magnitude, obstacles) || Physics.CheckSphere(randomPointPatrol, randomPointTolerance, obstacles))
            {
                randomPointPatrol = GetRandomPointWithinARadius(patrolRadius);
                dir = randomPointPatrol - transform.position; // recalcular cada vez
            }
        };
        patrol.OnUpdate += () =>
        {
            LookAtParameterWithVector3(randomPointPatrol);
            if (_isPlayerOnSight)
                Think();
            if (GetDistanceIgnoringY(randomPointPatrol) < dstToReach) //SI LLEGUE EMPIEZO A PENSAR QUE HACER
            {
                Think();
            }
        };
        patrol.OnFixedUpdate += () =>
        {
            FollowParamWithRBV3(randomPointPatrol);
        };
        patrol.OnExit += x => _anim.SetBool("Follow", false);
        #endregion
        #region FOLLOW
        follow.OnEnter += x => _anim.SetBool("Follow", true);
        follow.OnUpdate += () =>
        {
            if (target != null)
            {
                LookAtParameterOnY(target.transform);
                bool obstacleBlocking = Physics.Raycast(transform.position, (target.position - transform.position).normalized, (target.position - transform.position).magnitude, obstacles);
                if (obstacleBlocking) //TIRO UN RAYO HACIA EL PLAYER, SI LO TAPA UN OBSTACLE
                {
                    Think();
                }

                if (Vector3.Distance(target.transform.position, transform.position) < dstToFeast) //SI LLEGUE EMPIEZO A PENSAR QUE HACER
                {
                    Think();
                }
            }
        };
        follow.OnFixedUpdate += () =>
        {
            if (target != null)
                FollowParamWithRB(target.transform);
        };
        follow.OnExit += x => _anim.SetBool("Follow", false);
        #endregion
        #region EAT FOOD
        eatFood.OnEnter += x =>
        {
            _anim.SetBool("EatFromFloor", true);
            _animationFinished = false;
            if (target.GetComponent<MeatLife>())
            {
                meat = target.GetComponent<MeatLife>();
                //Debug.Log(meat);
            }
        };
        eatFood.OnUpdate += () =>
        {
            var u = UtilitiesAgus.GetAnimatorStateProgress("Zombie Idle (3)@Zombie Biting", _anim);

            if (GenericCounter(timeToEat) && meat != null)
            {
                //Debug.Log($"eating {meat.name} for {_counter}");
                meat.TakeDamage(0);
                _enemyLife.TakeHeal(8);
                ResetCounter();
            }
            if (target == null || u.finished) //ME COMI TODA LA COMIDA, BAJO 1 A HUNGER
            {
                _hunger--;
                Think();
            }
        };
        eatFood.OnExit += x =>
        {
            _anim.SetBool("EatFromFloor", false);
            ResetCounter();
        };
        #endregion
        #region FEAST
        feast.OnEnter += x =>
        {
            //_enemyLife.isInvulnerable = true;
            AnimationFinishedFalse();
            _anim.SetBool("FeastBool", true);
            _currentDmg = _dmgFeast;
            //Debug.Log("ENTRO A FEAST");
        };
        feast.OnUpdate += () =>
        {
            var u = UtilitiesAgus.GetAnimatorStateProgress("Feast", _anim);

            LookAtParameterOnY(player.transform);
            //if (_animationFinished)
            //{
            //    _animationFinished = false;
            //    Think();
            //    //Debug.Log(_animationFinished + " Paso a THINKING");
            //}
            
            if (u.t01 >= 0.95f)
            {
                Think();
                return;
            }
        };
        feast.OnExit += x =>
        {
            _enemyLife.isInvulnerable = false;
            _anim.SetBool("FeastBool", false);
            AnimationFinishedFalse();
            SetRestingFalse();
            //Debug.Log("SALGO DE FEAST");
        };
        #endregion
        #region SCRATCH
        scratch.OnEnter += x =>
        {
            _animationFinished = false;
            _anim.SetTrigger("Attack");
            _currentDmg = _dmgScratch;
            //LLAMO DESDE LA ANIMACION A ANIMATIONEVENT DAMAGEPLAYER PARA QUE CONCUERDE CON EL TIMING DEL GOLPE O MORDIDA
        };
        scratch.OnUpdate += () =>
        {
            var u = UtilitiesAgus.GetAnimatorStateProgress("Zombie Attack", _anim);

            //if (_animationFinished)
            //    Think();

            if (u.finished)
            {
                Think();
            }
        };
        scratch.OnExit += x =>
        {
            _anim.ResetTrigger("Attack");
            _hunger++;
        };
        #endregion
        #region HITSTUN
        hitstun.OnEnter += x =>
        {
            _anim.SetTrigger("OnHit");
            _animationFinished = false;
            if (_enemyLife.IsDead)
            {
                //Debug.Log("No le puedo pegar mas a BS");
                _myRb.excludeLayers = 1 << 7; //EXCLUYO LA LAYER DEL PLAYER SI ME MORI ANTES DE DEATH PARA PODER HACER LO DE STUNNED
            }
        };
        hitstun.OnUpdate += () =>
        {
            LookAtParameterOnY(player.transform);

            if (UtilitiesAgus.GetAnimatorStateProgress("Zombie Reaction Hit", _anim).finished)
                Think();
        };
        hitstun.OnExit += x =>
        {
            _anim.ResetTrigger("OnHit");
        };
        #endregion
        #region STUNNED
        stunned.OnEnter += x => //EN STUNNED HAGO QUE EL PLAYER NO LE PUEDA PEGAR ASI PODES ELEGIR LA OPCIONA PARA EL KARMA
        {
            _anim.SetTrigger("Stunned");
            _myRb.excludeLayers = 1 << 7; //EXCLUYO LA LAYER DEL PLAYER
            karmicTrigger.SetActive(true);
        };
        stunned.OnUpdate += () =>
        {

        };
        stunned.OnExit += x =>
        {

        };
        #endregion
        #region DEATH
        death.OnEnter += x =>
        {
            _anim.SetTrigger("Death");
            _amIDead = true;
        };
        death.OnUpdate += () =>
        {
        };
        #endregion
        #region FORGIVEN
        forgiven.OnEnter += x =>
        {
            _anim.SetTrigger("KneelDown");
            _animationFinished = false;
        };
        forgiven.OnUpdate += () =>
        {

        };
        forgiven.OnExit += x =>
        {

        };
        #endregion
        #region CHAINED
        chained.OnEnter += x =>
        {
            _anim.SetTrigger("Agonize");
            _animationFinished = false;
        };
        chained.OnUpdate += () =>
        {

        };
        chained.OnExit += x =>
        {

        };
        #endregion

        #endregion

        _myFsm = new EventFSM<BSInputs>(thinking);
    }

    private void OnDisable()
    {
        // 🔹 Desuscribirse de eventos EXTERNOS
        if (_enemyLife != null)
            _enemyLife.OnHit -= HandleOnHit;

        // 🔹 FSM (basta con nullificar, las lambdas internas se liberan solas)
        _myFsm = null;

        // 🔹 Reset de animaciones
        if (_anim != null)
        {
            _anim.SetBool("Idle", false);
            _anim.SetBool("Follow", false);
            _anim.SetBool("EatFromFloor", false);
            _anim.SetBool("FeastBool", false);

            _anim.ResetTrigger("Attack");
            _anim.ResetTrigger("OnHit");
            _anim.ResetTrigger("Stunned");
            _anim.ResetTrigger("Death");
            _anim.ResetTrigger("KneelDown");
            _anim.ResetTrigger("Agonize");
        }

        // 🔹 Apagar objetos auxiliares
        //if (karmicTrigger != null)
        //    karmicTrigger.SetActive(false);

        // 🔹 Reset de flags internos
        _amIDead = false;
        _amIStunned = false;
        _animationFinished = false;
    }

    private void OnDestroy()
    {
        // ⚠️ Llamamos la misma limpieza, por seguridad
        OnDisable();
    }

    #region VARIOUS METHODS
    public void GrabAttempt()
    {
        if (Vector3.Distance(player.transform.position, transform.position) <= dstToFeast)
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
    public void Forgive() => SendInputToFSM(BSInputs.FORGIVEN);
    public void Chain() => SendInputToFSM(BSInputs.CHAINED);
    public void Die() => SendInputToFSM(BSInputs.DEATH);
    void HandleOnHit()
    {
        if (_amIStunned) return;

        SendInputToFSM(BSInputs.HITSTUN);
    }
    void SetRestingTrue()
    {
        if (ServiceLocator.Instance.TryGetDependency<PlayerMVC>(out var playerMVC))
            playerMVC.SetResting(true);
        else
            Debug.Log("No se encontro PlayerMVC en ServiceLocator");
    }
    void SetRestingFalse()
    {
        if (ServiceLocator.Instance.TryGetDependency<PlayerMVC>(out var playerMVC))
            playerMVC.SetResting(false);
        else
            Debug.Log("No se encontro PlayerMVC en ServiceLocator");
    }
    Vector3 GetDir(Transform param) => param.transform.position - transform.position;
    Vector3 GetDirWithV3(Vector3 vector) => (vector - transform.position).normalized;
    float GetDistanceIgnoringY(Vector3 param)
    {
        return Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(param.x, 0, param.z));
    }
    void LookAtParameterOnY(Transform param)
    {
        if(param == null) return;

        Vector3 dir = GetDir(param.transform);
        dir.y = 0;
        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
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
    public void Think() => SendInputToFSM(BSInputs.THINKING);
    bool GenericCounter(float time)
    {
        if (_counter != time)
        {
            _counter += Time.deltaTime;
            _counter = Mathf.Clamp(_counter, 0, time);
            return false;
        }
        else return true;
    }
    void ResetCounter() { _counter = 0; }
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
    }
    void DebugState()
    {
        //debugState.text = _myFsm.Current.Name;
    }
    void DebugPreconditions()
    {
        if (texts == null) return;

        if (!debugPreConditions)
        {
            for (int i = 0; i < texts.Length; i++)
            {
                texts[i].text = "";
            }
        }

        for (int i = 0; i < texts.Length; i++)
        {
            texts[i].text = $"{allPreconditionsNames[i].ToString()}: {allPreconditions[i].ToString()}";
        }
    }
    void ConditionChecker()
    {
        _isPlayerOnSight = CheckFOV().isOnSight && CheckFOV().isPlayer;
        _isFoodOnSight = CheckFOV().isOnSight && !CheckFOV().isPlayer;
        _isPlayerResting = ServiceLocator.Instance.GetDependency<PlayerMVC>().isResting;
        _isPlayerOnScratchRange = _isPlayerOnSight && CheckFOV().distance <= dstToScratch; //SOLO SI LO ESTOY MIRANDO ES TRUE QUE PUEDE PEGARLE
        _isPlayerOnFeastRange = _isPlayerOnSight && CheckFOV().distance <= dstToFeast; //SOLO SI LO ESTOY MIRANDO ES TRUE QUE PUEDA MORDERLO
        _isFoodOnFeastRange = _isFoodOnSight && CheckFOV().distance <= dstToFeast; //SOLO SI LO ESTOY MIRANDO ES TRUE QUE PUEDA MORDERLO
        _amIHungry = _hunger >= _hungerToBeHungry;
        _amIStunned = _enemyLife.Life <= 0f;

        allPreconditionsNames = new string[]
        {
        "_isPlayerOnSight",
        "_isFoodOnSight",
        "_isPlayerResting",
        "_isPlayerOnScratchRange",
        "_isPlayerOnFeastRange",
        "_isFoodOnFeastRange",
        "_amIHungry",
        "_amIStunned",
        "_isPlayerDead"
        };
        allPreconditions = new bool[]
        {
        _isPlayerOnSight,
        _isFoodOnSight,
        _isPlayerResting,
        _isPlayerOnScratchRange,
        _isPlayerOnFeastRange,
        _isFoodOnFeastRange,
        _amIHungry,
        _amIStunned,
        _isPlayerDead
        };

        //OBJECTIVE
        _isPlayerDead = ServiceLocator.Instance.GetDependency<PlayerLife>().Life <= 0f;
    }
    //(bool isOnSight, bool isPlayer, bool isFood, float distance) CheckFOV()
    //{
    //    bool isPlayerOnSight = false;
    //    bool isFoodOnSight = false;
    //    Collider[] colliders = Physics.OverlapSphere(transform.position, viewDistance);
    //    if (colliders == null || colliders.Length == 0) 
    //        return (false, false, false, Mathf.Infinity);

    //    foreach (Collider collider in colliders)
    //    {
    //        Vector3 directionToTarget = (collider.transform.position - transform.position).normalized;
    //        float angle = Vector3.Angle(transform.forward, directionToTarget);

    //        if (angle <= angleOfView / 2f)
    //        {
    //            if (collider.GetComponent<PlayerLife>() && !ObstacleCovering(player.transform)) //Si esta dentro de mi FOV, tiene playerLife y no hay GO en layer Obstacles cubriendo
    //            {
    //                float distance = Vector3.Distance(transform.position, collider.transform.position); //Saco la distancia y devuelvo
    //                target = collider.transform; //LE AGREGO FOOD A TARGET SI ENCONTRO AL PLAYER, AUNQUE YA TENGA REFERENCIA
    //                isPlayerOnSight = true;
    //            }
    //            else if (collider.GetComponent<MeatLife>() && !ObstacleCovering(collider.transform)) //Si esta dentro de mi FOV, tiene tag Food y no hay GO en layer Obstacles cubriendo
    //            {
    //                float distance = Vector3.Distance(transform.position, collider.transform.position); //Saco la distancia y vuelvo
    //                target = collider.transform; //LE AGREGO FOOD A TARGET SI ENCONTRO COMIDA
    //                isFoodOnSight = true;
    //            }
    //            else
    //            {
    //                isFoodOnSight = false;
    //                isPlayerOnSight = false;
    //            }
    //        }
    //    }

    //    return (false, isPlayerOnSight, isFoodOnSight, Mathf.Infinity);
    //}
    #region OLD CHECK FOV
    //(bool isOnSight, bool isPlayer, float distance) CheckFOV()
    //{
    //    bool sawPlayer = false;
    //    float bestPlayerDist = Mathf.Infinity;
    //    Transform bestPlayer = null;

    //    float bestFoodDist = Mathf.Infinity;
    //    Transform bestFood = null;

    //    Collider[] colliders = Physics.OverlapSphere(transform.position, viewDistance);
    //    if (colliders == null || colliders.Length == 0)
    //        return (false, false, Mathf.Infinity);

    //    foreach (var c in colliders)
    //    {
    //        Vector3 dir = c.transform.position - transform.position;
    //        float angle = Vector3.Angle(transform.forward, dir.normalized);
    //        if (angle > angleOfView * 0.5f) continue;

    //        if (ObstacleCovering(c.transform)) continue;

    //        float d = dir.magnitude;

    //        if (c.GetComponent<PlayerLife>() != null)
    //        {
    //            sawPlayer = true;
    //            if (d < bestPlayerDist)
    //            {
    //                bestPlayerDist = d;
    //                bestPlayer = c.transform;
    //            }
    //        }
    //        else if (c.GetComponent<MeatLife>() != null)
    //        {
    //            if (d < bestFoodDist)
    //            {
    //                bestFoodDist = d;
    //                bestFood = c.transform;
    //            }
    //        }
    //    }

    //    // Elegir target según prioridad (player > comida)
    //    Transform chosenTarget = null;
    //    float chosenDist = Mathf.Infinity;

    //    if (sawPlayer)
    //    {
    //        chosenTarget = bestPlayer;
    //        chosenDist = bestPlayerDist;
    //    }
    //    else if (bestFood != null)
    //    {
    //        chosenTarget = bestFood;
    //        chosenDist = bestFoodDist;
    //    }

    //    target = chosenTarget;
    //    bool any = (chosenTarget != null);

    //    return (any, sawPlayer, chosenDist);
    //}
    #endregion

    #region NEW CHECK FOV
    // ➤ VARIABLES NECESARIAS (ponerlas arriba con las demás)
    float playerMemoryTime = 1f;      // Cuánto tiempo recuerda al player
    float playerLastSeenTime = -999f; // Última vez que lo vio

    // ➤ MÉTODO CHECKFOV COMPLETO (reemplazar el tuyo)
    (bool isOnSight, bool isPlayer, float distance) CheckFOV()
    {
        bool sawPlayerThisFrame = false;
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

            // ——— DETECTA PLAYER ———
            if (c.GetComponent<PlayerLife>() != null)
            {
                sawPlayerThisFrame = true;
                playerLastSeenTime = Time.time;        // 👈 REGISTRA el frame donde lo vio

                if (d < bestPlayerDist)
                {
                    bestPlayerDist = d;
                    bestPlayer = c.transform;
                }
            }
            // ——— DETECTA COMIDA ———
            else if (c.GetComponent<MeatLife>() != null)
            {
                if (d < bestFoodDist)
                {
                    bestFoodDist = d;
                    bestFood = c.transform;
                }
            }
        }

        // ——————————————————————————————————————————
        //               PRIORIDAD INTELIGENTE
        // ——————————————————————————————————————————

        // Si lo vio hace poco, aunque este frame no esté en FOV → TODAVÍA LO RECUERDA
        bool rememberPlayer = (Time.time - playerLastSeenTime) <= playerMemoryTime;

        Transform chosenTarget = null;
        float chosenDist = Mathf.Infinity;

        if (sawPlayerThisFrame || rememberPlayer)
        {
            // Si no lo ve este frame pero lo recuerda → usa el último target player
            if (bestPlayer != null)
            {
                chosenTarget = bestPlayer;
                chosenDist = bestPlayerDist;
            }
            else if (target != null)
            {
                chosenTarget = target; // el último target fue el player
                chosenDist = Vector3.Distance(transform.position, target.position);
            }
        }
        else if (bestFood != null)
        {
            chosenTarget = bestFood;
            chosenDist = bestFoodDist;
        }

        target = chosenTarget;
        bool any = chosenTarget != null;

        return (any, sawPlayerThisFrame, chosenDist);
    }

    #endregion

    bool ObstacleCovering(Transform target)
    {
        Vector3 toTarget = target.position - transform.position;
        return Physics.Raycast(transform.position, toTarget.normalized, toTarget.magnitude, obstacles);
    }

    //float nextCheckTime = 0f;
    //float checkInterval = 0.5f;
    private void Update()
    {
        if (_amIDead) return;

        //if (Time.time >= nextCheckTime)
        //{
        //    nextCheckTime = Time.time + checkInterval;
        //    ConditionChecker();
        //}
        debugState.text = _myFsm.Current.Name;

        ConditionChecker();
        DebugPreconditions();
        DebugState();

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
    private void SendInputToFSM(BSInputs inp)
    {
        _myFsm.SendInput(inp);
        //Debug.Log($"Input enviado: {inp}");
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, viewDistance);
        //Quiero debuggear el angle con lineas
        Vector3 leftBoundary = Quaternion.Euler(0, -angleOfView / 2f, 0) * transform.forward * viewDistance;
        Vector3 rightBoundary = Quaternion.Euler(0, angleOfView / 2f, 0) * transform.forward * viewDistance;
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);

        if (player != null)
            Gizmos.DrawRay(transform.position, (player.transform.position - transform.position).normalized * viewDistance);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, dstToFeast);
        Gizmos.DrawWireSphere(transform.position, dstToScratch);

        if (randomPointPatrol != Vector3.zero)
            Gizmos.DrawSphere(randomPointPatrol, randomPointTolerance);
    }
    //void OnDestroy()
    //{
    //    if (_enemyLife != null) _enemyLife.OnHit -= HandleOnHit;
    //}
    private void OnTriggerEnter(Collider other)
    {
        //if (other.GetComponent<SwordCollider>() && _amIStunned) //SI ME PEGA LA ESPADA Y ESTOY STUNNED VOY A DEATH
        //{
        //    SendInputToFSM(BSInputs.DEATH);
        //}
    }
    #endregion

    #region NECESSARY DEPENDENCIES
    void AnimationFinishedFalse() { _animationFinished = false; }
    void AnimationFinishedTrue()
    {
        _animationFinished = true;
        //Debug.Log("TERMINE ANIMACION");
    }
    public void DamagePlayerAnimMethod()
    {
        if (_isPlayerOnFeastRange && _myFsm.Current.Name == "FEAST")
        {
            _hunger--;
            player?.TakeDamage(_currentDmg);
        }
        else if (_isPlayerOnScratchRange && _myFsm.Current.Name == "SCRATCH")
        {
            player?.TakeDamage(_currentDmg);
        }
    }
    public void DisableMovement()
    {

    }

    #region SLOWABLE
    float originalSpeed;
    public void SlowEntity()
    {
        originalSpeed = _speed;
        _speed /= 2;
        _anim.speed = .25f;

        Invoke("UnSlowEntity", 3.5f);
    }

    public void UnSlowEntity()
    {
        _speed = originalSpeed;
        _anim.speed = 1;
    }
    #endregion

    #endregion
}
