using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaceHitbox : MonoBehaviour
{
    VorcarbisEFSM wielder1;
    Beelzebub wielder2;
    void Awake()
    {
        wielder1 = GetComponentInParent<VorcarbisEFSM>();
        if(wielder2 == null)
            wielder2 = GetComponentInParent<Beelzebub>();
    }
    void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent<PlayerLife>(out PlayerLife playerLife) && wielder1 != null)
        {
            playerLife.TakeDamage(wielder1.dmgAttk1);
        }
        else if (other.TryGetComponent<PlayerLife>(out PlayerLife playerLife1) && wielder2 != null)
        {
            playerLife1.TakeDamage(wielder2.GetCurrentDmg());
        }
    }
}
