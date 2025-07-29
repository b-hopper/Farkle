using UnityEngine;

public class FarkleLogger
{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public static void Log(string message)
    {
        Debug.Log($"<color=white>[FARKLE]</color> {message}");
    }

    public static void LogWarning(string message)
    {
        Debug.LogWarning($"<color=yellow>[FARKLE]</color> {message}");
    }

    public static void LogError(string message)
    {
        Debug.LogError($"<color=red>[FARKLE]</color> {message}");
    }
#else
    public static void Log(string message) { }
    public static void LogWarning(string message) { }
    public static void LogError(string message) { }
#endif
}
