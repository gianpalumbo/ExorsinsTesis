using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manager que controla la visibilidad de la barra de vida de los enemigos
/// </summary>
public class HealthBarVisibilityManager : MonoBehaviour
{    
    //Mismo sistema que que el cursorManagerUI. Le paso una contraseña o razon para mantenerse prendido
    public static HealthBarVisibilityManager Instance { get; private set; }
    private Dictionary<IHealthBar, HashSet<string>> requests = new Dictionary<IHealthBar, HashSet<string>>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void RequestShow(IHealthBar bar, string reason)
    {
        if (!requests.TryGetValue(bar, out var reasons))
        {
            reasons = new HashSet<string>();
            requests[bar] = reasons;
        }
        reasons.Add(reason);
        bar.ShowLifeBar();
    }

    public void ReleaseShow(IHealthBar bar, string reason)
    {
        if (requests.TryGetValue(bar, out var reasons))
        {
            reasons.Remove(reason);
            if (reasons.Count == 0)
            {
                requests.Remove(bar);
                bar.HideLifeBar();
            }
        }
    }
}
