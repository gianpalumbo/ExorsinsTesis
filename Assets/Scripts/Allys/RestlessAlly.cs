using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IA2;
using System;
using JetBrains.Annotations;
using Unity.VisualScripting;

public class RestlessAlly : MonoBehaviour
{

    public enum RSInputs { IDLE, FOLLOWPLAYER, FOLLOWENEMY, ATTACK, HITSTUN, DIE }
    [SerializeField] private EventFSM<RSInputs> _myFsm;

    private Rigidbody _myRb;
    [SerializeField] Animator _anim;
    [SerializeField] EnemyLife myLife;
    [SerializeField] GameObject _target;
    [SerializeField] GameObject _player;
    #region Steering
    Vector3 _currentVelocity;
    float steeringRadius = 1f;
    #endregion

    //Radius Attack and Follow
    float counter;
    [SerializeField] float followEnemyRadius, attackRadius, checkForEnemiesRadius;
    [SerializeField] float _speed = 2f, _currentSpeed, _rotationSpeed = 8f, _currentRotation;
    [SerializeField] float dmg, distanceFromPlayer, lifeTimeinSeconds = 60f;

    [SerializeField] bool isOnEnemyFollowRange, isOnAttackRange, isDeath, animationFinished;

    private void Awake()
    {
        _currentSpeed = _speed;
        _currentRotation = _rotationSpeed;
        counter = 0f;
        //LE AGREGO EL ONHITTRUE Y CUANDO TERMINO DE HACER LA ANIMACION SE VUELVE FALSE ASI NO VUELVE A ONHIT

        _myRb = GetComponent<Rigidbody>();
        _anim = GetComponent<Animator>();

        //CHELO WAS HERE: LE TENGO QUE PASAR UNA REFERENCIA DE PLAYER O NO HACE NADA
        _player = FindObjectOfType<PlayerLife>().gameObject; 

        #region States Declaration
        //var thinking = new State<RSInputs>("THINKING");
        var idle = new State<RSInputs>("IDLE");
        var followPlayer = new State<RSInputs>("FOLLOWPLAYER");
        var followEnemy = new State<RSInputs>("FOLLOWENEMY");
        var attack = new State<RSInputs>("ATTACK");
        var hitstun = new State<RSInputs>("HITSTUN");
        var die = new State<RSInputs>("DIE");
        #endregion
        #region StateConfigurer States currently 9 States
        StateConfigurer.Create(idle)
            .SetTransition(RSInputs.FOLLOWPLAYER, followPlayer)
            .SetTransition(RSInputs.FOLLOWENEMY, followEnemy)
            .SetTransition(RSInputs.DIE, die)
            .Done();
        StateConfigurer.Create(followPlayer)
            .SetTransition(RSInputs.FOLLOWENEMY, followEnemy)
            .SetTransition(RSInputs.IDLE, idle)
            .SetTransition(RSInputs.DIE, die)
            .Done();
        StateConfigurer.Create(followEnemy)
            .SetTransition(RSInputs.ATTACK, attack)
            .SetTransition(RSInputs.FOLLOWPLAYER, followPlayer)
            .SetTransition(RSInputs.IDLE, idle)
            .SetTransition(RSInputs.DIE, die)
            .Done();
        StateConfigurer.Create(attack)
            .SetTransition(RSInputs.HITSTUN, hitstun)
            .SetTransition(RSInputs.ATTACK, attack)
            .SetTransition(RSInputs.FOLLOWENEMY, followEnemy)
            .Done();
        StateConfigurer.Create(hitstun)
            .SetTransition(RSInputs.DIE, die)
            .SetTransition(RSInputs.FOLLOWPLAYER, followPlayer)
            .SetTransition(RSInputs.FOLLOWENEMY, followEnemy)
            .SetTransition(RSInputs.ATTACK, attack)
            .Done();

        StateConfigurer.Create(die)
            .Done();
        #endregion
        #region StatesLogicAndTransitions
        //IDLE
        idle.OnEnter += x => { _anim.SetBool("Idle", true); };
        idle.OnUpdate += () =>
        {
            if (isOnEnemyFollowRange)
            {
                SendInputToFSM(RSInputs.FOLLOWENEMY);
            }
            else if (DistanceToParameter(transform, _player.transform) >= distanceFromPlayer)
            {
                SendInputToFSM(RSInputs.FOLLOWPLAYER);
            }
        };
        idle.OnExit += x => { _anim.SetBool("Idle", false); };
        //FOLLOWPLAYER
        followPlayer.OnEnter += x => { _anim.SetBool("Follow", true); };
        followPlayer.OnFixedUpdate += () =>
        {
            Vector3 steerToPlayer = GetSteerToParameter(_player.transform, _currentSpeed);
            _myRb.MovePosition(transform.position + steerToPlayer); //ME MUEVO HACIA EL PLAYER
        };
        followPlayer.OnUpdate += () =>
        {
            LookAtParameterOnY(_player.transform);
            if (DistanceToParameter(transform, _player.transform) <= distanceFromPlayer)
            {
                SendInputToFSM(RSInputs.IDLE);
            }
            if (isOnEnemyFollowRange)
            {
                SendInputToFSM(RSInputs.FOLLOWENEMY);
            }
        };
        followPlayer.OnExit += x => { _anim.SetBool("Follow", false); };
        //FOLLOWENEMY
        followEnemy.OnEnter += x => { _anim.SetBool("Follow", true); };
        followEnemy.OnFixedUpdate += () =>
        {
            //CHELO WAS HERE: PROBLEMA NULO, SI VA CAMINANDO Y MUERE EL ENEMIGO NO SABE QUE HACER Y EXPLOTA TODO
            if (_target != null)
            {
                Vector3 steerToEnemy = GetSteerToParameter(_target.transform, _currentSpeed);
                _myRb.MovePosition(transform.position + steerToEnemy);
            }
        };
        followEnemy.OnUpdate += () =>
        {
            if (_target == null || !_target.activeInHierarchy)
            {
                SendInputToFSM(RSInputs.FOLLOWPLAYER);
                return;
            }

            LookAtParameterOnY(_target.transform);
            if (isOnAttackRange) { SendInputToFSM(RSInputs.ATTACK); }

            //LookAtParameterOnY(_target.transform);
            //if (isOnAttackRange)
            //{
            //    SendInputToFSM(RSInputs.ATTACK);
            //}
            //else if (_target == null)
            //    SendInputToFSM(RSInputs.FOLLOWPLAYER);
        };
        followEnemy.OnExit += x => { _anim.SetBool("Follow", false); };
        //ATTACK
        attack.OnEnter += x => { _anim.SetTrigger("Attack"); };
        attack.OnUpdate += () =>
        {
            if (!animationFinished) return;
            //SI LA ANIMACION TERMINO, PERO SIGO EN EL RANGO DE ATAQUE, VUELVO A ATACAR
            if (isOnAttackRange)
            {
                SendInputToFSM(RSInputs.ATTACK);
            }
            else
            {
                SendInputToFSM(RSInputs.FOLLOWENEMY);
            }
            if (_target == null)
            {
                SendInputToFSM(RSInputs.FOLLOWPLAYER);
            }
        };
        attack.OnExit += x => animationFinished = false;
        //DIE
        die.OnEnter += x =>
        {
            _anim.SetTrigger("Death");
            DisableMovement();
        };
        #endregion

        _myFsm = new EventFSM<RSInputs>(idle);
    }

