using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DEMOLOADFAITH : MonoBehaviour
{
    public string loadScene;
    void Start()
    {
        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(loadScene, UnityEngine.SceneManagement.LoadSceneMode.Additive);
    }
}
