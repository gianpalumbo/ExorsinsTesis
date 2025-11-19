using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(BoxCollider))]
public class MeatLife : MonoBehaviour, IDamageable
{
    [Header("RightClick and (AdjustCollider) to retrieve dependencies")]
    [SerializeField] float collSizeMagnitude = 0.05f;
    [SerializeField] int hitsToDestroy = 3;
    [SerializeField] VisualEffect particles;

    int hitCount = 0;

    public void Knockback(Vector3 vector, float knock){}

    public void TakeDamage(float damage)
    {
        hitCount++;
        if (particles != null)
        {
            particles.SetVector3("WeaponDir", transform.right);
            particles.SetVector3("WeaponAngle", transform.eulerAngles);
            particles.SendEvent("BloodSplatter");
        }
        transform.localScale -= transform.localScale * .1f; // Reduce el tamaño del objeto al recibir daño
        if (hitCount >= hitsToDestroy)
            Destroy(gameObject);
    }

    [ContextMenu("Adjust Collider")]
    void AdjustCollider()
    {
        if (particles == null) GetComponentInChildren<VisualEffect>();
        particles.transform.parent.gameObject.transform.position = Vector3.zero;
        particles.transform.position = transform.position + new Vector3(-0.7f, 1.3f, .25f);
        BoxCollider collider = this.GetComponent<BoxCollider>();
        collider.isTrigger = enabled;
        gameObject.tag = "Enemy";
        if (collider != null)
        {
            collider.size = new Vector3(collSizeMagnitude, collSizeMagnitude, collSizeMagnitude);
        }
        Debug.Log("Dependencies retrieved");
    }
}
