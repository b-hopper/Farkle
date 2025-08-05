using Managers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UI
{
    public class PauseMenu : FarkleUIElement
    {
        // We don't want to use `gameObject` here, because we need to listen for the escape key (back button) press
        // So we need this object to stay active in the scene
        [SerializeField] private GameObject pauseMenuUIObj;

        protected override void Init()
        {
            ElementName = "PauseMenu";

            base.Init();
        }

        void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                pauseMenuUIObj.gameObject.SetActive(!pauseMenuUIObj.gameObject.activeSelf);
            }
        }

        public void ResumeGame()
        {
            Hide();
        }

        public void MainMenu()
        {
            FarkleGame.Instance.ReturnToMainMenu();
            Hide();
        }

        public void Options()
        {
            // No-op for now
            FarkleLogger.Log("Options button clicked. Implement options menu here.");
        }
    }
}