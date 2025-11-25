using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using System;
using UnityEngine.UI;

public class BossLife : Entity, IHealthBar
{
    [SerializeField] Rigidbody _rb;
    [SerializeField] private int soulReward = 100;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    public VisualEffect bloodVFX, disolveVFX;
    [SerializeField] float _disolveTime = 1;
    [SerializeField] Renderer _renderer;
    private MaterialPropertyBlock _mpb;
    Vector3 offsetY = new Vector3(0, 1, 0);
    [SerializeField] GameObject vfxSoulsReward;

    //Agus ADD-ON ANIMATOR - SAQUE SETACTIVE - REEMPLACE POR UNA CORRUTINA QUE HACE ANIMACION DE MUERTE
    [SerializeField] private Animator _anim;
    //[SerializeField] private FSMEnemy _fsm; //CHELO WAS HERE: CON LA MODIFICACION DE AGUS CREO QUE ESTA NO VA MAS
    [SerializeField] private EliteEnemy _eliteEnemy;
    [SerializeField] private RestlessSoul _restlessSoul;
    [SerializeField] private RestlessAlly _restlessAlly;
    public event Action OnHit = delegate { };

    [SerializeField] BoxCollider _boxCollider;

    ////CHELO WAS HERE hardcodee las referencias
    //[Header("KARMIC REF")]
    //[SerializeField] private GameObject _karmicTrigger;
    //[SerializeField] private GameObject _karmicCanvas;
    //[SerializeField] private KarmicMenu _myKarmicMenu;
    //[SerializeField] private GameObject currentAlly;
    //[SerializeField] private GameObject currentEnemy;



    //CHELO WAS HERE: REF CANVAS BOSS
    [Header("BOSS CANVAS")]
    //[SerializeField] public Canvas _bossCanvas;
    
    [SerializeField] Image ImageBackground;
    [SerializeField] Image lifeImage;
    [SerializeField] Image lazyLifeImage;
    //[SerializeField] GameObject nameText;
    [SerializeField] Canvas _bossCanvas;
    [SerializeField] private CanvasGroup _bossCanvasGroup;
    [SerializeField] float lerpSpeed;
    public bool isInvulnerable = false; //AGUS ADDON


    //CHELO WAS HERE: OCULTAR Y MOSTRAR BARRA DE VIDA
    //[SerializeField] private float _timeDelay = 3f;  // tiempo para que aparesca o desaparesca
    //private Coroutine hideCoroutine;

    //public HashSet<IHealthBar> activeBars = new HashSet<IHealthBar>();
    //private Coroutine delayedLifeBarCoroutine;



    public float fadeCanvasDuration = 1f;



    //[Header("DUPLICADOS RESET")]
    //[SerializeField] private GameObject _initialKarmicTrigger;
    //[SerializeField] private GameObject _initialKarmicCanvas;
    //[SerializeField] private KarmicMenu _initialMyKarmicMenu;
    //[SerializeField] private GameObject _initialCurrentAlly;
    //[SerializeField] private GameObject _initialCurrentEnemy;
    //[SerializeField] private Animator _initialAnimator;

    bool _isDead = false;
    public bool IsDead
    {
        get => _isDead;
        set => _isDead = value;
    }

    //CHELO WAS HERE
    //private float _slowSpeed;
    //[Range(0f, 1f)] public float slowFactor = 0.5f; //lo meti en entity, cualquiera puede tener o no un % de slow
    //EN ENTITY NO SE DEJA VER POR NADIE, NI EN PUBLICO

    protected override void Awake()
    {
        
        //lifeImage = transform.FindInChildren("LifeBar").GetComponent<Image>();
        //lazyLifeImage = transform.FindInChildren("LazyLifeBar").GetComponent<Image>();

        _rb = GetComponent<Rigidbody>();

        _mpb = new MaterialPropertyBlock();
        base.Awake();
        initialPosition = transform.position;
        initialRotation = transform.rotation;

        //_initialKarmicTrigger = _karmicTrigger;
        //_initialKarmicCanvas = _karmicCanvas;
        //_initialMyKarmicMenu = _myKarmicMenu;
        //_initialCurrentAlly = currentAlly;
        //_initialCurrentEnemy = currentEnemy;

        ////_anim = GetComponent<Animator>();
        //_initialAnimator = _anim;

        //_fsm = GetComponent<FSMEnemy>();
        //_eliteEnemy = GetComponent<EliteEnemy>();
        //_restlessAlly = GetComponent<RestlessAlly>();



        ResetTrigger.RegisterEnemy(this); // se registra el enemigo en la lista estatica para el reset
        //SI LO PIENSO PARA BOSSES TENGO QUE CAMBIAR UN POCO ESTA LOGICA PERO PARA ESTO SIRVE
        //SERIA QUE AL MORIR EL JUGADOR SE REINICIEN LOS ENEMIGOS, Y SE REINICIE EL JEFE SI NO LO MATARON ANTES

        _boxCollider.enabled = true;

        //CHELO WAS HERE
        //if (_karmicTrigger == null) return; //salteo el error nulo si enemigos no lo tienen        
        UpdateLifeBar();

        //CHELO WAS HERE: OCULTAR Y MOSTRAR BARRA DE VIDA
        //HideLifeBar();        

        //forzar alpha en 1, para asegurarme
        _bossCanvasGroup.gameObject.SetActive(true);
        //Color c = _bossCanvas.color;
        //c.a = 1f;
        //_bossCanvas.color = c;
        _bossCanvasGroup.alpha = 0f; //el alpha del canvasgroup (todos sus componentes) es 0

        //StartCoroutine(Fade(0f, fadeDuration));


        //_bossCanvas.enabled = false; //apago el canvas del boss
        //_bossCanvasGroup.gameObject.SetActive(false);
        _bossCanvas.gameObject.SetActive(false);



    }

