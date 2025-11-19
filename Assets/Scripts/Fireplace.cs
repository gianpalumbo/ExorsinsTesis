using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireplace : MonoBehaviour
{
    #region Values
    [SerializeField] float soundRadius = 10f, toleranceRadius = 50f;
    [Header("<color=red>Solo asignarle Transform del Player</color>")]
    [SerializeField] Transform _player;
    #endregion
    void Start()
    {
        if(FireplaceManager.Instance != null)
            FireplaceManager.Instance.SetVolume(0);
    }
    void Update()
    {
        if(_player == null) return;
        float distance = Vector3.Distance(_player.transform.position, transform.position);
        if (distance > toleranceRadius) return;

        float volume = Mathf.InverseLerp(10f, 1.1f, distance);

        if(distance < soundRadius)
        {
            FireplaceManager.Instance.SetVolume(Mathf.Clamp01(volume));
        }
    }
}
