using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DeathManager : MonoBehaviour
{
    public GameObject player;    
    public Vector3 lastSpawn; //CHELO WAS HERE: GUARDO EL ULTIMO SANTUARIO VISITADO PARA QUE CUANDO MUERA O INGRESE AL JUEGO LO MANTE AHI
    //public Vector2 originalSpawn;
    public event Action OnRespawn;

    private void Awake()
    {
        if (lastSpawn == new Vector3(0, 0, 0))
        {
            //Debug.Log($"{gameObject.name} LASTSPAWN ES NULO");
            lastSpawn = player.transform.position;
            //Debug.Log(lastSpawn);
        }

        ServiceLocator.Instance.RegisterDependency<DeathManager>(this);
    }

    private void OnDestroy()
    {
        ServiceLocator.Instance.RemoveDependency<DeathManager>();
    }

    public void LastSpawnPoint(Vector3 spawnLocation) 
    { 
        lastSpawn = spawnLocation;
        Debug.Log(lastSpawn);
    }

    public void Respawn()
    {
        Debug.Log("entity revivio");
        player.transform.position = lastSpawn;
        var pc = player.GetComponent<PlayerLife>();
        if (pc != null) pc.ResetPlayer();

        pc.isDead = false;
        var pmvc = player.GetComponent<PlayerMVC>();
        pmvc.enabled = true;

        Beelzebub gula = ServiceLocator.Instance.GetDependency<Beelzebub>();

        gula.GoToThinkAndRestartLife();
        //gula.gameObject.GetComponent<BossLife>().HideLifeBar();

        //AGUS WAS HERE
        OnRespawn?.Invoke();

        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;

        ResetTrigger.ResetAllEnemies();
        PointsManager.Instance.SubtractPoints(PointsManager.Instance.CurrentPoints / 2);

        //ADDON PRENDO MOUSE CUANDO ABRO CANVAS
        UtilitiesAgus.ToggleCursor(false);
    }
}
