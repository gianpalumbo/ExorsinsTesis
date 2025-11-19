using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoToMainMenuAnimationEvent : MonoBehaviour
{
    public void GoToMainMenu()
    {
        // Assuming you have a method to load the main menu scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
    }
}
