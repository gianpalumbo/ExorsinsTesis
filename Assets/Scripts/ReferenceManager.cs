//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class ReferenceManager : MonoBehaviour
//{
//    public static ReferenceManager Instance;
//    [Header("Global References")]
//    public PlayerMVC playerMVC;
//    public PlayerLife playerLife;
//    public ControllerPlayer controller;
//    public CheloCamera cheloCamera;
//    public KarmicMenu karmicMenu;

//    void Awake()
//    {
//        if (Instance == null)
//        {
//            Instance = this;
//            DontDestroyOnLoad(gameObject);
//        }
//        else
//        {
//            Destroy(gameObject);
//        }
//        controller = playerMVC.Controller; 
//    }
//    void Start() 
//    { 
//        cheloCamera = Camera.main.GetComponent<CheloCamera>();
//    }
//}
