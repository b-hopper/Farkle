using System;
using UnityEngine;
using System.IO;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Utils;

namespace Managers
{
    public class PlayerSettingsManager : Singleton<PlayerSettingsManager>, IGameManager
    {
        [ReadOnly] private PlayerSettings _settings;
        public static PlayerSettings Settings => Instance._settings;

        private static string SavePath => Path.Combine(Application.persistentDataPath, "player_profiles.json");

        public void SetSettings(PlayerSettings newSettings)
        {
            if (newSettings == null)
            {
                FarkleLogger.LogError("Cannot set null player settings.");
                return;
            }

            _settings = newSettings;
            LoadProfiles();
        }

        public void SaveProfiles()
        {
            string json = JsonUtility.ToJson(new PlayerProfileList { profiles = _settings.playerProfiles }, true);
            try
            {
                File.WriteAllText(SavePath, json);
                FarkleLogger.Log("Player profiles saved successfully.");
            }
            catch (Exception e)
            {
                FarkleLogger.LogError($"Error saving player profiles: {e.Message}");
                throw;
            }
        }

        public void LoadProfiles()
        {
            if (!File.Exists(SavePath))
            {
                FarkleLogger.LogWarning("Player profiles file not found.");
                return;
            }

            try
            {
                string json = File.ReadAllText(SavePath);
                PlayerProfileList list = JsonUtility.FromJson<PlayerProfileList>(json);
                if (list == null || list.profiles == null || list.profiles.Length == 0)
                {
                    FarkleLogger.LogWarning("No player profiles found in the file.");
                    return;
                }

                _settings.playerProfiles = list.profiles;
                FarkleLogger.Log("Player profiles loaded successfully.");
            }
            catch (Exception e)
            {
                FarkleLogger.LogError($"Error loading player profiles: {e.Message}");
                throw;
            }
        }

        [Serializable]
        private class PlayerProfileList
        {
            public PlayerProfile[] profiles;
        }

        public async Task InitAsync()
        {
            DontDestroyOnLoad(this);
            
            var handle = Addressables.LoadAssetAsync<ScriptableObject>("PlayerSettings_Default");
            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                SetSettings(handle.Result as PlayerSettings);
                if (_settings == null)
                {
                    FarkleLogger.LogError("PlayerSettings_Default does not contain a PlayerSettings component.");
                }
            }
            else
            {
                FarkleLogger.LogError("Failed to load PlayerSettings_Default from Addressables.");
            }
        }
    }
}