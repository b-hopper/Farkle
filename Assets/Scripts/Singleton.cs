using UnityEngine;

public class Singleton<T> : UnityEngine.MonoBehaviour where T : Singleton<T>
{
    public static T Instance { get; private set; }
    
    protected virtual void Awake()
    {
        if (Instance == null)
        {
            if (gameObject == null)
            {
                FarkleLogger.LogError($"Singleton instance cannot be null: {typeof(T).Name}");
                return;
            }
            
            Instance = (T)this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
