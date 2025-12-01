using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

public class TriggerToCastle : MonoBehaviour
{
    [SerializeField] CanvasGroup interactuableCG;
    public bool isOnTrigger, hasEntered;

    PlayerMVC player;

    private void Awake()
    {
        ServiceLocator.Instance.RegisterDependency<TriggerToCastle>(this);
    }
    private void OnDisable()
    {
        ServiceLocator.Instance.RemoveDependency<TriggerToCastle>();
    }
    void Start()
    {
        UtilitiesAgus.ToggleCanvasGroup(interactuableCG, false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (player == null)
            player = other.GetComponent<PlayerMVC>();

        UtilitiesAgus.ToggleCanvasGroup(interactuableCG, true);
        isOnTrigger = true;
    }
    private void OnTriggerExit(Collider other)
    {
        UtilitiesAgus.ToggleCanvasGroup(interactuableCG, false);
        isOnTrigger = false;
    }

    void Update()
    {
        if (isOnTrigger && !hasEntered && Input.GetKeyDown(KeyCode.E))
        {
            hasEntered = true;
            ServiceLocator.Instance.GetDependency<PlayerMVC>().FreezeAllRB();
            StartCoroutine(DoFadeAndLoadScene());
        }
    }

    //IEnumerator DoFadeAndLoadScene()
    //{
    //    if (player == null) yield return null;

    //    ServiceLocator.Instance.GetDependency<BlackPanelFade>().FadeNoCoroutine(1f, .75f, false, false); //FADE IN
    //    yield return new WaitForSeconds(1f); //ESPERA 1 SEG
    //    player.startingScene = PlayerMVC.StartingScene.Outside; //SETTEA OUTSIDE COMO ESCENA PARA DESCARGAR
    //    player.LoadScene(PlayerMVC.StartingScene.Castle); //CARGA ADITIVAMENTE ESCENA CASTLE

    //    string sceneToCheck = PlayerMVC.GetSceneName(PlayerMVC.StartingScene.Castle);

    //    yield return new WaitUntil(() => SceneManager.GetSceneByName(sceneToCheck).isLoaded);
    //    yield return new WaitForSeconds(1f);
    //    ServiceLocator.Instance.GetDependency<BlackPanelFade>().FadeNoCoroutine(0f, 5f, false, false);
    //}
    IEnumerator DoFadeAndLoadScene()
    {
        if (player == null) yield break;

        ServiceLocator.Instance.GetDependency<BlackPanelFade>().FadeNoCoroutine(1f, .75f, false, false);
        yield return new WaitForSeconds(1f);

        player.startingScene = PlayerMVC.StartingScene.Outside;
        player.LoadScene(PlayerMVC.StartingScene.Castle);

        string sceneToCheck = PlayerMVC.GetSceneName(PlayerMVC.StartingScene.Castle);

        bool sceneLoaded = false;
        SceneManager.sceneLoaded += (Scene scene, LoadSceneMode mode) =>
        {
            if (scene.name == sceneToCheck) sceneLoaded = true;
        };

        yield return new WaitUntil(() => sceneLoaded);

        ServiceLocator.Instance.GetDependency<BlackPanelFade>().FadeNoCoroutine(0f, 5f, false, false);
    }

}
