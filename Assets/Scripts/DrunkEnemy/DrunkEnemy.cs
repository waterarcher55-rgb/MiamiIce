using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class PatrolChaseEnemy : BaseEnemy
{
    private enum AIState
    {
        Idle,
        Patrol,
        Attack
    }

    [Header("Stage")]
    [SerializeField] private StageTimer stageTimer;

    [Header("Cone Detection")]
    [SerializeField] private ConeDetector coneDetector;

    [Header("Player")]
    [SerializeField] private Transform player;

    //Layers that block the enemy's line of sight. Implement later
    [SerializeField] private LayerMask visionBlockers;

    //Time to wait before returning to patrol after losing sight.
    [SerializeField, Min(0f)] private float loseSightDelay = 0.75f;

    [SerializeField, Min(0f)] private float turnSpeed = 540f;

    [Tooltip("Rotation offset if the enemy's forward direction is not local right.")]
    [SerializeField] private float facingAngleOffset = 0f;

    [Header("Patrol")]
    [SerializeField] private Transform[] patrolPoints;

    [SerializeField, Min(0f)] private float waypointReachDistance = 0.2f;

    [SerializeField, Min(0f)] private float chaseStoppingDistance = 0.2f;

    [SerializeField, Min(0.05f)] private float chaseUpdateInterval = 0.1f;

    private AIState currentState = AIState.Idle;

    private Transform cachedPlayerColliderRoot;
    private Collider2D[] playerColliders = new Collider2D[0];

    private int patrolIndex;
    private float lastTimePlayerSeen;
    private float nextChaseDestinationTime;

    private bool hasPatrolDestination;
    private bool warnedAboutPatrolPoints;

    protected override void Awake()
    {
        base.Awake();

        //NavMeshPlus2D config
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.speed = moveSpeed;
        agent.stoppingDistance = 0f;
        agent.autoBraking = false;

        if (coneDetector == null)
        {
            coneDetector = GetComponentInChildren<ConeDetector>(true);
        }
    }

    private void OnEnable()
    {
        if (stageTimer != null)
        {
            stageTimer.TimerExpired += OnStageTimerExpired; //Subscribe to event in StageTimer
        }
    }

    private void OnDisable()
    {
        if (stageTimer != null)
        {
            stageTimer.TimerExpired -= OnStageTimerExpired;
        }
    }

    private void Start()
    {
        //Find the player automatically if not assigned.
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            {
                SetPlayerTarget(playerObject.transform);
            }
        }
        else
        {
            SetPlayerTarget(player);
        }

        if (coneDetector == null || coneDetector.col == null)
        {
            Debug.LogError("Cone Detector Missing");
        }

        if (stageTimer == null)
        {
            Debug.LogError("Stage Timer Missing");

            EnterIdle();
            return;
        }

        if (!agent.isOnNavMesh)
        {
            Debug.LogError("NavMesh Not found");
        }

        //If stage timer is set to 0, immediate aggression
        if (stageTimer.HasExpired)
        {
            BeginHunting();
        }
        else
        {
            EnterIdle();
        }
    }

    private void Update()
    {
        if (agent == null || !agent.isActiveAndEnabled)
            return;

        //Check the timer before requiring the agent to be on the NavMesh.
        if (currentState == AIState.Idle && stageTimer != null && stageTimer.HasExpired)
        {
            BeginHunting();
        }

        if (!agent.isOnNavMesh)
            return;

        switch (currentState)
        {
            case AIState.Idle:
                break;

            case AIState.Patrol:
                UpdatePatrol();
                break;

            case AIState.Attack:
                UpdateAttackState();
                break;
        }
    }

    //Called by StageTimer when the timer reaches zero
    private void OnStageTimerExpired()
    {
        BeginHunting();
    }

    private void EnterIdle()
    {
        currentState = AIState.Idle;
        hasPatrolDestination = false;

        if (agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }

    private void BeginHunting()
    {
        if (currentState != AIState.Idle)
            return;

        currentState = AIState.Patrol;

        if (!agent.isOnNavMesh)
        {
            hasPatrolDestination = false;
            return;
        }

        agent.isStopped = false;
        agent.stoppingDistance = 0f;
        agent.autoBraking = false;

        //If the player is still in the cone when hunting begins, immediately begin hunting
        if (IsPlayerVisible())
        {
            EnterAttackState();
            return;
        }

        hasPatrolDestination = SetPatrolDestination();

        if (!hasPatrolDestination) WarnAboutPatrolPoints(); //Alert if no patrol points set
    }

    //ConeDetector calls this through: root.SendMessage("SeePlayer", other.gameObject.transform);
    public void SeePlayer(Transform seenPlayer)
    {
        if (seenPlayer == null)
            return;

        SetPlayerTarget(seenPlayer);

        if (currentState == AIState.Idle)
            return;

        //Cone entry identifies a possible target. Confirm the overlap and any configured line-of-sight blockers.
        if (!IsPlayerVisible())
            return;

        lastTimePlayerSeen = Time.time;

        if (currentState == AIState.Patrol)
        {
            EnterAttackState();
        }
    }

    private void SetPlayerTarget(Transform target)
    {
        if (target == null)
            return;

        if (player == target && cachedPlayerColliderRoot == target)
        {
            return;
        }

        player = target;
        cachedPlayerColliderRoot = target;

        playerColliders = target.GetComponentsInChildren<Collider2D>();
    }

    private void UpdatePatrol()
    {
        //The cone determines whether the player is visible.
        if (IsPlayerVisible())
        {
            EnterAttackState();
            return;
        }

        //Face current direction during patrol
        FaceDirection(agent.desiredVelocity);

        //Try again if the agent not on NavMesh
        if (!hasPatrolDestination)
        {
            hasPatrolDestination = SetPatrolDestination();

            if (!hasPatrolDestination)
            {
                WarnAboutPatrolPoints();
                return;
            }
        }

        if (agent.pathPending) return;

        //Move to next waypoint after reaching the current one.
        if (agent.remainingDistance <= waypointReachDistance)
        {
            if (patrolPoints == null || patrolPoints.Length == 0) return;

            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;

            hasPatrolDestination = SetPatrolDestination();

            if (!hasPatrolDestination) WarnAboutPatrolPoints(); //Alert if no patrol points set
        }
    }

    private void EnterAttackState()
    {
        currentState = AIState.Attack;

        lastTimePlayerSeen = Time.time;
        nextChaseDestinationTime = 0f;

        agent.isStopped = false;
        agent.stoppingDistance = chaseStoppingDistance;
        agent.autoBraking = true;
    }

    private void UpdateAttackState()
    {
        if (player == null)
        {
            EnterPatrolState();
            return;
        }

        //Tracks player
        FacePosition(player.position);

        bool canCurrentlySeePlayer = IsPlayerVisible();

        if (canCurrentlySeePlayer)
        {
            lastTimePlayerSeen = Time.time;
        }
        else if (Time.time - lastTimePlayerSeen >= loseSightDelay)
        {
            EnterPatrolState();
            return;
        }

        //Continue updating the destination while chasing.
        if (Time.time >= nextChaseDestinationTime)
        {
            agent.SetDestination(player.position);

            nextChaseDestinationTime = Time.time + chaseUpdateInterval;
        }

        //Attack/damage logic probably goes here when implemented
    }

    private void EnterPatrolState()
    {
        currentState = AIState.Patrol;

        agent.isStopped = false;
        agent.stoppingDistance = 0f;
        agent.autoBraking = false;

        //Resume the previously selected waypoint when player is lost
        hasPatrolDestination = SetPatrolDestination();

        if (!hasPatrolDestination) WarnAboutPatrolPoints(); //Alert if no patrol points set
    }

    private bool SetPatrolDestination()
    {
        if (patrolPoints == null || patrolPoints.Length == 0 || !agent.isOnNavMesh)
        {
            return false;
        }

        //Search for a valid waypoint.
        for (int i = 0; i < patrolPoints.Length; i++)
        {
            Transform waypoint = patrolPoints[patrolIndex];

            if (waypoint != null && agent.SetDestination(waypoint.position))
            {
                return true;
            }

            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
        }

        return false;
    }

    private bool IsPlayerVisible()
    {
        //Baseline Checks
        if (player == null || coneDetector == null || coneDetector.col == null || !coneDetector.col.enabled || !coneDetector.gameObject.activeInHierarchy)
        {
            return false;
        }

        //Check overlap with the cone collider
        bool overlapsCone = false;

        foreach (Collider2D playerCollider in playerColliders)
        {
            if (playerCollider == null || !playerCollider.enabled || !playerCollider.gameObject.activeInHierarchy)
                continue;

            ColliderDistance2D distance = coneDetector.col.Distance(playerCollider);

            if (distance.isOverlapped)
            {
                overlapsCone = true;
                break;
            }
        }

        if (!overlapsCone) return false;

        //Empty Mask, no line-of-sight blockers
        if (visionBlockers.value == 0) return true;

        RaycastHit2D hit = Physics2D.Linecast(transform.position, player.position, visionBlockers);

        return hit.collider == null;
    }

    private void FacePosition(Vector3 targetPosition)
    {
        Vector2 direction = (Vector2)(targetPosition - transform.position);

        FaceDirection(direction);
    }

    private void FaceDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.0001f) return;

        //Local +X is forward, Important for Cone
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        angle += facingAngleOffset;

        Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle);

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
    }

    private void WarnAboutPatrolPoints()
    {
        if (warnedAboutPatrolPoints) return;
        warnedAboutPatrolPoints = true;
        Debug.LogWarning("No Patrol Point Found");
    }

    public override void Attack()
    {
        //Attack behavior will be implemented later.
    }
}