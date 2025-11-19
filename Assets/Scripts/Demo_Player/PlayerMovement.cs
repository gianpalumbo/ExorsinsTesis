using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    Vector2 _turn;
    public bool isGrounded, canRoll, canAttack;
    Rigidbody playerRB;
    AnimationController animControl;
    [Header("Demo Knight Values")]
    //[SerializeField] float _sensitivity;
    [SerializeField] float _speed;
    float _maxSpeed = 5;
    [SerializeField] float _jumpStrenght, rollStrenght;
    [SerializeField] float _runningSpeed;
    [SerializeField] float countRoll, rollMaxCD;
    [SerializeField] float countAttack, attackMaxCD;
    [SerializeField] KeyCode jumpKey, runningKey, rollKey;

    private void Start()
    {
        PauseManager.instance.Subscribe(ArtificialUpdate);

        if (playerRB == null) playerRB = GetComponent<Rigidbody>();
        if (animControl == null) animControl = GetComponent<AnimationController>();

        _speed = _maxSpeed;

        countRoll = rollMaxCD;
        countAttack = attackMaxCD;
    }

    private void ArtificialUpdate()
    {
        if(Input.GetKey(runningKey))
        {
            _speed = _runningSpeed;
        }
        else
        {
            _speed = _maxSpeed;
        }

        Vector3 mov = new (Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));

        mov = transform.TransformDirection(mov) * (_speed * Time.deltaTime);

        //transform.rotation = Quaternion.Euler(0, _turn.x ,0);
        transform.position += new Vector3 (mov.x , 0 , mov.z);

        if(Input.GetKeyDown(jumpKey) && isGrounded)
        {
            Jump();
        }

        //Roll logic
        countRoll += Time.deltaTime;
        countRoll = Mathf.Clamp(countRoll, 0, rollMaxCD);
        canRoll = countRoll == rollMaxCD;

        if(Input.GetKeyDown(rollKey) && isGrounded && canRoll) 
        {
            countRoll = 0;
            Roll();
        }

        countAttack += Time.deltaTime;
        countAttack = Mathf.Clamp(countAttack, 0, attackMaxCD);
        canAttack = countAttack == attackMaxCD;
        if (Input.GetMouseButtonDown(0) && isGrounded && canAttack)
        {
            countAttack = 0;
            Roll();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == 8)
        {
            Debug.Log("GROUNDED");
            isGrounded = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.layer == 8)
        {
            Debug.Log("NOT GROUNDED");
            isGrounded = false;
        }
    }

    public void Jump()
    {
        playerRB.AddForce(new Vector3 (0 , _jumpStrenght , 0) ,ForceMode.Impulse); 
    }

    public void Roll()
    {
        playerRB.AddForce(new Vector3(Input.GetAxisRaw("Horizontal") * rollStrenght, 0, Input.GetAxisRaw("Vertical") * rollStrenght), ForceMode.Impulse);
    }

    private void OnDisable()
    {
        PauseManager.instance.Unsubscribe(ArtificialUpdate);
    }


    //CHELO WAS HERE
    private void OnDestroy()
    {
        PauseManager.instance.Unsubscribe(ArtificialUpdate);
    }
}
