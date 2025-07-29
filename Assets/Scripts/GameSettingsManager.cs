using UnityEngine;
using UnityEngine.Serialization;

public class GameSettingsManager : Singleton<GameSettingsManager>
{
    [SerializeField] private GameSettings _settings;

    public static GameSettings Settings => Instance._settings;
    
    protected override void Awake()
    {
        base.Awake();
        
        if (_settings == null)
        {
            FarkleLogger.LogError("Default game settings are not set. Please assign them in the inspector.");
        }
    }
    
    public void SetSettings(GameSettings newSettings)
    {
        if (newSettings == null)
        {
            FarkleLogger.LogError("Cannot set null game settings.");
            return;
        }
        
        _settings = newSettings;
        FarkleLogger.Log("Game settings updated successfully.");
    }
}
