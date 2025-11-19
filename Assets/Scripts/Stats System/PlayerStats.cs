using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gestiona ataque y vida del jugador usando estadísticas del PointsManager.
/// </summary>
public class PlayerStats : MonoBehaviour
{
    //    [Header("Datos base de combate")]
    //    public float BaseAttack = 10f;

    //    [Header("Variables de vida")]
    //    public float CurrentHealth;    // Vida actual del jugador
    //    private float maxHealth;       // Vida máxima, calculada desde la stat

    //    public PointsManager2 pm;
    //    //private Enemy target; // Tu referencia al enemigo (ajusta según tu arquitectura)

    //    void Awake()
    //    {
    //        //pm = FindObjectOfType<PointsManager2>();

    //        // target = ... obtén la referencia a tu enemigo

    //        // Inicializar vida máxima desde la stat de Health
    //        //maxHealth = pm.GetStatValue(EStat.Health);
    //        //CurrentHealth = maxHealth;  // Al iniciar, vida completa
    //    }

    //    private void Start()
    //    {
    //        maxHealth = pm.GetStatValue(EStat.Health);
    //        CurrentHealth = maxHealth;  // Al iniciar, vida completa
    //    }

    //    /// <summary>
    //    /// Calcula el daño que infliges basado en tu Fuerza y ataque base.
    //    /// </summary>
    //    //public float CalculateDamage()
    //    //{
    //    //    float strengthBonus = pm.GetStatValue(EStat.Strength);
    //    //    float rawAttack = BaseAttack + strengthBonus;
    //    //    float defense = target != null ? target.Defense : 0f;

    //    //    // Fórmula básica: ataque - defensa, mínimo 1 de daño
    //    //    return Mathf.Max(1f, rawAttack - defense);
    //    //}

    //    /// <summary>
    //    /// Aplica daño al jugador y actualiza CurrentHealth.
    //    /// </summary>
    //    /// <param name="damage">Cantidad de daño recibida.</param>
    //    public void ReceiveDamage(float damage)
    //    {
    //        CurrentHealth -= damage;
    //        // Asegura que CurrentHealth no baje de 0
    //        CurrentHealth = Mathf.Max(0f, CurrentHealth);
    //        // Aquí podrías disparar eventos de muerte si CurrentHealth == 0
    //    }

    //    /// <summary>
    //    /// Cura al jugador y actualiza CurrentHealth.
    //    /// </summary>
    //    /// <param name="healAmount">Cantidad de vida a recuperar.</param>
    //    public void Heal(float healAmount)
    //    {
    //        CurrentHealth += healAmount;
    //        // Asegura que CurrentHealth no supere maxHealth
    //        CurrentHealth = Mathf.Min(CurrentHealth, maxHealth);
    //    }

    //    /// <summary>
    //    /// Llamar cuando la stat de Health sube de nivel para actualizar maxHealth.
    //    /// </summary>
    //    public void OnHealthStatUpgraded()
    //    {
    //        // Recalcula vida máxima y mantiene proporción de vida actual
    //        float percentage = CurrentHealth / maxHealth;
    //        maxHealth = pm.GetStatValue(EStat.Health);
    //        CurrentHealth = maxHealth * percentage;
    //    }
    //}

    [Header("Datos base de combate")]
    public float BaseAttack = 10f;

    [Header("Variables de vida")]
    public float CurrentHealth;

    [SerializeField] private float maxHealth;      // Vida máxima según la stat de Health
    [SerializeField] private float strengthBonus;  // Bonus de fuerza (stat Strength)
    [SerializeField] private float totalAttack;      // BaseAttack + strengthBonus

    void Start()
    {
        // Inicializamos CurrentHealth y maxHealth una vez al comienzo
        maxHealth = PointsManager.Instance.GetStatValue(EStat.Health);
        CurrentHealth = maxHealth;

        // También calculamos la fuerza bruta inicial
        RecalculateBonuses();
    }

    /// <summary>
    /// Recalcula únicamente las variables que queremos exponer:
    /// - strengthBonus
    /// - rawAttack
    /// - maxHealth (si cambió Health)
    /// </summary>
    public void RecalculateBonuses()
    {
        strengthBonus = PointsManager.Instance.GetStatValue(EStat.Strength);
        totalAttack = BaseAttack + strengthBonus;

        float newMaxHealth = PointsManager.Instance.GetStatValue(EStat.Health);

        if (!Mathf.Approximately(newMaxHealth, maxHealth))
        {
            float porcentaje = CurrentHealth / maxHealth;
            maxHealth = newMaxHealth;
            CurrentHealth = maxHealth * porcentaje;
        }
        else
        {
            maxHealth = newMaxHealth; // Si no cambió Health, simplemente actualizamos maxHealth para el Inspector
        }
    }

    /// <summary>
    /// Devuelve el daño que el jugador haría con los valores actuales (puede usarse en combate).
    /// </summary>
    public float CalculateDamage()
    {
        return Mathf.Max(1f, totalAttack); // Aquí rawAttack ya está actualizado porque llamamos a RecalculateBonuses() tras cada upgrade
    }

    public void ReceiveDamage(float damage)
    {
        CurrentHealth = Mathf.Max(0f, CurrentHealth - damage);
    }

    public void Heal(float healAmount)
    {
        CurrentHealth = Mathf.Min(CurrentHealth + healAmount, maxHealth);
    }
}
/*
    [Header("Datos base de combate")]
    public float BaseAttack = 10f;

    [Header("Variables de vida")]
    public float CurrentHealth;
    private float maxHealth;

    void Start()
    {
        // Inicializa vida con la stat de Health
        maxHealth = PointsManager.Instance.GetStatValue(EStat.Health);
        CurrentHealth = maxHealth;
    }

    /// <summary>Calcula el daño según Fuerza y ataque base.</summary>
    public float CalculateDamage()
    {
        float strengthBonus = PointsManager.Instance.GetStatValue(EStat.Strength);
        float rawAttack = BaseAttack + strengthBonus;
        return Mathf.Max(1f, rawAttack);
    }

    public void ReceiveDamage(float damage)
    {
        CurrentHealth = Mathf.Max(0, CurrentHealth - damage);
    }

    public void Heal(float healAmount)
    {
        CurrentHealth = Mathf.Min(CurrentHealth + healAmount, maxHealth);
    }

    /// <summary>
    /// Llama esto tras subir Health para recalcular la vida máxima preservando %.
    /// </summary>
    public void OnHealthStatUpgraded()
    {
        float pct = CurrentHealth / maxHealth;
        maxHealth = PointsManager.Instance.GetStatValue(EStat.Health);
        CurrentHealth = maxHealth * pct;
    }
}
*/
