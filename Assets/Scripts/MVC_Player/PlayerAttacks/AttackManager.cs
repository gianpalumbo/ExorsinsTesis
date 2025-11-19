//REPLACED FOR NEW ATTACK MANAGER

//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;
//using UnityEngine.VFX;

//public class AttackManager : MonoBehaviour
//{
//    #region Values

//    [Header("Speeds (Light)")]
//    [SerializeField] float anticipationSpeed = 0.7f;
//    [SerializeField] float slashSpeed = 1.25f;
//    [SerializeField] float recoverySpeed = 0.85f;

//    [Header("Speeds (Heavy)")]
//    [SerializeField] float anticipationSpeedHvy = 0.6f;
//    [SerializeField] float slashSpeedHvy = 1.1f;
//    [SerializeField] float recoverySpeedHvy = 0.8f;

//    [Header("Damage")]
//    public float lightDmg = 10f;
//    public float heavyDmg = 16f;
//    public float fullyChargedDmg = 25f;
//    [SerializeField] float lerpSpeed = 5f;
//    public float currentDmg;

//    [Header("Refs")]
//    [SerializeField] private Animator anim;
//    [SerializeField] private PlayerMVC playerMVC;
//    [SerializeField] private GameObject karmicCanvas;
//    [SerializeField] VisualEffect _trail;
//    public Collider sword;

//    [Header("Cooldowns")]
//    [SerializeField] float maxLightCD = 0.35f;
//    [SerializeField] float maxHeavyCD = 0.8f;
//    float _counterLight, _counterHeavy;
//    bool canHeavyAttack, canLightAttack;

//    [Header("State")]
//    [SerializeField] bool colEnabled;
//    public bool ColEnabled => colEnabled;

//    // ====== INPUT BUFFER ======
//    [Header("Combo/Input")]
//    [SerializeField] float inputBufferWindow = 0.25f; // tiempo para guardar un click
//    bool clickBuffered;
//    float clickBufferedAt;

//    [Header("Heavy Hold")]
//    public float longPressTime = 0.25f;
//    private bool clicking = false;
//    private float pressedTime = 0f;
//    private bool longPressInvoked = false;

//    // ====== FRAME DATA (0..1) ======
//    [Header("Frame Data - Attack 1")]
//    [Range(0, 1f)] public float A1_AntEnd = 0.54f;
//    [Range(0, 1f)] public float A1_SlashEnd = 0.78f;
//    [Range(0, 1f)] public float A1_Accept = 0.50f;  // aceptar input temprano
//    [Range(0, 1f)] public float A1_Commit = 0.56f;  // momento seguro para saltar a A2
//    [Range(0, 0.2f)] public float A1_HitLead = 0.02f; // collider apenas antes del fin de anticipation

//    [Header("Frame Data - Attack 2")]
//    [Range(0, 1f)] public float A2_AntEnd = 0.33f;
//    [Range(0, 1f)] public float A2_SlashEnd = 0.52f;
//    [Range(0, 1f)] public float A2_Accept = 0.45f;
//    [Range(0, 1f)] public float A2_Commit = 0.52f;  // tras impacto de A2
//    [Range(0, 0.2f)] public float A2_HitLead = 0.02f;

//    [Header("Frame Data - Attack 3")]
//    [Range(0, 1f)] public float A3_AntEnd = 0.24f;
//    [Range(0, 1f)] public float A3_SlashEnd = 0.43f;
//    [Range(0, 1f)] public float A3_Accept = 0.38f;
//    [Range(0, 1f)] public float A3_Commit = 0.45f;  // tras impacto de A3
//    [Range(0, 0.2f)] public float A3_HitLead = 0.02f;

//    // Heavy tune (simple)
//    [Header("Frame Data - Heavy (simple)")]
//    [Range(0, 1f)] public float H_AntEnd = 0.48f;
//    [Range(0, 1f)] public float H_ColliderEnd = 0.55f; // tramo corto donde ya puede pegar
//    [Range(0, 1f)] public float H_SlashEnd = 0.80f;

