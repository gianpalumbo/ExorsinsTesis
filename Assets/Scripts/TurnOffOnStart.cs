using UnityEngine;

public class TurnOffOnStart : MonoBehaviour
{
    void Start()
    {
        this.gameObject.SetActive(false);   
    }
}
