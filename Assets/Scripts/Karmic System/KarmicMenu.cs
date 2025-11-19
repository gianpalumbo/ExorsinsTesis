using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class KarmicMenu : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private PlayerMVC _playerMVC;
    //AGUS ADD ON temporal
    //[SerializeField] AttackManager attackManager;
    [SerializeField] private Canvas _canvas;
    [SerializeField] private Button _addPointsButton;
    [SerializeField] private Button _decreasePointsButton;
    [SerializeField] private Button _acquireSoulButton;
    [SerializeField] private BoxCollider _myBoxCollider;

    [Header("Configuracion")] [Tooltip("Cantidad de puntos karmicos a añadir")]
    [SerializeField] private int _pointsToAdd = 1;
    [SerializeField] private int _pointsToDecrease = 1;

    //public GameObject _currentEnemy;
    //CHELO WAS HERE
    [SerializeField] public GameObject _currentEnemy;
    [SerializeField] public GameObject _currentAlly;

    //AGUS ADDON
    public bool hasSelected;

    //CHELO WAS HERE: ID CURSOR MANAGER
    private readonly string id = "KarmicMenu";



    private void Awake()
    {
        if (_canvas == null) Debug.LogError("NO ASIGNASTE CANVAS");
        //gameObject.SetActive(false);

        ServiceLocator.Instance.RegisterDependency<KarmicMenu>(this);
        gameObject.SetActive(false);
        //CHELO WAS HERE
        //el canvas no hace awake hasta que lo prende el enemigo, osea que el enemigo lo pone true, entra en awake y lo cambia a false, por eso lo apago
        //Debug.Log("KarmicMenu Awake: activeSelf=" + _canvas.gameObject.activeSelf);
        //_canvas.gameObject.SetActive(false);
        //Debug.Log("KarmicMenu Awake después: activeSelf=" + _canvas.gameObject.activeSelf);

        //_addPointsButton.onClick.AddListener(OnAddPointsClicked);
        //_decreasePointsButton.onClick.AddListener(OnDecresePointsClicked);
        //_acquireSoulButton.onClick.AddListener(OnAcquireSoulClicked);
    }
    private void OnDestroy()
    {
        ServiceLocator.Instance.RemoveDependency<KarmicMenu>();
    }
    private void OnEnable()
    {
        //Cursor.lockState = CursorLockMode.None;
        //Cursor.visible = true;

        hasSelected = false;
        //attackManager.canAttackAtAll = true;

        //CHELO WAS HERE: AGREGUE ID AL CURSOR MANAGER
        CursorUIManager.Instance.RequestCursorState(true, id);
    }

    //CHELO WAS HERE hardcodee codigo
    //private void OnTriggerEnter(Collider other)
    //{
    //    if (other.CompareTag("Player"))
    //    {
    //        _canvas.gameObject.SetActive(true);
    //        Cursor.lockState = CursorLockMode.None;
    //        Cursor.visible = true;
    //    }
    //}

    public void OpenMenu()
    {
        _canvas.gameObject.SetActive(true);
        //Cursor.lockState = CursorLockMode.None;
        //Cursor.visible = true;

        //ADDON PRENDO MOUSE CUANDO ABRO CANVAS
        UtilitiesAgus.ToggleCursor(true);

        //_playerMVC.SetResting(true);
        //attackManager.canAttackAtAll = false;
        //ReferenceManager.Instance.controller.canAttackAtAll = false;
        if (ServiceLocator.Instance.TryGetDependency<ControllerPlayer>(out var controller))
            controller.canAttackAtAll = false;
    }

    public void OnAddPointsClicked()
    {
        // Aumenta los puntos kármicos en PlayerMVC, PUNTOS DE CIELO
        if (_playerMVC != null)
        {
            _playerMVC.AddKarmicPoints(_pointsToAdd);
        }
        //RemoveFromList();

        if (ServiceLocator.Instance.TryGetDependency<KarmicToggle>(out var karmic))
            karmic.TurnThisOff();

        CloseMenu();
    }

    public void OnDecresePointsClicked()
    {
        // Decrese los puntos kármicos en PlayerMVC, PUNTOS DE INFIERNO
        if (_playerMVC != null)
        {
            _playerMVC.DecreaseKarmicPoints(_pointsToDecrease);
            
        
            
            //CHELO WAS HERE, despues le paso por parametro la cantidad de almas que le va a dar el enemigo
            PointsManager.Instance.AddPoints(1000);
        
        
        
        }

        if (ServiceLocator.Instance.TryGetDependency<KarmicToggle>(out var karmic))
            karmic.TurnThisOff();

        //RemoveFromList();
        CloseMenu();
    }

    public void OnAcquireSoulClicked()
    {
        // Añade el enemigo a la lista de almas y lo desactiva
        if (_playerMVC != null && _currentAlly != null) _playerMVC.AcquireSoul(_currentAlly);
        RemoveFromList();

        if (ServiceLocator.Instance.TryGetDependency<KarmicToggle>(out var karmic))
            karmic.TurnThisOff();

        CloseMenu();
    }

    //private void OnCancelClicked()
    //{
    //    // Simplemente cierra el menu sin sumar puntos
    //    CloseMenu();
    //}

    //private void Condenacion()
    //{

    //}

    public void CloseMenu()
    {
        hasSelected = true;

        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;

        _canvas.gameObject.SetActive(false);
        //_playerMVC.SetResting(false);
        //attackManager.canAttackAtAll = true;

        //CHELO WAS HERE: ELIMINO ID DEL CURSOR MANAGER
        CursorUIManager.Instance.ReleaseCursorRequest(id);

        if (ServiceLocator.Instance.TryGetDependency<ControllerPlayer>(out var controller))
            controller.canAttackAtAll = true;

        //ADDON PRENDO MOUSE CUANDO ABRO CANVAS
        UtilitiesAgus.ToggleCursor(false);

        //if (ServiceLocator.Instance.TryGetDependency<KarmicToggle>(out var karmic))
        //    karmic.TurnThisOff(); NO LO APAGO PORQUE ME GENERA QUE NO ME APAREZCA EL MENU KARMICO
    }

    //ESTO NO DEBERIA SER ASI, SINO QUE CUANDO EL JUGADOR ESTE EN LA FOGATA SE ABRA EL MENU Y NO PUEDA MOVERSE HASTA DARLE A LA OPCION, PERO POR AHORA SIRVE
    private void OnTriggerExit(Collider other)
    {
        //CloseMenu();
    }


    //ANDA BIEN PERO TENGO QUE PASAR AL ENEMIGO QUE QUIERO REINICIAR AL KARMICMENU, SINO SOLO ESTOY AGARRANDO AL PREFAB
    private void RemoveFromList()
    {
        var thisEnemy = _currentEnemy.GetComponent<Entity>();
        if (thisEnemy != null)
        {
            Debug.Log($"mande un enemigo a removerse {thisEnemy.name}");
            ResetTrigger.RemoveEnemy(thisEnemy);
        }
    }


    //public void OnEnable()
    //{
    //    _myBoxCollider.gameObject.SetActive(true);
    //}
    //public void OnDisable()
    //{
    //    _myBoxCollider.gameObject.SetActive(false);
    //}
}
