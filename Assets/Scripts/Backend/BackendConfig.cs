using UnityEngine;

namespace Farkle.Backend
{
    [CreateAssetMenu(fileName = "BackendConfig", menuName = "ScriptableObjects/Backend Config")]
    public class BackendConfig : ScriptableObject
    {
        [Header("Base URL (no trailing slash). Example: http://localhost:8000")]
        public string BaseUrl = "http://localhost:8000";

        [Header("Request timeout (seconds)")]
        public int TimeoutSeconds = 10;
    }
}