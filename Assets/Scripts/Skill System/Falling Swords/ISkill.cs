using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISkill
{
    //void Activate(GameObject user); //si quisiera decirle de donde sale, animacion, rigid, velocidades, etc
    //void Activate();
    void Activate(MonoBehaviour runner);

    bool IsCooling();
}

public abstract class SkillBase : ISkill
{
    /*
    //protected float _cooldown;
    //public abstract float Cooldown { get; }
    public abstract float Cooldown { get; }
    protected bool _isCooling = false; //_isCooling impide que se use la habilidad mientras está activa
    //private Coroutine _cooldownCoroutine;
    public bool CanUse => !_isCooling; //retorna el booleano
    public void Activate(MonoBehaviour runner) //usar Activate(), se lanza la corutina que controla el cooldown
    {
        if (!CanUse) return;
        OnActivate(runner);
        //_cooldownCoroutine = runner.StartCoroutine(CooldownRoutine());
        runner.StartCoroutine(CooldownRoutine());
    }

    protected abstract void OnActivate(MonoBehaviour runner);
    private IEnumerator CooldownRoutine() //Como cada skill instancia su propia corutina, y éstas se ejecutan en SkillManager, no interfieren entre sí aunque cambies de habilidad
    {
        _isCooling = true;
        yield return new WaitForSeconds(Cooldown);
        _isCooling = false;
    }
    */

    protected float _cooldown = 1f;  // valor por default
    protected bool _isCooling = false;
    //public float Cooldown => _cooldown;
    public abstract float Cooldown { get; }  // propiedad

    public void Activate(MonoBehaviour runner)
    {
        if (_isCooling) return;
        OnActivate(runner);
        runner.StartCoroutine(CooldownRoutine());
    }

    private IEnumerator CooldownRoutine()
    {
        _isCooling = true;
        yield return new WaitForSeconds(Cooldown);
        _isCooling = false;
    }
    protected abstract void OnActivate(MonoBehaviour runner);

    public bool IsCooling()
    {
        return _isCooling;
    }
}

//USO UN CONSTRUCTOR DESDE EL SKILLMANAGER PARA PASARLE LAS COSAS Y QUE EL ACTIVATE() LO HAGA
//public class FireSkill : ISkill
public class SlowSkill : SkillBase
{
    ////public void Activate()
    //public void Activate(MonoBehaviour runner)
    //{
    //    Debug.Log("Casting fire skill!");
    //}

    public override float Cooldown => _cooldown;  // propiedad override
    private GameObject _slowPrefab;
    private Transform _playerTransform;
    private Transform _slowSpawner;

    public SlowSkill(
    GameObject slowPrefab,
    Transform playerTransform,
    Transform slowSpawner,
    float cdFire)
    {
        _slowPrefab = slowPrefab;
        _playerTransform = playerTransform;
        _slowSpawner = slowSpawner;
        _cooldown = cdFire;
    }

    //public void Activate(MonoBehaviour runner)
    //{
    //    Debug.Log("Casting fire skill!");

    //    //StartCoroutine(SpawnForTime());
    //    //StartCoroutine(SpawnForDuration());
    //    //runner.StartCoroutine(SpawnForDuration(runner));
    //    UnityEngine.Object.Instantiate(_slowPrefab, _slowSpawner.position, _slowSpawner.rotation);
    //}
    
    protected override void OnActivate(MonoBehaviour runner)
    {
        UnityEngine.Object.Instantiate(_slowPrefab, _slowSpawner.position, _slowSpawner.rotation);

    }
}

//public class IceSkill : ISkill
public class SwordFallingSkill : SkillBase
{
    private readonly GameObject _bulletPrefab;
    private readonly Transform _playerTransform;
    private readonly Transform _spawner;
    private readonly float _spawnHeight;
    private readonly float _outerRadius;
    private readonly float _innerRadius;
    private readonly float _spawnInterval;
    private readonly float _spawnDuration;
    private readonly int _spawnCount;


    public override float Cooldown => _cooldown;  // propiedad override


    public SwordFallingSkill(
        GameObject bulletPrefab,
        Transform playerTransform,
        Transform spawner,
        float spawnHeight,
        float outerRadius,
        float innerRadius,
        float spawnInterval,
        float spawnDuration,
        int spawnCount,
        float cdIce)
    {
        _bulletPrefab = bulletPrefab;
        _playerTransform = playerTransform;
        _spawner = spawner; //NO LO ESTOY USANDO
        _spawnHeight = spawnHeight;
        _outerRadius = outerRadius;
        _innerRadius = innerRadius;
        _spawnInterval = spawnInterval;
        _spawnDuration = spawnDuration;
        _spawnCount = spawnCount;
        _cooldown = cdIce;
    }

    //public void Activate()
    //public void Activate(MonoBehaviour runner)
    //{
    //    Debug.Log("Casting ice skill!");

    //    //StartCoroutine(SpawnForTime());
    //    //StartCoroutine(SpawnForDuration());
    //    //runner.StartCoroutine(SpawnForDuration(runner));
    //    runner.StartCoroutine(SpawnForDuration());
    //}

    protected override void OnActivate(MonoBehaviour runner)
    {
        UnityEngine.Object.Instantiate(_bulletPrefab, _spawner.position, _spawner.rotation);
    }

    //private IEnumerator SpawnForDuration(MonoBehaviour runner)
    //{
    //    float elapsed = 0f;
    //    while (elapsed < _spawnDuration)
    //    {
    //        //SpawnBullet(runner);
    //        SpawnBullet();
    //        yield return new WaitForSeconds(_spawnInterval);
    //        elapsed += _spawnInterval;
    //    }
    //}

