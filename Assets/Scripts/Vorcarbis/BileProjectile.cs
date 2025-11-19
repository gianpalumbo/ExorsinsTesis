using System.Collections;
using UnityEngine;

public class BileProjectile : MonoBehaviour
{
    [SerializeField] float dmg, maxDmg, dmgPerInterval, poisonTime = 3f;
    Rigidbody rb;

    [SerializeField] GameObject bilePuddle;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        dmg = Random.Range(dmg, maxDmg);
    }

    // Inicializa el proyectil con objetivo y ángulo
    public void Init(Transform mouth, Transform target, float angle = 45f)
    {
        // Calculamos dirección horizontal
        Vector3 dir = target.position - mouth.position;
        Vector3 dirXZ = new Vector3(dir.x, 0, dir.z);

        float dist = dirXZ.magnitude;
        float yOffset = dir.y;

        float g = Mathf.Abs(Physics.gravity.y);
        float radAngle = angle * Mathf.Deg2Rad;

        // Fórmula para la velocidad necesaria
        float v2 = (g * dist * dist) /
                   (2 * (dist * Mathf.Tan(radAngle) - yOffset) * Mathf.Pow(Mathf.Cos(radAngle), 2));

        if (v2 <= 0)
        {
            Debug.LogWarning("No hay solución balística válida para ese ángulo/objetivo.");
            return;
        }

        float v = Mathf.Sqrt(v2);

        // Velocidad inicial en componentes
        Vector3 velocity = dirXZ.normalized * v * Mathf.Cos(radAngle);
        velocity.y = v * Mathf.Sin(radAngle);

        rb.velocity = velocity;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerLife>(out PlayerLife player))
        {
            player.TakeDamage(dmg / 2);
            player.PoisonPlayer(dmgPerInterval, poisonTime);
        }
        else if (other.gameObject.layer == 10) // Obstacles
        {
            var copy = Instantiate(bilePuddle, transform.position, Quaternion.identity);
        }
    }
}
