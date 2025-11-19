using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using UnityEngine.VFX;

public class Sanctuary : MonoBehaviour
{
    [SerializeField] private SanctuaryCanvas mySanctuaryCanvas;
    [SerializeField] private PlayerMVC player;

    [Header("Light")]
    [SerializeField] private Light santuatyLight;
    [SerializeField] private Color offColor;
    [SerializeField] private Color onColor;

    [Header("SanctuaryBools")]
    [SerializeField] private bool playerInRange = false;
    [SerializeField] private bool onSantuary; //chequea si esta en el santuario asi no entra constantemente si toca E en el sanctuary
    [SerializeField] private GameObject spawnPoint; //spawnpoint a donde va el jugador si se muere

    [Header("Lerps First Enter")] //CHELO WAS HERE: implementacion cuando entra por primera vez
    [SerializeField] private int firstTime = 0;
    [SerializeField] private Canvas firstCanvas; //CANVAS DEL TEXTO
    //[SerializeField] private CanvasGroup firstCanvasGroup;
    [SerializeField] private TextMeshProUGUI firstText;
    //[SerializeField] private Material firstScreenShader;
    [SerializeField] private float fadeTextDuration = 2f; //duracion fade texto

    [SerializeField] VisualEffect sanctuaryVfx;    
    
    [Header("Camera Position")] //CHELO WAS HERE: implementacion de camara hacia el santuario
    [SerializeField] private Transform sanctuaryCameraPos;
    [SerializeField] private float duration = 0.2f;
    private Vector3 originalCamPosition;
    private Quaternion originalCamRotation;
    
    private void Awake()
    {
        if (player == null)
        {
            //Debug.LogError("La referencia a PlayerMVC está vacía en Sanctuary!");
            return;
        }
        firstText.alpha = 0;
        firstCanvas.gameObject.SetActive(false);
    }
    private void Start()
    {
        player = ServiceLocator.Instance.GetDependency<PlayerMVC>();
    }
    void Update()
    {        
        if (playerInRange) //CON MYSANCTUARYCANVAS
            if (!DialogueManager.Instance.dialogueSystem.IsDialoguing)
                if (Input.GetKeyDown(KeyCode.E) && onSantuary == false)
                {
                    onSantuary = true;
                    mySanctuaryCanvas.actualSanctuary = this;
                    //playerCameraPos = Camera.main.transform;
                    //////CHELOS WAS HERE: AGREGADO FUTURO, un bool, si es la primera vez que entra larga una Screen Shadergraph y despues abre el menu.                    
                    originalCamPosition = Camera.main.transform.position; //Guardo la pos y rot actual de la camara
                    originalCamRotation = Camera.main.transform.rotation;
                    //por si acaso hay otra corrutina rompiendo las bolas
                    //StopAllCoroutines();
                    //Inicio transicion de santuario 
                    StartCoroutine(MoveAndRotate(Camera.main.transform.position, Camera.main.transform.rotation, sanctuaryCameraPos.position, sanctuaryCameraPos.rotation, true));
                    if (firstTime == 0)
                    {
                        ServiceLocator.Instance.GetDependency<PlayerMVC>().SetResting(true);
                        ServiceLocator.Instance.GetDependency<PlayerMVC>().FreezeAllRB();
                        ServiceLocator.Instance.GetDependency<ControllerPlayer>().isResting = true;
                        ServiceLocator.Instance.GetDependency<CheloCamera>().isResting = true;
                        ServiceLocator.Instance.GetDependency<DashPlayer>().ResetRollVariables();
                        PauseManager.instance.isInAnotherPanel = true;

                        //ADDON PRENDO MOUSE CUANDO ABRO CANVAS
                        UtilitiesAgus.ToggleCursor(true);

                        DialogueManager.Instance.UnlockNextBlock(); // Desbloquea un nuevo dialogo en el manager

                        FirstTimeIn();                        
                        firstTime = 1;
;                   }
                    else mySanctuaryCanvas.OpenSanctuaryMenu();
               }
        //if (!playerInRange) return;
        //if (!DialogueManager.Instance.dialogueSystem.IsDialoguing && Input.GetKeyDown(KeyCode.X)) DialogueManager.Instance.TryPlayDialogue();
        //if (Input.GetKeyDown(KeyCode.R)) ResetTrigger.ResetAllEnemies();
    }

