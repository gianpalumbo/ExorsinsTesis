using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomTrigger : MonoBehaviour
{
    [SerializeField] RoomManager _roomManager;
    [SerializeField] RoomComponent _myRoom;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) StartCoroutine(_roomManager.RoomChange(_myRoom));
        
    }
}
