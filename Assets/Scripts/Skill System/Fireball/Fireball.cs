using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class Fireball : MonoBehaviour
{
    [SerializeField] private float _dmg = 10f;
    [SerializeField] private float _speed = 6f;
    [SerializeField] private float _timer = 3f;
    VisualEffect _fire;
    bool _hasCollide;
    private void Awake()
    {
        _fire = GetComponentInChildren<VisualEffect>();
    }
    void Start()
    {
        StartCoroutine(ExplodeByTime());
    }

    void Update()
    {
        //transform.position += (Vector3.forward * _speed * Time.deltaTime);
        if(!_hasCollide) transform.position += (transform.forward * _speed * Time.deltaTime);

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) return;
        IDamageable damageable = other.GetComponent<IDamageable>();
        if (other.CompareTag("Enemy"))
        {
            if (damageable != null)
            {
                damageable.TakeDamage(_dmg);
            }
        }
        if (!_hasCollide) Explode();
    }

    IEnumerator ExplodeByTime()
    {
        yield return new WaitForSeconds(5);
        if(!_hasCollide) Explode();
    }

    void Explode()
    {
        _hasCollide = true;
        _fire.SetBool("HasExplode", true);
        _fire.SendEvent("Explode");
        GetComponent<Collider>().enabled = false;
        Destroy(gameObject, 3);
    }

}