    private void FirstTimeIn()
    {
        PauseManager.instance.isInAnotherPanel = true;

        firstCanvas.gameObject.SetActive(true);
        StartCoroutine(FadeColor());
        StartCoroutine(Fadetext());
        //mySanctuaryCanvas.OpenSanctuaryMenu();
        sanctuaryVfx.SetBool("IsPurified", true);
    }

    IEnumerator FadeColor() //CHELO WAS HERE: COLOR DE LA LUZ DEL SANTUARIO
    {
        float elapsedTime = 0f;
        while (elapsedTime < fadeTextDuration)
        {
            santuatyLight.color = Color.Lerp(offColor, onColor, elapsedTime / fadeTextDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        santuatyLight.color = onColor;
        yield return null;
    }
    IEnumerator Fadetext()
    {
        float elapsedTime = 0f;
        firstText.alpha = 0f;

        while (elapsedTime < fadeTextDuration+1)
        {
            firstText.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeTextDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        yield return new WaitForSeconds(2f);
        firstText.alpha = 1f;
        elapsedTime = 0f;
        while (elapsedTime < fadeTextDuration)
        {
            firstText.alpha = Mathf.Lerp(1f, 0, elapsedTime / fadeTextDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        //StartCoroutine(MoveAndRotate(Camera.main.transform.position, Camera.main.transform.rotation, sanctuaryCameraPos.position, sanctuaryCameraPos.rotation, true));
        firstText.alpha = 0f;
        firstCanvas.gameObject.SetActive(false);
        mySanctuaryCanvas.OpenSanctuaryMenu();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        
        playerInRange = true;
        mySanctuaryCanvas.TurnInteractuable(true);
        

        //CHELO WAS HERE: LE MANDO LA POSICION DE SU HIJO AL PLAYER PARA RESPAWNEAR AHI
        //other.GetComponent<PlayerLife>().LastSpawnPoint(spawnPoint.transform.position);
        //CHELO WAS HERE: LO CAMBIE PARA QUE LO HAGA EL DEATHMANAGER
        DeathManager dm = FindObjectOfType<DeathManager>();
        if (dm != null) dm.LastSpawnPoint(spawnPoint.transform.position);        
    }
    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = false;
        mySanctuaryCanvas.TurnSanctuaryCGs(false);

        mySanctuaryCanvas.TurnInteractuable(false);

        //mySanctuaryCanvas.InteractuableCanvas.SetActive(false);
        //if (DialogueManager.Instance.dialogueSystem.IsDialoguing()) DialogueManager.Instance.dialogueSystem.EndDialogue();
    }
    
    private IEnumerator MoveAndRotate(Vector3 startPos, Quaternion startRot, Vector3 endPos, Quaternion endRot, bool cameraActive)
    {
        float elapsed = 0f;
        Debug.Log($"elased es" + elapsed);
        ServiceLocator.Instance.GetDependency<CheloCamera>().isResting = cameraActive;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            Camera.main.transform.position = Vector3.Lerp(startPos, endPos, t);
            Camera.main.transform.rotation = Quaternion.Slerp(startRot, endRot, t);
            yield return null;
        }
        Camera.main.transform.position = endPos;
        Camera.main.transform.rotation = endRot;
    }

    public void MoveAgainCamera()
    {
        //por si acaso hay otra corrutina rompiendo las bolas
        //StopAllCoroutines();
        //Debug.Log($"estoy llamando MoveAgainCamera");
        StartCoroutine(MoveAndRotateAgain(Camera.main.transform.position, Camera.main.transform.rotation, originalCamPosition, originalCamRotation, false));
    }

    private IEnumerator MoveAndRotateAgain(Vector3 startPos, Quaternion startRot, Vector3 endPos, Quaternion endRot, bool cameraActive)
    {
        ServiceLocator.Instance.GetDependency<CheloCamera>().isResting = !cameraActive;
        float elapsed = 0f;
        Debug.Log($"elased es" + elapsed);
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            Camera.main.transform.position = Vector3.Lerp(startPos, endPos, t);
            Camera.main.transform.rotation = Quaternion.Slerp(startRot, endRot, t);
            yield return null;
        }
        Camera.main.transform.position = endPos;
        Camera.main.transform.rotation = endRot;
        ServiceLocator.Instance.GetDependency<CheloCamera>().isResting = cameraActive;
        onSantuary = false;
    }
}
