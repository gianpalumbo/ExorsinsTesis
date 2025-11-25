using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetTrigger : MonoBehaviour
{
    //    // lista estatica para almacenar referencias a los enemigos registrados, la mejor opcion si hay muchos triggers a futuro, en vez de duplicar listas por cada trigger
    //    private static List<EnemyLife> enemies = new List<EnemyLife>();

    //    public static void RegisterEnemy(EnemyLife enemy) // metodo para registrar un enemigo, se llama desde el awake de cada EnemyLife. si contiene enemy life se agrega
    //    {
    //        if (!enemies.Contains(enemy))
    //        {
    //            enemies.Add(enemy);
    //        }
    //    }

    //    private void OnTriggerEnter(Collider other)
    //    {
    //        if (other.CompareTag("Player"))
    //        {            
    //            foreach (EnemyLife enemy in enemies)
    //            {
    //                // filtro que asegura que el enemigo forma parte de la escena asi descarta assets o prefabs no instanciados
    //                if (enemy != null && enemy.gameObject.scene.isLoaded)
    //                {
    //                    // si esta prendido reinicia el enemigo desactivandolo y volviendolo a activar para que ejecute OnEnable. asi enemigos en estado de alerta o persecusion tambien se resetean. sino lo activa
    //                    if (enemy.gameObject.activeInHierarchy)
    //                    {
    //                        enemy.gameObject.SetActive(false);
    //                        enemy.gameObject.SetActive(true);
    //                    }
    //                    else
    //                    {
    //                        enemy.gameObject.SetActive(true);
    //                    }
    //                }
    //            }
    //        }
    //    }
    //}

    /* //SCRIPT BUENO QUE ANDABA CON ENEMYLIFE
        private static List<EnemyLife> enemies = new List<EnemyLife>();

    public static void RegisterEnemy(EnemyLife enemy)
    {
        if (!enemies.Contains(enemy)) enemies.Add(enemy);
    }

    public static void ResetAllEnemies()
    {
        foreach (EnemyLife enemy in enemies)
        {
            if (enemy != null && enemy.gameObject.scene.isLoaded)
            {
                bool wasActive = enemy.gameObject.activeInHierarchy;
                enemy.gameObject.SetActive(false);
                enemy.gameObject.SetActive(true);

                // si no estaba activo, lo activamos
                //if (!wasActive) enemy.gameObject.SetActive(true);
            }
        }
    }
}
    */

    [SerializeField] private static List<Entity> enemies = new List<Entity>();

    public static void RegisterEnemy(Entity enemy)
    {
        //if (!enemies.Contains(enemy)) enemies.Remove(enemy);
        //enemies.Remove(enemy);
        //if (enemies.Contains(enemy)) enemies.Add(enemy);
        enemies.Add(enemy);

    }

    public static void RemoveEnemy(Entity enemy)
    {
        if (!enemies.Contains(enemy)) enemies.Remove(enemy);
        Debug.Log($"se removio un enemigo: {enemy.name}. conteo actual de listas enemies: {enemies.Count} ");
    }

    public static void ResetAllEnemies()
    {
        foreach (Entity enemy in enemies)
        {
            if (enemy.GetComponent<VorcarbisEFSM>() || enemy.GetComponent<Beelzebub>())
                continue;

            if (enemy != null && enemy.gameObject.scene.isLoaded)
            {
                bool wasActive = enemy.gameObject.activeInHierarchy;
                enemy.gameObject.SetActive(false);
                enemy.gameObject.SetActive(true);
                enemy.GetComponent<EnemyLife>().ReviveReset();

                // si no estaba activo, lo activamos
                //if (!wasActive) enemy.gameObject.SetActive(true);
            }
        }
        Debug.Log($"conteo actual de listas enemies: {enemies.Count}");
    }
}



