using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class MenuButton : MonoBehaviour
{
    [Header("Shader WarmUp")]
    public ShaderVariantCollection shaderVariantCollection;
    [Header("Menu Variables")]
    public GameObject buttonContainer;
    public GameObject creditsPanel, preloadShadersPanel;
    //AGUS WAS HERE
    [SerializeField] GameObject loadingPanel;
    [SerializeField] Image loadingBarFill;

    public Image blackPanel;
    public float fadeDuration = 2f;

    public AudioSource musicSource;
    //CHELO WAS HERE: id para pasarlo al CursorUIManager y se desbloqueen los mouse
    private readonly string id = "MenuButton";
    void Awake()
    {
        if (musicSource != null) musicSource.volume = 1f;

        //forzar alpha en 1, para asegurarme
        blackPanel.gameObject.SetActive(true);
        Color c = blackPanel.color;
        c.a = 1f;
        blackPanel.color = c;

        StartCoroutine(Fade(0f, fadeDuration));
        creditsPanel.SetActive(false);

        //CHELO WAS HERE: AGREGUE ID AL CURSOR MANAGER
        CursorUIManager.Instance.RequestCursorState(true, id);
    }

    private void Start()
    {
        SoundManager.Instance?.ChangeToMenuMusic();
        if(SoundManager.Instance != null) SoundManager.Instance.menuButtonPanel = gameObject;
    }

    public void SoundManagerCanvasGroupOn() => SoundManager.Instance?.CanvasGroupTurnOn();

    private void OnDisable()
    {
        //SoundManager.Instance?.ChangeToCaveAmbience();
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(FadeAndLoad(sceneName));
    }

    IEnumerator Fade(float targetAlpha, float duration)
    {
        float startAlphaValue = blackPanel.color.a;
        float timeElapsed = 0f;


        float startVolume = musicSource ? musicSource.volume : 0f;
        

        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            var c = blackPanel.color;
            c.a = Mathf.Lerp(startAlphaValue, targetAlpha, timeElapsed / duration);
            blackPanel.color = c;
            

            //audio fade
            if (musicSource) musicSource.volume = Mathf.Lerp(startVolume, (targetAlpha > startAlphaValue ? 0f : 1f), timeElapsed / duration);
            yield return null;
        }
        var endAlphaValue = blackPanel.color;
        endAlphaValue.a = targetAlpha;
        blackPanel.color = endAlphaValue;
        

        if (musicSource) musicSource.volume = (targetAlpha > startAlphaValue ? 0f : 1f);

        if (targetAlpha > startAlphaValue && musicSource) musicSource.Stop();
    }

    //private IEnumerator FadeAndLoad(string sceneName)
    //{
    //    yield return StartCoroutine(Fade(1f, fadeDuration));
    //    buttonContainer.SetActive(false);
    //    loadingPanel.SetActive(true); // Mostrar el panel de carga
    //    //Cursor.lockState = CursorLockMode.Locked;

    //    //AGUS WAS HERE ANTES DE CARGAR ESCENA PRENDO PANEL DE PRECARGAR SHADERS PRECARGO LOS SHADERS
    //    preloadShadersPanel.SetActive(true);
    //    while (shaderVariantCollection.isWarmedUp)
    //    {
    //        shaderVariantCollection.WarmUp();
    //        yield return null;
    //    }
    //    preloadShadersPanel.SetActive(false);
    //    //Cursor.visible = false;
    //    //CHELO WAS HERE: ELIMINO ID DEL CURSOR MANAGER
    //    CursorUIManager.Instance.ReleaseCursorRequest(id);

    //    //AGUS WAS HERE
    //    AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
    //    while (!op.isDone)
    //    {
    //        float progress = Mathf.Clamp01(op.progress / 0.9f);
    //        loadingBarFill.fillAmount = progress;
    //        yield return new WaitForSeconds(.5f); 
    //    } 
    //}

    private IEnumerator FadeAndLoad(string sceneName)
    {
        UtilitiesAgus.ToggleCursor(false);

        // Mostrar panel de precarga de shaders
        preloadShadersPanel.SetActive(true);
        yield return new WaitForSeconds(3f);
        // Precalentar shaders SOLO si aún no lo están
        if (shaderVariantCollection != null && !shaderVariantCollection.isWarmedUp)
        {
            //Debug.Log("🔥 Precargando shaders...");
            shaderVariantCollection.WarmUp();
            yield return null; // un frame para asegurarse que termine
        }

        preloadShadersPanel.SetActive(false);

        yield return new WaitForSeconds(1f);
        
        yield return StartCoroutine(Fade(1f, fadeDuration));
        buttonContainer.SetActive(false);
        loadingPanel.SetActive(true);

        yield return new WaitForSeconds(3f);

        buttonContainer.SetActive(true);
        loadingPanel.SetActive(false);

        // Eliminar el cursor del manager
        CursorUIManager.Instance.ReleaseCursorRequest(id);

        // Cargar escena asíncrona
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
        {
            //float progress = Mathf.Clamp01(op.progress / 0.9f);
            //loadingBarFill.fillAmount = progress;
            yield return null;
        }

        // Activar la escena cuando está lista
        op.allowSceneActivation = true;
    }

    public void ExitGame()
    {
        //CHELO WAS HERE: ELIMINO ID DEL CURSOR MANAGER
        CursorUIManager.Instance.ReleaseCursorRequest(id);
        Application.Quit();
    }

    public void OpenCreditsPanel()
    {
        creditsPanel.SetActive(true);
        buttonContainer.SetActive(false);
    }

    public void CloseCreditsPanel()
    {
        creditsPanel.SetActive(false);
        buttonContainer.SetActive(true);
    }
}
