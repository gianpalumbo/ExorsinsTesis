using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerToCastle : MonoBehaviour
{
    [SerializeField] CanvasGroup interactuableCG;
    bool isOnTrigger, hasEntered;

    void Start()
    {
        UtilitiesAgus.ToggleCanvasGroup(interactuableCG, false);
    }

    private void OnTriggerEnter(Collider other)
    {
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

            StartCoroutine(DoFadeAndThenLoad());
        }
    }

    IEnumerator DoFadeAndThenLoad()
    {
        StartCoroutine(ServiceLocator.Instance.GetDependency<BlackPanelFade>().Fade(1f, .5f, false, false));

        yield return new WaitForSeconds(1.5f);

        ServiceLocator.Instance.GetDependency<PlayerMVC>().startingScene = PlayerMVC.StartingScene.Outside;
        ServiceLocator.Instance.GetDependency<PlayerMVC>().LoadScene(PlayerMVC.StartingScene.Castle);
    }
}
