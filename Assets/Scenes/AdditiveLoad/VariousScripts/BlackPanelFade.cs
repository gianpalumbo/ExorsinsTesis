using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BlackPanelFade : MonoBehaviour
{
    Image blackPanel => GetComponent<Image>();
    [SerializeField] float duration = 2f;
    float targetAlpha = 0f;

    [SerializeField] GameObject button, text;

    string startingScene;

    private void Awake()
    {
        ServiceLocator.Instance.RegisterDependency<BlackPanelFade>(this);
    }
    private void OnDestroy()
    {
        ServiceLocator.Instance.RemoveDependency<BlackPanelFade>();
    }
    private void Start()
    {
        StartCoroutine(Fade(targetAlpha, duration, false, true));

        startingScene = PlayerMVC.GetSceneName(ServiceLocator.Instance.GetDependency<PlayerMVC>().startingScene);
    }

    public void FadeInOutNoCoroutine(float fadeInTime, float holdTime, float fadeOutTime)
    {
        StartCoroutine(FadeInOut(fadeInTime,holdTime,fadeOutTime));
    }

    public void FadeNoCoroutine(float targetAlpha, float duration, bool withButtons, bool waitScene)
    {
        StartCoroutine(Fade(targetAlpha, duration, withButtons, waitScene));
    }

    public IEnumerator Fade(float targetAlpha, float duration, bool withButtons, bool waitScene)
    {
        Debug.Log("HAGO FADE");

        // --- UI Setup según si es fade de victoria o fade normal ---
        UtilitiesAgus.ToggleCursor(withButtons);
        //UtilitiesAgus.ToggleCanvasGroup(
        //    gameObject.GetComponentInParent<CanvasGroup>(),
        //    withButtons
        //);

        text.SetActive(withButtons);
        button.SetActive(withButtons);

        // --- IMPORTANTE: esperar al menos 1 frame para que Start() ejecute ---
        yield return null;

        // --- No esperar por escena si startingScene está vacío ---
        if (waitScene && !string.IsNullOrEmpty(startingScene))
            yield return new WaitForSeconds(2f);

        // --- Congelar player si existe ---
        var player = ServiceLocator.Instance.GetDependency<PlayerMVC>();
        if (player != null)
            player.FreezeRotRB();

        // --- Fade real ---
        float startAlpha = blackPanel.color.a;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float lerpValue = Mathf.Lerp(startAlpha, targetAlpha, t / duration);

            var c = blackPanel.color;
            c.a = lerpValue;
            blackPanel.color = c;

            yield return null;
        }

        // --- Seteo final por seguridad ---
        var finalColor = blackPanel.color;
        finalColor.a = targetAlpha;
        blackPanel.color = finalColor;
    }

    public IEnumerator FadeInOut(float fadeInTime, float holdTime, float fadeOutTime)
    {
        // --- Fade IN (0 ? 1) ---
        float t = 0f;
        float startAlpha = blackPanel.color.a;
        while (t < fadeInTime)
        {
            t += Time.deltaTime;
            float lerp = Mathf.Lerp(startAlpha, 1f, t / fadeInTime);

            var c = blackPanel.color;
            c.a = lerp;
            blackPanel.color = c;

            yield return null;
        }

        // Fijar alpha 1
        var c1 = blackPanel.color;
        c1.a = 1f;
        blackPanel.color = c1;

        // --- Espera ---
        if (holdTime > 0f)
            yield return new WaitForSeconds(holdTime);

        // --- Fade OUT (1 ? 0) ---
        t = 0f;
        while (t < fadeOutTime)
        {
            t += Time.deltaTime;
            float lerp = Mathf.Lerp(1f, 0f, t / fadeOutTime);

            var c = blackPanel.color;
            c.a = lerp;
            blackPanel.color = c;

            yield return null;
        }

        // Fijar alpha 0
        var c0 = blackPanel.color;
        c0.a = 0f;
        blackPanel.color = c0;
    }


    public static bool IsSceneLoaded(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        return scene.isLoaded;
    }
}
