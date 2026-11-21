using UnityEngine;

public class PlayerBullet : MonoBehaviour
{
	public float speed = 12f;
	public float lifetime = 3f;
	private Vector2 moveDir = Vector2.up;
	[SerializeField] float damage = 25;

	void Start()
	{
		Destroy(gameObject, lifetime);
	}

	public void SetDirection(Vector2 dir)
	{
		moveDir = dir;
	}

	void Update()
	{
		transform.position += (Vector3)(moveDir * speed * Time.deltaTime);
	}

	//private void OnTriggerEnter2D(Collider2D other)
	//{
	//	if (other.CompareTag("Enemy"))
	//	{
	//		Debug.Log("Bullet hit an enemy!");
	//		other.gameObject.SendMessage("TakeDamage", 25);
	//		Destroy(gameObject);
	//	}
	//	else if (other.CompareTag("Wall"))
	//	{
	//		Debug.Log("Bullet hit a wall!");
	//		Destroy(gameObject);
	//	}
	//	else
	//	{
	//		Debug.Log($"Bullet hit something else idk\nTag: {other.tag}\nLayer:{other.gameObject.layer}");
	//	}
		
	//}

	private void OnCollisionEnter2D(Collision2D other)
	{
		if (other.gameObject.CompareTag("Enemy"))
		{
			Debug.Log("Bullet hit an enemy!");
			other.gameObject.SendMessage("TakeDamage", 25);
			Destroy(gameObject);
		}
		else if (other.gameObject.CompareTag("Wall"))
		{
			Debug.Log("Bullet hit a wall!");
			Destroy(gameObject);
		}
		else
		{
			Debug.Log($"Bullet hit something else idk\nTag: {other.gameObject.tag}\nLayer:{other.gameObject.layer}");
		}
	}
}
