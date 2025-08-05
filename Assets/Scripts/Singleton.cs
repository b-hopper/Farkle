using UnityEngine;

public class Singleton<T> : UnityEngine.MonoBehaviour where T : Singleton<T>
{
    private static T _instance;
    public static T Instance {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<T>();
                if (_instance == null)
                {
                    if (thisObject == null)
                    {
                        thisObject = new GameObject(typeof(T).Name);
                        thisObject.AddComponent<T>();
                    }

                    _instance = thisObject.GetComponent<T>();
                    thisObject.transform.SetParent(null, false);
                    DontDestroyOnLoad(thisObject);
                }
            }

            return _instance;
        } 
        private set => _instance = value;
    }

    private static GameObject thisObject;
}
