using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using IA2;
using System;
using JetBrains.Annotations;

[System.Serializable]
public struct AttacksData
{
    public float anticipationEnd, slashEnd;
    public float anticipationSpeed, slashSpeed, recoverySpeed;
    public float dmg;
}
public class AttackEFSM : MonoBehaviour
{
    #region ENUMS AND PRIVATE VARIABLES
    public enum FaithInputs { THINKING, LIGHT1, LIGHT2, LIGHT3, HEAVY1 }
    EventFSM<FaithInputs> _myFsm;
    Rigidbody _myRb; //MI RIGIDBODY POR LAS DUDAS
    Animator _anim; //ANIMATOR PARA LAS ANIMACIONES DE ATAQUE
    PlayerMVC _player; //PLAYER MVC PARA EL IS RESTING 
    PlayerLife _playerLife;
    DeathManager deathManager;
    //Collider _sword; //SWORD COLLIDER PARA ACTIVAR GOLPES
    ControllerPlayer _controller; //CONTROLLER PARA SUSCRIBIRME A LOS EVENTOS DEL CONTROLLER
    [HideInInspector] public float currentDmg;
    #endregion

    #region ATTACKS DATA
    [Header("<color=yellow>AttacksData (IMPORTANT! FILL ATTACKS DATA)</color>")]
    public AttacksData attack1;
    public AttacksData attack2;
    public AttacksData attack3;
    public AttacksData heavyAttack;
    #endregion

    #region HEAVY HOLD
    [Header("<color=purple>Heavy Hold</color>")]
    public float longPressTime = 0.25f;
    private bool clicking = false;
    private float pressedTime = 0f;
    private bool longPressInvoked = false;
    #endregion

    #region CDs
    [SerializeField] float maxLightCD = 0.35f;
    [SerializeField] float maxHeavyCD = 0.8f;
    float _counterLight, _counterHeavy;
    bool canHeavyAttack, canLightAttack;
    #endregion

    #region VARIOUS
    [SerializeField] float sphereCheck = 2f;
    [SerializeField] float angleFOV = 60f;
    #endregion

    #region VFX AND SFX VARIABLES
    [SerializeField] VisualEffect _slash;
    #endregion

    #region FLAGS
    bool hasPlayedSound = false, hasPlayedTrail = false, isClickingAgus = false, hasHitEnemy = false;
    #endregion

    #region INPUT BUFFER
    [Header("<color=green>Coyote Time</color>")]
    Queue<KeyCode> inputBuffer;
    [SerializeField] float unclickingWindow = .2f;
    #endregion //NO USANDO POR AHORA - CAMBIE POR UN BOOL QUE SE PRENDA Y CON UN INVOKE EN UNA VENTANA SE APAGUE

    private void OnEnable()
    {
        ServiceLocator.Instance.RegisterDependency<AttackEFSM>(this);
    }

