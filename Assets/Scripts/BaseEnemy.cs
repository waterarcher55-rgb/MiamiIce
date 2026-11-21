using System;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyStatus
{
	None,
	Idling,
	Chasing,
	Searching,
	Returning,
	Patrolling,
	Investigating,
	Dead,

}

public abstract class BaseEnemy : MonoBehaviour
{
	//Components
	[Header("BaseEnemy Components & Game Objects")]
	public EnemyStatus Status = EnemyStatus.None;
	public GameObject mesh;    //Animated mesh for the enemies (LUKE ONLY!)
	protected Animator anim;   //Controller for animations (Luke will add these in at a later point)
	protected Rigidbody2D rb;
	protected NavMeshAgent agent;
	protected Vector3 startPosition;
	protected bool invoked = false;

	public static event Action<BaseEnemy> OnEnemyKill;

	[Header("BaseEnemy Stats")] 
    [SerializeField] protected float maxHealth;
    [SerializeField] protected float moveSpeed;
    [SerializeField] protected float damage;
	[SerializeField] protected float waitTime = 5;

	protected float currentHealth;

    public event Action<float> OnDamageTaken;

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
		//anim will need to be implemented in Start() b/c it won't be attached to this object.
		rb = GetComponent<Rigidbody2D>();
		agent = GetComponent<NavMeshAgent>();
		startPosition = transform.position;
    }

    protected virtual void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0) { Die(); }
    }
    public abstract void Attack(); //Likely different for each enemy

    protected virtual void Die()
    {
		Debug.Log("I'm dead!");
		agent.enabled = false;
		OnEnemyKill?.Invoke(this);
    }

	public virtual void ManualSend(Vector3 position, EnemyStatus Status)
	{
		this.Status = Status;
		Debug.Log($"{gameObject.name} has been sent to investigate!");
		startPosition = transform.position;
		agent.destination = position;
	}

	public virtual void ManualReturn()
	{
		agent.destination = startPosition;
		invoked = false;
		Status = EnemyStatus.None;
	}

}
