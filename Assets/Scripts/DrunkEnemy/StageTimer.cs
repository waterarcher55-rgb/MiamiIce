using System;
using UnityEngine;

public class StageTimer : MonoBehaviour //Simple Timer script, currently only for DrunkEnemy
{
    [Header("Stage Settings")]
    [SerializeField, Min(0f)] private float stageDuration = 30f;

    private float TimeRemaining;
    public bool HasExpired;

    public event Action TimerExpired;

    private void Awake()
    {
        TimeRemaining = Mathf.Max(0f, stageDuration);
        HasExpired = TimeRemaining <= 0f; //false if time is not set to 0
    }

    private void Update()
    {
        if (HasExpired)
            return;

        TimeRemaining = Mathf.Max(0f, TimeRemaining - Time.deltaTime);

        if (TimeRemaining <= 0f)
        {
            HasExpired = true;
            TimerExpired?.Invoke(); //Calls event in DrunkEnemy
        }
    }
}