    void OnDestroy()
    {
        ServiceLocator.Instance.RemoveDependency<BossLife>();
    }

    protected override void Update()
    {
        if (lifeImage == null || lazyLifeImage == null) return;
        UpdateLazyBar();
        if (Camera.main != null)
        {
            var cam = Camera.main.transform;
            _bossCanvas.transform.rotation = Quaternion.LookRotation(cam.forward, cam.up);
        }
    }

    #region LifeBar LazyLifeBar
    void UpdateLifeBar() => lifeImage.fillAmount = Life / maxLife; // actualizo la barra de vida
    void UpdateLazyBar() { lazyLifeImage.fillAmount = Mathf.Lerp(lazyLifeImage.fillAmount, lifeImage.fillAmount, lerpSpeed); }
    #endregion

    protected virtual void OnEnable() // se ejecuta cada vez que el objeto se activa. osea, reinicio la vida cada vez que el objeto se activa, por eso tambien le paso la posicion y rotacion inicial
    {
        InitializeOnEnable();

        ServiceLocator.Instance.RegisterDependency<BossLife>(this);
        //CHELO WAS HERE: OCULTAR Y MOSTRAR BARRA DE VIDA
        //HideLifeBar();
    }

    public override void TakeDamage(float damage) //overrideo para enemigos
    {
        if (_isDead || isInvulnerable) return; // si ya esta muerto no hago nada

        ////CHELO WAS HERE: OCULTAR Y MOSTRAR BARRA DE VIDA
        //HealthBarVisibilityManager.Instance.RequestShow(this, "Damage");        

        Life -= damage;
        UpdateLifeBar();
        bloodVFX.SendEvent("BloodSplatter");
        OnHit?.Invoke(); //PREGUNTO SI ES NO ES NULL Y EJECUTO
        SoundManager.Instance.PlayOneShotFromIndex(4);

        if (Life <= 0 && !_isDead)
        {
            _isDead = true;
            //StartCoroutine(DeathDisolve());
            _rb.velocity = Vector3.zero;
            //if (_eliteEnemy != null) _eliteEnemy.SendInputToFSM(EliteEnemy.EliteEnemyInputs.DIE);
            //else if (_restlessAlly != null) _restlessAlly.SendInputToFSM(RestlessAlly.RSInputs.DIE);
            //_anim.SetTrigger("Death");
            //if (_restlessAlly == null)
            //{
            //    DropSouls();
            //    gameObject.GetComponent<BoxCollider>().isTrigger = true;
            //    _rb.useGravity = false;
            //}
        }
    }

    public void ShowLifeBar()
    {
        //if (nameText != null) nameText.gameObject.SetActive(true);
        //if (ImageBackground != null) ImageBackground.gameObject.SetActive(true);
        //if (lifeImage != null) lifeImage.gameObject.SetActive(true);
        //if (lazyLifeImage != null) lazyLifeImage.gameObject.SetActive(true);


        //if (delayedLifeBarCoroutine != null) StopCoroutine(delayedLifeBarCoroutine);
        //delayedLifeBarCoroutine = StartCoroutine(DelayedLifeBar());

        //blackPanel.gameObject.SetActive(true);
        //Color c = blackPanel.color;
        //c.a = 1f;
        //blackPanel.color = c;
        _bossCanvasGroup.alpha = 0f;
        _bossCanvas.gameObject.SetActive(true);
        StartCoroutine(Fade(1f, fadeCanvasDuration));
        Debug.Log("APARECE BARRA DE VIDA BOSS");
    }
    public void HideLifeBar()
    {
        //if (nameText != null) nameText.gameObject.SetActive(false);
        //if (ImageBackground != null) ImageBackground.gameObject.SetActive(false);
        //if (lifeImage != null) lifeImage.gameObject.SetActive(false);
        //if (lazyLifeImage != null) lazyLifeImage.gameObject.SetActive(false);


        //if (delayedLifeBarCoroutine != null) StopCoroutine(delayedLifeBarCoroutine);
        //delayedLifeBarCoroutine = StartCoroutine(DelayedLifeBar());

        //blackPanel.gameObject.SetActive(true);
        //Color c = blackPanel.color;
        //c.a = 1f;
        //blackPanel.color = c;

        _bossCanvasGroup.alpha = 1f;
        StartCoroutine(Fade(0f, fadeCanvasDuration));
        _bossCanvas.gameObject.SetActive(false);
        Debug.Log("DESAPARECE BARRA DE VIDA BOSS");

    }
    //IEnumerator DelayedLifeBar()
    //{
    //    yield return new WaitForSeconds(_timeDelay);
    //    //HealthBarVisibilityManager.Instance.ReleaseShow(this, "Damage");
    //    delayedLifeBarCoroutine = null;
    //}



