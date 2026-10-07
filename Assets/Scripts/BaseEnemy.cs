using System;
using UnityEngine;

public abstract class BaseEnemy : MonoBehaviour
{
    [Header("Stats")] 
    [SerializeField] protected float maxHealth;
    [SerializeField] protected float moveSpeed;
    [SerializeField] protected float damage;

    protected float currentHealth;

    public event Action<float> OnDamageTaken;
    protected virtual void Awake()
    {
        currentHealth = maxHealth;
    }

    protected virtual void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0) { Die(); }
    }
    public abstract void Attack(); //Likely different for each enemy

    protected virtual void Die()
    {
        Destroy(gameObject);
    }
}
