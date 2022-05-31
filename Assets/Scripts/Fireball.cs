
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


public class Fireball : MonoBehaviour
{
    private bool _isAttacked=false;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<Health>().TakeDamage(Random.Range(5, 15),"Player");
        }
        if (other.CompareTag("Enemy"))
        {
            other.GetComponent<Health>().TakeDamage(Random.Range(5, 15),"Enemy");
        }
        Destroy(this.gameObject);
    }

    private void OnTriggerExit(Collider other)
    { 
        Destroy(this.gameObject);
    }
}