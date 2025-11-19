using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheloCamera : MonoBehaviour
{
    [Header("Target")] public Transform target;

    [Header("Orbit Settings")]
    public float distance = 5f, minDistance = 1.5f, maxDistance = 10f;
    public float zoomSpeed = 2f, xSpeed = 120f, ySpeed = 120f;
    public float yMinLimit = -20f, yMaxLimit = 80f;

    [Header("Control Sensitivity")]
    public float mouseSensitivity = 3f;
    public float joystickSensitivity = 100f;
    public float joystickThreshold = 0.1f;

    [Header("Collision Avoidance")]
    public LayerMask ignoreLayers;
    public float sphereCastRadius = 0.5f;
    public float raycastBuffer = 0.2f;

    [Header("Smooth Collision Transition")]
    public float collisionEnterTime = 0.05f;
    public float collisionExitTime = 0.3f;

    [Header("LockOn Settings (FOV)")]
    public KeyCode lockOnKey = KeyCode.Tab;
    public KeyCode nextTargetKey = KeyCode.E, prevTargetKey = KeyCode.Q;
    public float lockOnSpeed = 5f, lockOnRange = 10f, viewAngle = 60f;
    [Tooltip("Altura que se sube la camara al taggear")]
    public float lockOnHeight = 1.5f;
    public float yOffset;

    public Transform lockTarget { get; private set; }

    float xAngle, yAngle;
    float currHeightOffset;
    readonly Collider[] _overlapBuffer = new Collider[32];
    public bool isResting = false;

    //suavizado distancia
    float currentDistance;
    float currentDistVelocity;

    //Datos Gizmos
    Vector3 lastPivot;
    Vector3 lastDir;
    float lastDistance;
    RaycastHit lastHit;
    bool didHit;

    private IHealthBar currentHealthBar; //MOSTRAR BARRA DE VIDA ENEMIGA




    //CHELO WAS HERE: LIMITES LOCK
    [Header("LockOn Pitch Limits (grados)")]
    public float lockOnPitchMin = -30f;   // angulo mínimo de pitch permitidos
    public float lockOnPitchMax = 45f;    // angulo máximo de pitch permitidos



    public bool canRotate = false; //CHELO WAS HERE: evita rotar la camara para cuando esta viendo las habilidades, si usas isResting tampoco usa la posicion y eso si nos interesa


    private void Awake()
    {
        if (SoundManager.Instance == null)
            SoundManager.Instance.ChangeToCaveAmbience();

        //gameObject.SetActive(false);
        ServiceLocator.Instance.RegisterDependency<CheloCamera>(this);
    }

    void Start()
    {
        var e = transform.eulerAngles;
        xAngle = e.y; yAngle = e.x;
        Cursor.lockState = CursorLockMode.Locked;
        currentDistance = distance;
    }

    void Update()
    {
        //AGUS ADDON CAMARA ROTA AUN EN PAUSE
        if (isResting) return;
        if (target == null) return;
        if (lockTarget != null && Vector3.Distance(target.position, lockTarget.position) > lockOnRange) lockTarget = null;
        
        if (lockTarget != null) //CHELOWASHERE: dejar de lockear si el objetivo muere
        {
            var life = lockTarget.GetComponent<EnemyLife>();
            if (life != null && life.IsDead)  // suponer que exposes esa propiedad
            {
                lockTarget = null;
                UpdateHealthBarTarget();  // para que la barra deje de mostrarse
            }
        }        
        HandleLockInput(); //Inputs de Lock
        HandleRotationInput(); //Inputs de Rotacion
        yAngle = Mathf.Clamp(yAngle, yMinLimit, yMaxLimit); //clamp general cuando no esta Lockeando
    }

    void LateUpdate()
    {
        if (isResting || target == null) return;

        //ZoomCamera();
        SmoothHeightOffset();
        PositionCamera();
    }

    void HandleLockInput() //Inputs de Lock
    {
        if (Input.GetKeyDown(lockOnKey) || (Input.GetButtonDown("JoystickL3"))) lockTarget = (lockTarget == null ? FindTarget() : null);
        if (lockTarget != null)
        {
            if (Input.GetKeyDown(nextTargetKey)) lockTarget = CycleTarget(true);
            if (Input.GetKeyDown(prevTargetKey)) lockTarget = CycleTarget(false);
        }
        //if (lockTarget != null) //y la layer del target NO tiene BOSS, NO VA A MOSTRAR LA BARRA DE VIDA, rompe el boss eso
        //UpdateHealthBarTarget(); //MOSTRAR BARRA DE VIDA ENEMIGA
        if (!(currentHealthBar is BossLife)) UpdateHealthBarTarget(); //MOSTRAR BARRA DE VIDA SI NO ES UN BOSS

    }

    void UpdateHealthBarTarget() //MOSTRAR BARRA DE VIDA ENEMIGA
    {
        if (currentHealthBar != null)
        {
            HealthBarVisibilityManager.Instance.ReleaseShow(currentHealthBar, "LockOn");
        }

        if (lockTarget != null)
        {
            currentHealthBar = lockTarget.GetComponent<IHealthBar>();
            if (currentHealthBar != null) HealthBarVisibilityManager.Instance.RequestShow(currentHealthBar, "LockOn");
        }
    }

    void HandleRotationInput() //Inputs de Rotacion
    {
        float finalX, finalY;

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        float joyX = Input.GetAxis("RightStickX");
        float joyY = Input.GetAxis("RightStickY");



        if (canRotate == true && lockTarget == null) return; //CHELO WAS HERE: evita rotar la camara para cuando esta viendo las habilidades, si usas isResting tampoco usa la posicion y eso si nos interesa



        finalX = Mathf.Abs(mouseX) > joystickThreshold ? mouseX * mouseSensitivity : joyX * joystickSensitivity * Time.deltaTime;
        finalY = Mathf.Abs(mouseY) > joystickThreshold ? mouseY * mouseSensitivity : joyY * joystickSensitivity * Time.deltaTime;

        if (lockTarget == null)
        {
            xAngle += finalX;
            yAngle -= finalY;
        }
        else //Si esta lockeado la camara gira en base a la posicion del enemigo
        {
            var pivot = target.position + Vector3.up * lockOnHeight;
            var aim = lockTarget.position + Vector3.up * lockOnHeight;
            var desired = Quaternion.LookRotation((aim - pivot).normalized);
            //xAngle = Mathf.LerpAngle(xAngle, desired.eulerAngles.y, lockOnSpeed * Time.deltaTime);
            //yAngle = Mathf.LerpAngle(yAngle, desired.eulerAngles.x, lockOnSpeed * Time.deltaTime);

            float desiredYaw = desired.eulerAngles.y;
            float desiredPitch = desired.eulerAngles.x;
            //Ajustar el desiredPitch para que no pase los limites definidos
            desiredPitch = ClampAngle(desiredPitch, lockOnPitchMin, lockOnPitchMax);
            xAngle = Mathf.LerpAngle(xAngle, desiredYaw, lockOnSpeed * Time.deltaTime);
            yAngle = Mathf.LerpAngle(yAngle, desiredPitch, lockOnSpeed * Time.deltaTime);
        }
    }
    //Funcion auxiliar para clamping de angulos teniendo en cuenta wrap (0-360)
    float ClampAngle(float angle, float min, float max)
    {        
        angle = NormalizeAngle(angle); //convierte angulo a rango -180 a +180        
        float clamped = Mathf.Clamp(angle, min, max); //clamp dentro del intervalo
        return clamped;
    }
    float NormalizeAngle(float angle)
    {
        angle %= 360f;
        if (angle < -180f) angle += 360f;
        if (angle > 180f) angle -= 360f;
        return angle;
    }

    void ZoomCamera()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        distance = Mathf.Clamp(distance - scroll * zoomSpeed, minDistance, maxDistance);
    }

    void SmoothHeightOffset()
    {
        float targetOffset = (lockTarget != null ? lockOnHeight : 0f);
        currHeightOffset = Mathf.Lerp(currHeightOffset, targetOffset, lockOnSpeed * Time.deltaTime);
    }

    void PositionCamera()
    {
        Vector3 pivot = target.position + Vector3.up * currHeightOffset;
        Quaternion rot = Quaternion.Euler(yAngle, xAngle, 0f);
        Vector3 dir = rot * Vector3.forward;
        float desiredDist = distance;

        lastPivot = pivot;
        lastDir = -dir;

        Ray ray = new Ray(pivot, -dir);
        if (Physics.Raycast(ray, out RaycastHit hit, distance + raycastBuffer, ~ignoreLayers))
        {
            desiredDist = Mathf.Max(minDistance, hit.distance - raycastBuffer);
            lastHit = hit;
            didHit = true;
        }
        else
        {
            didHit = false;
        }

        float smoothTime = (desiredDist < currentDistance) ? collisionEnterTime : collisionExitTime;
        currentDistance = Mathf.SmoothDamp(currentDistance, desiredDist, ref currentDistVelocity, smoothTime);
        Vector3 desiredPos = pivot - dir * currentDistance;

        transform.rotation = rot;
        transform.position = desiredPos;
        lastDistance = currentDistance;
    }

    Transform FindTarget()
    {
        int count = Physics.OverlapSphereNonAlloc(target.position, lockOnRange, _overlapBuffer);
        Vector3 pivot = target.position + new Vector3(0, yOffset, 0) ; //AGUS WAS HERE
        Vector3 fwdXZ = new Vector3(transform.forward.x, 0, transform.forward.z).normalized;
        Transform best = null;
        float bestDistSqr = float.MaxValue;
        float halfAngle = viewAngle * 0.5f;

        for (int i = 0; i < count; i++)
        {
            var col = _overlapBuffer[i];
            if (!col.CompareTag("Enemy")) continue;

            Vector3 dir = col.transform.position - pivot;
            dir.y = 0;
            var dirNorm = dir.normalized;
            float angle = Vector3.Angle(fwdXZ, dirNorm);
            if (angle > halfAngle) continue;

            float distSqr = dir.sqrMagnitude;
            if (distSqr < bestDistSqr)
            {
                bestDistSqr = distSqr;
                best = col.transform;
            }
        }
        return best;
    }

    Transform CycleTarget(bool next)
    {
        int count = Physics.OverlapSphereNonAlloc(target.position, lockOnRange, _overlapBuffer);
        Vector3 pivot = target.position;
        Vector3 fwd = new Vector3(transform.forward.x, 0, transform.forward.z).normalized;
        float halfAngle = viewAngle * 0.5f;

        var entries = new List<(Transform t, float angle)>();
        entries.Capacity = count;

        for (int i = 0; i < count; i++)
        {
            var col = _overlapBuffer[i];
            if (!col.CompareTag("Enemy")) continue;

            Vector3 dir = col.transform.position - pivot;
            dir.y = 0;
            var dirNorm = dir.normalized;
            float angle = Vector2.SignedAngle(
                new Vector2(fwd.x, fwd.z),
                new Vector2(dirNorm.x, dirNorm.z));

            if (Mathf.Abs(angle) <= halfAngle)
                entries.Add((col.transform, angle));
        }

        if (entries.Count == 0) return null;

        entries.Sort((a, b) => a.angle.CompareTo(b.angle));

        int idx = entries.FindIndex(e => e.t == lockTarget);
        if (idx < 0) idx = 0;
        idx = (idx + (next ? 1 : -1) + entries.Count) % entries.Count;
        return entries[idx].t;
    }

    void OnDrawGizmos()
    {
        if (target == null) return;

        // Lock-on visuals
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(target.position, lockOnRange);

        Vector3 fwd = new Vector3(transform.forward.x, 0, transform.forward.z).normalized;
        Vector3 left = Quaternion.Euler(0, -viewAngle * 0.5f, 0) * fwd;
        Vector3 right = Quaternion.Euler(0, viewAngle * 0.5f, 0) * fwd;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(target.position, target.position + left * lockOnRange);
        Gizmos.DrawLine(target.position, target.position + right * lockOnRange);

        if (lockTarget != null)
        {
            var p = target.position + Vector3.up * currHeightOffset;
            var e = lockTarget.position + Vector3.up * currHeightOffset;
            Gizmos.color = Color.yellow; Gizmos.DrawSphere(p, .05f);
            Gizmos.color = Color.red; Gizmos.DrawSphere(e, .05f);
            Gizmos.color = Color.magenta; Gizmos.DrawLine(p, e);
        }

        // Raycast Gizmo
        Gizmos.color = Color.green;
        Gizmos.DrawRay(lastPivot, lastDir * lastDistance);
        if (didHit)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(lastHit.point, 0.1f);
        }

        // SphereCast Gizmo
        Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
        if (didHit)
        {
            Gizmos.DrawWireSphere(lastPivot, sphereCastRadius);
            Gizmos.DrawWireSphere(lastHit.point, sphereCastRadius);
            Gizmos.DrawLine(lastPivot, lastHit.point);
        }
    }

    private void OnDestroy()
    {
        ServiceLocator.Instance.RemoveDependency<CheloCamera>();
    }
}