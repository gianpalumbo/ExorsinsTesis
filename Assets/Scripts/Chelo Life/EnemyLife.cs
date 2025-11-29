using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//AGUS ADD-ON
using UnityEngine.VFX;
using System;
using UnityEngine.UI;

public class EnemyLife : Entity, IHealthBar
{
    [SerializeField] Rigidbody _rb;
    [SerializeField]private int soulReward = 100;    
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    public VisualEffect bloodVFX, disolveVFX;
    [SerializeField] float _disolveTime = 1;
    [SerializeField] Renderer _renderer;
    private MaterialPropertyBlock _mpb;
    Vector3 offsetY = new Vector3(0, 1, 0);
    [SerializeField] GameObject vfxSoulsReward;
    [SerializeField] Transform[] dropItems;

    //Agus ADD-ON ANIMATOR - SAQUE SETACTIVE - REEMPLACE POR UNA CORRUTINA QUE HACE ANIMACION DE MUERTE
    [SerializeField] private Animator _anim;
    //[SerializeField] private FSMEnemy _fsm; //CHELO WAS HERE: CON LA MODIFICACION DE AGUS CREO QUE ESTA NO VA MAS
    [SerializeField] private EliteEnemy _eliteEnemy;
    [SerializeField] private RestlessSoul _restlessSoul;
    [SerializeField] private RestlessAlly _restlessAlly;
    public event Action OnHit = delegate { };

    [SerializeField] BoxCollider _boxCollider;

    //CHELO WAS HERE hardcodee las referencias
    [Header("KARMIC REF")]
    [SerializeField] private GameObject _karmicTrigger;
    [SerializeField] private GameObject _karmicCanvas;
    [SerializeField] private KarmicMenu _myKarmicMenu;
    [SerializeField] private GameObject currentAlly;
    [SerializeField] private GameObject currentEnemy;

    [Header("DUPLICADOS RESET")]
    [SerializeField] private GameObject _initialKarmicTrigger;
    [SerializeField] private GameObject _initialKarmicCanvas;
    [SerializeField] private KarmicMenu _initialMyKarmicMenu;
    [SerializeField] private GameObject _initialCurrentAlly;
    [SerializeField] private GameObject _initialCurrentEnemy;
    [SerializeField] private Animator _initialAnimator;

    bool _isDead = false, isBoneScraper;
    public bool IsDead
    {
        get => _isDead;
        set => _isDead = value;
    }
    public bool isInvulnerable = false; //AGUS ADDON

    [Header("LifeBar values")]
    [SerializeField] Image ImageBackground;
    [SerializeField] Image lifeImage;
    [SerializeField] Image lazyLifeImage;
    [SerializeField] GameObject nameText;
    [SerializeField] Canvas myCanvas;
    [SerializeField] float lerpSpeed;
    
    //CHELO WAS HERE: OCULTAR Y MOSTRAR BARRA DE VIDA
    [SerializeField] private float hideDelay = 10f;  // Tiempo hasta que se oculta
    private Coroutine hideCoroutine;

    public HashSet<IHealthBar> activeBars = new HashSet<IHealthBar>();
    private Coroutine delayedReleaseCoroutine;

    //CHELO WAS HERE
    //private float _slowSpeed;
    //[Range(0f, 1f)] public float slowFactor = 0.5f; //lo meti en entity, cualquiera puede tener o no un % de slow
    //EN ENTITY NO SE DEJA VER POR NADIE, NI EN PUBLICO

    [Header("HAS KEY")]
    public bool hasKey = false;

