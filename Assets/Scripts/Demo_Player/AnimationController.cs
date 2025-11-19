using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    [SerializeField] private Animator anim;
    [SerializeField] PlayerMovement pMov;
    public KeyCode runKey = KeyCode.LeftShift, jumpKey = KeyCode.Space, rollKey = KeyCode.LeftControl;
    [SerializeField] Transform feet;
    [SerializeField] float landingRayDistance = .5f;
    private float maxValue = 0.5f; // Inicializado en 0.5 (caminando)
    public bool hasLanded;

    private void Awake()
    {
        if (anim == null) anim = GetComponent<Animator>();
    }


    private void Update()
    {
        bool isMoving = Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0;
        bool isRunning = isMoving && Input.GetKey(runKey);
        
        hasLanded = Physics.Raycast(feet.position, Vector3.down, landingRayDistance);
        anim.SetBool("hasLanded", hasLanded);
        //Si apreto espacio primero se reproduce JumpingUp que pasa por fallingIdle y por ultimo chequeo si estoy a punto de landear
        if (Input.GetKeyDown(jumpKey) && pMov.isGrounded) anim.SetTrigger("Jump");

        if(Input.GetKeyDown(rollKey) && pMov.canRoll && pMov.isGrounded) anim.SetTrigger("Roll");

        // Si no hay movimiento, activamos Idle
        anim.SetBool("Idle", !isMoving);

        float rollY = Input.GetAxisRaw("Vertical");

        if (rollY == 0) rollY = 1;

        anim.SetFloat("rollY", rollY);

        // Si hay movimiento, ajustamos valores de Blend Tree
        if (isMoving)
        {
            // Interpolamos gradualmente entre caminar (0.5) y correr (1)
            float targetValue = isRunning ? 1f : 0.5f;

            anim.SetBool("isRunning", isRunning);

            maxValue = Mathf.MoveTowards(maxValue, targetValue, Time.deltaTime * 2); // Ajusta la velocidad de transición

            anim.SetFloat("movX", Mathf.Clamp(Input.GetAxis("Horizontal"), -maxValue, maxValue));
            anim.SetFloat("movY", Mathf.Clamp(Input.GetAxis("Vertical"), -maxValue, maxValue));
        }
        else
        {
            maxValue = 0.5f; // Reiniciamos a caminar cuando el personaje deja de moverse
        }

        if (pMov.canAttack && pMov.isGrounded && Input.GetMouseButtonDown(0)) AttackAnimation();
    }

    public void AttackAnimation()
    {
        anim.SetTrigger("Attack");
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red; // Color de la línea
        Vector3 start = feet.position;
        Vector3 end = start + Vector3.down * landingRayDistance;
        Gizmos.DrawLine(start, end);
    }

}