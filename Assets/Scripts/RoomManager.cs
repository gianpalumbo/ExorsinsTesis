using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    [SerializeField] RoomComponent [] _rooms;
    [SerializeField] RoomComponent _initialRoom;

    private void Start()
    {
        foreach (var room in _rooms) room.gameObject.SetActive(false);
        _initialRoom.gameObject.SetActive(true);
        foreach (var neighboor in _initialRoom._neighboors) neighboor.gameObject.SetActive(true);
    }

    public void RoomChange(RoomComponent newRoom)
    {
        foreach (var room in _rooms) room.gameObject.SetActive(false);
        newRoom.gameObject.SetActive(true);
        foreach (var neighboor in newRoom._neighboors) neighboor.gameObject.SetActive(true);
    }
}
