//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.VFX;

//public class NewAttackManager : MonoBehaviour
//{
//    #region CDs
//    [SerializeField] float maxLightCD = 0.35f;
//    [SerializeField] float maxHeavyCD = 0.8f;
//    float _counterLight, _counterHeavy;
//    bool canHeavyAttack, canLightAttack;
//    #endregion

//    Animator _anim;
//    ControllerPlayer _controller;
//    PlayerLife playerLife;
//    [Header("<color=purple>Dependencies</color>")]
//    [SerializeField] VisualEffect _trail;
//    [SerializeField] VisualEffect _slash;
//    [SerializeField] Collider _sword;

//    [Header("<color=orange>Variables</color>")]
//    [HideInInspector] public float currentDmg;
//    [SerializeField] float comboBufferTime = 0.2f; // 200 ms extra
//    float comboBufferTimer = 0f;
//    [SerializeField] float comboCoyoteTime = 0.15f; // 150ms extra
//    float comboCoyoteTimer = 0f;
//    [SerializeField] float lerpSpeed = 5f;

//    [Header("<color=yellow>AttacksData (FILL ATTACKS DATA)</color>")]
//    public AttacksData attack1;
//    public AttacksData attack2;
//    public AttacksData attack3;
//    public AttacksData heavyAttack;
//    AttacksData _nextAttack;
//    int currentAttkIndex = 1;

//    [Header("<color=green>Heavy Hold</color>")]
//    public float longPressTime = 0.25f;
//    private bool clicking = false;
//    private float pressedTime = 0f;
//    private bool longPressInvoked = false;
//    public float fullyChargedDmg = 25f;

//    #region MONOBEHAVIOUR METHODS
//    private void Awake()
//    {
//        _anim = GetComponent<Animator>();
//        _sword = GetComponentInChildren<SwordCollider>().GetComponent<Collider>();
//        ServiceLocator.Instance.RegisterDependency<NewAttackManager>(this);
//    }
//    private void Start()
//    {
//        _controller = ServiceLocator.Instance.GetDependency<ControllerPlayer>();

//        playerLife = ServiceLocator.Instance.GetDependency<PlayerLife>();
//        playerLife.OnPlayerHit += StopAllAttacksOnHit;

//        _sword.enabled = false;

//        _controller.OnMouseDown += LeftClickDown;
//        _controller.OnMouseUp += LeftClickUp;
//        _controller.OnMouse += LeftClick;
//        _controller.isAttacking = false;
//    }
//    private void Update()
//    {
//        DEMO PARA PROBAR SI LO QUE ESTA JODIENDO ES EL ISATTACKING
//        if (Input.GetKeyDown(KeyCode.L))
//            StopAllAttacksOnHit();

//        if (_counterLight != maxLightCD && !_controller.isAttacking) //SUMO CD SOLO SI NO ESTOY ATACANDO
//        {
//            _counterLight = Mathf.Clamp(_counterLight + Time.deltaTime, 0, maxLightCD);
//        }
//        if (_counterHeavy != maxHeavyCD && !_controller.isAttacking) //ACA LO MISMO, SUMO SOLO SI NO ESTOY ATACANDO
//        {
//            _counterHeavy = Mathf.Clamp(_counterHeavy + Time.deltaTime, 0, maxHeavyCD);
//        }
//        canLightAttack = _counterLight == maxLightCD;
//        canHeavyAttack = _counterHeavy == maxHeavyCD;

//        if (Input.GetMouseButtonDown(0))
//            comboBufferTimer = comboBufferTime; // guardo el click
//        else if (comboBufferTimer > 0f)
//            comboBufferTimer -= Time.deltaTime;

//        if (comboCoyoteTimer > 0f)
//            comboCoyoteTimer -= Time.deltaTime;

//        Debug.Log($"Sword: {_sword.enabled}");
//    }
//    private void OnDestroy()
//    {
//        _controller.OnMouseDown -= LeftClickDown;
//        _controller.OnMouseUp -= LeftClickUp;
//        _controller.OnMouse -= LeftClick;

//        playerLife.OnPlayerHit -= StopAllAttacksOnHit;
//    }
//    #endregion

