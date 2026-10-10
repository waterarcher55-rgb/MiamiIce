using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region PlayerNoise
    //Things that need to be in a game manager for noises
    private PlayerNoiseEmitter noiseEmitter;

    private void Awake()
    {
        noiseEmitter = new PlayerNoiseEmitter();
    }

    private void OnDestroy()
    {
        noiseEmitter?.Dispose();
    }
    #endregion
}
