using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSettings", menuName = "ScriptableObjects/PlayerSettings", order = 1)]
public class PlayerSettings : ScriptableObject
{
    [Header("Gameplay")] 
    public int playerCount = 2;
    
    [Header("Player Profiles")]
    public PlayerProfile[] playerProfiles;
    
    [Header("UI")]
    public bool rotateUIAtEndOfTurn = true;
    
    [Header("Animations")]
    [Range(0.1f, 2f)]
    public float animationSpeedMultiplier = 1f;
}
