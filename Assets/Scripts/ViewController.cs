using Managers;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class ViewController : Singleton<ViewController>
{
    public DiceView[] dieViews;
    [SerializeField] Transform diceParent;
    [SerializeField] DiceView diceViewPrefab;
    
    [SerializeField] private float rollDiceVariation = 0.25f;

    private void Start()
    {
        Addressables.LoadAssetAsync<GameObject>("DiceViewPrefab").Completed += handle =>
        {
            if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
            {
                handle.Result.TryGetComponent(out diceViewPrefab);

                if (diceViewPrefab == null)
                {
                    FarkleLogger.LogError("DiceView prefab does not contain a DiceView component.");
                    return;
                }
                
                Initialize();
            }
            else
            {
                FarkleLogger.LogError("Failed to load DiceView prefab from Addressables.");
            }
        };    
    }

    private void Initialize()
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
        FarkleGame.Instance.OnGameStart.AddListener(UpdateDiceView);
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
        if (diceParent == null)
        {
            // Attempt to find the parent object if not set
            diceParent = GameObject.Find("DiceParent")?.transform;
            if (diceParent == null)
            {
                FarkleLogger.LogError("DiceParent not found in the scene.");
                return;
            }
        }

        if (diceViewPrefab == null)
        {
            FarkleLogger.LogError("DieView prefab is not set in the ViewController.");
            return;
        }
        
        if (diceParent.childCount > 0)
        {
            // Clear existing die views if any
            foreach (Transform child in diceParent)
            {
                Destroy(child.gameObject);
            }
        }
        
        dieViews = new DiceView[DiceManager.Instance.Dice.Length];
        
        for (var i = 0; i < DiceManager.Instance.Dice.Length; i++)
        {
            dieViews[i] = Instantiate(diceViewPrefab, diceParent);
            dieViews[i].SetDice(DiceManager.Instance.Dice[i]);
        }
    }
    
}
