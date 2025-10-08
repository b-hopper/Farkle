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
        
        private async void Start()
        {
            _currentSelectedPlayers = new List<PlayerUIWidget>();
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
            
            PlayerManager.Instance.InitPlayers(profiles.ToArray());
            FarkleSceneManager.Instance.LoadGameScene();
        }
        
        public void Options()
        {
            // No-op for now
            FarkleLogger.Log("Options button clicked. Implement options menu here.");
        }

        private void PopulatePlayers()
        {
            FarkleLogger.Log("Populating players: " + PlayerSettingsManager.Settings.playerProfiles.Length);
            // Clear existing entries
            foreach (var entry in playerEntries)
            {
                Destroy(entry.gameObject);
            }
            playerEntries.Clear();
            _currentSelectedPlayers.Clear();
            
            foreach (var playerProfile in PlayerSettingsManager.Settings.playerProfiles)
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
                }
                catch (Exception e)
                {
                    FarkleLogger.LogError("Failed to instantiate player entry: " + e);
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