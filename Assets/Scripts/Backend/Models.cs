using System.Collections.Generic;
using Newtonsoft.Json;

namespace Farkle.Backend
{
    // ----- Requests -----

    public class CreatePlayerRequest
    {
        [JsonProperty("user_id")]     public string UserId;
        [JsonProperty("display_name")] public string DisplayName;
    }

    public class GameResultEntry
    {
        [JsonProperty("player_id")]  public string PlayerId;
        [JsonProperty("score")]      public int Score;
        [JsonProperty("turns")]      public int Turns;
        [JsonProperty("farkles")]    public int Farkles;
        [JsonProperty("won")]        public bool Won;
    }

    public class PostGameResultRequest
    {
        [JsonProperty("user_id")] public string UserId;

        // GDD shows { "game": { "results": [...] } } in example, but the current backend accepts
        // top-level "results" as implemented in tests. If your server expects the "game" wrapper,
        // switch to: [JsonProperty("game")] public GameEnvelope Game;  with a class GameEnvelope{ public List<GameResultEntry> results; }
        [JsonProperty("results")] public List<GameResultEntry> Results;
    }

    // ----- Responses -----

    public class CreatePlayerResponse
    {
        [JsonProperty("player_id")] public string PlayerId;
    }

    public class PostGameResultResponse
    {
        [JsonProperty("game_id")] public string GameId;
    }

    public class PlayerStatsResponse
    {
        [JsonProperty("player_id")]   public string PlayerId;
        [JsonProperty("games_played")]public int GamesPlayed;
        [JsonProperty("wins")]        public int Wins;
        [JsonProperty("avg_score")]   public float AvgScore;
        [JsonProperty("total_points")]public long TotalPoints;
        [JsonProperty("farkles")]     public int Farkles;
        [JsonProperty("high_turn")]   public int HighTurn;
    }

    public class LeaderboardRow
    {
        [JsonProperty("player_id")]   public string PlayerId;
        [JsonProperty("display_name")]public string DisplayName;
        [JsonProperty("wins")]        public int Wins;
        [JsonProperty("avg_score")]   public float AvgScore;
        [JsonProperty("total_points")]public long TotalPoints;
    }

    public class LeaderboardResponse
    {
        [JsonProperty("rows")] public List<LeaderboardRow> Rows;
    }

    public class UserPlayersResponse
    {
        public class PlayerEntry
        {
            [JsonProperty("player_id")]   public string PlayerId;
            [JsonProperty("display_name")]public string DisplayName;
        }

        [JsonProperty("players")] public List<PlayerEntry> Players;
    }
}
