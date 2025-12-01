using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnTriggerTurnOffMusic : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<PlayerMVC>())
        {
            if(SoundManager.Instance != null)
            {
                SoundManager.Instance.MuteMusic();
            }
        }
    }
}
