using System;
using UnityEngine;

//This is currently only for the Blind enemy, uses events to announce when a noise is made
public static class NoiseEvents
{
    public static event Action<Vector2, float> OnNoiseEmitted;
    public static event Action<float, Vector2> OnPlayerSpeedChanged;

    //Sends speed from PlayerMovement to PlayerNoiseEmitter
    public static void PublishSpeed(float speed, Vector2 position) 
    {
        OnPlayerSpeedChanged?.Invoke(speed, position);
    }

    //Sends noise from PlayerNoiseEmitter to BlindEnemy
    public static void EmitNoise(Vector2 position, float loudness)
    {
        OnNoiseEmitted?.Invoke(position, loudness);
    }
}