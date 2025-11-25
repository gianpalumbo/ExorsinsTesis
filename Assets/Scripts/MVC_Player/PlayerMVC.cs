using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.VFX;

using UnityEngine.SceneManagement;

public class PlayerMVC : MonoBehaviour
{
    ModelPlayer _model;

    public StartingScene startingScene;

    [SerializeField] Vector3 caveSpawn, outsideSpawn, castleSpawn;
    public enum StartingScene
    {
        NoScene,
        Cave,
        Outside,
        Castle_1,
        Castle
    };

    public TextMeshProUGUI textFade;

    public ModelPlayer Model { get { return _model; } }
    ViewPlayer _view;
    ControllerPlayer _controller;
    public ControllerPlayer Controller { get { return _controller; } }

    public float speed, originalSpeed, jumpStrenght, rollStrenght, rollCD, maxLife, sensitivity,
        clampViewY, maxStamina, mana, manaPerSecond, maxMana;

    Vector2 _turn;
    float life, counterRoll, stamina;
    bool isGrounded, canRoll;
    [SerializeField] VisualEffect[] _karmicVfxs;
    [SerializeField] Material[] _playerMats;
    [SerializeField] Rigidbody myRB;
    [SerializeField] Animator animator;
    public Animator AnimatorPlayer
    {
        get { return animator; }
    }
    [SerializeField] Transform pivot;

    [SerializeField] Image sliderStamina;
    [SerializeField] TextMeshProUGUI tmp;

    [SerializeField] Transform cameraTransform;
    [SerializeField] public CheloCamera cameraOrbit;

    //CHELO WAS HERE: propiedad del controller para que el santuario pueda agarrarla y bloquear inputs del jugador al ingresar
    //public ControllerPlayer Controller => _controller;
    //public ViewPlayer View => _view;

    //CHELO WAS HERE: var para bloquear inputs
    public bool isResting = false;


    [Header("Karmic Points")]
    [SerializeField] private int _karmicPoints = 0;
    private int _actualKarmicPoints;

    [Header("Summon Skill")]
    public KeyCode soulSummon;
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private List<GameObject> _acquiredSouls = new List<GameObject>();
    // El KarmicMenu le pasa un "alma", por ahora un gameobject, despues veo como hacerlo para c/u
    private void Awake()
    {
        _model = new ModelPlayer(myRB, transform, speed, jumpStrenght, rollStrenght, maxLife, life, isGrounded, canRoll,
            counterRoll, rollCD, sensitivity, clampViewY, _turn, pivot, stamina, maxStamina, cameraTransform, cameraOrbit, mana, manaPerSecond, maxMana, ServiceLocator.Instance.GetDependency<SkillManager>());
        _view = new ViewPlayer(_model, animator, transform);
        _controller = new ControllerPlayer(_model);

        ServiceLocator.Instance.RegisterDependency<PlayerMVC>(this);
        ServiceLocator.Instance.RegisterDependency<ControllerPlayer>(_controller);
        ServiceLocator.Instance.RegisterDependency<ModelPlayer>(_model);

        myRB = GetComponent<Rigidbody>();

        AdditiveSceneManagerAgus.Initialize(this);
        if (!string.IsNullOrEmpty(GetSceneName(startingScene)))
            AdditiveSceneManagerAgus.LoadSceneAdditiveByName(GetSceneName(startingScene));

        //ARRANCO FREEZEADO PARA NO CAERME Y ME DESFREEZEO DESPUES
        FreezeAllRB();
    }

    public void LoadScene(StartingScene scene)
    {
        if (scene == StartingScene.Cave) _spawnPoint.position = caveSpawn;
        else if (scene == StartingScene.Outside) _spawnPoint.position = outsideSpawn;
        else if (scene == StartingScene.Castle) _spawnPoint.position = castleSpawn;

        transform.position = _spawnPoint.position;

        AdditiveSceneManagerAgus.LoadSceneAdditiveByName(GetSceneName(scene), GetSceneName(startingScene));

        startingScene = scene;
    }

    public static string GetSceneName(StartingScene scene)
    {
        switch (scene)
        {
            case StartingScene.Cave: return "Cave_1";
            case StartingScene.Outside: return "Outside_1";
            case StartingScene.Castle_1: return "Castle_1";
            case StartingScene.Castle: return "Castle";
            default: return "";
        }
    }