    protected override void Awake()
    {
        //lifeImage = transform.FindInChildren("LifeBar").GetComponent<Image>();
        //lazyLifeImage = transform.FindInChildren("LazyLifeBar").GetComponent<Image>();

        _rb = GetComponent<Rigidbody>();


        _mpb = new MaterialPropertyBlock();
        base.Awake();
        initialPosition = transform.position;
        initialRotation = transform.rotation;

        _initialKarmicTrigger =  _karmicTrigger;
        _initialKarmicCanvas =  _karmicCanvas;
        _initialMyKarmicMenu =  _myKarmicMenu;
        _initialCurrentAlly =  currentAlly;
        _initialCurrentEnemy = currentEnemy;

        //_anim = GetComponent<Animator>();
        _initialAnimator = _anim;

        //_fsm = GetComponent<FSMEnemy>();
        _eliteEnemy = GetComponent<EliteEnemy>();
        _restlessAlly = GetComponent<RestlessAlly>();


        // se registra el enemigo en la lista estatica para el reset
        ResetTrigger.RegisterEnemy(this);
        //SI LO PIENSO PARA BOSSES TENGO QUE CAMBIAR UN POCO ESTA LOGICA PERO PARA ESTO SIRVE
        //SERIA QUE AL MORIR EL JUGADOR SE REINICIEN LOS ENEMIGOS, Y SE REINICIE EL JEFE SI NO LO MATARON ANTES

        _boxCollider.enabled = true;

        //CHELO WAS HERE
        if (_karmicTrigger == null) return; //salteo el error nulo si enemigos no lo tienen        
        UpdateLifeBar();
        //CHELO WAS HERE: OCULTAR Y MOSTRAR BARRA DE VIDA
        HideLifeBar();        
    }

    //protected override void Update()
    //{
    //}

    //private void Start()
    //{
    //    if (_restlessSoul != null) //RESETEO LA FSM
    //    {
    //        Debug.Log("EnemyLife: llamando ResetFSM a RestlessSoul");
    //        _restlessSoul.ResetFSM();
    //    }
    //}

    protected override void Update()
    {
        if (lifeImage == null || lazyLifeImage == null) return;
        UpdateLazyBar();
        var cam = ServiceLocator.Instance.GetDependency<CheloCamera>().transform;
        myCanvas.transform.rotation = Quaternion.LookRotation(cam.forward, cam.up);
    }

    #region LifeBar LazyLifeBar
    void UpdateLifeBar() => lifeImage.fillAmount = Life / maxLife; // actualizo la barra de vida

    void UpdateLazyBar()
    {
        lazyLifeImage.fillAmount = Mathf.Lerp(lazyLifeImage.fillAmount, lifeImage.fillAmount, lerpSpeed);
    }
    #endregion

    protected virtual void OnEnable() // se ejecuta cada vez que el objeto se activa. osea, reinicio la vida cada vez que el objeto se activa, por eso tambien le paso la posicion y rotacion inicial
    {
        //Life = maxLife;
        //// puedo resetear otras variables, estados, animaciones, etc.)

        //transform.position = initialPosition;
        //transform.rotation = initialRotation;
        isBoneScraper = GetComponent<BoneScraper>();
        //if (isBoneScraper)
        //    Debug.Log($"SOY BONESCRAPER? {isBoneScraper} Y SOY EL GO {gameObject.name}");
        //_boxCollider.enabled = true;
        //_anim.SetTrigger("Idle");
        //Awake();
        InitializeOnEnable();

        //CHELO WAS HERE: OCULTAR Y MOSTRAR BARRA DE VIDA
        HideLifeBar();       

    }

