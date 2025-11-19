using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantiateFaith : MonoBehaviour
{
    private void Start()
    {
        AdditiveSceneManagerAgus.Initialize(this);
        AdditiveSceneManagerAgus.LoadSceneAdditiveByName("FaithScene");
    }
}
