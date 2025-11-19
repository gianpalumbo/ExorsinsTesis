using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BileAcidPuddle : MonoBehaviour
{
    [SerializeField] float dmg = 4, time = 4, lifetime = 8;
    float counter = 0;

    void OnEnable()
    {
        counter = 0;
    }
    private void Update()
    {
        counter += Time.deltaTime;
        if (counter >= lifetime) Destroy(gameObject);
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent<PlayerLife>(out PlayerLife player))
        {
            player.PoisonPlayer(dmg, time);
        }
    }
}
