using System;
using System.Collections;
using UnityEngine;

public class SecurityCamera : MonoBehaviour
{
    public static event Action<SecurityCamera, Vector3> OnPlayerSpotted;
    public static event Action<SecurityCamera, Vector3> OnCameraDestroyed;

    [Header("Detection Settings")]
    [SerializeField] private float viewDistance = 8f;
    [SerializeField] private float viewAngle = 60f;
    [SerializeField] private float scanInterval = 0.1f;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Vision Cone Visuals (Child Reference)")]
    [SerializeField] private MeshFilter coneMeshFilter;
    [SerializeField] private MeshRenderer coneMeshRenderer;
    [SerializeField] private int coneRayCount = 30;
    [SerializeField, Range(0f, 1f)] private float coneAlpha = 0.25f;

    [Header("Sweep & Tracking Rotation")]
    [SerializeField] private bool sweepBackAndForth = true;
    [SerializeField] private float sweepAngle = 45f;
    [SerializeField] private float sweepSpeed = 1.5f;
    [SerializeField] private float trackSpeed = 8f;

    [Header("Health & State")]
    [SerializeField] private float maxHealth = 25f;
    [SerializeField] private MeshRenderer cameraRenderer;
    [SerializeField] private Color normalColor = Color.green;
    [SerializeField] private Color alertColor = Color.red;
    [SerializeField] private Color destroyedColor = Color.gray;
	[SerializeField] private bool SentForBackup = false;

    private float currentHealth;
    private bool isDestroyed = false;
    private bool isPlayerSpotted = false;
    private Vector3 startRotation;
    private float sweepTimer = 0f;
    private Transform playerTransform;
    private Mesh coneMesh;
    private Material coneMaterial;

	Collider2D col;

    public bool IsDestroyed => isDestroyed;

	void Awake()
	{
		col = GetComponent<Collider2D>();
	}


    private void Start()
    {
        currentHealth = maxHealth;
        startRotation = transform.eulerAngles;

        if (cameraRenderer == null)
            cameraRenderer = GetComponent<MeshRenderer>();

        SetupVisionCone();
        SetVisualState(normalColor);

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            playerTransform = playerObj.transform;

        StartCoroutine(ScanRoutine());
    }

    private void SetupVisionCone()
    {
        if (coneMeshFilter == null || coneMeshRenderer == null)
        {
            Debug.LogError("Please assign the child VisionConeVisual components in the Inspector!");
            return;
        }

        coneMesh = new Mesh();
        coneMesh.name = "VisionConeMesh";
        coneMeshFilter.mesh = coneMesh;

        Shader shader = Shader.Find("Sprites/Default");
        coneMaterial = new Material(shader);
        coneMeshRenderer.material = coneMaterial;
        coneMeshRenderer.sortingOrder = 5;
    }

