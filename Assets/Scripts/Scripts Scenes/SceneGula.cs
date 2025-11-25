using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;


public class SceneGula : MonoBehaviour
{
    [SerializeField] private PlayableDirector director;
    //public GameObject vorcarbis;
    public GameObject gula;
    [SerializeField] GameObject cameraFake, faithFake;
    [SerializeField] PlayerMVC playerMVC;
    [SerializeField] CheloCamera cheloCamera;

    private void Start()
    {
        //yield return new WaitForSeconds(1f);

        //if(ServiceLocator.Instance.TryGetDependency<PlayerMVC>(out PlayerMVC player))
        //{
        //    playerMVC = player;
        //    playerMVC.gameObject.SetActive(false);
        //    ServiceLocator.Instance.GetDependency<CheloCamera>().gameObject.SetActive(false);
        //}

        
    }

    bool hasAlreadyPassed = false;
    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<PlayerMVC>() && !hasAlreadyPassed)
        {
            hasAlreadyPassed = true;
            playerMVC = other.GetComponent<PlayerMVC>();

            other.gameObject.SetActive(false);
            cheloCamera = ServiceLocator.Instance.GetDependency<CheloCamera>();

            cheloCamera.gameObject.SetActive(false);

            cameraFake.SetActive(true);
            faithFake.SetActive(true);

            director.Play();
        }
    }

    public void SceneVorGula()
    {
        playerMVC.gameObject.SetActive(true);
        cheloCamera.gameObject.SetActive(true);

        cheloCamera.isResting = false;
        ServiceLocator.Instance.GetDependency<ControllerPlayer>().isResting = false;

        gula.GetComponent<Beelzebub>().isResting = false;

        cameraFake.SetActive(false);
        faithFake.SetActive(false);

        gula.GetComponent<BossLife>().ShowLifeBar();

        CanvasGroup cgPlayer = ServiceLocator.Instance.GetDependency<ManaUI>().gameObject.GetComponent<CanvasGroup>();
        UtilitiesAgus.ToggleCanvasGroup(cgPlayer, true);

        StartCoroutine(TurnOffDirector());
    }

    IEnumerator TurnOffDirector()
    {
        yield return new WaitForSeconds(2f);
        director.gameObject.SetActive(false);
    }
}
