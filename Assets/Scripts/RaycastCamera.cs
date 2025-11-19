using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastCamera : MonoBehaviour
{
    public Transform cameraPos;                     // Cámara actual
    public Transform originalCameraTransform;    // Posición original relativa (hijo del pivot)
    public float smoothSpeed = 10f;              // Qué tan suave vuelve

    private Vector3 currentVelocity = Vector3.zero; // Variable para almacenar la velocidad

    private void LateUpdate()
    {
        Vector3 dir = originalCameraTransform.position - transform.position;
        float distance = dir.magnitude;
        dir.Normalize();

        // Punto objetivo hacia donde mover la cámara
        Vector3 targetPosition = originalCameraTransform.position;

        if (Physics.Raycast(transform.position, dir, out RaycastHit hit, distance))
        {
            Debug.DrawRay(transform.position, dir * hit.distance, Color.red);

            if (hit.collider.CompareTag("Obstacle"))
            {
                Debug.Log("Tocó obstáculo: " + hit.collider.name);

                // Reposiciona la cámara justo antes del obstáculo
                targetPosition = hit.point + hit.normal * 0.5f;
            }
        }

        // Mover la cámara de forma suave hacia targetPosition
        cameraPos.position = Vector3.SmoothDamp(cameraPos.position, targetPosition, ref currentVelocity, 0.1f);
    }

    private void OnDrawGizmos()
    {
        Vector3 dir = cameraPos.transform.position - transform.position;

        Gizmos.DrawRay(transform.position, dir);
    }
}
