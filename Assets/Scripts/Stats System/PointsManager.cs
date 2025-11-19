using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

/// <summary>
/// Gestiona los puntos del jugador y sus estad�sticas.
/// </summary>
public class PointsManager : MonoBehaviour
{
    public static PointsManager Instance { get; private set; }

    [SerializeField] private int currentPoints;
    [SerializeField] private TextMeshProUGUI _pointsText;
    [SerializeField] private TextMeshProUGUI _pointsTextMenu;

    //CHELO WAS HERE Nuevo event que se dispara cuando cambian los puntos
    public event Action<int> OnPointsChanged;  // podr�as pasar int si te sirve ver valor nuevo



    public void SetUI(TextMeshProUGUI pointsText, TextMeshProUGUI pointsTextMenu)
    {
        _pointsText = pointsText;
        _pointsTextMenu = pointsTextMenu;
        UIText();  // Actualizar la UI al asignar
    }

    private Dictionary<EStat, Stats> stats; // Diccionario de estad�sticas

    /// <summary>Propiedad controlada para puntos, actualiza UI al setear.</summary>
    public int CurrentPoints
    {
        get => currentPoints;
        private set
        {
            currentPoints = value;
            UIText();

            //CHELO WAS HERE: disparar evento
            OnPointsChanged?.Invoke(currentPoints);
        }
    }

    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Inicializa el diccionario de stats
        stats = new Dictionary<EStat, Stats>()
        {
            { EStat.Strength,       new Stats { Level = 1, BaseCost = 5,  CostMultiplier = 1.2f, BaseValue = 0f,   ValuePerLevel = 2f   } },
            { EStat.Health,         new Stats { Level = 1, BaseCost = 8,  CostMultiplier = 1.1f, BaseValue = 100f, ValuePerLevel = 20f  } },
            { EStat.Speed,          new Stats { Level = 1, BaseCost = 6,  CostMultiplier = 1.15f,BaseValue = 5f,   ValuePerLevel = 0.5f } },
            { EStat.Defense,        new Stats { Level = 1, BaseCost = 7,  CostMultiplier = 1.1f, BaseValue = 0f,   ValuePerLevel = 1f   } },
            { EStat.CriticalChance, new Stats { Level = 1, BaseCost = 10, CostMultiplier = 1.25f,BaseValue = 0f,   ValuePerLevel = 0.01f} },
        };
    }

    private void Start()
    {
        //EN EL JUEGO SI EMPIEZO CON 0
        //CurrentPoints = 0;
        UIText();

        //CHELO WAS HERE: dispara evento por si acaso para refrescar la UI
        OnPointsChanged?.Invoke(currentPoints);
    }

    public void AddPoints(int amount)
    {
        CurrentPoints += amount;
        //Debug.Log($"Puntos a�adidos: {amount} | Total: {CurrentPoints}");
    }

    public void SubtractPoints(int amount)
    {
        CurrentPoints = Mathf.Max(0, CurrentPoints - amount);
        //Debug.Log($"Puntos restados: {amount} | Total: {CurrentPoints}");
    }

    public void ResetPoints()
    {
        CurrentPoints = 0;
        //Debug.Log("Puntos reiniciados.");
    }

    private void UIText()
    {
        if (_pointsText != null) _pointsText.text = (currentPoints.ToString());
        if (_pointsTextMenu != null) _pointsTextMenu.text = (currentPoints.ToString());

    }

    // ��� M�todos para stats ���

    /// <summary>Intenta subir de nivel la stat; devuelve true si tuvo �xito.</summary>
    public bool UpgradeStat(EStat statKey)
    {
        if (!stats.TryGetValue(statKey, out Stats s)) return false;

        int cost = s.CurrentCost;
        if (CurrentPoints < cost) return false;

        SubtractPoints(cost);
        s.Level++;
        stats[statKey] = s;
        return true;
    }

    /// <summary>Obtiene el coste actual para la stat.</summary>
    public int GetStatCost(EStat statKey) => stats.TryGetValue(statKey, out Stats s) ? s.CurrentCost : -1;

    /// <summary>Obtiene el nivel actual de la stat.</summary>
    public int GetStatLevel(EStat statKey) => stats.TryGetValue(statKey, out Stats s) ? s.Level : 0;

    /// <summary>Obtiene el valor efectivo de la stat (da�o extra, vida m�x., etc.).</summary>
    public float GetStatValue(EStat statKey) => stats.TryGetValue(statKey, out Stats s) ? s.Value : 0f;
}