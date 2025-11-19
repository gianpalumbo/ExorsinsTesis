using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushHitbox : MonoBehaviour
{
    [SerializeField] float pushDmg, force;
    [SerializeField] GameObject owner, owner2;

    private void Awake()
    {
        if (GetComponentInParent<VorcarbisEFSM>())
            owner = GetComponentInParent<VorcarbisEFSM>().gameObject;
        else if (GetComponentInParent<Beelzebub>())
            owner2 = GetComponentInParent<Beelzebub>().gameObject;
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent<PlayerLife>(out PlayerLife player) && owner != null)
        {
            player.TakeDamage(pushDmg);
            player.Knockback(owner.transform.forward, force);
        }
        if (other.TryGetComponent<PlayerLife>(out PlayerLife player2) && owner2 != null)
        {
            Vector3 dir = (player2.gameObject.transform.position - owner2.transform.position).normalized;
            player2.TakeDamage(pushDmg);
            player2.Knockback(dir , force);
        }
    }
}
