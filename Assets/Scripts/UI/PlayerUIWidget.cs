using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIWidget : FarkleUIElement
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private Image highlightBackground;

    public void Setup(Player player, bool highlight)
    {
        if (player == null) return;

        nameText.text = player.Name;
        scoreText.text = player.Score.Score.ToString();
        
        if (highlightBackground != null)
        {
            highlightBackground.enabled = highlight;
        }
    }
}
