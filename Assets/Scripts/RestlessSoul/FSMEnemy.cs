using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FSMEnemy : MonoBehaviour
{
    #region States
    BaseState currentState;
    public IdleStateEnemy idleStateEnemy = new IdleStateEnemy();
    public AlertedStateEnemy alertedStateEnemy = new AlertedStateEnemy();
    public FollowStateEnemy followStateEnemy = new FollowStateEnemy();
    public AttackStateEnemy attackStateEnemy = new AttackStateEnemy();
    public OnHitStateEnemy onHitStateEnemy = new OnHitStateEnemy();
    #endregion

    [Header("References")]
    public Animator anim;
    [SerializeField] Transform target;
    public Rigidbody enemyRB;
    public Entity player;
    public PlayerMVC playerMVC;
    ModelPlayer _modelPlayer;

    [Header("Values")]
    public float normalSpeed = 8f;    
    public float _currentSpeed;
    public float slowSpeed;
    private float stopSpeed = 0;
    public bool isSlowed = false;

    [SerializeField] private bool isDead = false;
    public float alertedRadius;
    public float followRadius;
    public float attackRadius;
    [HideInInspector] public bool canFollow;

    public float CooldownFollow 
    {
        get => coolDownFollow;
        set => coolDownFollow = Mathf.Clamp(coolDownFollow, 0, maxCDFollow);
    }

    float coolDownFollow;
    [SerializeField] float maxCDFollow = 0.7f;
    //float _distanceToPlayer;
    //public float DistanceToPlayer { get { return _distanceToPlayer; } }
    //Vector3 _dirToTarget;
    //public Vector3 DirToTarget { get { return _dirToTarget; } }
    public float maxAttackCD = 1.5f;
    float counterAttack = 0;
    public float dmg;
    float rotationSpeed = 5f;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        enemyRB = GetComponent<Rigidbody>();

        if (_modelPlayer != null) Debug.Log("ENCONTRE MODEL");

        currentState = idleStateEnemy;

        currentState.OnEnter(this);
        _currentSpeed = normalSpeed;
        canFollow = true;
        isDead = false;

        slowSpeed = normalSpeed * 0.5f;
    }

    private void Start()
    {
        _modelPlayer = playerMVC.Model;
        Debug.Log(_modelPlayer);
    }

    public void DebugCurrentState()
    {
        Debug.Log($"Estoy en {currentState}");
    }

    public void LookAtPlayer()
    {
        Quaternion lookRotation = Quaternion.LookRotation(DirToTarget());
        lookRotation = new Quaternion(0, lookRotation.y, 0, lookRotation.w);
        Quaternion smoothRotation = Quaternion.Slerp(enemyRB.rotation, lookRotation, rotationSpeed * Time.fixedDeltaTime);
        enemyRB.MoveRotation(smoothRotation);
    } 

    private void Update()
    {
        DistanceToPlayer();
        DirToTarget();

        currentState.Execute(this);
        //DebugCurrentState();
    }

    public void CanFollowFalse() { canFollow = false; }
    public void CanFollowTrue() { canFollow = true; }

    public bool CounterFollowBool() => CooldownFollow == maxCDFollow;
    public void CounterFollowTime() => CooldownFollow += Time.deltaTime;
    public void RestartCounterFollow() => CooldownFollow = 0;
    public float DistanceToPlayer() => Vector3.Distance(transform.position, target.position);
    public Vector3 DirToTarget() => (target.position - transform.position).normalized;

    public void SwitchState(BaseState state)
    {
        currentState.OnExit(this);
        currentState = state;
        currentState.OnEnter(this);
    }

    //public void DisableMovement() { moveSpeed = 0; } 
    //public void EnableMovement() { moveSpeed = _originalMoveSpeed; }


    //CHELO WAS HERE. el problema con el SLOW este, al devolver el movimiento lo pone con la velocidad normal
    //MI SOLUCION: UN METODO QUE DETECTE UN BOOLEANO Y LO CAMBIE EN EL UPDATE
    public void DisableMovement() { _currentSpeed = stopSpeed; }
    public void EnableMovement()
    {
        //_currentSpeed = normalSpeed;
        if (isSlowed) _currentSpeed = slowSpeed;
        else _currentSpeed = normalSpeed;
    }


    public bool IsDead() => isDead;

    public void DisableColliderAndGravity() 
    {
        isDead = true;

        if (TryGetComponent(out Collider collider)) collider.enabled = false;
        if (TryGetComponent(out Rigidbody rigidbody))
        {
            rigidbody.useGravity = false;
            rigidbody.velocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
            rigidbody.Sleep();
        }
    }
    //public IEnumerator SwitchStateDelayed(float seconds, BaseState state)
    //{
    //    yield return new WaitForSeconds(seconds);
    //    SwitchState(state);
    //}

    public void DamagePlayerAnimMethod()
    {
        if (_modelPlayer != null)
        {
            _modelPlayer.TakeDamage(dmg);
        }
        else
        {
            Debug.LogWarning("ModelPlayer no encontrado en PlayerMVC.");
        }
    }

    public bool AttackPlayer()
    {
        if (counterAttack != maxAttackCD)
        {
            counterAttack += Time.deltaTime;
            counterAttack = Mathf.Clamp(counterAttack, 0, maxAttackCD);
            return false;
        }
        else return true;

        //CHELO WAS HERE
        //counterAttack += Time.deltaTime;
        //if (counterAttack >= maxAttackCD)
        //{
        //    counterAttack = 0f;
        //    return true;
        //}
        //return false;

    }
    public void ResetCounterAttack() => counterAttack = 0;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, alertedRadius);

        Gizmos.color = new Color(1f, 0.5f, 0f, 1f);
        Gizmos.DrawWireSphere(transform.position, followRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }

    private void OnTriggerEnter(Collider other)
    {
        //if (other.GetComponent<SwordCollider>()) SwitchState(onHitStateEnemy);

        if (isDead == true) return;
        else if (other.GetComponent<SwordCollider>()) SwitchState(onHitStateEnemy);
    }



    //CHELO WAS HERE
    public void ResetFSM()
    {
        // restablezco los triggers
        isDead = false;        
        //normalSpeed = _currentSpeed;
        _currentSpeed = normalSpeed;


        canFollow = true;
        // pongo el estado en Idle y ejecuto OnEnter
        //currentState.OnExit(this);
        //currentState = idleStateEnemy;
        //currentState.OnEnter(this);

        if (currentState != null) currentState.OnExit(this);
        currentState = idleStateEnemy;
        if (currentState != null) currentState.OnEnter(this);
    }

    //public void ChangeTarget(string newtarget)
    //{
    //    target = newtarget;
    //}
}