    public override void TakeDamage(float damage) //overrideo para enemigos
    {
        if (_isDead || isInvulnerable) return; // si ya esta muerto no hago nada
                
        ////CHELO WAS HERE: OCULTAR Y MOSTRAR BARRA DE VIDA
        HealthBarVisibilityManager.Instance.RequestShow(this, "Damage");

        if (delayedReleaseCoroutine != null) StopCoroutine(delayedReleaseCoroutine);
        delayedReleaseCoroutine = StartCoroutine(DelayedRelease());

        IEnumerator DelayedRelease()
        {
            yield return new WaitForSeconds(hideDelay);
            HealthBarVisibilityManager.Instance.ReleaseShow(this, "Damage");
            delayedReleaseCoroutine = null;
        }
        Life -= damage;
        //Debug.Log($"me hicieron {damage} de daño, tengo {Life} de vida");
        UpdateLifeBar();
        bloodVFX.SendEvent("BloodSplatter");
        OnHit?.Invoke(); //PREGUNTO SI ES NO ES NULL Y EJECUTO
        SoundManager.Instance.PlayOneShotFromIndex(4);
        
        if (Life <= 0 && !_isDead )
        {
            //AGUS WAS HERE, HARDCODEE QUE TE DE LLAVE SI LO MATAS Y SI HASKEY
            //if (hasKey && ServiceLocator.Instance.GetDependency<AttackEFSM>().keyCount < 3)
            //    ServiceLocator.Instance.GetDependency<AttackEFSM>().keyCount++;
            if (hasKey)
            {
                var efsm = ServiceLocator.Instance.GetDependency<AttackEFSM>();
                efsm.keyCount++;
                efsm.keyCountTMP.text = efsm.keyCount.ToString()+"/3 keys";
                Debug.Log($"Sumé llave. Ahora tengo: {efsm.keyCount}");
            }
            else
            {
                Debug.Log("Este enemigo NO tiene hasKey!");
            }

            _isDead = true;
            StartCoroutine(DeathDisolve());
            _rb.velocity = Vector3.zero;
            if (_eliteEnemy != null) _eliteEnemy.SendInputToFSM(EliteEnemy.EliteEnemyInputs.DIE);
            else if (_restlessAlly != null) _restlessAlly.SendInputToFSM(RestlessAlly.RSInputs.DIE);

            if(!isBoneScraper) //!TryGetComponent<BoneScraper>(out BoneScraper bs) CAMBIADO POR UN BOOL MAS RAPIDO
                _anim.SetTrigger("Death");
            
            if (_restlessAlly == null) 
            { 
                DropSouls();
                gameObject.GetComponent<BoxCollider>().isTrigger = true;
                _rb.useGravity = false;
            }
        }        
    }

    public void ShowLifeBar()
    {
        if (nameText != null)
            nameText.gameObject.SetActive(true);
        if (ImageBackground != null)
            ImageBackground.gameObject.SetActive(true);
        if (lifeImage != null)
            lifeImage.gameObject.SetActive(true);
        if (lazyLifeImage != null)
            lazyLifeImage.gameObject.SetActive(true);
    }

    public void HideLifeBar()
    {
        if (nameText != null)
            nameText.gameObject.SetActive(false);
        if (ImageBackground != null)
            ImageBackground.gameObject.SetActive(false);
        if (lifeImage != null)
            lifeImage.gameObject.SetActive(false);
        if (lazyLifeImage != null)
            lazyLifeImage.gameObject.SetActive(false);
    }
    
    public void TakeHeal(float heal)
    {
        Life += heal;
        UpdateLifeBar();
    }

    public override void Knockback(Vector3 vector, float knock)
    {
        if (_isDead) return;
        _rb.velocity = Vector3.zero; // reseteo la velocidad del rigidbody para que no se quede con la velocidad anterior
        //_rb.velocity += vector * knock;
        StartCoroutine(Knockback1(vector, knock, .025f));
    }

    IEnumerator DeathDisolve()
    {
        //Debug.Log("va");
        float t = 0;
        //QUIERO DESACTIVAR LAS COLISIONES Y LA GRAVEDAD ASI NO SE CAE
        while (t < 1)
        {
            t += Time.deltaTime / _disolveTime;
            if(t > .4f && t < .6f) disolveVFX.SendEvent("Play");
            _renderer.GetPropertyBlock(_mpb);
            _mpb.SetFloat("_Disolved", t);
            _renderer.SetPropertyBlock(_mpb);
            yield return null;
        }
        disolveVFX.SendEvent("Stop");
    }