    public void SendInputToFSM(RSInputs inp)
    {
        Debug.Log(inp);
        _myFsm.SendInput(inp);
    }
    void ConditionChecker()
    {
        // Si no hay target o el target está muerto, busca uno nuevo
        if (_target == null || !_target.activeInHierarchy || _target.GetComponent<EnemyLife>()?.Life <= 0)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, checkForEnemiesRadius);
            GameObject closestEnemy = null;
            float closestDistance = Mathf.Infinity;

            foreach (var hit in hits)
            {
                if (hit.gameObject == this.gameObject) continue; // Evita self-target

                //CHELO WAS HERE: CON EL TAG EVITO QUE SE PEGUEN ENTRE ELLOS
                if (!hit.CompareTag("Enemy")) continue;

                EnemyLife enemy = hit.GetComponent<EnemyLife>();
                if (enemy != null && enemy.Life > 0)
                {
                    float dist = Vector3.Distance(transform.position, hit.transform.position);
                    if (dist < closestDistance)
                    {
                        closestDistance = dist;
                        closestEnemy = enemy.gameObject;
                    }
                }
            }
            _target = closestEnemy; // Será null si no hay enemigos válidos
        }

        if (_target != null)
        {
            isOnEnemyFollowRange = Vector3.Distance(transform.position, _target.transform.position) <= followEnemyRadius;
            isOnAttackRange = Vector3.Distance(transform.position, _target.transform.position) <= attackRadius;
        }
        else
        {
            isOnEnemyFollowRange = false;
            isOnAttackRange = false;
        }
        isDeath = myLife.Life <= 0;
    }
    private void Update()
    {
        _myFsm.Update();
        ConditionChecker();
        if (GenericCounter(lifeTimeinSeconds))
            SendInputToFSM(RSInputs.DIE);
    }
    private void FixedUpdate()
    {
        _myFsm.FixedUpdate();
    }
    void LateUpdate()
    {
        _myFsm.LateUpdate();
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, followEnemyRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }

    #region VariousMethods
    public void DisableMovement()
    {
        _currentSpeed = 0;
        _rotationSpeed = 0;
        //gameObject.layer = 10;
    }
    public void EnableMovement()
    {
        _currentSpeed = _speed;
        _rotationSpeed = _currentRotation;
        //gameObject.layer = 10;
    }
    Vector3 GetSteerToParameter(Transform parameter, float speed)
    {
        //DIR TO PLAYER
        Vector3 dirToTarget = new Vector3(parameter.transform.position.x, 0, parameter.transform.position.z) - new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 desiredVelocity = dirToTarget.normalized * speed * Time.fixedDeltaTime;// velocidad deseada = direccion normalizada * aceleracion
        Vector3 steering = desiredVelocity - _currentVelocity;// correccion de velocidad = velocidad deseada - velocidad actual
        _currentVelocity += steering;// a la velocidad actual se le suma la correccion 
        return _currentVelocity *= Mathf.Clamp01(dirToTarget.magnitude / steeringRadius);
    }
    float DistanceToParameter(Transform myTransform, Transform parameter)
    {
        return Vector3.Distance(myTransform.position, parameter.position);
    }
    Vector3 GetDir() => _player.transform.position - transform.position;
    void LookAtParameterOnY(Transform param)
    {
        Vector3 dir = (param.position - transform.position).normalized;
        dir.y = 0;
        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
        }
    }
    public void DamagePlayerAnimMethod()
    {
        //if (_player != null && isOnAttackRange) _player.Model.TakeDamage(dmg);
        if (_player != null && isOnAttackRange) _target.GetComponent<EnemyLife>().TakeDamage(dmg);
    }
    void AnimationFinishedFalse() { animationFinished = false; }
    void AnimationFinishedTrue() { animationFinished = true; }
    bool GenericCounter(float time)
    {
        //variable que va a ir de cero a time, mientras que no es igual a time no se vuelve true
        if (counter != time)
        {
            //Debug.Log($"tiempo de contador: {counter}");
            counter += Time.deltaTime;
            counter = Mathf.Clamp(counter, 0, time);
            return false;
        }
        else
        {
            Debug.Log("termine");
            return true;
        }
    }
    #endregion
}
