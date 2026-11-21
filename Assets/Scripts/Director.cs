using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Director : MonoBehaviour
{
	[Header("Camera List")]
	[SerializeField] List<SecurityCamera> Cameras = new();

	[Header("Enemies. DOES NOT CONTAIN DRUNKS OR KNIVES (knives do not use NavMesh agents for some reason, and drunks don't make sense (they act like sentries)")]
	[SerializeField] List<BaseEnemy> Enemies = new();



	private void OnEnable()
	{
		SecurityCamera.OnPlayerSpotted += PlayerSpotted;
		SecurityCamera.OnCameraDestroyed += CameraDestroyed;
		BaseEnemy.OnEnemyKill += EnemyKilled;
	}

	private void OnDisable()
	{
		SecurityCamera.OnPlayerSpotted -= PlayerSpotted;
		SecurityCamera.OnCameraDestroyed -= CameraDestroyed;
		BaseEnemy.OnEnemyKill -= EnemyKilled;
	}

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
    {
		
	}

    // Update is called once per frame
    void Update()
    {
        
    }

	private void PlayerSpotted(SecurityCamera camera, Vector3 position)
	{
		Debug.Log("Player Spotted!");
		DispatchIntercept(position);
	}

	private void CameraDestroyed(SecurityCamera camera, Vector3 position)
	{
		DispatchInvestigate(position);
		Cameras.Remove(camera);

	}

	private void EnemyKilled(BaseEnemy enemy)
	{
		Enemies.Remove(enemy);
		Destroy(enemy.gameObject);
	}

	void DispatchIntercept(Vector3 position)
	{
		if (Enemies.Count > 2)
		{
			int temp = -1;
			
			int enemy1 = -1;
			while (enemy1 == -1)
			{
				temp = UnityEngine.Random.Range(0, Enemies.Count - 1);
				if (Enemies[temp].Status != EnemyStatus.Chasing)
				{
					enemy1 = temp;
				}
			}

			int enemy2 = -1;
			while (enemy2 == -1)
			{
				temp = UnityEngine.Random.Range(0, Enemies.Count - 1);
				if (Enemies[temp].Status != EnemyStatus.Chasing && temp != enemy1)
				{
					enemy2 = temp;
				}
			}

			Enemies[enemy1].ManualSend(position, EnemyStatus.Investigating);
			Enemies[enemy2].ManualSend(position, EnemyStatus.Investigating);
		}
		else if (Enemies.Count <= 2)
		{
			int temp = -1;

			int enemy1 = -1;
			while (enemy1 == -1)
			{
				temp = UnityEngine.Random.Range(0, Enemies.Count);
				if (Enemies[temp].Status != EnemyStatus.Chasing)
				{
					enemy1 = temp;
				}
			}
			Enemies[enemy1].ManualSend(position, EnemyStatus.Investigating);
		}
	}

	void DispatchInvestigate(Vector3 position)
	{
		if (Enemies.Count > 6)
		{
			int temp = -1;

			int enemy1 = -1;
			while (enemy1 == -1)
			{
				temp = UnityEngine.Random.Range(0, Enemies.Count - 1);
				if (Enemies[temp].Status != EnemyStatus.Chasing)
				{
					enemy1 = temp;
				}
			}

			int enemy2 = -1;
			while (enemy2 == -1)
			{
				temp = UnityEngine.Random.Range(0, Enemies.Count - 1);
				if (Enemies[temp].Status != EnemyStatus.Chasing && temp != enemy1)
				{
					enemy2 = temp;
				}
			}

			int enemy3 = -1;
			while (enemy3 == -1)
			{
				temp = UnityEngine.Random.Range(0, Enemies.Count - 1);
				if (Enemies[temp].Status != EnemyStatus.Chasing && temp != enemy1 && temp != enemy2)
				{
					enemy3 = temp;
				}
			}

			Enemies[enemy1].ManualSend(position, EnemyStatus.Investigating);
			Enemies[enemy2].ManualSend(position, EnemyStatus.Investigating);
			Enemies[enemy3].ManualSend(position, EnemyStatus.Investigating);
		}
		else if (Enemies.Count > 3)
		{
			int temp = -1;

			int enemy1 = -1;
			while (enemy1 == -1)
			{
				temp = UnityEngine.Random.Range(0, Enemies.Count - 1);
				if (Enemies[temp].Status != EnemyStatus.Chasing)
				{
					enemy1 = temp;
				}
			}

			int enemy2 = -1;
			while (enemy2 == -1)
			{
				temp = UnityEngine.Random.Range(0, Enemies.Count - 1);
				if (Enemies[temp].Status != EnemyStatus.Chasing && temp != enemy1)
				{
					enemy2 = temp;
				}
			}

			Enemies[enemy1].ManualSend(position, EnemyStatus.Investigating);
			Enemies[enemy2].ManualSend(position, EnemyStatus.Investigating);
		}
		else if (Enemies.Count <= 3)
		{
			int temp = -1;

			int enemy1 = -1;
			while (enemy1 == -1)
			{
				temp = UnityEngine.Random.Range(0, Enemies.Count - 1);
				if (Enemies[temp].Status != EnemyStatus.Chasing)
				{
					enemy1 = temp;
				}
			}
			Enemies[enemy1].ManualSend(position, EnemyStatus.Investigating);
		}
	}
}