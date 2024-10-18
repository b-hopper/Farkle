using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }
    public int CurrentPlayerIndex => _currentPlayerIndex;

    [SerializeField] private int _playerCount = 2;
    
    [SerializeField] private bool _startGameOnAwake = true;

    private int _currentPlayerIndex;

    public TurnFlowState CurrentState => _currentState;
    
    TurnFlowState _currentState = TurnFlowState.NONE;

    public bool IsGameEnding = false;
    
    public enum TurnFlowState
    {
        NONE,
        START_TURN,
        BOTTOM_FEED,
        ROLL_DICE,
        SELECT_DICE,
        END_TURN,
        FARKLE,
        GAME_OVER
    }
    
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

    private void Start()
    {
        
        InitDelegates();
        
        if (_startGameOnAwake)
        {
            Initialize(_playerCount);
        }
    }
    
    public void Initialize(int playerCount = 2)
    {
        _playerCount = playerCount;
        _currentPlayerIndex = 0;
        IsGameEnding = false;
        
        PlayerManager.Instance.Initialize(playerCount);
        SetTurnFlowState(TurnFlowState.START_TURN);
    }
    
    private void InitDelegates()
    {
        FarkleGame.Instance.OnRollDiceFinish.AddListener(OnRollDiceFinish);
        FarkleGame.Instance.OnDiceHeld.AddListener(OnDiceHeld);
        FarkleGame.Instance.OnFarkle.AddListener(OnFarkle);
    }

    private void OnFarkle()
    {
        SetTurnFlowState(TurnFlowState.FARKLE);
    }

    private void OnDiceHeld()
    {
        SetTurnFlowState(TurnFlowState.END_TURN);
    }

    private void OnRollDiceFinish()
    {
        SetTurnFlowState(TurnFlowState.SELECT_DICE);
    }

    public void NextPlayer()
    {
        _currentPlayerIndex = (_currentPlayerIndex + 1) % _playerCount;
        
        SetTurnFlowState(TurnFlowState.START_TURN);
    }
    
    public void GameWinningScoreReached()
    {
        IsGameEnding = true;
        UIManager.Instance.DoSplashText($"{PlayerManager.Instance.CurrentPlayer.Name} has reached {ScoreManager.Instance.ScoreToWin} points!\n" +
                                        $"One more round to finish the game...");
    }
    
    public void SetTurnFlowState(TurnFlowState state)
    {
        _currentState = state;
        
        UIManager.Instance.UpdateDebugText($"{_currentState}");
        Debug.Log($"(TurnManager::SetTurnFlowState) <color=green>{_currentState}</color>");
        
        HandleTurnFlow();
        
    }

    
    private void HandleTurnFlow()
    {
        switch (_currentState)
        {
            case TurnFlowState.NONE:
                break;
            case TurnFlowState.START_TURN:
                OnStartTurnFlowEntered();
                break;
            case TurnFlowState.BOTTOM_FEED:
                OnBottomFeedFlowEntered();
                break;
            case TurnFlowState.ROLL_DICE:
                OnRollDiceFlowEntered();
                break;
            case TurnFlowState.SELECT_DICE:
                OnSelectDiceFlowEntered();
                break;
            case TurnFlowState.END_TURN:
                OnEndTurnFlowEntered();
                break;
            case TurnFlowState.FARKLE:
                OnFarkleFlowEntered();
                break;
            case TurnFlowState.GAME_OVER:
                OnGameOverFlowEntered();
                break;
        }

        UIManager.Instance.UpdateUI();
    }


    private void OnFarkleFlowEntered()
    {
        SetTurnFlowState(TurnFlowState.END_TURN);
    }

    private void OnStartTurnFlowEntered()
    {
        if (PlayerManager.Instance.CurrentPlayer.HasTakenFinalTurn)
        {
            SetTurnFlowState(TurnFlowState.GAME_OVER);
            return;
        }
        
        if (IsGameEnding)
        {
            UIManager.Instance.DoSplashText($"{PlayerManager.Instance.CurrentPlayer.Name}'s FINAL TURN!");
        }
        else
        {
            UIManager.Instance.DoSplashText($"{PlayerManager.Instance.CurrentPlayer.Name}'s turn");
        }
        
        DiceManager.Instance.FirstRoll = true;
    }
    
    private void OnBottomFeedFlowEntered()
    {
        if (ScoreManager.Instance.BottomFeedScore == 0)
        {
            SetTurnFlowState(TurnFlowState.ROLL_DICE);
        }
    }
    

    private void OnRollDiceFlowEntered()
    {
        FarkleGame.Instance.RollDice();
    }


    private void OnSelectDiceFlowEntered()
    {
    }

    private void OnEndTurnFlowEntered()
    {
        if (IsGameEnding)
        {
            Debug.LogWarning($"(TurnManager::OnEndTurnFlowEntered) Game is ending... {PlayerManager.Instance.CurrentPlayer.Name} setting final turn flag");
            PlayerManager.Instance.CurrentPlayer.HasTakenFinalTurn = true;
        }
        UIManager.Instance.UpdateUI();
    }
    
    private void OnGameOverFlowEntered()
    {
        Player winner = ScoreManager.Instance.GetWinner();
        
        UIManager.Instance.DoSplashText($"Game Over! {winner.Name} wins!");
    }
}
