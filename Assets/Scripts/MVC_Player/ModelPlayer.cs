using System;

using UnityEngine;

public class ModelPlayer
{
    Rigidbody _rigidbody;
    Transform _transform;
    Transform _pivot;
    float _speed;
    float _jumpStrenght;
    float _rollStrenght;
    float _maxLife;
    float _life;
    bool _isGrounded, _canRoll;
    float _counterRoll, _rollCD;
    float _sensitivity, _clampViewY;
    float _stamina, _maxStamina;
    public float Stamina
    {
        get => _stamina;
        private set => Mathf.Clamp(value, 0, _stamina);
    }
    UnityEngine.Vector2 _turn;
    float _mana, _manaPerSecond;
    public static float _maxMana;
    SkillManager _skillManager;

    #region Events and actions
    //public event Action EventJump;
    public event Action<bool> EventLand;
    public event Action EventIdle;
    //public event Action<float, float> EventWalk;
    public event Action<float> EventRun;
    public event Action EventTakeDamage;
    public event Action EventDeath;
    //public event Action<float> EventRoll;

    public event Action<float> OnRechargingMana;
    #endregion

    #region Values for Movement

    UnityEngine.Vector3 inputDir = UnityEngine.Vector3.zero;
    public Transform cameraTransform;
    public CheloCamera cameraOrbit;
    public float turnSpeed = 10f;

    #endregion

    public ModelPlayer(Rigidbody rigidbody, Transform transform, float speed,
                        float jumpStrenght, float rollStrenght, float maxLife, float life,
                        bool isGrounded, bool canRoll, float counterRoll, float rollCD,
                        float sensitivity, float clampViewY, UnityEngine.Vector2 turn, Transform pivot, float stamina, float maxStamina,
                        Transform cameraTransform, CheloCamera cameraOrbit, float mana, float manaPerSecond, float maxMana, SkillManager skillManager)
    {
        _rigidbody = rigidbody;
        _transform = transform;
        _speed = speed;
        _jumpStrenght = jumpStrenght;
        _rollStrenght = rollStrenght;
        _maxLife = maxLife;
        _life = maxLife;
        _isGrounded = isGrounded;
        _canRoll = canRoll;
        _counterRoll = counterRoll;
        _rollCD = rollCD;
        _sensitivity = sensitivity;
        _clampViewY = clampViewY;
        _turn = turn;
        _pivot = pivot;
        _stamina = stamina;
        _maxStamina = maxStamina;

        _mana = mana;
        _manaPerSecond = manaPerSecond;
        _maxMana = maxMana;
        _skillManager = skillManager;

        if (_skillManager != null)
        {
            //Debug.Log("Me suscribi a SkillManager");
            skillManager.OnSkillUse += CheckMana;
        }

        _mana = _maxMana;

        this.cameraTransform = cameraTransform;
        this.cameraOrbit = cameraOrbit;

        ResetStamina();
    }
    public void Idle() { if (EventIdle != null) EventIdle(); }

    bool CheckMana(float cost) //EL TEMA ES QUE ACA LE RESTAS SIN IMPORTAR SI QUEDA MENOR A 0
    {
        var futureMana = _mana - cost;

        if (futureMana < 0)
        {
            //Debug.Log($"intento usar skill con {cost} de mana y no pude");
            return false; //SI NO PUEDO RESTAR MANA NO HAGO SKILL
        }
        else
        {
            //Debug.Log($"intento usar skill con {cost} de mana y pude");
            _mana -= cost;
            return true; //SI TENGO MANA SUFICIENTE RESTO Y HAGO SKILL
        }
    }
    public void RechargeMana()
    {
        if (_mana >= _maxMana)
        {
            _mana = _maxMana;
        }
        else
        {
            OnRechargingMana?.Invoke(_manaPerSecond);
            _mana += Time.deltaTime * _manaPerSecond;
            //Debug.Log($"mana: {_mana}");
        }
    }

