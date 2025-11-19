using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClampTo : MonoBehaviour
{
    [SerializeField] Transform headParent;
    void Update()
    {
        transform.position = headParent.position;
    }
}
