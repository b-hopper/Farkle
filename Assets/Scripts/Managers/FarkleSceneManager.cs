using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers
{
    public class FarkleSceneManager : Singleton<FarkleSceneManager>
    {
        private Scene currentScene;

        [SerializeField] private Scene mainMenuScene;
        [SerializeField] private Scene gameScene;

        protected void Awake()
        {
            currentScene = SceneManager.GetActiveScene();
        }

        public void LoadMainMenu()
        {
            if (currentScene.name != mainMenuScene.name)
            {
                SceneManager.LoadScene(mainMenuScene.name);
                currentScene = SceneManager.GetActiveScene();
            }
        }
        
        public void LoadGameScene()
        {
            if (currentScene.name != gameScene.name)
            {
                SceneManager.LoadScene(gameScene.name);
                currentScene = SceneManager.GetActiveScene();
            }
        }
    }
}