    private void Start()
    {
        inputBuffer = new Queue<KeyCode>();

        #region GET COMPONENT DEPENDENCIES, SWORD START DISABLED
        _myRb = GetComponent<Rigidbody>();
        _anim = GetComponent<Animator>();
        _player = GetComponent<PlayerMVC>();
        _playerLife = GetComponent<PlayerLife>();
        //_sword = GetComponentInChildren<SwordCollider>().GetComponent<Collider>();
        deathManager = ServiceLocator.Instance.GetDependency<DeathManager>();
        //if (_sword != null) 
        //    _sword.enabled = false;
        _controller = ServiceLocator.Instance.GetDependency<ControllerPlayer>();

        _playerLife.OnPlayerHit += ResetPlayerOnHit;
        PauseManager.instance.OnResetScene += ResetPlayerOnHit;
        deathManager.OnRespawn += ResetPlayerOnHit;
        #endregion

        #region CONTROLLER EVENT SUSCRIPTION
        _controller.OnMouseDown += LeftClickDown;
        _controller.OnMouseUp += LeftClickUp;
        _controller.OnMouse += LeftClick;
        #endregion

        #region STATES DECLARATION
        var thinking = new State<FaithInputs>("THINKING");
        var light1 = new State<FaithInputs>("LIGHT1");
        var light2 = new State<FaithInputs>("LIGHT2");
        var light3 = new State<FaithInputs>("LIGHT3");
        var heavy1 = new State<FaithInputs>("HEAVY1");
        #endregion

        #region STATES TRANSITION
        StateConfigurer.Create(thinking)
            .SetTransition(FaithInputs.LIGHT1, light1)
            .SetTransition(FaithInputs.HEAVY1, heavy1)
            .Done();

        StateConfigurer.Create(light1)
            .SetTransition(FaithInputs.LIGHT2, light2)
            .SetTransition(FaithInputs.THINKING, thinking)
            .Done();
        StateConfigurer.Create(light2)
            .SetTransition(FaithInputs.LIGHT3, light3)
            .SetTransition(FaithInputs.THINKING, thinking)
            .Done();
        StateConfigurer.Create(light3)
            .SetTransition(FaithInputs.THINKING, thinking).
            Done();

        StateConfigurer.Create(heavy1)
            .SetTransition(FaithInputs.THINKING, thinking).
            Done();
        #endregion

        #region STATES LOGIC
        //THINKING
        thinking.OnEnter += x => 
        {
            _controller.isAttacking = false;
            _counterLight = 0;
            _counterHeavy = 0;
        };
        thinking.OnUpdate += () =>
        {
            HandleCooldowns(); //METODO PARA SUMAR Y CAMBIAR BOOL DE COOLDOWNS

            var u = UtilitiesAgus.GetAnimatorStateProgress("Running BT", _anim);
            //if (canLightAttack)
            //    SendInputToFSM(FaithInputs.LIGHT1);
            
            //if (u.inState) _controller.isAttacking = false;
            
            //if (Input.GetMouseButtonDown(1) && canHeavyAttack)
                //SendInputToFSM(FaithInputs.HEAVY1);
        };
        //LIGHT1
        light1.OnEnter += x =>
        {
            _controller.isAttacking = true;
            Debug.Log(_controller.isAttacking);
            _anim.SetTrigger("hit1");
            currentDmg = attack1.dmg;

            hasHitEnemy = false;

            StartCoroutine(RotateToCamera(.2f));
        };
        light1.OnUpdate += () =>
        {
            var u = GetStateProgress("hit1");
            var result = CheckEnemiesInSlash(100f); //ME GUARDO EL RESULTADO DE CHECK ENEMIES Y USO SUS VALORES

            #region HANDLE ANIMATION SPEEDS
            //Handle animation speeds (until end of anticipation put anticipation Speed)
            bool canAntiSpeed = u.t01 <= (attack1.anticipationEnd / 68f); //FRAMES ARE ALWAYS CONSTANT
            bool canSlashSpeed = u.t01 <= (attack1.slashEnd / 68f) && !canAntiSpeed; //if we are not in Anticipation and frames are lower than slash we are in slash
            bool canRecoverySpeed = u.t01 > (attack1.slashEnd / 68f) && !u.finished; //if not in slash and not finished i am in recovery
                                                                                     //bool canCombo = u.t01 >= ((attack1.slashEnd + 68f) / 2f); FOR NOW THE WINDOW FOR COMBO WILL BE IN RECOVERY
                                                                                     //if normalized time more than average i can combo and go directly to other attack
            #endregion

            #region FRAME DATA HANDLING
            if (!u.inState) return;

            if (canAntiSpeed)
            {
                _anim.speed = attack1.anticipationSpeed;
                //Debug.Log($"ATTACK1 in anticipation {canAntiSpeed}");
            }
            else if (canSlashSpeed) //SLASH DURATION I TURN ON SWORD, TURN ON TRAILACTIVATOR WITH A FLAG AND TURN ON SOUND WITH A FLAG
            {
                _anim.speed = attack1.slashSpeed;
                //_sword.enabled = true;
                //Debug.Log($"ATTACK1 in slash {canSlashSpeed}, Sword Enabled {_sword.enabled}");

                if (result.isOnFOV && !hasHitEnemy)
                {
                    hasHitEnemy = true;

                    foreach (Entity e in result.entities)
                    {
                        DamageEntity(e);
                    }
                }

                if (!hasPlayedSound)
                {
                    hasPlayedSound = true;
                    PlayWhooshSound(); //IT ALREADY HAS DEBUG
                }
                //LOGICA DE TRAIL CON UN FLAG TAMBN
                if (!hasPlayedTrail)
                {
                    hasPlayedTrail = true;
                    _slash.SendEvent("attack1");
                }
            }
            else if (canRecoverySpeed)
            {
                //_sword.enabled = false;
                _anim.speed = attack1.recoverySpeed;
                //Debug.Log($"ATTACK1 in recovery {canRecoverySpeed}, Sword Enabled {_sword.enabled}");
                if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonUp(0))
                {
                    SendInputToFSM(FaithInputs.LIGHT2);
                }
            }
            else if (u.finished)//IM FINISHED
            {
                //Debug.Log($"ATTACK2 hasFinished {u.finished}. going to thinking");
                Think();
            }
            #endregion
        };
        light1.OnExit += x => //JUST IN CAUSE TURN OFF SWORD, ANIM TO 1 AGAIN FLAGS RESET
        {
            hasPlayedSound = false;
            hasPlayedTrail = false;
            //_sword.enabled = false;
            _anim.speed = 1;
            //_controller.isAttacking = false;
            _anim.ResetTrigger("hit1");
        };
        //LIGHT2
        light2.OnEnter += x =>
        {
            //Debug.Log("ESTOY EN LIGHT 2");
            _anim.SetTrigger("hit2");

            hasHitEnemy = false;
            currentDmg = attack2.dmg;

            StartCoroutine(RotateToCamera(.2f));
        };
        light2.OnUpdate += () =>
        {
            var u = GetStateProgress("hit2");
            var result = CheckEnemiesInSlash(100f); //ME GUARDO EL RESULTADO DE CHECK ENEMIES Y USO SUS VALORES

            #region HANDLE ANIMATION SPEEDS
            //Handle animation speeds (until end of anticipation put anticipation Speed)
            bool canAntiSpeed = u.t01 <= (attack2.anticipationEnd / 135f); //FRAMES ARE ALWAYS CONSTANT
            bool canSlashSpeed = u.t01 <= (attack2.slashEnd / 135f) && !canAntiSpeed; //if we are not in Anticipation and frames are lower than slash we are in slash
            bool canRecoverySpeed = u.t01 > (attack2.slashEnd / 135f) && !u.finished; //if not in slash and not finished i am in recovery
            bool canCombo = u.t01 >= ((attack2.slashEnd + 135f) / 2f);
            //if normalized time more than average i can combo and go directly to other attack
            #endregion

            #region FRAME DATA HANDLING
            if (!u.inState) return;

            if (canAntiSpeed)
            {
                _anim.speed = attack2.anticipationSpeed;
                //Debug.Log($"ATTACK2 in anticipation {canAntiSpeed}");
            }
            else if (canSlashSpeed) //SLASH DURATION I TURN ON SWORD, TURN ON TRAILACTIVATOR WITH A FLAG AND TURN ON SOUND WITH A FLAG
            {
                _anim.speed = attack2.slashSpeed;
                //_sword.enabled = true;
                //Debug.Log($"ATTACK2 in slash {canSlashSpeed}, Sword Enabled {_sword.enabled}");

                if (result.isOnFOV && !hasHitEnemy)
                {
                    hasHitEnemy = true;

                    foreach (Entity e in result.entities)
                    {
                        DamageEntity(e);
                    }
                }

                if (!hasPlayedSound)
                {
                    hasPlayedSound = true;
                    PlayWhooshSound(); //IT ALREADY HAS DEBUG
                }
                //LOGICA DE TRAIL CON UN FLAG TAMBN
                if (!hasPlayedTrail)
                {
                    hasPlayedTrail = true;
                    _slash.SendEvent("attack2");
                }
            }
            else if (canRecoverySpeed)
            {
                //_sword.enabled = false;
                _anim.speed = attack2.recoverySpeed;
                //Debug.Log($"ATTACK2 in recovery {canRecoverySpeed}, Sword Enabled {_sword.enabled}");

                if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonUp(0))
                {
                    SendInputToFSM(FaithInputs.LIGHT3);
                }
            }
            else if (u.finished)//IM FINISHED
            {
                //Debug.Log($"ATTACK2 hasFinished {u.finished}. going to thinking");
                Think();
            }

