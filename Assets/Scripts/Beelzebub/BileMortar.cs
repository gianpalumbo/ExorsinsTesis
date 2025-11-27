using UnityEngine;

public class BileMortar : MonoBehaviour
{
    [SerializeField] float dmg, maxDmg, dmgPerInterval, poisonTime = 3f;
    [SerializeField] float force = 10f;
    [SerializeField] float spread = 0.5f; // cuanto se dispersan los proyectiles

    [SerializeField] GameObject acidPool;

    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        dmg = Random.Range(dmg, maxDmg);
    }

    private void Start()
    {
        Destroy(gameObject, 5f);
    }

    public void Init(Transform mouth)
    {
        // Base direction hacia arriba
        Vector3 baseDir = mouth.up;

        // Agregar un poco de variación (spread)
        Vector3 randomOffset = new Vector3(
            Random.Range(-spread, spread),
            Random.Range(0f, spread * 0.5f), // menos variación vertical
            Random.Range(-spread, spread)
        );

        Vector3 finalDir = (baseDir + randomOffset).normalized;

        // Aplicar fuerza impulsiva
        rb.velocity = finalDir * force;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerLife>(out PlayerLife player))
        {
            player.TakeDamage(dmg / 2);
            player.PoisonPlayer(dmgPerInterval, poisonTime);
        }
        else if (other.gameObject.layer == 10)
        {
            Instantiate(acidPool, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
