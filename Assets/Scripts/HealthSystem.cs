using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    [SerializeField] private int health = 100;
    private int healthMax;
    public event EventHandler OnDead;
    public event EventHandler OnDamaged; 
        
    private void Awake()
    {
        healthMax = health;
    }
    public void Damage(int damageAmount)
    {
        health = Math.Clamp(health - damageAmount, 0, 100);
        OnDamaged?.Invoke(this, EventArgs.Empty);
        if (health == 0)
        {
            Die();
        }
    }

    public float GetHealthNormalized() => health * 1.0f / healthMax;

    private void Die()
    {
        OnDead?.Invoke(this, EventArgs.Empty);
    }
}