//    #region CONTROLLER ACTION METHODS
//    void LeftClickDown() //PRENDO CLICKING - PRESSEDTIME SE REINICIA - LONGPRESSED SE HACE FALSO
//    {
//        clicking = true;
//        Debug.Log("CLICKING TRUE");
//        pressedTime = 0f;
//        longPressInvoked = false;
//    }
//    void LeftClickUp()
//    {
//        if (clicking && canLightAttack && !_controller.isAttacking) //SI CLICKING PUEDO ATACAR Y NO ESTOYATACANDO
//        {
//            if (!longPressInvoked) //Y SI LONGPRESSED ES FALSE Y COMBOQUEUED ES FALSE 
//            {
//                currentAttkIndex = 1;
//                StartCoroutine(DoLightAttack(attack1)); //HAGO LIGHTATTACK 1 SIEMPRE
//            }
//            clicking = false;
//        }
//    }
//    void LeftClick()
//    {
//        if (clicking && canHeavyAttack && !_controller.isAttacking)
//        {
//            pressedTime += Time.deltaTime;
//            if (!longPressInvoked && pressedTime >= longPressTime)
//            {
//                longPressInvoked = true;
//                StartCoroutine(DoHeavyAttack());
//            }
//        }
//    }
//    #endregion

//    #region ATTACK METHODS
//    IEnumerator DoLightAttack(AttacksData attack) //ENTRO A ATTACK1 DESDE LEFTCLICKUP
//    {
//        CHELO WAS HERE: ROTACION A DONDE MIRA LA CAMARA
//        StartCoroutine(RotateToCamera(.2f));


//        currentDmg = attack.dmg; //CAMBIO DANIO
//        _anim.SetTrigger("hit" + currentAttkIndex); //SETTEO ANIMACION EN BASE A INDEX
//        _controller.isAttacking = true; //ESTOY ATACANDO

//        bool trailPlayed = false; //BOOL TEMP PARA TRAIL
//        bool comboQueued = false; //BOOL PARA HACER COMBO

//        yield return new WaitUntil(() => UtilitiesAgus.GetAnimatorStateProgress(attack.stateName, _anim).inState); //Hace yield return hasta que entres al estado correcto
//        while (true) //CUANDO ESTOY EN EL ESTADO CORRECTO ENTRO AL WHILE
//        {
//            var u = UtilitiesAgus.GetAnimatorStateProgress(attack.stateName, _anim); //ME TRAIGO LA INFO DE ANIMATION

//            Debug.Log($"{u.inState} and {u.t01}, playing hit + {currentAttkIndex}");
//            TrailActivator();

//            ANTICIPATION
//            if (u.t01 < (attack.anticipationEnd / attack.maxFrames)) //SI ANIM TIME ES MENOR A FRAME DE ANTICIPATION
//            {
//                _anim.speed = attack.anticipationSpeed; //SETTEO VELOCIDAD
//                _sword.enabled = false; //APAGO COLLIDER
//                Debug.Log("ESTOY EN ANTI");
//            }
//            SLASH
//            else if (u.t01 < (attack.slashEnd / attack.maxFrames)) //SI ES MAYOR A ANTICIPATION Y MENOR A SLASH
//            {
//                _anim.speed = attack.slashSpeed; //SETTEO SLASHSPEED
//                _sword.enabled = true; //PRENDO COL
//                Debug.Log("PODES CLICKEAR PARA COMBEAR");
//                if (!trailPlayed) // s�lo una vez
//                {
//                    TrailActivator();
//                    _slash.SendEvent($"attack{currentAttkIndex}");
//                    trailPlayed = true;
//                }
//            }
//            else if (!u.finished) // Recovery
//            {
//                _anim.speed = attack.recoverySpeed;
//                _sword.enabled = false;

//                if (!comboQueued && (comboBufferTimer > 0f || comboCoyoteTimer > 0f))
//                {
//                    comboBufferTimer = 0f;
//                    comboCoyoteTimer = 0f;
//                    comboQueued = true;

//                    currentAttkIndex++;
//                    if (currentAttkIndex > 3) currentAttkIndex = 1;

//                    NextAttackChecker();
//                    StartCoroutine(DoLightAttack(_nextAttack));
//                    break;
//                }
//            }
//            else
//            {
//                _controller.isAttacking = false;
//                Debug.Log("Llegue al ELSE isAttacking es " + _controller.isAttacking);
//                _anim.speed = 1f;
//                comboCoyoteTimer = comboCoyoteTime; // arrancar coyote cuando termina
//                break;
//            }

//            yield return null;
//        }

