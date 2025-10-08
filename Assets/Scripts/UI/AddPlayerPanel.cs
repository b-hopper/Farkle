using System;
using Farkle.Backend;
using Farkle.Managers;
using Farkle.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class AddPlayerPanel : FarkleUIElement
{
    [SerializeField] TMP_InputField playerNameInputField;

    bool isAddingPlayer = false;
    
    public UnityAction OnPlayerAdded;
    
    public async void OnAddPlayerClicked()
    {
        if (isAddingPlayer) return;
        
        if (playerNameInputField.text != "")
        {
            isAddingPlayer = true;
            await PlayerSettingsManager.Instance.CreatePlayerProfileAsync(playerNameInputField.text);

            playerNameInputField.text = "";
            OnPlayerAdded?.Invoke();
            isAddingPlayer = false;
            Hide();
        }
        else
        {
            Alert("Player name cannot be empty.");
        }
    }
    
    public void OnCancelClicked()
    {
        playerNameInputField.text = "";
        Hide();
    }
}