    public void ReviveReset()
    {
        IsDead = false;

        if(TryGetComponent<BoneScraper>(out BoneScraper boneScraper))
        {
            boneScraper.Think();
        }
        if (TryGetComponent<VorcarbisEFSM>(out VorcarbisEFSM vorcarbis))
        {
            vorcarbis.Think();
        }

        StartCoroutine(UnDisolveCoroutine());
        //Debug.Log("HAGO UNDISSOLVE");
    }
    IEnumerator UnDisolveCoroutine()
    {
        float t = 1;
        //QUIERO DESACTIVAR LAS COLISIONES Y LA GRAVEDAD ASI NO SE CAE
        while (t > 0)
        {
            t -= Time.deltaTime / _disolveTime;
            if (t > .4f && t < .6f) disolveVFX.SendEvent("Play");
            if(_renderer != null)
                _renderer.GetPropertyBlock(_mpb);
            _mpb.SetFloat("_Disolved", t);
            if (_renderer != null)
                _renderer.SetPropertyBlock(_mpb);
            yield return null;
        }
        disolveVFX.SendEvent("Stop");
        gameObject.GetComponent<BoxCollider>().isTrigger = false;
    }

    public void DropSouls()
    {
        // manda los puntos por el singleton
        //Debug.Log("almas obtenidas " + soulReward);
        Instantiate(vfxSoulsReward, transform.position + offsetY, transform.rotation);
    }

    public void Death()
    {
        //CHELO WAS HERE, HARDCODEE PARA QUE APARESCA EL PANEL Y HAGA LA INSTANCIACION
        if (_karmicCanvas != null)
        {
            //Instantiate(_karmicTrigger, gameObject.transform.position, gameObject.transform.rotation);
            _karmicCanvas.SetActive(true);
            _myKarmicMenu._currentAlly = currentAlly;
            _myKarmicMenu._currentEnemy = currentEnemy;
            //Debug.Log($"{gameObject.name} cambiando current ally y enemy para el panel karmico");

        }
        if (dropItems.Length > 0)
        {
            foreach (var item in dropItems)
            {
                item.SetParent(null);
            }
        }
        gameObject.SetActive(false);
    }
    //CHELO WAS HERE
    private void InitializeOnEnable()
    {       
        life = maxLife;
        transform.position = initialPosition;
        transform.rotation = initialRotation;


        _karmicTrigger = _initialKarmicTrigger;
        _karmicCanvas = _initialKarmicCanvas;
        _myKarmicMenu = _initialMyKarmicMenu;
        currentAlly = _initialCurrentAlly;
        currentEnemy = _initialCurrentEnemy;


        _anim = _initialAnimator;

        //if (_fsm != null) //RESETEO LA FSM
        //{
        //    _fsm.ResetFSM();
        //}

        //SI REINICIO LA ESCENA HAY UN PROBLEMA NULO CON AWAKE Y ONENABLED, LO PONGO EN START
        //if (_restlessSoul != null) //RESETEO LA FSM
        //{
        //    _restlessSoul.ResetFSM();
        //}

        if (_anim != null) //RESETEO LOS TRIGGERS Y ANIMACION
        {
            _anim.ResetTrigger("Death");
            _anim.ResetTrigger("Idle");
            //_anim.Play("Idle");
        }

        //if (_restlessSoul!= null) _restlessSoul.ResetFSM();
        if (_boxCollider != null) _boxCollider.enabled = true; // REACTIVO COLISION
    }

    //CHELO WAS HERE
    public override void SpeedSlower()
    {
        //if (slowFactor <= 0) return;
        //_slowSpeed = _fsm.normalSpeed * slowFactor;
        //_fsm._currentSpeed = _fsm.slowSpeed;
    }

    IEnumerator Knockback1(Vector3 dir,float force, float duration)
    {
        var rb = GetComponent<Rigidbody>();
        while (duration > 0)
        {
            duration -= Time.deltaTime;
            rb.AddForce(dir * force * Time.deltaTime, ForceMode.Impulse);
            yield return null;
        }
    }

    //public override void SpeedReset()
    //{
    //    //if (slowFactor <= 0) return;
    //    _fsm.isSlowed = false;
    //    _fsm._currentSpeed = _fsm.normalSpeed;
    //}
}
