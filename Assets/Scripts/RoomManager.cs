using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    [SerializeField] RoomComponent [] _rooms;
    [SerializeField] RoomComponent _initialRoom;
    RoomComponent _actualRoom;

    private void Start()
    {
        foreach (var room in _rooms) room.gameObject.SetActive(false);
        _initialRoom.gameObject.SetActive(true);
        foreach (var neighboor in _initialRoom.neighboors) neighboor.gameObject.SetActive(true);
        _actualRoom = _initialRoom;
    }

    private void Update()
    {
    }

    public IEnumerator RoomChange(RoomComponent newRoom)
    {
        _actualRoom = newRoom;
        foreach (var room in _rooms) 
        { 
            yield return null;
            room.gameObject.SetActive(false);
        }
        newRoom.gameObject.SetActive(true);
        foreach (var neighboor in newRoom.neighboors) 
        {
            yield return null;
            neighboor.gameObject.SetActive(true); 
        }
    }
}
