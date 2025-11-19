using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorUIManager : MonoBehaviour
{
    public static CursorUIManager Instance { get; private set; }
    //ya esta implementado en: SkillWheelUI, PlayerLife, SanctuaryCanvas, PauseManager, KarmicMenu, MenuButton

    // Diccionario que mapea ID, requiere desbloqueo
    private readonly Dictionary<string, bool> requests = new Dictionary<string, bool>(); //diccionario de bools

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }

    public void RequestCursorState(bool unlock, string id)
    {
        requests[id] = unlock;
        UpdateCursor();
    }

    public void ReleaseCursorRequest(string id)
    {
        if (requests.Remove(id))
            UpdateCursor();
    }

    private void UpdateCursor()
    {
        bool any = false;
        foreach (var kv in requests) //kv = KeyValue
        {
            if (kv.Value)
            {
                any = true;
                break;
            }
        }

        if (any)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
