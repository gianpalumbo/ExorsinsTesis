using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DEMOHITBOXELITEENEMY : MonoBehaviour
{
    private float _dmgSlow;
    private float _dmgFrenzy;
    private bool _frenzyModeActive;

    private void Start()
    {
        EliteEnemy elite = GetComponentInParent<EliteEnemy>();
        _dmgSlow = elite.DmgSlow;
        _dmgFrenzy = elite.DmgFrenzy;
        _frenzyModeActive = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent<PlayerLife>(out PlayerLife player) && !_frenzyModeActive)
        {
            player.TakeDamage(_dmgSlow);
            Debug.Log($"DAÑO AL PERSONAJE {_dmgSlow} EN MODO SLOW");
        }
        else if (other.GetComponent<PlayerLife>() && _frenzyModeActive)
        {
            player.TakeDamage(_dmgFrenzy);
            Debug.Log($"DAÑO AL PERSONAJE {_dmgFrenzy} EN MODO FRENZY");
        }
    }
}
