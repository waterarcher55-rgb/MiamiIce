using UnityEngine;
using UnityEngine.AI;

// Creates the Brute enemy using BaseEnemy
public class BruteEnemy : BaseEnemy
{

    // Patrol points the Brute walks between
    [Header("Patrol Points")]
    public Transform patrolPoint1;
    public Transform patrolPoint2;

    [Header("Player Detection")]
    public Transform player;
    public float detectionRadius = 4f;
    public float loseRadius = 6f;
    public float chaseTime = 3f;
    public LayerMask wallLayer;

    // The point the Brute is moving toward
    private Transform currentTarget;

    // Brute enemy rigidbody
    private Rigidbody2D bruteRb;

    // Handles navigation around walls
    private NavMeshAgent agent;

    // Tracks if the Brute is chasing
    private bool isChasing = false;

    // Counts time after losing the player
    private float loseTimer = 0f;
    
    // Sets up the the Brute when the game starts
    protected override void Awake()
    {
        // Gets health and components from BaseEnemy
        base.Awake();

        // Get Brute rigidbody
        bruteRb = GetComponent<Rigidbody2D>();

        agent = GetComponent<NavMeshAgent>();

        if (agent != null)
        {
            agent.updateRotation = false;
            agent.updateUpAxis = false;
            agent.updatePosition = false;
        }

        // Set the first patrol point
        currentTarget = patrolPoint1;
    }

    // Moves the Brute every physics update
    private void FixedUpdate()
    {
        if (bruteRb == null) return;

        // Check distance from player
        if (player != null)
        {
            float distance = Vector2.Distance(transform.position, player.position);

            // Check if a wall is between the Brute and player
            Vector2 direction = player.position - transform.position;

            RaycastHit2D wallHit = Physics2D.Raycast(transform.position, direction.normalized, distance, wallLayer);

            // Start chasing only if no wall blocks the player
            if (!isChasing && distance <= detectionRadius && wallHit.collider == null)
            {
                isChasing = true;
                loseTimer = 0f;
            }

            // Continue chasing while the player is close
            if (isChasing)
            {
                if (distance > loseRadius || wallHit.collider != null)
                {
                    Debug.Log("Brute lost sight of player");
                    loseTimer += Time.fixedDeltaTime;

                    // Give up after 3 seconds
                    if (loseTimer >= chaseTime)
                    {
                        isChasing = false;
                        loseTimer = 0f;
                    }
                }
                else
                {
                    loseTimer = 0f;
                }
            }
        }
        else
        {
            isChasing = false;
        }

        // Choose whether to chase or patrol
        if (isChasing && player != null)
        {
            MoveTo(player.position);
        }
        else
        {
            Patrol();
        }
    }

    // Moves Brute using the nav mesh 
    private void MoveTo(Vector2 destination)
    {
        if (agent == null || !agent.isOnNavMesh) return;

        // Tell the NavMesh where to go
        agent.SetDestination(destination);

        // Get the direction of the next path corner
        Vector2 nextPosition = new Vector2(agent.steeringTarget.x, agent.steeringTarget.y);

        // Move the brute toward the next path corner
        Vector2 newPosition = Vector2.MoveTowards(bruteRb.position, nextPosition, moveSpeed * Time.fixedDeltaTime);

        bruteRb.MovePosition(newPosition);

        // Keep the NavMeshAgent synced with the rigidbody
        agent.nextPosition = new Vector3(newPosition.x, newPosition.y, transform.position.z);
    }

    // Makes the  brute walk between points
    private void Patrol()
    {
        if (patrolPoint1 == null || patrolPoint2 == null) return;

       if (currentTarget == null)
        {
            currentTarget = patrolPoint1;
        }

        // Move towards the current patrol point
        MoveTo(currentTarget.position);

        // Switch points when close enough
        float distance = Vector2.Distance(bruteRb.position, currentTarget.position);

        if (distance < 0.1f)
        {
            currentTarget = currentTarget == patrolPoint1 ? patrolPoint2 : patrolPoint1;
        }
    }

    // Controls how the Brute attaks
    public override void Attack()
    {
        // Code will be added later
    }
}
