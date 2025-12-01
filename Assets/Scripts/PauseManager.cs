using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    public static PauseManager instance;
    public bool isPaused = false, isInAnotherPanel = false, canPause = true;
    public GameObject panel;
    public Action ArtificialUpdate;
    public Action ArtificialLate;

    public Button volumeButton;
    public GameObject audioPanel;

    public event Action TotalUpdates = delegate { };
    public event Action TotalLateUpdates = delegate { }; // Agregado para LateUpdate

    //private string sceneMenu = "Menu";

    CheloCamera cheloCam;

    //CHELO WAS HERE: ID CURSOR MANAGER
    private readonly string id = "PauseManager";

    #region ONRESETSCENES
    public event Action OnResetScene;
    #endregion

    private void Awake()
    {
        ArtificialUpdate = TotalUpdates;
        ArtificialLate = TotalLateUpdates; // Inicializar con eventos vacíos

        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;

        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogWarning("Más de una instancia de PauseManager detectada. Eliminando esta instancia.");
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        audioPanel = GameObject.FindGameObjectWithTag("AudioPanel");

        volumeButton.onClick.RemoveAllListeners();
        volumeButton.onClick.AddListener(() =>
        {
            if (audioPanel != null)
            {
                CanvasGroup cg = audioPanel.GetComponent<CanvasGroup>();
                cg.alpha = 255f;
                cg.interactable = true;
                cg.blocksRaycasts = true;

                isInAnotherPanel = true;
            }
        });
    }
    void Start()
    {
        cheloCam = ServiceLocator.Instance.GetDependency<CheloCamera>();
        //Debug.Log(cheloCam + "cheloCam");
    }
    public void Subscribe(Action callback, bool isLateUpdate = false)
    {
        if (isLateUpdate)
        {
            TotalLateUpdates += callback;
            ArtificialLate = TotalLateUpdates;
        }
        else
        {
            TotalUpdates += callback;
            ArtificialUpdate = TotalUpdates;
        }
    }

    public void Unsubscribe(Action callback, bool isLateUpdate = false)
    {
        if (isLateUpdate)
        {
            TotalLateUpdates -= callback;
            ArtificialLate = TotalLateUpdates;
        }
        else
        {
            TotalUpdates -= callback;
            ArtificialUpdate = TotalUpdates;
        }
    }

    private void Update()
    {
        if (isInAnotherPanel) return;

        if (Input.GetKeyDown(KeyCode.Escape) && canPause)
        {
            isPaused = !isPaused;
            Pause(isPaused);
        }

        ArtificialUpdate();
    }

    //private void LateUpdate()
    //{
    //    ArtificialLate();
    //}

    public void ChangePaused(bool paused)
    {
        isPaused = paused;
    }

    public void ResetScene()
    {
        //Pause(false);
        //Cursor.lockState = CursorLockMode.None;
        //Cursor.visible = true;



        //CHELO WAS HERE: ELIMINO ID DEL CURSOR MANAGER
        CursorUIManager.Instance.ReleaseCursorRequest(id);


        Pause(false); //DESPAUSO PARA REINICIAR

        OnResetScene?.Invoke();

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void IsInVolumePanel() => isInAnotherPanel = true;
    public void IsNotInVolumePanel() => isInAnotherPanel = false;

    //CHELO WAS HERE
    public void MenuScene()
    {
        Pause(false);



        //CHELO WAS HERE: ELIMINO ID DEL CURSOR MANAGER
        CursorUIManager.Instance.ReleaseCursorRequest(id);

        if(SoundManager.Instance != null)
        {
            SoundManager.Instance.ChangeToMenuMusic();
            SoundManager.Instance.UnMuteMusic();
        }

        SceneManager.LoadScene(0);
    }



    public void Pause(bool active)
    {
        if (isInAnotherPanel) return;

        if (active)
        {
            panel.SetActive(active);
            //Debug.Log("JUEGO PAUSADO");
            Time.timeScale = 0f;
            //Cursor.lockState = CursorLockMode.None;
            //Cursor.visible = true;

            //ReferenceManager.Instance.cheloCamera.isResting = true;
            cheloCam.isResting = true;
            //Debug.Log($"CheloCamera es: {cheloCam}");
            //CHELO WAS HERE: AGREGUE ID AL CURSOR MANAGER
            CursorUIManager.Instance.RequestCursorState(true, id);

            UtilitiesAgus.ToggleCursor(active);

            ArtificialUpdate = delegate { };
            ArtificialLate = delegate { }; // Asegurar que LateUpdate también se detiene
        }
        else
        {
            Time.timeScale = 1f;
            panel.SetActive(active);
            //isPaused = false;
            //Cursor.lockState = CursorLockMode.Locked;

            //ReferenceManager.Instance.cheloCamera.isResting = false;
            cheloCam.isResting = false;

            //CHELO WAS HERE: ELIMINO ID DEL CURSOR MANAGER
            CursorUIManager.Instance.ReleaseCursorRequest(id);

            UtilitiesAgus.ToggleCursor(active);

            ArtificialUpdate = TotalUpdates;
            ArtificialLate = TotalLateUpdates;
        }
    }


}
