using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    public bool onlyY;
    void Update()
    {
        if(Camera.main != null)
            transform.LookAt(Camera.main.transform);
        if (!onlyY) return;
        transform.rotation = new Quaternion(0, transform.rotation.y, 0, transform.rotation.w);
    }
}
