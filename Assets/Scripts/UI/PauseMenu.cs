using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuUI;
    
    void Update()
    {
#if UNITY_ANDROID
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            pauseMenuUI.gameObject.SetActive(!pauseMenuUI.gameObject.activeSelf);
        }
#endif
    }
    
    public void ResumeGame()
    {
        pauseMenuUI.gameObject.SetActive(false);
    }

    public void MainMenu()
    {
        // No-op for now
        FarkleLogger.Log("Main Menu button clicked. Implement scene loading here.");
    }
    
    public void Options()
    {
        // No-op for now
        FarkleLogger.Log("Options button clicked. Implement options menu here.");
    }
}