    private void Update()
    {
        if (isDestroyed) return;

        if (isPlayerSpotted && playerTransform != null)
        {
            // Smoothly track the player
            Vector3 dirToPlayer = playerTransform.position - transform.position;
            dirToPlayer.z = 0f;

            float targetAngleZ = Mathf.Atan2(dirToPlayer.y, dirToPlayer.x) * Mathf.Rad2Deg - 90f;
            Quaternion targetRotation = Quaternion.Euler(startRotation.x, startRotation.y, targetAngleZ);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * trackSpeed);

            // Continuously update sweepTimer so it matches our current angle when we eventually lose the player
            float currentAngleOffset = Mathf.DeltaAngle(startRotation.z, transform.eulerAngles.z);
            currentAngleOffset = Mathf.Clamp(currentAngleOffset, -sweepAngle, sweepAngle);

            float clampedRatio = Mathf.Clamp(currentAngleOffset / sweepAngle, -1f, 1f);
            sweepTimer = Mathf.Asin(clampedRatio);
        }
        else if (sweepBackAndForth)
        {
            // Advance the sweep smoothly from where it left off
            sweepTimer += Time.deltaTime * sweepSpeed;
            float angleOffset = Mathf.Sin(sweepTimer) * sweepAngle;
            transform.rotation = Quaternion.Euler(startRotation.x, startRotation.y, startRotation.z + angleOffset);
        }
    }

    private void LateUpdate()
    {
        if (!isDestroyed && coneMesh != null)
            DrawVisionCone();
    }

    private void DrawVisionCone()
    {
        int stepCount = Mathf.Max(2, coneRayCount);
        float stepAngleSize = viewAngle / stepCount;

        Vector3[] vertices = new Vector3[stepCount + 2];
        int[] triangles = new int[stepCount * 3];

        vertices[0] = coneMeshFilter.transform.InverseTransformPoint(transform.position);

        for (int i = 0; i <= stepCount; i++)
        {
            float localAngle = -((viewAngle * 0.5f) - (stepAngleSize * i));
            Vector2 worldDir = Quaternion.Euler(0f, 0f, localAngle) * transform.up;

            RaycastHit2D hit = Physics2D.Raycast(transform.position, worldDir, viewDistance, obstacleLayer);
            float dist = (hit.collider != null) ? hit.distance : viewDistance;

            Vector3 worldPos = transform.position + (Vector3)(worldDir * dist);
            vertices[i + 1] = coneMeshFilter.transform.InverseTransformPoint(worldPos);

            if (i < stepCount)
            {
                int triIndex = i * 3;
                triangles[triIndex] = 0;
                triangles[triIndex + 1] = i + 1;
                triangles[triIndex + 2] = i + 2;
            }
        }

        coneMesh.Clear();
        coneMesh.vertices = vertices;
        coneMesh.triangles = triangles;
        coneMesh.RecalculateNormals();
    }

    private IEnumerator ScanRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(scanInterval);

        while (!isDestroyed)
        {
            CheckForPlayer();
            yield return wait;
        }
    }

    private void CheckForPlayer()
    {
        if (playerTransform == null)
        {
            LosePlayer();
            return;
        }

        Vector2 dirToPlayer = (Vector2)(playerTransform.position - transform.position);
        float distToPlayer = dirToPlayer.magnitude;

        if (distToPlayer > viewDistance)
        {
            LosePlayer();
            return;
        }

        float angle = Vector2.Angle(transform.up, dirToPlayer.normalized);
        if (angle > viewAngle * 0.5f)
        {
            LosePlayer();
            return;
        }

        RaycastHit2D hit = Physics2D.Raycast(transform.position, dirToPlayer.normalized, distToPlayer, obstacleLayer);
        if (hit.collider != null)
        {
            LosePlayer();
            return;
        }

        isPlayerSpotted = true;
        SetVisualState(alertColor);
		if (!SentForBackup)
		{
			OnPlayerSpotted?.Invoke(this, playerTransform.position);
			SentForBackup = true;
			Invoke(nameof(BackupCooldown), 10);
		}
    }

    private void LosePlayer()
    {
        isPlayerSpotted = false;
		CancelInvoke(nameof(BackupCooldown));
		BackupCooldown();
        SetVisualState(normalColor);
    }

    public void TakeDamage(float amount)
    {
        if (isDestroyed) return;

        currentHealth -= amount;
        if (currentHealth <= 0f)
        {
            DestroyCamera();
        }
    }

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (collision.gameObject.CompareTag("Bullet") || collision.gameObject.CompareTag("Projectile"))
			TakeDamage(25f);
	}


	//private void OnTriggerEnter2D(Collider2D collision)
	//{
	//	if (collision.CompareTag("Bullet") || collision.CompareTag("Projectile"))
	//		TakeDamage(25f);
	//}

	private void DestroyCamera()
    {
        isDestroyed = true;
        isPlayerSpotted = false;
        SetVisualState(destroyedColor);

        if (coneMeshRenderer != null)
            coneMeshRenderer.enabled = false;

        OnCameraDestroyed?.Invoke(this, transform.position);
    }

    private void SetVisualState(Color c)
    {
        if (cameraRenderer != null && cameraRenderer.material.color != c)
            cameraRenderer.material.color = c;

        if (coneMaterial != null)
        {
            Color translucent = new Color(c.r, c.g, c.b, coneAlpha);
            if (coneMaterial.color != translucent)
                coneMaterial.color = translucent;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewDistance);

        Vector3 leftBoundary = Quaternion.Euler(0, 0, viewAngle * 0.5f) * transform.up * viewDistance;
        Vector3 rightBoundary = Quaternion.Euler(0, 0, -viewAngle * 0.5f) * transform.up * viewDistance;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);
    }

	private void BackupCooldown()
	{
		SentForBackup = false;
	}
}