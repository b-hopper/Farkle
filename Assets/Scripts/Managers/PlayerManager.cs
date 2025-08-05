using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;


namespace Managers
{
    [Serializable]
    public class Player
    {
        public PlayerProfile Profile;

        public string Name => Profile?.playerName ?? "Player";
        [SerializeField] public ScoreManager.PlayerScore Score;

        public bool HasTakenFinalTurn = false;

        public Player(PlayerProfile profile)
        {
            Profile = profile;
            Score = new ScoreManager.PlayerScore(0, 0);
        }
    }

    public class PlayerManager : Singleton<PlayerManager>, IGameManager
    {
        [SerializeField] private Player[] players;

        public Player CurrentPlayer
        {
            get
            {
                int idx = TurnManager.Instance.CurrentPlayerIndex;

                if (idx >= players.Length || idx < 0)
                {
                    FarkleLogger.LogError($"(PlayerManager::CurrentPlayer) Invalid player index: {idx}");
                    return null;
                }

                return players[idx];
            }
        }

        public Player NextPlayer
        {
            get
            {
                int idx = TurnManager.Instance.CurrentPlayerIndex + 1;

                if (idx >= players.Length)
                {
                    idx = 0;
                }

                return players[idx];
            }
        }

        public Player[] AllPlayers => players;

        public void InitPlayers(int playerCount = -1)
        {
            var profiles = PlayerSettingsManager.Settings.playerProfiles;
            playerCount = playerCount > 0 ? playerCount : profiles.Length;

            players = new Player[playerCount];

            for (int i = 0; i < playerCount; i++)
            {
                var profile = i < profiles.Length ? profiles[i] : new PlayerProfile { playerName = $"Player {i + 1}" };
                players[i] = new Player(profile);
            }
        }

        public void RecordGameResults(Player winner)
        {
            foreach (var player in players)
            {
                var profile = player.Profile;
                profile.gamesPlayed++;
                profile.totalScore += player.Score.Score;

                if (player.Score.Score > profile.highScore)
                {
                    profile.highScore = player.Score.Score;
                }

                if (player == winner)
                {
                    profile.gamesWon++;
                }
            }
        }


        public Task InitAsync()
        {
            var players = PlayerSettingsManager.Settings.playerCount;
            InitPlayers(players);
            return Task.CompletedTask;
        }
    }
}