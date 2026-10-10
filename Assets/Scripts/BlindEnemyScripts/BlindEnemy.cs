using UnityEngine;
using UnityEngine.AI;

public class BlindEnemy : BaseEnemy
{
    public enum EnemyState
    {
        Dormant,
        Hunting,
        Patrol
    }

    [Header("Hearing")]
    [SerializeField] private float hearingRadius = 12f;
    [SerializeField] private float proximityMultiplier = 2f;
    [SerializeField] private float activationThreshold = 60f;

    [Header("Commotion")]
    [SerializeField] private float maxCommotion = 100f;
    [SerializeField] private float dormantDecayRate = 2f;
    [SerializeField] private float alertDecayRate = 5f;
    [SerializeField] private float patrolCommotionFloor = 25f;
    [SerializeField] private float patrolThreshold = 25f;
    [SerializeField] private float huntThreshold = 60f;

    [Header("Movement")]
    [SerializeField] private float patrolSpeed = 1.5f;
    [SerializeField] private float minHuntSpeed = 3f;
    [SerializeField] private float maxHuntSpeed = 7f;
    [SerializeField] private float patrolRadius = 6f;
    [SerializeField] private float destinationTolerance = 0.5f;

    [Header("Debug")] //Temp, for testing
    [SerializeField] private EnemyState state = EnemyState.Dormant;
    [SerializeField, Range(0f, 100f)] private float commotion;

    private Vector2 lastNoisePosition;
    private Vector3 spawnPosition;
    private bool hasActivated;

    public float Commotion => commotion;
    public EnemyState CurrentState => state;

    protected override void Awake()
    {
        base.Awake();

        spawnPosition = transform.position;

        if (agent != null)
        {
            agent.updateRotation = false;
            agent.updateUpAxis = false;
            agent.isStopped = true;
        }
    }

    private void OnEnable()
    {
        NoiseEvents.OnNoiseEmitted += HandleNoise;
    }

    private void OnDisable()
    {
        NoiseEvents.OnNoiseEmitted -= HandleNoise;
    }

    private void HandleNoise(Vector2 noisePosition, float loudness)
    {
        float distance = Vector2.Distance(transform.position, noisePosition);

        if (distance > hearingRadius || loudness <= 0f) return;

        float proximity = 1f - Mathf.Clamp01(distance / hearingRadius);

        float multiplier = 1f + proximityMultiplier * proximity;

        // Every noise event increases accumulated awareness.
        commotion = Mathf.Clamp(commotion + loudness * multiplier, 0f, maxCommotion);

        lastNoisePosition = noisePosition;

        // Activation is permanent.
        if (!hasActivated && commotion >= activationThreshold)
        {
            hasActivated = true;
            state = EnemyState.Hunting;

            if (agent != null) agent.isStopped = false;
        }
        else if (hasActivated && state == EnemyState.Patrol && commotion >= huntThreshold)
        {
            state = EnemyState.Hunting;
        }
    }

    private void Update()
    {
        if (agent == null || !agent.isOnNavMesh)
            return;

        //Decay happens continuously
        float decay = hasActivated ? alertDecayRate : dormantDecayRate;

        float floor = hasActivated ? patrolCommotionFloor : 0f;

        commotion = Mathf.Max(floor, commotion - decay * Time.deltaTime);

        switch (state)
        {
            case EnemyState.Dormant:
                agent.isStopped = true;
                break;

            case EnemyState.Hunting:
                UpdateHunting();
                break;

            case EnemyState.Patrol:
                UpdatePatrol();
                break;
        }
    }

    private void UpdateHunting()
    {
        agent.isStopped = false;
        agent.speed = CalculateHuntSpeed();

        // Investigate the last position where noise was heard.
        agent.SetDestination(lastNoisePosition);

        if (commotion <= patrolThreshold)
        {
            state = EnemyState.Patrol;
            ChoosePatrolDestination();
        }
    }

    private void UpdatePatrol()
    {
        agent.isStopped = false;
        agent.speed = patrolSpeed;

        if (!agent.pathPending && (!agent.hasPath || agent.remainingDistance <= destinationTolerance))
        {
            ChoosePatrolDestination();
        }
    }

    private float CalculateHuntSpeed()
    {
        float t = Mathf.InverseLerp(patrolCommotionFloor, maxCommotion, commotion);

        return Mathf.Lerp(minHuntSpeed, maxHuntSpeed, t);
    }

    private void ChoosePatrolDestination()
    {
        for (int i = 0; i < 10; i++)
        {
            Vector2 offset =
                Random.insideUnitCircle * patrolRadius;

            Vector3 candidate = spawnPosition + new Vector3(offset.x, offset.y, 0f);

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, patrolRadius, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
                return;
            }
        }

        agent.SetDestination(spawnPosition);
    }

    public override void Attack()
    {
        //To be implemented once we have actual combat
    }
}