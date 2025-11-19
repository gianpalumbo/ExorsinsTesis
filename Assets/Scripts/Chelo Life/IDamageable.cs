using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    void TakeDamage(float dmg);

    public void Knockback(Vector3 vector, float knock);
}