//        _anim.speed = 1f; //SETTEO NORMAL SPEED
//        _sword.enabled = false; //APAGO COL
//        _counterLight = 0; //COUNTER EN 0 PARA CORRER CD
//    }
//    IEnumerator DoHeavyAttack() //ENTRA A HEAVY ATTACK 
//    {
//        CHELO WAS HERE: ROTACION A DONDE MIRA LA CAMARA
//        StartCoroutine(RotateToCamera(.2f));

//        currentDmg = heavyAttack.dmg; //CAMBIO DANIO
//        _anim.SetTrigger("heavyHit"); //SETTEO ANIMACION A HEAVY
//        _controller.isAttacking = true; //ESTOY ATACANDO

//        bool trailPlayed = false; //BOOL TEMP PARA TRAIL
//        bool comboQueued = false; //BOOL PARA HACER COMBO

//        yield return new WaitUntil(() => UtilitiesAgus.GetAnimatorStateProgress(heavyAttack.stateName, _anim).inState);

//        while (_anim.GetCurrentAnimatorStateInfo(0).IsName(heavyAttack.stateName))
//        {
//            var u = UtilitiesAgus.GetAnimatorStateProgress(heavyAttack.stateName, _anim);

//            ANTICIPATION
//            if (u.t01 < (heavyAttack.anticipationEnd / heavyAttack.maxFrames))
//            {
//                if (Input.GetMouseButton(0))
//                {
//                    currentDmg = Mathf.Min(fullyChargedDmg,
//                        currentDmg + Time.deltaTime * lerpSpeed * (fullyChargedDmg - heavyAttack.dmg));
//                    _anim.speed = heavyAttack.anticipationSpeed / 2f;
//                }
//                else _anim.speed = heavyAttack.anticipationSpeed;
//            }
//            SLASH
//            else if (u.t01 < (heavyAttack.slashEnd / heavyAttack.maxFrames))
//            {
//                _anim.speed = heavyAttack.slashSpeed;
//                _sword.enabled = true;

//                if (!trailPlayed)
//                {
//                    TrailActivator();
//                    trailPlayed = true;
//                }
//            }
//            RECOVERY
//            else
//            {
//                _anim.speed = heavyAttack.recoverySpeed;
//                _sword.enabled = false;
//            }

//            yield return null;
//        }

//        _anim.speed = 1f;
//        _sword.enabled = false;
//        _controller.isAttacking = false;
//        _counterHeavy = 0; // ojo, este es el cooldown del heavy, no el light
//    }
//    public void StopAllAttacksOnHit()
//    {
//        StopAllCoroutines();
//        _anim.speed = 1f;
//        _controller.isAttacking = false;
//        _sword.enabled = false;
//        _counterLight = 0;
//        _counterHeavy = 0;

//        ServiceLocator.Instance.GetDependency<PlayerMVC>().SetResting(false);

//        Debug.Log("PARE TODAS LAS CORRUTINAS");
//    }
//    void NextAttackChecker()
//    {
//        if (currentAttkIndex == 1) _nextAttack = attack1;
//        else if (currentAttkIndex == 2) _nextAttack = attack2;
//        else if (currentAttkIndex == 3) _nextAttack = attack3;
//    }
//    #endregion

//    IEnumerator RotateToCamera(float duration)
//    {
//        Quaternion startRotation = transform.rotation;
//        Quaternion targetRotation = Quaternion.Euler(0f, Camera.main.transform.rotation.eulerAngles.y, 0f);
//        float elapsedTime = 0f;
//        while (elapsedTime < duration)
//        {
//            float t = elapsedTime / duration;
//            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
//            elapsedTime += Time.deltaTime;
//            yield return null;
//        }
//        transform.rotation = targetRotation;
//    }

//    #region VIEW
//    public void PlayWhooshSound() { SoundManager.Instance.PlayOneShotFromIndex(6); }
//    public void PlaySwordOnDirt() => SoundManager.Instance.PlayOneShotFromIndex(12);
//    /*public void TrailActivator()
//    {
//        Debug.Log("ACTIVO TRAIL");
//        _trail.SendEvent("OnPlay");
//    }*/
//    public void SlowAnimSpeed() { StartCoroutine(AnimSpeed()); }
//    IEnumerator AnimSpeed()
//    {
//        _anim.speed = .5f;
//        yield return new WaitForSeconds(.05f);
//        _anim.speed = 1f;
//    }
//    #endregion
//}

