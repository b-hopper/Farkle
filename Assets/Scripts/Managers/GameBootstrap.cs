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
        private static bool _initialized = false;
        private static Task _initTask = null;

        private async void Start()
        {
            FarkleLogger.Log("GameBootstrap Start");
            await EnsureInitializedAsync();
            FarkleSceneManager.Instance.LoadMainMenu();
        }

        /// <summary>
        /// Ensures all game managers are initialized. Safe to call from any scene —
        /// runs only once regardless of how many callers await it.
        /// </summary>
        public static Task EnsureInitializedAsync()
        {
            if (_initialized) return Task.CompletedTask;
            if (_initTask != null) return _initTask;

            _initTask = RunInitAsync();
            return _initTask;
        }

        private static async Task RunInitAsync()
        {
            FarkleLogger.Log("GameBootstrap: Initializing...");
            _ = LoadBackendConfigurationAsync(); // fire and forget — backend is optional
            await InitializeGameManagersAsync();
            _initialized = true;
            FarkleLogger.Log("GameBootstrap: Initialization complete.");
        }

        private static async Task LoadBackendConfigurationAsync()
        {
            FarkleLogger.Log("Loading BackendConfig from Addressables...");
            var handle = Addressables.LoadAssetAsync<BackendConfig>("BackendConfig_Default");
            await handle.Task;
            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                FarkleLogger.LogError("Failed to load BackendConfig from Addressables.");
                return;
            }
            BackendService.Initialize(handle.Result);
        }

        private static async Task InitializeGameManagersAsync()
        {
            FarkleLogger.Log("Initializing game managers...");
            var managers = new List<IGameManager>
            {
                GameSettingsManager.Instance,
                PlayerSettingsManager.Instance,
                PlayerManager.Instance,
            };

            foreach (var manager in managers)
            {
                if (manager != null)
                {
                    FarkleLogger.Log($"Initializing manager: {manager.GetType().Name}");
                    await manager.InitAsync();
                }
            }
        }
    }
}