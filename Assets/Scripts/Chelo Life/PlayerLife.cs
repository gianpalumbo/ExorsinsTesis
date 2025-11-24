using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.VFX;

public class PlayerLife : Entity
{
    [Header("UI Life")]
    public Slider mySlider;
    public Image fillImage;
    public Image lazyImage; //AGUS ADD-ON
    public Gradient lifeGradient;
    public GameObject deathPanel;

    [Header("playerattack")]
    public GameObject attackTrigger;

    //[Header("Camera")]
    //public Camera myCamera;

    [Header("Controls")]
    public KeyCode healCode;
    public KeyCode dmgCode;
    //public KeyCode attackCode; //debug da�o trigger

    Animator _anim;

    [HideInInspector] public bool isDead = false, hasBeenHurt, hasBeenHealed, isInvulnerable = false, canFlinch = true; //AGUS ADD-ON HASBEENHURT HASBEENHEALED BOOL
    bool _isPoisoned = false;
    //CHELO WAS HERE: AGREGO EL DEATHMANAGER
    [SerializeField] private DeathManager myDeathManager;

    float poisonDmg, timePoisoned, intervalPoison = .75f;
    float counter, poisonCounter;

    public event Action OnPlayerHit;
    [SerializeField] VisualEffect[] poisonVFXs;
    [SerializeField] Material[] mats;

    //awake tiene ENTITY

    //CHELO WAS HERE: GUARDO EL ULTIMO SANTUARIO VISITADO PARA QUE CUANDO MUERA O INGRESE AL JUEGO LO MANTE AHI
    //public Vector3 lastSpawn;
    //CHELO WAS HERE: QUITE LA LOGICA DE LOS SPAWNPOINTS PARA QUE LO HAGA EN UN DEATHMANAGER



    //CHELO WAS HERE: id para pasarlo al CursorUIManager y se desbloqueen los mouse
    private readonly string id = "PlayerLife";

    public Rigidbody MyRigidbody;

    protected override void Awake()
    {
        base.Awake();

        ServiceLocator.Instance.RegisterDependency<PlayerLife>(this);

        //if (lastSpawn == new Vector3(0,0,0))
        //{
        //    Debug.Log($"{gameObject.name} LASTSPAWN ES NULO");
        //    lastSpawn = gameObject.transform.position;
        //}

        deathPanel.SetActive(false);
    }
    private void OnDestroy()
    {
        ServiceLocator.Instance.RemoveDependency<PlayerLife>();
    }
    private void Start()
    {
        _anim = GetComponent<Animator>();

        StartUIBars();

        UpdateUI(); //SOLO PARA EL GRADIENTE QUEDO ESTA
    }

    void StartUIBars()
    {
        //AGUS ADD-ON
        lazyImage.fillAmount = Life / MaxLife;
        mySlider.maxValue = MaxLife;
        mySlider.value = Life;
    }

    public override void TakeDamage(float damage) //AGUS ADDON PARA EL VENENO NO QUIERO QUE CADA VEZ QUE LE HAGA DAÑO HAGA ANIMACION ENTONCES PONGO BOOL
    {
        if (isDead || isInvulnerable) return;

        base.TakeDamage(damage);

        OnPlayerHit?.Invoke(); //SI ONHIT NO ES UN DELEGATE NULO CORRERLO

        _anim.SetTrigger("onHit");

        //OLD REFMANAGER
        //ReferenceManager.Instance.controller.isAttacking = false;
        //NEW SERVICELOCATOR ABSTRACT
        //ServiceLocator.Instance.TryGetDependency<ControllerPlayer>(out var controller);
        //controller.isAttacking = false;
        //SI ES PARA EVITAR QUE ATAQUE DURANTE ON HIT NO DEBERIA HABER PROBLEMA PORQUE ONHIT NO ESTA CONECTADO A ATTACK Y COMO TIENE ROOT NO SE MUEVE

        SoundManager.Instance.PlayOneShotFromIndex(8);

        LifeBarTakesDamage();

        //CHELO WAS HERE: rb quitar el bug de los gordos
        //MyRigidbody.velocity = Vector3.zero;
        //MyRigidbody.velocity = new Vector3(0, 0, 0);

        if (Life <= 0)
        {
            Debug.Log("entity murio");
            //Cursor.lockState = CursorLockMode.None;
            //Cursor.visible = true;
            //CHELO WAS HERE: IMPLEMENTACION DEL CANVAS LO HACE POR ANIMATION TRIGGER
            //CHELO WAS HERE: IMPLEMENTACION DEL CANVAS LO HACE POR VIDA <= 0



            //CHELO WAS HERE: AGREGUE ID AL CURSOR MANAGER
            CursorUIManager.Instance.RequestCursorState(true, id);



            _anim.SetTrigger("death");
        }
    }
    public void TakeDamageWithoutFlinching(float dmg)
    {
        if (isDead || isInvulnerable) return;

        OnPlayerHit?.Invoke(); //SI ONHIT NO ES UN DELEGATE NULO CORRERLO

        Life -= dmg;
        LifeBarTakesDamage();

        //SoundManager.Instance.PlayOneShotFromIndex(8);

        if (Life <= 0)
        {
            CursorUIManager.Instance.RequestCursorState(true, id);

            _anim.SetTrigger("death");
        }
    }

