using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnOnArray : MonoBehaviour
{
    [SerializeField] GameObject[] gameObjects;

    IEnumerator Start()
    {
        //INICIALIZADO EN PLAYER
        yield return new WaitForSeconds(1f);
        TurnOnStuff();
    }
    void TurnOnStuff() => StartCoroutine(ActivateStuff());

    IEnumerator ActivateStuff()
    {
        foreach (GameObject go in gameObjects)
        {
            go.SetActive(true);
            yield return new WaitForSeconds(.2f);
        }
    }
    //IEnumerator ActivateGroupsSmoothly(GameObject[] parentArray, int batchSize = 10, float delay = 0.05f)
    //{
    //    foreach (GameObject parent in parentArray)
    //    {
    //        if (parent == null) continue; // por si hay referencias rotas

    //        if (!parent.activeInHierarchy) parent.SetActive(true);

    //        int count = 0;

    //        foreach (Transform child in parent.transform)
    //        {
    //            child.gameObject.SetActive(true);
    //            count++;

    //            if (count >= batchSize)
    //            {
    //                count = 0;
    //                yield return new WaitForSeconds(delay);
    //            }
    //        }

    //        // pequeño delay entre cada grupo de padres (opcional)
    //        yield return new WaitForSeconds(delay);
    //    }
    //}
}
