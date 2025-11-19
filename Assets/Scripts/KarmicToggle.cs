using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
public class KarmicToggle : MonoBehaviour
{
    [SerializeField] GameObject karmicOptions;
    [SerializeField] float distance;
    [SerializeField] KarmicMenu karmicMenu;
    PlayerMVC player;

    bool canOpenMenu = false, hasSelected = false;

    public event Action OnMenuEnabled, OnMenuDisabled;

    void Awake()
    {
        ServiceLocator.Instance.RegisterDependency<KarmicToggle>(this);
    }

    private void OnEnable()
    {
        if(karmicOptions == null)
            karmicOptions = ServiceLocator.Instance.GetDependency<KarmicMenu>().gameObject;
        //else
            //Debug.Log("No encontre karmic options GO");

        if (karmicMenu == null)
            karmicMenu = ServiceLocator.Instance.GetDependency<KarmicMenu>();
        //else
        //Debug.Log("No encontre karmic menu");

        karmicOptions.SetActive(false);
    }

    IEnumerator Start()
    {
        yield return new WaitForSeconds(1f);

        player = ServiceLocator.Instance.GetDependency<PlayerMVC>();

        //Debug.Log("encontre a " + player);

        gameObject.SetActive(false);
    }

    float nextCheckTime = 0f;
    float checkInterval = 0.5f;
    private void Update()
    {
        if (player == null) return;

        if (Time.time >= nextCheckTime)
        {
            nextCheckTime = Time.time + checkInterval;

            canOpenMenu = Vector3.Distance(player.transform.position, transform.position) <= distance && !hasSelected;

            if (canOpenMenu)
            {
                //karmicOptions.SetActive(true);
                ServiceLocator.Instance.GetDependency<ControllerPlayer>().canAttackAtAll = false;
                Time.timeScale = .5f;
                OnMenuEnabled?.Invoke();
                karmicOptions.SetActive(true);
                karmicMenu.OpenMenu();
                ServiceLocator.Instance.GetDependency<CheloCamera>().isResting = true;
                ServiceLocator.Instance.GetDependency<PlayerMVC>().SetResting(true);
            }
            else
            {
                //karmicOptions.SetActive(false);
                ServiceLocator.Instance.GetDependency<ControllerPlayer>().canAttackAtAll = true;
                OnMenuDisabled?.Invoke();
                Time.timeScale = 1;
                karmicOptions.SetActive(false);
                karmicMenu.CloseMenu();
                ServiceLocator.Instance.GetDependency<CheloCamera>().isResting = false;
                ServiceLocator.Instance.GetDependency<PlayerMVC>().SetResting(false);
            }

        }
    }

    public void TurnThisOff() 
    { 
        gameObject.SetActive(false);
        hasSelected = true;
        Time.timeScale = 1;
        OnMenuDisabled?.Invoke();
        ServiceLocator.Instance.GetDependency<ControllerPlayer>().canAttackAtAll = true;

        ServiceLocator.Instance.GetDependency<CheloCamera>().isResting = false;
        ServiceLocator.Instance.GetDependency<PlayerMVC>().SetResting(false);
    }

    private void OnDisable()
    {
        if (ServiceLocator.Instance.TryGetDependency<ControllerPlayer>(out var controllerPlayer))
            controllerPlayer.canAttackAtAll = true;

        ServiceLocator.Instance.RemoveDependency<KarmicToggle>();
    }

    private void OnDestroy()
    {
        ServiceLocator.Instance.RemoveDependency<KarmicToggle>();
    }
}