//    // Corrutina activa
//    Coroutine _attackCo;

//    #endregion

//    private void Awake()
//    {
//        if (anim == null) anim = GetComponent<Animator>();
//        _counterLight = maxLightCD;
//        _counterHeavy = maxHeavyCD;
//    }

//    private void Start()
//    {
//        colEnabled = false;
//        sword.enabled = false;

//        // Suscripción a eventos del controller
//        ReferenceManager.Instance.controller.OnMouseDown += LeftClickDown;
//        ReferenceManager.Instance.controller.OnMouseUp += LeftClickUp;
//        ReferenceManager.Instance.controller.OnMouse += LeftClick;

//        ReferenceManager.Instance.controller.isAttacking = false;
//    }

//    private void Update()
//    {
//        // ===== Cooldowns =====
//        if (_counterLight != maxLightCD)
//        {
//            _counterLight = Mathf.Clamp(_counterLight + Time.deltaTime, 0, maxLightCD);
//        }
//        if (_counterHeavy != maxHeavyCD)
//        {
//            _counterHeavy = Mathf.Clamp(_counterHeavy + Time.deltaTime, 0, maxHeavyCD);
//        }
//        canLightAttack = _counterLight == maxLightCD;
//        canHeavyAttack = _counterHeavy == maxHeavyCD;

//        if (!ReferenceManager.Instance.controller.isAttacking && colEnabled)
//            colEnabled = false;

//        sword.enabled = colEnabled;
//        IdleEndOfAtk();
//    }

//    #region Input Buffer Helpers
//    void BufferClick() { clickBuffered = true; clickBufferedAt = Time.time; }
//    bool HasBufferedClick() => clickBuffered && (Time.time - clickBufferedAt) <= inputBufferWindow;
//    bool ConsumeBufferedClick()
//    {
//        if (HasBufferedClick()) { clickBuffered = false; return true; }
//        return false;
//    }
//    #endregion

//    #region Event Methods for ControllerPlayer
//    void LeftClickDown()
//    {
//        clicking = true;
//        pressedTime = 0f;
//        longPressInvoked = false;

//        BufferClick();
//    }

//    void LeftClickUp()
//    {
//        if (clicking && canLightAttack && !ReferenceManager.Instance.controller.isAttacking)
//        {
//            if (!longPressInvoked)
//            {
//                StartAttackCoroutine(Attack1());
//            }
//            clicking = false;
//        }
//    }

//    void LeftClick()
//    {
//        if (clicking && canHeavyAttack && !ReferenceManager.Instance.controller.isAttacking)
//        {
//            pressedTime += Time.deltaTime;
//            if (!longPressInvoked && pressedTime >= longPressTime)
//            {
//                longPressInvoked = true;
//                StartAttackCoroutine(HeavyAttack());
//            }
//        }
//    }
//    #endregion

//    #region Sounds
//    public void PlayWhooshSound() { SoundManager.Instance.PlayOneShotFromIndex(6); }
//    public void PlaySwordOnDirt() => SoundManager.Instance.PlayOneShotFromIndex(12);
//    #endregion

//    #region Attack Flow Helpers
//    void StartAttackCoroutine(IEnumerator routine)
//    {
//        if (_attackCo != null) StopCoroutine(_attackCo);
//        _attackCo = StartCoroutine(routine);
//    }
//    #endregion

//    // ========================= HEAVY ATTACK =========================
//    IEnumerator HeavyAttack()
//    {
//        colEnabled = false;
//        bool canPlaySound = true;
//        ReferenceManager.Instance.controller.isAttacking = true;
//        currentDmg = heavyDmg;
//        anim.SetTrigger("heavyHit");
//        _counterHeavy = 0;

//        yield return new WaitUntil(() => anim.GetCurrentAnimatorStateInfo(0).IsName("HeavyAttack"));

//        while (true)
//        {
//            var s = anim.GetCurrentAnimatorStateInfo(0);
//            if (!s.IsName("HeavyAttack")) break;

//            float t = s.normalizedTime % 1f;