    private void OnDestroy()
    {
        if (_model != null)
        {
            _model.UnsubscribeFromSkillManager();
            ServiceLocator.Instance.RemoveDependency<PlayerMVC>();
            ServiceLocator.Instance.RemoveDependency<ControllerPlayer>();
            ServiceLocator.Instance.RemoveDependency<ModelPlayer>();
        }
    }
    private void Start()
    {
        SoundManager.Instance.ChangeToCaveAmbience();

        tmp.text = "Karmic Points: " + _karmicPoints;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false; // Make the cursor visible

        if (_karmicPoints == 0)
        {
            foreach (var kv in _karmicVfxs) kv.SendEvent("Stop");
            foreach (var mat in _playerMats) mat.SetFloat("_IsNeutral", 1);
        }
    }
    public void AcquireSoul(GameObject soul)
    {
        if (soul == null) return;

        //soul.gameObject.tag = "Ally";
        //soul.ChangeTarget("Enemy");
        _acquiredSouls.Add(soul);
        //soul.SetActive(false);
        Debug.Log($"conteo actual de listas ally: {_acquiredSouls.Count} ");
    }

    // invoco con la Q, despues lo arreglo
    public void SummonSouls(Transform spawnPoint)
    {
        foreach (var soul in _acquiredSouls)
        {
            //if (soul == null)
            if (_acquiredSouls.Count == 0)
            {
                Debug.Log("SUMMON SOULS: No hay almas en la lista");
                return;
            }

            Debug.Log("SUMMON SOULS: hay almas en la lista");
            // Instancio y guardo la nueva copia para poder activarla
            //esto es encesario porque sino tengo referencia del prefab, y no puedo prender un prefab que ni esta en escena porque es algo generico, asi que genero una copia que guardo en una variable
            GameObject clone = Instantiate(soul, spawnPoint.position, Quaternion.identity);
            clone.SetActive(true);

            //AGUS WAS HERE - CAPPEO DEPENDIENDO CUANTAS SOULS TENGO
            _acquiredSouls.RemoveAt(0);
            if (_acquiredSouls.Count <= 0)
                break;

            // restaura su vida, cambio de tag, o los vfx antes de eso, etc
            //clone.tag = "Enemy";

            // les reseteo la vida con un ResetToMax() cuando tengan el OnEnable();?
            //var enemylife = soul.GetComponent<enemylife>();
            //if (enemylife != null)
            //health.ResetToMax();

            // le tengo que cambiar los target cuando lo invoque el jugador porque sino lo va a atacar y le cambio el tag para no atacarlo por accidente
            //soul.target = "Enemy"
            //soul.tag = "Shadow";

            Debug.Log("INVOCACION de " + clone.name);
        }

        // Opcional: limpiar la lista si no quiero usar la mismas almas de nuevo
        //_acquiredSouls.Clear();
    }

    public void DisableThisScript()
    {
        this.enabled = false;
        SoundManager.Instance.PlayOneShotFromIndex(9); // Sonido de desactivacion
    }

    public void AddSoulsNew(int souls)
    {
        PointsManager.Instance.AddPoints(souls);
    }

    public int GetEndingFromKarma()
    {
        if (_karmicPoints == 0) return 0; // Neutral Ending
        else if (_karmicPoints > 0) return 1; // Good Ending
        else return -1; // Bad Ending
    }

    private void FixedUpdate()
    {
        //CHELO WAS HERE: var para bloquear inputs
        //if (Controller.isResting) return;
        //if (isResting) return;

        if (isResting == true) return;

        _controller.ArtificialFixed();
    }

