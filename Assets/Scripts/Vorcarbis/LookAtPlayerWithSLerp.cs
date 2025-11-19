using UnityEngine;
using System.Collections;

public class LookAtPlayerWithSLerp: MonoBehaviour
{
    [SerializeField] float rotationSpeed = 5f, Y_Offset = 2f;
    Transform player;

    IEnumerator Start()
    {
        yield return new WaitUntil(() => ServiceLocator.Instance.TryGetDependency<PlayerMVC>(out PlayerMVC playerMVC));

        player = ServiceLocator.Instance.GetDependency<PlayerMVC>().transform;
    }

    void Update()
    {
        if (player == null) return;

        Vector3 playerDir = new Vector3(player.position.x , 0 , player.position.z);
        Vector3 mouthDir = new Vector3(transform.position.x, 0, transform.position.z);
        // dirección 3D completa (incluye arriba/abajo)
        //Vector3 direction = (player.position + Vector3.up * Y_Offset - transform.position).normalized;
        Vector3 dir = (playerDir - mouthDir).normalized;

        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }

    private void OnDrawGizmos()
    {
        if (player != null)
            Gizmos.DrawRay(transform.position, player.position + Vector3.up * Y_Offset - transform.position);
    }
}
