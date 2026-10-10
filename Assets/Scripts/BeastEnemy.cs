using System;
using UnityEngine;

public class BeastEnemy : BaseEnemy
{
	[Header("BeastEnemy Components & Game Objects")]
	[SerializeField] private Transform seat;
	[SerializeField] private GameObject seatPrefab;
	//public GameObject visionConeGO;
	Transform Target = null;

	[Header("BeastEnemy Stats")]
	[SerializeField] protected float sprintMod = 2;
	[SerializeField] protected float chaseDistance = 10;
	[SerializeField] private float attackDistance = 2.5f;
	[SerializeField] protected float waitTime = 5;
	[SerializeField] private bool enraged = false;
	[SerializeField] private bool returning = false;
	[SerializeField] private float attackCooldown = 2;
	private float attackTimer = 0.0f;
	private bool canAttack = true;

	void Start()
    {
		agent.updateRotation = false;
		agent.updateUpAxis = false;

		GameObject g = Instantiate(seatPrefab, transform.position, transform.rotation);
		seat = g.GetComponent<Transform>();

		agent.speed = moveSpeed;
    }

    // Update is called once per frame
    void Update()
    {
		if (!canAttack && attackTimer > 0)
		{
			attackTimer -= Time.deltaTime;
			if (attackTimer <= 0)
			{
				canAttack = true;
				attackTimer = 0f;
			}
		}
		
		if (enraged)
		{
			if (canAttack && agent.remainingDistance <= attackDistance)
			{
				Attack();
				canAttack = false;
				attackTimer = attackCooldown;
			}
				
			agent.SetDestination(Target.position);
			agent.obstacleAvoidanceType = UnityEngine.AI.ObstacleAvoidanceType.HighQualityObstacleAvoidance;
			float f = Mathf.Atan2(agent.velocity.x, -agent.velocity.y) * Mathf.Rad2Deg;
			//Debug.Log(f);
			mesh.transform.rotation = Quaternion.Slerp(mesh.transform.rotation, Quaternion.Euler(new Vector3(0, 0, f)), Time.deltaTime * 10f);
			agent.stoppingDistance = 2;
			if (agent.remainingDistance > chaseDistance)
			{
				//if the target is outside chase distance:
				Derage();
			}
		}


		if (returning)
		{
			float f = Mathf.Atan2(agent.velocity.x, -agent.velocity.y) * Mathf.Rad2Deg;
			//Debug.Log(f);
			mesh.transform.rotation = Quaternion.Slerp(mesh.transform.rotation, Quaternion.Euler(new Vector3(0, 0, f)), Time.deltaTime * 10f);

			if (agent.remainingDistance == 0 && this.transform.position == seat.transform.position)
			{
				agent.isStopped = true;
				//anim.Play("watch");
				returning = false;
				transform.rotation = seat.rotation;
			}
		}

    }

	protected override void TakeDamage(float amount)
	{
		currentHealth -= amount;
		if (currentHealth <= 0) { Die(); }

		if (!enraged)
		{

		}
	}

	public void SeePlayer(Transform playerPos)
	{
		if (!enraged)
		{
			//View is controlled by a cone that detects when the player enters it. The cone
			//Debug.Log("Did I see you...?");
			ContactFilter2D filter = new ContactFilter2D
			{
				useTriggers = false

			};
			RaycastHit2D[] hit = new RaycastHit2D[3];

			Physics2D.Linecast(transform.position, playerPos.position, filter, hit);
			//Debug.Log(hit[1].point + " " + hit[1].transform.gameObject.name);
			if (hit[1].transform.gameObject.tag.Equals("Player"))
			{
				//If the first object checked is the player and not a wall:
				//Debug.Log("YES!");
				Enrage(playerPos);

			}
			else
			{
				//Debug.Log("Nope");
			}
		}
	}

	void Enrage(Transform playerPos)
	{
		//Debug.Log("GET OVER HERE!");
		Target = playerPos;
		agent.isStopped = false;
		agent.speed = moveSpeed * sprintMod;
		enraged = true;
	}

	void Derage()
	{
		Target = null;
		enraged = false;
		
		//anim.Play("wait");
		Invoke(nameof(ReturnToSeat), waitTime);
	}

	void ReturnToSeat()
	{
		agent.SetDestination(seat.transform.position);
		agent.stoppingDistance = 0;
		agent.speed = moveSpeed;
		returning = true;
	}
	
	public override void Attack()
	{
		Debug.Log("Beast Attacks");
		//Target.gameObject.SendMessage("TakeDamage");
	}
}
