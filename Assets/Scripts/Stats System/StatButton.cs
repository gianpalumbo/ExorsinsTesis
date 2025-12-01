using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controla un bot�n de mejora para una estad�stica concreta.
/// </summary>

public class StatButton : MonoBehaviour
{
    [Tooltip("Selecciona aqu� la estad�stica a mejorar")]
    public EStat StatKey;
    public Button UpgradeButton;
    public TextMeshProUGUI LevelText;
    public TextMeshProUGUI CostText;

    private PlayerStats playerStats;

    public TextMeshProUGUI pointsSanctuary;

    void Start()
    {
        playerStats = FindObjectOfType<PlayerStats>();
        if (playerStats == null) Debug.LogWarning("No se encontr� ning�n PlayerStats en la escena.");

        //UpgradeButton.onClick.RemoveAllListeners();
        //UpgradeButton.onClick.AddListener(OnClick);
        RefreshUI();
    }


    //CHELO WAS HERE: Refresco la UI por evento, y Start por si acaso
    //CHELO WAS HERE: Uso RefreshUISafely() por el orden de ejecucion, sino salta null
    private void OnEnable()
    {
        if (PointsManager.Instance != null) PointsManager.Instance.OnPointsChanged += HandlePointsChanged;
        RefreshUISafely();
    }
    private void OnDisable()
    {
        if (PointsManager.Instance != null) PointsManager.Instance.OnPointsChanged -= HandlePointsChanged;
    }
    private void OnDestroy()
    {
        if (PointsManager.Instance != null) PointsManager.Instance.OnPointsChanged -= HandlePointsChanged;
    }
    private void HandlePointsChanged(int newPoints) // Cuando cambian los puntos, se refresca la UI
    {

        RefreshUI();
    }
    public void RefreshUISafely()
    {
        if (PointsManager.Instance == null || UpgradeButton == null || LevelText == null || CostText == null) return;
        RefreshUI();
    }


    void RefreshUI() //Refresco la UI actualizando costos y el interactuable
    {
        int lvl = PointsManager.Instance.GetStatLevel(StatKey);
        int cost = PointsManager.Instance.GetStatCost(StatKey);

        pointsSanctuary.text = PointsManager.Instance.currentPoints.ToString();

        LevelText.text = $"Lvl {lvl}";
        CostText.text = cost > 0 ? $"{cost} pts" : "�";
        UpgradeButton.interactable = (cost > 0 && PointsManager.Instance.CurrentPoints >= cost);
    }

    public void OnClick()
    {
        //El bool intenta subir la stat
        bool canUpgrade = PointsManager.Instance.UpgradeStat(StatKey);

        //Si se subio la stat se refresca la UI
        if (canUpgrade) RefreshUI();

        // 3)Y se obliga al PlayerStats a recalcular sus variables para que el Inspector se actualice inmediatamente
        if (canUpgrade && playerStats != null) playerStats.RecalculateBonuses();
    }
}