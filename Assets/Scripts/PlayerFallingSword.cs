using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.VFX;

public class PlayerFallingSword : MonoBehaviour
{
    [SerializeField] float damage, dmgInterval;
    VisualEffect vfx;
    float counter;
    float _genCounter;

    #region OLD FALLING SWORD
    //[Header("Caida")]
    //public float delayBeforeFall = 1f;
    //public float downwardForce = 10f;

    //[Header("Explosion al impactar")]
    //public float explosionRadius = 3f;
    //[Tooltip("Mascara de capas para filtrar sólo enemigos.")]
    //public LayerMask enemyLayerMask;

    //[Header("VFX opcional")]
    //public GameObject explosionVFX;

    //private Rigidbody _rb;
    //private bool _hasFallen = false;

    //private void Awake()
    //{
    //    if (_rb == null) _rb = GetComponent<Rigidbody>();
    //    _rb.velocity = Vector3.zero;
    //    _rb.angularVelocity = Vector3.zero;
    //    _rb.useGravity = false;
    //}

    //private void Start()
    //{
    //    StartCoroutine(DelayedFall());
    //}

    //private IEnumerator DelayedFall() // espera X segundos y luego activa la gravedad y aplica impulso hacia abajo
    //{
    //    yield return new WaitForSeconds(delayBeforeFall);

    //    _rb.useGravity = true;
    //    _rb.AddForce(Vector3.down * downwardForce, ForceMode.Impulse);
    //    _hasFallen = true;
    //}

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (!_hasFallen) return;
    //    if (other.gameObject.layer != 12)
    //    {
    //        Debug.Log("IceSkill: golpea algo que no es de la layer");

    //        if (explosionVFX != null) Instantiate(explosionVFX, transform.position, Quaternion.identity); // Instanciar VFX si esta asignado

    //        // Daño en área
    //        //Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius, enemyLayerMask); SI TENGO UNA LAYER DE ENEMIGOS LA PUEDO USAR PARA FILTRARLOS DIRECTAMENTE
    //        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);
    //        foreach (var hit in hits)
    //        {
    //            Debug.Log("IceSkill: entre al foreach");
    //            IDamageable damageable = hit.GetComponent<IDamageable>();
    //            if (hit.CompareTag("Enemy"))
    //            {
    //                Debug.Log("ICESKILL: le pego a un enemigo");
    //                if (damageable != null) damageable.TakeDamage(damage);
    //            }
    //        }
    //        Destroy(gameObject);
    //    }
    //}

    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
    //    Gizmos.DrawWireSphere(transform.position, explosionRadius);
    //}
    #endregion

    private void Start()
    {
        vfx = GetComponent<VisualEffect>();
        Destroy(gameObject, vfx.GetFloat("Lifetime") + 2);
    }
    private void Update()
    {
        counter += Time.deltaTime;
        if (counter > vfx.GetFloat("Lifetime")) GetComponent<Collider>().enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (counter / vfx.GetFloat("Lifetime") < .2f) return;
        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
        {
            if (other.CompareTag("Enemy"))
            {
                damageable.TakeDamage(damage);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (counter / vfx.GetFloat("Lifetime") < .2f) return;
        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
        {
            if (GenericCounter(dmgInterval))
            {
                ResetCounter();
                if (other.CompareTag("Enemy"))
                {
                    damageable.TakeDamage(damage);
                }
            }
        }
    }

    bool GenericCounter(float time)
    {
        _genCounter += Time.deltaTime;
        return _genCounter >= time;
    }
    void ResetCounter() { _genCounter = 0; }
}
