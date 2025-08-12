using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

namespace Farkle.Managers
{
    public class FarkleSceneManager : Singleton<FarkleSceneManager>
    {


        public void LoadMainMenu()
        {
            Addressables.LoadSceneAsync("Scene_MainMenu");
        }
        
        public void LoadGameScene()
        {
            Addressables.LoadSceneAsync("Scene_GameScene").Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    FarkleGame.Instance.NewGame();
                }
                else
                {
                    FarkleLogger.LogError("Failed to load game scene.");
                }
            };
        }

    }
}