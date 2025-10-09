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
                SetSettings(list.profiles);
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
            
            // Check if player name already exists
            if (_settings.playerProfiles != null)
            {
                if (Array.Exists(_settings.playerProfiles, p => p.playerName.Equals(playerName, StringComparison.OrdinalIgnoreCase)))
                {
                    FarkleLogger.LogWarning($"Player name '{playerName}' already exists. Choose a different name.");
                    return;
                }
            }

            try
            {
                var createdPlayer = await BackendService.CreatePlayerAsync(playerName);
                
                var profiles = new List<PlayerProfile>(_settings.playerProfiles ?? Array.Empty<PlayerProfile>());
                profiles.Add(new PlayerProfile { playerName = playerName, playerId = createdPlayer.PlayerId });
                _settings.playerProfiles = profiles.ToArray();
                
                FarkleLogger.Log($"Created player on backend with ID: {createdPlayer.PlayerId}");
                SaveProfilesToDisk();
            }
            catch (Exception)
            {
                FarkleLogger.LogError("Failed to create player on backend.");
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
                FarkleLogger.LogError("PlayerSettings is null. Cannot create profile.");
                return;
            }
            
            if (playerProfile == null || string.IsNullOrEmpty(playerProfile.playerId))
            {
                FarkleLogger.LogError("Invalid player profile. Cannot delete.");
                return;
            }
            
            try
            {
                var deleted = await BackendService.DeletePlayerAsync(playerProfile.playerId);
                if (deleted.Success)
                {
                    var profiles = new List<PlayerProfile>(_settings.playerProfiles ?? Array.Empty<PlayerProfile>());
                    if (profiles.Remove(playerProfile))
                    {
                        _settings.playerProfiles = profiles.ToArray();
                        FarkleLogger.Log($"Deleted player profile '{playerProfile.playerName}' with ID: {playerProfile.playerId}");
                        SaveProfilesToDisk();
                    }
                    else
                    {
                        FarkleLogger.LogWarning($"Player profile '{playerProfile.playerName}' not found in local settings.");
                    }
                }
                else
                {
                    FarkleLogger.LogError($"Failed to delete player profile '{playerProfile.playerName}' on backend.");
                }
            }
            catch (Exception)
            {
                FarkleLogger.LogError("Failed to delete player on backend.");
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

            try
            {
                var players = await BackendService.GetUserPlayersAsync();
                
                // If profiles exist, load them from backend
                PlayerProfile[] loadedProfiles = new  PlayerProfile[players.Players.Count];
                for (int i = 0; i < players.Players.Count; i++)
                {
                    loadedProfiles[i] = new PlayerProfile
                    {
                        playerId = players.Players[i].PlayerId,
                        playerName = players.Players[i].DisplayName,
                        gamesWon = players.Players[i].Wins,
                        totalScore = (int)players.Players[i].TotalPoints,
                    };
                }

                SetSettings(loadedProfiles);
                FarkleLogger.Log("Loaded player profiles from backend.");
            }
            catch (Exception e)
            {
                FarkleLogger.LogError("Failed to fetch player profiles from backend. Loading local profiles. Error: " + e.Message);
                LoadProfilesFromDisk();
                if (_settings == null || _settings.playerProfiles == null || _settings.playerProfiles.Length == 0)
                {
                    FarkleLogger.LogWarning("No local player profiles found. Loading default profiles.");
                    await LoadDefaultPlayerSettingsAsync();
                }
            }
        }

        private async Task LoadDefaultPlayerSettingsAsync()
        {
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