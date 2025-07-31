using UnityEngine;

public class Singleton<T> : UnityEngine.MonoBehaviour where T : Singleton<T>
{
    public static T Instance { get; private set; }
    
    [SerializeField] [Tooltip("This field is used to ensure the singleton instance is not destroyed on scene load.")]
    private bool _dontDestroyOnLoad = true;
    
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
            transform.SetParent(null, false);
            if (_dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
