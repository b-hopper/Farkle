using System;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    [SerializeField] private TMPro.TextMeshProUGUI _playerNameText;
    [SerializeField] private TMPro.TextMeshProUGUI _scoreText;
    [SerializeField] private TMPro.TextMeshProUGUI LeftButton_Text;
    [SerializeField] private TMPro.TextMeshProUGUI LeftButton_SubText;
    [SerializeField] private TMPro.TextMeshProUGUI LeftButton_SubValue;
    [SerializeField] private TMPro.TextMeshProUGUI RightButton_Text;
    [SerializeField] private TMPro.TextMeshProUGUI RightButton_SubText;
    [SerializeField] private TMPro.TextMeshProUGUI RightButton_SubValue;
    
    [SerializeField] private TMPro.TextMeshProUGUI _splashText;
    
    [SerializeField] private Button LeftButton;
    [SerializeField] private Button RightButton;
    
    [SerializeField] private RectTransform RotationTransform;
    
    [SerializeField] private TMPro.TextMeshProUGUI _debugText;
    
    protected override void Awake()
    {
        base.Awake();
        
        if (_playerNameText == null || _scoreText == null || 
            LeftButton_Text == null || RotationTransform == null ||
            LeftButton_SubText == null || LeftButton_SubValue == null || 
            RightButton_Text == null || RightButton_SubText == null || 
            RightButton_SubValue == null || _splashText == null ||
            LeftButton == null || RightButton == null)
        {
            FarkleLogger.LogError("UIManager: Missing references.");
        }
        
        InitDelegates();
    }

    private void InitDelegates()
    {
        FarkleGame.Instance.OnSelectDice.AddListener(UpdateUI);
        FarkleGame.Instance.OnGameStart.AddListener(ResetCanvasRotation);
    }

    private void Start()
    {
        UpdateUI();
    }

    public void UpdateUI(int _) => UpdateUI();

    public void UpdateUI()
    {
        UpdatePlayerUI();
        UpdateButtonUI(TurnManager.Instance.CurrentState);
    }
    
    private void UpdatePlayerUI()
    {
        Player currentPlayer = PlayerManager.Instance.CurrentPlayer;
        
        if (currentPlayer == null)
        {
            FarkleLogger.LogError("UIManager: Current player is null.");
            return;
        }
        
        ScoreManager.PlayerScore currentPlayerScore = currentPlayer.Score;
        
        _scoreText.text = $"{currentPlayerScore.Score}";
        //_turnScoreText.text = $"{currentPlayerScore.TurnScore + currentPlayerScore.SelectedScore}";
        //_selectedScoreText.text = $"{currentPlayerScore.SelectedScore}";
        _playerNameText.text = $"{currentPlayer.Name}";
    }

