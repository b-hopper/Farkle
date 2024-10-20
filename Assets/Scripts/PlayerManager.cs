using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class Player
{
    public string Name;
    [SerializeField] public ScoreManager.PlayerScore Score;
    
    public bool HasTakenFinalTurn = false;
    
    public Player(string name)
    {
        Score = new ScoreManager.PlayerScore(0, 0);
        Name = name;
    }
}

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    [SerializeField] private Player[] players;
    
    public Player CurrentPlayer
    {
        get
        {
            int idx = TurnManager.Instance.CurrentPlayerIndex;
            
            if (idx >= players.Length || idx < 0)
            {
                Debug.LogError($"(PlayerManager::CurrentPlayer) Invalid player index: {idx}");
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
    }


    public void Initialize(int playerCount = 1)
    {
        players = new Player[playerCount];

        for (int i = 0; i < players.Length; i++)
        {
            players[i] = new Player($"Player {i + 1}");
        }
    }
    
}