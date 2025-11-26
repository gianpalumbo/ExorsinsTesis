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

    private static IEnumerator LoadSceneAsync(string sceneToLoad, string sceneToUnload)
    {
        // Si ya estoy en esa escena, no la vuelvo a cargar
        if (SceneManager.GetSceneByName(sceneToLoad).isLoaded)
        {
            Debug.LogWarning($"[AdditiveSceneMgr] La escena '{sceneToLoad}' ya está cargada. Cancelando nuevo load.");
            isLoading = false;
            yield break;
        }

        isLoading = true;

        // Congelá TODO apenas empieza la carga
        var player = ServiceLocator.Instance.GetDependency<PlayerMVC>();
        if (player != null)
            player.FreezeAllRB();  // <-- acá frenás al rigid ANTES de empezar el load

        // --- Carga la escena ---
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);
        asyncLoad.allowSceneActivation = false;

        // Espera al 90% (ready to activate)
        while (asyncLoad.progress < 0.9f)
            yield return null;

        // Activá la escena
        asyncLoad.allowSceneActivation = true;

        // Esperá al final real
        yield return new WaitUntil(() => asyncLoad.isDone);

        // Esperá a que Unity la registre
        yield return new WaitUntil(() => SceneManager.GetSceneByName(sceneToLoad).isLoaded);

        // Esperá UN frame para que se instancien todos los objetos
        yield return null;

        // --- Ahora buscá al Player de nuevo (puede haber sido regenerado, movido o reaccesado) ---
        player = ServiceLocator.Instance.GetDependency<PlayerMVC>();

        

        if (player != null)
            player.FreezeRotRB(); // <-- acá lo descongelás SOLO de rotación (tu “unfreeze”)
        else
            Debug.LogWarning("[AdditiveSceneMgr] Player no encontrado después de cargar la escena.");

        isLoading = false;

        // --- Descarga la escena anterior ---
        if (!string.IsNullOrEmpty(sceneToUnload) && sceneToUnload != sceneToLoad)
            coroutineHost.StartCoroutine(UnloadSceneAsync(sceneToUnload));
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
