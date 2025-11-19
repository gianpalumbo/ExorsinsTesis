using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;


public class SceneVorcarbis : MonoBehaviour
{
    [SerializeField] private PlayableDirector director;
    public GameObject vorcarbis;
    private bool triggered = false; //evito que haga varias veces la cinematica

    [SerializeField] GameObject cameraFake, faithFake;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && triggered == false)
        {
            triggered = true;
            director.Play();
            SceneVorcarbisOn();
        }
    }

    public void SceneVorcarbisOn()
    {
        ServiceLocator.Instance.GetDependency<CheloCamera>().isResting = true;
        ServiceLocator.Instance.GetDependency<ControllerPlayer>().isResting = true;
        ServiceLocator.Instance.GetDependency<PlayerMVC>().gameObject.SetActive(false);
        ServiceLocator.Instance.GetDependency<VorcarbisEFSM>().isResting = true;

        CanvasGroup cgPlayer = ServiceLocator.Instance.GetDependency<ManaUI>().gameObject.GetComponent<CanvasGroup>();
        UtilitiesAgus.ToggleCanvasGroup(cgPlayer, false);

        Debug.Log("escena prendida");
    }

    public void SceneVorcarbisOff()
    {
        ServiceLocator.Instance.GetDependency<CheloCamera>().isResting = false;
        ServiceLocator.Instance.GetDependency<ControllerPlayer>().isResting = false;
        ServiceLocator.Instance.GetDependency<VorcarbisEFSM>().isResting = false;
        ServiceLocator.Instance.GetDependency<PlayerMVC>().gameObject.SetActive(true);

        CanvasGroup cgPlayer = ServiceLocator.Instance.GetDependency<ManaUI>().gameObject.GetComponent<CanvasGroup>();
        UtilitiesAgus.ToggleCanvasGroup(cgPlayer, true);

        cameraFake.SetActive(false);
        faithFake.SetActive(false);


        ServiceLocator.Instance.GetDependency<VorcarbisEFSM>().ActiveFade();




        var rigid = vorcarbis.GetComponent<Rigidbody>();
        if (rigid != null) rigid.velocity = Vector3.zero;
        vorcarbis.gameObject.transform.position = new Vector3(-216.7891f, 88.72359f, 103.9406f);
        rigid.velocity = new Vector3(0, 0, 0);
        rigid.velocity = Vector3.zero;


        StartCoroutine(AfterSceneOff(rigid));
        
        Debug.Log("escena apagada, comienza la bossfight");
    }

    IEnumerator AfterSceneOff(Rigidbody rigid)
    {
        yield return new WaitForSeconds(.5f);
        vorcarbis.gameObject.transform.position = new Vector3(-216.7891f, 88.72359f, 103.9406f);
        rigid.velocity = Vector3.zero;
        gameObject.SetActive(false);
        Debug.Log("TERMINO CORRUTINA");
    }
}
