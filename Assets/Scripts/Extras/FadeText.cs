using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FadeText : MonoBehaviour
{
    public TextMeshProUGUI zoneTMP; // o usa TextMeshPro si es en world space
    //Public float fadeDuration = 1.0f;
    private float fadeDuration = 2f; // tiempo de transicion del alpha

    [Header("Identificador de la Zona")] public string zoneID; // funciona tambien como texto

    //------------------------------------------------------------
    //CAMBIAR A FUTURO, PODRIA HACER QUE LA ULTIMA KEY LO ALMACENE EL JUGADOR Y CADA QUE TOQUE UN TRIGGER CON ID DISTINTO COMPARE Y ACTIVE, SERIA MAS OPTIMO QUE UNA STATIC PARA 1 SOLA VARIABLE
    //------------------------------------------------------------

    private static string lastZoneID = "";  // usamos una variable estatica para almacenar la ultima zona activada

    private void OnTriggerEnter(Collider other)
    {
        if (TryGetComponent<PlayerMVC>(out PlayerMVC player))
        {
            zoneTMP = player.textFade;
            // compara la zona actual con la ultima activada
            if (lastZoneID != zoneID)
            {
                lastZoneID = zoneID;
                zoneTMP.text = lastZoneID;
                StartCoroutine(FadeTextCoroutine());
            }
        }
    }

    //------------------------------------------------------------
    //HACER QUE ADEMAS DESACTIVE EL OBJETO DE TMP, ASI ES MAS OPTIMO. ACTUALEMTE SOLO ES UN ALPHA EN 0, IGUAL ESTA CONSUMIENDO RECURSOS (TAL VEZ)
    //------------------------------------------------------------

    private IEnumerator FadeTextCoroutine()
    {
        // fade in
        float elapsed = 0f;
        Color originalColor = zoneTMP.color;

        zoneTMP.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f); // aseguro que el texto inicia invisible

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / fadeDuration);
            zoneTMP.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        // espera 1s y hace fade out
        yield return new WaitForSeconds(1f);

        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(1 - (elapsed / fadeDuration));
            zoneTMP.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }
    }
}
