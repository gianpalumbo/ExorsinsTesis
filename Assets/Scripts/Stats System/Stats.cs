using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Enumeración de todas las estadísticas disponibles. Osea, los nombres
public enum EStat
{
//AGREGO MIS ESTADISTICAS
    Strength,
    Health,
    Speed,
    Defense,
    CriticalChance,    
}

// Representa una estadística con nivel, coste y valor de juego.
[System.Serializable]
public struct Stats
{
    public int Level;             // Nivel actual
    public int BaseCost;          // Coste base
    public float CostMultiplier;  // Multiplicador de coste

    public float BaseValue;       // Valor en nivel 1
    public float ValuePerLevel;   // Incremento por nivel

    // Calcula el coste actual
    public int CurrentCost => Mathf.FloorToInt(BaseCost * Mathf.Pow(CostMultiplier, Level));

    // Calcula el valor actual de la stat
    public float Value => BaseValue + Level * ValuePerLevel;

    // Intenta subir de nivel descontando puntos
    //availablePoints Puntos de los que dispone el jugador, true si se subió de nivel, false si no había puntos suficientes
    public bool TryLevelUp(ref int availablePoints)
    {
        int cost = CurrentCost;
        if (availablePoints < cost) return false;

        availablePoints -= cost;
        Level++;
        return true;
    }
}
