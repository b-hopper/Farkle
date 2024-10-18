using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class Dice
{
    public int Faces { get; } = 6;
    public int Value { get; private set; } = 0;
    public bool IsSelected { get; private set; }
    public bool IsHeld { get; private set; } = false;

    public void Roll()
    {
        Value = Random.Range(1, Faces + 1);
    }

    public void SetHeld()
    {
        IsHeld = true;
        IsSelected = false;
    }
    
    public void Select()
    {
        if (IsHeld)
        {
            return;
        }
        
        IsSelected = true;
    }
    
    public void Deselect()
    {
        IsSelected = false;
    }
    
    public void Reset()
    {
        Value = 0;
        IsSelected = false;
        IsHeld = false;
    }    
}

public class DiceManager : MonoBehaviour
{
    public static DiceManager Instance { get; private set; }
    
    public Dice[] Dice { get; private set; }
    
    [HideInInspector]
    public bool FirstRoll = true; // First roll of the turn

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
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
        {   // If the die is already selected, deselect it and its scoring set
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
            selectedDice = allUnheldDice.Where(x => x.Value == dieToSelect.Value).Select(x => Array.IndexOf(Dice, x)).ToArray();
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

        // Check for 6 of a kind
        if (valueCounts.ContainsValue(6))
        {
            int scoreToAdd = values[0] == 1 ? 4000 : values[0] * 100 * 4;

            return scoreToAdd;
        }
        
        // Check for 5 of a kind
        if (valueCounts.ContainsValue(5))
        {
            int scoringVal = valueCounts.First(x => x.Value == 5).Key;
            // 5 of a kind is worth 5 times the value of the die, or 3000 points for a 1
            int scoreToAdd = scoringVal == 1 ? 3000 : scoringVal * 100 * 3;
            
            // Recursively call CalculateScore with the 5 dice removed
            return CalculateScore(selectedDice.Where(x => x.Value != scoringVal).ToArray()) 
                   + scoreToAdd;
        }
        
        
        // Check for 2 triplets
        if (valueCounts.Count == 2 && valueCounts.All(x => x.Value == 3))
        {
            
            // 2 triplets is worth 2500 points
            return 2500;
        }
        
        // Check for 3 pairs
        if (valueCounts.Count == 3 && valueCounts.All(x => x.Value == 2))
        {
            // 3 pairs is worth 1500 points
            return 1500;
        }
        // Check for 3 pairs, but two of the pairs are the same
        if (valueCounts.Count == 2 && valueCounts.All(x => x.Value == 2 || x.Value == 4))
        {
            // 3 pairs is worth 1500 points
            return 1500;
        }
        
        // Check for a straight
        if (values.Distinct().OrderBy(x => x).SequenceEqual(new[] { 1, 2, 3, 4, 5, 6 }))
        {
            // A straight is worth 1500 points
            return 1500;
        }
        
        // Check for 4 of a kind
        if (valueCounts.ContainsValue(4))
        {
            int scoringVal = valueCounts.First(x => x.Value == 4).Key;
            // 4 of a kind is worth 4 times the value of the die, or 2000 points for a 1
            int scoreToAdd = scoringVal == 1 ? 2000 : scoringVal * 100 * 2;

            // Recursively call CalculateScore with the 4 dice removed
            return CalculateScore(selectedDice.Where(x => x.Value != scoringVal).ToArray()) 
                   + scoreToAdd;
        }
        
        // Check for 3 of a kind
        if (valueCounts.ContainsValue(3))
        {
            int scoringVal = valueCounts.First(x => x.Value == 3).Key;
            // 3 of a kind is worth 3 times the value of the die, or 1000 points for a 1
            int scoreToAdd = scoringVal == 1 ? 1000 : scoringVal * 100;
            
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
                scoreToAdd += valueCounts[1] * 100;
            }
            
            if (valueCounts.ContainsKey(5))
            {
                scoreToAdd += valueCounts[5] * 50;
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