#region Button UI
    private void UpdateButtonUI(TurnManager.TurnFlowState state)
    {
        ClearButtonUI();
        
        switch (state)
        {
            case TurnManager.TurnFlowState.NONE:
                break;
            case TurnManager.TurnFlowState.START_TURN:
                DoStartTurnUI();
                break;
            case TurnManager.TurnFlowState.BOTTOM_FEED:
                DoBottomFeedUI();
                break;
            case TurnManager.TurnFlowState.ROLL_DICE:
                DoRollDiceUI();
                break;
            case TurnManager.TurnFlowState.SELECT_DICE:
                DoSelectDiceUI();
                break;
            case TurnManager.TurnFlowState.END_TURN:
                DoEndTurnUI();
                break;
            case TurnManager.TurnFlowState.GAME_OVER:
                DoGameOverUI();
                break;
            default:
                FarkleLogger.LogError("UIManager: Invalid state.");
                break;
            
        }
    }

    private void DoStartTurnUI()
    {
        if (ScoreManager.Instance.BottomFeedScore > 0 && ScoreManager.Instance.CurrentPlayer.Score.BrokenIn)
        {
            LeftButton_Text.text = "RESET DICE";
            LeftButton_SubText.text = "Start fresh";
        
            RightButton_Text.text = "BOTTOM FEED";
            RightButton_SubText.text = "At stake: ";
            RightButton_SubValue.text = $"{ScoreManager.Instance.BottomFeedScore}";
        }
        else if (ScoreManager.Instance.BottomFeedScore > 0)
        {
            LeftButton_Text.text = "ROLL";
            
            RightButton_Text.text = "BOTTOM FEED";
            RightButton_Text.color = Color.grey;
            RightButton_SubText.text = "At stake: ";
            RightButton_SubValue.text = $"{ScoreManager.Instance.BottomFeedScore}";
        }
        else
        {
            LeftButton_Text.text = "ROLL";
        }
    }
    
    private void DoBottomFeedUI()
    {
        LeftButton_Text.text = "ROLL";
        LeftButton_SubText.text = "Bottom Feed";
        
        RightButton_SubText.text = "At stake:";
        int score = ScoreManager.Instance.BottomFeedScore;
        RightButton_SubValue.text = $"{score}";
    }
    
    private void DoRollDiceUI()
    {
        RightButton_SubText.text = "Points:";
            
        int score = PlayerManager.Instance.CurrentPlayer.Score.TurnScore +
                    PlayerManager.Instance.CurrentPlayer.Score.SelectedScore +
                    ScoreManager.Instance.BottomFeedScore;
            
        RightButton_SubValue.text = $"{score}";
    }
    
    private void DoSelectDiceUI()
    {
        if (DiceManager.Instance.FirstRoll || DiceManager.Instance.Dice.Any(x => x.IsSelected))
        {
            LeftButton_Text.text = "ROLL";
        }
        
        if (DiceManager.Instance.Dice.Any(die => die.IsSelected))
        {
            RightButton_Text.text = "HOLD";

            bool brokenIn = ScoreManager.Instance.CurrentPlayer.Score.BrokenIn;
            
            RightButton_Text.color = brokenIn ? Color.black: Color.grey;
            
        }
        
        RightButton_SubText.text = "Points:";
            
        int score = PlayerManager.Instance.CurrentPlayer.Score.TurnScore +
                    PlayerManager.Instance.CurrentPlayer.Score.SelectedScore +
                    ScoreManager.Instance.BottomFeedScore;
            
        RightButton_SubValue.text = $"{score}";
    }
    
    private void DoEndTurnUI()
    {
        var nextPlayer = PlayerManager.Instance.NextPlayer;
        
        if (nextPlayer.HasTakenFinalTurn)
        {
            LeftButton_Text.text = "FINISH";
        }
        else
        {
            LeftButton_Text.text = "NEXT PLAYER";
            LeftButton_SubText.text = "Up next:"; 
            LeftButton_SubValue.text = $"{nextPlayer.Name}";
        }
    }
    
    private void DoGameOverUI()
    {
        LeftButton_Text.text = "NEW GAME";
        LeftButton_SubText.text = "Rematch?";
        
        RightButton_Text.text = "EXIT";
        RightButton_SubText.text = "Goodbye!";
    }

    private void ClearButtonUI()
    {
        LeftButton_Text.text = "";
        LeftButton_SubText.text = "";
        LeftButton_SubValue.text = "";
        RightButton_Text.text = "";
        RightButton_SubText.text = "";
        RightButton_SubValue.text = "";
        
        RightButton_Text.color = Color.black;
    }

    public void DoSplashText(string text)
    {
        _splashText.text = text;
        
        DOTween.Kill(_splashText.transform);
        
        DOTween.Sequence()
            .Append(_splashText.DOFade(1f, 0f))
            .Append(_splashText.transform.DOScale(Vector3.zero, 0f))
            .AppendCallback(() => _splashText.gameObject.SetActive(true))
            .Append(_splashText.transform.DOScale(Vector3.one, 0.5f))
            .Append(_splashText.DOFade(0f, 0.5f))
            .AppendInterval(1f)
            .AppendCallback(() => _splashText.gameObject.SetActive(false));
    }
#endregion
    
