using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow
{    
    //Controls _controls;

    Camera _camera;
    float _mouseSensitivity;
    float _distance;
    float _hitOffSet;

    Transform _playerTransform; //target

    float _mouseX;
    float _mouseY;

    Vector3 _camPos;
    Vector3 _direction;
    Ray _ray;
    RaycastHit _raycastHit;
    bool _isCameraBlocked;
    Transform _rotationYCam;

    ControllerPlayer _controller;


    private float normalDistance; //esta es para actualizar la distancia de camara desde el script de playerMVC

    //private float horizontalRotation;

    //public CameraFollow(Transform player, Camera camera, float mouseSensitivity,
    //    float distance, float hitOffSet, Controls controls, Transform rotationYCam)
    //{
    //    _player = player;
    //    _camera = camera;
    //    _mouseSensitivity = mouseSensitivity;
    //    _distance = distance;
    //    _hitOffSet = hitOffSet;
    //    _controls = controls;

    //    //AGREGADO
    //    _rotationYCam = rotationYCam;
    //}

    //public CameraFollow (Transform player, Camera camera, float mouseSensitivity, float distance, float hitOffSet, Transform rotationYCam, float mouseX, float mouseY, ControllerPlayer controller)
    public CameraFollow(Transform player, Camera camera, float mouseSensitivity, float distance, float hitOffSet, Transform rotationYCam, ControllerPlayer controller)
    {
        _controller = controller;
        _playerTransform = player;
        _camera = camera;
        _mouseSensitivity = mouseSensitivity;
        _distance = distance;
        _hitOffSet = hitOffSet;
        _rotationYCam = rotationYCam;

        //_mouseX = mouseX;
        //_mouseY = mouseY;

    }

    public void UpdateValues(float newdistance)
    {
        normalDistance = newdistance;
    }

    public void CameraStart()
    {
        //Cursor.lockState = CursorLockMode.Locked;
        _mouseX = _controller.GetMouseX();
        _mouseY = _controller.GetMouseY();
    }

    //public void ArtificialUpdate()
    //{
    //    CameraDistance();
    //}

    //public void CameraDistance()
    //{        
    //    _distance = normalDistance;
    //}

    public void CameraFixedUpdate()
    {
        //CameraDistance();
        _ray = new Ray(_playerTransform.transform.position, _direction);
        _isCameraBlocked = Physics.SphereCast(_ray, 0.1f, out _raycastHit, _distance);
    }

    public void CameraLateUpdate()
    {
        _playerTransform.transform.position = _playerTransform.position;

        //AGREGADO
        _rotationYCam.transform.position = _rotationYCam.position;

        #region Cam Movement

        _mouseX += _controller.GetMouseX() * _mouseSensitivity * Time.deltaTime;
        _mouseY += _controller.GetMouseY() * _mouseSensitivity * Time.deltaTime;

        //_mouseX += Input.GetAxisRaw("Mouse X") * _mouseSensitivity * Time.deltaTime;
        //_mouseY += Input.GetAxisRaw("Mouse Y") * _mouseSensitivity * Time.deltaTime;

        if (_mouseX >= 360 || _mouseX <= -360)
        {
            _mouseX -= 360 * Mathf.Sign(_mouseX);
        }

        _mouseY = Mathf.Clamp(_mouseY, -89f, 30f);

        //ORIGINAL
        //_player.transform.rotation = Quaternion.Euler(-_mouseY, _mouseX, 0f);

        //MODIFICADO
        _playerTransform.transform.rotation = Quaternion.Euler(0, _mouseX, 0f);

        //AGREGADO
        _rotationYCam.transform.rotation = Quaternion.Euler(-_mouseY, _mouseX, _rotationYCam.transform.rotation.z);
        //_rotationYCam.transform.localRotation = Quaternion.Euler(-_mouseY, _mouseX, 0f);
        
        #endregion


        #region Spring Arm

        //ORIGINAL
        //_direction = -_player.transform.forward;
        //if (_isCameraBlocked)
        //    _camPos = _raycastHit.point - _direction * _hitOffSet;

        //else _camPos = _player.transform.position + _direction * _distance;

        _direction = -_rotationYCam.transform.forward;

        if (_isCameraBlocked) _camPos = _raycastHit.point - _direction * _hitOffSet;

        else _camPos = _rotationYCam.transform.position + _direction * _distance;

        //distancia camara-jugador y sigue al jugador
        _camera.transform.position = _camPos;

        //ORIGINAL
        //_camera.transform.LookAt(_player.transform.position);

        //MODIFICADO LOOK AT ROTA A LA CAMARA EN SU PROPIO EJE
        _camera.transform.LookAt(_rotationYCam.transform.position);

        #endregion

    }
    //public float GetHorizontalRotation()
    //{
    //    return horizontalRotation;
    //}

    #region GIZMOS

    public void OnDrawGizmoscam()
    {
        var position = _playerTransform.transform.position;

        Gizmos.color = Color.blue;

        Gizmos.DrawSphere(position, 0.1f);

        Gizmos.DrawSphere(_camPos, 0.1f);

        Gizmos.color = Color.red;

        Gizmos.DrawLine(position, _camPos);
    }

    #endregion
     
    

















    /*
     * SCRIPT DE HOY ORIGINAL QUE TENIA MIO, AGREGAR MONOBEHAVIOUR
     * 
     * 
    public Transform player;
    public float distance = 4f; // Distancia normal
    public float aimDistance = 3f; // Distancia al apuntar
    public float height = 2f;
    public float rotationSpeed = 5f;
    public float sensitivity = 1f;
    public float cameraSmoothness = 0.1f;
    public float verticalAngleMin = -10f;
    public float verticalAngleMax = 60f;

    private float currentDistance; // La distancia actual (para interpolar)
    private float horizontalRotation;
    private float verticalRotation;
    private float currentVerticalRotation;
    private Controls playerInput;

    void Start()
    {
        // Inicializamos la distancia actual con la distancia normal
        currentDistance = distance;
        //playerInput = player.GetComponent<Controls>();

        horizontalRotation = transform.eulerAngles.y;
        verticalRotation = transform.eulerAngles.x;
    }

    void LateUpdate()
    {
        //// Si estamos apuntando, ajustamos la distancia de la cámara
        //if (playerInput.IsAiming())
        //{
        //    currentDistance = Mathf.Lerp(currentDistance, aimDistance, cameraSmoothness); // Acercar la cámara
        //}
        //else
        //{
        //    currentDistance = Mathf.Lerp(currentDistance, distance, cameraSmoothness); // Alejar la cámara
        //}

        RotateCamera();

        Vector3 desiredPosition = CalculateCameraPosition();
        transform.position = Vector3.Lerp(transform.position, desiredPosition, cameraSmoothness);

        // Mirar al jugador
        transform.LookAt(player.position + Vector3.up * height);
    }

    private void RotateCamera()
    {
        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed * sensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed * sensitivity;

        horizontalRotation += mouseX;
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, verticalAngleMin, verticalAngleMax);

        currentVerticalRotation = Mathf.Lerp(currentVerticalRotation, verticalRotation, cameraSmoothness);
    }

    private Vector3 CalculateCameraPosition()
    {
        Quaternion rotation = Quaternion.Euler(currentVerticalRotation, horizontalRotation, 0f);
        Vector3 offset = new Vector3(0, height, -currentDistance); // Usar currentDistance para interpolar
        return player.position + rotation * offset;
    }

    */




    /*
    public Transform player; // El transform del jugador al que sigue la cámara

    public float distance = 4f; // Distancia desde la cámara al jugador
    public float height = 2f; // Altura de la cámara respecto al jugador
    public float rotationSpeed = 5f; // Velocidad de rotación de la cámara
    public float sensitivity = 1f; // Sensibilidad del mouse para la rotación
    public float cameraSmoothness = 0.1f; // Suavidad en la rotación de la cámara
    public float verticalAngleMin = -10f; // Ángulo mínimo de rotación vertical
    public float verticalAngleMax = 60f; // Ángulo máximo de rotación vertical

    private float horizontalRotation;
    private float verticalRotation;
    private float currentVerticalRotation;

    void Start()
    {
        // Inicializar las rotaciones según la posición inicial de la cámara
        horizontalRotation = transform.eulerAngles.y;
        verticalRotation = transform.eulerAngles.x;
    }

    void LateUpdate()
    {
        // Rotar la cámara según el movimiento del mouse
        RotateCamera();

        // Mantener la cámara alineada con el jugador
        Vector3 desiredPosition = CalculateCameraPosition();
        transform.position = Vector3.Lerp(transform.position, desiredPosition, cameraSmoothness);

        // Siempre apuntar hacia el jugador
        transform.LookAt(player.position + Vector3.up * height);
    }

    private void RotateCamera()
    {
        // Obtener la entrada del ratón, ajustada por la sensibilidad
        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed * sensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed * sensitivity;

        // Ajustar la rotación horizontal (en el eje Y)
        horizontalRotation += mouseX;

        // Ajustar la rotación vertical (en el eje X) y limitarla para evitar giros completos
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, verticalAngleMin, verticalAngleMax);

        // Aplicar la rotación suave usando Lerp para una transición más fluida
        currentVerticalRotation = Mathf.Lerp(currentVerticalRotation, verticalRotation, cameraSmoothness);
    }

    private Vector3 CalculateCameraPosition()
    {
        // Calcular la posición deseada de la cámara basado en la rotación y el offset
        Quaternion rotation = Quaternion.Euler(currentVerticalRotation, horizontalRotation, 0f);
        Vector3 offset = new Vector3(0, height, -distance);
        return player.position + rotation * offset;
    }
    */






    /*
    public Transform player;        // Referencia al transform del jugador
    public float distance = 5.0f;   // Distancia entre la cámara y el jugador
    public float height = 2.0f;     // Altura de la cámara respecto al jugador
    public float sensitivity = 5.0f; // Sensibilidad del movimiento del mouse
    public float rotationSmoothness = 0.1f; // Suavidad de la rotación

    private float currentX = 0.0f;  // Rotación actual en el eje X
    private float currentY = 0.0f;  // Rotación actual en el eje Y
    private Vector3 desiredPosition;

    void Start()
    {
        if (!player)
        {
            Debug.LogError("Player Transform is not assigned in the CameraFollowPlayer script.");
        }
    }

    void Update()
    {
        // Obtener la rotación en los ejes X e Y basadas en el movimiento del mouse
        currentX += Input.GetAxis("Mouse X") * sensitivity;
        currentY -= Input.GetAxis("Mouse Y") * sensitivity;

        // Restringir la rotación en Y (vertical) para evitar rotaciones invertidas
        currentY = Mathf.Clamp(currentY, -60, 60);

        // Calcular la posición deseada de la cámara
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        desiredPosition = player.position + rotation * new Vector3(0, height, -distance);
    }

    void LateUpdate()
    {
        // Mover la cámara a la posición deseada
        transform.position = desiredPosition;

        // Rotar la cámara para que apunte hacia el jugador
        transform.LookAt(player.position + Vector3.up * height);
    }
    */










    /*
    //asigno objetivo
    public Transform target;
    public Camera myCamera;

    //si quiero offset de camara
    //public float offset = 3;
    //Vector3 _offsetDir;

    //tamaño de la camara en una variable
    public float cameraView;

    private void Awake()
    {
        //el target sera el que tenga el tag player, si es que no esta puesto
        if (target == null) target = GameObject.FindGameObjectWithTag("Player").transform;

        //if (myCamera == null) myCamera = GetComponent<Camera>();
        //la camara va a ser la main, la main tiene esta propiedad del .main
        if (myCamera == null) myCamera = Camera.main;

    }
    // Start is called before the first frame update
    void Start()
    {
        //el tamaño de la camara la igualo a la variable que quiero
        Camera.main.orthographicSize = cameraView;
    }

    // Update is called once per frame
    void Update()
    {
        if (target != null)
        {
            //_offsetDir = target.right;
            transform.position = new Vector3(target.position.x, target.position.y, transform.position.z);

            //Con offset
            //transform.position = new Vector3(target.position.x, target.position.y, transform.position.z) + offset * _offsetDir;
        }

        //si no hay personaje, el script deja de funcionar para ahorrar recursos
        else Destroy(this);
    }
    */
}