using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerBlockingRock : MonoBehaviour
{
    PlayerMVC player;
    [SerializeField] BlockingRock rock;
    Vector3 myTransformIgnoreY;
    bool hasFallen = false;

    [SerializeField] GameObject timeline;

    private void Start()
    {
        player = ServiceLocator.Instance.GetDependency<PlayerMVC>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerMVC>(out PlayerMVC player) && !hasFallen)
        {
            hasFallen = true;
            //PRENDO TIMELINE Y TIMELINE SE ENCARGAR DE CARGAR Y DESCARGAR ESCENAS
            timeline.SetActive(true);
            ServiceLocator.Instance.GetDependency<DeathManager>().LastSpawnPoint(new Vector3(transform.position.x, player.transform.position.y, transform.position.z));
        }
    }
}
