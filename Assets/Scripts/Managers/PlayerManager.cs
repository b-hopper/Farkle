using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Farkle.Backend;
using UnityEngine;

namespace Farkle.Managers
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

        public override string ToString()
        {
            return $"{Name} || Profile: {Profile} || Score: {Score.Score}, TurnScore: {Score.TurnScore}, Turns: {Score.Turns}, Farkles: {Score.Farkles}";
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
        
        public void InitPlayers(PlayerProfile[] profiles)
        {
            if (profiles == null || profiles.Length == 0)
            {
                FarkleLogger.LogError("Cannot initialize players with null or empty profiles.");
                return;
            }
            
            players = new Player[profiles.Length];

            for (int i = 0; i < profiles.Length; i++)
            {
                var profile = profiles[i];
                players[i] = new Player(profile);
            }
        }

        public void RecordGameResults(Player winner)
        {
            List<GameResultEntry> results = new List<GameResultEntry>();
            
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
                
                var entry = new GameResultEntry
                {
                    PlayerId = profile.playerId,
                    Score = player.Score.Score,
                    Turns = player.Score.Turns,
                    Farkles = player.Score.Farkles,
                    Won = player == winner
                };
                results.Add(entry);
            }
            
            
            BackendService.PostGameResultAsync(results)
                .ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        FarkleLogger.LogError("Failed to post game results to backend.");
                    }
                    else
                    {
                        FarkleLogger.Log("Game results posted to backend successfully.");
                    }
                });;
        }


        public Task InitAsync()
        {
            return Task.CompletedTask;
        }
    }
}