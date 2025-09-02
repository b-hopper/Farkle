using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings", menuName = "ScriptableObjects/GameSettings", order = 1)]
public class GameSettings : ScriptableObject
{
    [Header("Game Win Conditions")]
    [Tooltip("Score required to win the game.")]
    public int targetScore = 10000;
    
    [Tooltip("Score the player must reach to break in.")]
    public int breakInScore = 500;

    [Header("Combos")]
    public List<ComboRule> combos = new();
    
    [Header("Of-a-Kind Scoring")]
    [Tooltip("Multiplier used for scoring 3, 4, 5, or 6 of a kind.")]
    [SerializeField]
    public int pointsMultiplierOfAKind = 100;
    // e.g., 3x3x3 = 3 * 100 = 300 points
    // 1s are treated specially as if they have a value of 10 (1000, 2000, etc.)

    [Header("Single Dice Scoring")]
    [Tooltip("Points per 1 rolled (not part of another combo).")]
    public int pointsPerOne = 100;

    [Tooltip("Points per 5 rolled (not part of another combo).")]
    public int pointsPerFive = 50;
    
    
    public void ResetToDefaults()
    {
        targetScore = 10000;
        breakInScore = 500;

        combos = new List<ComboRule>
        {
            new ComboRule { id = "straight_1to6", displayName = "Straight (1-6)", points = 1500, enabled = true, description = "All faces 1-6 once" },
            new ComboRule { id = "three_pairs", displayName = "Three Pairs", points = 1500, enabled = true, description = "Three pairs across six dice" },
            new ComboRule { id = "two_triplets", displayName = "Two Triplets", points = 2500, enabled = true, description = "Two sets of three-of-a-kind" },
        };
        
        pointsMultiplierOfAKind = 100;
        pointsPerOne = 100;
        pointsPerFive = 50;
    }
}

[Serializable]
public class ComboRule
{
    public string id;
    public string displayName;
    [TextArea] public string description;
    public int points;
    public bool enabled = true;
}