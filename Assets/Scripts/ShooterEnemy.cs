using UnityEngine;
using UnityEngine.AI;

public class ShooterEnemy : BaseEnemy
{
    [Header("Shooter Specific Components")]
    public Transform firePoint;
    public GameObject bulletPrefab;
    private Transform Target = null;

    [Header("Shooter Stats & Behavior")]
    [SerializeField] private float preferredRange = 6f;
    [SerializeField] private float stopRange = 4f;
    [SerializeField] private float chaseDistance = 12f;
    [SerializeField] private float fireRate = 1.5f;
    private float nextFireTime = 0f;

    [Header("State Flags")]
    [SerializeField] private bool isAlerted = false;
    [SerializeField] private bool searching = false;
    [SerializeField] private bool returning = false;

    [Header("Search Settings")]
    [SerializeField] private float searchDuration = 2.5f;
    private float searchTimer = 0f;
    private Vector2 lastKnownPlayerPos;
    private Vector2 guardPosition;

    void Start()
    {
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.speed = moveSpeed;

        guardPosition = transform.position;
    }

    void Update()
    {
        // -------------------------------------------------------------
        // 1. COMBAT / ALERT STATE
        // -------------------------------------------------------------
        if (isAlerted && Target != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, Target.position);

            // Continuous Line-of-Sight Check
            ContactFilter2D filter = new ContactFilter2D { useTriggers = false };
            RaycastHit2D[] hit = new RaycastHit2D[3];
            Physics2D.Linecast(transform.position, Target.position, filter, hit);
            bool hasLineOfSight = hit.Length > 1 && hit[1].transform != null && hit[1].transform.CompareTag("Player");

            // If player ran out of chase range or broke line of sight
            if (distanceToPlayer > chaseDistance || !hasLineOfSight)
            {
                StartSearching();
                return;
            }

            // ALWAYS face the player during combat (whether chasing, retreating, or holding ground)
            Vector2 dir = Target.position - transform.position;
            float f = Mathf.Atan2(dir.x, -dir.y) * Mathf.Rad2Deg;
            mesh.transform.rotation = Quaternion.Slerp(mesh.transform.rotation, Quaternion.Euler(0, 0, f), Time.deltaTime * 12f);

            // Movement & Shooting / Retreating Logic
            if (distanceToPlayer > preferredRange)
            {
                agent.isStopped = false;
                agent.SetDestination(Target.position);
            }
            else if (distanceToPlayer < stopRange)
            {
                // Retreat safely using NavMesh sampling with a sideways slide fallback
                agent.isStopped = false;
                Vector2 retreatDir = (transform.position - Target.position).normalized;
                Vector2 rawRetreatPos = (Vector2)transform.position + (retreatDir * 3f);

                NavMeshHit navHit;
                if (NavMesh.SamplePosition(rawRetreatPos, out navHit, 2f, NavMesh.AllAreas))
                {
                    agent.SetDestination(navHit.position);
                }
                else
                {
                    // Fallback: If straight back is blocked by a wall, slide sideways (perpendicular)
                    Vector2 perpDir = new Vector2(-retreatDir.y, retreatDir.x);
                    Vector2 sideRetreatPos = (Vector2)transform.position + (perpDir * 2.5f);

                    if (NavMesh.SamplePosition(sideRetreatPos, out navHit, 2f, NavMesh.AllAreas))
                    {
                        agent.SetDestination(navHit.position);
                    }
                    else
                    {
                        agent.isStopped = true;
                    }
                }

                // ALLOW FIRING WHILE RETREATING
                if (Time.time >= nextFireTime)
                {
                    Attack();
                    nextFireTime = Time.time + fireRate;
                }
            }
            else
            {
                agent.isStopped = true;

                // Stand ground and fire
                if (Time.time >= nextFireTime)
                {
                    Attack();
                    nextFireTime = Time.time + fireRate;
                }
            }
        }

        // -------------------------------------------------------------
        // 2. SEARCH / INVESTIGATE STATE
        // -------------------------------------------------------------
        if (searching)
        {
            if (agent.velocity.sqrMagnitude > 0.01f)
            {
                float f = Mathf.Atan2(agent.velocity.x, -agent.velocity.y) * Mathf.Rad2Deg;
                mesh.transform.rotation = Quaternion.Slerp(mesh.transform.rotation, Quaternion.Euler(0, 0, f), Time.deltaTime * 10f);
            }

            searchTimer -= Time.deltaTime;

            if (searchTimer <= 0f)
            {
                ReturnHome();
            }
        }

        // -------------------------------------------------------------
        // 3. RETURNING TO GUARD POST STATE
        // -------------------------------------------------------------
        if (returning)
        {
            if (agent.velocity.sqrMagnitude > 0.01f)
            {
                float f = Mathf.Atan2(agent.velocity.x, -agent.velocity.y) * Mathf.Rad2Deg;
                mesh.transform.rotation = Quaternion.Slerp(mesh.transform.rotation, Quaternion.Euler(0, 0, f), Time.deltaTime * 10f);
            }

            if (!agent.pathPending && agent.remainingDistance <= 0.2f)
            {
                agent.isStopped = true;
                returning = false;
            }
        }
    }

    public void SeePlayer(Transform playerPos)
    {
        ContactFilter2D filter = new ContactFilter2D { useTriggers = false };
        RaycastHit2D[] hit = new RaycastHit2D[3];

        Physics2D.Linecast(transform.position, playerPos.position, filter, hit);
        if (hit.Length > 1 && hit[1].transform != null && hit[1].transform.gameObject.CompareTag("Player"))
        {
            if (!isAlerted || searching || returning)
            {
                Debug.Log("Shooter spotted player!");
                Target = playerPos;
                isAlerted = true;
                searching = false;
                returning = false;
                agent.isStopped = false;
            }
        }
    }

    void StartSearching()
    {
        if (!searching && !returning)
        {
            Debug.Log("Shooter lost line of sight, investigating last known position.");
            if (Target != null)
            {
                Vector2 rawLastPos = Target.position;
                NavMeshHit navHit;
                if (NavMesh.SamplePosition(rawLastPos, out navHit, 2f, NavMesh.AllAreas))
                {
                    lastKnownPlayerPos = navHit.position;
                }
                else
                {
                    lastKnownPlayerPos = rawLastPos;
                }
            }

            isAlerted = false;
            searching = true;
            searchTimer = searchDuration;

            agent.isStopped = false;
            agent.speed = moveSpeed;
            agent.SetDestination(lastKnownPlayerPos);
        }
    }

    void ReturnHome()
    {
        Debug.Log("Area clear, returning to post.");
        searching = false;
        returning = true;

        agent.isStopped = false;
        agent.speed = moveSpeed;
        agent.SetDestination(guardPosition);
    }

    public override void Attack()
    {
        Debug.Log("Shooter Firing!");
        if (bulletPrefab != null && firePoint != null && Target != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

            Vector2 shootDir = (Target.position - firePoint.position).normalized;
            float bulletAngle = Mathf.Atan2(shootDir.x, -shootDir.y) * Mathf.Rad2Deg;
            bullet.transform.rotation = Quaternion.Euler(0, 0, bulletAngle);

            Bullet bulletScript = bullet.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.SetDirection(shootDir);
            }
        }
    }
}