using UnityEngine;

public class ShooterConeDetector : MonoBehaviour
{
    public GameObject root; // Drag your Shooter object here
    public Collider2D col;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            root.SendMessage("SeePlayer", other.gameObject.transform, SendMessageOptions.DontRequireReceiver);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            root.SendMessage("SeePlayer", other.gameObject.transform, SendMessageOptions.DontRequireReceiver);
        }
    }
}