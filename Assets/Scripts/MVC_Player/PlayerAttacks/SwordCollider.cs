using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using UnityEngine.VFX;
using UnityEngine.SocialPlatforms.Impl;

public class SwordCollider : MonoBehaviour
{
    [SerializeField] VisualEffect _sparks;
    [SerializeField] Material _mat;
    [SerializeField] AttackEFSM wielder;
    [SerializeField] Transform swordAngle, swordModel;
    [SerializeField] float knockback, bloodDuration = 1f;

    public event Action OnHit = delegate { };

    private void Awake()
    {
        if (wielder == null) wielder = GetComponentInParent<AttackEFSM>();
        _mat.SetFloat("_Hits", 0);
    }
    private void Update()
    {
        if (_mat.GetFloat("_Hits") > 0) _mat.SetFloat("_Hits", _mat.GetFloat("_Hits") - Time.deltaTime / bloodDuration);
    }

    private void OnTriggerEnter(Collider other)
    {
        //if (other.GetComponent<EnemyLife>() && other.gameObject.layer == 9)
        //{
        //    other.GetComponent<EnemyLife>().TakeDamage(wielder.lightDmg);
        //}
        //---------------------------------------

        //if (other.GetComponent<Entity>() && other.gameObject.layer == 9)
        //{
        //    other.GetComponent<Entity>().TakeDamage(wielder.lightDmg);
        //}
        //if (other.GetComponent<Entity>() && other.gameObject.layer == 9)
        //{
        //    other.GetComponent<Entity>().TakeDamage(wielder.currentDmg);
        //}
        //CON EL CURRENT BASTA Y SIGUE IGUAL

        IDamageable damageable = other.GetComponent<IDamageable>();
        var enemyBlood = other.GetComponentInChildren<VisualEffect>();
        if (other.CompareTag("Enemy"))
        //intente que si la layer es distinta a la invulnerable no le pegue, es un tema de animacion esto
        //if (other.CompareTag("Enemy") && other.gameObject.layer != 10)
        {
            //wielder.SlowAnimSpeed();
            if (damageable != null)
            {
                Vector3 dir = other.transform.position - wielder.transform.position;
                dir.Normalize();
                damageable.TakeDamage(wielder.currentDmg);
                damageable.Knockback(dir, knockback);
                OnHit();
            }
            if (enemyBlood != null)
            {
                swordAngle.rotation = Quaternion.LookRotation(swordModel.right, swordModel.forward);
                swordAngle.localRotation = Quaternion.Euler(0f, 0f, swordAngle.eulerAngles.z);
                enemyBlood.SetVector3("WeaponDir", swordAngle.right);
                enemyBlood.SetVector3("WeaponAngle", swordAngle.eulerAngles);
                //enemyBlood.SetInt("Hits", enemyBlood.GetInt("Hits") + 1);
            }
            _sparks.SetBool("IsEnemy", true);
            Vector3 hitPoint = other.ClosestPoint(transform.position);
            _sparks.transform.position = hitPoint;
            _sparks.transform.LookAt(transform.position);
            _sparks.SendEvent("OnPlay");
            if (_mat.GetFloat("_Hits") >= 5) return;
            _mat.SetFloat("_Hits", _mat.GetFloat("_Hits") + 1);
        }

        if (other.CompareTag("Sparkeable"))
        {
            _sparks.SetBool("IsEnemy", false);
            Vector3 hitPoint = other.ClosestPoint(transform.position);
            _sparks.transform.position = hitPoint;
            _sparks.transform.LookAt(transform.position);
            _sparks.SendEvent("OnPlay");

            //AGUS ADDON
            SoundManager.Instance.PlayOneShotFromIndex(15);
        }
    }
}
