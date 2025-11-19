using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackTrigger : MonoBehaviour
{
    public float dmg;

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        IDamageable damageable = other.GetComponent<IDamageable>();
        if (other.CompareTag("Enemy"))
        {
            if (damageable != null)
            {
                damageable.TakeDamage(dmg);
                gameObject.SetActive(false);
            }
        }

    }
}
