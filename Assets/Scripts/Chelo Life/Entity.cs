using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Entity : MonoBehaviour, IDamageable, ISpeedSlower
{
    [Header("Life")]
    [SerializeField] protected float maxLife = 100f;
    [SerializeField] protected float life;

    //[Range(0f, 1f)] protected float slowFactor = 0.5f; //NO SE DEJA VER POR LOS DEMAS NI EN PUBLICO

    public float Life
    {
        get { return life; }
        set
        {
            life = Mathf.Clamp(value, 0, maxLife); // el valor siempre esta entre 0 y maxLife
        }
    }

    public float MaxLife
    {
        get { return maxLife; }
        set { maxLife = value; }
    }

    protected virtual void Awake()
    { Life = maxLife; }
    
    public virtual void TakeDamage(float damage) // metodo para restar daño a la vida
    {
        Life -= damage; // se utiliza el setter de la propiedad Life para aplicar la reduccion
        Debug.Log("vida restante: " + Life);

        if (Life <= 0)
        {
            //Debug.Log("entity murio");
        }
    }

    protected virtual void Update()
    { }

    public virtual void SpeedSlower()
    { }

    public virtual void SpeedReset()
    { }

    public virtual void Knockback(Vector3 vector, float knock)
    {
        
    }

    //protected float GetActualLife()
    //{
    //    return Life;
    //}
}
