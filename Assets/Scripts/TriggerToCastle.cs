using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerToCastle : MonoBehaviour
{
    [SerializeField] CanvasGroup interactuableCG;
    public bool isOnTrigger, hasEntered;

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

            ServiceLocator.Instance.GetDependency<PlayerMVC>().startingScene = PlayerMVC.StartingScene.Outside;
            ServiceLocator.Instance.GetDependency<PlayerMVC>().LoadScene(PlayerMVC.StartingScene.Castle);
        }
    }

}
