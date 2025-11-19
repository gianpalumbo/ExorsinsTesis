using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

public class DEMOEmissor : MonoBehaviour
{
    public event Func<float, bool> OnSkillUse;
    public float cost = 5f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.U)) 
        {
            bool canUseSkill = OnSkillUse != null && OnSkillUse.Invoke(cost);
            Debug.Log($"intento usar skill con {cost} de mana y pude? {canUseSkill}");
        }
    }
}
