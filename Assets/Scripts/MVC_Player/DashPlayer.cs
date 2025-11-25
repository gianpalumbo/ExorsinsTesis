using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class DashPlayer : MonoBehaviour
{
    Rigidbody _rb;
    [SerializeField] Animator _anim;
    [SerializeField] VisualEffect _rollVfx;
    [SerializeField] float rollStrenght, dashStamina;
    Vector3 _dir;
    bool canDash = true, isDashing, clampToFloor, canApplyForce = false;
    [SerializeField] float timeDashing, timeAfterDash;
    PlayerMVC player;
    PlayerLife playerLife;

    private void Awake()
    {
        ServiceLocator.Instance.RegisterDependency<DashPlayer>(this);
        _rb = GetComponent<Rigidbody>();
        _anim = GetComponent<Animator>();
    }
    void Start()
    {
        player = ServiceLocator.Instance.GetDependency<PlayerMVC>();
        //ReferenceManager.Instance.controller.OnSpacebarDown += EventMethodForDash;
        if(ServiceLocator.Instance.TryGetDependency<ControllerPlayer>(out ControllerPlayer controller))
            controller.OnSpacebarDown += EventMethodForDash;
        playerLife = ServiceLocator.Instance.GetDependency<PlayerLife>();

        playerLife.OnPlayerHit += ResetRollProperties;
    }

    void Update()
    {
        canDash = player.Model.StaminaBarCheck(dashStamina) && 
                  !isDashing; //SI TENGO STAMINA Y SI TERMINO

        //if (clampToFloor)
        //ClampPlayerToTheFloor();
        //if (Input.GetKeyDown(KeyCode.Alpha9))
        //{
            
        //}
    }

    public void FixedUpdate()
    {
        if (canApplyForce) AddForceToRoll();
    }

    private void OnEnable()
    {
        _anim = GetComponent<Animator>();
    }

    public void EventMethodForDash()
    {
        //if (canDash)
        //{
        //    StartCoroutine(Dash());
        //    foreach (var d in _dashVfxs) d.SendEvent("OnPlay");
        //}

        if (canDash)
        {
            StartCoroutine(RollForward());
            _rollVfx.SendEvent("OnPlay");
        }
    }

    public void ResetRollVariables()
    {
        _anim.Play("Running BT");
        _anim.SetFloat("velocity", 0);
        _anim.updateMode = AnimatorUpdateMode.AnimatePhysics;
        _anim.applyRootMotion = true;
        _rb.excludeLayers = 0;
        playerLife.isInvulnerable = false;
        isDashing = false;
        ServiceLocator.Instance.GetDependency<PlayerMVC>().isResting = false;
        _rb.velocity = Vector3.zero;
    }

    public IEnumerator RollForward() //CUANDO HAGO ROLL ACTIVO IFRAMES HASTA EL 70%
    {
        //PARA EVITAR QUE SE PISEN Y SE ROMPA
        ServiceLocator.Instance.GetDependency<AttackEFSM>().Think();

        isDashing = true;
        _anim.applyRootMotion = false; // Deshabilitamos RootMotion para el empuje manual
        _anim.SetTrigger("RollFwd");
        playerLife.isInvulnerable = true;
        _anim.updateMode = AnimatorUpdateMode.Normal;

        //EXCLUDE ENEMIES LAYERS
        _rb.excludeLayers = 1 << 9;

        ServiceLocator.Instance.GetDependency<PlayerMVC>().isResting = true;

        if (UtilitiesAgus.GetAnimatorStateProgress("RollFwd", _anim).t01 >= (41f / 124f))
            canApplyForce = true;

        else if (UtilitiesAgus.GetAnimatorStateProgress("RollFwd", _anim).t01 >= (45f / 124f))
        {
            canApplyForce = false;
            _rb.velocity = Vector3.zero;
        }

        // --- Invulnerabilidad Activa (hasta el 70%) ---
        // Continuamos esperando hasta el 70% de la animación (la fuerza ya terminó)
        yield return new WaitUntil(() => (UtilitiesAgus.GetAnimatorStateProgress("RollFwd", _anim).t01 >= .7f));

        playerLife.isInvulnerable = false;

        // --- Final de la Animación ---
        yield return new WaitUntil(() => (UtilitiesAgus.GetAnimatorStateProgress("RollFwd", _anim).finished));

        //VACIO LA LAYER MASK DEL RIGID
        _rb.excludeLayers = 0;

        isDashing = false;
        _anim.updateMode = AnimatorUpdateMode.AnimatePhysics;
        ServiceLocator.Instance.GetDependency<PlayerMVC>().isResting = false;
        _anim.applyRootMotion = true; // Restauramos RootMotion

        if (playerLife.Life <= 0)
        {
            playerLife.TakeDamageWithoutFlinching(0f);
        }
    }

    public void ResetRollProperties() //SI ME PEGAN RESETTEO PROPIEDADES DE ROLL
    {
        StopAllCoroutines();
        Debug.Log("resetteo ROLL");
        isDashing = false;
        playerLife.isInvulnerable = false;
        canDash = true;
        canApplyForce = false;
        _anim.applyRootMotion = true;
        ServiceLocator.Instance.GetDependency<PlayerMVC>().isResting = false;
        _anim.updateMode = AnimatorUpdateMode.AnimatePhysics;
    }

    //public IEnumerator Dash()
    //{
    //    //myMat.SetColor("_BaseColor", new Color(0,0,0,0));
    //    clampToFloor = true;
    //    player.Model.DashUsesStamina(dashStamina);
    //    _anim.speed = 0;
    //    isDashing = true;
    //    canDash = false;
    //    yield return new WaitForSeconds(timeDashing);
    //    //dashParticles.Stop();
    //    _anim.speed = 1;
    //    isDashing = false;
    //    yield return new WaitForSeconds(2f);
    //    canDash = true;
    //    yield return new WaitForSeconds(timeAfterDash);
    //    clampToFloor = false;
    //    _rb.velocity = Vector3.zero;
    //}
    void AddForceToRoll()
    {
        _rb.AddForce(transform.forward * rollStrenght, ForceMode.Impulse);
    }
    //void ClampPlayerToTheFloor()
    //{
    //    //IGUALO LA Y DEL TRANSFORM A LA Y DEL RAYCAST
    //    Ray ray = new Ray(transform.position + Vector3.up, Vector3.down);
    //    RaycastHit hit;
    //    if (Physics.Raycast(ray, out hit, 5f))
    //    {
    //        float lerpSpeed = 80f; // cuanto m�s alto, m�s r�pido se pega
    //        float newY = Mathf.Lerp(transform.position.y, hit.point.y, Time.deltaTime * lerpSpeed);
    //        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    //    }
    //}
    void OnDestroy()
    {
        if (ServiceLocator.Instance.TryGetDependency<ControllerPlayer>(out ControllerPlayer controller))
            controller.OnSpacebarDown -= EventMethodForDash;

        playerLife.OnPlayerHit -= ResetRollProperties;

        ServiceLocator.Instance.RemoveDependency<DashPlayer>();
    }
}
