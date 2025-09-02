using System.Collections.Generic;
using System.Threading.Tasks;
using Farkle.Backend;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Farkle.Managers
{
    /// <summary>
    /// Responsible for orchestrating the initialization of all game managers
    /// that implement IGameManager. Attach this to a GameObject in your
    /// starting scene.
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            FarkleLogger.Log("GameBootstrap Awake");
            Task.Run(InitializeAsync);
        }

        private async Task InitializeAsync()
        {
            
            FarkleLogger.Log("GameBootstrap InitializeAsync started");

            await LoadBackendConfiguration();
            await InitializeGameManagersAsync();
            
            // Once managers are ready, load the first scene or start the game
            FarkleSceneManager.Instance.LoadMainMenu();
        }

        private static async Task InitializeGameManagersAsync()
        {
            FarkleLogger.Log("Initializing game managers...");
            var managers = new List<IGameManager>
            {
                GameSettingsManager.Instance,
                PlayerSettingsManager.Instance,
                PlayerManager.Instance,
                // Add additional managers that need async initialization here
            };

            FarkleLogger.Log("2 Initializing game managers...");
            // Initialise managers sequentially (respecting dependencies)
            foreach (var manager in managers)
            {
                if (manager != null)
                {
                    FarkleLogger.Log($"Initializing manager: {manager.GetType().Name}");
                    await manager.InitAsync();
                }
            }
        }

        private static async Task LoadBackendConfiguration()
        {
            FarkleLogger.Log("Loading BackendConfig from Addressables...");
            
            var handle2 = Addressables.LoadAssetsAsync<BackendConfig>(null);
            await handle2.Task;

            foreach (var config in handle2.Result)
            {
                Debug.Log($"Found config: {config.name}");
            }
            
            var handle = Addressables.LoadAssetAsync<BackendConfig>("BackendConfig");
            await handle.Task;
            
            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                FarkleLogger.LogError("Failed to load BackendConfig from Addressables.");
                return;
            }
            BackendService.Initialize(handle.Result);
        }
    }
}