    #region New New Movement
    public void CalculateMovement()
    {
        if (ServiceLocator.Instance.GetDependency<ControllerPlayer>().isAttacking)
            return;

        var h = Input.GetAxisRaw("Horizontal");
        var v = Input.GetAxisRaw("Vertical");

        if (h == 0 && v == 0)
        {
            EventRun?.Invoke(0f);
        }

        //if (cameraOrbit.lockTarget != null)
        //{
        //    // Siempre mirar al enemigo
        //    var toEnemy = cameraOrbit.lockTarget.position - _transform.position;
        //    toEnemy.y = 0;

        //    Quaternion rot = Quaternion.LookRotation(toEnemy.normalized);
        //    _transform.rotation = Quaternion.Slerp(_transform.rotation, rot, turnSpeed * Time.deltaTime);

        //    // Ejes relativos al enemigo
        //    Vector3 fwd = toEnemy.normalized;                 // adelante hacia el enemigo
        //    Vector3 rght = Vector3.Cross(Vector3.up, fwd);    // derecha relativa al enemigo

        //    // Movimiento estilo strafe (adelante, atrás, izquierda, derecha alrededor del enemigo)
        //    inputDir = (fwd * v + rght * h).normalized;

        //    Debug.DrawRay(_transform.position, inputDir * 2, Color.red); // para debug
        //}
        //else
        //{
        // Movimiento normal relativo a la cámara
        Vector3 fwd = Vector3.Scale(cameraTransform.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 rght = Vector3.Scale(cameraTransform.right, new Vector3(1, 0, 1)).normalized;
        inputDir = (fwd * v + rght * h).normalized;

        if (inputDir.sqrMagnitude > 0.01f)
        {
            Quaternion rot = Quaternion.LookRotation(inputDir);
            _transform.rotation = Quaternion.Slerp(_transform.rotation, rot, turnSpeed * Time.deltaTime);
        }

        Debug.DrawRay(_transform.position, inputDir * 2, Color.green); // para debug
        //}
    }

    public void NewRun(float hor, float ver)
    {
        if (ServiceLocator.Instance.GetDependency<ControllerPlayer>().isAttacking)
            return;

        var velY = _rigidbody.velocity.y;

        if (inputDir.sqrMagnitude > 0.01f)
        {
            _rigidbody.velocity = inputDir * _speed + Vector3.up * velY;

            // Usar el input crudo para animaciones (más realista que inputDir.magnitude)
            float velocityParam = new Vector2(hor, ver).magnitude;
            EventRun?.Invoke(velocityParam);
        }
        else
        {
            _rigidbody.velocity = new Vector3(0, velY, 0);
            EventRun?.Invoke(0f);
        }
    }
    #endregion

    #region Stamina
    public bool StaminaBarCheck(float neededStamina) => _stamina > neededStamina;
    //public void UsingStamina() => _stamina -= Time.fixedDeltaTime * 10;
    public void UsingStamina() => _stamina -= Time.fixedDeltaTime * 20;
    public void DashUsesStamina(float usedStamina) { _stamina -= usedStamina; }
    public void StaminaRechargePS()
    {
        if (_stamina != _maxStamina) _stamina += Time.fixedDeltaTime * 40;
        _stamina = Mathf.Clamp(_stamina, 0, _maxStamina);
    }
    public void ResetStamina()
    {
        _stamina = _maxStamina;
    }
    public void DebugStamina() { Debug.Log(_stamina); }
    public float UpdateStaminaBar() => _stamina;
    #endregion
    public bool LandUpdate() //Para chequear si estoy en el piso
    {
        //LOGICA DEL SALTO
        if (Physics.Raycast(_transform.position, UnityEngine.Vector3.down, .25f))
        {
            _isGrounded = true;
            EventLand(_isGrounded);
            return _isGrounded;
        }
        else
        {
            _isGrounded = false;
            EventLand(_isGrounded);
            return _isGrounded;
        }
    }
    public void TakeDamage(float dmg)
    {
        _life -= dmg;

        if (_life <= 0)
        {
            if (EventDeath != null)
                EventDeath();
            Debug.Log("GAME OVER");
        }

        if (EventTakeDamage != null)
            EventTakeDamage();
    }

    public void UnsubscribeFromSkillManager()
    {
        if (_skillManager != null)
        {
            //Debug.Log("Me desuscribi a SkillManager");
            _skillManager.OnSkillUse -= CheckMana;
        }
    }
}
