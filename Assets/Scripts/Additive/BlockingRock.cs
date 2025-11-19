using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockingRock : MonoBehaviour
{
    Animator anim;
    Rigidbody rb;
    [SerializeField] GameObject timeline;

    private void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
    }

    public void RockHasFallen()
    {
        //AdditiveSceneManagerAgus.Initialize(this);
        //INICIALIZADO EN PLAYER
        AdditiveSceneManagerAgus.LoadSceneAdditiveByName("Outside_1", "Cave_1");
    }

    public void TurnOffKinematic() => rb.isKinematic = false;
    public void TurnOnKinematic()
    {   
        rb.isKinematic = true;
    }
    public void TurnOffTimeline()
    {
        
        timeline.SetActive(false);
    }
}
