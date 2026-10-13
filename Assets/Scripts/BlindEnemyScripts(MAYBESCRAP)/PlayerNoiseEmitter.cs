
using UnityEngine;

public class PlayerNoiseEmitter
{
    private readonly float walkNoisePerSecond = 3f;
    private readonly float sprintNoisePerSecond = 8f;
    private readonly float walkSpeedThreshold = 0.1f;
    private readonly float sprintSpeedThreshold = 5f;

    private float currentSpeed;
    private Vector2 playerPosition;

    public PlayerNoiseEmitter()
    {
        NoiseEvents.OnPlayerSpeedChanged += HandleSpeedChanged;
    }

    private void HandleSpeedChanged(float speed, Vector2 position)
    {
        currentSpeed = speed;
        playerPosition = position;

        if (currentSpeed <= walkSpeedThreshold) return;

        float noiseRate = currentSpeed >= sprintSpeedThreshold ? sprintNoisePerSecond : walkNoisePerSecond;

        NoiseEvents.EmitNoise(playerPosition, noiseRate * Time.deltaTime);
        Debug.Log("NOISES");
    }

    public void Dispose()
    {
        NoiseEvents.OnPlayerSpeedChanged -= HandleSpeedChanged;
    }
}