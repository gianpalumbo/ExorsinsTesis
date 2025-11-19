using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class DEMOEnemyLife : MonoBehaviour
{
    //public Animator anim;
    //public Slider sliderLife;
    public float health, maxHP = 100;

    private void Start()
    {
        //sliderLife.maxValue = maxHP;
        health = maxHP;
        //sliderLife.value = health;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 11)
        {
            TakeDamage(25);
        }
    }

    public void TakeDamage(float dmg)
    {
        health -= dmg;
        //sliderLife.value -= dmg;

        //anim.SetTrigger("OnHit");

        if (health <= 0) Destroy(this.gameObject);
    }
    
}