    IEnumerator Fade(float targetAlpha, float duration)
    {
        //float startAlphaValue = blackPanel.color.a;
        float startAlphaValue = _bossCanvasGroup.alpha;

        //_bossCanvasGroup.alpha = 0f;
        float timeElapsed = 0f;
        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            //var c = blackPanel.color;
            var c = _bossCanvasGroup.alpha;
            c = Mathf.Lerp(startAlphaValue, targetAlpha, timeElapsed / duration);
            //blackPanel.color = c;
            _bossCanvasGroup.alpha = c;
            yield return null;
        }
        //var endAlphaValue = blackPanel.color;
        var endAlphaValue = _bossCanvasGroup.alpha;
        endAlphaValue = targetAlpha;
        _bossCanvasGroup.alpha = endAlphaValue;
    }




    public void TakeHeal(float heal) //recuperar vida si tiene esa opcion
    {
        Life += heal;
        UpdateLifeBar();
    }

    IEnumerator DeathDisolve()
    {
        float t = 0;
        //QUIERO DESACTIVAR LAS COLISIONES Y LA GRAVEDAD ASI NO SE CAE
        while (t < 1)
        {
            t += Time.deltaTime / _disolveTime;
            if (t > .4f && t < .6f) disolveVFX.SendEvent("Play");
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

        if (TryGetComponent<VorcarbisEFSM>(out VorcarbisEFSM vorcarbis))
        {
            vorcarbis.Think();
        }

        //StartCoroutine(UnDisolveCoroutine());
        //Debug.Log("HAGO UNDISSOLVE");
    }

    //IEnumerator UnDisolveCoroutine()
    //{
    //    float t = 1;
    //    //QUIERO DESACTIVAR LAS COLISIONES Y LA GRAVEDAD ASI NO SE CAE
    //    while (t > 0)
    //    {
    //        t -= Time.deltaTime / _disolveTime;
    //        if (t > .4f && t < .6f) disolveVFX.SendEvent("Play");
    //        _renderer.GetPropertyBlock(_mpb);
    //        _mpb.SetFloat("_Disolved", t);
    //        _renderer.SetPropertyBlock(_mpb);
    //        yield return null;
    //    }
    //    disolveVFX.SendEvent("Stop");
    //    gameObject.GetComponent<BoxCollider>().isTrigger = false;
    //}

    public void DropSouls()
    {
        // manda los puntos por el singleton
        Debug.Log("almas obtenidas " + soulReward);
        Instantiate(vfxSoulsReward, transform.position + offsetY, transform.rotation);
    }

    public void OpenkarmicCanvas()
    {
        ////CHELO WAS HERE, HARDCODEE PARA QUE APARESCA EL PANEL Y HAGA LA INSTANCIACION
        //if (_karmicCanvas != null)
        //{
        //    //Instantiate(_karmicTrigger, gameObject.transform.position, gameObject.transform.rotation);
        //    _karmicCanvas.SetActive(true);
        //    _myKarmicMenu._currentAlly = currentAlly;
        //    _myKarmicMenu._currentEnemy = currentEnemy;
        //    Debug.Log($"{gameObject.name} cambiando current ally y enemy para el panel karmico");

        //}
        //gameObject.SetActive(false);
    }


    //CHELO WAS HERE
    private void InitializeOnEnable()
    {
        life = maxLife;
        transform.position = initialPosition;
        transform.rotation = initialRotation;


        //_karmicTrigger = _initialKarmicTrigger;
        //_karmicCanvas = _initialKarmicCanvas;
        //_myKarmicMenu = _initialMyKarmicMenu;
        //currentAlly = _initialCurrentAlly;
        //currentEnemy = _initialCurrentEnemy;


        //_anim = _initialAnimator;



        //if (_fsm != null) //RESETEO LA FSM
        //{
        //    _fsm.ResetFSM();
        //}

        //SI REINICIO LA ESCENA HAY UN PROBLEMA NULO CON AWAKE Y ONENABLED, LO PONGO EN START
        //if (_restlessSoul != null) //RESETEO LA FSM
        //{
        //    _restlessSoul.ResetFSM();
        //}


        //if (_anim != null) //RESETEO LOS TRIGGERS Y ANIMACION
        //{
        //    _anim.ResetTrigger("Death");
        //    _anim.ResetTrigger("Idle");
        //    //_anim.Play("Idle");
        //}

        //if (_restlessSoul!= null) _restlessSoul.ResetFSM();
        if (_boxCollider != null) _boxCollider.enabled = true; // REACTIVO COLISION
    }
}