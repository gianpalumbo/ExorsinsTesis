using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleKinematic : MonoBehaviour
{
    Rigidbody myRigidbody;
    LayerMask layerPlayer = 1 << 7; //Layer 7 = Player
    public float radiusToKinematic = 1f;

    void Awake() { myRigidbody = GetComponent<Rigidbody>(); }
    void Update()
    {
        if(Physics.CheckSphere(transform.position, radiusToKinematic, layerPlayer))
            myRigidbody.isKinematic = true;
        else
            myRigidbody.isKinematic = false;
    }
}
