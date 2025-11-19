using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeInOut : MonoBehaviour
{
    public Image blackPanel;
    public float fadeDuration = 2f;
    // Start is called before the first frame update
    void Awake()
    {
        //forzar alpha en 1, para asegurarme
        blackPanel.gameObject.SetActive(true);
        Color c = blackPanel.color;
        c.a = 1f;
        blackPanel.color = c;

        StartCoroutine(Fade(0f, fadeDuration));
    }

    IEnumerator Fade(float targetAlpha, float duration)
    {
        float startAlphaValue = blackPanel.color.a;
        float timeElapsed = 0f;
        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            var c = blackPanel.color;
            c.a = Mathf.Lerp(startAlphaValue, targetAlpha, timeElapsed / duration);
            blackPanel.color = c;
            yield return null;
        }
        var endAlphaValue = blackPanel.color;
        endAlphaValue.a = targetAlpha;
        blackPanel.color = endAlphaValue;
    }
}
