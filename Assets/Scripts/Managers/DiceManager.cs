using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
    
namespace Farkle.Managers
{
    public class DiceManager : Singleton<DiceManager>
    {
        public Dice[] Dice { get; private set; }

        [HideInInspector] public bool FirstRoll = true; // First roll of the turn

        protected void Awake()
        {
            Dice = new Dice[6];

            for (int i = 0; i < Dice.Length; i++)
            {
                Dice[i] = new Dice();
            }
        }

        private void Start()
        {
            InitDelegates();
        }

        private void InitDelegates()
        {
            FarkleGame.Instance.OnFarkle.AddListener(OnFarkle);
            FarkleGame.Instance.OnRollDiceStart.AddListener(RollDice);
            FarkleGame.Instance.OnDiceHeld.AddListener(HoldSelectedDice);
            FarkleGame.Instance.OnTurnScoreUpdated.AddListener(OnTurnScoreUpdated);
        }


        public void RollDice(float _) => RollDice();

        public void RollDice()
        {
            bool allHeld = Dice.All(x => x.IsHeld);

            if (allHeld)
            {
                ResetDice();
            }

            for (var i = 0; i < Dice.Length; i++)
            {
                var die = Dice[i];
                if (!die.IsHeld)
                {
                    die.Roll();
                }
            }

            FirstRoll = false;
        }

        public bool CheckForFarkle()
        {
            var unheldDice = Dice.Where(x => !x.IsHeld).ToArray();
            if (CalculateScore(unheldDice) == 0)
            {
                return true;
            }

            return false;
        }

        void HoldSelectedDice()
        {
            Dice[] selectedDice = Dice.Where(x => x.IsSelected && !x.IsHeld).ToArray();

            foreach (var die in selectedDice)
            {
                die.SetHeld();
            }
        }

        public int HoldSelectedDiceAndScore()
        {
            Dice[] selectedDice = Dice.Where(x => x.IsSelected && !x.IsHeld).ToArray();
            int scoreToAdd = CalculateScore(selectedDice);

            foreach (var die in selectedDice)
            {
                die.SetHeld();
            }

            return scoreToAdd;
        }

        public void SelectDie(int index)
        {
            if (index < 0 ||
                index >= Dice.Length ||
                Dice[index].Value == 0 ||
                Dice[index].IsHeld)
            {
                return;
            }

            if (Dice[index].IsSelected)
            {
                // If the die is already selected, deselect it and its scoring set
                CanSelectDie(index, out int[] selectedDice);
                foreach (var dieIndex in selectedDice)
                {
                    Dice[dieIndex].Deselect();
                }
            }
            else if (CanSelectDie(index, out int[] selectedDice))
            {
                foreach (var dieIndex in selectedDice)
                {
                    Dice[dieIndex].Select();
                }
            }

            int selectedScore = CalculateScore(Dice.Where(x => x.IsSelected && !x.IsHeld).ToArray());
            ScoreManager.Instance.CurrentPlayer.Score.SelectedScore = selectedScore;
        }

        public void ResetDice()
        {
            for (var i = 0; i < Dice.Length; i++)
            {
                var die = Dice[i];
                die.Reset();
            }
        }

        private bool CanSelectDie(int index, out int[] selectedDice)
        {
            // Possibilities:
            // 1. 1 or 5
            // 2. x of a kind, selected die is part of the x
            // 3. 3 pairs, selected die is part of a pair
            // 4. 2 triplets, selected die is part of a triplet
            // 5. Straight, selected die is part of the straight
            // Output: selectedDice is the array of dice in the scoring set

            Dice[] allUnheldDice = Dice.Where(x => !x.IsHeld).ToArray();
            Dice dieToSelect = Dice[index];

            List<int> values = allUnheldDice.Select(x => x.Value).ToList();
            Dictionary<int, int> valueCounts = values.GroupBy(x => x).ToDictionary(g => g.Key, g => g.Count());

            // Check for 1 or 5
            if (dieToSelect.Value == 1 ||
                dieToSelect.Value == 5)
            {
                selectedDice = new[] { index };
                return true;
            }

            // Check for x of a kind
            if (valueCounts.ContainsKey(dieToSelect.Value) &&
                valueCounts[dieToSelect.Value] >= 3)
            {
                selectedDice = allUnheldDice.Where(x => x.Value == dieToSelect.Value)
                    .Select(x => Array.IndexOf(Dice, x)).ToArray();
                return true;
            }

            // Check for 3 pairs
            if (valueCounts.Count == 3 &&
                valueCounts.All(x => x.Value == 2))
            {
                selectedDice = new[] { 0, 1, 2, 3, 4, 5 };
                return true;
            }

            // Check for 3 pairs, but two of the pairs are the same
            if (allUnheldDice.Length == 6 &&
                valueCounts.Count == 2 &&
                valueCounts.All(x => x.Value == 2 || x.Value == 4))
            {
                selectedDice = new[] { 0, 1, 2, 3, 4, 5 };
                return true;
            }

            // Check for 2 triplets
            if (valueCounts.Count == 2 &&
                valueCounts.All(x => x.Value == 3))
            {
                selectedDice = new[] { 0, 1, 2, 3, 4, 5 };
                return true;
            }

            // Check for a straight
            if (values.Distinct().OrderBy(x => x).SequenceEqual(new[] { 1, 2, 3, 4, 5, 6 }))
            {
                selectedDice = new[] { 0, 1, 2, 3, 4, 5 };
                return true;
            }

            selectedDice = Array.Empty<int>();
            return false;

        }

