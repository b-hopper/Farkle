using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings", menuName = "ScriptableObjects/GameSettings", order = 1)]
public class GameSettings : ScriptableObject
{
    [Header("Game Win Conditions")]
    [Tooltip("Score required to win the game.")]
    public int targetScore = 10000;

    [Header("Special Combo Points")]
    [Tooltip("Points awarded for a straight (1-6).")]
    public int pointsForStraight = 1500;

    [Tooltip("Points awarded for three pairs (e.g., 2-2, 4-4, 5-5).")]
    public int pointsForThreePairs = 1500;

    [Tooltip("Points awarded for two triplets (e.g., 2-2-2 and 6-6-6).")]
    public int pointsForTwoTriplets = 2500;

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
}