    public void TakeAllDamage()
    {
        life -= maxLife;

        //CHELO WAS HERE: rb quitar el bug de los gordos
        //MyRigidbody.velocity = Vector3.zero;

        LifeBarTakesDamage();

        MyRigidbody.velocity = Vector3.zero;
        MyRigidbody.angularVelocity = Vector3.zero;

        if (Life <= 0)
        {
            Debug.Log("entity murio");
            //Cursor.lockState = CursorLockMode.None;
            //Cursor.visible = true;
            //CHELO WAS HERE: IMPLEMENTACION DEL CANVAS LO HACE POR ANIMATION TRIGGER
            //CHELO WAS HERE: IMPLEMENTACION DEL CANVAS LO HACE POR VIDA <= 0



            //CHELO WAS HERE: AGREGUE ID AL CURSOR MANAGER
            CursorUIManager.Instance.RequestCursorState(true, id);



            _anim.SetTrigger("death");
        }
    }

    public void TurnOnPanel()
    {
        //ADDON PRENDO MOUSE CUANDO ABRO CANVAS
        UtilitiesAgus.ToggleCursor(true);

        deathPanel.SetActive(true);
        //QUE NO DESACTIVE NADA
        //this.enabled = false;
    }

    public void TakeHeal(float heal)  // metodo para suma vida al life
    {
        Life += heal;
        Debug.Log("vida sumada: " + Life);

        SoundManager.Instance.PlayOneShotFromIndex(11);
        LifeBarGetsHealed();

        if (Life >= MaxLife) { Debug.Log("entity tiene vida maxima"); }
    }

    public void TakeAllHeal()  // metodo recargar toda la vida
    {
        Life += maxLife;
        Debug.Log("vida maxima" + Life);

        SoundManager.Instance.PlayOneShotFromIndex(11);
        LifeBarGetsHealed();

        if (Life >= MaxLife) { Debug.Log("entity tiene vida maxima"); }
    }
    void LifeBarTakesDamage()
    {
        //AGUS ADDON
        lazyImage.color = new Color(1f, 0.4f, 0f);
        hasBeenHurt = true;
        mySlider.value = Life; //AGUS ADD-ON: actualizo la barra de vida inmediatamente al recibir daño
    }
    void LifeBarGetsHealed()
    {
        //AGUS ADDON
        hasBeenHealed = true;
        lazyImage.color = Color.green; //AGUS ADD-ON: cambio el color de la barra de vida al curarme
        lazyImage.fillAmount = Life / MaxLife; //AGUS ADD-ON: actualizo la barra de vida inmediatamente al recibir curacion
    }
    public void UpdateUI()
    {
        if (mySlider != null)
        {
            //mySlider.maxValue = MaxLife;
            //mySlider.value = Life;

            if (fillImage != null && lifeGradient != null)
            {
                float t = Life / MaxLife;
                fillImage.color = lifeGradient.Evaluate(t);
            }
        }
    }

