using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UtilitiesAgus
{
    public static (bool inState, float t01, bool finished) GetAnimatorStateProgress(string stateName, Animator _anim)
    {
        var info = _anim.GetCurrentAnimatorStateInfo(0);

        bool inState = info.IsName(stateName);

        // Para la ventana us� t en [0..1]
        float t01 = Mathf.Clamp01(info.normalizedTime);

        // Para "termin�", NO uses % 1f: dej� el valor real (puede ser > 1 en no-loop)
        bool finished = inState && info.normalizedTime >= 1f;

        return (inState, t01, finished);
    }

    public static void ToggleCursor(bool cursorOn)
    {
        if (cursorOn)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            //Debug.Log("VISIBLE TRUE Y CURSOR NONE");
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            //Debug.Log("VISIBLE FALSE Y CURSOR LOCKED");
        }
    }

    public static void ToggleCanvasGroup(CanvasGroup cg, bool toggle)
    {
        cg.alpha = toggle ? 1f : 0f;
        cg.interactable = toggle;
        cg.blocksRaycasts = toggle;
    }
}