//            if (t < H_AntEnd) // ANTICIPATION (permite cargar)
//            {
//                if (Input.GetMouseButton(0))
//                {
//                    currentDmg = Mathf.Min(fullyChargedDmg, currentDmg + Time.deltaTime * lerpSpeed * (fullyChargedDmg - heavyDmg));
//                    anim.speed = anticipationSpeedHvy / 2f;
//                }
//                else anim.speed = anticipationSpeedHvy;
//            }
//            else if (t < H_ColliderEnd) // COLLIDER temprano
//            {
//                colEnabled = true;
//            }
//            else if (t < H_SlashEnd) // SLASH
//            {
//                if (canPlaySound) { canPlaySound = false; PlayWhooshSound(); }
//                TrailActivator();
//                anim.speed = slashSpeedHvy;

//                // Commit combo por buffer
//                if (ConsumeBufferedClick())
//                {
//                    colEnabled = false; // opcional: evitar doble hit
//                    StartAttackCoroutine(Attack1());
//                    yield break;
//                }
//            }
//            else if (t < 0.95f) // RECOVERY extendida
//            {
//                colEnabled = false;
//                anim.speed = recoverySpeedHvy;

//                if (ConsumeBufferedClick())
//                {
//                    StartAttackCoroutine(Attack1());
//                    yield break;
//                }
//            }

//            yield return null;
//        }

//        anim.speed = 1f;
//        ReferenceManager.Instance.controller.isAttacking = false;
//    }

//    // ========================= ATTACK 1 =========================
//    IEnumerator Attack1()
//    {
//        colEnabled = false;   // seguridad
//        bool canPlaySound = true, comboQueued = false;
//        ReferenceManager.Instance.controller.isAttacking = true;
//        currentDmg = lightDmg;
//        anim.SetTrigger("hit1");
//        _counterLight = 0;

//        yield return new WaitUntil(() => anim.GetCurrentAnimatorStateInfo(0).IsName("hit1"));

//        while (true)
//        {
//            var s = anim.GetCurrentAnimatorStateInfo(0);
//            if (!s.IsName("hit1")) break;

//            float t = s.normalizedTime % 1f;

//            // Acepto el input un poco antes y lo dejo en cola
//            if (!comboQueued && t >= A1_Accept && HasBufferedClick()) comboQueued = true;

//            if (t < A1_AntEnd) // ANTICIPATION
//            {
//                anim.speed = anticipationSpeed;
//                if (t >= A1_AntEnd - A1_HitLead) colEnabled = true; // collider un pelito antes
//            }
//            else if (t < A1_SlashEnd) // SLASH
//            {
//                if (canPlaySound) { canPlaySound = false; PlayWhooshSound(); TrailActivator(); }
//                colEnabled = true;
//                anim.speed = slashSpeed;

//                if (t >= A1_Commit && (comboQueued || ConsumeBufferedClick()))
//                {
//                    colEnabled = false;
//                    StartAttackCoroutine(Attack2());
//                    yield break;
//                }
//            }
//            else // RECOVERY con coyote-time
//            {
//                colEnabled = false;
//                anim.speed = recoverySpeed;

//                if (t < 0.95f && (comboQueued || ConsumeBufferedClick()))
//                {
//                    colEnabled = false;
//                    StartAttackCoroutine(Attack2());
//                    yield break;
//                }
//            }

//            yield return null;
//        }

//        anim.speed = 1f;
//        ReferenceManager.Instance.controller.isAttacking = false;
//    }

//    // ========================= ATTACK 2 =========================
//    IEnumerator Attack2()
//    {
//        colEnabled = false;   // seguridad
//        bool canPlaySound = true, comboQueued = false;
//        ReferenceManager.Instance.controller.isAttacking = true;
//        currentDmg = lightDmg;
//        anim.SetTrigger("hit2");

//        yield return new WaitUntil(() => anim.GetCurrentAnimatorStateInfo(0).IsName("hit2"));

//        while (true)
//        {
//            var s = anim.GetCurrentAnimatorStateInfo(0);
//            if (!s.IsName("hit2")) break;

