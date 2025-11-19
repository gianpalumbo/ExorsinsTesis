using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class ManaUI : MonoBehaviour
{
    SkillManager skillManager;
    ModelPlayer modelPlayer;
    [SerializeField] Image manaSlider;
    float maxManaUINormalized, currentManaNormalized;

    private void Awake()
    {
        ServiceLocator.Instance.RegisterDependency<ManaUI>(this);
    }

    private void OnDisable()
    {
        ServiceLocator.Instance.RemoveDependency<ManaUI>();
    }

    private void Start()
    {
        if (ServiceLocator.Instance.TryGetDependency<SkillManager>(out var skillManager))
        {
            this.skillManager = skillManager;
            //Debug.Log("me agarro skill manager");
            skillManager.OnSkillUse += UpdateMana;
        }
        if (ServiceLocator.Instance.TryGetDependency<ModelPlayer>(out var modelPlayer))
        {
            this.modelPlayer = modelPlayer;
            //Debug.Log("me agarro model player");
            modelPlayer.OnRechargingMana += RechargeManaUI;
        }

        maxManaUINormalized = ModelPlayer._maxMana / 100f;
        currentManaNormalized = maxManaUINormalized;
    }

    bool UpdateMana(float manaCost) //mana sobre max me va a dar entre 0 y 1 pero si le quiero bajar tiene que ser el equivalente, dividido 100
    {
        var manaCostNormalized = manaCost / 100f; //AHORA SI TANTO MAXMANA COMO MANACOST ME QUEDAN DE 0 A 1

        if (currentManaNormalized - manaCostNormalized < 0) return false;
        else
        {
            //Debug.Log($"Bajo {manaCost} mana a la UI");
            currentManaNormalized -= manaCostNormalized;
            manaSlider.fillAmount = currentManaNormalized;
            return true;
        }
    }
                        //ESTE PARAMETRO ME LO PASA MODEL DE SU EVENT
    void RechargeManaUI(float manaPerSecond)
    {
        if(currentManaNormalized > maxManaUINormalized)
        {
            currentManaNormalized = maxManaUINormalized;
        }
        else
        {
            //Debug.Log($"Recargo mana a este paso {(Time.deltaTime * manaPerSecond) / 100f} y maxManaNormalized es {maxManaUINormalized}");
            currentManaNormalized += (Time.deltaTime * manaPerSecond) / 100f;
        }

        manaSlider.fillAmount = currentManaNormalized;
    }

    private void OnDestroy()
    {
        if(skillManager != null)
            skillManager.OnSkillUse -= UpdateMana;
        if (modelPlayer != null)
            modelPlayer.OnRechargingMana -= RechargeManaUI;
    }
}