//GEMI ATTK MANAGER
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.VFX;

//[System.Serializable]
//public struct AttacksData
//{
//    public string stateName;
//    public float anticipationEnd, slashEnd, maxFrames;
//    public float anticipationSpeed, slashSpeed, recoverySpeed;
//    public float dmg;
//}
//public class NewAttackManager : MonoBehaviour
//{
//    #region CDs
//    [SerializeField] float maxLightCD = 0.35f;
//    [SerializeField] float maxHeavyCD = 0.8f;
//    float _counterLight, _counterHeavy;
//    bool canHeavyAttack, canLightAttack;
//    #endregion

//    Animator _anim;
//    ControllerPlayer _controller;
//    PlayerLife playerLife;
//    [Header("<color=purple>Dependencies</color>")]
//    [SerializeField] VisualEffect _trail;
//    [SerializeField] Collider _sword;

//    [Header("<color=orange>Variables</color>")]
//    [HideInInspector] public float currentDmg;
//    [SerializeField] float comboBufferTime = 0.2f; // 200 ms extra
//    float comboBufferTimer = 0f;
//    [SerializeField] float comboCoyoteTime = 0.15f; // 150ms extra
//    float comboCoyoteTimer = 0f;
//    [SerializeField] float lerpSpeed = 5f;

//    [Header("<color=yellow>AttacksData (FILL ATTACKS DATA)</color>")]
//    public AttacksData attack1;
//    public AttacksData attack2;
//    public AttacksData attack3;
//    public AttacksData heavyAttack;
//    AttacksData _nextAttack;
//    int currentAttkIndex = 1;

//    [Header("<color=green>Heavy Hold</color>")]
//    public float longPressTime = 0.25f;
//    private bool clicking = false;
//    private float pressedTime = 0f;
//    private bool longPressInvoked = false;
//    public float fullyChargedDmg = 25f;

//    #region MONOBEHAVIOUR METHODS
//    private void Awake()
//    {
//        _anim = GetComponent<Animator>();
//        _sword = GetComponentInChildren<SwordCollider>().GetComponent<Collider>();
//        ServiceLocator.Instance.RegisterDependency<NewAttackManager>(this);
//    }
//    private void Start()
//    {
//        _controller = ServiceLocator.Instance.GetDependency<ControllerPlayer>();

//        playerLife = ServiceLocator.Instance.GetDependency<PlayerLife>();
//        playerLife.OnPlayerHit += StopAllAttacksOnHit;

//        _sword.enabled = false;

//        _controller.OnMouseDown += LeftClickDown;
//        _controller.OnMouseUp += LeftClickUp;
//        _controller.OnMouse += LeftClick;
//        _controller.isAttacking = false;
//    }
//    private void Update()
//    {
//        if (_counterLight != maxLightCD && !_controller.isAttacking) //SUMO CD SOLO SI NO ESTOY ATACANDO
//        {
//            _counterLight = Mathf.Clamp(_counterLight + Time.deltaTime, 0, maxLightCD);
//        }
//        if (_counterHeavy != maxHeavyCD && !_controller.isAttacking) //ACA LO MISMO, SUMO SOLO SI NO ESTOY ATACANDO
//        {
//            _counterHeavy = Mathf.Clamp(_counterHeavy + Time.deltaTime, 0, maxHeavyCD);
//        }
//        canLightAttack = _counterLight == maxLightCD;
//        canHeavyAttack = _counterHeavy == maxHeavyCD;

//        // La lógica de Input.GetMouseButtonDown(0) se ha movido a LeftClickDown para mejor control.
//        if (comboBufferTimer > 0f)
//            comboBufferTimer -= Time.deltaTime;

//        if (comboCoyoteTimer > 0f)
//            comboCoyoteTimer -= Time.deltaTime;
//    }
//    private void OnDestroy()
//    {
//        _controller.OnMouseDown -= LeftClickDown;
//        _controller.OnMouseUp -= LeftClickUp;
//        _controller.OnMouse -= LeftClick;

//        playerLife.OnPlayerHit -= StopAllAttacksOnHit;
//    }
//    #endregion

//    //--------------------------------------------------------------------------------------------------

