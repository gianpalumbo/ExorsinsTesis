using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum SkillType
{
    SwordsFalling,
    SlowZone,
    FireBall
    // agrego mas habilidadesa aca
}

public class SkillButtonUI : MonoBehaviour
{
    [SerializeField] private SkillType skillType;       // tipo de habilidad a asignar
    [SerializeField] private SkillManager skillManager; 
    //[SerializeField] private SkillWheelUI skillWheelUI; 

    //private void Awake()
    //{
    //    // se asigna el listener al boton
    //    var button = GetComponent<Button>();
    //    if (button != null)
    //        button.onClick.AddListener(OnButtonClicked);
    //    else
    //        Debug.LogWarning("SkillButtonUI: No se encontro buton.");
    //}

    public void OnButtonClicked()
    {
        if (skillManager != null) skillManager.SetSkill(skillType);
        else Debug.LogWarning("SkillButtonUI: SkillManager no asignado.");

        //if (skillWheelUI != null) skillWheelUI.CloseSkillWheel();
        //else Debug.LogWarning("SkillButtonUI: SkillWheelUI no asignado.");
    }
}