            #endregion
        };
        light2.OnExit += x => //JUST IN CAUSE TURN OFF SWORD, ANIM TO 1 AGAIN FLAGS RESET
        {
            hasPlayedSound = false;
            hasPlayedTrail = false;
            //_sword.enabled = false;
            _anim.speed = 1;
            //_controller.isAttacking = false;
            _anim.ResetTrigger("hit2");
        };
        //LIGHT2
        light3.OnEnter += x =>
        {
            //Debug.Log("ESTOY EN LIGHT 3");
            _anim.SetTrigger("hit3");

            hasHitEnemy = false;
            currentDmg = attack3.dmg;

            StartCoroutine(RotateToCamera(.2f));
        };
        light3.OnUpdate += () =>
        {
            var u = GetStateProgress("hit3");
            var result = CheckEnemiesInSlash(80f); //ME GUARDO EL RESULTADO DE CHECK ENEMIES Y USO SUS VALORES

            #region HANDLE ANIMATION SPEEDS
            //Handle animation speeds (until end of anticipation put anticipation Speed)
            bool canAntiSpeed = u.t01 <= (attack3.anticipationEnd / 105f); //FRAMES ARE ALWAYS CONSTANT
            bool canSlashSpeed = u.t01 <= (attack3.slashEnd / 105f) && !canAntiSpeed; //if we are not in Anticipation and frames are lower than slash we are in slash
            bool canRecoverySpeed = u.t01 > (attack3.slashEnd / 105f) && !u.finished; //if not in slash and not finished i am in recovery
            bool canCombo = u.t01 >= ((attack3.slashEnd + 105f) / 2f);
            //if normalized time more than average i can combo and go directly to other attack
            #endregion

            #region FRAME DATA HANDLING
            if (!u.inState) return;

            if (canAntiSpeed)
            {
                _anim.speed = attack3.anticipationSpeed;
                //Debug.Log($"ATTACK3 in anticipation {canAntiSpeed}");
            }
            else if (canSlashSpeed) //SLASH DURATION I TURN ON SWORD, TURN ON TRAILACTIVATOR WITH A FLAG AND TURN ON SOUND WITH A FLAG
            {
                _anim.speed = attack3.slashSpeed;
                //_sword.enabled = true;
                //Debug.Log($"ATTACK3 in slash {canSlashSpeed}, Sword Enabled {_sword.enabled}");

                if (result.isOnFOV && !hasHitEnemy)
                {
                    hasHitEnemy = true;

                    foreach (Entity e in result.entities)
                    {
                        DamageEntity(e);
                    }
                }

                if (!hasPlayedSound)
                {
                    hasPlayedSound = true;
                    PlayWhooshSound(); //IT ALREADY HAS DEBUG
                }
                //LOGICA DE TRAIL CON UN FLAG TAMBN
                if (!hasPlayedTrail)
                {
                    hasPlayedTrail = true;
                    _slash.SendEvent("attack3");
                }
            }
            else if (canRecoverySpeed)
            {
                //_sword.enabled = false;
                _anim.speed = attack3.recoverySpeed;
                //Debug.Log($"ATTACK3 in recovery {canRecoverySpeed}, Sword Enabled {_sword.enabled}");

            }
            else if (u.finished)//IM FINISHED
            {
                //Debug.Log($"ATTACK2 hasFinished {u.finished}. going to thinking");
                Think();
            }

            #endregion
        };
        light3.OnExit += x => //JUST IN CAUSE TURN OFF SWORD, ANIM TO 1 AGAIN FLAGS RESET
        {
            hasPlayedSound = false;
            hasPlayedTrail = false;
            //_sword.enabled = false;
            _anim.speed = 1;
            //_controller.isAttacking = false;
            _anim.ResetTrigger("hit3");
        };
        //HEAVY1
        heavy1.OnEnter += x =>
        {
            //Debug.Log("ESTOY EN HEAVY 1");
            _anim.SetTrigger("heavyHit");

            hasHitEnemy = false;
            currentDmg = heavyAttack.dmg;

            StartCoroutine(RotateToCamera(.2f));
        };
        heavy1.OnUpdate += () =>
        {
            var u = GetStateProgress("heavyHit");
            var result = CheckEnemiesInSlash(120f); //ME GUARDO EL RESULTADO DE CHECK ENEMIES Y USO SUS VALORES

            #region HANDLE ANIMATION SPEEDS
            //Handle animation speeds (until end of anticipation put anticipation Speed)
            bool canAntiSpeed = u.t01 <= (heavyAttack.anticipationEnd / 224f); //FRAMES ARE ALWAYS CONSTANT
            bool canSlashSpeed = u.t01 <= (heavyAttack.slashEnd / 224f) && !canAntiSpeed; //if we are not in Anticipation and frames are lower than slash we are in slash
            bool canRecoverySpeed = u.t01 > (heavyAttack.slashEnd / 224f) && !u.finished; //if not in slash and not finished i am in recovery
            bool canCombo = u.t01 >= ((heavyAttack.slashEnd + 224f) / 2f);
            //if normalized time more than average i can combo and go directly to other attack
            #endregion

            #region FRAME DATA HANDLING
            if (!u.inState) return;

            if (canAntiSpeed)
            {
                _anim.speed = heavyAttack.anticipationSpeed;
                //Debug.Log($"ATTACK3 in anticipation {canAntiSpeed}");
            }
            else if (canSlashSpeed) //SLASH DURATION I TURN ON SWORD, TURN ON TRAILACTIVATOR WITH A FLAG AND TURN ON SOUND WITH A FLAG
            {
                _anim.speed = heavyAttack.slashSpeed;
                //_sword.enabled = true;
                //Debug.Log($"ATTACK3 in slash {canSlashSpeed}, Sword Enabled {_sword.enabled}");

                if (result.isOnFOV && !hasHitEnemy)
                {
                    hasHitEnemy = true;

                    foreach (Entity e in result.entities)
                    {
                        DamageEntity(e);
                    }
                }

                if (!hasPlayedSound)
                {
                    hasPlayedSound = true;
                    PlayWhooshSound(); //IT ALREADY HAS DEBUG
                }
                //LOGICA DE TRAIL CON UN FLAG TAMBN
                if (!hasPlayedTrail)
                {
                    hasPlayedTrail = true;
                    //_slash.SendEvent("attack3");
                }
            }
            else if (canRecoverySpeed)
            {
                //_sword.enabled = false;
                _anim.speed = heavyAttack.recoverySpeed;
                //Debug.Log($"heavyAttack in recovery {canRecoverySpeed}, Sword Enabled {_sword.enabled}");

            }
            else //IM FINISHED
            {
                //Debug.Log($"heavyAttack hasFinished {u.finished}. going to thinking");
                Think();
            }

            #endregion
        };
        heavy1.OnExit += x => //JUST IN CAUSE TURN OFF SWORD, ANIM TO 1 AGAIN FLAGS RESET
        {
            hasPlayedSound = false;
            hasPlayedTrail = false;
            //_sword.enabled = false;
            _anim.speed = 1;
            _controller.isAttacking = false;
            _anim.ResetTrigger("heavyHit");
        };
        #endregion

        //DECLARE THINKING AS FIRST STATE
        _myFsm = new EventFSM<FaithInputs>(thinking);
    }

    #region LEFT CLICK HANDLER (CAMBIADO A LEFT LIGHTATTK Y RIGHT HEAVYATTK)
    void LeftClickDown() //PRENDO CLICKING - PRESSEDTIME SE REINICIA - LONGPRESSED SE HACE FALSO
    {
        clicking = true;
        //Debug.Log("CLICKING TRUE");
        pressedTime = 0f;
        longPressInvoked = false;
    }
    void LeftClickUp()
    {
        if (clicking && !_controller.isAttacking) //SI CLICKING Y NO ESTOYATACANDO
        {
            if (!longPressInvoked) //Y SI LONGPRESSED ES FALSE Y COMBOQUEUED ES FALSE 
            {
                //isClickingAgus = true;
                SendInputToFSM(FaithInputs.LIGHT1);
                //Invoke("UnClickBool", unclickingWindow);
                //ENQUEUE INPUT
                //if (inputBuffer.Count < 4)
                //{
                //    inputBuffer.Enqueue(KeyCode.Mouse0);
                //    SendInputToFSM(FaithInputs.LIGHT1); //GO TO LIGHT 1 IF YOU ARE IN THINKING
                //    Debug.LogWarning($"Sumo a inputBuffer InputCount {inputBuffer.Count}");
                //    Invoke("DequeueInput", 1.5f);
                //}
            }
            clicking = false;
        }
    }
    //void UnClickBool()
    //{
    //    isClickingAgus = false;
    //    Debug.Log("COYOTE DEL CLICKING FALSE");
    //}
    void LeftClick()
    {
        if (clicking && canHeavyAttack && !_controller.isAttacking)
        {
            pressedTime += Time.deltaTime;
            if (!longPressInvoked && pressedTime >= longPressTime)
            {
                longPressInvoked = true;
                //DO HEAVY ATTACK
                SendInputToFSM(FaithInputs.HEAVY1);
            }
        }
    }
    #endregion

    #region VARIOUS METHODS
    //void DequeueInput()
    //{
    //    //Debug.Log("Borrando input...");
    //    inputBuffer.Dequeue();
    //}

    (bool isOnFOV, List<Entity> entities) CheckEnemiesInSlash(float fovAngle)
    {
        List<Entity> found = new List<Entity>();
        Collider[] hits = Physics.OverlapSphere(transform.position, sphereCheck);

        if (hits == null || hits.Length == 0)
            return (false, found);

        foreach (Collider col in hits)
        {
            Entity entity = col.GetComponent<Entity>();

            if (!col.GetComponent<EnemyLife>() && !col.GetComponent<BossLife>()) continue;
            
            Vector3 dirToTarget = (col.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, dirToTarget);

            if (angle < fovAngle * 0.5f)
            {
                found.Add(entity);
            }
        }

        return (found.Count > 0, found);
    }


    void DamageEntity(Entity entity) => entity.TakeDamage(currentDmg);

    public void ResetPlayerOnHit()
    {
        hasPlayedSound = false;
        hasPlayedTrail = false;
        //_sword.enabled = false;
        _anim.speed = 1;
        _controller.isAttacking = false;
        Think(); //If they hit me, I reset all variables to normal and I go to Think
    }
    public IEnumerator RotateToCamera(float duration)
    {
        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.Euler(0f, Camera.main.transform.rotation.eulerAngles.y, 0f);
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.rotation = targetRotation;
    }
    void HandleCooldowns()
    {
        if (_counterLight != maxLightCD && !_controller.isAttacking) //SUMO CD SOLO SI NO ESTOY ATACANDO
        {
            _counterLight += Time.deltaTime;
        }
        if (_counterHeavy != maxHeavyCD && !_controller.isAttacking) //ACA LO MISMO, SUMO SOLO SI NO ESTOY ATACANDO
        {
            _counterHeavy += Time.deltaTime;
        }
        canLightAttack = _counterLight >= maxLightCD;
        canHeavyAttack = _counterHeavy >= maxHeavyCD;
    }

    /// <summary>
    /// RETURNS IF IM IN STATE, NORMALIZED TIME FROM 0 TO 1 AND IF IT HAS MORE THAN 90% EXIT TIME AND ALSO IS IN STATE
    /// </summary>
    /// <param name="stateName"></param>
    /// <returns></returns>
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
    public void Think()
    {
        SendInputToFSM(FaithInputs.THINKING);
    }
    #endregion

    #region UPDATE, FIXED, LATE, ONDESTROY AND SENDINPUT
    private void Update()
    {
        _myFsm.Update();
    }
    private void FixedUpdate()
    {
        _myFsm.FixedUpdate();
    }
    private void LateUpdate()
    {
        _myFsm.LateUpdate();
    }
    private void SendInputToFSM(FaithInputs inp)
    {
        _myFsm.SendInput(inp);
        //Debug.Log($"Input enviado: {inp}");
    }
    private void OnDestroy()
    {
        if (_playerLife != null)
            _playerLife.OnPlayerHit -= ResetPlayerOnHit;
        if (PauseManager.instance != null)
            PauseManager.instance.OnResetScene -= ResetPlayerOnHit;
        if(deathManager != null)
            deathManager.OnRespawn -= ResetPlayerOnHit;
        if (_controller != null)
        {
            _controller.OnMouseDown -= LeftClickDown;
            _controller.OnMouseUp -= LeftClickUp;
            _controller.OnMouse -= LeftClick;
        }

        ServiceLocator.Instance.RemoveDependency<AttackEFSM>();
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position,sphereCheck);
    }
    #endregion

    #region VFX AND SFX
    public void PlayWhooshSound() 
    {
        //Debug.Log("WHOOOSH!");
        SoundManager.Instance.PlayOneShotFromIndex(6); 
    }
    public void TrailActivator()
    {
        //AGREGAR LO DE GIAN
    }
    #endregion
}
