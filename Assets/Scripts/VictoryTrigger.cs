using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryTrigger : MonoBehaviour
{
    [SerializeField] PlayerMVC playerMVC;
    [SerializeField] GameObject victoryPanel;
    [SerializeField] GameObject goodEnding, badEnding, neutralEnding;

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerLife>())
        {
            Cursor.lockState = CursorLockMode.None; // Unlock the cursor when going to the main menu
            Cursor.visible = true; // Make the cursor visible

            victoryPanel.SetActive(true);
            if (playerMVC.GetEndingFromKarma() == 0) neutralEnding.SetActive(true);
            else if (playerMVC.GetEndingFromKarma() > 0) goodEnding.SetActive(true);
            else if (playerMVC.GetEndingFromKarma() < 0) badEnding.SetActive(true);
        }
    }

    //private void OnDisable()
    //{
    //    victoryPanel.SetActive(false);
    //    goodEnding.SetActive(false);
    //    badEnding.SetActive(false);
    //    neutralEnding.SetActive(false);
    //}
}
