using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSettings", menuName = "ScriptableObjects/PlayerSettings", order = 1)]
public class PlayerSettings : ScriptableObject
{
    [Header("Gameplay")] 
    public int playerCount = 2;
    
    [Header("Player Profiles")]
    public PlayerProfile[] playerProfiles;

    public override string ToString()
    {
        if (playerProfiles == null || playerProfiles.Length == 0)
        {
            return "PlayerSettings: No player profiles set.";
        }

        string profilesStr = "";
        for (int i = 0; i < playerProfiles.Length; i++)
        {
            profilesStr += $"Profile {i + 1}: {playerProfiles[i]}\n";
        }
        
        return $"PlayerSettings: Player Count = {playerCount}\n{profilesStr}";
    }
}
