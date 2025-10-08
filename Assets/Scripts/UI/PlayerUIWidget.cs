using Farkle.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Farkle.UI
{
    public class PlayerUIWidget : FarkleUIElement
    {
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private Image highlightBackground;

        [SerializeField] public Button selectButton;

        public PlayerProfile playerProfile;
        
        public void Setup(Player player, bool highlight)
        {
            if (player == null) return;

            playerProfile = player.Profile;
            
            if (nameText != null)
            {
                nameText.text = player.Name;
            }

            if (scoreText != null)
            {
                scoreText.text = player.Score.Score.ToString();
            }

            SetHighlight(highlight);
        }

        public void Setup(PlayerProfile newProfile)
        {
            if (newProfile == null) return;
            
            playerProfile = newProfile;
            
            if (nameText != null)
            {
                nameText.text = newProfile.playerName;
            }
        }

        public void SetHighlight(bool highlight)
        {
            if (highlightBackground != null)
            {
                highlightBackground.enabled = highlight;
            }
        }
        
        public void ShowPlayerStats()
        {
            // TODO
            FarkleLogger.Log($"Showing stats for player: {playerProfile}");
        }
    }
}