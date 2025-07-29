using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class FarkleGame : MonoBehaviour
{
    public static FarkleGame Instance { get; private set; }
    
    // UnityEvents
#region UnityEvents

    /// <summary>
    /// Event invoked when the player starts rolling the dice
    /// Used to trigger animations
    /// Argument: duration of the animation
    /// </summary>
    public UnityEvent<float> OnRollDiceStart;
    
    public UnityEvent OnRollDiceFinish;
    
    public UnityEvent OnFarkle;
    public UnityEvent OnTurnEnd;
    public UnityEvent OnDiceHeld;
    public UnityEvent OnGameStart;
    
    public UnityEvent<int> OnSelectDice;
    public UnityEvent<int> OnTurnScoreUpdated;
    
#endregion

    public bool InputLock => _inputLock;

    private bool _inputLock = false; // Lock input while animations are playing
    
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
    
    public void RollDice()
    {
        if (_inputLock)
        {
            return;
        }
        
        if (!CanRollDice())
        {
            return;
        }
        
        LockInput();
        
        int score = DiceManager.Instance.HoldSelectedDiceAndScore();
        OnTurnScoreUpdated.Invoke(score);

        DOTween.Sequence()
            .AppendCallback(() => OnRollDiceStart.Invoke(1f))
            .AppendInterval(1f)
            .AppendCallback(() =>
            {
                OnRollDiceFinish.Invoke();
                UnlockInput();
        
                if (DiceManager.Instance.CheckForFarkle())
                {
                    
                    LockInput();
                    DOTween.Sequence()
                        .AppendCallback(() => UIManager.Instance.DoSplashText("Farkle!"))
                        .AppendInterval(1.0f)
                        .AppendCallback(() => UIManager.Instance.FlipScreen())
                        .AppendInterval(1.5f)
                        .AppendCallback(() => EndTurn(true));
                }
            });
    }

    private bool CanRollDice()
    {
        // Can only roll dice if it's the first roll, or if there are already held dice
        
        if (DiceManager.Instance.FirstRoll)
        {
            return true;
        }
        
        return DiceManager.Instance.Dice.Any(die => die.IsHeld || die.IsSelected) || ScoreManager.Instance.CurrentPlayer.Score.TurnScore > 0;
    }

    public void HoldSelectedDiceAndEndTurn()
    {
        if (_inputLock)
        {
            return;
        }
        
        int score = DiceManager.Instance.HoldSelectedDiceAndScore();
        OnTurnScoreUpdated.Invoke(score);
        OnDiceHeld.Invoke();
        
        LockInput();
        
        DOTween.Sequence()
            .AppendCallback(() => UIManager.Instance.DoSplashText("Holding dice"))
            .AppendInterval(1.0f)
            .AppendCallback(() => UIManager.Instance.FlipScreen())
            .AppendInterval(1.5f)
            .AppendCallback(() => EndTurn());
    }

    private void EndTurn(bool farkle = false)
    {
        if (farkle)
        {
            ScoreManager.Instance.SetBottomFeed(0);
            OnFarkle.Invoke();
        }
        
        OnTurnEnd.Invoke();
        UnlockInput();
    }

    public void SelectDie(int index)
    {
        if (_inputLock)
        {
            return;
        }
        
        DiceManager.Instance.SelectDie(index);
        OnSelectDice.Invoke(index);
    }
    
    public void LockInput()
    {
        _inputLock = true;
    }

    public void UnlockInput()
    {
        _inputLock = false;
    }

    public void NewGame()
    {
        DiceManager.Instance.ResetDice();
        TurnManager.Instance.Initialize();
        
        OnGameStart.Invoke();
    }
}
