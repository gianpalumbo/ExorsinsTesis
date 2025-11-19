using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")] public float moveSpeed = 5f, turnSpeed = 10f;
    public CheloCamera cameraOrbit;
    [Tooltip("Camera reference for relative movement")] public Transform cameraTransform;

    Rigidbody rb;
    Vector3 inputDir;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    //ESTO PODRIA IR EN EL UPDATE DIRECTAMENTE DEL CONTROLLER PARA QUE LO CALCULE TODO EL TIEMPO
    void Update()
    {
        var h = Input.GetAxisRaw("Horizontal");
        var v = Input.GetAxisRaw("Vertical");

        Vector3 forward = Vector3.Scale(cameraTransform.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 right = Vector3.Scale(cameraTransform.right, new Vector3(1, 0, 1)).normalized;
        inputDir = (forward * v + right * h).normalized;

        if (cameraOrbit.lockTarget != null)
        {
            var toEnemy = cameraOrbit.lockTarget.position - transform.position;
            toEnemy.y = 0;
            Quaternion rot = Quaternion.LookRotation(toEnemy.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, turnSpeed * Time.deltaTime);
        }
        else if (inputDir.sqrMagnitude > 0.01f)
        {
            Quaternion rot = Quaternion.LookRotation(inputDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, turnSpeed * Time.deltaTime);
        }
    }

    //ESTE VENDRIA A SER EL NUEVO RUN EN MODEL
    void FixedUpdate()
    {
        var velY = rb.velocity.y;
        if (inputDir.sqrMagnitude > 0.01f)
            rb.velocity = inputDir * moveSpeed + Vector3.up * velY;
        else
            rb.velocity = new Vector3(0, velY, 0);
    }
}