#region Input
    public void PressLeftButton()
    {
        if (FarkleGame.Instance.InputLock)
        {
            FarkleLogger.LogWarning("UIManager: Input is locked, ignoring left button press.");
            return;
        }
        
        TurnManager.TurnFlowState state = TurnManager.Instance.CurrentState;
        FarkleLogger.Log($"UIManager: Pressed left button in state: {state}");
        
        switch (state)
        {
            case TurnManager.TurnFlowState.START_TURN:
                // Reset dice
                DiceManager.Instance.ResetDice();
                ScoreManager.Instance.SetBottomFeed(0);
                TurnManager.Instance.SetTurnFlowState(TurnManager.TurnFlowState.ROLL_DICE);
                break;
            
            case TurnManager.TurnFlowState.BOTTOM_FEED:
                // Roll dice
                TurnManager.Instance.SetTurnFlowState(TurnManager.TurnFlowState.ROLL_DICE);
                break;
            
            case TurnManager.TurnFlowState.ROLL_DICE:
                break;
            
            case TurnManager.TurnFlowState.SELECT_DICE:
                // Roll dice
                if (DiceManager.Instance.Dice.Any(x => x.IsSelected))
                {
                    TurnManager.Instance.SetTurnFlowState(TurnManager.TurnFlowState.ROLL_DICE);
                }
                break;
            
            case TurnManager.TurnFlowState.END_TURN:
                TurnManager.Instance.NextPlayer();
                break;
            
            case TurnManager.TurnFlowState.GAME_OVER:
                FarkleGame.Instance.NewGame();
                break;
            
            default:
                FarkleLogger.LogError("UIManager: Invalid state.");
                break;
        }
    }
    public void PressRightButton()
    {
        if (FarkleGame.Instance.InputLock)
        {
            return;
        }
        
        TurnManager.TurnFlowState state = TurnManager.Instance.CurrentState;
        
        switch (state)
        {
            case TurnManager.TurnFlowState.START_TURN:
                // Bottom Feed
                if (ScoreManager.Instance.BottomFeedScore > 0 && ScoreManager.Instance.CurrentPlayer.Score.BrokenIn)
                {
                    TurnManager.Instance.SetTurnFlowState(TurnManager.TurnFlowState.BOTTOM_FEED);
                }
                else if (ScoreManager.Instance.BottomFeedScore > 0)
                {
                    DoSplashText("You need at least 500 points to bottom feed!");
                }
                break;
            
            case TurnManager.TurnFlowState.BOTTOM_FEED:
                // Nothing
                break;
            
            case TurnManager.TurnFlowState.ROLL_DICE:
                // Nothing
                break;
            
            case TurnManager.TurnFlowState.SELECT_DICE:
                // Hold dice and end turn
                bool brokenIn = ScoreManager.Instance.CurrentPlayer.Score.BrokenIn;
                
                if (DiceManager.Instance.Dice.Any(die => die.IsSelected) && brokenIn) 
                {
                    FarkleGame.Instance.HoldSelectedDiceAndEndTurn();
                }
                else if (!brokenIn)
                {
                    DoSplashText("You need at least 500 points to hold dice!");
                }
                
                break;
            
            case TurnManager.TurnFlowState.END_TURN:
                break;
            
            case TurnManager.TurnFlowState.GAME_OVER:
                Application.Quit();
                break;
            
            default:
                FarkleLogger.LogError("UIManager: Invalid state.");
                break;
        }
    }
#endregion

    public bool FlipScreen()
    {
        if (!PlayerSettingsManager.Settings.rotateUIAtEndOfTurn)
        {
            return false;
        }
        
        RotateCanvas();
        return true;
    }

    private void RotateCanvas(float angle = 180.0f, float duration = 1.5f)
    {
        float currentAngle = RotationTransform.eulerAngles.z;
        float targetAngle = currentAngle + angle;
        
        RotateCanvasTo(targetAngle, duration);
    }
    
    private void RotateCanvasTo(float angle = 180.0f, float duration = 1.5f)
    {
        RotationTransform.DORotate(new Vector3(0, 0, angle), duration).SetEase(Ease.InOutQuint);
    }
    
    private void ResetCanvasRotation()
    {
        RotateCanvasTo(0.0f, 1.5f);
    }
    
    public void UpdateDebugText(string text)
    {
        if (_debugText == null)
        {
            return;
        }
        
        _debugText.text = text;
    }
}