        public int CalculateScore(Dice[] selectedDice)
        {
            if (selectedDice.Length == 0)
            {
                return 0;
            }

            List<int> values = selectedDice.Select(x => x.Value).ToList();
            Dictionary<int, int> valueCounts = values.GroupBy(x => x).ToDictionary(g => g.Key, g => g.Count());
            var settings = GameSettingsManager.Settings;

            // Check for 6 of a kind
            if (valueCounts.ContainsValue(6))
            {
                int valuePerDie = values[0] == 1
                    ? 10 * settings.pointsMultiplierOfAKind
                    : values[0] * settings.pointsMultiplierOfAKind;

                int scoreToAdd = valuePerDie * 4;

                return scoreToAdd;
            }

            // Check for 5 of a kind
            if (valueCounts.ContainsValue(5))
            {
                int scoringVal = valueCounts.First(x => x.Value == 5).Key;

                int valuePerDie = scoringVal == 1
                    ? 10 * settings.pointsMultiplierOfAKind
                    : scoringVal * settings.pointsMultiplierOfAKind;

                int scoreToAdd = valuePerDie * 3;

                // Recursively call CalculateScore with the 5 dice removed
                return CalculateScore(selectedDice.Where(x => x.Value != scoringVal).ToArray())
                       + scoreToAdd;
            }


            // Check for 2 triplets
            if (valueCounts.Count == 2 && valueCounts.All(x => x.Value == 3))
            {

                return settings.pointsForTwoTriplets;
            }

            // Check for 3 pairs
            if (valueCounts.Count == 3 && valueCounts.All(x => x.Value == 2))
            {
                return settings.pointsForThreePairs;
            }

            // Check for 3 pairs, but two of the pairs are the same
            if (valueCounts.Count == 2 && valueCounts.All(x => x.Value == 2 || x.Value == 4))
            {
                return settings.pointsForThreePairs;
            }

            // Check for a straight
            if (values.Distinct().OrderBy(x => x).SequenceEqual(new[] { 1, 2, 3, 4, 5, 6 }))
            {
                return settings.pointsForStraight;
            }

            // Check for 4 of a kind
            if (valueCounts.ContainsValue(4))
            {
                int scoringVal = valueCounts.First(x => x.Value == 4).Key;

                int valuePerDie = scoringVal == 1
                    ? 10 * settings.pointsMultiplierOfAKind
                    : scoringVal * settings.pointsMultiplierOfAKind;

                int scoreToAdd = valuePerDie * 2; // 4 of a kind is worth double the value of the die

                // Recursively call CalculateScore with the 4 dice removed
                return CalculateScore(selectedDice.Where(x => x.Value != scoringVal).ToArray())
                       + scoreToAdd;
            }

            // Check for 3 of a kind
            if (valueCounts.ContainsValue(3))
            {
                int scoringVal = valueCounts.First(x => x.Value == 3).Key;

                int scoreToAdd = scoringVal == 1
                    ? 10 * settings.pointsMultiplierOfAKind
                    : scoringVal * settings.pointsMultiplierOfAKind;

                // Recursively call CalculateScore with the 3 dice removed
                return CalculateScore(selectedDice.Where(x => x.Value != scoringVal).ToArray())
                       + scoreToAdd;
            }

            // Check for 1s and 5s
            if (valueCounts.ContainsKey(1) || valueCounts.ContainsKey(5))
            {
                int scoreToAdd = 0;

                if (valueCounts.ContainsKey(1))
                {
                    scoreToAdd += valueCounts[1] * settings.pointsPerOne;
                }

                if (valueCounts.ContainsKey(5))
                {
                    scoreToAdd += valueCounts[5] * settings.pointsPerFive;
                }

                // Recursively call CalculateScore with the 1s and 5s removed
                return CalculateScore(selectedDice.Where(x => x.Value != 1 && x.Value != 5).ToArray())
                       + scoreToAdd;
            }

            return 0;
        }

        public void OnFarkle()
        {
            ResetDice();
        }

        public void OnTurnScoreUpdated(int score)
        {
            if (Dice.All(x => x.IsHeld))
            {
                ResetDice();
            }
            else
            {
                for (var i = 0; i < Dice.Length; i++)
                {
                    var die = Dice[i];
                    if (!die.IsHeld)
                    {
                        die.Reset();
                    }
                }
            }
        }



    }
}