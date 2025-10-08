using Farkle.UI;
using UnityEngine;
using UnityEngine.Events;

public class ConfirmPanel : FarkleUIElement
{
    [SerializeField] private TMPro.TextMeshProUGUI messageText;
    [SerializeField] private UnityEngine.UI.Button yesButton;
    [SerializeField] private UnityEngine.UI.Button noButton;
    
    public void Show(string message, UnityAction onYes, UnityAction onNo = null)
    {
        messageText.text = message;

        yesButton.onClick.RemoveAllListeners();
        noButton.onClick.RemoveAllListeners();

        yesButton.onClick.AddListener(() =>
        {
            onYes?.Invoke();
            gameObject.SetActive(false);
        });

        noButton.onClick.AddListener(() =>
        {
            onNo?.Invoke();
            gameObject.SetActive(false);
        });

        gameObject.SetActive(true);
    }
}
