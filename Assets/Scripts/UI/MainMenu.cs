using Managers;
using UnityEngine;

namespace UI
{
    public class MainMenu : MonoBehaviour
    {
        public void PressPlayGame()
        {
            FarkleSceneManager.Instance.LoadGameScene();
        }
    }
}