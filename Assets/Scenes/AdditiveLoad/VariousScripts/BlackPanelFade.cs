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

    string startingScene;

    private void Start()
    {
        StartCoroutine(Fade(targetAlpha, duration));

        startingScene = PlayerMVC.GetSceneName(ServiceLocator.Instance.GetDependency<PlayerMVC>().startingScene);
    }
    IEnumerator Fade(float targetAlpha, float duration)
    {
        float startAlphaValue = blackPanel.color.a;
        float timeElapsed = 0f;

        if (!string.IsNullOrEmpty(startingScene))
            yield return new WaitUntil(() => IsSceneLoaded(startingScene));
        else
            yield return null;

        ServiceLocator.Instance.GetDependency<PlayerMVC>().FreezeRotRB();

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
    public static bool IsSceneLoaded(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        return scene.isLoaded;
    }
}
