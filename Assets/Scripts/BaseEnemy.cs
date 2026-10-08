using System;
using UnityEngine;
using UnityEngine.AI;

public abstract class BaseEnemy : MonoBehaviour
{
	//Components
	[Header("BaseEnemy Components & Game Objects")]
	public GameObject mesh; //Animated mesh for the enemies (LUKE ONLY!)
	Animator anim;          //Controller for animations (Luke will add these in at a later point)
	Rigidbody2D rb;
	NavMeshAgent agent;


	
	[Header("BaseEnemy Stats")] 
    [SerializeField] protected float maxHealth;
    [SerializeField] protected float moveSpeed;
    [SerializeField] protected float damage;

    protected float currentHealth;

    public event Action<float> OnDamageTaken;

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
		//anim will need to be implemented in Start() b/c it won't be attached to this object.
		rb = GetComponent<Rigidbody2D>();
		agent = GetComponent<NavMeshAgent>();
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
