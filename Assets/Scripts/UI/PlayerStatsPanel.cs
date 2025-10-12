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
        : instance = FindFirstObjectByType<PlayerStatsPanel>(FindObjectsInactive.Include);
    
    public async Task Setup(PlayerProfile profile)
    {
        if (profile == null) return;
        currentProfile = profile;
        
        var playerStats = await BackendService.GetPlayerStatsAsync(profile.playerId);

        if (playerNameText != null)
        {
            playerNameText.text = profile.playerName;
        }

        if (totalGamesText != null)
        {
            totalGamesText.text = playerStats.GamesPlayed.ToString();
        }

        if (totalWinsText != null)
        {
            totalWinsText.text = playerStats.Wins.ToString();
        }

        if (highestScoreText != null)
        {
            highestScoreText.text = playerStats.HighTurn.ToString();
        }

        if (averageScoreText != null)
        {
            averageScoreText.text = playerStats.AvgScore.ToString();
        }
        Show();
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
