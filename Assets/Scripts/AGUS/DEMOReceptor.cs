using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DEMOReceptor : MonoBehaviour
{
    [SerializeField] DEMOEmissor demoEmissor;
    public float mana;

    private void Start()
    {
        demoEmissor.OnSkillUse += CheckMana;
    }

    private void OnDestroy()
    {
        demoEmissor.OnSkillUse -= CheckMana;
    }

    bool CheckMana(float cost) //EL TEMA ES QUE ACA LE RESTAS SIN IMPORTAR SI QUEDA MENOR A 0
    {
        var futureMana = mana - cost;

        if (futureMana < 0) return false; //SI NO PUEDO RESTAR MANA NO HAGO SKILL
        else
        {
            mana -= cost;
            return true; //SI TENGO MANA SUFICIENTE RESTO Y HAGO SKILL
        }
    }
}
