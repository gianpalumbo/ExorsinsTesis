using UnityEngine;

public class ViewPlayer
{
    ModelPlayer _model;
    Animator _animator;
    Transform _transform;

    float _smoothTime = 0.1f;

    private SmoothInput smoothX = new SmoothInput();
    private SmoothInput smoothY = new SmoothInput();

    public ViewPlayer(ModelPlayer model, Animator animator, Transform transform)
    {
        _model = model;
        _animator = animator;
        _transform = transform;

        _model.EventIdle += Idle;
        //_model.EventJump += Jump;
        //_model.EventLand += Land;
        //_model.EventRoll += Roll;
        //_model.EventWalk += Walk;
        _model.EventRun += NewRun;
        _model.EventTakeDamage += TakeDamage;
    }

    public void Idle()
    {
        //_animator.applyRootMotion = true;

        _animator.SetBool("Idle", true);
    }

    //public void Walk(float horizontal, float vertical)
    //{
    //    float x = Mathf.Clamp(Input.GetAxis("Horizontal"), -0.5f, 0.5f);
    //    float y = Mathf.Clamp(Input.GetAxis("Vertical"), -0.5f, 0.5f);

    //    _animator.SetFloat("movX", x, _smoothTime, Time.deltaTime);
    //    _animator.SetFloat("movY", y, _smoothTime, Time.deltaTime);
    //}
    //public void Run(float horizontal, float vertical)
    //{
    //    float x = Input.GetAxis("Horizontal");
    //    float y = Input.GetAxis("Vertical");

    //    _animator.SetFloat("movX", x, _smoothTime, Time.deltaTime);
    //    _animator.SetFloat("movY", y, _smoothTime, Time.deltaTime);
    //}

    public void Walk(float horizontal, float vertical)
    {
        UpdateMovement(horizontal * 0.5f, vertical * 0.5f); // m�s lento
    }

    public void NewRun(float param)
    {
        //_animator.applyRootMotion = false;
        _animator.SetFloat("velocity", param, _smoothTime, Time.deltaTime);

        if (Input.GetKey(KeyCode.LeftShift))
        {
            _animator.speed = 1.5f;
        }
        else if (Input.GetKeyDown(KeyCode.P) && Input.GetKeyDown(KeyCode.Z))
        {
            _animator.speed = 3;
        }
        else
        {
            _animator.speed = 1;
        }
        //_animator.applyRootMotion = Camera.main.GetComponent<CheloCamera>().lockTarget == null; //SI LOCKTARGET NO ES NULL ENTONCES FALSE AL ROOTMOTION SINO TRUE
    }

    public void Run(float horizontal, float vertical)
    {
        UpdateMovement(horizontal, vertical); // velocidad normal
    }

    private void UpdateMovement(float horizontal, float vertical)
    {
        float smoothedX = smoothX.Update(horizontal, _smoothTime);
        float smoothedY = smoothY.Update(vertical, _smoothTime);

        _animator.SetFloat("movX", smoothedX);
        _animator.SetFloat("movY", smoothedY);
    }

    public void TakeDamage()
    {
        _animator.SetTrigger("onHit");
        Debug.Log("OUCH ESO DOLIO!");
    }
}