//    #region CONTROLLER ACTION METHODS
//    void LeftClickDown() //PRENDO CLICKING - PRESSEDTIME SE REINICIA - LONGPRESSED SE HACE FALSO
//    {
//        // Lógica de Heavy y reseteo de flags
//        clicking = true;
//        //Debug.Log("CLICKING TRUE");
//        pressedTime = 0f;
//        longPressInvoked = false;

//        // 🚩 ARREGLO 1: Si ya estamos atacando, este clic debe ser guardado como combo.
//        if (_controller.isAttacking)
//        {
//            comboBufferTimer = comboBufferTime;
//        }
//    }
//    void LeftClickUp()
//    {
//        // Si ya se invocó el heavy, ignoramos el up
//        if (longPressInvoked) return;

//        // Si ya estamos atacando, ignoramos el UpClick (el Down ya guardó el combo)
//        if (_controller.isAttacking)
//        {
//            clicking = false;
//            return;
//        }

//        // Condición para INICIAR el Primer Ataque Ligero
//        if (clicking && canLightAttack)
//        {
//            // Solo si no estamos cargando Heavy
//            if (!longPressInvoked)
//            {
//                currentAttkIndex = 1;
//                StartCoroutine(DoLightAttack(attack1)); // HAGO LIGHTATTACK 1 SIEMPRE
//            }
//            clicking = false;
//        }
//    }
//    void LeftClick()
//    {
//        if (clicking && canHeavyAttack && !_controller.isAttacking)
//        {
//            pressedTime += Time.deltaTime;
//            if (!longPressInvoked && pressedTime >= longPressTime)
//            {
//                longPressInvoked = true;
//                StartCoroutine(DoHeavyAttack());
//                clicking = false;
//            }
//        }
//    }
//    #endregion

//    //--------------------------------------------------------------------------------------------------

//    #region ATTACK METHODS
//    //IEnumerator DoLightAttack(AttacksData attack) //ENTRO A ATTACK1 DESDE LEFTCLICKUP
//    //{
//    //    StartCoroutine(RotateToCamera(.2f));

//    //    currentDmg = attack.dmg;
//    //    _anim.SetTrigger("hit" + currentAttkIndex);
//    //    _controller.isAttacking = true; // ESTABLECIDO A TRUE AL INICIO

//    //    bool trailPlayed = false;
//    //    bool comboQueued = false; // FLAG CRÍTICO para la limpieza

//    //    try // 🚩 ARREGLO 2: INICIO DEL BLOQUE CRÍTICO PARA DETECCIÓN DE BREAK
//    //    {
//    //        yield return new WaitUntil(() => UtilitiesAgus.GetAnimatorStateProgress(attack.stateName, _anim).inState);
//    //        while (true) //CUANDO ESTOY EN EL ESTADO CORRECTO ENTRO AL WHILE
//    //        {
//    //            var u = UtilitiesAgus.GetAnimatorStateProgress(attack.stateName, _anim);

//    //            // ANTICIPATION
//    //            if (u.t01 < (attack.anticipationEnd / attack.maxFrames))
//    //            {
//    //                _anim.speed = attack.anticipationSpeed;
//    //                _sword.enabled = false;
//    //            }
//    //            // SLASH
//    //            else if (u.t01 < (attack.slashEnd / attack.maxFrames))
//    //            {
//    //                _anim.speed = attack.slashSpeed;
//    //                _sword.enabled = true;
//    //                if (!trailPlayed)
//    //                {
//    //                    TrailActivator();
//    //                    trailPlayed = true;
//    //                }
//    //            }
//    //            else if (!u.finished) // Recovery
//    //            {
//    //                _anim.speed = attack.recoverySpeed;
//    //                _sword.enabled = false;

//    //                if (!comboQueued && (comboBufferTimer > 0f || comboCoyoteTimer > 0f))
//    //                {
//    //                    comboBufferTimer = 0f;
//    //                    comboCoyoteTimer = 0f;
//    //                    comboQueued = true; // El combo SE HA ENCOLADO

//    //                    currentAttkIndex++;
//    //                    if (currentAttkIndex > 3) currentAttkIndex = 1;

//    //                    NextAttackChecker();
//    //                    StartCoroutine(DoLightAttack(_nextAttack));
//    //                    break; // Salimos de este 'while (true)'
//    //                }
//    //            }
//    //            else // La animación terminó SIN COMBO
//    //            {
//    //                comboCoyoteTimer = comboCoyoteTime;
//    //                break;
//    //            }

//    //            yield return null;
//    //        }

