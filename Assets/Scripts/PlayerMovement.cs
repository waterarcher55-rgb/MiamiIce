using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class PlayerMovement : MonoBehaviour
{
	Vector2 MoveVector;
	bool moving;
	bool looking;
	bool sprinting;
	bool aiming;
	

	[Header("Components")]
	public Rigidbody2D rb;
	public CinemachineCamera cam;
	ParticleSystem flash;

	[Header("GameObjects")]
	public GameObject PlayerModel;
	public GameObject MuzzleFlash;
	public GameObject CameraBrain;
	public GameObject CursorLocale;

	[Header("Player Stats")]
	public float baseSpeed = 5.0f;
	public float sprintMod = 2f;
	public float aimingMod = 0.5f;
	public float maxAimDist = 10f;
	
    void Start()
    {
		rb.GetComponent<Rigidbody2D>();
		cam = CameraBrain.GetComponent<CinemachineCamera>();

		flash = MuzzleFlash.GetComponent<ParticleSystem>();

		CursorLocale.gameObject.SetActive(true);

		MoveVector = new Vector2();
		moving = false;
		sprinting = false;
		aiming = false;
    }

    // Update is called once per frame
    void Update()
    {
		//Aim
		if (aiming)
		{
			if (cam.Follow == this.gameObject.transform)
			{
				CursorLocale.gameObject.SetActive(true);
				cam.Follow = CursorLocale.transform;
			}
		}


		//Looking
		Vector2 vec = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
		
		//CursorLocale.transform.position = new Vector2(x, y);

		float f = Mathf.Atan2(vec.x - PlayerModel.transform.position.x, vec.y - PlayerModel.transform.position.y) * Mathf.Rad2Deg;
		PlayerModel.transform.localRotation = Quaternion.Slerp(PlayerModel.transform.localRotation, Quaternion.Euler(0, 0, -f), Time.deltaTime * 10);
		
		//Movement
		if (MoveVector != Vector2.zero)
		{
			moving = true;
			float finalSpeed = baseSpeed;
			if (!aiming)
			{
				finalSpeed *= ((sprinting && !aiming) ? sprintMod : 1);
			}
			else
			{
				finalSpeed *= aimingMod;
			}
			rb.linearVelocity = new Vector2(MoveVector.x * finalSpeed, MoveVector.y * finalSpeed);
		}
		else if (MoveVector == Vector2.zero && moving)
		{
			rb.linearVelocity = Vector2.zero;
			moving = false;
		}
    }

	public void OnMove(InputAction.CallbackContext context)
	{
		MoveVector = context.ReadValue<Vector2>();
	}

	public void OnSprint(InputAction.CallbackContext context)
	{
		Debug.Log(context.ReadValue<float>());
		if (context.ReadValue<float>() > 0.4f)
		{
			sprinting = true;
			Debug.Log("Sprinting!");
		}
		else if (context.canceled)
		{
			sprinting = false;
			Debug.Log("Not Sprinting!");
		}
	}

	public void OnAim(InputAction.CallbackContext context)
	{
		aiming = true;
		if (context.canceled)
		{
			aiming = false;
			cam.Follow = this.transform;
			CursorLocale.gameObject.SetActive(false);
		}
	}

	public void OnAttack(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			Debug.Log("bang!");
			flash.Emit(10);
		}
		
	}
}
