using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeBarLookAtPlayer : MonoBehaviour
{
    PlayerLife _player;
    float _rotationSpeed;

    void Awake()
    {
        _player = FindObjectOfType<PlayerLife>();
    }
    void Update()
    {
        LookAtPlayerOnY();
    }
    void LookAtPlayerOnY()
    {
        Vector3 dir = (_player.transform.position - transform.position).normalized;
        dir.y = 0;
        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
        }
    }
}