//            float t = s.normalizedTime % 1f;

//            if (!comboQueued && t >= A2_Accept && HasBufferedClick()) comboQueued = true;

//            if (t < A2_AntEnd) // ANTICIPATION
//            {
//                anim.speed = anticipationSpeed;
//                if (t >= A2_AntEnd - A2_HitLead) colEnabled = true;
//            }
//            else if (t < A2_SlashEnd) // SLASH
//            {
//                if (canPlaySound) { canPlaySound = false; PlayWhooshSound(); TrailActivator(); }
//                colEnabled = true;
//                anim.speed = slashSpeed;
//            }
//            else // RECOVERY -> commit a Attack3
//            {
//                colEnabled = false;
//                anim.speed = recoverySpeed;

//                if (t >= A2_Commit && (comboQueued || ConsumeBufferedClick()))
//                {
//                    colEnabled = false;
//                    StartAttackCoroutine(Attack3());
//                    yield break;
//                }
//            }

//            yield return null;
//        }

//        anim.speed = 1f;
//        ReferenceManager.Instance.controller.isAttacking = false;
//    }

//    // ========================= ATTACK 3 =========================
//    IEnumerator Attack3()
//    {
//        colEnabled = false;   // seguridad
//        bool canPlaySound = true, comboQueued = false; // comboQueued te permite loop opcional a Attack1
//        ReferenceManager.Instance.controller.isAttacking = true;
//        currentDmg = lightDmg; // si querés, poné un bonus de finisher
//        anim.SetTrigger("hit3");

//        yield return new WaitUntil(() => anim.GetCurrentAnimatorStateInfo(0).IsName("hit3"));

//        while (true)
//        {
//            var s = anim.GetCurrentAnimatorStateInfo(0);
//            if (!s.IsName("hit3")) break;

//            float t = s.normalizedTime % 1f;

//            if (!comboQueued && t >= A3_Accept && HasBufferedClick()) comboQueued = true;

//            if (t < A3_AntEnd) // ANTICIPATION
//            {
//                anim.speed = anticipationSpeed;
//                if (t >= A3_AntEnd - A3_HitLead) colEnabled = true;
//            }
//            else if (t < A3_SlashEnd) // SLASH
//            {
//                if (canPlaySound) { canPlaySound = false; PlayWhooshSound(); TrailActivator(); }
//                //colEnabled = true;
//                anim.speed = slashSpeed;
//            }
//            else // RECOVERY (opcional loop a Attack1)
//            {
//                colEnabled = false;
//                anim.speed = recoverySpeed;

//                //// Si querés que el combo pueda seguir cíclico:
//                //if (t >= A3_Commit && (comboQueued || ConsumeBufferedClick()))
//                //{
//                //    StartAttackCoroutine(Attack1()); // desactivá esta línea si NO querés loop
//                //    yield break;
//                //}
//            }

//            yield return null;
//        }

//        anim.ResetTrigger("hit1");
//        anim.speed = 1f;
//        ReferenceManager.Instance.controller.isAttacking = false;
//    }

//    // ===== util =====
//    public void IdleEndOfAtk()
//    {
//        if (Input.GetAxis("Horizontal") == 0) anim.SetFloat("movX", 0);
//        if (Input.GetAxis("Vertical") == 0) anim.SetFloat("movY", 0);
//    }

//    public void TrailActivator() => _trail.SendEvent("OnPlay");

//    private void OnDestroy()
//    {
//        if (ReferenceManager.Instance != null && ReferenceManager.Instance.controller != null)
//        {
//            ReferenceManager.Instance.controller.OnMouseDown -= LeftClickDown;
//            ReferenceManager.Instance.controller.OnMouseUp -= LeftClickUp;
//            ReferenceManager.Instance.controller.OnMouse -= LeftClick;
//        }
//    }

//    public void SlowAnimSpeed() { StartCoroutine(AnimSpeed()); }
//    IEnumerator AnimSpeed()
//    {
//        anim.speed = .5f;
//        yield return new WaitForSeconds(.05f);
//        anim.speed = 1f;
//    }
//}
