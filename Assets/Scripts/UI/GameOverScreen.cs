using Managers;
using TMPro;
using UnityEngine;

namespace UI
{
    public class GameOverScreen : FarkleUIElement
    {
        // Things we'll need for the game over screen:
        // - Player profiles to display scores
        // - A button to return to the main menu
        // - A button to restart the game

        [SerializeField] private PlayerUIWidget playerWidgetPrefab;
        [SerializeField] private Transform playerWidgetsContainer;
        private PlayerUIWidget[] _playerWidgets;

        [SerializeField] private TextMeshProUGUI winnerText;

        protected override void Init()
        {
            ElementName = "GameOverScreen";

            base.Init();
        }

        public void Show(Player[] players)
        {
            base.Show();

            var winner = ScoreManager.Instance.GetWinner();
            if (winner != null)
            {
                winnerText.text = $"{winner.Name} Wins!!";
            }

            _playerWidgets = new PlayerUIWidget[players.Length];
            for (int i = 0; i < players.Length; i++)
            {
                if (playerWidgetPrefab == null || playerWidgetsContainer == null)
                {
                    FarkleLogger.LogError("GameOverScreen::Init - Player widget prefab or container is not set.");
                    return;
                }

                if (_playerWidgets[i] == null)
                {
                    _playerWidgets[i] = Instantiate(playerWidgetPrefab, playerWidgetsContainer);
                }

                _playerWidgets[i].Setup(players[i], players[i] == winner);
            }
        }

        public override void Hide()
        {
            for (int i = 0; i < _playerWidgets.Length; i++)
            {
                if (_playerWidgets[i] != null)
                {
                    _playerWidgets[i].gameObject.SetActive(false);
                }
            }

            base.Hide();
        }

        public void OnRestartButtonClicked()
        {
            FarkleGame.Instance.NewGame();
            Hide();
        }

        public void OnMainMenuButtonClicked()
        {
            Hide();
            FarkleGame.Instance.ReturnToMainMenu();
        }
    }
}