using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class AdditiveSceneManagerAgus
{
    public static event Action OnSceneLoaded, OnSceneUnloaded;

    public static bool isLoading = false;

    private static MonoBehaviour coroutineHost;

    public static void Initialize(MonoBehaviour host)
    {
        coroutineHost = host;
    }

    public static void UnloadScene(string sceneToUnload)
    {
        SceneManager.UnloadSceneAsync(sceneToUnload);
    }

    public static void LoadSceneAdditiveByName(string sceneToLoad, string sceneToUnload = null)
    {
        if (coroutineHost == null)
        {
            Debug.LogError("AdditiveSceneManagerAgus no fue inicializado. Llamá Initialize(this) desde un MonoBehaviour.");
            return;
        }

        coroutineHost.StartCoroutine(LoadSceneAsync(sceneToLoad, sceneToUnload));
    }

    private static IEnumerator LoadSceneAsync(string sceneToLoad,string sceneToUnload)
    {
        isLoading = true;

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
            yield return null;

        asyncLoad.allowSceneActivation = true;
        //OnSceneLoaded?.Invoke();

        isLoading = false;
        //yield return new WaitForEndOfFrame();

        if (!string.IsNullOrEmpty(sceneToUnload) && asyncLoad.progress < .95f)
            coroutineHost.StartCoroutine(UnloadSceneAsync(sceneToUnload));


        // Precarga los shaders de la escena recién cargada (TARDA DEMASIADO)
        //if (sceneToLoad == "Cave_1")
        //{
        //    float start = Time.realtimeSinceStartup;
        //    Shader.WarmupAllShaders();
        //    Debug.Log($"[AdditiveSceneManagerAgus] Shaders de '{sceneToLoad}' precalentados en {Time.realtimeSinceStartup - start:F2}s ✅");
        //}
    }

    public static IEnumerator UnloadSceneAsync(string sceneToUnload)
    {
        isLoading = true;

        AsyncOperation asyncLoad = SceneManager.UnloadSceneAsync(sceneToUnload);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
            yield return null;

        isLoading = false;
        //Debug.Log("Escena descargada");
    }
}
