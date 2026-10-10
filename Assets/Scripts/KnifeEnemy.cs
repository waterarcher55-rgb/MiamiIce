
using System.Collections;
using UnityEngine;

public class KnifeEnemy : BaseEnemy
{
    [Header("Knife Runner Settings")]
    [SerializeField] private float attackRange = 1.3f;
    [SerializeField] private float attackCooldown = 1.0f;
    [SerializeField] private float delayBetweenHits = 0.15f;
    [SerializeField] private float recoveryPause = 2f;

    private Rigidbody2D rb;
    private Transform player;

    private float nextAttackTime;
    private bool isAttacking;

    protected override void Awake()
    {
      
        if (maxHealth <= 0f) maxHealth = 70f;
        if (moveSpeed <= 0f) moveSpeed = 6.5f;
        if (damage <= 0f) damage = 6f;

        base.Awake();

        rb = GetComponent<Rigidbody2D>();

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogWarning(
                "KnifeEnemy could not find a GameObject tagged Player.",
                this
            );
        }

        if (rb == null)
        {
            Debug.LogError(
                "KnifeEnemy needs a Rigidbody2D component.",
                this
            );
        }
    }


    private void Update()
    {
        if (rb == null)
            return;

        if (player == null)
        {
            GameObject playerObject =
                GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
                player = playerObject.transform;

            rb.linearVelocity = Vector2.zero;
            return;
        }

        // Stay still while attacking or recovering.
        if (isAttacking)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        float distance = Vector2.Distance(
            transform.position,
            player.position
        );

        if (distance > attackRange)
        {
            // Chase the player.
            Vector2 direction =
                ((Vector2)player.position -
                 (Vector2)transform.position).normalized;

            rb.linearVelocity = direction * moveSpeed;
        }
        else
        {
            // Stop and begin a double attack.
            rb.linearVelocity = Vector2.zero;

            if (Time.time >= nextAttackTime)
            {
                StartCoroutine(DoubleAttack());
            }
        }
    }



    private IEnumerator DoubleAttack()
    {
        isAttacking = true;

        LogHitIfInRange(1);
        yield return new WaitForSeconds(delayBetweenHits);

        LogHitIfInRange(2);
        yield return new WaitForSeconds(recoveryPause);

        nextAttackTime = Time.time + attackCooldown;
        isAttacking = false;
    }


    private void LogHitIfInRange(int hitNumber)
    {
        if (player == null)
            return;

        float distance = Vector2.Distance(
            transform.position,
            player.position
        );

        if (distance <= attackRange)
        {
            Debug.Log(
                "KNIFE RUNNER HIT PLAYER! " +
                "Hit " + hitNumber + "/2. " +
                "Damage: " + damage +
                ". Distance: " + distance.ToString("F2"),
                this
            );
        }
        else
        {
            Debug.Log(
                "Knife Runner missed hit " + hitNumber +
                " because the player moved out of range.",
                this
            );
        }
    }


    public override void Attack()
    {
       
    }

    public void ReceiveDamage(float amount)
    {
        TakeDamage(amount);
    }

}
