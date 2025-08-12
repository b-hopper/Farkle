using System.Collections.Generic;
using UnityEngine;

namespace Farkle.Managers
{
    /// <summary>
    /// Responsible for orchestrating the initialization of all game managers
    /// that implement IGameManager. Attach this to a GameObject in your
    /// starting scene.
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        private async void Awake()
        {
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);

            var managers = new List<IGameManager>
            {
                GameSettingsManager.Instance,
                PlayerSettingsManager.Instance,
                PlayerManager.Instance,
                // Add additional managers that need async initialization here
            };

            // Initialise managers sequentially (respecting dependencies)
            foreach (var manager in managers)
            {
                if (manager != null)
                {
                    await manager.InitAsync();
                }
            }

            // Once managers are ready, load the first scene or start the game
            FarkleSceneManager.Instance.LoadMainMenu();
        }
    }
}