    public void PoisonPlayer(float dmg, float time)
    {
        poisonDmg = dmg;
        timePoisoned = time;
        _isPoisoned = true;
        counter = 0;
        //nigga was here
        foreach (var poison in poisonVFXs)
        {
            poison.SendEvent("Play");
        }
        foreach (var mat in mats)
        {
            mat.SetFloat("_IsNeutral", 0);
        }
    }
    protected override void Update()
    {
        //base.Update();
        isDead = Life <= 0;
        if (Input.GetKeyDown(dmgCode))
        {
            Debug.Log("Damage");
            TakeDamage(20);
        }
        if (Input.GetKeyDown(healCode))
        {
            Debug.Log("Recovery");
            TakeHeal(20);
        }
        UpdateUI();

        //AGUS ADD-ON: LERP a la barra de vida lazy
        if (hasBeenHurt)
        {
            if (!(Mathf.Abs(lazyImage.fillAmount - (life / maxLife)) < 0.01f))
                lazyImage.fillAmount = Mathf.Lerp(lazyImage.fillAmount, life / maxLife, 0.05f); //HARDCODE .05f
            else
            {
                //Debug.LogWarning("Lazy bar interpolation finished");
                hasBeenHurt = false; //AGUS ADD-ON: si la barra de vida ya llego al valor, no tiene sentido seguir interpolando
            }
        }
        //AGUS ADD-ON: LERP a la barra de vida
        if (hasBeenHealed)
        {
            if (!(Mathf.Abs(mySlider.value - life) < 0.01f))
                mySlider.value = Mathf.Lerp(mySlider.value, life, 0.05f); //HARDCODE .05f
            else
            {
                Debug.LogWarning("Life bar interpolation finished");
                hasBeenHealed = false; //AGUS ADD-ON: si la barra de vida ya llego al valor, no tiene sentido seguir interpolando
            }
        }

        if (_isPoisoned) //SI ESTOY ENVENENADO ENTRO ACA
        {
            //Aca poner efecto de envenenamiento


            counter += Time.deltaTime; //CUENTO TIEMPO PARA CORTAR EL ENVENENAMIENTO
            if (counter < timePoisoned)
            {
                if (PoisonCounter(intervalPoison)) //CADA TANTOS INTERVALOS CUANTO PARA HACER DAÑO 
                {
                    var tempDMG = UnityEngine.Random.Range(poisonDmg / 2, poisonDmg * 2);
                    TakeDamageWithoutFlinching(tempDMG);
                    ResetPoisonCounter();   //Y CUANDO SE TERMINE EL INTERVALO ENTRO A RESETTEAR
                }
                Debug.Log($"isPoisoned: {_isPoisoned} doing to life {poisonDmg} aprroximately now you have {Life}");
            }
            else
            {
                _isPoisoned = false;
                foreach (var poison in poisonVFXs)
                {
                    poison.SendEvent("Stop");
                }
                foreach (var mat in mats)
                {
                    mat.SetFloat("_IsNeutral", 1);
                }
            }
        }
        //if (Input.GetKeyDown(healCode)) gameObject.transform.position = lastSpawn;
    }
    bool PoisonCounter(float time)
    {
        poisonCounter += Time.deltaTime;
        return poisonCounter >= time;
    }
    void ResetPoisonCounter() => poisonCounter = 0;

    public override void Knockback(Vector3 dir, float force)
    {
        if (isDead) return;
        MyRigidbody.velocity = Vector3.zero; // reseteo la velocidad del rigidbody para que no se quede con la velocidad anterior
        StartCoroutine(KnockbackWithStunCoroutine(dir, force, 0.25f, .75f)); //LO HAGO NEGATIVO PARA TRAERLO HACIA EL PLAYER Y QUE COMBEE MEJOR
    }

    IEnumerator KnockbackWithStunCoroutine(Vector3 dir, float force, float duration, float stunDuration)
    {
        while (duration > 0)
        {
            duration -= Time.deltaTime;
            ServiceLocator.Instance.GetDependency<PlayerMVC>().SetResting(true);
            MyRigidbody.AddForce(dir * force * Time.deltaTime, ForceMode.Impulse);
            yield return null;
        }
        ServiceLocator.Instance.GetDependency<PlayerMVC>().SetResting(true);
        yield return new WaitForSeconds(stunDuration);
        ServiceLocator.Instance.GetDependency<PlayerMVC>().SetResting(false);
    }

    public void ResetPlayer()
    {
        Life = MaxLife;
        _anim.ResetTrigger("death");
        deathPanel.SetActive(false);
        ServiceLocator.Instance.TryGetDependency<ControllerPlayer>(out var controller);
        controller.isAttacking = false;

        _anim.Rebind();
        _anim.Update(0f);
        hasBeenHealed = true;
        UpdateUI();

        //CHELO WAS HERE: ELIMINO ID DEL CURSOR MANAGER
        CursorUIManager.Instance.ReleaseCursorRequest(id);



    }

    //public void LastSpawnPoint(Vector3 spawnLocation)
    //{
    //    lastSpawn = spawnLocation;
    //}

    //private void Respawn()
    //{
    //    Debug.Log("entity revivio");
    //    Cursor.lockState = CursorLockMode.Locked;
    //    Cursor.visible = false;
    //}
}
