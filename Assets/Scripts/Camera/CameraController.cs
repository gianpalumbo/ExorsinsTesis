using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float _xSens;
    [SerializeField] private float _ySens;
    [SerializeField] private float _distance;
    [SerializeField] private float _yOffset;
    [SerializeField] private float _minYAngle;
    [SerializeField] private float _maxYAngle;

    [SerializeField] private Transform _target;
    [SerializeField] private Transform _orientation;


    private float xAngle;
    private float yAngle;

    public void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    void LateUpdate()
    {
        _distance -= Input.mouseScrollDelta.y * .1f;
        _distance = Mathf.Clamp(_distance, 1f, 30f);

        xAngle += Input.GetAxis("Mouse X") * _xSens * Time.deltaTime;
        yAngle -= Input.GetAxis("Mouse Y") * _ySens * Time.deltaTime;

        //xAngle += _xSens * Time.deltaTime;
        //yAngle -= _ySens * Time.deltaTime;

        yAngle = Mathf.Clamp(yAngle, _minYAngle, _maxYAngle);

        Vector3 direction = new Vector3(0, 0, -_distance);
        Quaternion rotation = Quaternion.Euler(yAngle, xAngle, 0);


        transform.position = _target.position + Vector3.up * _yOffset + rotation * direction;

        transform.LookAt(_target.position + Vector3.up * _yOffset);

        Vector3 lookDir = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
        _orientation.forward = lookDir;
    }
}
