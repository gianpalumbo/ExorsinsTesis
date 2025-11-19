using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DEMOVoidFall : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent<PlayerLife>(out PlayerLife playerLife))
        {
            playerLife.TakeDamage(playerLife.MaxLife);
        }
    }
}
