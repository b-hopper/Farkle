using System;
using System.Threading.Tasks;
using Farkle.Backend;
using Farkle.Managers;
using Farkle.UI;
using UnityEngine;

public class PlayerStatsPanel : FarkleUIElement
{
    [SerializeField] private TMPro.TextMeshProUGUI playerNameText;
    [SerializeField] private TMPro.TextMeshProUGUI totalGamesText;
    [SerializeField] private TMPro.TextMeshProUGUI totalWinsText;
    [SerializeField] private TMPro.TextMeshProUGUI highestScoreText;
    [SerializeField] private TMPro.TextMeshProUGUI averageScoreText;

    private PlayerProfile currentProfile;

    private static PlayerStatsPanel instance;

    public new static PlayerStatsPanel Instance => instance ? instance
        : instance = FindAnyObjectByType<PlayerStatsPanel>(FindObjectsInactive.Include);
    public async Task Setup(PlayerProfile profile)
    {
        if (profile == null) return;
        currentProfile = profile;

        if (playerNameText != null)
            playerNameText.text = profile.playerName;

        if (BackendService.IsConfigured)
        {
            try
            {
                var stats = await BackendService.GetPlayerStatsAsync(profile.playerId);
                if (totalGamesText != null)  totalGamesText.text  = stats.GamesPlayed.ToString();
                if (totalWinsText != null)   totalWinsText.text   = stats.Wins.ToString();
                if (highestScoreText != null) highestScoreText.text = stats.HighTurn.ToString();
                if (averageScoreText != null) averageScoreText.text = stats.AvgScore.ToString("F0");
            }
            catch (Exception)
            {
                FarkleLogger.LogWarning("Failed to fetch stats from backend, using local data.");
                PopulateFromLocalProfile(profile);
            }
        }
        else
        {
            PopulateFromLocalProfile(profile);
        }

        Show();
    }

    private void PopulateFromLocalProfile(PlayerProfile profile)
    {
        if (totalGamesText != null)   totalGamesText.text   = profile.gamesPlayed.ToString();
        if (totalWinsText != null)    totalWinsText.text    = profile.gamesWon.ToString();
        if (highestScoreText != null) highestScoreText.text = profile.highScore.ToString();
        if (averageScoreText != null)
        {
            float avg = profile.gamesPlayed > 0 ? (float)profile.totalScore / profile.gamesPlayed : 0f;
            averageScoreText.text = avg.ToString("F0");
        }
    }
    
    public void DeletePlayerProfile()
    {
        if (currentProfile == null) return;
        
        ConfirmPanel.Instance.Show($"Are you sure you want to delete the profile for {currentProfile.playerName}?",  async () =>
        {
            await PlayerSettingsManager.Instance.DeletePlayerProfileAsync(currentProfile);
            MainMenu.Instance?.PopulatePlayers();
            Hide();
        });

    }
}
