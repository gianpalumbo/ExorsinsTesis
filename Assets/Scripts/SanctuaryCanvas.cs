using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SanctuaryCanvas : MonoBehaviour
{
    public TextMeshProUGUI pointsSanctuary;

    public GameObject sanctuaryCanvas; // Panel con botones Hablar y Descansar
    public GameObject InteractuableCanvas; //Tecla E
    public Button dialogueButton;
    public Button closeButton;
    public Button restButton;

    //public PlayerMVC player;
    //public PlayerLife playerLife;
       
    private readonly string id = "SanctuaryCanvas"; //CHELO WAS HERE: ID CURSOR MANAGER

    //CHELO WAS HERE: LERP MENU
    [SerializeField] private CanvasGroup _cgSanctuaryCanvas, _cginteractuable; //canvas del santuario
    [SerializeField] private float _fadeCanvasDuration = 0.5f; //duracion fade canvas del santuario
    //[SerializeField] private float _fadeSanctuaryCanvasGroupDuration = 0.25f; //duracion fade canvas del santuario
    [SerializeField] private float _fadeCGInterfaceDuration = 0.15f; //duracion fade canvas de los puntos y la interfaz del player

    //CHELO WAS HERE: esconder canvas cuando entra al santuario y prenderlo al salir
    [SerializeField] private GameObject _playerCanvas; //canvas de la interfaz del player
    [SerializeField] private CanvasGroup _cgplayerCanvas;
    [SerializeField] private GameObject _pointsCanvas; //canvas de los puntos
    [SerializeField] private CanvasGroup _cgpointsCanvas;

    public Sanctuary actualSanctuary;
    
    private void OnEnable()
    {
        _cgSanctuaryCanvas.alpha = 0f;
        //dialogueButton.onClick.AddListener(OnDialogueButtonPressed);
        //closeButton.onClick.AddListener(OnCloseButtonPressed);
        //restButton.onClick.AddListener(OnRestButtonPressed);
        _playerCanvas = ServiceLocator.Instance.GetDependency<ManaUI>().gameObject;
        _cgplayerCanvas = ServiceLocator.Instance.GetDependency<ManaUI>().GetComponent<CanvasGroup>();

        _pointsCanvas = ServiceLocator.Instance.GetDependency<PointsTMP>().gameObject;
        _cgpointsCanvas = ServiceLocator.Instance.GetDependency<PointsTMP>().GetComponent<CanvasGroup>();

        TurnInteractuable(false);
        //sanctuaryCanvas.SetActive(false);
        //TurnOffCGs(_cgSanctuaryCanvas);
    }

    public void TurnSanctuaryCGs(bool toggle)
    {
        if (toggle)
        {
            //TurnOnCGs(_cginteractuable);
            TurnOnCGs(_cgSanctuaryCanvas);
        }
        else
        {
            //TurnOffCGs(_cginteractuable);
            TurnOffCGs(_cgSanctuaryCanvas);
        }
    }

    public void TurnInteractuable(bool toggle)
    {
        TurnOnCGs(_cginteractuable);
        InteractuableCanvas.SetActive(toggle);
    }

    public void TurnOffCGs(CanvasGroup cg)
    {
        cg.alpha = 0;
        cg.interactable = false;
        cg.blocksRaycasts = false;
    }

    public void TurnOnCGs(CanvasGroup cg)
    {
        cg.alpha = 1;
        cg.interactable = true;
        cg.blocksRaycasts = true;
    }

    /// <summary>
    /// Llamado cuando se presiona el boton Hablar
    /// Esconde el panel de opciones y lanza el dialogo
    /// </summary>
    public void OnDialogueButtonPressed()
    {
        TurnSanctuaryCGs(false);

        //ADD-ON AGUS PARA QUE NO SE PAUSE MIENTRAS SE HABLA
        ServiceLocator.Instance.GetDependency<PlayerMVC>().isResting = false;
        //ServiceLocator.Instance.GetDependency<CheloCamera>().isResting = false;
        ServiceLocator.Instance.GetDependency<CheloCamera>().isResting = true;
        ServiceLocator.Instance.GetDependency<ControllerPlayer>().isResting = true;
        ServiceLocator.Instance.GetDependency<PlayerMVC>().isResting = true;

        PauseManager.instance.canPause = false;
        //Cursor.lockState = CursorLockMode.Locked; // Bloquear el cursor al iniciar el dialogo
        //Cursor.visible = false; // Hacer invisible el cursor al iniciar el dialogo
        UtilitiesAgus.ToggleCursor(false);

        //PauseManager.instance.isInAnotherPanel = false;
        // Suscribir el callback a fin de dialogo
        DialogueSystem ds = DialogueManager.Instance.dialogueSystem;
        ds.OnDialogueEnd += ReopenSanctuaryMenu;

        DialogueManager.Instance.TryPlayDialogue(); // Pedir al manager que reproduzca el di�logo
    }

    /// <summary>
    /// Llamado cuando se presiona el bot�n �Descansar�.
    /// Resetea enemigos y mantiene el panel abierto.
    /// </summary>
    public void OnRestButtonPressed()
    {
        ServiceLocator.Instance.GetDependency<PlayerLife>().TakeAllHeal();
        ResetTrigger.ResetAllEnemies();
        Debug.Log("SANCTUARYCANVAS: enemigos reseteados");

        CloseSanctuaryMenu();
    }

    public void OnCloseButtonPressed()
    {
        CloseSanctuaryMenu();
        //animacion del personaje dejar de descansar y prender el canvas del pj: tambien podria ponerlo en un lugar fijo asi no causa problemas al acercar la camara cuando dialoga con el demonio
    }

    public void OpenSanctuaryMenu()
    {
        pointsSanctuary.text = PointsManager.Instance.currentPoints.ToString();

        //actualSanctuary.onSantuary = true;
        ServiceLocator.Instance.GetDependency<PlayerMVC>().SetResting(true); //CHELO WAS HERE: bloqueo inputs
        ServiceLocator.Instance.GetDependency<PlayerMVC>().FreezeAllRB();
        ServiceLocator.Instance.GetDependency<ControllerPlayer>().isResting = true;
        ServiceLocator.Instance.GetDependency<CheloCamera>().isResting = true;
        ServiceLocator.Instance.GetDependency<DashPlayer>().ResetRollVariables();

        PauseManager.instance.canPause = false;
        //PauseManager.instance.isInAnotherPanel = true;
        SoundManager.Instance.isResting = true; // Activar el estado de descanso en el SoundManager

        TurnOffCGs(_cgplayerCanvas);
        TurnOffCGs(_cgpointsCanvas);
        
        //CHELO WAS HERE: cambie los IEnumerator por algo mas generico
        //sanctuaryCanvas.SetActive(true);
        StartCoroutine(FadeInCanvas(sanctuaryCanvas, _cgSanctuaryCanvas, 0f, 1f, _fadeCGInterfaceDuration));
        StartCoroutine(FadeOutCanvas(_playerCanvas, _cgplayerCanvas, 1f, 0f, _fadeCGInterfaceDuration));
        StartCoroutine(FadeOutCanvas(_pointsCanvas, _cgpointsCanvas, 1f, 0f, _fadeCGInterfaceDuration));

        //Cursor.lockState = CursorLockMode.None;
        //Cursor.visible = true;


        TurnOnCGs(_cgSanctuaryCanvas);
        TurnInteractuable(false);
        
        CursorUIManager.Instance.RequestCursorState(true, id); //CHELO WAS HERE: AGREGUE ID AL CURSOR MANAGER

        //ADDON PRENDO MOUSE CUANDO ABRO CANVAS
        UtilitiesAgus.ToggleCursor(true);

        //ServiceLocator.Instance.TryGetDependency<StatButton>(out StatButton stat);
        //ACA TAMBIEN TIRABA NULL POR LA PROTECCION
        //stat.RefreshUISafely();
        Debug.Log("OPENDORPLI");
    }

    public void CloseSanctuaryMenu()
    {
        InteractuableCanvas.SetActive(true);

        if (actualSanctuary != null) actualSanctuary.MoveAgainCamera();
        //else Debug.Log("no hay actualSanctuary en SanctuaryCanvas");

        //CHELO WAS HERE: cambie los IEnumerator por algo mas generico
        //StartCoroutine(FadeOutCanvas(1f, 0f));
        StartCoroutine(FadeOutCanvas(sanctuaryCanvas, _cgSanctuaryCanvas, 1f, 0f, _fadeCGInterfaceDuration));
        
        StartCoroutine(FadeInCanvas(_playerCanvas, _cgplayerCanvas, 0f, 1f, _fadeCGInterfaceDuration));
        
        StartCoroutine(FadeInCanvas(_pointsCanvas, _cgpointsCanvas, 0f, 1f, _fadeCGInterfaceDuration));

        CursorUIManager.Instance.ReleaseCursorRequest(id); //CHELO WAS HERE: ELIMINO ID DEL CURSOR MANAGER

        PauseManager.instance.canPause = true;
        SoundManager.Instance.isResting = false; // Desactivar el estado de descanso en el SoundManager        

        ServiceLocator.Instance.GetDependency<PlayerMVC>().SetResting(false);
        ServiceLocator.Instance.GetDependency<PlayerMVC>().FreezeRotRB();
        ServiceLocator.Instance.GetDependency<ControllerPlayer>().isResting = false;

        PauseManager.instance.isInAnotherPanel = false;
        //ServiceLocator.Instance.GetDependency<CheloCamera>().isResting = false;

        TurnOnCGs(_cgplayerCanvas);
        TurnOnCGs(_cgpointsCanvas);

        Debug.Log("CLOSEBUTTONAPRETADO");

        actualSanctuary = null;
    }

    public void ReopenSanctuaryMenu()
    {
        OpenSanctuaryMenu();
        DialogueManager.Instance.dialogueSystem.OnDialogueEnd -= ReopenSanctuaryMenu; // Desuscribirse para no volver a llamar multiples veces
    }
    
    IEnumerator FadeInCanvas(GameObject C, CanvasGroup cg, float startDuration, float endDuration, float fadeduration)
    {
        float elapsedTime = 0f;
        cg.alpha = startDuration;
        //while (elapsedTime < _fadeCanvasDuration)
        while (elapsedTime < fadeduration)
        {
            cg.alpha = Mathf.Lerp(startDuration, endDuration, elapsedTime / fadeduration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        cg.alpha = endDuration;
    }
    IEnumerator FadeOutCanvas(GameObject C, CanvasGroup cg, float startDuration, float endDuration, float fadeduration)
    {
        float elapsedTime = 0f;
        cg.alpha = startDuration;
        //while (elapsedTime < _fadeCanvasDuration)
        while (elapsedTime < fadeduration)
        {
            cg.alpha = Mathf.Lerp(startDuration, endDuration, elapsedTime / fadeduration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        cg.alpha = endDuration;
        //sanctuaryCanvas.SetActive(false);
        //C.SetActive(false);
    }
}
