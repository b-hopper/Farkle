using System;
using UnityEngine;

public class ViewController : MonoBehaviour
{
    public static ViewController Instance { get; private set; }
    
    [SerializeField] public DieView[] dieViews;
    [SerializeField] private float rollDiceVariation = 0.25f;
    
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
        
        if (dieViews.Length == 0)
        {
            Debug.LogError("No DieViews found.");
        }
    }

    private void Start()
    {
        InitDelegates();
        InitDiceViews();
        UpdateDiceView();
    }

    private void InitDelegates()
    {
        FarkleGame.Instance.OnRollDiceStart.AddListener(DoRollDiceAnimation);
        FarkleGame.Instance.OnTurnScoreUpdated.AddListener(UpdateDiceView);
        FarkleGame.Instance.OnSelectDice.AddListener(UpdateDiceView);
        FarkleGame.Instance.OnFarkle.AddListener(UpdateDiceView);
        FarkleGame.Instance.OnTurnEnd.AddListener(UpdateDiceView);
        FarkleGame.Instance.OnDiceHeld.AddListener(UpdateDiceView);
    }

    private void DoRollDiceAnimation(float duration)
    {
        for (var i = 0; i < dieViews.Length; i++)
        {
            if (DiceManager.Instance.Dice[i].IsHeld) continue;
            
            var variation = UnityEngine.Random.Range(-rollDiceVariation, rollDiceVariation);
            dieViews[i].DoRollDiceAnimation(duration + variation);
        }
    }

    private void UpdateDiceView(int _) => UpdateDiceView();
    private void UpdateDiceView()
    {
        for (var i = 0; i < DiceManager.Instance.Dice.Length; i++)
        {
            dieViews[i].UpdateFromDice();
        }
    }
    
    private void InitDiceViews()
    {
        for (var i = 0; i < dieViews.Length; i++)
        {
            dieViews[i].SetDice(DiceManager.Instance.Dice[i]);
        }
    }
    
}
