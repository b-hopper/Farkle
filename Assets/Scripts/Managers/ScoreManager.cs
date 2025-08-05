using System;
using UnityEngine;

namespace Managers
{
    public class ScoreManager : Singleton<ScoreManager>
    {
        public int ScoreToWin => GameSettingsManager.Settings.targetScore;
        public int BottomFeedScore { get; private set; }

        [Serializable]
        public struct PlayerScore
        {
            public int Score;
            public int TurnScore;
            public int SelectedScore;

            public bool BrokenIn => (Score >= GameSettingsManager.Settings.breakInScore 
              || TurnScore + SelectedScore >= GameSettingsManager.Settings.breakInScore);

            public PlayerScore(int score, int turnScore)
            {
                Score = score;
                TurnScore = turnScore;
                SelectedScore = 0;
            }

            public void AddScore(int score)
            {
                Score += score;
            }

            public void AddTurnScore(int score)
            {
                TurnScore += score;
            }

            public void ResetTurnScore()
            {
                SelectedScore = 0;
                TurnScore = 0;
            }
        }

        public Player CurrentPlayer => PlayerManager.Instance.CurrentPlayer;

        private void Start()
        {
            InitDelegates();
        }
        
        private void InitDelegates()
        {
            FarkleGame.Instance.OnFarkle.AddListener(OnFarkle);
            FarkleGame.Instance.OnTurnScoreUpdated.AddListener(AddTurnScore);
            // FarkleGame.Instance.OnDiceHeld.AddListener(OnDiceHeld);
        }

        private void OnFarkle()
        {
            ResetTurnScore();
        }

        public void OnDiceHeld()
        {
            AddTurnScoreToTotal();
        }

        private void AddTurnScoreToTotal()
        {
            CurrentPlayer.Score.AddScore(CurrentPlayer.Score.TurnScore);

            CheckForWinningScore();

            SetBottomFeed(CurrentPlayer.Score.TurnScore);
            ResetTurnScore();
        }

        private void CheckForWinningScore()
        {
            if (CurrentPlayer.Score.Score >= ScoreToWin)
            {
                TurnManager.Instance.GameWinningScoreReached();
            }
        }

        public Player GetWinner()
        {
            Player winner = null;
            int highestScore = 0;

            foreach (var player in PlayerManager.Instance.AllPlayers)
            {
                if (player.Score.Score > highestScore)
                {
                    highestScore = player.Score.Score;
                    winner = player;
                }
            }

            return winner;
        }

        private void AddTurnScore(int score)
        {
            CurrentPlayer.Score.SelectedScore = 0;
            CurrentPlayer.Score.AddTurnScore(score + BottomFeedScore);
            BottomFeedScore = 0;
        }

        private void ResetTurnScore()
        {
            CurrentPlayer.Score.ResetTurnScore();
        }

        public void SetBottomFeed(int scoreAtRisk)
        {
            BottomFeedScore = scoreAtRisk;
        }
    }
}