//    //        // LIMPIEZA Condicional: Solo si NO se hizo combo, apagamos isAttacking aquí.
//    //        if (!comboQueued)
//    //        {
//    //            _controller.isAttacking = false;
//    //        }
//    //    }
//    //    finally // 🚩 ARREGLO 3: ESTE BLOQUE SIEMPRE SE EJECUTA, garantizando limpieza visual/de CD
//    //    {
//    //        _anim.speed = 1f; // SETTEO NORMAL SPEED
//    //        _sword.enabled = false; // APAGO COL
//    //        _counterLight = 0; // COUNTER EN 0 PARA CORRER CD
//    //        Debug.Log($"FIN DE ATAQUE {currentAttkIndex}. isAttacking: {_controller.isAttacking}. Combo Queued: {comboQueued}");
//    //    }
//    //}

//    //--------------------------------------------------------------------------------------------------
//    IEnumerator DoLightAttack(AttacksData attack) //ENTRO A ATTACK1 DESDE LEFTCLICKUP
//    {
//        StartCoroutine(RotateToCamera(.2f));

//        currentDmg = attack.dmg;
//        _anim.SetTrigger("hit" + currentAttkIndex);
//        _controller.isAttacking = true;

//        bool trailPlayed = false;
//        bool comboQueued = false;
//        bool isSwordActive = false; // 🚩 NUEVO: Flag para asegurar el estado del collider

//        try
//        {
//            yield return new WaitUntil(() => UtilitiesAgus.GetAnimatorStateProgress(attack.stateName, _anim).inState);
//            while (true)
//            {
//                var u = UtilitiesAgus.GetAnimatorStateProgress(attack.stateName, _anim);

//                // ANTICIPATION
//                if (u.t01 < (attack.anticipationEnd / attack.maxFrames))
//                {
//                    _anim.speed = attack.anticipationSpeed;

//                    // 🚩 FIX: Si está activo al entrar en Anticipation, lo apagamos por seguridad.
//                    if (isSwordActive)
//                    {
//                        _sword.enabled = false;
//                        isSwordActive = false;
//                    }
//                }
//                // SLASH (PUNTO DE CONEXIÓN DE COMBO)
//                else if (u.t01 < (attack.slashEnd / attack.maxFrames))
//                {
//                    _anim.speed = attack.slashSpeed;

//                    // 🚩 FIX: Activación Garantizada: Solo se activa si NO estaba activo.
//                    if (!isSwordActive)
//                    {
//                        _sword.enabled = true; // PRENDO COL
//                        isSwordActive = true;
//                        Debug.LogWarning($"SWORD ACTIVATED: {currentAttkIndex}");
//                    }

//                    if (!trailPlayed)
//                    {
//                        TrailActivator();
//                        trailPlayed = true;
//                    }

//                    // 🚩 CHEQUEO DE COMBO ADELANTADO (Mantenemos la lógica de combo)
//                    if (!comboQueued && (comboBufferTimer > 0f || comboCoyoteTimer > 0f))
//                    {
//                        comboBufferTimer = 0f;
//                        comboCoyoteTimer = 0f;
//                        comboQueued = true;

//                        currentAttkIndex++;
//                        if (currentAttkIndex > 3) currentAttkIndex = 1;

//                        NextAttackChecker();
//                        StartCoroutine(DoLightAttack(_nextAttack));
//                        break; // Salimos de la corrutina actual para iniciar la siguiente
//                    }
//                }
//                // RECOVERY
//                else if (!u.finished)
//                {
//                    _anim.speed = attack.recoverySpeed;

//                    // 🚩 FIX: Desactivación Garantizada: Solo se desactiva si ESTABA activo.
//                    if (isSwordActive)
//                    {
//                        _sword.enabled = false;
//                        isSwordActive = false;
//                        Debug.LogWarning($"SWORD DEACTIVATED: {currentAttkIndex}");
//                    }
//                }
//                else // La animación terminó SIN COMBO
//                {
//                    comboCoyoteTimer = comboCoyoteTime;
//                    break;
//                }

//                yield return null;
//            }

//            // Limpieza Condicional: Solo si NO se hizo combo
//            if (!comboQueued)
//            {
//                _controller.isAttacking = false;
//            }
//        }
//        finally // ESTE BLOQUE SIEMPRE SE EJECUTA
//        {
//            _anim.speed = 1f;
//            _sword.enabled = false; // DOBLE CHECK DE APAGADO
//            _counterLight = 0;
//            Debug.Log($"FIN DE ATAQUE {currentAttkIndex}. isAttacking: {_controller.isAttacking}. Combo Queued: {comboQueued}");
//        }
//    }