    RaycastHit hit;
    public void Update()
    {
        sliderStamina.fillAmount = _model.UpdateStaminaBar() / maxStamina;

        //CHELO WAS HERE: var para bloquear inputs
        //if (Controller.isResting) return;
        //if (isResting) return;
        if (isResting == true) return;
        _controller.ArtificialUpdate();

        if (Physics.Raycast(transform.position, Vector3.down, out hit, 2f))
        {
            Vector3 floorNormal = hit.normal;
            myRB.AddForce(-floorNormal, ForceMode.VelocityChange);
        }

        if (Input.GetKeyDown(KeyCode.Alpha7) && !AdditiveSceneManagerAgus.isLoading)
        {
            AdditiveSceneManagerAgus.isLoading = true;
            LoadScene(StartingScene.Cave);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha8) && !AdditiveSceneManagerAgus.isLoading)
        {
            AdditiveSceneManagerAgus.isLoading = true;
            LoadScene(StartingScene.Outside);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha9) && !AdditiveSceneManagerAgus.isLoading)
        {
            AdditiveSceneManagerAgus.isLoading = true;
            LoadScene(StartingScene.Castle);
        }

        //AGUS ADDON
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayFootsteps();

        //NUEVO CHELO LOGICA DEL SUMMON SOULS, ES PARA PROBAR
        if (Input.GetKeyDown(soulSummon)) { SummonSouls(_spawnPoint); }
    }

    //public void LateUpdate()
    //{
    //}

    //CHELO WAS HERE: var para bloquear inputs
    //public void SetResting(bool resting, ViewPlayer view = null)
    public bool SetResting(bool resting)
    {
        isResting = resting;

        Debug.Log($"Resing es: " + isResting);
        //if (isResting)
        //{
        //    _model.SetVelocity(Vector3.zero);
        //    view?.SetAnimationMoving(false);
        //}
        if (resting) animator.SetFloat("velocity", 0);

        return isResting;
    }

    public void FreezeAllRB() => myRB.constraints = RigidbodyConstraints.FreezeAll;
    public void FreezeRotRB()
    {
        myRB.constraints = RigidbodyConstraints.FreezeRotation; // | RigidbodyConstraints.FreezePositionY;
    }

    public void GoForwardWithRB(float strenght)
    {
        if (isResting == true) return;

        myRB.velocity += transform.forward * strenght;
    }

    public void AnimToGettingBit()
    {
        animator.SetTrigger("OnHit");

        SetResting(true);
    }

    #region KarmicSystem
    public int AddKarmicPoints(int addKarmicPoints)
    {
        _karmicPoints += addKarmicPoints;
        tmp.text = "Karmic Points: " + _karmicPoints;
        Debug.Log("se sumaron puntos al sistema karmico");
        //Gian was here
        if (_karmicPoints > 0)
        {
            foreach (var kv in _karmicVfxs)
            {
                kv.SendEvent("Play");
                kv.SetBool("IsEvil", false);
            }
            foreach (var mat in _playerMats)
            {
                mat.SetFloat("_IsEvil", 0);
                mat.SetFloat("_IsNeutral", 0);
            }
        }
        else if (_karmicPoints == 0)
        {
            foreach (var kv in _karmicVfxs) kv.SendEvent("Stop");
            foreach (var mat in _playerMats) mat.SetFloat("_IsNeutral", 1);
        }
        else if (_karmicPoints < 0)
        {
            foreach (var kv in _karmicVfxs)
            {
                kv.SendEvent("Play");
                kv.SetBool("IsEvil", true);
            }
            foreach (var mat in _playerMats)
            {
                mat.SetFloat("_IsEvil", 1);
                mat.SetFloat("_IsNeutral", 0);
            }
        }
        return _karmicPoints;
    }
    public int DecreaseKarmicPoints(int decreaseKarmicPoints)
    {
        _karmicPoints -= decreaseKarmicPoints;
        tmp.text = "Karmic Points: " + _karmicPoints;
        Debug.Log("se restaron puntos al sistema karmico");
        //Gian was here
        if (_karmicPoints > 0)
        {
            foreach (var kv in _karmicVfxs)
            {
                kv.SendEvent("Play");
                kv.SetBool("IsEvil", false);
            }
            foreach (var mat in _playerMats)
            {
                mat.SetFloat("_IsEvil", 0);
                mat.SetFloat("_IsNeutral", 0);
            }
        }
        else if (_karmicPoints == 0)
        {
            foreach (var kv in _karmicVfxs) kv.SendEvent("Stop");
            foreach (var mat in _playerMats) mat.SetFloat("_IsNeutral", 1);
        }
        else if (_karmicPoints < 0)
        {
            foreach (var kv in _karmicVfxs)
            {
                kv.SendEvent("Play");
                kv.SetBool("IsEvil", true);
            }
            foreach (var mat in _playerMats)
            {
                mat.SetFloat("_IsEvil", 1);
                mat.SetFloat("_IsNeutral", 0);
            }
        }
        return _karmicPoints;
    }
    #endregion
}
