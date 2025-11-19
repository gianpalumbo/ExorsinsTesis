//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using IA2;
//using System;
//using JetBrains.Annotations;

//public class Personaje : MonoBehaviour
//{
//    public enum PlayerInputs {  }
//    private EventFSM<PlayerInputs> _myFsm;
//    private Rigidbody _myRb;
//    public Renderer _myRen;

//    private void Awake()
//    {

//        _myFsm = new EventFSM<PlayerInputs>(idle);
//    }

//    private void SendInputToFSM(PlayerInputs inp) => _myFsm.SendInput(inp);

//    private void Update()
//    {
//        _myFsm.Update();

//        if (Input.GetKeyDown(KeyCode.R))
//            SendInputToFSM(PlayerInputs.DIE);
//    }

//    private void FixedUpdate()
//    {
//        _myFsm.FixedUpdate();
//    }

//    /*void LateUpdate()
//     {
//         _myFsm.LateUpdate();
//     }*/

//    private void OnCollisionEnter(Collision collision)
//    {
//        SendInputToFSM(PlayerInputs.IDLE);
//    }
//}
