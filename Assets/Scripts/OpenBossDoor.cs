using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenBossDoor : MonoBehaviour
{
    [SerializeField] CanvasGroup interactuableCG;
    public bool isOnTrigger, hasEntered;

    public int playerKeyCount;

    private void OnDisable()
    {
        ServiceLocator.Instance.RemoveDependency<TriggerToCastle>();
    }
    void Start()
    {
        UtilitiesAgus.ToggleCanvasGroup(interactuableCG, false);
    }

    //private void OnCollisionEnter(Collision collision)
    //{
    //    if (hasEntered) return;

    //    if (collision.gameObject.TryGetComponent<AttackEFSM>(out AttackEFSM player))
    //        playerKeyCount = player.keyCount;
    //    UtilitiesAgus.ToggleCanvasGroup(interactuableCG, true);
    //    isOnTrigger = true;
    //}

    private void OnTriggerEnter(Collider other)
    {
        if (hasEntered) return;

        if (other.gameObject.TryGetComponent<AttackEFSM>(out AttackEFSM player))
            playerKeyCount = player.keyCount;
        UtilitiesAgus.ToggleCanvasGroup(interactuableCG, true);
        isOnTrigger = true;
    }
    //private void OnCollisionExit(Collision collision)
    //{
    //    if (collision.gameObject.TryGetComponent<AttackEFSM>(out AttackEFSM player))
    //    {
    //        UtilitiesAgus.ToggleCanvasGroup(interactuableCG, false);
    //        isOnTrigger = false;
    //    }
    //}

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.TryGetComponent<AttackEFSM>(out AttackEFSM player))
        {
            UtilitiesAgus.ToggleCanvasGroup(interactuableCG, false);
            isOnTrigger = false;
        }
    }

    void Update()
    {
        if (isOnTrigger && !hasEntered && Input.GetKeyDown(KeyCode.E))
        {
            if (playerKeyCount != 3) return;
            hasEntered = true;
            UtilitiesAgus.ToggleCanvasGroup(interactuableCG, false);
            GetComponent<Animator>().SetTrigger("OpenDoor");
        }
    }
}
