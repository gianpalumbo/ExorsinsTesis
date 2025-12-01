using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PointsTMP : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI _pointsText;
    [SerializeField] private TextMeshProUGUI _pointsTextMenu;

    private void Awake()
    {
        ServiceLocator.Instance.RegisterDependency<PointsTMP>(this);

        if (PointsManager.Instance != null) PointsManager.Instance.SetUI(_pointsText, _pointsTextMenu);
    }

    private void OnDestroy()
    {
        ServiceLocator.Instance.RemoveDependency<PointsTMP>();
    }
}
