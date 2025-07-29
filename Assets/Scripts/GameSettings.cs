using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings", menuName = "ScriptableObjects/GameSettings", order = 1)]
public class GameSettings : ScriptableObject
{
    public int targetScore = 10000;
    
    public int pointsForStraight = 1500;
    public int pointsForThreePairs = 1500;
    public int pointsForTwoTriplets = 2500;
    
    /// <summary>
    /// Points for three of a kind, four of a kind, five of a kind, and six of a kind. 
    /// Dice values are multiplied by this value.
    /// [1s are considered a dice value of 10]
    /// </summary>
    public int pointsMultiplierOfAKind = 100;

    public int pointsPerOne = 100;
    public int pointsPerFive = 50;
}