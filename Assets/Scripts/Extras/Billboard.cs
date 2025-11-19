using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Esta clase se encarga de girar el texto de un objeto 3D hacia la camara
/// </summary>

public class Billboard : MonoBehaviour
{
    void Update()
    {
        if (Camera.main != null)
        {
            // hace que el objeto mire hacia la camara. si es necesario, rota 180 grados en Y para corregir la orientación
            transform.LookAt(Camera.main.transform);           
            transform.Rotate(0, 180, 0);        }
    }
}
