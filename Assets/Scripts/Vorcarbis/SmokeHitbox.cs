using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmokeHitbox : MonoBehaviour
{
    [SerializeField] float dmgPerInterval, timeBetweenHit ,lifetimeSmoke;
    bool isPlayerInside, hasDisipated;
    PlayerLife playerLife;
    float _counter, lifetimeCounter;

    public bool HasDisipated
    {
        get { return hasDisipated; }   // lo que hace cuando alguien lee
        set { hasDisipated = value; }  // lo que hace cuando alguien asigna
    }

    private void OnEnable()
    {
        _counter = 0;
        playerLife = ServiceLocator.Instance.GetDependency<PlayerLife>();
        hasDisipated = false;
        lifetimeCounter = 0;
    }
    private void Update()
    {
        if (isPlayerInside)
        {
            if (GenericCounterSHBox(timeBetweenHit))
            {
                playerLife.TakeDamage(dmgPerInterval);
                ResetCounter();
            }
        }
        lifetimeCounter += Time.deltaTime;
        if (lifetimeCounter >= lifetimeSmoke)
            gameObject.SetActive(false);
    }

    bool GenericCounterSHBox(float time)
    {
        _counter += Time.deltaTime;
        return _counter >= time;
    }
    void ResetCounter() => _counter = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerLife>(out PlayerLife player))
        {
            isPlayerInside = true;
            if (playerLife == null) 
                playerLife = player;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        isPlayerInside = false;
    }
    private void OnDisable()
    {
        hasDisipated = true;
        isPlayerInside = false;
    }
}