//    IEnumerator DoHeavyAttack() //ENTRA A HEAVY ATTACK 
//    {
//        StartCoroutine(RotateToCamera(.2f));

//        currentDmg = heavyAttack.dmg;
//        _anim.SetTrigger("heavyHit");
//        _controller.isAttacking = true;

//        bool trailPlayed = false;

//        try // 🚩 AÑADIDO: Bloque try para Heavy Attack también
//        {
//            yield return new WaitUntil(() => UtilitiesAgus.GetAnimatorStateProgress(heavyAttack.stateName, _anim).inState);

//            while (_anim.GetCurrentAnimatorStateInfo(0).IsName(heavyAttack.stateName))
//            {
//                var u = UtilitiesAgus.GetAnimatorStateProgress(heavyAttack.stateName, _anim);

//                // ANTICIPATION (Mantiene la lógica de carga de daño)
//                if (u.t01 < (heavyAttack.anticipationEnd / heavyAttack.maxFrames))
//                {
//                    if (Input.GetMouseButton(0))
//                    {
//                        currentDmg = Mathf.Min(fullyChargedDmg,
//                            currentDmg + Time.deltaTime * lerpSpeed * (fullyChargedDmg - heavyAttack.dmg));
//                        _anim.speed = heavyAttack.anticipationSpeed / 2f;
//                    }
//                    else _anim.speed = heavyAttack.anticipationSpeed;
//                }
//                // SLASH
//                else if (u.t01 < (heavyAttack.slashEnd / heavyAttack.maxFrames))
//                {
//                    _anim.speed = heavyAttack.slashSpeed;
//                    _sword.enabled = true;

//                    if (!trailPlayed)
//                    {
//                        TrailActivator();
//                        trailPlayed = true;
//                    }
//                }
//                // RECOVERY
//                else
//                {
//                    _anim.speed = heavyAttack.recoverySpeed;
//                    _sword.enabled = false;
//                }

//                yield return null;
//            }
//        }
//        finally // 🚩 GARANTIZAR LIMPIEZA DE HEAVY
//        {
//            _anim.speed = 1f;
//            _sword.enabled = false;
//            _controller.isAttacking = false; // APAGADO DE FLAG
//            _counterHeavy = 0;
//        }
//    }

//    public void StopAllAttacksOnHit()
//    {
//        StopAllCoroutines();
//        _anim.speed = 1f;
//        _controller.isAttacking = false;
//        _sword.enabled = false;
//        _counterLight = 0;
//        _counterHeavy = 0;
//        Debug.Log("PARE TODAS LAS CORRUTINAS");
//    }

//    void NextAttackChecker()
//    {
//        if (currentAttkIndex == 1) _nextAttack = attack1;
//        else if (currentAttkIndex == 2) _nextAttack = attack2;
//        else if (currentAttkIndex == 3) _nextAttack = attack3;
//    }
//    #endregion

//    //--------------------------------------------------------------------------------------------------

//    IEnumerator RotateToCamera(float duration)
//    {
//        Quaternion startRotation = transform.rotation;
//        Quaternion targetRotation = Quaternion.Euler(0f, Camera.main.transform.rotation.eulerAngles.y, 0f);
//        float elapsedTime = 0f;
//        while (elapsedTime < duration)
//        {
//            float t = elapsedTime / duration;
//            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
//            elapsedTime += Time.deltaTime;
//            yield return null;
//        }
//        transform.rotation = targetRotation;
//    }

//    #region VIEW
//    public void PlayWhooshSound() { SoundManager.Instance.PlayOneShotFromIndex(6); }
//    public void PlaySwordOnDirt() => SoundManager.Instance.PlayOneShotFromIndex(12);
//    public void TrailActivator()
//    {
//        //Debug.Log("ACTIVO TRAIL");
//        _trail.SendEvent("OnPlay");
//    }
//    public void SlowAnimSpeed() { StartCoroutine(AnimSpeed()); }
//    IEnumerator AnimSpeed()
//    {
//        _anim.speed = .5f;
//        yield return new WaitForSeconds(.05f);
//        _anim.speed = 1f;
//    }
//    #endregion
//}