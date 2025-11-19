using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplyForceOnStay : MonoBehaviour
{
    [SerializeField] Vector3 forceDir;
    [SerializeField] float force;

    private void OnTriggerStay(Collider other)
    {
        if(other.GetComponent<PlayerMVC>())
        {
            Rigidbody playerRB = other.GetComponent<Rigidbody>();
            playerRB.AddForce(forceDir * force, ForceMode.VelocityChange);
        }
    }
}
