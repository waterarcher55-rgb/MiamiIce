using UnityEngine;

public class ConeDetector : MonoBehaviour
{
	// Start is called once before the first execution of Update after the MonoBehaviour is created
	public GameObject root;
	public Collider2D col;


	private void OnTriggerEnter2D(Collider2D other)
	{
		Debug.Log("I saw something!");
		if (other.gameObject.tag.Equals("Player"))
		{
			Debug.Log("MIAMI!!!");
			root.SendMessage("SeePlayer", other.gameObject.transform);
		}
	}
}
