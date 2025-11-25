using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;


public class SceneGula : MonoBehaviour
{
    [SerializeField] private PlayableDirector director;
    public GameObject vorcarbis;
    public GameObject gula;
    [SerializeField] GameObject cameraFake, faithFake;

    private void Awake()
    {
            director.Play();

    }

    public void SceneVorGula()
    {
        ServiceLocator.Instance.GetDependency<CheloCamera>().isResting = true;
        ServiceLocator.Instance.GetDependency<ControllerPlayer>().isResting = true;
        ServiceLocator.Instance.GetDependency<PlayerMVC>().gameObject.SetActive(false);
        //ServiceLocator.Instance.GetDependency<VorcarbisEFSM>().isResting = true;

        CanvasGroup cgPlayer = ServiceLocator.Instance.GetDependency<ManaUI>().gameObject.GetComponent<CanvasGroup>();
        UtilitiesAgus.ToggleCanvasGroup(cgPlayer, false);
    }


}
