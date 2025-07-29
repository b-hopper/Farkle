using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIWidget : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private Image highlightBackground;

    public void Setup(Player player, bool isCurrent)
    {
        if (player == null) return;

        nameText.text = player.Name;
        scoreText.text = player.Score.Score.ToString();
        
        if (highlightBackground != null)
        {
            highlightBackground.enabled = isCurrent;
        }
    }
}
