//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;

//public class SkillWheelUI : MonoBehaviour
//{
//    [SerializeField] private SkillManager skillManager;
//    //[SerializeField] private GameObject skillWheelPanel; // Panel de la rueda
//    [SerializeField] private KeyCode toggleKey = KeyCode.Tab;
//    [SerializeField] private KeyCode activateKey = KeyCode.Mouse0, changeSkillKey = KeyCode.Tab; // clic izquierdo por defecto

//    //CHELO WAS HERE: ID CURSOR MANAGER
//    private readonly string id = "SkillWheelUI";

//    //AGUS WAS HERE
//    [SerializeField] bool bulletTimeToggle = false;

//    private void Awake()
//    {
//        //    skillManager = GetComponent<SkillManager>();
//        if (skillManager == null) Debug.LogError("PlayerSkillInput: falta SkillManager en el GameObject.");
//        //skillWheelPanel.SetActive(false);
//    }

//    //private void Update()
//    //{
//    //    if (Input.GetKeyDown(toggleKey)) ToggleSkillWheel();
//    //    if (Input.GetKeyDown(toggleKey)) { ToggleSkillWheel(); }
//    //    if (Input.GetKeyUp(toggleKey)) { CloseSkillWheel(); }

//    //    if (Input.GetKeyDown(changeSkillKey)) skillManager.ChangeSkill();

//    //    if (Input.GetKeyDown(activateKey)) skillManager.UseSkill();
//    //}

//    public void ToggleSkillWheel()
//    {
//        ServiceLocator.Instance.GetDependency<ControllerPlayer>().canAttackAtAll = false;
//        Debug.Log(ServiceLocator.Instance.GetDependency<ControllerPlayer>().canAttackAtAll);
//        //Cursor.lockState = CursorLockMode.None;
//        //Cursor.visible = true;
//        //if (skillWheelPanel != null)
//        //{
//        //    skillWheelPanel.SetActive(true);
//        //    Debug.Log("PRENDO SKILLWHEEL");
//        //}
//        //else Debug.LogWarning("SkillWheelUI: skillWheelPanel no asignado.");


//        if (bulletTimeToggle) Time.timeScale = .5f;

//        //CHELO WAS HERE: AGREGUE ID AL CURSOR MANAGER
//        CursorUIManager.Instance.RequestCursorState(true, id);

//        //AGUS WAS HERE
//        UtilitiesAgus.ToggleCursor(true);



//        //CHELO WAS HERE: A LA MARA LE ACTIVO EL BOOLEANO PARA QUE NO ROTE SI TIENE EL BOOL ACTIVADO
//        ServiceLocator.Instance.GetDependency<CheloCamera>().canRotate = true;



//    }

//    public void CloseSkillWheel()
//    {
//        //Cursor.lockState = CursorLockMode.Locked;
//        //Cursor.visible = false;
//        //if (skillWheelPanel != null)
//        //{
//        //    skillWheelPanel.SetActive(false);
//        //    Debug.Log("APAGO SKILLWHEEL");
//        //}
//        //else Debug.LogWarning("SkillWheelUI: skillWheelPanel no asignado.");

//        Time.timeScale = 1f;

//        //CHELO WAS HERE: ELIMINO ID DEL CURSOR MANAGER
//        CursorUIManager.Instance.ReleaseCursorRequest(id);

//        ServiceLocator.Instance.GetDependency<ControllerPlayer>().canAttackAtAll = true;
//        Debug.Log(ServiceLocator.Instance.GetDependency<ControllerPlayer>().canAttackAtAll);

//        //AGUS WAS HERE
//        UtilitiesAgus.ToggleCursor(false);



//        //CHELO WAS HERE: A LA MARA LE ACTIVO EL BOOLEANO PARA QUE NO ROTE SI TIENE EL BOOL ACTIVADO
//        ServiceLocator.Instance.GetDependency<CheloCamera>().canRotate = false;
//    }
//}