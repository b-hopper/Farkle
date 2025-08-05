using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;
using Utils;

namespace Managers
{
    public class GameSettingsManager : Singleton<GameSettingsManager>
    {
        [ReadOnly] private GameSettings _settings;

        public static GameSettings Settings => Instance._settings;

        protected void Awake()
        {
            Addressables.LoadAssetAsync<ScriptableObject>("GameSettings_LowScore").Completed += handle =>
            {
                if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                {
                    SetSettings(handle.Result as GameSettings);

                    if (_settings == null)
                    {
                        FarkleLogger.LogError("GameSettings_Default does not contain a GameSettings component.");
                        return;
                    }
                }
                else
                {
                    FarkleLogger.LogError("Failed to load GameSettings_Default from Addressables.");
                }
            };    
        }

        public void SetSettings(GameSettings newSettings)
        {
            if (newSettings == null)
            {
                FarkleLogger.LogError("Cannot set null game settings.");
                return;
            }

            _settings = newSettings;
        }
    }
}