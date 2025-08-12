using System.Collections.Generic;
using System.Threading.Tasks;
using Farkle.Backend;
using UnityEngine;

public class FarkleBackendDemo : MonoBehaviour
{
    [SerializeField]
    private BackendConfig Config;

    [SerializeField]
    private string DemoDisplayName = "Tester";

    async void Start()
    {
        if (Config == null)
        {
            FarkleLogger.LogError("Assign BackendConfig in the inspector.");
            return;
        }

        BackendService.Initialize(Config);

        if (string.IsNullOrWhiteSpace(Config.UserId))
        {
            // In production, store this once (e.g., PlayerPrefs) or use Google Play Games ID.
            Config.UserId = System.Guid.NewGuid().ToString();
            FarkleLogger.Log($"Generated temp user_id: {Config.UserId}");
        }

        try
        {
            // 1) Create a player
            var create = await BackendService.CreatePlayerAsync(Config.UserId, DemoDisplayName);
            FarkleLogger.Log($"CreatePlayer -> player_id: {create.PlayerId}");

            // 2) Post a fake game result
            var results = new List<GameResultEntry>
            {
                new GameResultEntry {
                    PlayerId = create.PlayerId,
                    Score = 10000, Turns = 9, Farkles = 2, Won = true
                }
            };
            var posted = await BackendService.PostGameResultAsync(Config.UserId, results);
            FarkleLogger.Log($"GameResult -> game_id: {posted.GameId}");

            // 3) Fetch stats
            var stats = await BackendService.GetPlayerStatsAsync(create.PlayerId);
            FarkleLogger.Log($"Stats -> GP:{stats.GamesPlayed} Wins:{stats.Wins} Avg:{stats.AvgScore} TP:{stats.TotalPoints}");

            // 4) Leaderboard
            var lb = await BackendService.GetLeaderboardAsync("avg_score");
            string leaderboardLog = $"Leaderboard rows: <color=green>{lb.Rows.Count}</color>";
            foreach (var row in lb.Rows)
            {
                leaderboardLog += $"\n\t<color=yellow>Leaderboard</color> -> {row.PlayerId} - {row.DisplayName} - Avg: {row.AvgScore}";
            }
            FarkleLogger.Log(leaderboardLog);
            
            // 5) List user players
            var players = await BackendService.GetUserPlayersAsync(Config.UserId);
            string userPlayersLog = $"User players: <color=green>{players.Players.Count}</color>";
            foreach (var player in players.Players)
            {
                userPlayersLog += $"\n\t<color=yellow>Player</color> -> {player.PlayerId} - {player.DisplayName}";
            }
            FarkleLogger.Log(userPlayersLog);
        }
        catch (System.Exception ex)
        {
            FarkleLogger.LogError($"Backend error: {ex.Message}");
        }
    }
}
