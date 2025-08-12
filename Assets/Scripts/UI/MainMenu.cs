using Farkle.Managers;
using UnityEngine;

namespace Farkle.UI
{
    public class MainMenu : MonoBehaviour
    {
        public void PressPlayGame()
        {
            FarkleSceneManager.Instance.LoadGameScene();
        }
    }
}