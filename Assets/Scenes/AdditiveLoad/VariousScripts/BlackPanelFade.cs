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
    public IEnumerator Fade(float targetAlpha, float duration, bool withButtons, bool waitScene)
    {
        // --- UI Setup según si es fade de victoria o fade normal ---
        UtilitiesAgus.ToggleCursor(withButtons);
        UtilitiesAgus.ToggleCanvasGroup(
            gameObject.GetComponentInParent<CanvasGroup>(),
            withButtons
        );

        text.SetActive(withButtons);
        button.SetActive(withButtons);

        // --- IMPORTANTE: esperar al menos 1 frame para que Start() ejecute ---
        yield return null;

        // --- No esperar por escena si startingScene está vacío ---
        if (waitScene && !string.IsNullOrEmpty(startingScene))
            yield return new WaitUntil(() => IsSceneLoaded(startingScene));

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

    public static bool IsSceneLoaded(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        return scene.isLoaded;
    }
}
