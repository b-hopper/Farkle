using UnityEngine;

public class GameSettingsManager : Singleton<GameSettingsManager>
{
    [SerializeField] private GameSettings _defaultSettings;

    public static GameSettings Settings => Instance._defaultSettings;
    
    protected override void Awake()
    {
        base.Awake();
        
        if (_defaultSettings == null)
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
        
        _defaultSettings = newSettings;
        FarkleLogger.Log("Game settings updated successfully.");
    }
}
