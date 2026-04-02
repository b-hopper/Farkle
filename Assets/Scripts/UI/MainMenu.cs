using System;
using System.Collections.Generic;
using Farkle.Managers;
using UnityEngine;

namespace Farkle.UI
{
    public class MainMenu : FarkleUIElement
    {
        [SerializeField] private PlayerUIWidget playerEntryPrefab;
        [SerializeField] private Transform playerEntryParent;
        private List<Transform> playerEntries = new List<Transform>();
        
        private List<PlayerUIWidget> _currentSelectedPlayers;
        
        [SerializeField] private AddPlayerPanel addPlayerPanel;
        [SerializeField] private ConfirmPanel confirmPanel;

        private static MainMenu instance;
        public new static MainMenu Instance => instance ? instance : instance = 
            FindAnyObjectByType<MainMenu>(FindObjectsInactive.Include);
        
        private async void Start()
        {
            _currentSelectedPlayers = new List<PlayerUIWidget>();
            await GameBootstrap.EnsureInitializedAsync();
            PopulatePlayers();
        }

        public void PressPlayGame()
        {
            if (_currentSelectedPlayers.Count < 2)
            {
                Alert("Please select at least 2 players to start the game.");
                return;
            }

            var profiles = new List<PlayerProfile>();
            foreach (var player in _currentSelectedPlayers)
            {
                profiles.Add(player.playerProfile);
            }

            SaveLastSelectedPlayers(profiles);
            PlayerManager.Instance.InitPlayers(profiles.ToArray());
            FarkleSceneManager.Instance.LoadGameScene();
        }

        private const string LastPlayersKey = "LastSelectedPlayerIds";

        private static void SaveLastSelectedPlayers(List<PlayerProfile> profiles)
        {
            var ids = new string[profiles.Count];
            for (int i = 0; i < profiles.Count; i++)
                ids[i] = profiles[i].playerId;
            PlayerPrefs.SetString(LastPlayersKey, string.Join(",", ids));
            PlayerPrefs.Save();
        }

        private static List<string> LoadLastSelectedPlayerIds()
        {
            string saved = PlayerPrefs.GetString(LastPlayersKey, "");
            var list = new List<string>();
            if (string.IsNullOrEmpty(saved)) return list;
            foreach (var id in saved.Split(','))
                if (!string.IsNullOrEmpty(id)) list.Add(id);
            return list;
        }
        
        public void Options()
        {
            // No-op for now
            FarkleLogger.Log("Options button clicked. Implement options menu here.");
        }

        public void PopulatePlayers()
        {
            if (PlayerSettingsManager.Settings == null || PlayerSettingsManager.Settings.playerProfiles == null)
            {
                FarkleLogger.LogWarning("PopulatePlayers: PlayerSettings not ready yet.");
                return;
            }

            FarkleLogger.Log("Populating players: " + PlayerSettingsManager.Settings.playerProfiles.Length);
            // Clear existing entries
            foreach (var entry in playerEntries)
            {
                Destroy(entry.gameObject);
            }
            playerEntries.Clear();
            _currentSelectedPlayers.Clear();
            
            PlayerProfile[] sortedProfiles = PlayerSettingsManager.Settings.playerProfiles;
            Array.Sort(sortedProfiles, (a, b) => b.gamesPlayed.CompareTo(a.gamesPlayed));

            List<string> lastSelectedIds = LoadLastSelectedPlayerIds();
            var entryByPlayerId = new Dictionary<string, PlayerUIWidget>();

            foreach (var playerProfile in sortedProfiles)
            {
                try
                {
                    var entry = Instantiate(playerEntryPrefab, playerEntryParent);
                    entry.Setup(playerProfile);
                    entry.selectButton.onClick.AddListener(() =>
                    {
                        if (!_currentSelectedPlayers.Contains(entry))
                        {
                            if (_currentSelectedPlayers.Count < 4)
                            {
                                _currentSelectedPlayers.Add(entry);
                                entry.SetHighlight(true);
                            }
                            else
                            {
                                Alert("Cannot select more than 4 players!");
                            }
                        }
                        else
                        {
                            _currentSelectedPlayers.Remove(entry);
                            entry.SetHighlight(false);
                        }
                    });
                    playerEntries.Add(entry.transform);
                    entryByPlayerId[playerProfile.playerId] = entry;
                }
                catch (Exception e)
                {
                    FarkleLogger.LogError("Failed to instantiate player entry: " + e);
                }
            }

            // Restore selections in saved turn order, skipping any deleted profiles.
            foreach (var id in lastSelectedIds)
            {
                if (entryByPlayerId.TryGetValue(id, out var entry))
                {
                    _currentSelectedPlayers.Add(entry);
                    entry.SetHighlight(true);
                }
            }
        }
        
        public void AddPlayerButton()
        {
            addPlayerPanel.Show();
            addPlayerPanel.OnPlayerAdded = () =>
            {
                PopulatePlayers();
            };
        }
    }
}