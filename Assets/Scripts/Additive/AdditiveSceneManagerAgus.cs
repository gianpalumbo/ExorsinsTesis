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
        // Evitar doble carga
        if (SceneManager.GetSceneByName(sceneToLoad).isLoaded)
        {
            Debug.LogWarning($"[AdditiveSceneMgr] La escena '{sceneToLoad}' ya está cargada.");
            isLoading = false;
            yield break;
        }

        isLoading = true;

        // 1) Congelar TODO ANTES de cargar
        var player = ServiceLocator.Instance.GetDependency<PlayerMVC>();
        if (player != null)
            player.FreezeAllRB();

        // 2) Empezar carga async
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
            yield return null;

        asyncLoad.allowSceneActivation = true;
        yield return new WaitUntil(() => asyncLoad.isDone);

        // 3) Esperar a que Unity registre la escena
        yield return new WaitUntil(() => SceneManager.GetSceneByName(sceneToLoad).isLoaded);

        // 4) Esperar 1 frame para que instancien objetos
        yield return null;

        // ‼️ 5) Esperar un ciclo de físicas (EL FIX REAL)
        yield return new WaitForFixedUpdate();

        // 6) Buscar player actualizado
        player = ServiceLocator.Instance.GetDependency<PlayerMVC>();

        if (player != null)
            player.FreezeRotRB();
        else
            Debug.LogWarning("[AdditiveSceneMgr] Player no encontrado luego de cargar escena.");

        isLoading = false;

        // Unload anterior
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
