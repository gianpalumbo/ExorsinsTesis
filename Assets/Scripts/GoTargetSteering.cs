using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoTargetSteering : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] float baseSpeed = 5f;        // Velocidad inicial
    [SerializeField] float acceleration = 2f;     // Qué tan rápido acelera con el tiempo
    [SerializeField] int soulsToAdd;
    [SerializeField] float offsetY = 1.5f;
    [SerializeField] float maxSpeed = 25f;        // Velocidad máxima límite

    bool followingPlayer = false;
    float currentSpeed;                           // Velocidad actual que irá aumentando

    private void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player").transform;
        currentSpeed = baseSpeed; // Arranca con la velocidad base
    }

    private void Update()
    {
        if (!followingPlayer || target == null) return;

        // Dirección hacia el jugador (con offset Y)
        Vector3 direction = (target.position + new Vector3(0, offsetY, 0) - transform.position);
        float distance = direction.magnitude;
        Vector3 dirNormalized = direction.normalized;

        // Acelera progresivamente hasta un límite
        currentSpeed = Mathf.Min(currentSpeed + acceleration * Time.deltaTime, maxSpeed);

        // Movimiento hacia el jugador
        transform.position += dirNormalized * currentSpeed * Time.deltaTime;

        // Destruir si está muy cerca del jugador para evitar loops infinitos
        if (distance < 0.5f)
        {
            target.GetComponent<PlayerMVC>()?.AddSoulsNew(soulsToAdd);
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerMVC>(out PlayerMVC player))
        {
            player.AddSoulsNew(soulsToAdd);
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        followingPlayer = true;
    }
}