    //private IEnumerator SpawnForDuration()
    //{
    //    Debug.Log("SPAWN ");
    //    float elapsed = 0f;
    //    while (elapsed < _spawnDuration)
    //    {
    //        for (int i = 0; i < _spawnCount; i++)
    //        {
    //            // Ángulo fijo en círculo
    //            float theta = 2 * Mathf.PI * i / _spawnCount;
    //            // Radio aleatorio ponderado por área entre inner y outer
    //            float r = Mathf.Sqrt(Random.Range(_innerRadius * _innerRadius, _outerRadius * _outerRadius));
    //            SpawnBullet(theta, r);
    //            yield return null;
    //        }
    //        yield return new WaitForSeconds(_spawnInterval);
    //        elapsed += _spawnInterval;
    //    }
    //}

    //LO QUE ESTA PASANDO ES QUE VUELVE A ENTRAR CUANDO FINALIZA
    //private IEnumerator SpawnForDuration()
    //{
    //    Debug.Log("SPAWN");
    //    //float elapsed = 0f;
    //    //while (elapsed < _spawnDuration)
    //    //{
    //        for (int i = 0; i < _spawnCount; i++)
    //        {
    //            // angulo fijo en circulo
    //            float theta = 2 * Mathf.PI * i / _spawnCount;
    //            // radio aleatorio ponderado por area entre inner y outer
    //            //float r = Mathf.Sqrt(Random.Range(_innerRadius * _innerRadius, _outerRadius * _outerRadius));
    //            float r = Mathf.Sqrt(Random.Range(_innerRadius, _outerRadius));
    //            SpawnBullet(theta, r);
    //            //yield return null; //PUEDO HACERLE QUE INSTANCIE DE FORMA INSTANTANEA
    //            yield return new WaitForSeconds (0.2f); //PUEDO HACERLE QUE ESPERE UNOS FRAMES ANTES DE VOLVER A INSTANCIAR

    //    }
    //    //    yield return new WaitForSeconds(_spawnInterval);
    //    //    elapsed += _spawnInterval;
    //    //}
    //}

    //private void SpawnBullet()
    //{
    //    // Ángulo aleatorio
    //    float theta = Random.Range(0f, Mathf.PI * 2f);
    //    // Radio ponderado por área para densidad uniforme
    //    float r = Mathf.Sqrt(Random.Range(innerRadius * innerRadius, outerRadius * outerRadius));
    //    // Posición en XZ y altura fija
    //    Vector3 offset = new Vector3(Mathf.Cos(theta), 0f, Mathf.Sin(theta)) * r;
    //    Vector3 spawnPos = transform.position + offset + Vector3.up * spawnHeight;

    //    // Instancia la bala
    //    Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
    //}

    //private void SpawnBullet(MonoBehaviour runner)
    //{
    //    float theta = Random.Range(0f, Mathf.PI * 2f);
    //    float r = Mathf.Sqrt(Random.Range(_innerRadius * _innerRadius, _outerRadius * _outerRadius));
    //    Vector3 offset = new Vector3(Mathf.Cos(theta), 0f, Mathf.Sin(theta)) * r;
    //    Vector3 spawnPos = _spawner.position + offset + Vector3.up * _spawnHeight;
    //    UnityEngine.Object.Instantiate(_bulletPrefab, spawnPos, Quaternion.identity);
    //}

    //private void SpawnBullet()
    //{
    //    // Ángulo y radio aleatorio ponderado por área para densidad uniforme
    //    float theta = Random.Range(0f, Mathf.PI * 2f);
    //    float r = Mathf.Sqrt(Random.Range(_innerRadius * _innerRadius, _outerRadius * _outerRadius));
    //    Vector3 offset = new Vector3(Mathf.Cos(theta), 0f, Mathf.Sin(theta)) * r;

    //    // Centrar en la posición del jugador y elevar a la altura deseada
    //    Vector3 spawnPos = _playerTransform.position + Vector3.up * _spawnHeight + offset;

    //    // Instancia usando la referencia estática de UnityEngine.Object
    //    UnityEngine.Object.Instantiate(_bulletPrefab, spawnPos, Quaternion.identity);
    //}

    //private void SpawnBullet(float theta, float r)
    //{
    //    Vector3 offset = new Vector3(Mathf.Cos(theta), 0f, Mathf.Sin(theta)) * r;
    //    Vector3 spawnPos = _playerTransform.position + Vector3.up * _spawnHeight + offset;
    //    //UnityEngine.Object.Instantiate(_bulletPrefab, spawnPos, Quaternion.identity);
    //    Quaternion desiredRotation = Quaternion.Euler(-180f, 0f, 0f); // Example rotation (Euler angles)
    //    UnityEngine.Object.Instantiate(_bulletPrefab, spawnPos, desiredRotation);
    //}
}

public class FireballSkill : SkillBase
{
    GameObject _fireballPrefab;
    Transform _fireballSpawner;
    float _cdFireball;

    public FireballSkill(GameObject fireballPrefab, Transform fireballSpawner, float cdFireball)
    {
        _fireballPrefab = fireballPrefab;
        _fireballSpawner = fireballSpawner;
        _cdFireball = cdFireball;
    }

    public override float Cooldown => _cooldown;  // propiedad override

    protected override void OnActivate(MonoBehaviour runner)
    {
        //UnityEngine.Object.Instantiate(_fireballPrefab, _fireballSpawner.position, _fireballSpawner.rotation);
        UnityEngine.Object.Instantiate(_fireballPrefab, _fireballSpawner.position, Camera.main.transform.rotation);
    }
}