using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Threading.Tasks;
using Farkle.Backend;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Utils;

namespace Farkle.Managers
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
            
            if (_settings != null)
            {
                Destroy(_settings);
            }
            
            _settings = Instantiate(newSettings);
            
            FarkleLogger.Log(_settings.ToString());
        }
        
        public void SetSettings(PlayerProfile[] profiles)
        {
            if (_settings != null)
            {
                Destroy(_settings);
            }

            _settings = ScriptableObject.CreateInstance<PlayerSettings>();
            _settings.playerProfiles = profiles;
            
            SaveProfilesToDisk();
        }

        public void SaveProfilesToDisk()
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

        public void LoadProfilesFromDisk()
        {
            if (!File.Exists(SavePath))
            {
                FarkleLogger.LogWarning("Player profiles file not found.");
                return;
            }

            try
            {
                string json = File.ReadAllText(SavePath);
                Debug.Log("Loaded JSON: " + json);
                if (string.IsNullOrEmpty(json))
                {
                    FarkleLogger.LogWarning("Player profiles file is empty.");
                    return;
                }
                PlayerProfileList list = JsonUtility.FromJson<PlayerProfileList>(json);
                if (list == null || list.profiles == null || list.profiles.Length == 0)
                {
                    FarkleLogger.LogWarning("No player profiles found in the file.");
                    return;
                }

                Debug.Log("Deserialized Profiles");

                // Assign IDs to any profiles that are missing one (e.g. old default profiles).
                bool dirty = false;
                foreach (var p in list.profiles)
                {
                    if (string.IsNullOrEmpty(p.playerId))
                    {
                        p.playerId = System.Guid.NewGuid().ToString();
                        dirty = true;
                    }
                }

                SetSettings(list.profiles); // always saves
                if (dirty)
                    FarkleLogger.Log("Assigned IDs to profiles that were missing them.");
                FarkleLogger.Log("Player profiles loaded successfully.");
            }
            catch (Exception e)
            {
                FarkleLogger.LogError($"Error loading player profiles: {e.Message}");
                throw;
            }
        }
        
        public async Task CreatePlayerProfileAsync(string playerName)
        {
            if (_settings == null)
            {
                FarkleLogger.LogError("PlayerSettings is null. Cannot create profile.");
                return;
            }

            if (_settings.playerProfiles != null)
            {
                if (Array.Exists(_settings.playerProfiles, p => p.playerName.Equals(playerName, StringComparison.OrdinalIgnoreCase)))
                {
                    FarkleLogger.LogWarning($"Player name '{playerName}' already exists. Choose a different name.");
                    return;
                }
            }

            // Create locally first with a generated ID so the player always gets saved.
            string playerId = System.Guid.NewGuid().ToString();
            var profiles = new List<PlayerProfile>(_settings.playerProfiles ?? Array.Empty<PlayerProfile>());
            profiles.Add(new PlayerProfile { playerName = playerName, playerId = playerId });
            _settings.playerProfiles = profiles.ToArray();
            SaveProfilesToDisk();

            // Optionally sync to backend and update the ID if it succeeds.
            if (BackendService.IsConfigured)
            {
                try
                {
                    var createdPlayer = await BackendService.CreatePlayerAsync(playerName);
                    int idx = Array.FindIndex(_settings.playerProfiles, p => p.playerId == playerId);
                    if (idx >= 0)
                    {
                        _settings.playerProfiles[idx].playerId = createdPlayer.PlayerId;
                        SaveProfilesToDisk();
                    }
                    FarkleLogger.Log($"Player '{playerName}' synced to backend with ID: {createdPlayer.PlayerId}");
                }
                catch (Exception)
                {
                    FarkleLogger.LogWarning($"Player '{playerName}' saved locally but backend sync failed.");
                }
            }
        }
        
        public void UpdatePlayerProfiles(PlayerProfile[] playerProfiles)
        {
            if (_settings == null)
            {
                FarkleLogger.LogError("PlayerSettings is null. Cannot update profile.");
                return;
            }

            foreach (var playerProfile in playerProfiles)
            {
                if (playerProfile == null || string.IsNullOrEmpty(playerProfile.playerId))
                {
                    FarkleLogger.LogError("Invalid player profile. Cannot update.");
                    return;
                }

                var profiles = new List<PlayerProfile>(_settings.playerProfiles ?? Array.Empty<PlayerProfile>());
                int index = profiles.FindIndex(p => p.playerId == playerProfile.playerId);
                if (index >= 0)
                {
                    profiles[index] = playerProfile;
                    _settings.playerProfiles = profiles.ToArray();
                    FarkleLogger.Log(
                        $"Updated player profile '{playerProfile.playerName}' with ID: {playerProfile.playerId}");
                }
                else
                {
                    FarkleLogger.LogWarning(
                        $"Player profile with ID '{playerProfile.playerId}' not found. Cannot update.");
                }
            }
            SaveProfilesToDisk();
        }

        public async Task DeletePlayerProfileAsync(PlayerProfile playerProfile)
        {
            if (_settings == null)
            {
                FarkleLogger.LogError("PlayerSettings is null. Cannot delete profile.");
                return;
            }

            if (playerProfile == null || string.IsNullOrEmpty(playerProfile.playerId))
            {
                FarkleLogger.LogError("Invalid player profile. Cannot delete.");
                return;
            }

            // Always delete locally first.
            var profiles = new List<PlayerProfile>(_settings.playerProfiles ?? Array.Empty<PlayerProfile>());
            int removed = profiles.RemoveAll(p => p.playerId == playerProfile.playerId);
            if (removed > 0)
            {
                _settings.playerProfiles = profiles.ToArray();
                SaveProfilesToDisk();
                FarkleLogger.Log($"Deleted player '{playerProfile.playerName}' locally.");
            }
            else
            {
                FarkleLogger.LogWarning($"Player '{playerProfile.playerName}' not found locally.");
            }

            // Best-effort backend delete.
            if (BackendService.IsConfigured)
            {
                try
                {
                    await BackendService.DeletePlayerAsync(playerProfile.playerId);
                }
                catch (Exception)
                {
                    FarkleLogger.LogWarning($"Backend delete failed for '{playerProfile.playerName}' — removed locally only.");
                }
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

            // Load from disk first — fast, no network required.
            LoadProfilesFromDisk();

            if (_settings == null || _settings.playerProfiles == null || _settings.playerProfiles.Length == 0)
            {
                FarkleLogger.LogWarning("No local player profiles found. Loading default profiles.");
                await LoadDefaultPlayerSettingsAsync();
            }
        }

        private async Task LoadDefaultPlayerSettingsAsync()
        {
            var handle = Addressables.LoadAssetAsync<ScriptableObject>("PlayerSettings_Default");
            await handle.Task;

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                FarkleLogger.LogError("Failed to load PlayerSettings_Default from Addressables.");
                return;
            }

            var defaultSettings = handle.Result as PlayerSettings;
            if (defaultSettings == null)
            {
                FarkleLogger.LogError("PlayerSettings_Default does not contain a PlayerSettings component.");
                return;
            }

            // Ensure every default profile has an ID, then save so they become real local profiles.
            var profiles = defaultSettings.playerProfiles ?? Array.Empty<PlayerProfile>();
            foreach (var p in profiles)
            {
                if (string.IsNullOrEmpty(p.playerId))
                    p.playerId = System.Guid.NewGuid().ToString();
            }

            SetSettings(profiles); // calls SaveProfilesToDisk()
            FarkleLogger.Log("Default player profiles adopted and saved to disk.");
        }
    }
}