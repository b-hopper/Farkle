using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Serialization;
using Utils;

namespace Managers
{
    public class GameSettingsManager : Singleton<GameSettingsManager>, IGameManager
    {
        [ReadOnly] private GameSettings _settings;

        public static GameSettings Settings => Instance._settings;

        public void SetSettings(GameSettings newSettings)
        {
            if (newSettings == null)
            {
                FarkleLogger.LogError("Cannot set null game settings.");
                return;
            }

            _settings = newSettings;
        }

        public async Task InitAsync()
        {
            DontDestroyOnLoad(this);
            
            var handle = Addressables.LoadAssetAsync<ScriptableObject>("GameSettings_LowScore");
            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                SetSettings(handle.Result as GameSettings);
                if (_settings == null)
                {
                    FarkleLogger.LogError("GameSettings_LowScore does not contain a GameSettings component.");
                }
            }
            else
            {
                FarkleLogger.LogError("Failed to load GameSettings_LowScore from Addressables.");
            }
        }
    }
}